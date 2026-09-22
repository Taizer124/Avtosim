using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Диагностика руля MOZA: показывает живое состояние всех кнопок,
    /// крестовин и энкодеров. Нужна, чтобы проверить раскладку на настоящем
    /// железе, а не полагаться на схему.
    ///
    /// Как пользоваться: повесить на любой объект в сцене, запустить, нажать
    /// по очереди каждую кнопку обода. Оверлей покажет номер и имя. Внизу
    /// копится список всех кнопок, отработавших за сессию, — если какая-то
    /// не появилась, она не читается.
    ///
    /// В бой не тащить — это инструмент настройки.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/Input/MOZA Wheel Input Tester")]
    public class MozaWheelInputTester : MonoBehaviour
    {
        [Header("Вывод")]
        [Tooltip("Оверлей поверх игры.")]
        [SerializeField] private bool _showOverlay = true;

        [Tooltip("Писать каждое нажатие в консоль.")]
        [SerializeField] private bool _logToConsole = true;

        [Tooltip("Показывать кнопки, у которых нет имени на схеме обода.")]
        [SerializeField] private bool _showUnnamed = true;

        [Header("Залипшие кнопки")]
        [Tooltip("Сколько секунд кнопка должна быть зажата, чтобы счесть её " +
                 "залипшей. Многопозиционные селекторы залипают штатно — они " +
                 "физически держат позицию, это не поломка.")]
        [SerializeField] private float _stuckThresholdSeconds = 5f;

        private readonly SortedSet<int> _seen = new SortedSet<int>();
        private readonly float[] _heldSince = new float[MozaWheelInput.MaxButtons];
        private readonly StringBuilder _sb = new StringBuilder();
        private GUIStyle _style;

        private void OnEnable()
        {
            MozaWheelInput.OnButtonDown += HandleDown;
            MozaWheelInput.OnButtonUp += HandleUp;
        }

        private void OnDisable()
        {
            MozaWheelInput.OnButtonDown -= HandleDown;
            MozaWheelInput.OnButtonUp -= HandleUp;
        }

        private void HandleDown(int number)
        {
            _seen.Add(number);
            _heldSince[number] = Time.unscaledTime;

            if (_logToConsole)
                Debug.Log($"[MOZA] нажата кнопка {number} ({Describe(number)})");
        }

        private void HandleUp(int number)
        {
            _heldSince[number] = 0f;

            if (_logToConsole)
                Debug.Log($"[MOZA] отпущена кнопка {number} ({Describe(number)})");
        }

        private void OnGUI()
        {
            if (!_showOverlay)
                return;

            _style ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                richText = true,
                wordWrap = true,
            };

            _sb.Clear();
            _sb.AppendLine(MozaWheelInput.IsConnected
                ? "<b>MOZA: подключён</b>"
                : "<b>MOZA: НЕ подключён</b>");

            _sb.Append("Руль: ").Append(MozaWheelInput.Steering.ToString("+0.00;-0.00"));
            _sb.Append(" (").Append(MozaWheelInput.SteeringAngleDegrees.ToString("0")).Append("° из ±");
            _sb.Append((MozaWheelInput.SteeringRangeDegrees / 2f).ToString("0")).Append("°)");
            _sb.Append("   Газ: ").Append(MozaWheelInput.Throttle.ToString("0.00"));
            _sb.Append("   Тормоз: ").Append(MozaWheelInput.Brake.ToString("0.00"));
            _sb.Append("   Сцепление: ").Append(MozaWheelInput.Clutch.ToString("0.00"));
            _sb.AppendLine();

            _sb.Append("Лепестки передач: L(13)=").Append(MozaWheelInput.GetButton(MozaButton.LeftPaddle) ? "ЗАЖАТ" : "—");
            _sb.Append("  R(14)=").Append(MozaWheelInput.GetButton(MozaButton.RightPaddle) ? "ЗАЖАТ" : "—");
            _sb.Append("   Лепестки сцепления: ").Append(MozaWheelInput.ClutchPaddle.ToString("0.00"));
            _sb.Append(" (L ").Append(MozaWheelInput.ClutchPaddleLeft.ToString("0.00"));
            _sb.Append(" / R ").Append(MozaWheelInput.ClutchPaddleRight.ToString("0.00")).Append(')');
            _sb.AppendLine();

            _sb.Append("Передача: ").Append(GearName(MozaWheelInput.Gear));
            _sb.Append("    Ручник: ").Append(MozaWheelInput.Handbrake ? "да" : "нет");
            _sb.AppendLine();

            _sb.Append("Крестовины: лево=").Append(MozaWheelInput.LeftRocker);
            _sb.Append("  право=").Append(MozaWheelInput.RightRocker);
            _sb.AppendLine();

            _sb.Append("Энкодеры: L ").Append(MozaWheelInput.LeftKnobPosition);
            _sb.Append(" (").Append(Signed(MozaWheelInput.LeftKnobDelta)).Append(')');
            _sb.Append("   R ").Append(MozaWheelInput.RightKnobPosition);
            _sb.Append(" (").Append(Signed(MozaWheelInput.RightKnobDelta)).Append(')');
            _sb.AppendLine();

            _sb.Append("Селекторы: ");
            for (int i = 0; i < MozaWheelInput.MultiSegmentKnob.Length; i++)
                _sb.Append(MozaWheelInput.MultiSegmentKnob[i]).Append(' ');
            _sb.AppendLine();

            AppendHeld();
            AppendStuck();
            AppendSeen();

            GUI.Label(new Rect(12f, 12f, Screen.width - 24f, Screen.height - 24f), _sb.ToString(), _style);
        }

        private void AppendHeld()
        {
            _sb.AppendLine().AppendLine("<b>Зажаты сейчас:</b>");

            bool any = false;

            for (int i = 1; i < MozaWheelInput.MaxButtons; i++)
            {
                if (!MozaWheelInput.GetButton(i))
                    continue;

                string name = Name(i);

                if (name == null && !_showUnnamed)
                    continue;

                _sb.Append(i).Append(" (").Append(name ?? "без имени на схеме").Append(")   ");
                any = true;
            }

            if (!any)
                _sb.Append("—");

            _sb.AppendLine();
        }

        private void AppendStuck()
        {
            bool any = false;

            for (int i = 1; i < MozaWheelInput.MaxButtons; i++)
            {
                if (_heldSince[i] <= 0f || !MozaWheelInput.GetButton(i))
                    continue;

                if (Time.unscaledTime - _heldSince[i] < _stuckThresholdSeconds)
                    continue;

                if (!any)
                {
                    _sb.AppendLine().AppendLine("<b>Держатся дольше порога:</b>");
                    any = true;
                }

                _sb.Append(i).Append(" (").Append(Name(i) ?? "без имени").Append(")   ");
            }

            if (any)
            {
                _sb.AppendLine();
                _sb.AppendLine("Для многопозиционного селектора это норма — он держит позицию физически.");
            }
        }

        private void AppendSeen()
        {
            _sb.AppendLine().Append("<b>Отработали за сессию (").Append(_seen.Count).AppendLine("):</b>");

            if (_seen.Count == 0)
            {
                _sb.Append("— пока ни одной");
                return;
            }

            foreach (int number in _seen)
                _sb.Append(number).Append(' ');
        }

        private static string Signed(int value) => value > 0 ? "+" + value : value.ToString();

        private static string GearName(int gear) =>
            gear < 0 ? "R" : gear == 0 ? "N" : gear.ToString();

        private static string Describe(int number) => Name(number) ?? "нет имени на схеме";

        /// <summary>Имя кнопки со схемы обода, либо null.</summary>
        private static string Name(int number) =>
            System.Enum.IsDefined(typeof(MozaButton), number)
                ? ((MozaButton)number).ToString()
                : null;
    }
}
