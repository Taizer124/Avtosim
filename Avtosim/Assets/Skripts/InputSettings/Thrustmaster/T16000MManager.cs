using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Layouts;

namespace Assets.VehicleController
{
    /// <summary>
    /// Опрос джойстика Thrustmaster T.16000M FCS. Раскладывает его состояние
    /// в <see cref="T16000MInput"/>.
    ///
    /// Джойстик — обычное HID-устройство: драйвер Thrustmaster для работы не
    /// нужен, Unity Input System сама строит для него раскладку по
    /// HID-дескриптору. Имена контролов в этой раскладке такие:
    ///   trigger, button2 … button16 — кнопки (номер = номер кнопки);
    ///   stick (stick/x, stick/y)     — наклон рукоятки, X и Y по 14 бит;
    ///   rz                           — поворот рукоятки, 8 бит;
    ///   slider                       — ползунок тяги, 8 бит;
    ///   hat                          — 8-позиционная хатка.
    /// Менеджер читает их по именам, поэтому студенту не нужно знать, как
    /// устроен HID-отчёт.
    ///
    /// DefaultExecutionOrder = -1000: менеджер должен отработать раньше всех,
    /// кто читает T16000MInput, иначе GetButtonDown будет опаздывать на кадр.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class T16000MManager : MonoBehaviour
    {
        private static T16000MManager _instance;

        private InputDevice _device;
        private readonly ButtonControl[] _buttons = new ButtonControl[T16000MInput.ButtonCount + 1];
        private StickControl _stick;
        private AxisControl _twist;
        private AxisControl _slider;
        private Vector2Control _hat;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            var go = new GameObject("T16000M_Manager");
            go.AddComponent<T16000MManager>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            InputSystem.onDeviceChange += HandleDeviceChange;
            FindDevice();
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= HandleDeviceChange;
        }

        private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                case InputDeviceChange.Enabled:
                    if (_device == null)
                        FindDevice();
                    break;

                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                case InputDeviceChange.Disabled:
                    if (device == _device)
                        Bind(null);
                    break;
            }
        }

        private void Update()
        {
            if (_device == null || !_device.added || !_device.enabled)
            {
                T16000MInput.SetConnected(false, null);
                return;
            }

            T16000MInput.SetConnected(true, _device.displayName);

            for (int number = 1; number <= T16000MInput.ButtonCount; number++)
            {
                ButtonControl button = _buttons[number];

                if (button == null)
                    continue;

                T16000MInput.SetButton(number, button.isPressed,
                    button.wasPressedThisFrame, button.wasReleasedThisFrame);
            }

            Vector2 stick = _stick != null ? _stick.ReadValue() : Vector2.zero;
            float twist = _twist != null ? _twist.ReadValue() : 0f;

            // Ползунок Input System нормирует в -1..1 (середина хода = 0).
            // У T.16000M положение «от себя» даёт минимум, поэтому переворачиваем:
            // от себя = 1, на себя = 0.
            float slider = _slider != null ? _slider.ReadValue() : 1f;
            float throttle = Mathf.Clamp01((1f - slider) * 0.5f);

            if (T16000MInput.InvertThrottle)
                throttle = 1f - throttle;

            float y = ApplyDeadzone(stick.y, T16000MInput.StickDeadzone);

            T16000MInput.SetAxes(
                ApplyDeadzone(stick.x, T16000MInput.StickDeadzone),
                T16000MInput.InvertStickY ? -y : y,
                ApplyDeadzone(twist, T16000MInput.TwistDeadzone),
                throttle);

            T16000MInput.SetHat(_hat != null ? _hat.ReadValue() : Vector2.zero);
        }

        // ---------- поиск устройства ----------

        private void FindDevice()
        {
            InputDevice fallback = null;

            foreach (InputDevice device in InputSystem.devices)
            {
                if (IsT16000M(device))
                {
                    Bind(device);
                    return;
                }

                if (fallback == null && device is Joystick)
                    fallback = device;
            }

            Bind(T16000MInput.AcceptAnyJoystick ? fallback : null);
        }

        /// <summary>
        /// Узнаём джойстик по USB VID/PID из HID-дескриптора, а если его нет
        /// (например, устройство пришло не через HID) — по имени.
        /// </summary>
        private static bool IsT16000M(InputDevice device)
        {
            InputDeviceDescription description = device.description;

            if (description.interfaceName == "HID" && !string.IsNullOrEmpty(description.capabilities))
            {
                try
                {
                    HID.HIDDeviceDescriptor hid = HID.HIDDeviceDescriptor.FromJson(description.capabilities);

                    if (hid.vendorId == T16000MInput.VendorId && hid.productId == T16000MInput.ProductId)
                        return true;
                }
                catch (Exception)
                {
                    // битый JSON — проверим по имени
                }
            }

            string product = description.product ?? string.Empty;
            return product.IndexOf("T.16000M", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void Bind(InputDevice device)
        {
            _device = device;
            Array.Clear(_buttons, 0, _buttons.Length);
            _stick = null;
            _twist = null;
            _slider = null;
            _hat = null;

            if (device == null)
            {
                T16000MInput.SetConnected(false, null);
                return;
            }

            _buttons[1] = device.TryGetChildControl<ButtonControl>("trigger")
                          ?? device.TryGetChildControl<ButtonControl>("button1");

            for (int number = 2; number <= T16000MInput.ButtonCount; number++)
                _buttons[number] = device.TryGetChildControl<ButtonControl>("button" + number);

            _stick = device.TryGetChildControl<StickControl>("stick");
            _twist = device.TryGetChildControl<AxisControl>("rz")
                     ?? device.TryGetChildControl<AxisControl>("{Twist}");
            _slider = device.TryGetChildControl<AxisControl>("slider")
                      ?? device.TryGetChildControl<AxisControl>("z");
            _hat = device.TryGetChildControl<Vector2Control>("hat");

            Debug.Log($"[T.16000M] Подключён: {device.displayName} ({device.layout}). " +
                      $"Кнопок найдено: {CountButtons()}, stick={_stick != null}, " +
                      $"twist={_twist != null}, slider={_slider != null}, hat={_hat != null}");
        }

        private int CountButtons()
        {
            int count = 0;

            for (int i = 1; i <= T16000MInput.ButtonCount; i++)
            {
                if (_buttons[i] != null)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Мёртвая зона с перемасштабированием: за её пределами ось снова
        /// начинается с нуля, а не прыгает сразу на величину зоны.
        /// </summary>
        private static float ApplyDeadzone(float value, float deadzone)
        {
            float magnitude = Mathf.Abs(value);

            if (magnitude <= deadzone)
                return 0f;

            return Mathf.Sign(value) * Mathf.Clamp01((magnitude - deadzone) / (1f - deadzone));
        }

        /// <summary>Текущее устройство — для тестера (список контролов).</summary>
        internal static InputDevice CurrentDevice => _instance != null ? _instance._device : null;
    }
}
