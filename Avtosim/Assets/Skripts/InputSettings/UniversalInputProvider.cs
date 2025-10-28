using UnityEngine;
using LogitechG29.Sample.Input;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Input/All-in-One Input Provider")]
    public class AllInOneInputProvider : MonoBehaviour,
        IVehicleControllerInputProvider,
        IManualTransmissionInputProvider,
        ITransmissionTypeSettable
    {
        public enum InputMode { InputSystem, Wheel, Auto }

        [Header("Input Mode")]
        [SerializeField] private InputMode _currentMode = InputMode.Auto;

        [Header("Wheel Input")]
        [SerializeField] private InputControllerReader _wheelInput;

        [Header("Transmission Settings")]
        [SerializeField] private bool _useSequentialShifting = false;

        [Header("Input System Settings")]
        [SerializeField] private bool _forceGasInputDuringNitrous = false;

        // Для Input System
        private PlayerVehicleInputActions _inputActions;

        // Общие переменные ввода
        private float _gasInput;
        private float _brakeInput;
        private float _steeringInput;
        private bool _handbrakeInput;
        private bool _gearUpInput;
        private bool _gearDownInput;
        private bool _nitroInput;

        // Для механической коробки передач
        private bool[] _gearInputs = new bool[8]; // 0-N, 1-7 передачи
        private int _currentGear = 0; // 0 - нейтраль, 1-7 - передачи

        private bool _enabled = true;

        private void Awake()
        {
            // Инициализация Input System
            _inputActions = new PlayerVehicleInputActions();
            _inputActions.Enable();
        }

        private void OnEnable()
        {
            // Подписка на события руля
            if (_wheelInput != null)
            {
                _wheelInput.ThrottleCallback += OnThrottle;
                _wheelInput.BrakeCallback += OnBrake;
                _wheelInput.SteeringCallback += OnSteering;
                _wheelInput.HandbrakeCallback += OnHandbrake;
                _wheelInput.OnRightShiftCallback += OnGearUp;
                _wheelInput.OnLeftShiftCallback += OnGearDown;
                _wheelInput.OnEastButtonCallback += OnNitro;

                // Подписываемся на события механической коробки
                SubscribeToManualGearEvents();
            }
        }

        private void OnDisable()
        {
            // Отписка от событий руля
            if (_wheelInput != null)
            {
                _wheelInput.ThrottleCallback -= OnThrottle;
                _wheelInput.BrakeCallback -= OnBrake;
                _wheelInput.SteeringCallback -= OnSteering;
                _wheelInput.HandbrakeCallback -= OnHandbrake;
                _wheelInput.OnRightShiftCallback -= OnGearUp;
                _wheelInput.OnLeftShiftCallback -= OnGearDown;
                _wheelInput.OnEastButtonCallback -= OnNitro;

                // Отписываемся от событий механической коробки
                UnsubscribeFromManualGearEvents();
            }

            _inputActions?.Disable();
        }

        private void SubscribeToManualGearEvents()
        {
            // Подписываемся на события передач механической коробки
            _wheelInput.Shifter1Callback += OnShifter1;
            _wheelInput.Shifter2Callback += OnShifter2;
            _wheelInput.Shifter3Callback += OnShifter3;
            _wheelInput.Shifter4Callback += OnShifter4;
            _wheelInput.Shifter5Callback += OnShifter5;
            _wheelInput.Shifter6Callback += OnShifter6;
            _wheelInput.Shifter7Callback += OnShifter7;
        }

        private void UnsubscribeFromManualGearEvents()
        {
            // Отписываемся от событий передач механической коробки
            _wheelInput.Shifter1Callback -= OnShifter1;
            _wheelInput.Shifter2Callback -= OnShifter2;
            _wheelInput.Shifter3Callback -= OnShifter3;
            _wheelInput.Shifter4Callback -= OnShifter4;
            _wheelInput.Shifter5Callback -= OnShifter5;
            _wheelInput.Shifter6Callback -= OnShifter6;
            _wheelInput.Shifter7Callback -= OnShifter7;
        }

        private void Update()
        {
            if (!_enabled)
            {
                ResetInputs();
                return;
            }

            // Автоматическое определение активного устройства
            if (_currentMode == InputMode.Auto)
            {
                bool wheelConnected = IsWheelConnected();
                _currentMode = wheelConnected ? InputMode.Wheel : InputMode.InputSystem;
            }

            // Обновление ввода в зависимости от режима
            if (_currentMode == InputMode.InputSystem)
            {
                UpdateInputSystem();
            }
            else if (_currentMode == InputMode.Wheel)
            {
                // Для Wheel данные обновляются через события
                UpdateWheelInput();
            }

            // Обрабатываем переключения передач в зависимости от режима
            ProcessGearChanges();
        }

        private void UpdateInputSystem()
        {
            _gasInput = _inputActions.Vehicle.GasInput.ReadValue<float>();
            _brakeInput = _inputActions.Vehicle.BrakeInput.ReadValue<float>();
            _steeringInput = _inputActions.Vehicle.HorizontalInput.ReadValue<float>();

            _handbrakeInput = _inputActions.Vehicle.HandbrakeInput.ReadValue<float>() != 0;
            _nitroInput = _inputActions.Vehicle.NitroBoostInput.ReadValue<float>() != 0;

            if (_forceGasInputDuringNitrous && _nitroInput && _gasInput == 0)
                _gasInput = 1;

            // Для Input System используем только секвентальное переключение
            _gearUpInput = _inputActions.Vehicle.GearUpInput.WasPerformedThisFrame();
            _gearDownInput = _inputActions.Vehicle.GearDownInput.WasPerformedThisFrame();
        }

        private void UpdateWheelInput()
        {
            // Для Wheel mode данные обновляются через события (колбэки)
            // Здесь можно добавить дополнительную логику если нужно
        }

        private void ProcessGearChanges()
        {
            if (_currentMode == InputMode.Wheel && !_useSequentialShifting)
            {
                // Механическая коробка - новая логика
                ProcessManualShifting();
            }
            // Для Input System и секвентального режима используется стандартная логика
        }

        private void ProcessManualShifting()
        {
            // Проверяем, какая передача нажата в механической коробке
            int selectedGear = -1;

            for (int i = 0; i < _gearInputs.Length; i++)
            {
                if (_gearInputs[i])
                {
                    selectedGear = i;
                    break;
                }
            }

            // Если нажата какая-то передача и она отличается от текущей
            if (selectedGear != -1 && selectedGear != _currentGear)
            {
                Debug.Log($"Manual gear change: {_currentGear} -> {selectedGear}");
                _currentGear = selectedGear;
            }

            // Если не нажата ни одна передача - устанавливаем нейтраль
            if (selectedGear == -1 && _currentGear != 0)
            {
                _currentGear = 0;
                Debug.Log("Gear set to Neutral");
            }
        }

        #region Wheel Input Handlers
        private void OnThrottle(float value) => _gasInput = value;
        private void OnBrake(float value) => _brakeInput = value;
        private void OnSteering(float value) => _steeringInput = value;
        private void OnHandbrake(float value) => _handbrakeInput = value > 0.5f;

        private void OnGearUp(bool pressed)
        {
            if (_useSequentialShifting)
            {
                _gearUpInput = pressed;
            }
        }

        private void OnGearDown(bool pressed)
        {
            if (_useSequentialShifting)
            {
                _gearDownInput = pressed;
            }
        }

        private void OnNitro(bool pressed) => _nitroInput = pressed;
        #endregion

        #region Manual Gear Handlers
        private void OnShifter1(bool pressed)
        {
            _gearInputs[1] = pressed;
            Debug.Log($"Shifter 1: {pressed}");
        }

        private void OnShifter2(bool pressed)
        {
            _gearInputs[2] = pressed;
            Debug.Log($"Shifter 2: {pressed}");
        }

        private void OnShifter3(bool pressed)
        {
            _gearInputs[3] = pressed;
            Debug.Log($"Shifter 3: {pressed}");
        }

        private void OnShifter4(bool pressed)
        {
            _gearInputs[4] = pressed;
            Debug.Log($"Shifter 4: {pressed}");
        }

        private void OnShifter5(bool pressed)
        {
            _gearInputs[5] = pressed;
            Debug.Log($"Shifter 5: {pressed}");
        }

        private void OnShifter6(bool pressed)
        {
            _gearInputs[6] = pressed;
            Debug.Log($"Shifter 6: {pressed}");
        }

        private void OnShifter7(bool pressed)
        {
            _gearInputs[7] = pressed;
            Debug.Log($"Shifter 7: {pressed}");
        }
        #endregion

        private bool IsWheelConnected()
        {
            return _wheelInput != null &&
                   (Mathf.Abs(_wheelInput.Throttle) > 0.01f ||
                    Mathf.Abs(_wheelInput.Brake) > 0.01f ||
                    Mathf.Abs(_wheelInput.Steering) > 0.01f);
        }

        private void ResetInputs()
        {
            _gasInput = 0f;
            _brakeInput = 0f;
            _steeringInput = 0f;
            _handbrakeInput = false;
            _gearUpInput = false;
            _gearDownInput = false;
            _nitroInput = false;

            // Сбрасываем состояния передач механической коробки
            for (int i = 0; i < _gearInputs.Length; i++)
            {
                _gearInputs[i] = false;
            }
            _currentGear = 0;
        }

        #region IVehicleControllerInputProvider Implementation
        public void EnableInput(bool enable)
        {
            _enabled = enable;
            if (!enable) ResetInputs();
        }

        public float GetGasInput() => _gasInput;
        public float GetBrakeInput() => _brakeInput;
        public bool GetNitroBoostInput() => _nitroInput;
        public bool GetHandbrakeInput() => _handbrakeInput;
        public float GetHorizontalInput() => _steeringInput;

        public bool GetGearUpInput()
        {
            if (_useSequentialShifting || _currentMode == InputMode.InputSystem)
            {
                bool result = _gearUpInput;
                _gearUpInput = false; // Сбрасываем после чтения
                return result;
            }
            return false;
        }

        public bool GetGearDownInput()
        {
            if (_useSequentialShifting || _currentMode == InputMode.InputSystem)
            {
                bool result = _gearDownInput;
                _gearDownInput = false; // Сбрасываем после чтения
                return result;
            }
            return false;
        }

        public float GetPitchInput() => 0f;
        public float GetYawInput() => 0f;
        public float GetRollInput() => 0f;
        #endregion

        #region IManualTransmissionInputProvider Implementation
        // Новый метод для получения текущей передачи (для механической коробки)
        public int GetCurrentGear()
        {
            return _currentGear;
        }

        // Новый метод для проверки, включена ли механическая коробка
        public bool IsManualTransmission()
        {
            return _currentMode == InputMode.Wheel && !_useSequentialShifting;
        }
        #endregion

        #region ITransmissionTypeSettable Implementation
        // Метод для переключения типа коробки передач
        public void SetTransmissionType(bool useSequential)
        {
            _useSequentialShifting = useSequential;
            if (useSequential)
            {
                _currentGear = 0; // Сброс в нейтраль при переключении режима
            }
        }
        #endregion

        public void SetInputMode(InputMode mode) => _currentMode = mode;
        public InputMode GetCurrentInputMode() => _currentMode;
    }
}