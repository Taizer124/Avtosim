using LogitechG29.Sample.Input;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Input/All-in-One Input Provider")]
    public class AllInOneInputProvider : MonoBehaviour, IVehicleControllerInputProvider
    {
        public enum InputMode { Keyboard, Wheel, Auto }
        public enum TransmissionMode { Automatic, Sequential, Manual }

        [Header("Settings")]
        [SerializeField] private InputMode _inputMode = InputMode.Auto;
        [SerializeField] private TextMeshProUGUI _textInputSys;
        [SerializeField] private TransmissionMode _transmissionMode = TransmissionMode.Automatic;
        [SerializeField] private InputControllerReader _wheelInput;

        [Header("Optional Controls")]
        [SerializeField] private bool enableHandbrakeInput = true;
        [SerializeField, Range(0f, 1f)] private float handbrakeDeadzone = 0.3f;

        private PlayerVehicleInputActions _inputActions;
        private bool _initialized = false;
        private bool _enabled = true;

        // ✅ СЦЕПЛЕНИЕ
        private float _clutchInput;

        private float _gas, _brake, _steer;
        private bool _handbrake, _gearUp, _gearDown, _nitro;
        private bool[] _manualGears = new bool[8];
        private int _currentGear;

        private float _lastWheelInputTime;
        private const float _wheelActivityTimeout = 2f;
        private bool _wasWheelConnected;

        // --- Локальное состояние кнопок (Кэш) ---
        private bool _northPressedRaw;
        private bool _southPressedRaw;
        private bool _eastPressedRaw;
        private bool _westPressedRaw;

        // --- Исправленные свойства ---
        public bool Return => _wheelInput != null && _wheelInput.Return;
        public bool NorthButton => _northPressedRaw;
        public bool SouthButton => _southPressedRaw;
        public bool EastButton => _eastPressedRaw;
        public bool WestButton => _westPressedRaw;

        public event System.Action<float> OnBrakeChanged;
        public event System.Action OnEastPressed;
        public event System.Action OnWestPressed;
        public event System.Action OnNorthPressed;
        public event System.Action OnSouthPressed;

        // Кулдаун
        private float _buttonCooldown = 0.4f;
        private float _lastEastTime, _lastWestTime, _lastNorthTime, _lastSouthTime;

        private void Awake()
        {
            _inputActions = new PlayerVehicleInputActions();
        }

        private void OnEnable()
        {
            if (_inputActions == null) _inputActions = new PlayerVehicleInputActions();
            try
            {
                _inputActions.Enable();
                _initialized = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"AllInOneInputProvider: failed to enable input actions: {e.Message}");
                _initialized = false;
            }

            if (_wheelInput != null) SubscribeWheel();
        }

        private void OnDisable()
        {
            if (_wheelInput != null) UnsubscribeWheel();
            if (_inputActions != null) _inputActions.Disable();
            _initialized = false;
        }

        private void Update()
        {
            if (!_enabled)
            {
                ResetInputs();
                return;
            }

            if (_inputMode == InputMode.Auto)
            {
                DetectActiveInput();
                if (_textInputSys != null)
                    _textInputSys.text = _inputMode.ToString();
            }

            if (_inputMode == InputMode.Keyboard)
                UpdateKeyboard();
            // ✅ Wheel + Manual с сцеплением
            else if (_inputMode == InputMode.Wheel)
            {
                if (_transmissionMode == TransmissionMode.Manual)
                    ProcessManualShifting();
                else if (_transmissionMode == TransmissionMode.Sequential)
                    ProcessSequentialShifting();
            }
        }

        private void DetectActiveInput()
        {
            bool wheelConnected = _wheelInput != null;

            if (!wheelConnected)
            {
                _inputMode = InputMode.Keyboard;
                _wasWheelConnected = false;
                return;
            }

            if (Mathf.Abs(_wheelInput.Throttle) > 0.05f ||
                Mathf.Abs(_wheelInput.Brake) > 0.05f ||
                Mathf.Abs(_wheelInput.Steering) > 0.05f ||
                _northPressedRaw || _southPressedRaw ||
                _eastPressedRaw || _westPressedRaw)
            {
                _lastWheelInputTime = Time.time;
                _wasWheelConnected = true;
                _inputMode = InputMode.Wheel;
                return;
            }

            if (Time.time - _lastWheelInputTime >= _wheelActivityTimeout)
                _inputMode = InputMode.Keyboard;
        }

        // === КЛАВИАТУРА ===
        private void UpdateKeyboard()
        {
            if (_inputActions == null)
            {
                _gas = Mathf.Clamp01(Input.GetAxis("Vertical"));
                _brake = Mathf.Clamp01(-Input.GetAxis("Vertical"));
                _steer = Input.GetAxis("Horizontal");
                _handbrake = enableHandbrakeInput && Input.GetKey(KeyCode.Space);
                _nitro = Input.GetKey(KeyCode.LeftShift);
                _gearUp = Input.GetKeyDown(KeyCode.E);
                _gearDown = Input.GetKeyDown(KeyCode.Q);
                _clutchInput = Input.GetKey(KeyCode.C) ? 0f : 1f; // C = сцепление
                return;
            }

            _gas = _inputActions.Vehicle.GasInput.ReadValue<float>();
            _brake = _inputActions.Vehicle.BrakeInput.ReadValue<float>();
            _steer = _inputActions.Vehicle.HorizontalInput.ReadValue<float>();
            _handbrake = enableHandbrakeInput && (_inputActions.Vehicle.HandbrakeInput.ReadValue<float>() != 0);
            _nitro = _inputActions.Vehicle.NitroBoostInput.ReadValue<float>() != 0;
            _gearUp = _inputActions.Vehicle.GearUpInput.WasPerformedThisFrame();
            _gearDown = _inputActions.Vehicle.GearDownInput.WasPerformedThisFrame();
        }

        // --- Подписки ---
        private void SubscribeWheel()
        {
            _wheelInput.ClutchCallback += Wheel_OnClutch; //  СЦЕПЛЕНИЕ
            _wheelInput.ThrottleCallback += Wheel_OnThrottle;
            _wheelInput.BrakeCallback += Wheel_OnBrake;
            _wheelInput.SteeringCallback += Wheel_OnSteering;
            _wheelInput.HandbrakeCallback += Wheel_OnHandbrake;
            _wheelInput.OnRightShiftCallback += Wheel_OnRightShift;
            _wheelInput.OnLeftShiftCallback += Wheel_OnLeftShift;

            _wheelInput.OnEastButtonCallback += Wheel_OnEast;
            _wheelInput.OnWestButtonCallback += Wheel_OnWest;
            _wheelInput.OnNorthButtonCallback += Wheel_OnNorth;
            _wheelInput.OnSouthButtonCallback += Wheel_OnSouth;

            // Shifters
            _wheelInput.Shifter1Callback += Wheel_OnShifter1;
            _wheelInput.Shifter2Callback += Wheel_OnShifter2;
            _wheelInput.Shifter3Callback += Wheel_OnShifter3;
            _wheelInput.Shifter4Callback += Wheel_OnShifter4;
            _wheelInput.Shifter5Callback += Wheel_OnShifter5;
            _wheelInput.Shifter6Callback += Wheel_OnShifter6;
            _wheelInput.Shifter7Callback += Wheel_OnShifter7;
        }

        private void UnsubscribeWheel()
        {
            if (_wheelInput == null) return;

            _wheelInput.ClutchCallback -= Wheel_OnClutch; // ✅ СЦЕПЛЕНИЕ
            _wheelInput.ThrottleCallback -= Wheel_OnThrottle;
            _wheelInput.BrakeCallback -= Wheel_OnBrake;
            _wheelInput.SteeringCallback -= Wheel_OnSteering;
            _wheelInput.HandbrakeCallback -= Wheel_OnHandbrake;
            _wheelInput.OnRightShiftCallback -= Wheel_OnRightShift;
            _wheelInput.OnLeftShiftCallback -= Wheel_OnLeftShift;

            _wheelInput.OnEastButtonCallback -= Wheel_OnEast;
            _wheelInput.OnWestButtonCallback -= Wheel_OnWest;
            _wheelInput.OnNorthButtonCallback -= Wheel_OnNorth;
            _wheelInput.OnSouthButtonCallback -= Wheel_OnSouth;

            _wheelInput.Shifter1Callback -= Wheel_OnShifter1;
            _wheelInput.Shifter2Callback -= Wheel_OnShifter2;
            _wheelInput.Shifter3Callback -= Wheel_OnShifter3;
            _wheelInput.Shifter4Callback -= Wheel_OnShifter4;
            _wheelInput.Shifter5Callback -= Wheel_OnShifter5;
            _wheelInput.Shifter6Callback -= Wheel_OnShifter6;
            _wheelInput.Shifter7Callback -= Wheel_OnShifter7;
        }

        // --- Wheel Input Callbacks ---
        private void Wheel_OnClutch(float clutchValue)
        {
            _clutchInput = clutchValue;
            Debug.Log($"Clutch: {_clutchInput:F2}");
        }

        private void Wheel_OnThrottle(float v) { _gas = v; _lastWheelInputTime = Time.time; }
        private void Wheel_OnBrake(float v) { _brake = v; OnBrakeChanged?.Invoke(v); _lastWheelInputTime = Time.time; }
        private void Wheel_OnSteering(float v) { _steer = v; _lastWheelInputTime = Time.time; }
        private void Wheel_OnHandbrake(float v)
        {
            _handbrake = enableHandbrakeInput && (v > handbrakeDeadzone);
            _lastWheelInputTime = Time.time;
        }

        private void Wheel_OnRightShift(bool p) { if (_transmissionMode == TransmissionMode.Sequential) _gearUp = p; }
        private void Wheel_OnLeftShift(bool p) { if (_transmissionMode == TransmissionMode.Sequential) _gearDown = p; }

        // --- КНОПКИ ---
        private void Wheel_OnEast(bool p)
        {
            _eastPressedRaw = p;
            if (!p) return;
            if (Time.time - _lastEastTime < _buttonCooldown) return;
            _lastEastTime = Time.time;
            Debug.Log("EAST pressed");
            OnEastPressed?.Invoke();
        }

        private void Wheel_OnWest(bool p)
        {
            _westPressedRaw = p;
            if (!p) return;
            if (Time.time - _lastWestTime < _buttonCooldown) return;
            _lastWestTime = Time.time;
            Debug.Log("WEST pressed");
            OnWestPressed?.Invoke();
        }

        private void Wheel_OnNorth(bool p)
        {
            _northPressedRaw = p;
            if (!p) return;
            if (Time.time - _lastNorthTime < _buttonCooldown) return;
            _lastNorthTime = Time.time;
            Debug.Log("NORTH pressed");
            OnNorthPressed?.Invoke();
        }

        private void Wheel_OnSouth(bool p)
        {
            _southPressedRaw = p;
            if (!p) return;
            if (Time.time - _lastSouthTime < _buttonCooldown) return;
            _lastSouthTime = Time.time;
            Debug.Log("SOUTH pressed");
            OnSouthPressed?.Invoke();
        }

        private void Wheel_OnShifter1(bool pressed) => _manualGears[1] = pressed;
        private void Wheel_OnShifter2(bool pressed) => _manualGears[2] = pressed;
        private void Wheel_OnShifter3(bool pressed) => _manualGears[3] = pressed;
        private void Wheel_OnShifter4(bool pressed) => _manualGears[4] = pressed;
        private void Wheel_OnShifter5(bool pressed) => _manualGears[5] = pressed;
        private void Wheel_OnShifter6(bool pressed) => _manualGears[6] = pressed;
        private void Wheel_OnShifter7(bool pressed) => _manualGears[7] = pressed;

        // ✅ СЦЕПЛЕНИЕ + Manual
        private void ProcessManualShifting()
        {
            int selectedGear = -1;
            for (int i = 1; i < _manualGears.Length; i++)
            {
                if (_manualGears[i]) { selectedGear = i; break; }
            }

            // СЦЕПЛЕНИЕ: если разомкнуто - нейтраль
            if (_clutchInput < 0.1f)
            {
                _currentGear = 0;
                return;
            }

            _currentGear = selectedGear != -1 ? selectedGear : 0;
        }

        private void ProcessSequentialShifting()
        {
            if (_clutchInput < 0.1f)
            {
                _currentGear = 0;
                return;
            }
            // Sequential логика (+1/-1)
        }

        private void ResetInputs()
        {
            _gas = _brake = _steer = _clutchInput = 0;
            _handbrake = _gearUp = _gearDown = _nitro = false;
            _northPressedRaw = _southPressedRaw = _eastPressedRaw = _westPressedRaw = false;
            for (int i = 0; i < _manualGears.Length; i++) _manualGears[i] = false;
            _currentGear = 0;
        }

        #region Interface
        public void EnableInput(bool enable)
        {
            _enabled = enable;
            if (!enable) ResetInputs();
        }

        // ✅ СЦЕПЛЕНИЕ
        public float GetClutchInput() => _clutchInput;

        public float GetGasInput() => _gas;
        public float GetBrakeInput() => _brake;
        public bool GetNitroBoostInput() => _nitro;
        public bool GetHandbrakeInput() => _handbrake;
        public float GetHorizontalInput() => _steer;

        public bool GetGearUpInput() { bool res = _gearUp; _gearUp = false; return res; }
        public bool GetGearDownInput() { bool res = _gearDown; _gearDown = false; return res; }

        public int GetCurrentGear() => _currentGear;
        public bool IsManualTransmission() => _transmissionMode == TransmissionMode.Manual;
        public float GetPitchInput() => 0f;
        public float GetYawInput() => 0f;
        public float GetRollInput() => 0f;

        public void ReinitializeInputSystem()
        {
            if (_inputActions == null) _inputActions = new PlayerVehicleInputActions();
            try { _inputActions.Enable(); _initialized = true; }
            catch { _initialized = false; }
            ResetInputs();
            _inputMode = InputMode.Auto;
        }

        public void SetTransmissionMode(TransmissionMode mode) => _transmissionMode = mode;
        public TransmissionMode GetTransmissionMode() => _transmissionMode;
        public void SetInputMode(InputMode mode) => _inputMode = mode;
        public InputMode GetCurrentInputMode() => _inputMode;
        #endregion
    }
}
