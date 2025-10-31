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

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = true;

        private PlayerVehicleInputActions _inputActions;
        private bool _isInitialized = false;

        // Общие переменные ввода
        private float _gasInput;
        private float _brakeInput;
        private float _steeringInput;
        private bool _handbrakeInput;
        private bool _gearUpInput;
        private bool _gearDownInput;
        private bool _nitroInput;

        // Для механической коробки
        private bool[] _gearInputs = new bool[8]; // 0-N, 1–7 передачи
        private int _currentGear = 0;

        private bool _enabled = true;

        // Свойства для DemoManager
        public bool Return => false;
        public bool NorthButton => _gearUpInput;
        public bool SouthButton => _gearDownInput;
        public bool EastButton => _nitroInput;

        // Для отладки
        public bool IsWheelAssigned => _wheelInput != null;
        public float DebugThrottle => _wheelInput != null ? _wheelInput.Throttle : -1f;
        public float DebugSteering => _wheelInput != null ? _wheelInput.Steering : -1f;

        private void Awake()
        {
            InitializeInputSystem();
        }

        private void Start()
        {
            Debug.Log($"AllInOneInputProvider Started - Mode: {_currentMode}, Wheel Assigned: {_wheelInput != null}");
        }

        private void OnEnable()
        {
            if (!_isInitialized)
            {
                InitializeInputSystem();
            }

            if (_wheelInput != null)
            {
                SubscribeToWheelEvents();
                Debug.Log("AllInOneInputProvider: Subscribed to wheel events");
            }
            else
            {
                Debug.LogWarning("AllInOneInputProvider: Wheel input is not assigned!");
            }
        }

        private void OnDisable()
        {
            if (_wheelInput != null)
            {
                UnsubscribeFromWheelEvents();
                Debug.Log("AllInOneInputProvider: Unsubscribed from wheel events");
            }

            if (!gameObject.activeInHierarchy)
            {
                _inputActions?.Disable();
            }
        }

        private void OnDestroy()
        {
            _inputActions?.Disable();
            _inputActions = null;
            _isInitialized = false;
        }

        public void ReinitializeInputSystem()
        {
            _inputActions?.Disable();
            _inputActions = null;
            _isInitialized = false;

            InitializeInputSystem();
            Debug.Log("Input system reinitialized");
        }

        private void InitializeInputSystem()
        {
            if (_isInitialized) return;

            try
            {
                _inputActions = new PlayerVehicleInputActions();
                _inputActions.Enable();
                _isInitialized = true;
                Debug.Log("Input system initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize input system: {e.Message}");
            }
        }

        private void Update()
        {
            if (!_enabled)
            {
                ResetInputs();
                return;
            }

            // Автоопределение режима
            if (_currentMode == InputMode.Auto)
            {
                bool wheelConnected = IsWheelConnected();
                InputMode previousMode = _currentMode;
                _currentMode = wheelConnected ? InputMode.Wheel : InputMode.InputSystem;

                if (previousMode != _currentMode && _enableDebugLogs)
                {
                    Debug.Log($"Auto mode switched to: {_currentMode}");
                }
            }

            // Обновляем ввод в зависимости от режима
            if (_currentMode == InputMode.InputSystem)
            {
                UpdateInputSystem();
            }
            else if (_currentMode == InputMode.Wheel)
            {
                UpdateWheelInput();
            }

            // Логика передач
            ProcessGearChanges();

            // Отладочная информация
            if (_enableDebugLogs && Time.frameCount % 120 == 0)
            {
                Debug.Log($"Input - Mode: {_currentMode}, Gas: {_gasInput:F2}, Brake: {_brakeInput:F2}, Steering: {_steeringInput:F2}, " +
                         $"GearUp: {_gearUpInput}, GearDown: {_gearDownInput}, Nitro: {_nitroInput}, CurrentGear: {_currentGear}");
            }
        }

        #region --- Input Updates ---
        private void UpdateInputSystem()
        {
            if (!_isInitialized || _inputActions == null)
            {
                if (_enableDebugLogs)
                    Debug.LogWarning("Input system not initialized!");
                return;
            }

            try
            {
                _gasInput = _inputActions.Vehicle.GasInput.ReadValue<float>();
                _brakeInput = _inputActions.Vehicle.BrakeInput.ReadValue<float>();
                _steeringInput = _inputActions.Vehicle.HorizontalInput.ReadValue<float>();

                _handbrakeInput = _inputActions.Vehicle.HandbrakeInput.ReadValue<float>() > 0.5f;
                _nitroInput = _inputActions.Vehicle.NitroBoostInput.ReadValue<float>() > 0.5f;

                if (_forceGasInputDuringNitrous && _nitroInput && _gasInput == 0)
                    _gasInput = 1;

                // Для клавиатуры используем только секвентальное переключение
                _gearUpInput = _inputActions.Vehicle.GearUpInput.WasPerformedThisFrame();
                _gearDownInput = _inputActions.Vehicle.GearDownInput.WasPerformedThisFrame();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error reading input system: {e.Message}");
            }
        }

        private void UpdateWheelInput()
        {
            // Данные руля обновляются через события, но здесь можно добавить дополнительную логику
            if (_wheelInput == null)
            {
                Debug.LogWarning("Wheel input is null in Wheel mode!");
                return;
            }
        }
        #endregion

        #region --- Wheel Event Subscriptions ---
        private void SubscribeToWheelEvents()
        {
            if (_wheelInput == null)
            {
                Debug.LogError("Cannot subscribe to wheel events - wheel input is null!");
                return;
            }

            _wheelInput.ThrottleCallback += OnThrottle;
            _wheelInput.BrakeCallback += OnBrake;
            _wheelInput.SteeringCallback += OnSteering;
            _wheelInput.HandbrakeCallback += OnHandbrake;
            _wheelInput.OnRightShiftCallback += OnGearUp;
            _wheelInput.OnLeftShiftCallback += OnGearDown;
            _wheelInput.OnEastButtonCallback += OnNitro;

            // Передачи механики
            _wheelInput.Shifter1Callback += OnShifter1;
            _wheelInput.Shifter2Callback += OnShifter2;
            _wheelInput.Shifter3Callback += OnShifter3;
            _wheelInput.Shifter4Callback += OnShifter4;
            _wheelInput.Shifter5Callback += OnShifter5;
            _wheelInput.Shifter6Callback += OnShifter6;
            _wheelInput.Shifter7Callback += OnShifter7;

            Debug.Log("Successfully subscribed to all wheel events");
        }

        private void UnsubscribeFromWheelEvents()
        {
            if (_wheelInput == null) return;

            _wheelInput.ThrottleCallback -= OnThrottle;
            _wheelInput.BrakeCallback -= OnBrake;
            _wheelInput.SteeringCallback -= OnSteering;
            _wheelInput.HandbrakeCallback -= OnHandbrake;
            _wheelInput.OnRightShiftCallback -= OnGearUp;
            _wheelInput.OnLeftShiftCallback -= OnGearDown;
            _wheelInput.OnEastButtonCallback -= OnNitro;

            _wheelInput.Shifter1Callback -= OnShifter1;
            _wheelInput.Shifter2Callback -= OnShifter2;
            _wheelInput.Shifter3Callback -= OnShifter3;
            _wheelInput.Shifter4Callback -= OnShifter4;
            _wheelInput.Shifter5Callback -= OnShifter5;
            _wheelInput.Shifter6Callback -= OnShifter6;
            _wheelInput.Shifter7Callback -= OnShifter7;
        }
        #endregion

        #region --- Gear Logic ---
        private void ProcessGearChanges()
        {
            if (_currentMode == InputMode.Wheel && !_useSequentialShifting)
            {
                ProcessManualShifting();
            }
        }

        private void ProcessManualShifting()
        {
            int selectedGear = -1;

            for (int i = 0; i < _gearInputs.Length; i++)
            {
                if (_gearInputs[i])
                {
                    selectedGear = i;
                    break;
                }
            }

            if (selectedGear != -1 && selectedGear != _currentGear)
            {
                _currentGear = selectedGear;
                if (_enableDebugLogs)
                    Debug.Log($"Manual gear change: {_currentGear}");
            }

            if (selectedGear == -1 && _currentGear != 0)
            {
                _currentGear = 0;
                if (_enableDebugLogs)
                    Debug.Log("Gear set to Neutral");
            }
        }
        #endregion

        #region --- Wheel Event Handlers ---
        private void OnThrottle(float value)
        {
            _gasInput = value;
            if (_enableDebugLogs && Time.frameCount % 60 == 0)
                Debug.Log($"Throttle: {value:F2}");
        }

        private void OnBrake(float value)
        {
            _brakeInput = value;
            if (_enableDebugLogs && Time.frameCount % 60 == 0)
                Debug.Log($"Brake: {value:F2}");
        }

        private void OnSteering(float value)
        {
            _steeringInput = value;
            if (_enableDebugLogs && Time.frameCount % 60 == 0)
                Debug.Log($"Steering: {value:F2}");
        }

        private void OnHandbrake(float value)
        {
            _handbrakeInput = value > 0.5f;
            if (_enableDebugLogs && value > 0.5f)
                Debug.Log($"Handbrake: {value:F2}");
        }

        private void OnGearUp(bool pressed)
        {
            if (_useSequentialShifting)
            {
                _gearUpInput = pressed;
                if (_enableDebugLogs && pressed)
                    Debug.Log("GearUp pressed");
            }
        }

        private void OnGearDown(bool pressed)
        {
            if (_useSequentialShifting)
            {
                _gearDownInput = pressed;
                if (_enableDebugLogs && pressed)
                    Debug.Log("GearDown pressed");
            }
        }

        private void OnNitro(bool pressed)
        {
            _nitroInput = pressed;
            if (_enableDebugLogs && pressed)
                Debug.Log("Nitro pressed");
        }

        private void OnShifter1(bool pressed) { _gearInputs[1] = pressed; if (_enableDebugLogs && pressed) Debug.Log("Shifter 1"); }
        private void OnShifter2(bool pressed) { _gearInputs[2] = pressed; if (_enableDebugLogs && pressed) Debug.Log("Shifter 2"); }
        private void OnShifter3(bool pressed) { _gearInputs[3] = pressed; if (_enableDebugLogs && pressed) Debug.Log("Shifter 3"); }
        private void OnShifter4(bool pressed) { _gearInputs[4] = pressed; if (_enableDebugLogs && pressed) Debug.Log("Shifter 4"); }
        private void OnShifter5(bool pressed) { _gearInputs[5] = pressed; if (_enableDebugLogs && pressed) Debug.Log("Shifter 5"); }
        private void OnShifter6(bool pressed) { _gearInputs[6] = pressed; if (_enableDebugLogs && pressed) Debug.Log("Shifter 6"); }
        private void OnShifter7(bool pressed) { _gearInputs[7] = pressed; if (_enableDebugLogs && pressed) Debug.Log("Shifter 7"); }
        #endregion

        private bool IsWheelConnected()
        {
            if (_wheelInput == null)
            {
                return false;
            }

            // Более надежная проверка - считаем руль подключенным если он назначен в инспекторе
            // и не проверяем значения осей (они могут быть нулевыми изначально)
            bool isConnected = _wheelInput != null;

            if (_enableDebugLogs && Time.frameCount % 120 == 0)
            {
                Debug.Log($"Wheel connected: {isConnected}, Throttle: {_wheelInput.Throttle:F2}, Steering: {_wheelInput.Steering:F2}");
            }

            return isConnected;
        }

        private void ResetInputs()
        {
            _gasInput = _brakeInput = _steeringInput = 0f;
            _handbrakeInput = _gearUpInput = _gearDownInput = _nitroInput = false;
            for (int i = 0; i < _gearInputs.Length; i++)
                _gearInputs[i] = false;
            _currentGear = 0;
        }

        #region --- Interface Implementations ---
        public void EnableInput(bool enable)
        {
            _enabled = enable;
            if (!enable) ResetInputs();

            Debug.Log($"Input {(enable ? "enabled" : "disabled")}");
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

        public int GetCurrentGear() => _currentGear;
        public bool IsManualTransmission() => _currentMode == InputMode.Wheel && !_useSequentialShifting;

        public void SetTransmissionType(bool useSequential)
        {
            _useSequentialShifting = useSequential;
            if (useSequential)
                _currentGear = 0;

            Debug.Log($"Transmission type set to: {(useSequential ? "Sequential" : "Manual")}");
        }

        public void SetInputMode(InputMode mode)
        {
            _currentMode = mode;
            Debug.Log($"Input mode set to: {mode}");
        }

        public InputMode GetCurrentInputMode() => _currentMode;

        public bool IsInputSystemInitialized() => _isInitialized;
        #endregion

        [ContextMenu("Reinitialize Input System")]
        private void ReinitializeFromContextMenu()
        {
            ReinitializeInputSystem();
        }

        [ContextMenu("Debug Input State")]
        private void DebugInputState()
        {
            Debug.Log($"=== INPUT DEBUG ===\n" +
                     $"Mode: {_currentMode}\n" +
                     $"Wheel Assigned: {_wheelInput != null}\n" +
                     $"Gas: {_gasInput:F2}\n" +
                     $"Brake: {_brakeInput:F2}\n" +
                     $"Steering: {_steeringInput:F2}\n" +
                     $"Handbrake: {_handbrakeInput}\n" +
                     $"Nitro: {_nitroInput}\n" +
                     $"GearUp: {_gearUpInput}\n" +
                     $"GearDown: {_gearDownInput}\n" +
                     $"Current Gear: {_currentGear}\n" +
                     $"Input System Initialized: {_isInitialized}");
        }
    }
}