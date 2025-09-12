#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using LogitechG29.Runtime.LogitechSDK;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Scripting;

#endregion

namespace LogitechG29.Runtime
{
#if UNITY_EDITOR
    [Preserve, InitializeOnLoad]
#endif
    [InputControlLayout(displayName = "Logitech G29", stateType = typeof(LogitechG29State),
        description = "Logitech G29 Racing Wheel with Force Feedback", stateFormat = "HID")]
    public sealed class LogitechG29 : InputDevice, IInputUpdateCallbackReceiver
    {
        private const bool IGNORE_XINPUT_CONTROLLERS = false;

        public enum G29Button : byte
        {
            South = 0,
            West = 1,
            East = 2,
            North = 3,

            Cross = South,
            Triangle = North,
            Circle = East,
            Square = West,

            RightBumper = 4,
            LeftBumper = 5,
            RightTrigger = 6,
            LeftTrigger = 7,
            Share = 8,
            Options = 9,
            RightStick = 10,
            LeftStick = 11,

            Shifter1 = 12,
            Shifter2 = 13,
            Shifter3 = 14,
            Shifter4 = 15,
            Shifter5 = 16,
            Shifter6 = 17,
            Shifter7 = 18,

            Plus = 19,
            Minus = 20,

            RightSpin = 21,
            LeftSpin = 22,
            EnterSpin = 23,

            Home = 24,

            DpadUp,
            DpadDown,
            DpadLeft,
            DpadRight
        }

        public ButtonControl this[G29Button button] => button switch
        {
            G29Button.Cross => SouthButton,
            G29Button.North => NorthButton,
            G29Button.East => EastButton,
            G29Button.West => WestButton,

            G29Button.RightBumper => RightBumper,
            G29Button.LeftBumper => LeftBumper,
            G29Button.Options => Options,
            G29Button.Share => Share,
            G29Button.Home => Home,
            G29Button.LeftStick => LeftStickButton,
            G29Button.RightStick => RightStickButton,
            G29Button.LeftTrigger => LeftShift,
            G29Button.RightTrigger => RightShift,
            G29Button.Plus => Plus,
            G29Button.Minus => Minus,

            G29Button.Shifter1 => Shifter1,
            G29Button.Shifter2 => Shifter2,
            G29Button.Shifter3 => Shifter3,
            G29Button.Shifter4 => Shifter4,
            G29Button.Shifter5 => Shifter5,
            G29Button.Shifter6 => Shifter6,
            G29Button.Shifter7 => Shifter7,

            G29Button.LeftSpin => LeftSpin,
            G29Button.RightSpin => RightSpin,
            G29Button.EnterSpin => EnterSpin,

            G29Button.DpadUp => HatSwitch.up,
            G29Button.DpadDown => HatSwitch.down,
            G29Button.DpadLeft => HatSwitch.left,
            G29Button.DpadRight => HatSwitch.right,

            _ => throw new InvalidEnumArgumentException("button", (int)button, typeof(G29Button))
        };

        public void OnUpdate()
        {
            if (!IsConnected)
            {
                return;
            }

            // if the return value is false, means that the application has not been initialized
            if (!LogitechGsdk.LogiUpdate())
            {
            }
        }

        public static event UnityAction OnInitialized;
        public static event UnityAction OnShutdown;

        public override void MakeCurrent()
        {
            base.MakeCurrent();
            Current = this;
        }

        protected override void OnAdded()
        {
            base.OnAdded();
            sAllMyDevices.Add(this);
        }

        protected override void OnRemoved()
        {
            base.OnRemoved();

            if (Current == this)
            {
                Current = null;
            }

            sAllMyDevices.Remove(this);
        }

        protected override void FinishSetup()
        {
            Steering = GetChildControl<AxisControl>("stick/x");
            Throttle = GetChildControl<AxisControl>("throttleAxis");
            Brake = GetChildControl<AxisControl>("brakeAxis");
            Clutch = GetChildControl<AxisControl>("clutchAxis");

            NorthButton = GetChildControl<ButtonControl>("northButton");
            SouthButton = GetChildControl<ButtonControl>("southButton");
            EastButton = GetChildControl<ButtonControl>("eastButton");
            WestButton = GetChildControl<ButtonControl>("westButton");

            RightBumper = GetChildControl<ButtonControl>("rightBumperButton");
            LeftBumper = GetChildControl<ButtonControl>("leftBumperButton");
            RightShift = GetChildControl<ButtonControl>("rightShiftButton");
            LeftShift = GetChildControl<ButtonControl>("leftShiftButton");
            RightStickButton = GetChildControl<ButtonControl>("rightStickButton");
            LeftStickButton = GetChildControl<ButtonControl>("leftStickButton");
            Share = GetChildControl<ButtonControl>("shareButton");
            Options = GetChildControl<ButtonControl>("optionsButton");
            Home = GetChildControl<ButtonControl>("homeButton");
            Plus = GetChildControl<ButtonControl>("plusButton");
            Minus = GetChildControl<ButtonControl>("minusButton");
            RightSpin = GetChildControl<ButtonControl>("rightTurnButton");
            LeftSpin = GetChildControl<ButtonControl>("leftTurnButton");
            EnterSpin = GetChildControl<ButtonControl>("returnButton");
            HatSwitch = GetChildControl<DpadControl>("hatSwitch");

            Shifter1 = GetChildControl<ButtonControl>("shifter1");
            Shifter2 = GetChildControl<ButtonControl>("shifter2");
            Shifter3 = GetChildControl<ButtonControl>("shifter3");
            Shifter4 = GetChildControl<ButtonControl>("shifter4");
            Shifter5 = GetChildControl<ButtonControl>("shifter5");
            Shifter6 = GetChildControl<ButtonControl>("shifter6");
            Shifter7 = GetChildControl<ButtonControl>("shifter7");

            base.FinishSetup();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            InputSystem.RegisterLayout<LogitechG29>
            (
                "Logitech G29 Racing Wheel",
                new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithManufacturer("Logitech")
                    .WithProduct("G29 Driving Force Racing Wheel")
                    .WithCapability("vendorId", 0x46D)
                    .WithCapability("productId", 0xC24F)
                    .WithCapability("usagePage", 1)
                //.WithCapability("usage", 4)
                //.WithVersion("35072")
                //.WithCapability("usagePage", "GenericDesktop")
            );

            if (!Application.isPlaying)
            {
                return;
            }

            var init = LogitechGsdk.LogiSteeringInitialize(IGNORE_XINPUT_CONTROLLERS);
            Application.quitting += OnQuit;

#if UNITY_EDITOR
            if (EditorPrefs.GetBool(DEBUG_PATH))
            {
                if (init)
                {
                    Debug.Log("Logitech G29 has been initialized!");
                }
                else
                {
                    Debug.LogError("Logitech G29 has not been initialized!");
                }
            }
#endif

            OnInitialized?.Invoke();
        }

        private static void OnQuit()
        {
            Application.quitting -= OnQuit;

            if (Current != null)
            {
                Current.StopAllForceFeedback();
            }

            LogitechGsdk.LogiSteeringShutdown();

#if UNITY_EDITOR
            if (EditorPrefs.GetBool(DEBUG_PATH))
            {
                Debug.Log("Logitech G29 has been shutdown");
            }
#endif

            OnShutdown?.Invoke();
        }

#pragma warning disable IDE1006 // Naming Styles
        public static LogitechG29 Current { get; private set; }

        public static IReadOnlyList<LogitechG29> All => sAllMyDevices;
        private static readonly List<LogitechG29> sAllMyDevices = new();

        #region Variables

        public bool IsPlayingSpringForce => LogitechGsdk.LogiIsPlaying(Index, LogitechGsdk.LOGI_FORCE_SPRING);
        public bool IsPlayingDamperForce => LogitechGsdk.LogiIsPlaying(Index, LogitechGsdk.LOGI_FORCE_DAMPER);
        public bool IsPlayingConstantForce => LogitechGsdk.LogiIsPlaying(Index, LogitechGsdk.LOGI_FORCE_CONSTANT);
        public bool IsPlayingSurfaceEffect => LogitechGsdk.LogiIsPlaying(Index, LogitechGsdk.LOGI_FORCE_SURFACE_EFFECT);
        public bool IsPlayingSoftstopForce => LogitechGsdk.LogiIsPlaying(Index, LogitechGsdk.LOGI_FORCE_SOFTSTOP);
        public bool IsPlayingDirtRoadEffect => LogitechGsdk.LogiIsPlaying(Index, LogitechGsdk.LOGI_FORCE_DIRT_ROAD);
        public bool IsPlayingBumpyRoadEffect => LogitechGsdk.LogiIsPlaying(Index, LogitechGsdk.LOGI_FORCE_BUMPY_ROAD);
        public bool IsPlayingCarAirborne => LogitechGsdk.LogiIsPlaying(Index, LogitechGsdk.LOGI_FORCE_CAR_AIRBORNE);

        public bool IsPlayingSlippyRoadEffect =>
            LogitechGsdk.LogiIsPlaying(Index, LogitechGsdk.LOGI_FORCE_SLIPPERY_ROAD);

        public int Index { get; } = 0;
        public bool IsConnected => LogitechGsdk.LogiIsDeviceConnected(Index, LogitechGsdk.LOGI_DEVICE_TYPE_WHEEL);

        //public AnyKeyControl anyButton { get; private set; }
        public ButtonControl NorthButton { get; private set; }
        public ButtonControl SouthButton { get; private set; }
        public ButtonControl EastButton { get; private set; }
        public ButtonControl WestButton { get; private set; }
        public ButtonControl RightBumper { get; private set; }
        public ButtonControl LeftBumper { get; private set; }
        public ButtonControl RightShift { get; private set; }
        public ButtonControl LeftShift { get; private set; }
        public ButtonControl Share { get; private set; }
        public ButtonControl Options { get; private set; }
        public ButtonControl Home { get; private set; }
        public ButtonControl RightStickButton { get; private set; }
        public ButtonControl LeftStickButton { get; private set; }
        public ButtonControl Plus { get; private set; }
        public ButtonControl Minus { get; private set; }
        public ButtonControl RightSpin { get; private set; }
        public ButtonControl LeftSpin { get; private set; }
        public ButtonControl EnterSpin { get; private set; }

        public DpadControl HatSwitch { get; private set; }

        public ButtonControl Shifter1 { get; private set; }
        public ButtonControl Shifter2 { get; private set; }
        public ButtonControl Shifter3 { get; private set; }
        public ButtonControl Shifter4 { get; private set; }
        public ButtonControl Shifter5 { get; private set; }
        public ButtonControl Shifter6 { get; private set; }
        public ButtonControl Shifter7 { get; private set; }

        public AxisControl Steering { get; private set; }
        public AxisControl Throttle { get; private set; }
        public AxisControl Brake { get; private set; }
        public AxisControl Clutch { get; private set; }

        public ButtonControl TriangleButton => NorthButton;
        public ButtonControl SquareButton => WestButton;
        public ButtonControl CircleButton => EastButton;
        public ButtonControl CrossButton => SouthButton;

#pragma warning restore IDE1006 // Naming Styles

        #endregion

        #region Force Feedback

        public void StopAllForceFeedback()
        {
            if (IsPlayingConstantForce)
            {
                StopConstantForce();
            }

            if (IsPlayingDamperForce)
            {
                StopDamperForce();
            }

            if (IsPlayingBumpyRoadEffect)
            {
                StopBumpyRoadEffect();
            }

            if (IsPlayingCarAirborne)
            {
                StopCarAirborne();
            }

            if (IsPlayingDirtRoadEffect)
            {
                StopDirtRoadEffect();
            }

            if (IsPlayingSlippyRoadEffect)
            {
                StopSlipperyRoadEffect();
            }

            if (IsPlayingSoftstopForce)
            {
                StopSoftstopForce();
            }

            if (IsPlayingSpringForce)
            {
                StopSpringForce();
            }

            if (IsPlayingSurfaceEffect)
            {
                StopSurfaceEffect();
            }
        }

        /// <summary>
        ///     The dynamic spring force gets played on the X axis.
        ///     If a joystick is connected, all forces generated by the Steering Wheel SDK will be played on the X axis.
        ///     And in addition there will be a constant spring on the Y axis.
        /// </summary>
        /// <param name="offset">
        ///     Specifies the center of the spring force effect. Valid range is -100 to 100.
        ///     Specifying 0 centers the spring. Any values outside this range are silently clamped.
        /// </param>
        /// <param name="saturation">
        ///     Specify the level of saturation of the spring force effect.
        ///     The saturation stays constant after a certain deflection from the center of the spring.
        ///     It is comparable to a magnitude. Valid ranges are 0 to 100.
        ///     Any value higher than 100 is silently clamped.
        /// </param>
        /// <param name="coefficient">
        ///     Specify the slope of the effect strength increase relative
        ///     to the amount of deflection from the center of the condition.
        ///     Higher values mean that the saturation level is reached sooner.
        ///     Valid ranges are -100 to 100. Any value outside the valid range is silently clamped.
        /// </param>
        public bool PlaySpringForce(int offset, int saturation, int coefficient)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiPlaySpringForce(Index, offset, saturation, coefficient);
        }

        /// <summary>
        ///     Stop current spring force
        /// </summary>
        /// <returns></returns>
        public bool StopSpringForce()
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiStopSpringForce(Index);
        }

        /// <summary>
        ///     A constant force works best when continuously updated with a value tied to the physics engine.
        ///     Tie the steering wheel/joystick to the car's physics engine via a vector force.
        ///     This will create a centering spring effect, a sliding effect, a feeling for the car's inertia,
        ///     and depending on the physics engine it should also give side collisions
        ///     (wheel/joystick jerks in the opposite way of the wall the car just touched).
        ///     The vector force could for example be calculated from the lateral force measured at the front tires.
        ///     This vector force should be 0 when at a stop or driving straight.
        ///     When driving through a turn or when driving on a banked surface the vector force
        ///     should have a magnitude that grows in a proportional way.
        /// </summary>
        /// <param name="magnitude">
        ///     Specifies the magnitude of the constant force effect.
        ///     A negative value reverses the direction of the force.
        ///     Valid ranges for magnitudePercentage are -100 to 100.
        ///     Any values outside the valid range are silently clamped.
        /// </param>
        /// <returns></returns>
        public bool PlayConstantForce(int magnitude)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiPlayConstantForce(Index, magnitude);
        }

        /// <summary>
        ///     Stop current constant force
        /// </summary>
        /// <returns></returns>
        public bool StopConstantForce()
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiStopConstantForce(Index);
        }

        /// <summary>
        ///     Simulate surfaces that are hard to turn on (mud, car at a stop) or slippery surfaces (snow, ice)
        /// </summary>
        /// <param name="coefficient">
        ///     Specify the slope of the effect strength increase relative
        ///     to the amount of deflection from the center of the condition.
        ///     Higher values mean that the saturation level is reached sooner.
        ///     Valid ranges are -100 to 100. Any value outside the valid range is silently clamped.
        ///     -100 simulates a very slippery effect, +100 makes the wheel/joystick very hard to move,
        ///     simulating the car at a stop or in mud.
        /// </param>
        /// <returns></returns>
        public bool PlayDamperForce(int coefficient)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiPlayDamperForce(Index, coefficient);
        }

        /// <summary>
        ///     Stop current damper force
        /// </summary>
        /// <returns></returns>
        public bool StopDamperForce()
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiStopDamperForce(Index);
        }

        /// <summary>
        /// </summary>
        /// <param name="magnitude">
        ///     : specifies the magnitude of the frontal collision force effect.
        ///     Valid ranges for magnitudePercentage are 0 to 100.
        ///     Values higher than 100 are silently clamped.
        /// </param>
        /// <returns></returns>
        public bool PlayFontalCollisionForce(int magnitude)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiPlayFrontalCollisionForce(Index, magnitude);
        }

        /// <summary>
        ///     If you are already using a constant force tied to a vector force from the physics engine,
        ///     then you may not need to add side collisions since depending on your physics
        ///     engine the side collisions may automatically be taken care of by the constant force.
        /// </summary>
        /// <param name="magnitude">
        ///     Specifies the magnitude of the side collision force effect.
        ///     A negative value reverses the direction of the force.
        ///     Valid ranges for magnitudePercentage are -100 to 100.
        ///     Any values outside the valid range are silently clamped.
        /// </param>
        /// <returns></returns>
        public bool PlaySideCollisionForce(int magnitude)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiPlaySideCollisionForce(Index, magnitude);
        }

        /// <summary>
        /// </summary>
        /// <param name="magnitude">
        ///     Specifies the magnitude of the dirt road effect.
        ///     Valid ranges for magnitudePercentage are 0 to 100.
        ///     Values higher than 100 are silently clamped.
        /// </param>
        /// <returns></returns>
        public bool PlayDirtRoadEffect(int magnitude)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiPlayDirtRoadEffect(Index, magnitude);
        }

        /// <summary>
        ///     Stop current dirt-road effect
        /// </summary>
        /// <returns></returns>
        public bool StopDirtRoadEffect()
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiStopDirtRoadEffect(Index);
        }

        /// <summary>
        /// </summary>
        /// <param name="magnitude">
        ///     Specifies the magnitude of the bumpy road effect.
        ///     Valid ranges for magnitudePercentage are 0 to 100.
        ///     Values higher than 100 are silently clamped.
        /// </param>
        /// <returns></returns>
        public bool PlayBumpyRoadEffect(int magnitude)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiPlayBumpyRoadEffect(Index, magnitude);
        }

        /// <summary>
        ///     Stop current bumpy-road effect
        /// </summary>
        /// <returns></returns>
        public bool StopBumpyRoadEffect()
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiStopBumpyRoadEffect(Index);
        }

        /// <summary>
        /// </summary>
        /// <param name="magnitude">
        ///     Specifies the magnitude of the slippery road effect.
        ///     Valid ranges for magnitudePercentage are 0 to 100.
        ///     100 corresponds to the most slippery effect.
        /// </param>
        /// <returns></returns>
        public bool PlaySlipperyRoadEffect(int magnitude)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiPlaySideCollisionForce(Index, magnitude);
        }

        /// <summary>
        ///     Stop current slippery-road effect
        /// </summary>
        /// <returns></returns>
        public bool StopSlipperyRoadEffect()
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiStopSlipperyRoadEffect(Index);
        }

        public enum SurfaceType
        {
            Sine = 0,
            Square = 1,
            Triangle = 2
        }

        /// <summary>
        /// </summary>
        /// <param name="type">Specifies the type of force effect.</param>
        /// <param name="magnitude">
        ///     Specifies the magnitude of the surface effect.
        ///     Valid ranges for magnitudePercentage are 0 to 100.
        ///     Values higher than 100 are silently clamped.
        /// </param>
        /// <param name="period">
        ///     Specifies the period of the periodic force effect.
        ///     The value is the duration for one full cycle of the periodic function measured in milliseconds.
        ///     A good range of values for the period is 20 ms (sand) to 120 ms (wooden bridge or cobblestones).
        ///     For a surface effect the period should not be any bigger than 150 ms.
        /// </param>
        /// <returns></returns>
        public bool PlaySurfaceEffect(SurfaceType type, int magnitude, int period)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiPlaySurfaceEffect(Index, (int)type, magnitude, period);
        }

        /// <summary>
        ///     Stop current surface effect
        /// </summary>
        /// <returns></returns>
        public bool StopSurfaceEffect()
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiStopSurfaceEffect(Index);
        }

        public bool PlayCarAirborne()
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiPlayCarAirborne(Index);
        }

        /// <summary>
        ///     Stop current airborne effect
        /// </summary>
        /// <returns></returns>
        public bool StopCarAirborne()
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiStopCarAirborne(Index);
        }

        /// <summary>
        /// </summary>
        /// <param name="usableRange">Specifies the deadband in percentage of the softstop force effect.</param>
        /// <returns></returns>
        public bool PlaySoftstopForce(int usableRange)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiPlaySoftstopForce(Index, usableRange);
        }

        /// <summary>
        ///     Stop current soft-stop force
        /// </summary>
        /// <returns></returns>
        public bool StopSoftstopForce()
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiStopSoftstopForce(Index);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Set current Logitech Controller's settings
        /// </summary>
        /// <param name="propertiesData"></param>
        /// <returns></returns>
        [Obsolete(".DLL does not support this method")]
        public bool SetControllerProperties(LogitechGsdk.LogiControllerPropertiesData propertiesData)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiSetPreferredControllerProperties(propertiesData);
        }

        /// <summary>
        ///     Get current Logitech Controller's settings
        /// </summary>
        /// <param name="propertiesData"></param>
        /// <returns></returns>
        [Obsolete(".DLL does not support this method")]
        public bool GetControllerProperties(ref LogitechGsdk.LogiControllerPropertiesData propertiesData)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiGetCurrentControllerProperties(Index, ref propertiesData);
        }

        public enum ShiftType
        {
            Unknown,
            Sequential,
            Gated
        }

        public ShiftType GetShiftType()
        {
            if (!IsConnected)
            {
                return ShiftType.Unknown;
            }

            var value = LogitechGsdk.LogiGetShifterMode(Index);

            return value switch
            {
                -1 => ShiftType.Unknown,
                0 => ShiftType.Sequential,
                1 => ShiftType.Gated,
                _ => ShiftType.Unknown
            };
        }

        public string GetDisplayName()
        {
            if (!IsConnected)
            {
                return string.Empty;
            }

            var deviceName = new StringBuilder(256);
            LogitechGsdk.LogiGetFriendlyProductName(Index, deviceName, 256);
            return deviceName.ToString();
        }

        /// <summary>
        ///     Set current steering wheel range. Prefer (900° degrees)
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public bool SetOperatingRange(int range)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiSetOperatingRange(Index, range);
        }

        /// <summary>
        ///     Get current steering wheel range.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public bool GetOperatingRange(ref int range)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiGetOperatingRange(Index, ref range);
        }

        /// <summary>
        ///     Set current LEDS with RPM parameters
        /// </summary>
        /// <param name="currentRpm"></param>
        /// <param name="rpmMin"></param>
        /// <param name="rpmMax"></param>
        public bool PlayLeds(float currentRpm, float rpmMin, float rpmMax)
        {
            if (!IsConnected)
            {
                return false;
            }

            return LogitechGsdk.LogiPlayLeds(Index, currentRpm, rpmMin, rpmMax);
        }

        #endregion

#if UNITY_EDITOR

        private const string DEBUG_PATH = "Tools/Inputter/Enable Debug Mode";

        [MenuItem(DEBUG_PATH)]
        private static void EnableDebugMode()
        {
            var value = !EditorPrefs.GetBool(DEBUG_PATH);
            EditorPrefs.SetBool(DEBUG_PATH, value);
            Menu.SetChecked(DEBUG_PATH, value);
        }

        static LogitechG29()
        {
            Init();
        }
#endif
    }
}