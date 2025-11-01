using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.VehicleController
{
    public class DemoManager : MonoBehaviour
    {
        [Header("Vehicle")]
        [SerializeField] private CustomVehicleController _vehicleController;

        [Header("Input Provider")]
        [SerializeField] private MonoBehaviour _wheelInputProvider; // универсальная ссылка (WheelInputProvider / AllInOneInputProvider)
        private IVehicleControllerInputProvider _inputProvider;

        private VehicleEngineSoundManager _engineSoundManager;

        [Header("Presets & Sounds")]
        [SerializeField] private CarEngineSoundSO[] _engineSoundSOArray;
        [SerializeField] private VehiclePartsPresetSO[] _vehiclePartsPressets;
        private int _currentPresetId = -1;

        [Header("Cameras")]
        [SerializeField] private Camera[] _cameraArray;
        private int _currentCameraID = 0;
        private string[] _cameraNameArray = { "Orbit", "Hood", "Top Down" };

        [Header("UI Groups")]
        [SerializeField] private GameObject _staticUIParent;
        [SerializeField] private GameObject _dynamicUIParent;
        [SerializeField] private GameObject _vehicleControlsMenu;
        [SerializeField] private GameObject _demoControlsMenu;
        [SerializeField] private GameObject _partsMenu;
        [SerializeField] private GameObject _currentPartsStaticMenu;
        [SerializeField] private GameObject _currentPartsDynamicMenu;

        [Header("UI Text")]
        [SerializeField] private Text _currentEngine;
        [SerializeField] private Text _currentNitro;
        [SerializeField] private Text _currentTransmission;
        [SerializeField] private Text _currentTires;
        [SerializeField] private Text _currentSuspension;
        [SerializeField] private Text _currentBrakes;
        [SerializeField] private Text _currentBody;
        [SerializeField] private Text _currentFI;
        [SerializeField] private Text _transmissionType;
        [SerializeField] private Text _drivetrainType;
        [SerializeField] private Text _presetType;
        [SerializeField] private Text _cameraTypeName;
        [SerializeField] private Text _enginePartName;

        [Header("Audio")]
        [SerializeField] private AudioMixer _audioMixer;
        private float _currentVolume = 0.7f;

        [Header("Vehicle Parts")]
        [SerializeField] private EngineSO[] _engineArray;
        [SerializeField] private ForcedInductionSO[] _forcedInductionArray;
        [SerializeField] private NitrousSO[] _nitrousArray;
        [SerializeField] private TransmissionSO[] _transmissionArray;
        [SerializeField] private SuspensionSO[] _suspensionArray;
        [SerializeField] private TiresSO[] _tireArray;
        [SerializeField] private BrakesSO[] _brakesArray;
        [SerializeField] private VehicleBodySO[] _vehicleBodyArray;
        [SerializeField] private CustomEnginePart[] _customEngineParts;

        private int _currentEnginePartID;
        private int[] _partsIdArray;

        // состояния кнопок (заменили Return на West для перезапуска)
        private bool _westButtonPressed;
        private bool _northButtonPressed;
        private bool _southButtonPressed;
        private bool _eastButtonPressed;

        // reflection fallback
        private System.Type _wheelInputType;
        private System.Reflection.PropertyInfo _westButtonProp;
        private System.Reflection.PropertyInfo _northButtonProp;
        private System.Reflection.PropertyInfo _southButtonProp;
        private System.Reflection.PropertyInfo _eastButtonProp;

        private void Start()
        {
            _engineSoundManager = _vehicleController.GetComponent<VehicleEngineSoundManager>();
            VehiclePartsSetWrapper.OnAnyPresetChanged += VehiclePartsSetWrapper_OnAnyPresetChanged;

            _partsIdArray = new int[8];
            _vehicleController.UsePreset = false;

            // начальные детали
            _vehicleController.SetNewPartToCustomizableSet(_engineArray[0]);
            _vehicleController.SetNewPartToCustomizableSet(_forcedInductionArray[0]);
            _vehicleController.SetNewPartToCustomizableSet(_nitrousArray[0]);
            _vehicleController.SetNewPartToCustomizableSet(_transmissionArray[0]);
            _vehicleController.SetNewPartToCustomizableSet(_tireArray[0], true);
            _vehicleController.SetNewPartToCustomizableSet(_tireArray[0], false);
            _vehicleController.SetNewPartToCustomizableSet(_suspensionArray[0], true);
            _vehicleController.SetNewPartToCustomizableSet(_suspensionArray[0], false);
            _vehicleController.SetNewPartToCustomizableSet(_brakesArray[0]);
            _vehicleController.SetNewPartToCustomizableSet(_vehicleBodyArray[0]);

            UpdatePartsMenu();
            _audioMixer.SetFloat("AudioVolume", Mathf.Log(_currentVolume) * 20);
            InitializeInputProvider();
        }

        private void OnDestroy()
        {
            VehiclePartsSetWrapper.OnAnyPresetChanged -= VehiclePartsSetWrapper_OnAnyPresetChanged;
        }

        private void InitializeInputProvider()
        {
            if (_wheelInputProvider == null)
            {
                Debug.LogWarning("Wheel Input Provider not assigned in DemoManager");
                return;
            }

            _inputProvider = _wheelInputProvider as IVehicleControllerInputProvider;
            if (_inputProvider == null)
            {
                _wheelInputType = _wheelInputProvider.GetType();
                // reflection properties: теперь берём West вместо Return
                _westButtonProp = _wheelInputType.GetProperty("WestButton");
                _northButtonProp = _wheelInputType.GetProperty("NorthButton");
                _southButtonProp = _wheelInputType.GetProperty("SouthButton");
                _eastButtonProp = _wheelInputType.GetProperty("EastButton");
            }
        }

        private void Update()
        {
            UpdateStatsMenu();
            HandleAudio();
            HandlePartsChanges();
            UpdateWheelButtonStates();

            // Перезапуск сцены — через West (или клавиша R)
            if (Input.GetKeyDown(KeyCode.R) || _westButtonPressed)
            {
                SceneManager.LoadScene("mcp_day");
                _westButtonPressed = false;
            }

            if (Input.GetKeyDown(KeyCode.T) || _northButtonPressed)
            {
                SwapTransmissionType();
                _northButtonPressed = false;
            }

            if (Input.GetKeyDown(KeyCode.Y) || _southButtonPressed)
            {
                SwapPreset();
                _southButtonPressed = false;
            }

            if (Input.GetKeyDown(KeyCode.U) || _eastButtonPressed)
            {
                SwapDrivetrainType();
                _eastButtonPressed = false;
            }

            if (Input.GetKeyDown(KeyCode.V)) ChangeCamera();

            OpenCloseMenus(_vehicleControlsMenu, KeyCode.F1, _demoControlsMenu);
            OpenCloseMenus(_demoControlsMenu, KeyCode.F2, _vehicleControlsMenu);
            OpenCloseMenus(_staticUIParent, KeyCode.F9);
            OpenCloseMenus(_dynamicUIParent, KeyCode.F9);
            HandlePartsMenu();
        }

        // === ЦИКЛИЧЕСКОЕ ПЕРЕКЛЮЧЕНИЕ 3 РЕЖИМОВ КОРОБКИ ===
        public void SwapTransmissionType()
        {
            switch (_vehicleController.TransmissionType)
            {
                case TransmissionType.Automatic:
                    _vehicleController.TransmissionType = TransmissionType.Sequential;
                    break;
                case TransmissionType.Sequential:
                    _vehicleController.TransmissionType = TransmissionType.Manual;
                    break;
                default:
                    _vehicleController.TransmissionType = TransmissionType.Automatic;
                    break;
            }

            // синхронизация с провайдерами
            if (_wheelInputProvider is VehicleControllerWheelInputProvider wheelProvider)
            {
                switch (_vehicleController.TransmissionType)
                {
                    case TransmissionType.Automatic:
                        wheelProvider.SetTransmissionMode(VehicleControllerWheelInputProvider.TransmissionMode.Automatic);
                        break;
                    case TransmissionType.Sequential:
                        wheelProvider.SetTransmissionMode(VehicleControllerWheelInputProvider.TransmissionMode.Sequential);
                        break;
                    case TransmissionType.Manual:
                        wheelProvider.SetTransmissionMode(VehicleControllerWheelInputProvider.TransmissionMode.Manual);
                        break;
                }
            }
            else if (_wheelInputProvider is AllInOneInputProvider allInOne)
            {
                switch (_vehicleController.TransmissionType)
                {
                    case TransmissionType.Automatic:
                        allInOne.SetTransmissionMode(AllInOneInputProvider.TransmissionMode.Automatic);
                        break;
                    case TransmissionType.Sequential:
                        allInOne.SetTransmissionMode(AllInOneInputProvider.TransmissionMode.Sequential);
                        break;
                    case TransmissionType.Manual:
                        allInOne.SetTransmissionMode(AllInOneInputProvider.TransmissionMode.Manual);
                        break;
                }
            }

            Debug.Log($"Transmission switched to: {_vehicleController.TransmissionType}");
        }

        private void UpdateWheelButtonStates()
        {
            if (_wheelInputProvider == null) return;

            if (_inputProvider != null)
            {
                // если используем интерфейс провайдера, он даёт только нужную функциональность:
                _northButtonPressed = _inputProvider.GetGearUpInput();
                _southButtonPressed = _inputProvider.GetGearDownInput();
                _eastButtonPressed = _inputProvider.GetNitroBoostInput();
                // нет стандартного GetWestInput в интерфейсе — используем false (reflection fallback покрывает большинство wheel readers)
                _westButtonPressed = false;
                return;
            }

            // fallback через reflection (для ScriptableObject InputControllerReader и т.п.)
            try
            {
                if (_westButtonProp != null)
                    _westButtonPressed = (bool)_westButtonProp.GetValue(_wheelInputProvider);
                if (_northButtonProp != null)
                    _northButtonPressed = (bool)_northButtonProp.GetValue(_wheelInputProvider);
                if (_southButtonProp != null)
                    _southButtonPressed = (bool)_southButtonProp.GetValue(_wheelInputProvider);
                if (_eastButtonProp != null)
                    _eastButtonPressed = (bool)_eastButtonProp.GetValue(_wheelInputProvider);
            }
            catch
            {
                // безопасно игнорируем ошибки reflection
            }
        }

        private void SwapPreset()
        {
            if (_vehiclePartsPressets.Length == 0) return;

            _currentPresetId++;
            if (_currentPresetId >= _vehiclePartsPressets.Length)
            {
                _vehicleController.UsePreset = false;
                _currentPresetId = -1;
                UpdateEngineSoundFromPart();
            }
            else
            {
                _vehicleController.SetVehiclePresetSO(_vehiclePartsPressets[_currentPresetId]);
                _vehicleController.UsePreset = true;
                UpdateEngineSoundFromPreset();
            }
        }

        private void SwapDrivetrainType()
        {
            _vehicleController.DrivetrainType = GetNextDrivetrainType(_vehicleController.DrivetrainType);
        }

        private DrivetrainType GetNextDrivetrainType(DrivetrainType current)
        {
            return current switch
            {
                DrivetrainType.RWD => DrivetrainType.AWD,
                DrivetrainType.AWD => DrivetrainType.FWD,
                _ => DrivetrainType.RWD,
            };
        }

        private void HandleAudio()
        {
            if (Input.GetKeyDown(KeyCode.Minus)) _currentVolume -= 0.1f;
            if (Input.GetKeyDown(KeyCode.Equals)) _currentVolume += 0.1f;
            _currentVolume = Mathf.Clamp(_currentVolume, 0.001f, 1);
            _audioMixer.SetFloat("AudioVolume", Mathf.Log(_currentVolume) * 20);
        }

        private void HandlePartsChanges()
        {
            ChangeEngine();
            ChangeNitrous();
            ChangeTransmission();
            ChangeSuspension();
            ChangeTires();
            ChangeBrakes();
            ChangeBody();
            ChangeEnginePart();
            ChangeFI();
        }

        private void ChangeEnginePart()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha8)) return;
            _enginePartName.text = _customEngineParts[_currentEnginePartID].name;
            _vehicleController.SetNewEnginePart(_customEngineParts[_currentEnginePartID]);
            _currentEnginePartID = (_currentEnginePartID + 1) % _customEngineParts.Length;
        }

        private void ChangeEngine()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha1)) return;
            if (_vehicleController.UsePreset) return;
            _partsIdArray[0] = (_partsIdArray[0] + 1) % _engineArray.Length;
            _vehicleController.SetNewPartToCustomizableSet(_engineArray[_partsIdArray[0]]);
            UpdateEngineSoundFromPart();
        }

        private void ChangeNitrous()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha2)) return;
            if (_vehicleController.UsePreset) return;
            _partsIdArray[1] = (_partsIdArray[1] + 1) % _nitrousArray.Length;
            _vehicleController.SetNewPartToCustomizableSet(_nitrousArray[_partsIdArray[1]]);
        }

        private void ChangeTransmission()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha3)) return;
            if (_vehicleController.UsePreset) return;
            _partsIdArray[2] = (_partsIdArray[2] + 1) % _transmissionArray.Length;
            _vehicleController.SetNewPartToCustomizableSet(_transmissionArray[_partsIdArray[2]]);
        }

        private void ChangeTires()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha4)) return;
            if (_vehicleController.UsePreset) return;
            _partsIdArray[3] = (_partsIdArray[3] + 1) % _tireArray.Length;
            _vehicleController.SetNewPartToCustomizableSet(_tireArray[_partsIdArray[3]], true);
            _vehicleController.SetNewPartToCustomizableSet(_tireArray[_partsIdArray[3]], false);
        }

        private void ChangeSuspension()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha5)) return;
            if (_vehicleController.UsePreset) return;
            _partsIdArray[4] = (_partsIdArray[4] + 1) % _suspensionArray.Length;
            _vehicleController.SetNewPartToCustomizableSet(_suspensionArray[_partsIdArray[4]], true);
            _vehicleController.SetNewPartToCustomizableSet(_suspensionArray[_partsIdArray[4]], false);
        }

        private void ChangeBrakes()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha6)) return;
            if (_vehicleController.UsePreset) return;
            _partsIdArray[5] = (_partsIdArray[5] + 1) % _brakesArray.Length;
            _vehicleController.SetNewPartToCustomizableSet(_brakesArray[_partsIdArray[5]]);
        }

        private void ChangeBody()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha7)) return;
            if (_vehicleController.UsePreset) return;
            _partsIdArray[6] = (_partsIdArray[6] + 1) % _vehicleBodyArray.Length;
            _vehicleController.SetNewPartToCustomizableSet(_vehicleBodyArray[_partsIdArray[6]]);
        }

        private void ChangeFI()
        {
            if (!Input.GetKeyDown(KeyCode.Alpha9)) return;
            if (_vehicleController.UsePreset) return;
            _partsIdArray[7] = (_partsIdArray[7] + 1) % _forcedInductionArray.Length;
            if (_forcedInductionArray[_partsIdArray[7]] != null)
                _vehicleController.SetNewPartToCustomizableSet(_forcedInductionArray[_partsIdArray[7]]);
            else
                _vehicleController.RemoveForcedInduction();
        }

        private void UpdateEngineSoundFromPart()
        {
            _engineSoundManager.SetNewCarEngineSoundSO(_engineSoundSOArray[_partsIdArray[0]]);
        }

        private void UpdateEngineSoundFromPreset()
        {
            int id = Mathf.Clamp(_currentPresetId, 0, _engineSoundSOArray.Length - 1);
            _engineSoundManager.SetNewCarEngineSoundSO(_engineSoundSOArray[id]);
        }

        private void VehiclePartsSetWrapper_OnAnyPresetChanged() => UpdatePartsMenu();

        private void UpdateStatsMenu()
        {
            _transmissionType.text = _vehicleController.TransmissionType.ToString();
            _presetType.text = _vehicleController.UsePreset ? _vehicleController.GetVehiclePreset().name : "Custom";
            _drivetrainType.text = _vehicleController.DrivetrainType.ToString();
            _cameraTypeName.text = _cameraNameArray[_currentCameraID];
        }

        private void OpenCloseMenus(GameObject menu, KeyCode key, GameObject conflict = null)
        {
            if (Input.GetKeyDown(key))
            {
                if (conflict != null && conflict.activeSelf) conflict.SetActive(false);
                menu.SetActive(!menu.activeSelf);
            }
        }

        private void HandlePartsMenu()
        {
            _partsMenu.SetActive(!_vehicleController.UsePreset);
            if (_vehicleController.UsePreset)
            {
                _currentPartsStaticMenu.SetActive(false);
                _currentPartsDynamicMenu.SetActive(false);
                return;
            }

            OpenCloseMenus(_currentPartsStaticMenu, KeyCode.F3);
            OpenCloseMenus(_currentPartsDynamicMenu, KeyCode.C);
        }

        private void UpdatePartsMenu()
        {
            VehiclePartsCustomizableSet parts = _vehicleController.GetCustomizableSet();
            _currentEngine.text = parts.Engine.name;
            _currentNitro.text = parts.Nitrous.name;
            _currentTransmission.text = parts.Transmission.name;
            _currentTires.text = parts.FrontTires.name;
            _currentSuspension.text = parts.FrontSuspension.name;
            _currentBrakes.text = parts.Brakes.name;
            _currentBody.text = parts.Body.name;
            _currentFI.text = parts.ForcedInduction == null ? "None" : parts.ForcedInduction.name;
        }

        private void ChangeCamera()
        {
            _cameraArray[_currentCameraID].gameObject.SetActive(false);
            _currentCameraID = (_currentCameraID + 1) % _cameraArray.Length;
            _cameraArray[_currentCameraID].gameObject.SetActive(true);
        }
    }
}
