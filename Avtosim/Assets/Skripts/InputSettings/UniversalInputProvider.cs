using LogitechG29.Sample.Input;
using TMPro;
using UnityEngine;

namespace Assets.VehicleController
{
    // Единственный источник ввода: InputControllerReader (_wheelInput) уже объединяет
    // руль, клавиатуру и (в перспективе) MOZA на уровне InputController.inputactions —
    // сюда прилетают события от любого подключённого устройства, и это единственное
    // место, которое их читает. Второй, дублирующий источник (PlayerVehicleInputActions +
    // отдельная ветка чтения клавиатуры) был удалён: он не пробрасывал сцепление и не
    // участвовал в переключении передач, а также не позволял читать клавиатуру и руль
    // одновременно.
    [AddComponentMenu("CustomVehicleController/Input/All-in-One Input Provider")]
    public class AllInOneInputProvider : MonoBehaviour, IVehicleControllerInputProvider, IManualTransmissionInputProvider
    {
        public enum InputMode { Keyboard, Wheel, Auto } // порядок сохранён — сериализован в существующих префабах
        public enum TransmissionMode { Automatic, Sequential, Manual }

        [Header("Settings")]
        [SerializeField] private InputMode _inputMode = InputMode.Auto;
        [SerializeField] private TextMeshProUGUI _textInputSys;
        [SerializeField] private TransmissionMode _transmissionMode = TransmissionMode.Automatic;
        [SerializeField] private InputControllerReader _wheelInput;

        [Header("Optional Controls")]
        [SerializeField] private bool enableHandbrakeInput = true;
        [SerializeField, Range(0f, 1f)] private float handbrakeDeadzone = 0.3f;

        private bool _enabled = true;

        private float _clutchInput;
        private float _gas, _brake, _steer;
        private bool _handbrake, _gearUp, _gearDown, _nitro;
        private readonly bool[] _manualGears = new bool[8];
        private int _currentGear;

        private bool _northPressedRaw;
        private bool _southPressedRaw;
        private bool _eastPressedRaw;
        private bool _westPressedRaw;

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

        private void OnEnable()
        {
            if (_wheelInput != null)
            {
                // ScriptableObject-ридер не может сам надёжно включить карты
                // ввода (его OnEnable привязан к загрузке ассета, а не к Play
                // Mode) — включаем явно отсюда, из жизненного цикла сцены.
                _wheelInput.EnsureInitialized();
                SubscribeWheel();
            }
        }

        private void OnDisable()
        {
            if (_wheelInput != null) UnsubscribeWheel();
            ResetInputs();
        }

        private void Update()
        {
            if (!_enabled)
            {
                ResetInputs();
                return;
            }

            // Самовосстановление: если карты ввода выключились (domain reload,
            // повторная енумерация HID и т.п.) — включаем заново. Дешёвая
            // проверка bool, лечит и историческое "руль отваливался".
            if (_wheelInput != null && !_wheelInput.IsInitialized)
                _wheelInput.EnsureInitialized();

            if (_textInputSys != null)
                _textInputSys.text = _inputMode.ToString();

            // Ручная коробка читается отсюда через GetCurrentGear(). Sequential и
            // Automatic обрабатываются самим CustomVehicleController через
            // GetGearUpInput()/GetGearDownInput() — дублировать эту логику здесь не нужно.
            if (_transmissionMode == TransmissionMode.Manual)
                ProcessManualShifting();
        }

        // --- Подписки ---
        private void SubscribeWheel()
        {
            _wheelInput.ClutchCallback += Wheel_OnClutch;
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
            _wheelInput.ClutchCallback -= Wheel_OnClutch;
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

        // --- Callbacks (руль + клавиатура, объединены в InputController.inputactions) ---
        private void Wheel_OnClutch(float v) => _clutchInput = v;
        private void Wheel_OnThrottle(float v) => _gas = v;

        private void Wheel_OnBrake(float v)
        {
            _brake = v;
            OnBrakeChanged?.Invoke(v);
        }

        private void Wheel_OnSteering(float v) => _steer = v;

        private void Wheel_OnHandbrake(float v)
        {
            _handbrake = enableHandbrakeInput && v > handbrakeDeadzone;
        }

        // _gearUp/_gearDown читаются и сбрасываются в GetGearUpInput()/GetGearDownInput(),
        // которые CustomVehicleController вызывает раз за кадр в Sequential-режиме.
        private void Wheel_OnRightShift(bool pressed)
        {
            if (pressed && _transmissionMode == TransmissionMode.Sequential) _gearUp = true;
        }

        private void Wheel_OnLeftShift(bool pressed)
        {
            if (pressed && _transmissionMode == TransmissionMode.Sequential) _gearDown = true;
        }

        private void Wheel_OnEast(bool pressed)
        {
            _eastPressedRaw = pressed;
            if (pressed) OnEastPressed?.Invoke();
        }

        private void Wheel_OnWest(bool pressed)
        {
            _westPressedRaw = pressed;
            if (pressed) OnWestPressed?.Invoke();
        }

        private void Wheel_OnNorth(bool pressed)
        {
            _northPressedRaw = pressed;
            if (pressed) OnNorthPressed?.Invoke();
        }

        private void Wheel_OnSouth(bool pressed)
        {
            _southPressedRaw = pressed;
            if (pressed) OnSouthPressed?.Invoke();
        }

        private void Wheel_OnShifter1(bool pressed) => _manualGears[1] = pressed;
        private void Wheel_OnShifter2(bool pressed) => _manualGears[2] = pressed;
        private void Wheel_OnShifter3(bool pressed) => _manualGears[3] = pressed;
        private void Wheel_OnShifter4(bool pressed) => _manualGears[4] = pressed;
        private void Wheel_OnShifter5(bool pressed) => _manualGears[5] = pressed;
        private void Wheel_OnShifter6(bool pressed) => _manualGears[6] = pressed;
        private void Wheel_OnShifter7(bool pressed) => _manualGears[7] = pressed;

        private void ProcessManualShifting()
        {
            if (_clutchInput < 0.1f)
            {
                _currentGear = 0;
                return;
            }

            int selectedGear = 0;
            for (int i = 1; i < _manualGears.Length; i++)
            {
                if (_manualGears[i]) { selectedGear = i; break; }
            }

            _currentGear = selectedGear;
        }

        private void ResetInputs()
        {
            _gas = _brake = _steer = _clutchInput = 0f;
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
