using UnityEngine;
using System;
using mozaAPI;

namespace Assets.VehicleController
{
    /// <summary>
    /// Опрос руля MOZA. Раскладывает HIDData в <see cref="MozaWheelInput"/>
    /// (все кнопки, крестовины, энкодеры) и в AllInOneInputProvider (педали,
    /// руль, КПП + четыре кнопки ромба, которые использует сама игра).
    ///
    /// DefaultExecutionOrder = -1000: менеджер обязан отработать раньше всех,
    /// кто читает MozaWheelInput, иначе GetButtonDown у них будет опаздывать
    /// на кадр или пропадать.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class MozaSdkManager : MonoBehaviour
    {
        private static MozaSdkManager _instance;
        public static MozaSdkManager Instance => _instance;

        private bool _isSdkInstalled = false;
        private AllInOneInputProvider _inputProvider;
        private int _limitAngle = 900;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            GameObject sdkGO = new GameObject("MOZA_SDK_Manager");
            sdkGO.AddComponent<MozaSdkManager>();
            DontDestroyOnLoad(sdkGO);
            Debug.Log("[MOZA SDK] Runtime Manager auto-created.");
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

        private void Start()
        {
            try
            {
                mozaAPI.mozaAPI.installMozaSDK();
                _isSdkInstalled = true;
                Debug.Log("[MOZA SDK] SDK successfully installed.");

                ERRORCODE err = ERRORCODE.NORMAL;
                var limit = mozaAPI.mozaAPI.getMotorLimitAngle(ref err);
                if (err == ERRORCODE.NORMAL && limit != null)
                {
                    _limitAngle = Math.Abs(limit.Item2);
                    if (_limitAngle <= 0) _limitAngle = 900;
                    Debug.Log($"[MOZA SDK] Steering Wheel Limit Angle: {_limitAngle} degrees.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MOZA SDK] Failed to install SDK: {ex.Message}");
            }

            FindInputProvider();
        }

        private void FindInputProvider()
        {
            _inputProvider = FindFirstObjectByType<AllInOneInputProvider>();
        }

        private void Update()
        {
            if (!_isSdkInstalled) return;

            // Провайдер ищем, но его отсутствие НЕ повод бросать опрос: руль
            // может использоваться и без него — напрямую через MozaWheelInput.
            // Раньше здесь стоял return, и в сцене без AllInOneInputProvider
            // кнопки не читались вообще.
            if (_inputProvider == null)
                FindInputProvider();

            try
            {
                ERRORCODE err = ERRORCODE.NORMAL;
                HIDData data = mozaAPI.mozaAPI.getHIDData(ref err);

                if (err == ERRORCODE.NORMAL)
                {
                    // 1. Steering Wheel Angle
                    float steer = 0f;
                    if (!float.IsNaN(data.fSteeringWheelAngle))
                    {
                        float halfLimit = _limitAngle / 2f;
                        steer = Mathf.Clamp(data.fSteeringWheelAngle / halfLimit, -1f, 1f);
                    }
                    else
                    {
                        steer = (float)data.steeringWheelAxle / 32767f;
                    }

                    // 2. Pedals: throttle, brake, clutch (Int16 to 0..1f)
                    float gas = Mathf.Clamp01((float)(data.throttle - (-32768)) / 65535f);
                    float brake = Mathf.Clamp01((float)(data.brake - (-32768)) / 65535f);
                    float clutch = Mathf.Clamp01((float)(data.clutch - (-32768)) / 65535f);

                    // 3. Handbrake
                    bool handbrake = data.buttonHandbrake;
                    if (!handbrake)
                    {
                        float hbVal = Mathf.Clamp01((float)(data.handbrake - (-32767)) / 65534f);
                        handbrake = hbVal > 0.5f;
                    }

                    // 4. Shifter Gear
                    int gear = 0;
                    switch (data.shift)
                    {
                        case GEAR.GEAR0th:
                            gear = 0;
                            break;
                        case GEAR.GEAR1st:
                            gear = 1;
                            break;
                        case GEAR.GEAR2nd:
                            gear = 2;
                            break;
                        case GEAR.GEAR3rd:
                            gear = 3;
                            break;
                        case GEAR.GEAR4th:
                            gear = 4;
                            break;
                        case GEAR.GEAR5th:
                            gear = 5;
                            break;
                        case GEAR.GEAR6th:
                            gear = 6;
                            break;
                        case GEAR.GEAR7th:
                            gear = 7;
                            break;
                        case GEAR.R:
                            gear = -1;
                            break;
                    }

                    // _limitAngle уже хранит полный диапазон "стопор-в-стопор"
                    // (см. Start() — halfLimit там же считается как _limitAngle/2),
                    // поэтому для CockpitSteeringWheel передаём как есть.
                    // Оси уходят и в MozaWheelInput — чтобы собственный провайдер
                    // студента мог читать руль и педали, не завися от
                    // AllInOneInputProvider.
                    MozaWheelInput.SetAxes(
                        steer, gas, brake, clutch,
                        float.IsNaN(data.fSteeringWheelAngle) ? 0f : data.fSteeringWheelAngle,
                        _limitAngle);

                    if (_inputProvider != null)
                        _inputProvider.SetMozaInputs(gas, brake, clutch, steer, handbrake, gear, _limitAngle);

                    // Аналоговые лепестки сцепления (FSR, KS, CS...). У ES их
                    // нет — SDK отдаёт значение по умолчанию 0x8001, то есть 0.
                    MozaWheelInput.SetClutchPaddles(
                        PaddleAxis(data.clutchSynthesisShaft),
                        PaddleAxis(data.clutchIndependentShaftL),
                        PaddleAxis(data.clutchIndependentShaftR));

                    MozaWheelInput.SetSteeringDynamics(
                        float.IsNaN(data.fSteeringWheelVelocity) ? 0f : data.fSteeringWheelVelocity,
                        float.IsNaN(data.fSteeringWheelAcceleration) ? 0f : data.fSteeringWheelAcceleration);

                    // 5. КНОПКИ. Разбираем ВЕСЬ обод, а не четыре кнопки:
                    // плагином пользуются извне, и там могут понадобиться любые.
                    // Полное состояние уходит в MozaWheelInput, откуда его
                    // читают по номеру или по имени (MozaButton.Start и т.д.).
                    //
                    // НУМЕРАЦИЯ 1-BASED. data.buttons[N] — кнопка с номером N
                    // на схеме руля; buttons[0] в схеме не участвует. Основания:
                    //   * официальный пример SDK sdk_api_test.cc перебирает
                    //     for (int i = 1; i < 113; i++) d->buttons[i];
                    //   * имена полей HIDData кодируют номера напрямую —
                    //     leftRocker5_8, rightRocker9_12, knobL45_46, knobR47_48,
                    //     multiSegmentKnob26_27or53_64 и т.д.;
                    //   * на схеме обода крестовина подписана 5..8 — ровно то же,
                    //     что и поле leftRocker5_8.
                    //
                    // getHIDData отдаёт данные за цикл опроса ("all hid data
                    // during the cycle"): startValue — состояние на НАЧАЛО окна,
                    // changeNum — сколько раз оно менялось внутри (-1 — ни разу).
                    // Состояние на конец окна — LastPressState(). IsPressed() не
                    // подходит: она отвечает "нажималась ли хоть раз за цикл" и
                    // залипает на true.
                    //
                    // Число нажатий считаем САМИ (см. CountEdges), а не через
                    // PressNum(): та возвращает changeNum/2 + (changeNum & 1),
                    // то есть число ИЗМЕНЕНИЙ пополам с округлением вверх.
                    // Отпускание кнопки — одно изменение, и PressNum() = 1, из-за
                    // чего GetButtonDown срабатывал и на нажатие, и на отпускание.
                    MozaWheelInput.BeginFrame();
                    MozaWheelInput.SetConnected(true);

                    MozaDirection leftRocker = ToDirection(data.leftRocker5_8.LastDir());
                    MozaDirection rightRocker = ToDirection(data.rightRocker9_12.LastDir());
                    MozaWheelInput.SetRockers(leftRocker, rightRocker);

                    if (data.buttons != null)
                    {
                        int last = Mathf.Min(data.buttons.Length, MozaWheelInput.MaxButtons);

                        for (int number = 1; number < last; number++)
                        {
                            HIDButton button = data.buttons[number];
                            bool level = button.LastPressState();
                            CountEdges(button, out int presses, out int releases);

                            // Крестовина в Pit House может быть в режиме hat
                            // switch — тогда кнопки 5-12 в buttons[] молчат, а
                            // направление приходит только в leftRocker/rightRocker.
                            // Сводим направление обратно в кнопки, чтобы
                            // GetButton(MozaButton.DpadUp) работал в любом режиме.
                            if (number >= 5 && number <= 12)
                                level |= RockerHolds(number, leftRocker, rightRocker);

                            MozaWheelInput.SetButton(number, level, presses, releases);
                        }
                    }

                    // У энкодеров GetOffset() — сумма щелчков за цикл опроса,
                    // со знаком. Это уже дельта, накапливать её не нужно.
                    MozaWheelInput.SetKnobs(
                        data.knobL45_46.GetOffset(),
                        data.knobR47_48.GetOffset());

                    MozaWheelInput.SetMultiSegmentKnob(0, data.multiSegmentKnob26_27or53_64.GetLastKey());
                    MozaWheelInput.SetMultiSegmentKnob(1, data.multiSegmentKnob28_29or65_76.GetLastKey());
                    MozaWheelInput.SetMultiSegmentKnob(2, data.multiSegmentKnob30_31or77_88.GetLastKey());
                    MozaWheelInput.SetMultiSegmentKnob(3, data.multiSegmentKnob39_40or89_100.GetLastKey());
                    MozaWheelInput.SetMultiSegmentKnob(4, data.multiSegmentKnob43_44or101_112.GetLastKey());

                    MozaWheelInput.SetVehicleState(gear, handbrake);

                    // Подрулевые лепестки -> секвентальная коробка игры.
                    // Раньше их в игру не передавали вовсе: провайдер получал
                    // только педали и ромб, а GetGearUpInput/GetGearDownInput
                    // заполнялись лишь из Input System (раскладка Logitech G29).
                    // С рулём MOZA лепестки читались в MozaWheelInput, но
                    // передачу не переключали.
                    if (_inputProvider != null)
                    {
                        _inputProvider.SetMozaShiftPaddles(
                            MozaWheelInput.GetButtonDown(MozaButton.RightPaddle),
                            MozaWheelInput.GetButtonDown(MozaButton.LeftPaddle));
                    }

                    // Четвёрка ромба для самой игры. Раскладка по схеме обода:
                    // 1=A (низ), 2=B (право), 3=X (лево), 4=Y (верх).
                    // Раньше здесь читались buttons[0..3] — слот вне схемы плюс
                    // сдвиг на единицу, из-за чего кнопка 4 не читалась совсем,
                    // а X и Y были перепутаны местами.
                    if (_inputProvider != null)
                    {
                        _inputProvider.SetMozaButtons(
                            MozaWheelInput.GetButton(MozaButton.Y),
                            MozaWheelInput.GetButton(MozaButton.A),
                            MozaWheelInput.GetButton(MozaButton.B),
                            MozaWheelInput.GetButton(MozaButton.X));
                    }
                }
                else
                {
                    MozaWheelInput.SetConnected(false);

                    if (_inputProvider != null)
                        _inputProvider.SetMozaDisconnected();
                }
            }
            catch (Exception)
            {
                MozaWheelInput.SetConnected(false);

                if (_inputProvider != null)
                    _inputProvider.SetMozaDisconnected();
            }
        }

        /// <summary>
        /// Сколько нажатий и отпусканий уместилось в цикл опроса.
        /// Изменения чередуются, начиная от startValue: из отпущенного
        /// состояния первое изменение — нажатие, из зажатого — отпускание.
        /// </summary>
        private static void CountEdges(HIDButton button, out int presses, out int releases)
        {
            int changes = Math.Max(button.changeNum, 0);
            int odd = (changes + 1) / 2;
            int even = changes / 2;

            presses = button.startValue ? even : odd;
            releases = button.startValue ? odd : even;
        }

        /// <summary>Держит ли крестовина направление, соответствующее кнопке 5-12.</summary>
        private static bool RockerHolds(int number, MozaDirection left, MozaDirection right)
        {
            MozaDirection dir = number <= 8 ? left : right;

            switch ((number - 5) % 4) // 0 = верх, 1 = право, 2 = низ, 3 = лево
            {
                case 0: return dir == MozaDirection.Up || dir == MozaDirection.UpLeft || dir == MozaDirection.UpRight;
                case 1: return dir == MozaDirection.Right || dir == MozaDirection.UpRight || dir == MozaDirection.DownRight;
                case 2: return dir == MozaDirection.Down || dir == MozaDirection.DownLeft || dir == MozaDirection.DownRight;
                default: return dir == MozaDirection.Left || dir == MozaDirection.UpLeft || dir == MozaDirection.DownLeft;
            }
        }

        /// <summary>Ось лепестка сцепления: int16 -32767..32767 -> 0..1.</summary>
        private static float PaddleAxis(short raw) =>
            Mathf.Clamp01((raw + 32767f) / 65534f);

        /// <summary>
        /// Переводит направление крестовины из SDK в наш enum.
        /// Имена в SDK с опечатками (DOWM, RIGHTDOWM) — так в оригинале.
        /// </summary>
        private static MozaDirection ToDirection(ROCKEREDIR dir)
        {
            switch (dir)
            {
                case ROCKEREDIR.UP: return MozaDirection.Up;
                case ROCKEREDIR.RIGHTUP: return MozaDirection.UpRight;
                case ROCKEREDIR.RIGHT: return MozaDirection.Right;
                case ROCKEREDIR.RIGHTDOWM: return MozaDirection.DownRight;
                case ROCKEREDIR.DOWM: return MozaDirection.Down;
                case ROCKEREDIR.LEFTDOWM: return MozaDirection.DownLeft;
                case ROCKEREDIR.LEFT: return MozaDirection.Left;
                case ROCKEREDIR.LEFTUP: return MozaDirection.UpLeft;
                default: return MozaDirection.None;
            }
        }

        private void OnDestroy()
        {
            if (_isSdkInstalled)
            {
                try
                {
                    mozaAPI.mozaAPI.removeMozaSDK();
                    Debug.Log("[MOZA SDK] SDK successfully removed.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MOZA SDK] Failed to remove SDK: {ex.Message}");
                }
            }
        }
    }
}
