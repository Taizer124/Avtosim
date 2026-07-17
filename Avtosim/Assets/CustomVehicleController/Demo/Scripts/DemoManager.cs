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
        [SerializeField] private MonoBehaviour _wheelInputProvider;
        private IVehicleControllerInputProvider _inputProvider;
        private AllInOneInputProvider _allInOneProvider; // ����� ������ ���� ������������ AllInOneInputProvider


        private VehicleEngineSoundManager _engineSoundManager;

        [Header("Presets & Sounds")]
        [SerializeField] private CarEngineSoundSO[] _engineSoundSOArray;
        [SerializeField] private VehiclePartsPresetSO[] _vehiclePartsPressets;
        private int _currentPresetId = -1;


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

        // ��������� ������
        private bool _westButtonPressed;
        private bool _northButtonPressed;
        private bool _southButtonPressed;
        private bool _eastButtonPressed;

        private System.Type _wheelInputType;
        private System.Reflection.PropertyInfo _westButtonProp;
        private System.Reflection.PropertyInfo _northButtonProp;
        private System.Reflection.PropertyInfo _southButtonProp;
        private System.Reflection.PropertyInfo _eastButtonProp;

        // cooldown ��� ������
        private float _buttonCooldown = 0.4f;
        private float _lastButtonTime = 0f;

        // ���� �����
        public bool IsPaused { get; private set; }

        // Блокировка ручного тюнинга на время гонки: pre-race меню фиксирует
        // выбранные коробку/привод, поэтому переключать их с руля (North/East),
        // менять пресет (South) и перезагружать сцену (West) во время заезда
        // нельзя. Само городское меню сохраняется — блокировка снимается на
        // финише гонки.
        private bool _tuningLocked;
        public void SetTuningLocked(bool locked) => _tuningLocked = locked;

        private void Start()
        {
            ResolvePlayerReferences();

            if (_vehicleController == null)
            {
                Debug.LogError("[DemoManager] Активный игрок не найден (нет машины с тегом Player). DemoManager отключён.");
                enabled = false;
                return;
            }

            _engineSoundManager = _vehicleController.GetComponent<VehicleEngineSoundManager>();
            VehiclePartsSetWrapper.OnAnyPresetChanged += VehiclePartsSetWrapper_OnAnyPresetChanged;

            _partsIdArray = new int[8];
            _vehicleController.UsePreset = false;

            // ��������� ������
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
            _audioMixer.SetFloat("MasterVolume", Mathf.Log(_currentVolume) * 20);
            InitializeInputProvider();
        }

        private void OnDestroy()
        {
            VehiclePartsSetWrapper.OnAnyPresetChanged -= VehiclePartsSetWrapper_OnAnyPresetChanged;
        }

        // Раньше _vehicleController и _wheelInputProvider жёстко задавались в
        // инспекторе на ОДНУ конкретную машину. DemoManager — сценовый синглтон
        // (не внутри префаба машины), поэтому при выборе другой машины его
        // инспекторная ссылка указывала бы на СТАРУЮ (теперь выключенную)
        // машину. Поэтому активный игрок (машина с тегом Player от
        // PlayerCarSelector) ПРИОРИТЕТНЕЕ инспектора: если он есть — берём его
        // и его input-провайдер. Инспектор используется только как fallback для
        // сцен без селектора (демо), где активного тегованного игрока нет.
        private void ResolvePlayerReferences()
        {
            CustomVehicleController active = PlayerLocator.GetActivePlayer();
            if (active != null)
            {
                _vehicleController = active;

                AllInOneInputProvider provider = active.GetComponentInChildren<AllInOneInputProvider>();
                if (provider != null)
                    _wheelInputProvider = provider;
            }

            // fallback: провайдер с уже назначенной машины, если выше не нашли.
            if (_wheelInputProvider == null && _vehicleController != null)
                _wheelInputProvider = _vehicleController.GetComponentInChildren<AllInOneInputProvider>();
        }

        private void InitializeInputProvider()
        {
            if (_wheelInputProvider == null)
            {
                Debug.LogWarning("Wheel Input Provider not assigned in DemoManager");
                return;
            }

            _inputProvider = _wheelInputProvider as IVehicleControllerInputProvider;
            _allInOneProvider = _wheelInputProvider as AllInOneInputProvider;

            //  ����: ������ if (_inputProvider == null) - ������ �������������� ���������
            _wheelInputType = _wheelInputProvider.GetType();
            _westButtonProp = _wheelInputType.GetProperty("WestButton");
            _northButtonProp = _wheelInputType.GetProperty("NorthButton");
            _southButtonProp = _wheelInputType.GetProperty("SouthButton");
            _eastButtonProp = _wheelInputType.GetProperty("EastButton");

            Debug.Log($" ��������� ������: West={_westButtonProp != null}");
        }


        private void Update()
        {
            if (IsPaused)
                return;

            UpdateStatsMenu();
            HandleAudio();
            UpdateWheelButtonStates();

            // R/T/Y/U теперь приходят через Input Actions (карта Buttons,
            // экшены West/North/South/East) наравне с Logitech и MOZA —
            // _westButtonPressed/etc уже объединяют все подключённые
            // устройства, отдельная проверка Input.GetKeyDown не нужна.
            if (_tuningLocked)
            {
                // Идёт гонка — тюнинг зафиксирован pre-race меню. Гасим
                // накопленные нажатия, чтобы они не сработали разом на финише
                // при разблокировке.
                _westButtonPressed = _northButtonPressed = _southButtonPressed = _eastButtonPressed = false;
            }
            else
            {
                bool canPress = (Time.time - _lastButtonTime > _buttonCooldown);

                if (_westButtonPressed && canPress)
                {
                    SceneManager.LoadScene("MVP");
                    _lastButtonTime = Time.time;
                    _westButtonPressed = false;
                }

                if (_northButtonPressed && canPress)
                {
                    SwapTransmissionType();
                    _lastButtonTime = Time.time;
                    _northButtonPressed = false;
                }

                if (_southButtonPressed && canPress)
                {
                    SwapPreset();
                    _lastButtonTime = Time.time;
                    _southButtonPressed = false;
                }

                if (_eastButtonPressed && canPress)
                {
                    SwapDrivetrainType();
                    _lastButtonTime = Time.time;
                    _eastButtonPressed = false;
                }
            }

            OpenCloseMenus(_vehicleControlsMenu, KeyCode.F1, _demoControlsMenu);
            OpenCloseMenus(_demoControlsMenu, KeyCode.F2, _vehicleControlsMenu);
            OpenCloseMenus(_staticUIParent, KeyCode.F9);
            OpenCloseMenus(_dynamicUIParent, KeyCode.F9);
            HandlePartsMenu();
        }

        public void SetPauseState(bool paused)
        {
            IsPaused = paused;
        }

        private void HandleAudio()
        {
            if (IsPaused)
                return;

            if (Input.GetKeyDown(KeyCode.Minus))
                _currentVolume -= 0.1f;

            if (Input.GetKeyDown(KeyCode.Equals))
                _currentVolume += 0.1f;

            _currentVolume = Mathf.Clamp(_currentVolume, 0.001f, 1);
            _audioMixer.SetFloat("MasterVolume", Mathf.Log(_currentVolume) * 20);
        }

        private void UpdateWheelButtonStates()
        {
            if (_wheelInputProvider == null)
                return;

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
            catch { }
        }


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

            Debug.Log($"Transmission switched to: {_vehicleController.TransmissionType}");

            // ���� ������������ AllInOneInputProvider, �������������� ����� ����������� � ��� ����
            if (_allInOneProvider != null)
            {
                switch (_vehicleController.TransmissionType)
                {
                    case TransmissionType.Automatic:
                        _allInOneProvider.SetTransmissionMode(AllInOneInputProvider.TransmissionMode.Automatic);
                        break;
                    case TransmissionType.Sequential:
                        _allInOneProvider.SetTransmissionMode(AllInOneInputProvider.TransmissionMode.Sequential);
                        break;
                    case TransmissionType.Manual:
                        _allInOneProvider.SetTransmissionMode(AllInOneInputProvider.TransmissionMode.Manual);
                        break;
                }
            }
        }

        private void SwapPreset()
        {
            if (_vehiclePartsPressets.Length == 0)
                return;

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
        }

        private void OpenCloseMenus(GameObject menu, KeyCode key, GameObject conflict = null)
        {
            if (Input.GetKeyDown(key))
            {
                if (conflict != null && conflict.activeSelf)
                    conflict.SetActive(false);

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
    }
}
