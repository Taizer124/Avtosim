using UnityEngine;
using LogitechG29.Sample.Input;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Input/Vehicle Controller Wheel Input Provider")]
    public class VehicleControllerWheelInputProvider : MonoBehaviour, IVehicleControllerInputProvider
    {
        [Header("Wheel Input Settings")]
        [SerializeField] private InputControllerReader _wheelInput;

        [Header("Input Mapping")]
        [SerializeField] private bool _useSequentialShifting = false;

        // Для хранения состояний ввода
        private float _gasInput;
        private float _brakeInput;
        private float _steeringInput;
        private bool _handbrakeInput;
        private bool _gearUpInput;
        private bool _gearDownInput;
        private bool _nitroInput;

        // Свойства для DemoManager
        public bool Return => false;
        public bool NorthButton => _gearUpInput;
        public bool SouthButton => _gearDownInput;
        public bool EastButton => _nitroInput;

        // Для механической коробки передач
        private bool[] _gearInputs = new bool[8];
        private int _currentGear = 0;
        private bool _clutchInput = false;
        private bool _enabled = true;

        // ДЛЯ ОТЛАДКИ
        public bool IsWheelAssigned => _wheelInput != null;
        public float DebugThrottle => _wheelInput != null ? _wheelInput.Throttle : -1f;
        public float DebugSteering => _wheelInput != null ? _wheelInput.Steering : -1f;

        private void OnEnable()
        {
            if (_wheelInput == null)
            {
                Debug.LogError("Wheel Input not assigned in VehicleControllerWheelInputProvider!");
                return;
            }

            Debug.Log("VehicleControllerWheelInputProvider: Subscribing to wheel events");

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
            _wheelInput.HandbrakeCallback += OnHandbrake;
            _wheelInput.OnRightShiftCallback += OnGearUp;
            _wheelInput.OnLeftShiftCallback += OnGearDown;
            _wheelInput.OnEastButtonCallback += OnNitro;

            SubscribeToManualGearEvents();
        }

        private void UnsubscribeFromWheelEvents()
        {
            _wheelInput.ThrottleCallback -= OnThrottle;
            _wheelInput.BrakeCallback -= OnBrake;
            _wheelInput.SteeringCallback -= OnSteering;
            _wheelInput.HandbrakeCallback -= OnHandbrake;
            _wheelInput.OnRightShiftCallback -= OnGearUp;
            _wheelInput.OnLeftShiftCallback -= OnGearDown;
            _wheelInput.OnEastButtonCallback -= OnNitro;

            UnsubscribeFromManualGearEvents();
        }

        private void SubscribeToManualGearEvents()
        {
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

            // Для отладки - выводим значения периодически
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"Wheel Input - Gas: {_gasInput:F2}, Brake: {_brakeInput:F2}, Steering: {_steeringInput:F2}, " +
                         $"GearUp: {_gearUpInput}, GearDown: {_gearDownInput}, Nitro: {_nitroInput}");
            }

            ProcessGearChanges();
        }

        private void ProcessGearChanges()
        {
            if (_useSequentialShifting)
            {
                ProcessSequentialShifting();
            }
            else
            {
                ProcessManualShifting();
            }
        }

        private void ProcessSequentialShifting()
        {
            // Логика обрабатывается в GetGearUpInput/GetGearDownInput через сброс флагов
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
                if (!_clutchInput)
                {
                    Debug.Log($"Manual gear change: {_currentGear} -> {selectedGear}");
                    _currentGear = selectedGear;
                }
            }

            if (selectedGear == -1 && _currentGear != 0)
            {
                _currentGear = 0;
            }
        }

        #region Wheel Input Handlers
        private void OnThrottle(float value)
        {
            _gasInput = value;
        }

        private void OnBrake(float value)
        {
            _brakeInput = value;
        }

        private void OnSteering(float value)
        {
            _steeringInput = value;
        }

        private void OnHandbrake(float value)
        {
            _handbrakeInput = value > 0.5f;
        }

        private void OnGearUp(bool pressed)
        {
            if (_useSequentialShifting)
            {
                _gearUpInput = pressed;
                if (pressed) Debug.Log("GearUp pressed");
            }
        }

        private void OnGearDown(bool pressed)
        {
            if (_useSequentialShifting)
            {
                _gearDownInput = pressed;
                if (pressed) Debug.Log("GearDown pressed");
            }
        }

        private void OnNitro(bool pressed)
        {
            _nitroInput = pressed;
            if (pressed) Debug.Log("Nitro pressed");
        }
        #endregion

        #region Manual Gear Handlers
        private void OnShifter1(bool pressed) { _gearInputs[1] = pressed; if (pressed) Debug.Log("Shifter 1"); }
        private void OnShifter2(bool pressed) { _gearInputs[2] = pressed; if (pressed) Debug.Log("Shifter 2"); }
        private void OnShifter3(bool pressed) { _gearInputs[3] = pressed; if (pressed) Debug.Log("Shifter 3"); }
        private void OnShifter4(bool pressed) { _gearInputs[4] = pressed; if (pressed) Debug.Log("Shifter 4"); }
        private void OnShifter5(bool pressed) { _gearInputs[5] = pressed; if (pressed) Debug.Log("Shifter 5"); }
        private void OnShifter6(bool pressed) { _gearInputs[6] = pressed; if (pressed) Debug.Log("Shifter 6"); }
        private void OnShifter7(bool pressed) { _gearInputs[7] = pressed; if (pressed) Debug.Log("Shifter 7"); }
        #endregion

        private void ResetInputs()
        {
            _gasInput = 0f;
            _brakeInput = 0f;
            _steeringInput = 0f;
            _handbrakeInput = false;
            _gearUpInput = false;
            _gearDownInput = false;
            _nitroInput = false;

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
            if (_useSequentialShifting)
            {
                bool result = _gearUpInput;
                _gearUpInput = false; // Сбрасываем после чтения
                return result;
            }
            return false;
        }

        public bool GetGearDownInput()
        {
            if (_useSequentialShifting)
            {
                bool result = _gearDownInput;
                _gearDownInput = false; // Сбрасываем после чтения
                return result;
            }
            return false;
        }

        public int GetCurrentGear() => _currentGear;
        public bool IsManualTransmission() => !_useSequentialShifting;

        public float GetPitchInput() => 0f;
        public float GetYawInput() => 0f;
        public float GetRollInput() => 0f;
        #endregion
    }
}