using System;
using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Номера кнопок обода MOZA — те же, что подписаны на официальной схеме
    /// руля и показаны в MOZA Pit House. Значение enum ЕСТЬ номер кнопки,
    /// поэтому <c>(int)MozaButton.Start == 36</c>.
    ///
    /// Нумерация начинается с 1, как в SDK: официальный пример перебирает
    /// <c>for (int i = 1; i &lt; 113; i++) d-&gt;buttons[i]</c>, а имена полей
    /// HIDData прямо кодируют номера (leftRocker5_8, knobL45_46 и т.д.).
    ///
    /// Здесь перечислено то, что физически подписано на схеме этого обода.
    /// Кнопки, которых в списке нет, всё равно читаются — по номеру:
    /// <c>MozaWheelInput.GetButton(47)</c>.
    /// </summary>
    public enum MozaButton
    {
        None = 0,

        // Правый ромб (раскладка Xbox-style).
        A = 1,  // низ    -> South
        B = 2,  // право  -> East
        X = 3,  // лево   -> West
        Y = 4,  // верх   -> North

        // Левая крестовина. В SDK эта же четвёрка приходит ещё и как
        // направление — см. MozaWheelInput.LeftRocker.
        DpadUp = 5,
        DpadRight = 6,
        DpadDown = 7,
        DpadLeft = 8,

        // Правая крестовина (есть не на всех ободах; на ES её нет).
        RightDpadUp = 9,
        RightDpadRight = 10,
        RightDpadDown = 11,
        RightDpadLeft = 12,

        // Подрулевые лепестки переключения передач.
        // 13 = левый (понижение), 14 = правый (повышение).
        LeftPaddle = 13,
        RightPaddle = 14,

        // Левый верхний блок.
        N = 19,
        Wip = 20,
        FL = 21,

        // Левый нижний блок.
        Cam = 22,
        Radio = 23,
        S1 = 24,
        Home = 25,

        // Правый верхний блок.
        P = 32,
        Box = 33,
        PL = 34,

        // Правый нижний блок.
        R = 35,
        Start = 36,
        S2 = 37,
        Menu = 38,
    }

    /// <summary>Направление крестовины/джойстика.</summary>
    public enum MozaDirection
    {
        None = 0,
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft,
    }

    /// <summary>
    /// Полное состояние кнопок и энкодеров руля MOZA.
    ///
    /// Данные заполняет <see cref="MozaSdkManager"/> раз в кадр, до всех
    /// остальных скриптов (у него DefaultExecutionOrder = -1000), поэтому
    /// читать отсюда можно из обычного Update.
    ///
    /// Пример:
    /// <code>
    /// if (MozaWheelInput.GetButtonDown(MozaButton.Start)) StartRace();
    /// if (MozaWheelInput.GetButton(MozaButton.RightPaddle)) ShiftUp();
    /// int clicks = MozaWheelInput.RightKnobDelta;   // энкодер: +вправо/-влево
    /// </code>
    /// </summary>
    public static class MozaWheelInput
    {
        /// <summary>Размер таблицы кнопок. HIDData.buttons в SDK — 128 штук.</summary>
        public const int MaxButtons = 128;

        private static readonly bool[] _level = new bool[MaxButtons];
        private static readonly bool[] _down = new bool[MaxButtons];
        private static readonly bool[] _up = new bool[MaxButtons];
        private static readonly int[] _pressCount = new int[MaxButtons];

        /// <summary>Подключён ли руль и валидны ли данные в этом кадре.</summary>
        public static bool IsConnected { get; private set; }

        /// <summary>
        /// Левая крестовина (кнопки 5-8) как направление. Кнопки 5-8 при этом
        /// тоже работают: менеджер сводит крестовину в них сам, в каком бы
        /// режиме (кнопки/hat) она ни была настроена в Pit House.
        /// </summary>
        public static MozaDirection LeftRocker { get; private set; }

        /// <summary>Правая крестовина (кнопки 9-12) как направление.</summary>
        public static MozaDirection RightRocker { get; private set; }

        /// <summary>Левый энкодер (45-46): щелчков за кадр, знак = сторона.</summary>
        public static int LeftKnobDelta { get; private set; }

        /// <summary>Правый энкодер (47-48): щелчков за кадр, знак = сторона.</summary>
        public static int RightKnobDelta { get; private set; }

        /// <summary>Левый энкодер: накопленная позиция с момента запуска.</summary>
        public static int LeftKnobPosition { get; private set; }

        /// <summary>Правый энкодер: накопленная позиция с момента запуска.</summary>
        public static int RightKnobPosition { get; private set; }

        /// <summary>
        /// Многопозиционные селекторы: последняя выбранная позиция.
        /// Индекс 0..4 в порядке полей HIDData
        /// (26_27or53_64, 28_29or65_76, 30_31or77_88, 39_40or89_100, 43_44or101_112).
        /// </summary>
        public static readonly int[] MultiSegmentKnob = new int[5];

        // ---------- оси: руль и педали ----------

        /// <summary>Поворот руля, -1..1. Отрицательное — влево.</summary>
        public static float Steering { get; private set; }

        /// <summary>Педаль газа, 0..1.</summary>
        public static float Throttle { get; private set; }

        /// <summary>Педаль тормоза, 0..1.</summary>
        public static float Brake { get; private set; }

        /// <summary>Педаль сцепления, 0..1.</summary>
        public static float Clutch { get; private set; }

        /// <summary>
        /// Аналоговые лепестки сцепления на ободе (есть у FSR, KS, CS и др.;
        /// у ES их нет — там всегда 0). Совмещённая ось, 0..1.
        /// Это НЕ подрулевые лепестки передач: те — кнопки 13 и 14.
        /// </summary>
        public static float ClutchPaddle { get; private set; }

        /// <summary>Левый лепесток сцепления в раздельном режиме, 0..1.</summary>
        public static float ClutchPaddleLeft { get; private set; }

        /// <summary>Правый лепесток сцепления в раздельном режиме, 0..1.</summary>
        public static float ClutchPaddleRight { get; private set; }

        /// <summary>Скорость вращения руля, град/с (прошивка базы 1.2.4.x и новее, иначе 0).</summary>
        public static float SteeringVelocity { get; private set; }

        /// <summary>Ускорение вращения руля, град/с² (прошивка базы 1.2.4.x и новее, иначе 0).</summary>
        public static float SteeringAcceleration { get; private set; }

        /// <summary>
        /// Фактический угол поворота руля в градусах от центра.
        /// Приходит с базы напрямую, знак совпадает со <see cref="Steering"/>.
        /// </summary>
        public static float SteeringAngleDegrees { get; private set; }

        /// <summary>
        /// Полный диапазон руля «стопор-в-стопор» в градусах — то, что
        /// реально настроено в MOZA Pit House. Нужен, чтобы 3D-модель руля
        /// в кабине крутилась один в один с настоящим.
        /// </summary>
        public static float SteeringRangeDegrees { get; private set; } = 900f;

        /// <summary>Текущая передача рычага КПП: -1 = R, 0 = нейтраль, 1..7.</summary>
        public static int Gear { get; private set; }

        /// <summary>Кнопка ручного тормоза на ручнике MOZA.</summary>
        public static bool Handbrake { get; private set; }

        /// <summary>Нажатие любой кнопки в этом кадре (для экрана «нажмите кнопку»).</summary>
        public static event Action<int> OnButtonDown;

        /// <summary>Отпускание любой кнопки в этом кадре.</summary>
        public static event Action<int> OnButtonUp;

        // ---------- чтение состояния ----------

        /// <summary>Кнопка удерживается прямо сейчас.</summary>
        public static bool GetButton(int number) =>
            IsValid(number) && _level[number];

        /// <summary>Кнопку нажали в этом кадре (фронт).</summary>
        public static bool GetButtonDown(int number) =>
            IsValid(number) && _down[number];

        /// <summary>Кнопку отпустили в этом кадре (спад).</summary>
        public static bool GetButtonUp(int number) =>
            IsValid(number) && _up[number];

        /// <summary>
        /// Сколько раз кнопку нажали за прошедший цикл опроса. Обычно 0 или 1,
        /// но ловит и совсем короткие нажатия, уложившиеся внутрь одного кадра.
        /// </summary>
        public static int GetPressCount(int number) =>
            IsValid(number) ? _pressCount[number] : 0;

        public static bool GetButton(MozaButton button) => GetButton((int)button);
        public static bool GetButtonDown(MozaButton button) => GetButtonDown((int)button);
        public static bool GetButtonUp(MozaButton button) => GetButtonUp((int)button);
        public static int GetPressCount(MozaButton button) => GetPressCount((int)button);

        /// <summary>
        /// Номер любой кнопки, нажатой в этом кадре, иначе 0.
        /// Удобно для экрана переназначения управления.
        /// </summary>
        public static int GetAnyButtonDown()
        {
            for (int i = 1; i < MaxButtons; i++)
            {
                if (_down[i])
                    return i;
            }

            return 0;
        }

        private static bool IsValid(int number) =>
            number > 0 && number < MaxButtons;

        // ---------- заполнение (вызывает MozaSdkManager) ----------

        internal static void BeginFrame()
        {
            Array.Clear(_down, 0, _down.Length);
            Array.Clear(_up, 0, _up.Length);
            Array.Clear(_pressCount, 0, _pressCount.Length);

            LeftKnobDelta = 0;
            RightKnobDelta = 0;
        }

        /// <summary>
        /// Обновляет одну кнопку. <paramref name="level"/> — состояние на конец
        /// цикла опроса, <paramref name="presses"/>/<paramref name="releases"/> —
        /// сколько нажатий и отпусканий случилось внутри цикла.
        /// </summary>
        internal static void SetButton(int number, bool level, int presses, int releases)
        {
            if (!IsValid(number))
                return;

            bool previous = _level[number];

            // presses/releases ловят нажатие, целиком уместившееся между двумя
            // опросами: уровень при этом мог и не измениться.
            bool down = presses > 0 || (level && !previous);
            bool up = releases > 0 || (previous && !level);

            _level[number] = level;
            _down[number] = down;
            _up[number] = up;
            _pressCount[number] = Mathf.Max(presses, down ? 1 : 0);

            if (down)
                OnButtonDown?.Invoke(number);

            if (up)
                OnButtonUp?.Invoke(number);
        }

        internal static void SetRockers(MozaDirection left, MozaDirection right)
        {
            LeftRocker = left;
            RightRocker = right;
        }

        internal static void SetKnobs(int leftDelta, int rightDelta)
        {
            LeftKnobDelta = leftDelta;
            RightKnobDelta = rightDelta;
            LeftKnobPosition += leftDelta;
            RightKnobPosition += rightDelta;
        }

        internal static void SetMultiSegmentKnob(int index, int value)
        {
            if (index >= 0 && index < MultiSegmentKnob.Length)
                MultiSegmentKnob[index] = value;
        }

        internal static void SetVehicleState(int gear, bool handbrake)
        {
            Gear = gear;
            Handbrake = handbrake;
        }

        internal static void SetClutchPaddles(float combined, float left, float right)
        {
            ClutchPaddle = combined;
            ClutchPaddleLeft = left;
            ClutchPaddleRight = right;
        }

        internal static void SetSteeringDynamics(float velocity, float acceleration)
        {
            SteeringVelocity = velocity;
            SteeringAcceleration = acceleration;
        }

        internal static void SetAxes(float steering, float throttle, float brake, float clutch,
            float steeringAngleDegrees, float steeringRangeDegrees)
        {
            Steering = steering;
            Throttle = throttle;
            Brake = brake;
            Clutch = clutch;
            SteeringAngleDegrees = steeringAngleDegrees;

            if (steeringRangeDegrees > 0f)
                SteeringRangeDegrees = steeringRangeDegrees;
        }

        internal static void SetConnected(bool connected)
        {
            if (IsConnected == connected)
                return;

            IsConnected = connected;

            if (!connected)
                Reset();
        }

        /// <summary>Гасит все состояния — при отключении руля.</summary>
        internal static void Reset()
        {
            for (int i = 0; i < MaxButtons; i++)
            {
                // Отпускание отправляем честно, иначе подписчики останутся
                // думать, что кнопка всё ещё зажата.
                if (_level[i])
                {
                    _level[i] = false;
                    _up[i] = true;
                    OnButtonUp?.Invoke(i);
                }

                _down[i] = false;
                _pressCount[i] = 0;
            }

            LeftRocker = MozaDirection.None;
            RightRocker = MozaDirection.None;
            LeftKnobDelta = 0;
            RightKnobDelta = 0;
            Array.Clear(MultiSegmentKnob, 0, MultiSegmentKnob.Length);
            Gear = 0;
            Handbrake = false;
            Steering = 0f;
            Throttle = 0f;
            Brake = 0f;
            Clutch = 0f;
            ClutchPaddle = 0f;
            ClutchPaddleLeft = 0f;
            ClutchPaddleRight = 0f;
            SteeringVelocity = 0f;
            SteeringAcceleration = 0f;
            SteeringAngleDegrees = 0f;
        }
    }
}
