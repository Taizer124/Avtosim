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
        private bool _initialized = true;
        private bool _enabled = true;

        private float _gas, _brake, _steer;
        private bool _handbrake, _gearUp, _gearDown, _nitro;
        private bool[] _manualGears = new bool[8];
        private int _currentGear;

        private float _lastWheelInputTime;
        private const float _wheelActivityTimeout = 2f;
        private bool _wasWheelConnected;

        // --- Êíîïêè ðóëÿ ---
        public bool Return => _wheelInput != null && _wheelInput.Return;
        public bool NorthButton => _wheelInput != null && _wheelInput.NorthButton;
        public bool SouthButton => _wheelInput != null && _wheelInput.SouthButton;
        public bool EastButton => _wheelInput != null && _wheelInput.EastButton;
        public bool WestButton => _wheelInput != null && _wheelInput.WestButton;

        public event System.Action OnEastPressed;
        public event System.Action OnWestPressed;
        public event System.Action OnNorthPressed;
        public event System.Action OnSouthPressed;

        private float _buttonCooldown = 0.4f;
        private float _lastButtonPressTime = 0f;

        private void OnEnable()
        {
            if (_wheelInput != null) SubscribeWheel();
        }

        private void OnDisable()
        {
            if (_wheelInput != null) UnsubscribeWheel();
        }

        private void Update()
        {
            if (!_enabled) { ResetInputs(); return; }

            if (_inputMode == InputMode.Auto)
            {
                DetectActiveInput();
                if (_textInputSys != null)
                    _textInputSys.text = _inputMode.ToString();
            }

            if (_inputMode == InputMode.Keyboard)
                UpdateKeyboard();
            else if (_inputMode == InputMode.Wheel)
                UpdateWheel();

            if (_inputMode == InputMode.Wheel && _transmissionMode == TransmissionMode.Manual)
                ProcessManualShifting();
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
                _wheelInput.NorthButton || _wheelInput.SouthButton ||
                _wheelInput.EastButton || _wheelInput.WestButton)
            {
                _lastWheelInputTime = Time.time;
                _wasWheelConnected = true;
                _inputMode = InputMode.Wheel;
                return;
            }

            if (Time.time - _lastWheelInputTime >= _wheelActivityTimeout)
                _inputMode = InputMode.Keyboard;
        }

        // === ÊËÀÂÈÀÒÓÐÀ ×ÅÐÅÇ ÑÒÀÐÛÉ INPUT MANAGER ===
        private void UpdateKeyboard()
        {
            _gas = Mathf.Clamp01(Input.GetAxis("Vertical"));
            _brake = Mathf.Clamp01(-Input.GetAxis("Vertical"));
            _steer = Input.GetAxis("Horizontal");

            if (enableHandbrakeInput)
                _handbrake = Input.GetKey(KeyCode.Space);
            else
                _handbrake = false;

            _nitro = Input.GetKey(KeyCode.LeftShift);
            _gearUp = Input.GetKeyDown(KeyCode.E);
            _gearDown = Input.GetKeyDown(KeyCode.Q);
        }

        private void UpdateWheel() { }

        // --- Ïîäïèñêè ---
        private void SubscribeWheel()
        {
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
        }

        private void UnsubscribeWheel()
        {
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
        }

        // --- Wheel Input Callbacks ---
        private void Wheel_OnThrottle(float v) { _gas = v; _lastWheelInputTime = Time.time; }
        private void Wheel_OnBrake(float v) { _brake = v; _lastWheelInputTime = Time.time; }
        private void Wheel_OnSteering(float v) { _steer = v; _lastWheelInputTime = Time.time; }
        private void Wheel_OnHandbrake(float v)
        {
            if (enableHandbrakeInput)
                _handbrake = v > handbrakeDeadzone;
            else
                _handbrake = false;
            _lastWheelInputTime = Time.time;
        }

        private void Wheel_OnRightShift(bool p) { if (_transmissionMode == TransmissionMode.Sequential) _gearUp = p; }
        private void Wheel_OnLeftShift(bool p) { if (_transmissionMode == TransmissionMode.Sequential) _gearDown = p; }

        private void Wheel_OnEast(bool p)
        {
            if (p && Time.time - _lastButtonPressTime > _buttonCooldown)
            {
                _lastButtonPressTime = Time.time;
                Debug.Log("EAST pressed — call drivetrain switch");
                OnEastPressed?.Invoke();
            }
        }

        private void Wheel_OnWest(bool p)
        {
            if (p && Time.time - _lastButtonPressTime > _buttonCooldown)
            {
                _lastButtonPressTime = Time.time;
                OnWestPressed?.Invoke();
            }
        }

        private void Wheel_OnNorth(bool p)
        {
            if (p && Time.time - _lastButtonPressTime > _buttonCooldown)
            {
                _lastButtonPressTime = Time.time;
                OnNorthPressed?.Invoke();
            }
        }

        private void Wheel_OnSouth(bool p)
        {
            if (p && Time.time - _lastButtonPressTime > _buttonCooldown)
            {
                _lastButtonPressTime = Time.time;
                OnSouthPressed?.Invoke();
            }
        }

        private void ProcessManualShifting()
        {
            int selected = -1;
            for (int i = 1; i < _manualGears.Length; i++)
                if (_manualGears[i]) { selected = i; break; }
            _currentGear = selected != -1 ? selected : 0;
        }

        private void ResetInputs()
        {
            _gas = _brake = _steer = 0;
            _handbrake = _gearUp = _gearDown = _nitro = false;
            for (int i = 0; i < _manualGears.Length; i++) _manualGears[i] = false;
            _currentGear = 0;
        }

        #region Interface
        public void EnableInput(bool enable)
        {
            _enabled = enable;
            if (!enable) ResetInputs();
        }

        public float GetGasInput() => _gas;
        public float GetBrakeInput() => _brake;
        public bool GetNitroBoostInput() => _nitro;
        public bool GetHandbrakeInput() => _handbrake;
        public float GetHorizontalInput() => _steer;

        public bool GetGearUpInput()
        {
            bool res = _gearUp;
            _gearUp = false;
            return res;
        }

        public bool GetGearDownInput()
        {
            bool res = _gearDown;
            _gearDown = false;
            return res;
        }

        public int GetCurrentGear() => _currentGear;
        public bool IsManualTransmission() => _transmissionMode == TransmissionMode.Manual;
        public float GetPitchInput() => 0f;
        public float GetYawInput() => 0f;
        public float GetRollInput() => 0f;

        // --- ÂÎÑÑÒÀÍÎÂË¨ÍÍÛÉ ÌÅÒÎÄ ---
        public void ReinitializeInputSystem()
        {
            if (_inputActions == null)
                _inputActions = new PlayerVehicleInputActions();

            try
            {
                _inputActions.Enable();
                _initialized = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"ReinitializeInputSystem failed: {e.Message}");
                _initialized = false;
            }

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
