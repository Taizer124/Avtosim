using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace Assets.VehicleController
{
    public interface IManualTransmissionInputProvider
    {
        bool IsManualTransmission();
        int GetCurrentGear();
        float GetClutchInput();
    }


    public interface ITransmissionTypeSettable
    {
        void SetTransmissionType(bool useSequential);
    }

    [RequireComponent(typeof(CarVisualsEssentials)),
    RequireComponent(typeof(Rigidbody)), DisallowMultipleComponent, AddComponentMenu("CustomVehicleController/Core/Custom Vehicle Controller"),
    HelpURL("https://distubredone.io/custom-vehicle-controller/")]
    public class CustomVehicleController : MonoBehaviour
    {
        public bool UsePreset = true;
        [SerializeField]
        private VehiclePartsPresetSO _vehiclePartsPreset;
        [SerializeField]
        private VehiclePartsCustomizableSet _customizableSet;

        [SerializeField, Separator]
        private EnginePartsContainer _enginePartsContainer;

        // Новое поле для отслеживания типа коробки передач
        [Header("Transmission Settings")]
        [SerializeField] private bool _useSequentialShifting = false;
        public bool UseSequentialShifting
        {
            get => _useSequentialShifting;
            set => _useSequentialShifting = value;
        }

        public EnginePartsContainer GetEnginePartsContainer() => _enginePartsContainer;

        public float GetClutchInput()
        {
            if (_inputProvider is IManualTransmissionInputProvider manualInput && manualInput.IsManualTransmission())
                return manualInput.GetClutchInput();
            return 0f;
        }

        //Reference type field allows other classes to cache it and use the up-to-date parts scriptable objects.
        //This class holds the parts that the vehicle is using either in the form of VehiclePartsPresetSO of a VehiclePartsCustomizableSet.
        //This class has a static and object specific event when any part field value changes.
        public VehiclePartsSetWrapper VehiclePartsSetWrapper;

        private VehicleControllerStatsManager _statsManager;
        private VehicleControllerPartsManager _partsManager;

        private CarVisualsEssentials _carVisualsEssentials;

        #region Handling Settings
        [Header("   Handling settings")]
        public DrivetrainType DrivetrainType;
        public TransmissionType TransmissionType;

        [SerializeField, Separator, Space, Min(0), Tooltip("Maximum steering angle in degrees")]
        private float _steerAngle = 25;
        public float SteerAngle
        {
            get => _steerAngle;
            set { _steerAngle = Mathf.Clamp(value, 0, 90); }
        }

        [SerializeField, Min(0), Tooltip("Time in which wheels will reach maximum steering angle.")]
        private float _steerSpeed = 0.2f;
        public float SteerSpeed
        {
            get => _steerSpeed;
            set { _steerSpeed = Mathf.Clamp(value, 0, 100); }
        }

        [SerializeField, Min(0), Tooltip("Time in which wheels will return to their default rotation when there is no steering input.")]
        private float _centeringSpeed = 0.1f;
        public float CenteringSpeed
        {
            get => _centeringSpeed;
            set { _centeringSpeed = Mathf.Clamp(value, 0, _steerSpeed); }
        }

        #endregion

        #region Extra options
        [Header("   Extra options")]
        [SerializeField, Range(0f, 100f), Tooltip("Defines how much slipping is allowed until the wheel is considered to be forward slipping. " +
            "\n Forward slipping occurs when acceleration force is higher than the wheel load * tire grip.")]
        private float _forwardSlippingThreshold = 0.1f;
        public float ForwardSlippingThreshold
        {
            get => _forwardSlippingThreshold;
            set
            {
                _forwardSlippingThreshold = Mathf.Clamp(value, 0, 100f);
            }
        }

        [SerializeField, Range(0f, 1f), Tooltip("Defines how much slipping is allowed until the wheel is considered to be sideways slipping.")]
        private float _sidewaysSlippingThreshold = 0.5f;
        public float SidewaysSlippingThreshold
        {
            get => _sidewaysSlippingThreshold;
            set
            {
                _sidewaysSlippingThreshold = Mathf.Clamp(value, 0, 1f);
            }
        }

        //allows you to control the car in air. 
        [Space, Separator]
        public bool AerialControlsEnabled = false;
        public float AerialControlsSensitivity = 0;
        public bool RecoveryHelp = false;
        public float CenterOfMassOffset = -2;
        #endregion

        [Header("   Physics"), SerializeField, Tooltip("Assign rigidbody component to avoid using the costly GetComponent operation")]
        private Rigidbody _rigidbody;
        [SerializeField, Range(1, 27), Tooltip("The amount of raycasts that go along the forward axis of the wheel with an offset from -radius to +radius. " +
            "\n Recommended values: [3:9]")]
        private int _suspensionSimulationPrecision = 5;
        public int SuspensionSimulationPrecision
        {
            get => _suspensionSimulationPrecision;
            set
            {
                _suspensionSimulationPrecision = Mathf.Clamp(value, 1, 27);
            }
        }

        [SerializeField, Tooltip("TCS (Traction Control System) adjusts the amount of torque that's applied to the wheels in case there is wheelspin.")]
        private bool _tcsEnabled = false;
        public bool TCSEnabled => _tcsEnabled;
        [SerializeField, Tooltip("ABS (Anti-Lock Braking System) doesn't allow the car's wheels to lock when braking hard, giving the driver more control over steering and improves the braking.")]
        private bool _absEnabled = true;
        public bool ABSEnabled => _absEnabled;

        [SerializeField, Tooltip("In case you are using mesh collider or multiple colliders that the raycast will go through, mark those colliders with specific layer so that the raycast will ignore them. Otherwise you can leave it as it is.")]
        private LayerMask _ignoreLayers;
        #region Wheel Controllers
        [SerializeField, Space, Separator]
        private VehicleAxle[] _frontAxles;
        public VehicleAxle[] FrontAxles => _frontAxles;
        [SerializeField]
        private VehicleAxle[] _rearAxles;
        public VehicleAxle[] RearAxles => _rearAxles;
        [SerializeField]
        private VehicleAxle[] _steerAxles;
        public VehicleAxle[] SteerAxles => _steerAxles;
        #endregion
        [SerializeField, Tooltip("Center Of Mass of the vehicle. " +
    "\nUsually placed in the middle of the vehicle, slightly closer to the engine.")]
        private Transform _centerOfMass;

        [SerializeField, Header("   Current Car Stats Scriptable Object"), Tooltip("In case you want to expose current car stats, " +
    "you can create a scriptable object and assign it here. " +
    "This gives you the ability to access current car stats in your other scripts. This field is optional")]
        private CurrentCarStats CurrentCarStats;

        //Abstract interface for handling input. Create a monobehaviour script that implements this interface,
        //or use the input scripts that come with this package
        private IVehicleControllerInputProvider _inputProvider;

        // Кэш для метода SetGear в _partsManager
        private MethodInfo _setGearMethod;
        private int _currentManualGear = 0;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();

            FindInputProvider();

            if (CurrentCarStats == null)
                CurrentCarStats = ScriptableObject.CreateInstance<CurrentCarStats>();
            else
            {
                if (CurrentCarStats.ScriptableObjectOwners.Count > 0)
                    Debug.LogError("Assigning the same instance of CurrentCarStats Scriptable Object to different vehicles can lead to unexpected behaviour.");
                CurrentCarStats.ScriptableObjectOwners.Add(gameObject);
            }

            if (UsePreset && _vehiclePartsPreset == null)
            {
                _vehiclePartsPreset = VehiclePartsPresetSO.CreateDefaultVehiclePartsPresetSO();
#if UNITY_EDITOR
                Debug.Log("VehiclePartsPresetSO wasn't assigned, so default one was created instead.");
#endif
            }

            if (UsePreset)
                VehiclePartsSetWrapper = new(_vehiclePartsPreset, this);
            else
                VehiclePartsSetWrapper = new(_customizableSet, this);

            _carVisualsEssentials = GetComponent<CarVisualsEssentials>();
            _carVisualsEssentials.Initialize(_rigidbody, CurrentCarStats);

            VehicleControllerInitializer initializer = new();
            (_statsManager, _partsManager) = initializer.InitializeVehicleControllers(_frontAxles, _rearAxles,
                _steerAxles, _rigidbody, transform, VehiclePartsSetWrapper, _enginePartsContainer.EnginePartsList, _centerOfMass, CurrentCarStats);

            // Пытаемся найти метод SetGear в _partsManager
            _setGearMethod = _partsManager.GetType().GetMethod("SetGear");
        }

        private void Update()
        {
            if (UsePreset)
                VehiclePartsSetWrapper.UpdateVehiclePartsPresetIfRequired(_vehiclePartsPreset);
            else
                VehiclePartsSetWrapper.UpdateVehiclePartsPresetIfRequired(_customizableSet);

            _statsManager.ManageStats(_inputProvider.GetGasInput(), _inputProvider.GetBrakeInput(), _inputProvider.GetHandbrakeInput(),
                            _sidewaysSlippingThreshold, _forwardSlippingThreshold, DrivetrainType);

            // Обрабатываем переключение передач в зависимости от типа коробки
            if (_inputProvider is IManualTransmissionInputProvider manualInput && manualInput.IsManualTransmission())
            {
                int targetGear = manualInput.GetCurrentGear();
                if (_setGearMethod != null)
                {
                    _setGearMethod.Invoke(_partsManager, new object[] { targetGear });
                    _currentManualGear = targetGear;
                }
                else
                {
                    // Если метод SetGear не найден, используем альтернативный подход
                    HandleManualTransmissionFallback(targetGear);
                }
            }
            else
            {
                _partsManager.ManageTransmissionUpShift(_inputProvider.GetGearUpInput());
                _partsManager.ManageTransmissionDownShift(_inputProvider.GetGearDownInput());
            }

            _carVisualsEssentials.HandleWheelVisuals(_inputProvider.GetHorizontalInput(), _steerAxles[0].LeftHalfShaft.WheelController.SteerAngle, _steerAngle, _steerSpeed);
        }

        private void HandleManualTransmissionFallback(int targetGear)
        {
            // Альтернативная реализация для механической коробки, если SetGear недоступен
            if (targetGear != _currentManualGear)
            {
                // Эмулируем переключение через существующие методы
                if (targetGear > _currentManualGear)
                {
                    for (int i = _currentManualGear; i < targetGear; i++)
                    {
                        _partsManager.ManageTransmissionUpShift(true);
                        // Сбрасываем флаг после имитации нажатия
                        if (i == targetGear - 1)
                            _partsManager.ManageTransmissionUpShift(false);
                    }
                }
                else if (targetGear < _currentManualGear)
                {
                    for (int i = _currentManualGear; i > targetGear; i--)
                    {
                        _partsManager.ManageTransmissionDownShift(true);
                        // Сбрасываем флаг после имитации нажатия
                        if (i == targetGear + 1)
                            _partsManager.ManageTransmissionDownShift(false);
                    }
                }
                _currentManualGear = targetGear;
            }
        }

        private void FixedUpdate()
        {
            _partsManager.ManageCarParts(_inputProvider.GetGasInput(), _inputProvider.GetBrakeInput(), _inputProvider.GetNitroBoostInput(),
                _inputProvider.GetHorizontalInput(), _inputProvider.GetHandbrakeInput(),
                _steerAngle, _steerSpeed, _centeringSpeed, TransmissionType, DrivetrainType, _suspensionSimulationPrecision, _ignoreLayers, _tcsEnabled, _absEnabled, RecoveryHelp, CenterOfMassOffset);

            _partsManager.PerformAirControls(AerialControlsEnabled, AerialControlsSensitivity,
                _inputProvider.GetPitchInput(), _inputProvider.GetYawInput(), _inputProvider.GetRollInput());
        }

        public Transform GetCenterOfMass() => _centerOfMass;
        public CurrentCarStats GetCurrentCarStats() => CurrentCarStats;
        public Rigidbody GetRigidbody() => _rigidbody;

        // Новый метод для получения информации о типе трансмиссии
        public string GetTransmissionTypeInfo()
        {
            if (_inputProvider is IManualTransmissionInputProvider manualInput)
            {
                if (manualInput.IsManualTransmission())
                {
                    return "Mechanical (H-pattern)";
                }
                else
                {
                    return "Sequential";
                }
            }
            else
            {
                switch (TransmissionType)
                {
                    case TransmissionType.Automatic:
                        return "Automatic";
                    case TransmissionType.Manual:
                        return "Manual";
                    case TransmissionType.Sequential:
                        return "Sequential";
                    default:
                        return TransmissionType.ToString();
                }
            }
        }

        // Новый метод для получения текущей передачи (работает для обоих типов коробок)
        public int GetCurrentGear()
        {
            if (_inputProvider is IManualTransmissionInputProvider manualInput && manualInput.IsManualTransmission())
            {
                return manualInput.GetCurrentGear();
            }
            else
            {
                // Возвращаем текущую передачу из CurrentCarStats или _partsManager
                return _currentManualGear; // Заглушка - нужно заменить на реальное получение передачи
            }
        }

        // Новый метод для проверки типа коробки передач
        public bool IsManualTransmission()
        {
            return _inputProvider is IManualTransmissionInputProvider manualInput &&
                   manualInput.IsManualTransmission();
        }

        // Новый метод для смены типа коробки передач
        public void SetTransmissionType(bool useSequential)
        {
            _useSequentialShifting = useSequential;

            // Используем интерфейс вместо конкретного класса
            if (_inputProvider is ITransmissionTypeSettable transmissionSettable)
            {
                transmissionSettable.SetTransmissionType(useSequential);
            }
        }

        public void SetVehiclePresetSO(VehiclePartsPresetSO newPreset)
        {
            _vehiclePartsPreset = newPreset;
            VehiclePartsSetWrapper.UpdateVehiclePartsPresetIfRequired(newPreset);
        }
        public VehiclePartsPresetSO GetVehiclePreset() => _vehiclePartsPreset;
        public void SetNewPartToCustomizableSet(IVehiclePart newPart, bool front = true)
        {
            switch (newPart)
            {
                case EngineSO:
                    _customizableSet.Engine = newPart as EngineSO;
                    break;

                case ForcedInductionSO:
                    _customizableSet.ForcedInduction = newPart as ForcedInductionSO;
                    break;

                case NitrousSO:
                    _customizableSet.Nitrous = newPart as NitrousSO;
                    break;

                case TransmissionSO:
                    _customizableSet.Transmission = newPart as TransmissionSO;
                    break;

                case SuspensionSO:
                    if (front)
                        _customizableSet.FrontSuspension = newPart as SuspensionSO;
                    else
                        _customizableSet.RearSuspension = newPart as SuspensionSO;
                    break;

                case TiresSO:
                    if (front)
                        _customizableSet.FrontTires = newPart as TiresSO;
                    else
                        _customizableSet.RearTires = newPart as TiresSO;
                    break;

                case BrakesSO:
                    _customizableSet.Brakes = newPart as BrakesSO;
                    break;

                case VehicleBodySO:
                    _customizableSet.Body = newPart as VehicleBodySO;
                    break;
            }
        }
        public void RemoveForcedInduction() => _customizableSet.ForcedInduction = null;
        public void RemoveNitrous() => _customizableSet.Nitrous = null;
        public VehiclePartsCustomizableSet GetCustomizableSet() => _customizableSet;

        public void EnableTCS(bool enable) => _tcsEnabled = enable;
        public void EnableABS(bool enable) => _absEnabled = enable;

        public void EnableInput(bool enable)
        {
            _inputProvider.EnableInput(enable);
        }

        public void UpdateWheelsRadiusFromMeshes()
        {
            for (int i = 0; i < _frontAxles.Length; i++)
            {
                _frontAxles[i].LeftHalfShaft.WheelController.UpdateWheelRadiusFromMesh();
                _frontAxles[i].RightHalfShaft.WheelController.UpdateWheelRadiusFromMesh();
            }
            for (int i = 0; i < _rearAxles.Length; i++)
            {
                _rearAxles[i].LeftHalfShaft.WheelController.UpdateWheelRadiusFromMesh();
                _rearAxles[i].RightHalfShaft.WheelController.UpdateWheelRadiusFromMesh();
            }
        }

        public void SetNewEnginePart(CustomEnginePart newPart)
        {
            for (int i = 0; i < _enginePartsContainer.EnginePartsList.Count; i++)
            {
                //if part of the same type is already in the list, swap it
                if (_enginePartsContainer.EnginePartsList[i].GetType().Name == newPart.GetType().Name)
                {
                    _enginePartsContainer.EnginePartsList[i] = newPart;
                    return;
                }
            }

            //if the new part is of an unexisting type in the list, add it to the list
            _enginePartsContainer.EnginePartsList.Add(newPart);
        }

        public void AddNitroCharge(float amount) => _partsManager.AddNitro(amount);

        private void FindInputProvider()
        {
            IVehicleControllerInputProvider[] providersFound = GetComponentsInChildren<IVehicleControllerInputProvider>();

            if (providersFound.Length == 1)
            {
                _inputProvider = providersFound[0];
                return;
            }

#if INPUT_SYSTEM_INSTALLED
            if (providersFound.Length == 0)
            {
                Debug.LogWarning($"No input provider script found on {gameObject.name}." +
                    " Input provider implemented using Input System was added since this package is installed." +
                    " Please add a script component that expends IVehicleControllerInputProvider interface.");
                _inputProvider = gameObject.AddComponent<VehicleInputProviderDemo>();
                return;
            }
#endif

            if (providersFound.Length == 0)
            {
                Debug.LogWarning($"No input provider script found on {gameObject.name}." +
                        " Input provider implemented using old input system was added." +
                        " Please add a script component that expends IVehicleControllerInputProvider interface." +
                        " If you install Input System package, you can add VehicleInputProviderDemo script, which has full controller support");
                _inputProvider = gameObject.AddComponent<VehicleControllerInputProvider>();
                return;
            }

            Debug.LogWarning($"Multiple Input Providers on {gameObject.name}. Selecting the first one found");
            _inputProvider = providersFound[0];
        }

        private void OnDestroy()
        {
            if (CurrentCarStats != null)
                CurrentCarStats.Reset();
        }
    }

    [Serializable]
    public class VehiclePartsCustomizableSet
    {
        public EngineSO Engine;
        public ForcedInductionSO ForcedInduction;
        public NitrousSO Nitrous;
        public TransmissionSO Transmission;
        public TiresSO FrontTires;
        public TiresSO RearTires;
        public SuspensionSO FrontSuspension;
        public SuspensionSO RearSuspension;
        public BrakesSO Brakes;
        public VehicleBodySO Body;
    }
}