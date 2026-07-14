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

        // Полный диапазон поворота руля "стопор-в-стопор" в градусах для
        // устройств, для которых мы не знаем реальный физический угол (Logitech
        // через generic-ось, клавиатура). Подставь значение, настроенное в
        // драйвере руля (Logitech G HUB → Rotation), чтобы 3D-модель руля в
        // кабине поворачивалась 1-в-1 с реальным. Для MOZA диапазон приходит
        // напрямую с руля через SetMozaInputs и это значение игнорируется.
        [SerializeField] private float _defaultWheelRotationRangeDegrees = 900f;

        private bool _enabled = true;
        private bool _mozaConnected = false;
        private float _mozaWheelRangeDegrees = 0f;

        private float _clutchInput;
        private float _gas, _brake, _steer;
        private bool _handbrake, _gearUp, _gearDown, _nitro;
        private int _currentGear;

        // Порог сцепления, при котором разрешена смена передачи (0..1).
        // 0.5 — половина хода педали, так попросил пользователь (в т.ч. для
        // педали руля, где точное значение 0/1 менее вероятно, чем у клавиши).
        private const float CLUTCH_ENGAGED_THRESHOLD = 0.5f;

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

        public void SetMozaInputs(float gas, float brake, float clutch, float steer, bool handbrake, int gear, float wheelRangeDegrees = 0f)
        {
            _mozaConnected = true;
            _gas = gas;
            _brake = brake;
            _clutchInput = clutch;
            _steer = steer;
            _handbrake = handbrake;
            _mozaWheelRangeDegrees = wheelRangeDegrees;

            // Тот же гейт по сцеплению, что и для клавиатуры/Logitech (см.
            // TrySelectGear): передача меняется только при выжатом сцеплении.
            // MOZA SDK репортит АБСОЛЮТНОЕ текущее положение рычага (не
            // "нажата ли кнопка N"), поэтому здесь достаточно сравнить со
            // сцеплением напрямую — латчить нечего, само железо уже держит
            // рычаг в положении физически.
            if (clutch >= CLUTCH_ENGAGED_THRESHOLD)
                _currentGear = gear;
        }

        public void SetMozaDisconnected()
        {
            _mozaConnected = false;
        }

        // Реальный физический диапазон поворота руля "стопор-в-стопор" в
        // градусах — для 3D-модели руля в кабине (CockpitSteeringWheel), чтобы
        // её поворот совпадал с реальным рулём 1-в-1 по интенсивности. Для MOZA
        // берём то, что реально настроено на руле (SetMozaInputs передаёт его
        // с каждым кадром из getMotorLimitAngle), иначе — ручную настройку.
        public float GetWheelRotationRangeDegrees() =>
            (_mozaConnected && _mozaWheelRangeDegrees > 0f) ? _mozaWheelRangeDegrees : _defaultWheelRotationRangeDegrees;

        // Кнопки MOZA приходят через поллинг в MozaSdkManager.Update() (SDK не
        // даёт событий нажатия сам), поэтому фронт нажатия (переход false→true)
        // определяется здесь же, сравнением с предыдущим состоянием — иначе
        // OnNorthPressed и т.д. стреляли бы каждый кадр, пока кнопка зажата,
        // в отличие от Input Actions, которые шлют событие один раз на нажатие.
        public void SetMozaButtons(bool north, bool south, bool east, bool west)
        {
            if (north && !_northPressedRaw) OnNorthPressed?.Invoke();
            if (south && !_southPressedRaw) OnSouthPressed?.Invoke();
            if (east && !_eastPressedRaw) OnEastPressed?.Invoke();
            if (west && !_westPressedRaw) OnWestPressed?.Invoke();

            _northPressedRaw = north;
            _southPressedRaw = south;
            _eastPressedRaw = east;
            _westPressedRaw = west;
        }

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

            if (_mozaConnected)
            {
                if (_textInputSys != null)
                    _textInputSys.text = "MOZA (SDK)";
                return;
            }

            // Самовосстановление: если карты ввода выключились (domain reload,
            // повторная енумерация HID и т.п.) — включаем заново. Дешёвая
            // проверка bool, лечит и историческое "руль отваливался".
            if (_wheelInput != null && !_wheelInput.IsInitialized)
                _wheelInput.EnsureInitialized();

            if (_textInputSys != null)
                _textInputSys.text = _inputMode.ToString();

            // Выбор передачи для механики теперь edge-triggered (см.
            // TrySelectGear, вызывается из Wheel_OnShifterN/Wheel_OnNeutral),
            // а не polling каждый кадр — поэтому здесь ничего делать не нужно.
            // Sequential и Automatic обрабатываются самим CustomVehicleController
            // через GetGearUpInput()/GetGearDownInput().
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
            _wheelInput.NeutralCallback += Wheel_OnNeutral;
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
            _wheelInput.NeutralCallback -= Wheel_OnNeutral;
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

        // Выбор передачи — edge-triggered (срабатывает в момент нажатия, не
        // требует удержания клавиши/кнопки) и защищён сцеплением. Один раз
        // включённая передача остаётся включённой (латч) до следующего
        // валидного переключения — у рычага реальной коробки нет пружины,
        // которая возвращала бы его в нейтраль при отпускании.
        //
        // Раньше (polling каждый кадр по тому, что СЕЙЧАС зажато) с клавиатуры
        // это было физически неиграбельно: отпускаешь "1", чтобы дотянуться до
        // "W" — и передача тут же откатывалась в нейтраль. Отсюда и баги
        // "передача включается, а газа нет" и "машина не едет" — торк
        // обнулялся в Engine.Accelerate() ещё до того, как игрок успевал
        // нажать газ.
        private void Wheel_OnShifter1(bool pressed) { if (pressed) TrySelectGear(1); }
        private void Wheel_OnShifter2(bool pressed) { if (pressed) TrySelectGear(2); }
        private void Wheel_OnShifter3(bool pressed) { if (pressed) TrySelectGear(3); }
        private void Wheel_OnShifter4(bool pressed) { if (pressed) TrySelectGear(4); }
        private void Wheel_OnShifter5(bool pressed) { if (pressed) TrySelectGear(5); }
        private void Wheel_OnShifter6(bool pressed) { if (pressed) TrySelectGear(6); }
        private void Wheel_OnShifter7(bool pressed) { if (pressed) TrySelectGear(-1); } // 7-е положение шифтера = задний ход
        private void Wheel_OnNeutral(bool pressed) { if (pressed) TrySelectGear(0); }

        private void TrySelectGear(int gearId)
        {
            if (_transmissionMode != TransmissionMode.Manual)
                return;

            // Шестерни физически не совместятся, пока диск сцепления не
            // разомкнут — без этого порога передача просто не меняется
            // (остаётся как была), что бы ни нажималось.
            if (_clutchInput >= CLUTCH_ENGAGED_THRESHOLD)
                _currentGear = gearId;
        }

        private void ResetInputs()
        {
            _gas = _brake = _steer = _clutchInput = 0f;
            _handbrake = _gearUp = _gearDown = _nitro = false;
            _northPressedRaw = _southPressedRaw = _eastPressedRaw = _westPressedRaw = false;
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

        public void SetTransmissionMode(TransmissionMode mode)
        {
            // При входе в механику рычаг "новый" — начинаем с нейтрали, а не
            // с передачи, оставшейся с предыдущего раза в этом режиме.
            if (mode == TransmissionMode.Manual && _transmissionMode != TransmissionMode.Manual)
                _currentGear = 0;

            _transmissionMode = mode;
        }
        public TransmissionMode GetTransmissionMode() => _transmissionMode;
        public void SetInputMode(InputMode mode) => _inputMode = mode;
        public InputMode GetCurrentInputMode() => _inputMode;
        #endregion
    }
}
