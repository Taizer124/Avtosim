using UnityEngine;
using LogitechG29.Sample.Input;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Input/Vehicle Controller Wheel Input Provider")]
    public class VehicleControllerWheelInputProvider : MonoBehaviour, IVehicleControllerInputProvider
    {
        [Header("Wheel Input Settings")]
        [SerializeField] private InputControllerReader _wheelInput;

        public enum TransmissionMode { Automatic, Sequential, Manual }

        [Header("Transmission Mode")]
        [SerializeField] private TransmissionMode _transmissionMode = TransmissionMode.Automatic;

        private float _gasInput;
        private float _brakeInput;
        private float _steeringInput;
        private bool _handbrakeInput;
        private bool _gearUpInput;
        private bool _gearDownInput;
        private bool _nitroInput;
        private bool[] _gearInputs = new bool[8];
        private int _currentGear;
        private bool _enabled = true;

        private float _buttonCooldown = 0.25f;
        private float _lastEastPress, _lastWestPress, _lastNorthPress, _lastSouthPress;

        // ✅ Публичные события — могут быть вызваны DemoManager-ом
        public event System.Action OnEastPressed;
        public event System.Action OnWestPressed;
        public event System.Action OnNorthPressed;
        public event System.Action OnSouthPressed;

        // ✅ Публичные флаги
        public bool EastButton { get; private set; }
        public bool WestButton { get; private set; }
        public bool NorthButton { get; private set; }
        public bool SouthButton { get; private set; }

        private void OnEnable()
        {
            if (_wheelInput == null)
            {
                Debug.LogError("Wheel Input not assigned in VehicleControllerWheelInputProvider!");
                return;
            }
            SubscribeToWheelEvents();
        }

        private void OnDisable()
        {
            if (_wheelInput == null) return;
            UnsubscribeFromWheelEvents();
        }

        private void SubscribeToWheelEvents()
        {
            _wheelInput.ThrottleCallback += OnThrottle;
            _wheelInput.BrakeCallback += OnBrake;
            _wheelInput.SteeringCallback += OnSteering;
            _wheelInput.OnRightShiftCallback += OnGearUp;
            _wheelInput.OnLeftShiftCallback += OnGearDown;

            // ✅ кнопки
            _wheelInput.OnEastButtonCallback += OnEast;
            _wheelInput.OnWestButtonCallback += OnWest;
            _wheelInput.OnNorthButtonCallback += OnNorth;
            _wheelInput.OnSouthButtonCallback += OnSouth;

            // ✅ шифтеры
            _wheelInput.Shifter1Callback += OnShifter1;
            _wheelInput.Shifter2Callback += OnShifter2;
            _wheelInput.Shifter3Callback += OnShifter3;
            _wheelInput.Shifter4Callback += OnShifter4;
            _wheelInput.Shifter5Callback += OnShifter5;
            _wheelInput.Shifter6Callback += OnShifter6;
            _wheelInput.Shifter7Callback += OnShifter7;
        }

        private void UnsubscribeFromWheelEvents()
        {
            _wheelInput.ThrottleCallback -= OnThrottle;
            _wheelInput.BrakeCallback -= OnBrake;
            _wheelInput.SteeringCallback -= OnSteering;
            _wheelInput.OnRightShiftCallback -= OnGearUp;
            _wheelInput.OnLeftShiftCallback -= OnGearDown;

            _wheelInput.OnEastButtonCallback -= OnEast;
            _wheelInput.OnWestButtonCallback -= OnWest;
            _wheelInput.OnNorthButtonCallback -= OnNorth;
            _wheelInput.OnSouthButtonCallback -= OnSouth;

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
            if (!_enabled || _wheelInput == null)
            {
                ResetInputs();
                return;
            }

            switch (_transmissionMode)
            {
                case TransmissionMode.Manual:
                    ProcessManualShifting();
                    break;
                case TransmissionMode.Automatic:
                    _currentGear = 1;
                    break;
            }
        }

        private void ProcessManualShifting()
        {
            int selectedGear = -1;
            for (int i = 1; i < _gearInputs.Length; i++)
            {
                if (_gearInputs[i]) { selectedGear = i; break; }
            }
            _currentGear = selectedGear != -1 ? selectedGear : 0;
        }

        private void ResetInputs()
        {
            _gasInput = _brakeInput = _steeringInput = 0f;
            _gearUpInput = _gearDownInput = _nitroInput = false;
            for (int i = 0; i < _gearInputs.Length; i++) _gearInputs[i] = false;
            _currentGear = 0;
        }

        #region Callbacks
        private void OnThrottle(float v) => _gasInput = v;
        private void OnBrake(float v) => _brakeInput = v;
        private void OnSteering(float v) => _steeringInput = v;
        private void OnGearUp(bool p) { if (_transmissionMode == TransmissionMode.Sequential) _gearUpInput = p; }
        private void OnGearDown(bool p) { if (_transmissionMode == TransmissionMode.Sequential) _gearDownInput = p; }

        // ✅ Кнопки теперь обрабатываются отдельно с независимым cooldown
        private void OnEast(bool isPressed)
        {
            if (isPressed && Time.time - _lastEastPress > _buttonCooldown)
            {
                _lastEastPress = Time.time;
                EastButton = true;
                Debug.Log("EAST button pressed — switching drivetrain type");
                OnEastPressed?.Invoke();
            }
        }

        private void OnWest(bool isPressed)
        {
            if (isPressed && Time.time - _lastWestPress > _buttonCooldown)
            {
                _lastWestPress = Time.time;
                WestButton = true;
                Debug.Log("WEST button pressed");
                OnWestPressed?.Invoke();
            }
        }

        private void OnNorth(bool isPressed)
        {
            if (isPressed && Time.time - _lastNorthPress > _buttonCooldown)
            {
                _lastNorthPress = Time.time;
                NorthButton = true;
                Debug.Log("NORTH button pressed");
                OnNorthPressed?.Invoke();
            }
        }

        private void OnSouth(bool isPressed)
        {
            if (isPressed && Time.time - _lastSouthPress > _buttonCooldown)
            {
                _lastSouthPress = Time.time;
                SouthButton = true;
                Debug.Log("SOUTH button pressed");
                OnSouthPressed?.Invoke();
            }
        }

        private void OnShifter1(bool p) => _gearInputs[1] = p;
        private void OnShifter2(bool p) => _gearInputs[2] = p;
        private void OnShifter3(bool p) => _gearInputs[3] = p;
        private void OnShifter4(bool p) => _gearInputs[4] = p;
        private void OnShifter5(bool p) => _gearInputs[5] = p;
        private void OnShifter6(bool p) => _gearInputs[6] = p;
        private void OnShifter7(bool p) => _gearInputs[7] = p;
        #endregion

        #region Interface Implementation
        public void EnableInput(bool enable)
        {
            _enabled = enable;
            if (!enable) ResetInputs();
        }

        public float GetGasInput() => _gasInput;
        public float GetBrakeInput() => _brakeInput;
        public bool GetNitroBoostInput() => _nitroInput;
        public float GetHorizontalInput() => _steeringInput;
        public bool GetHandbrakeInput() => _handbrakeInput;
        public float GetPitchInput() => 0f;
        public float GetYawInput() => 0f;
        public float GetRollInput() => 0f;

        public bool GetGearUpInput()
        {
            if (_transmissionMode == TransmissionMode.Sequential)
            {
                bool res = _gearUpInput;
                _gearUpInput = false;
                return res;
            }
            return false;
        }

        public bool GetGearDownInput()
        {
            if (_transmissionMode == TransmissionMode.Sequential)
            {
                bool res = _gearDownInput;
                _gearDownInput = false;
                return res;
            }
            return false;
        }

        public int GetCurrentGear() => _currentGear;
        public bool IsManualTransmission() => _transmissionMode == TransmissionMode.Manual;
        #endregion

        public void SetTransmissionMode(TransmissionMode mode) => _transmissionMode = mode;
        public TransmissionMode GetTransmissionMode() => _transmissionMode;
    }
}
