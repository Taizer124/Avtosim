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

        private bool _enabled = true;

        private void OnEnable()
        {
            if (_wheelInput == null)
            {
                Debug.LogError("Wheel Input not assigned!");
                return;
            }

            // ПРАВИЛЬНЫЕ названия событий (с "On" в начале)
            _wheelInput.ThrottleCallback += OnThrottle;
            _wheelInput.BrakeCallback += OnBrake;
            _wheelInput.SteeringCallback += OnSteering;
            _wheelInput.HandbrakeCallback += OnHandbrake;
            _wheelInput.OnRightShiftCallback += OnGearUp;        // С "On"
            _wheelInput.OnLeftShiftCallback += OnGearDown;       // С "On"  
            _wheelInput.OnEastButtonCallback += OnNitro;         // С "On"
        }

        private void OnDisable()
        {
            if (_wheelInput == null) return;

            // Отписываемся от событий
            _wheelInput.ThrottleCallback -= OnThrottle;
            _wheelInput.BrakeCallback -= OnBrake;
            _wheelInput.SteeringCallback -= OnSteering;
            _wheelInput.HandbrakeCallback -= OnHandbrake;
            _wheelInput.OnRightShiftCallback -= OnGearUp;        // С "On"
            _wheelInput.OnLeftShiftCallback -= OnGearDown;       // С "On"
            _wheelInput.OnEastButtonCallback -= OnNitro;         // С "On"
        }

        private void Update()
        {
            if (!_enabled || _wheelInput == null)
            {
                ResetInputs();
                return;
            }

            // Для отладки - выводим значения каждую секунду
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"Wheel Input - Gas: {_gasInput}, Brake: {_brakeInput}, Steering: {_steeringInput}, " +
                         $"GearUp: {_gearUpInput}, GearDown: {_gearDownInput}, Nitro: {_nitroInput}");
            }
        }

        #region Wheel Input Handlers
        private void OnThrottle(float value)
        {
            _gasInput = value;
            Debug.Log($"Throttle: {value}");
        }

        private void OnBrake(float value)
        {
            _brakeInput = value;
            Debug.Log($"Brake: {value}");
        }

        private void OnSteering(float value)
        {
            _steeringInput = value;
            Debug.Log($"Steering: {value}");
        }

        private void OnHandbrake(float value)
        {
            _handbrakeInput = value > 0.5f;
            Debug.Log($"Handbrake: {value} -> {_handbrakeInput}");
        }

        private void OnGearUp(bool pressed)
        {
            if (_useSequentialShifting)
            {
                _gearUpInput = pressed;
                Debug.Log($"Gear Up: {pressed}");
            }
        }

        private void OnGearDown(bool pressed)
        {
            if (_useSequentialShifting)
            {
                _gearDownInput = pressed;
                Debug.Log($"Gear Down: {pressed}");
            }
        }

        private void OnNitro(bool pressed)
        {
            _nitroInput = pressed;
            Debug.Log($"Nitro: {pressed}");
        }
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
            bool result = _gearUpInput;
            _gearUpInput = false; // Сбрасываем после чтения
            return result;
        }

        public bool GetGearDownInput()
        {
            bool result = _gearDownInput;
            _gearDownInput = false; // Сбрасываем после чтения
            return result;
        }

        public float GetPitchInput() => 0f;
        public float GetYawInput() => 0f;
        public float GetRollInput() => 0f;
        #endregion
    }
}