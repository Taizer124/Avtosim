using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.VehicleController
{
    /// <summary>
    /// Диагностика джойстика Thrustmaster T.16000M FCS: живое состояние осей,
    /// хатки и всех 16 кнопок. Нужна, чтобы проверить раскладку на настоящем
    /// железе, а не полагаться на схему.
    ///
    /// Как пользоваться: повесить на любой объект в сцене, запустить, по
    /// очереди нажать каждую кнопку и провести каждую ось до упора в обе
    /// стороны. Внизу копится список кнопок, отработавших за сессию, — если
    /// какой-то нет, она не читается.
    ///
    /// В бой не тащить — это инструмент настройки.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/Input/T.16000M Input Tester")]
    public class T16000MInputTester : MonoBehaviour
    {
        [Tooltip("Оверлей поверх игры.")]
        [SerializeField] private bool _showOverlay = true;

        [Tooltip("Писать каждое нажатие в консоль.")]
        [SerializeField] private bool _logToConsole = true;

        [Tooltip("Показать имена всех контролов устройства, как их построила Input System.")]
        [SerializeField] private bool _showControlNames = false;

        private readonly SortedSet<int> _seen = new SortedSet<int>();
        private readonly StringBuilder _sb = new StringBuilder();
        private GUIStyle _style;

        private void OnEnable()
        {
            T16000MInput.OnButtonDown += HandleDown;
            T16000MInput.OnButtonUp += HandleUp;
        }

        private void OnDisable()
        {
            T16000MInput.OnButtonDown -= HandleDown;
            T16000MInput.OnButtonUp -= HandleUp;
        }

        private void HandleDown(int number)
        {
            _seen.Add(number);

            if (_logToConsole)
                Debug.Log($"[T.16000M] нажата кнопка {number} ({Name(number)})");
        }

        private void HandleUp(int number)
        {
            if (_logToConsole)
                Debug.Log($"[T.16000M] отпущена кнопка {number} ({Name(number)})");
        }

        private void OnGUI()
        {
            if (!_showOverlay)
                return;

            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true, wordWrap = true };

            _sb.Clear();
            _sb.AppendLine(T16000MInput.IsConnected
                ? "<b>T.16000M: подключён</b> — " + T16000MInput.DeviceName
                : "<b>T.16000M: НЕ подключён</b>");

            _sb.Append("X ").Append(Bar(T16000MInput.StickX, true));
            _sb.Append("   Y ").Append(Bar(T16000MInput.StickY, true)).AppendLine();
            _sb.Append("Поворот ").Append(Bar(T16000MInput.Twist, true));
            _sb.Append("   Тяга ").Append(Bar(T16000MInput.Throttle, false)).AppendLine();
            _sb.Append("Хатка: ").Append(T16000MInput.Hat).AppendLine();

            _sb.AppendLine().Append("<b>Зажаты сейчас:</b> ");
            bool any = false;

            for (int i = 1; i <= T16000MInput.ButtonCount; i++)
            {
                if (!T16000MInput.GetButton(i))
                    continue;

                _sb.Append(i).Append(" (").Append(Name(i)).Append(")   ");
                any = true;
            }

            _sb.AppendLine(any ? string.Empty : "—");

            _sb.AppendLine().Append("<b>Отработали за сессию (").Append(_seen.Count)
               .Append(" из ").Append(T16000MInput.ButtonCount).Append("):</b> ");

            foreach (int number in _seen)
                _sb.Append(number).Append(' ');

            _sb.AppendLine();

            if (_showControlNames)
                AppendControlNames();

            GUI.Label(new Rect(12f, 12f, Screen.width - 24f, Screen.height - 24f), _sb.ToString(), _style);
        }

        private void AppendControlNames()
        {
            InputDevice device = T16000MManager.CurrentDevice;

            if (device == null)
                return;

            _sb.AppendLine().Append("<b>Контролы (").Append(device.layout).Append("):</b> ");

            foreach (InputControl control in device.allControls)
                _sb.Append(control.path.Substring(device.path.Length + 1)).Append("  ");
        }

        /// <summary>Текстовая шкала оси: [----|##--] +0.35</summary>
        private static string Bar(float value, bool signed)
        {
            const int half = 10;
            var bar = new StringBuilder("[");

            if (signed)
            {
                int filled = Mathf.RoundToInt(Mathf.Abs(value) * half);

                for (int i = half; i > 0; i--)
                    bar.Append(value < 0f && i <= filled ? '#' : '-');

                bar.Append('|');

                for (int i = 1; i <= half; i++)
                    bar.Append(value > 0f && i <= filled ? '#' : '-');
            }
            else
            {
                int filled = Mathf.RoundToInt(value * half * 2);

                for (int i = 1; i <= half * 2; i++)
                    bar.Append(i <= filled ? '#' : '-');
            }

            return bar.Append("] ").Append(value.ToString(signed ? "+0.00;-0.00" : "0.00")).ToString();
        }

        private static string Name(int number) =>
            System.Enum.IsDefined(typeof(T16000MButton), number)
                ? ((T16000MButton)number).ToString()
                : "?";
    }
}
