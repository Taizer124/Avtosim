using UnityEngine;
using LogitechG29.Sample.Input;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Input/All-in-One Input Provider")]
    public class AllInOneInputProvider : MonoBehaviour, IVehicleControllerInputProvider
    {
        public enum InputMode { InputSystem, Wheel, Auto }

        [Header("Input Mode")]
        [SerializeField] private InputMode _currentMode = InputMode.Auto;

        [Header("Wheel Input")]
        [SerializeField] private InputControllerReader _wheelInput;

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
            }

            _inputActions?.Disable();
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

            _gearUpInput = _inputActions.Vehicle.GearUpInput.WasPerformedThisFrame();
            _gearDownInput = _inputActions.Vehicle.GearDownInput.WasPerformedThisFrame();
        }

        private void UpdateWheelInput()
        {
            // Для Wheel mode данные обновляются через события (колбэки)
            // Здесь можно добавить дополнительную логику если нужно
        }

        #region Wheel Input Handlers
        private void OnThrottle(float value) => _gasInput = value;
        private void OnBrake(float value) => _brakeInput = value;
        private void OnSteering(float value) => _steeringInput = value;
        private void OnHandbrake(float value) => _handbrakeInput = value > 0.5f;
        private void OnGearUp(bool pressed) => _gearUpInput = pressed;
        private void OnGearDown(bool pressed) => _gearDownInput = pressed;
        private void OnNitro(bool pressed) => _nitroInput = pressed;
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

        public void SetInputMode(InputMode mode) => _currentMode = mode;
        public InputMode GetCurrentInputMode() => _currentMode;
    }
}