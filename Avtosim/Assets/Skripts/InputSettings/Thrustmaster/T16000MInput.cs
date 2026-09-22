using System;
using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Кнопки джойстика Thrustmaster T.16000M FCS. Значение enum ЕСТЬ номер
    /// кнопки — тот же, что показывает Windows в «Игровых устройствах»
    /// (joy.cpl) и что подписан в руководстве пользователя.
    ///
    /// Всего 16 кнопок: 4 на рукоятке и по 6 на двух площадках основания.
    /// На площадках номера идут не построчно, а по кругу — точное положение
    /// каждой кнопки проверяйте тестером <see cref="T16000MInputTester"/>.
    /// </summary>
    public enum T16000MButton
    {
        None = 0,

        // Рукоятка.
        Trigger = 1,        // курок под указательным пальцем
        Thumb = 2,          // кнопка под большим пальцем, ниже хатки
        HeadLeft = 3,       // левая кнопка на верхушке рукоятки
        HeadRight = 4,      // правая кнопка на верхушке рукоятки

        // Левая площадка основания.
        LeftBase5 = 5,
        LeftBase6 = 6,
        LeftBase7 = 7,
        LeftBase8 = 8,
        LeftBase9 = 9,
        LeftBase10 = 10,

        // Правая площадка основания.
        RightBase11 = 11,
        RightBase12 = 12,
        RightBase13 = 13,
        RightBase14 = 14,
        RightBase15 = 15,
        RightBase16 = 16,
    }

    /// <summary>Положение 8-позиционной хатки (POV).</summary>
    public enum HatDirection
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
    /// Состояние джойстика Thrustmaster T.16000M FCS: 16 кнопок, хатка и
    /// четыре оси (наклон по X и Y, поворот рукоятки, ползунок тяги).
    ///
    /// Данные заполняет <see cref="T16000MManager"/> раз в кадр, раньше всех
    /// остальных скриптов (DefaultExecutionOrder = -1000), поэтому читать
    /// отсюда можно из обычного Update.
    ///
    /// Пример:
    /// <code>
    /// float steer = T16000MInput.StickX;                       // -1..1
    /// if (T16000MInput.GetButtonDown(T16000MButton.Trigger)) Fire();
    /// if (T16000MInput.Hat == HatDirection.Up) LookUp();
    /// </code>
    /// </summary>
    public static class T16000MInput
    {
        /// <summary>USB Vendor ID Thrustmaster.</summary>
        public const int VendorId = 0x044F;

        /// <summary>USB Product ID джойстика T.16000M.</summary>
        public const int ProductId = 0xB10A;

        /// <summary>Кнопок на джойстике. Номера 1..16.</summary>
        public const int ButtonCount = 16;

        private static readonly bool[] _level = new bool[ButtonCount + 1];
        private static readonly bool[] _down = new bool[ButtonCount + 1];
        private static readonly bool[] _up = new bool[ButtonCount + 1];

        // ---------- настройки ----------

        /// <summary>Мёртвая зона наклона рукоятки (X и Y), 0..1.</summary>
        public static float StickDeadzone = 0.05f;

        /// <summary>
        /// Мёртвая зона поворота рукоятки, 0..1. У поворота нет возвратной
        /// пружины такой же жёсткости, как у наклона, поэтому зона больше.
        /// </summary>
        public static float TwistDeadzone = 0.10f;

        /// <summary>Инвертировать ось Y (по умолчанию «от себя» = +1).</summary>
        public static bool InvertStickY = false;

        /// <summary>Инвертировать ползунок тяги (по умолчанию «от себя» = 1).</summary>
        public static bool InvertThrottle = false;

        /// <summary>
        /// Принимать любой HID-джойстик, если T.16000M не найден. Удобно для
        /// отладки на другом джойстике; номера кнопок у него будут свои.
        /// </summary>
        public static bool AcceptAnyJoystick = false;

        // ---------- состояние ----------

        /// <summary>Джойстик подключён и данные в этом кадре валидны.</summary>
        public static bool IsConnected { get; private set; }

        /// <summary>Имя устройства, как его видит Windows, или пустая строка.</summary>
        public static string DeviceName { get; private set; } = string.Empty;

        /// <summary>Наклон рукоятки влево/вправо, -1..1. Вправо — плюс.</summary>
        public static float StickX { get; private set; }

        /// <summary>Наклон рукоятки от себя/на себя, -1..1. От себя — плюс.</summary>
        public static float StickY { get; private set; }

        /// <summary>Поворот рукоятки вокруг оси (руль направления), -1..1. Вправо — плюс.</summary>
        public static float Twist { get; private set; }

        /// <summary>Ползунок тяги на основании, 0..1. От себя — 1.</summary>
        public static float Throttle { get; private set; }

        /// <summary>Положение хатки.</summary>
        public static HatDirection Hat { get; private set; }

        /// <summary>Хатка как вектор: x = -1/0/1 (влево/вправо), y = -1/0/1 (вниз/вверх).</summary>
        public static Vector2 HatVector { get; private set; }

        /// <summary>Джойстик подключили.</summary>
        public static event Action OnConnected;

        /// <summary>Джойстик отключили.</summary>
        public static event Action OnDisconnected;

        /// <summary>Нажатие любой кнопки в этом кадре; аргумент — номер кнопки.</summary>
        public static event Action<int> OnButtonDown;

        /// <summary>Отпускание любой кнопки в этом кадре; аргумент — номер кнопки.</summary>
        public static event Action<int> OnButtonUp;

        // ---------- чтение кнопок ----------

        /// <summary>Кнопка удерживается прямо сейчас.</summary>
        public static bool GetButton(int number) => IsValid(number) && _level[number];

        /// <summary>Кнопку нажали в этом кадре (фронт).</summary>
        public static bool GetButtonDown(int number) => IsValid(number) && _down[number];

        /// <summary>Кнопку отпустили в этом кадре (спад).</summary>
        public static bool GetButtonUp(int number) => IsValid(number) && _up[number];

        public static bool GetButton(T16000MButton button) => GetButton((int)button);
        public static bool GetButtonDown(T16000MButton button) => GetButtonDown((int)button);
        public static bool GetButtonUp(T16000MButton button) => GetButtonUp((int)button);

        /// <summary>Номер любой кнопки, нажатой в этом кадре, иначе 0.</summary>
        public static int GetAnyButtonDown()
        {
            for (int i = 1; i <= ButtonCount; i++)
            {
                if (_down[i])
                    return i;
            }

            return 0;
        }

        private static bool IsValid(int number) => number >= 1 && number <= ButtonCount;

        // ---------- заполнение (вызывает T16000MManager) ----------

        internal static void SetButton(int number, bool level, bool pressedThisFrame, bool releasedThisFrame)
        {
            if (!IsValid(number))
                return;

            bool previous = _level[number];
            bool down = pressedThisFrame || (level && !previous);
            bool up = releasedThisFrame || (previous && !level);

            _level[number] = level;
            _down[number] = down;
            _up[number] = up;

            if (down)
                OnButtonDown?.Invoke(number);

            if (up)
                OnButtonUp?.Invoke(number);
        }

        internal static void SetAxes(float stickX, float stickY, float twist, float throttle)
        {
            StickX = stickX;
            StickY = stickY;
            Twist = twist;
            Throttle = throttle;
        }

        internal static void SetHat(Vector2 vector)
        {
            int x = vector.x > 0.5f ? 1 : vector.x < -0.5f ? -1 : 0;
            int y = vector.y > 0.5f ? 1 : vector.y < -0.5f ? -1 : 0;

            HatVector = new Vector2(x, y);
            Hat = ToDirection(x, y);
        }

        internal static void SetConnected(bool connected, string deviceName)
        {
            if (IsConnected == connected)
                return;

            IsConnected = connected;
            DeviceName = connected ? deviceName : string.Empty;

            if (connected)
            {
                OnConnected?.Invoke();
            }
            else
            {
                Reset();
                OnDisconnected?.Invoke();
            }
        }

        /// <summary>Гасит все состояния — при отключении джойстика.</summary>
        private static void Reset()
        {
            for (int i = 1; i <= ButtonCount; i++)
            {
                // Отпускание отправляем честно, иначе подписчики останутся
                // думать, что кнопка всё ещё зажата.
                _up[i] = _level[i];
                _down[i] = false;

                if (_level[i])
                {
                    _level[i] = false;
                    OnButtonUp?.Invoke(i);
                }
            }

            StickX = StickY = Twist = Throttle = 0f;
            Hat = HatDirection.None;
            HatVector = Vector2.zero;
        }

        private static HatDirection ToDirection(int x, int y)
        {
            if (y > 0) return x > 0 ? HatDirection.UpRight : x < 0 ? HatDirection.UpLeft : HatDirection.Up;
            if (y < 0) return x > 0 ? HatDirection.DownRight : x < 0 ? HatDirection.DownLeft : HatDirection.Down;
            return x > 0 ? HatDirection.Right : x < 0 ? HatDirection.Left : HatDirection.None;
        }
    }
}
