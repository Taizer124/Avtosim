using UnityEngine;
using LogitechG29.Sample.Input;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Input/All-in-One Input Provider")]
    public class AllInOneInputProvider : MonoBehaviour, IVehicleControllerInputProvider
    {
        public enum InputMode { Keyboard, Wheel, Auto }
        public enum TransmissionMode { Automatic, Sequential, Manual }

        [Header("Settings")]
        [SerializeField] private InputMode _inputMode = InputMode.Auto;
        [SerializeField] private TransmissionMode _transmissionMode = TransmissionMode.Automatic;
        [SerializeField] private InputControllerReader _wheelInput;

        private PlayerVehicleInputActions _inputActions;
        private bool _initialized;
        private bool _enabled = true;

        private float _gas, _brake, _steer;
        private bool _handbrake, _gearUp, _gearDown, _nitro;
        private bool[] _manualGears = new bool[8];
        private int _currentGear;

        private float _lastWheelInputTime;
        private const float _wheelActivityTimeout = 2f;
        private bool _wasWheelConnected;

        public bool Return => _wheelInput != null && _wheelInput.Return;
        public bool NorthButton => _wheelInput != null && _wheelInput.NorthButton;
        public bool SouthButton => _wheelInput != null && _wheelInput.SouthButton;
        public bool EastButton => _wheelInput != null && _wheelInput.EastButton;

        private void Awake()
        {
            try
            {
                _inputActions = new PlayerVehicleInputActions();
                _inputActions.Enable();
                _initialized = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Input init failed: {e.Message}");
            }

            // При старте сразу выбираем клавиатуру
            _inputMode = InputMode.Keyboard;
        }

        private void OnEnable()
        {
            if (_wheelInput != null)
            {
                SubscribeWheel();
            }
        }

        private void OnDisable()
        {
            if (_wheelInput != null)
            {
                UnsubscribeWheel();
            }
        }

        private void Update()
        {
            if (!_enabled) { ResetInputs(); return; }

            if (_inputMode == InputMode.Auto)
            {
                DetectActiveInput();
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
            bool wheelConnected = false;

            if (_wheelInput != null)
            {
                // Проверка наличия объекта и факта подключения (если SDK это поддерживает)
                try
                {
                    var prop = _wheelInput.GetType().GetProperty("IsConnected");
                    if (prop != null)
                        wheelConnected = (bool)prop.GetValue(_wheelInput);
                    else
                        wheelConnected = true; // если такого свойства нет, считаем что подключен
                }
                catch
                {
                    wheelConnected = true;
                }
            }

            if (!wheelConnected)
            {
                _inputMode = InputMode.Keyboard;
                _wasWheelConnected = false;
                return;
            }

            bool wheelActive = false;

            if (_wheelInput != null)
            {
                // Проверяем реальные движения или нажатия
                if (Mathf.Abs(_wheelInput.Throttle) > 0.05f ||
                    Mathf.Abs(_wheelInput.Brake) > 0.05f ||
                    Mathf.Abs(_wheelInput.Steering) > 0.05f ||
                    _wheelInput.NorthButton || _wheelInput.SouthButton || _wheelInput.EastButton)
                {
                    _lastWheelInputTime = Time.time;
                    wheelActive = true;
                    _wasWheelConnected = true;
                }

                // Если активность недавно — руль активен
                if (Time.time - _lastWheelInputTime < _wheelActivityTimeout && _wasWheelConnected)
                {
                    _inputMode = InputMode.Wheel;
                    return;
                }
            }

            // Если руль неактивен слишком долго
            if (Time.time - _lastWheelInputTime >= _wheelActivityTimeout)
            {
                _inputMode = InputMode.Keyboard;
            }
        }

        private void UpdateKeyboard()
        {
            if (!_initialized) return;

            _gas = _inputActions.Vehicle.GasInput.ReadValue<float>();
            _brake = _inputActions.Vehicle.BrakeInput.ReadValue<float>();
            _steer = _inputActions.Vehicle.HorizontalInput.ReadValue<float>();
            _handbrake = _inputActions.Vehicle.HandbrakeInput.ReadValue<float>() > 0.5f;
            _nitro = _inputActions.Vehicle.NitroBoostInput.ReadValue<float>() > 0.5f;

            _gearUp = _inputActions.Vehicle.GearUpInput.WasPerformedThisFrame();
            _gearDown = _inputActions.Vehicle.GearDownInput.WasPerformedThisFrame();
        }

        private void UpdateWheel()
        {
            // Руль обновляется через коллбеки
        }

        private void SubscribeWheel()
        {
            _wheelInput.ThrottleCallback += v => { _gas = v; _lastWheelInputTime = Time.time; };
            _wheelInput.BrakeCallback += v => { _brake = v; _lastWheelInputTime = Time.time; };
            _wheelInput.SteeringCallback += v => { _steer = v; _lastWheelInputTime = Time.time; };
            _wheelInput.HandbrakeCallback += v => { _handbrake = v > 0.5f; _lastWheelInputTime = Time.time; };
            _wheelInput.OnRightShiftCallback += p => { if (_transmissionMode == TransmissionMode.Sequential) _gearUp = p; };
            _wheelInput.OnLeftShiftCallback += p => { if (_transmissionMode == TransmissionMode.Sequential) _gearDown = p; };
            _wheelInput.OnEastButtonCallback += p => { _nitro = p; };

            _wheelInput.Shifter1Callback += p => _manualGears[1] = p;
            _wheelInput.Shifter2Callback += p => _manualGears[2] = p;
            _wheelInput.Shifter3Callback += p => _manualGears[3] = p;
            _wheelInput.Shifter4Callback += p => _manualGears[4] = p;
            _wheelInput.Shifter5Callback += p => _manualGears[5] = p;
            _wheelInput.Shifter6Callback += p => _manualGears[6] = p;
            _wheelInput.Shifter7Callback += p => _manualGears[7] = p;
        }

        private void UnsubscribeWheel()
        {
            _wheelInput.ThrottleCallback -= v => _gas = v;
            _wheelInput.BrakeCallback -= v => _brake = v;
            _wheelInput.SteeringCallback -= v => _steer = v;
            _wheelInput.HandbrakeCallback -= v => _handbrake = v > 0.5f;
            _wheelInput.OnRightShiftCallback -= p => { if (_transmissionMode == TransmissionMode.Sequential) _gearUp = p; };
            _wheelInput.OnLeftShiftCallback -= p => { if (_transmissionMode == TransmissionMode.Sequential) _gearDown = p; };
            _wheelInput.OnEastButtonCallback -= p => _nitro = p;
        }

        private void ProcessManualShifting()
        {
            int selected = -1;
            for (int i = 1; i < _manualGears.Length; i++)
            {
                if (_manualGears[i]) { selected = i; break; }
            }
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
            if (_transmissionMode == TransmissionMode.Sequential || _inputMode == InputMode.Keyboard)
            {
                bool res = _gearUp;
                _gearUp = false;
                return res;
            }
            return false;
        }

        public bool GetGearDownInput()
        {
            if (_transmissionMode == TransmissionMode.Sequential || _inputMode == InputMode.Keyboard)
            {
                bool res = _gearDown;
                _gearDown = false;
                return res;
            }
            return false;
        }

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

        public int GetCurrentGear() => _currentGear;
        public bool IsManualTransmission() => _inputMode == InputMode.Wheel && _transmissionMode == TransmissionMode.Manual;

        public float GetPitchInput() => 0f;
        public float GetYawInput() => 0f;
        public float GetRollInput() => 0f;
        #endregion

        public void SetTransmissionMode(TransmissionMode mode) => _transmissionMode = mode;
        public TransmissionMode GetTransmissionMode() => _transmissionMode;

        public void SetInputMode(InputMode mode) => _inputMode = mode;
        public InputMode GetCurrentInputMode() => _inputMode;
    }
}
