#region

using System;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace LogitechG29.Sample.Input
{
    [CreateAssetMenu(fileName = "InputControllerReader", menuName = "Input/InputControllerLogitechG29Reader")]
    public class InputControllerReader : ScriptableObject,
        InputController.IButtonsActions,
        InputController.IHandbrakeActions,
        InputController.IPedalsActions,
        InputController.ISteeringwheelActions,
        InputController.ITransmissionActions
    {
        #region Unity

        private InputController _input;

        private void OnEnable()
        {
            _input = new InputController();
            _input.Buttons.SetCallbacks(this);
            _input.Handbrake.SetCallbacks(this);
            _input.Pedals.SetCallbacks(this);
            _input.Steeringwheel.SetCallbacks(this);
            _input.Transmission.SetCallbacks(this);

            _input.Buttons.Enable();
            _input.Handbrake.Enable();
            _input.Pedals.Enable();
            _input.Steeringwheel.Enable();
            _input.Transmission.Enable();

            RegisterDebugOnActions();
        }

        private void OnDisable()
        {
            _input.Buttons.Disable();
            _input.Handbrake.Disable();
            _input.Pedals.Disable();
            _input.Steeringwheel.Disable();
            _input.Transmission.Disable();

            _input.Buttons.SetCallbacks(null);
            _input.Handbrake.SetCallbacks(null);
            _input.Pedals.SetCallbacks(null);
            _input.Steeringwheel.SetCallbacks(null);
            _input.Transmission.SetCallbacks(null);

            UnRegisterDebugOnActions();
        }

        #endregion


        #region Properties

        public bool NorthButton { get; private set; }
        public bool SouthButton { get; private set; }
        public bool EastButton { get; private set; }
        public bool WestButton { get; private set; }
        public bool RightBumper { get; private set; }
        public bool LeftBumper { get; private set; }
        public bool RightShift { get; private set; }
        public bool LeftShift { get; private set; }
        public bool Share { get; private set; }
        public bool Options { get; private set; }
        public bool Home { get; private set; }
        public bool RightStickButton { get; private set; }
        public bool LeftStickButton { get; private set; }
        public bool Plus { get; private set; }
        public bool Minus { get; private set; }
        public bool RightTurn { get; private set; }
        public bool LeftTurn { get; private set; }
        public bool Return { get; private set; }

        public Vector2 HatSwitch { get; private set; }

        public bool Shifter1 { get; private set; }
        public bool Shifter2 { get; private set; }
        public bool Shifter3 { get; private set; }
        public bool Shifter4 { get; private set; }
        public bool Shifter5 { get; private set; }
        public bool Shifter6 { get; private set; }
        public bool Shifter7 { get; private set; }

        public float Steering { get; private set; }
        public float Throttle { get; private set; }
        public float Brake { get; private set; }
        public float Clutch { get; private set; }

        public float Handbrake { get; private set; }

        #endregion


        #region Events

        public event Action<bool> OnNorthButtonCallback;
        public event Action<bool> OnSouthButtonCallback;
        public event Action<bool> OnEastButtonCallback;
        public event Action<bool> OnWestButtonCallback;
        public event Action<bool> OnRightBumperCallback;
        public event Action<bool> OnLeftBumperCallback;
        public event Action<bool> OnRightShiftCallback;
        public event Action<bool> OnLeftShiftCallback;
        public event Action<bool> OnShareCallback;
        public event Action<bool> OnOptionsCallback;
        public event Action<bool> OnHomeCallback;
        public event Action<bool> OnRightStickButtonCallback;
        public event Action<bool> OnLeftStickButtonCallback;
        public event Action<bool> OnPlusCallback;
        public event Action<bool> OnMinusCallback;
        public event Action<bool> OnRightTurnCallback;
        public event Action<bool> OnLeftTurnCallback;
        public event Action<bool> OnReturnCallback;

        public event Action<Vector2> HatSwitchCallback;

        public event Action<bool> Shifter1Callback;
        public event Action<bool> Shifter2Callback;
        public event Action<bool> Shifter3Callback;
        public event Action<bool> Shifter4Callback;
        public event Action<bool> Shifter5Callback;
        public event Action<bool> Shifter6Callback;
        public event Action<bool> Shifter7Callback;

        public event Action<float> SteeringCallback;
        public event Action<float> ThrottleCallback;
        public event Action<float> BrakeCallback;
        public event Action<float> ClutchCallback;

        public event Action<float> HandbrakeCallback;

        #endregion


        #region Debug
        
        [SerializeField] private bool isDebugModeOnStartup;

        public bool GetDebugMode() => isDebugModeOnStartup;
        public void SetDebugMode(bool value) => isDebugModeOnStartup = value;

        private void RegisterDebugOnActions()
        {
            OnNorthButtonCallback += value => PrintDebug($"{nameof(OnNorthButtonCallback)}: {value}");
            OnSouthButtonCallback += value => PrintDebug($"{nameof(OnSouthButtonCallback)}: {value}");
            OnEastButtonCallback += value => PrintDebug($"{nameof(OnEastButtonCallback)}: {value}");
            OnWestButtonCallback += value => PrintDebug($"{nameof(OnWestButtonCallback)}: {value}");
            OnRightBumperCallback += value => PrintDebug($"{nameof(OnRightBumperCallback)}: {value}");
            OnLeftBumperCallback += value => PrintDebug($"{nameof(OnLeftBumperCallback)}: {value}");
            OnRightShiftCallback += value => PrintDebug($"{nameof(OnRightShiftCallback)}: {value}");
            OnLeftShiftCallback += value => PrintDebug($"{nameof(OnLeftShiftCallback)}: {value}");
            OnShareCallback += value => PrintDebug($"{nameof(OnShareCallback)}: {value}");
            OnOptionsCallback += value => PrintDebug($"{nameof(OnOptionsCallback)}: {value}");
            OnHomeCallback += value => PrintDebug($"{nameof(OnHomeCallback)}: {value}");
            OnRightStickButtonCallback += value => PrintDebug($"{nameof(OnRightStickButtonCallback)}: {value}");
            OnLeftStickButtonCallback += value => PrintDebug($"{nameof(OnLeftStickButtonCallback)}: {value}");
            OnPlusCallback += value => PrintDebug($"{nameof(OnPlusCallback)}: {value}");
            OnMinusCallback += value => PrintDebug($"{nameof(OnMinusCallback)}: {value}");
            OnRightTurnCallback += value => PrintDebug($"{nameof(OnRightTurnCallback)}: {value}");
            OnLeftTurnCallback += value => PrintDebug($"{nameof(OnLeftTurnCallback)}: {value}");
            OnReturnCallback += value => PrintDebug($"{nameof(OnReturnCallback)}: {value}");
            HatSwitchCallback += value => PrintDebug($"{nameof(HatSwitchCallback)}: {value}");
            Shifter1Callback += value => PrintDebug($"{nameof(Shifter1Callback)}: {value}");
            Shifter2Callback += value => PrintDebug($"{nameof(Shifter2Callback)}: {value}");
            Shifter3Callback += value => PrintDebug($"{nameof(Shifter3Callback)}: {value}");
            Shifter4Callback += value => PrintDebug($"{nameof(Shifter4Callback)}: {value}");
            Shifter5Callback += value => PrintDebug($"{nameof(Shifter5Callback)}: {value}");
            Shifter6Callback += value => PrintDebug($"{nameof(Shifter6Callback)}: {value}");
            Shifter7Callback += value => PrintDebug($"{nameof(Shifter7Callback)}: {value}");
            SteeringCallback += value => PrintDebug($"{nameof(SteeringCallback)}: {value}");
            ThrottleCallback += value => PrintDebug($"{nameof(ThrottleCallback)}: {value}");
            BrakeCallback += value => PrintDebug($"{nameof(BrakeCallback)}: {value}");
            ClutchCallback += value => PrintDebug($"{nameof(ClutchCallback)}: {value}");
            HandbrakeCallback += value => PrintDebug($"{nameof(HandbrakeCallback)}: {value}");
        }

        private void UnRegisterDebugOnActions()
        {
            OnNorthButtonCallback -= value => PrintDebug($"{nameof(OnNorthButtonCallback)}: {value}");
            OnSouthButtonCallback -= value => PrintDebug($"{nameof(OnSouthButtonCallback)}: {value}");
            OnEastButtonCallback -= value => PrintDebug($"{nameof(OnEastButtonCallback)}: {value}");
            OnWestButtonCallback -= value => PrintDebug($"{nameof(OnWestButtonCallback)}: {value}");
            OnRightBumperCallback -= value => PrintDebug($"{nameof(OnRightBumperCallback)}: {value}");
            OnLeftBumperCallback -= value => PrintDebug($"{nameof(OnLeftBumperCallback)}: {value}");
            OnRightShiftCallback -= value => PrintDebug($"{nameof(OnRightShiftCallback)}: {value}");
            OnLeftShiftCallback -= value => PrintDebug($"{nameof(OnLeftShiftCallback)}: {value}");
            OnShareCallback -= value => PrintDebug($"{nameof(OnShareCallback)}: {value}");
            OnOptionsCallback -= value => PrintDebug($"{nameof(OnOptionsCallback)}: {value}");
            OnHomeCallback -= value => PrintDebug($"{nameof(OnHomeCallback)}: {value}");
            OnRightStickButtonCallback -= value => PrintDebug($"{nameof(OnRightStickButtonCallback)}: {value}");
            OnLeftStickButtonCallback -= value => PrintDebug($"{nameof(OnLeftStickButtonCallback)}: {value}");
            OnPlusCallback -= value => PrintDebug($"{nameof(OnPlusCallback)}: {value}");
            OnMinusCallback -= value => PrintDebug($"{nameof(OnMinusCallback)}: {value}");
            OnRightTurnCallback -= value => PrintDebug($"{nameof(OnRightTurnCallback)}: {value}");
            OnLeftTurnCallback -= value => PrintDebug($"{nameof(OnLeftTurnCallback)}: {value}");
            OnReturnCallback -= value => PrintDebug($"{nameof(OnReturnCallback)}: {value}");
            HatSwitchCallback -= value => PrintDebug($"{nameof(HatSwitchCallback)}: {value}");
            Shifter1Callback -= value => PrintDebug($"{nameof(Shifter1Callback)}: {value}");
            Shifter2Callback -= value => PrintDebug($"{nameof(Shifter2Callback)}: {value}");
            Shifter3Callback -= value => PrintDebug($"{nameof(Shifter3Callback)}: {value}");
            Shifter4Callback -= value => PrintDebug($"{nameof(Shifter4Callback)}: {value}");
            Shifter5Callback -= value => PrintDebug($"{nameof(Shifter5Callback)}: {value}");
            Shifter6Callback -= value => PrintDebug($"{nameof(Shifter6Callback)}: {value}");
            Shifter7Callback -= value => PrintDebug($"{nameof(Shifter7Callback)}: {value}");
            SteeringCallback -= value => PrintDebug($"{nameof(SteeringCallback)}: {value}");
            ThrottleCallback -= value => PrintDebug($"{nameof(ThrottleCallback)}: {value}");
            BrakeCallback -= value => PrintDebug($"{nameof(BrakeCallback)}: {value}");
            ClutchCallback -= value => PrintDebug($"{nameof(ClutchCallback)}: {value}");
            HandbrakeCallback -= value => PrintDebug($"{nameof(HandbrakeCallback)}: {value}");
        }

        private void PrintDebug(string value)
        {
            if (isDebugModeOnStartup) Debug.Log($"{value}");
        }

        #endregion


        #region Input Actions

        public void OnDpad(InputAction.CallbackContext context)
        {
            HatSwitch = context.ReadValue<Vector2>();
            HatSwitchCallback?.Invoke(HatSwitch);
        }

        public void OnHome(InputAction.CallbackContext context)
        {
            Home = context.ReadValueAsButton();
            OnHomeCallback?.Invoke(Home);
        }

        public void OnEast(InputAction.CallbackContext context)
        {
            EastButton = context.ReadValueAsButton();
            OnEastButtonCallback?.Invoke(EastButton);
        }

        public void OnLeftBumper(InputAction.CallbackContext context)
        {
            LeftBumper = context.ReadValueAsButton();
            OnLeftBumperCallback?.Invoke(LeftBumper);
        }

        public void OnRightBumper(InputAction.CallbackContext context)
        {
            RightBumper = context.ReadValueAsButton();
            OnRightBumperCallback?.Invoke(RightBumper);
        }

        public void OnLeftShift(InputAction.CallbackContext context)
        {
            LeftShift = context.ReadValueAsButton();
            OnLeftShiftCallback?.Invoke(LeftShift);
        }

        public void OnRightStick(InputAction.CallbackContext context)
        {
            RightStickButton = context.ReadValueAsButton();
            OnRightStickButtonCallback?.Invoke(RightStickButton);
        }

        public void OnLeftStick(InputAction.CallbackContext context)
        {
            LeftStickButton = context.ReadValueAsButton();
            OnLeftStickButtonCallback?.Invoke(LeftStickButton);
        }

        public void OnLeftTurn(InputAction.CallbackContext context)
        {
            LeftTurn = context.ReadValueAsButton();
            OnLeftTurnCallback?.Invoke(LeftTurn);
        }

        public void OnRightTurn(InputAction.CallbackContext context)
        {
            RightTurn = context.ReadValueAsButton();
            OnRightTurnCallback?.Invoke(RightTurn);
        }

        public void OnRightShift(InputAction.CallbackContext context)
        {
            RightShift = context.ReadValueAsButton();
            OnRightShiftCallback?.Invoke(RightShift);
        }

        public void OnMinus(InputAction.CallbackContext context)
        {
            Minus = context.ReadValueAsButton();
            OnMinusCallback?.Invoke(Minus);
        }

        public void OnNorth(InputAction.CallbackContext context)
        {
            NorthButton = context.ReadValueAsButton();
            OnNorthButtonCallback?.Invoke(NorthButton);
        }

        public void OnOptions(InputAction.CallbackContext context)
        {
            Options = context.ReadValueAsButton();
            OnOptionsCallback?.Invoke(Options);
        }

        public void OnPlus(InputAction.CallbackContext context)
        {
            Plus = context.ReadValueAsButton();
            OnPlusCallback?.Invoke(Plus);
        }

        public void OnReturn(InputAction.CallbackContext context)
        {
            Return = context.ReadValueAsButton();
            OnReturnCallback?.Invoke(Return);
        }

        public void OnShare(InputAction.CallbackContext context)
        {
            Share = context.ReadValueAsButton();
            OnShareCallback?.Invoke(Share);
        }

        public void OnShifter1(InputAction.CallbackContext context)
        {
            Shifter1 = context.ReadValueAsButton();
            Shifter1Callback?.Invoke(Shifter1);
        }

        public void OnShifter2(InputAction.CallbackContext context)
        {
            Shifter2 = context.ReadValueAsButton();
            Shifter2Callback?.Invoke(Shifter2);
        }

        public void OnShifter3(InputAction.CallbackContext context)
        {
            Shifter3 = context.ReadValueAsButton();
            Shifter3Callback?.Invoke(Shifter3);
        }

        public void OnShifter4(InputAction.CallbackContext context)
        {
            Shifter4 = context.ReadValueAsButton();
            Shifter4Callback?.Invoke(Shifter4);
        }

        public void OnShifter5(InputAction.CallbackContext context)
        {
            Shifter5 = context.ReadValueAsButton();
            Shifter5Callback?.Invoke(Shifter5);
        }

        public void OnShifter6(InputAction.CallbackContext context)
        {
            Shifter6 = context.ReadValueAsButton();
            Shifter6Callback?.Invoke(Shifter6);
        }

        public void OnShifter7(InputAction.CallbackContext context)
        {
            Shifter7 = context.ReadValueAsButton();
            Shifter7Callback?.Invoke(Shifter7);
        }

        public void OnSouth(InputAction.CallbackContext context)
        {
            SouthButton = context.ReadValueAsButton();
            OnSouthButtonCallback?.Invoke(SouthButton);
        }

        public void OnWest(InputAction.CallbackContext context)
        {
            WestButton = context.ReadValueAsButton();
            OnWestButtonCallback?.Invoke(WestButton);
        }

        public void OnThrottle(InputAction.CallbackContext context)
        {
            Throttle = context.ReadValue<float>();
            ThrottleCallback?.Invoke(Throttle);
        }

        public void OnClutch(InputAction.CallbackContext context)
        {
            Clutch = context.ReadValue<float>();
            ClutchCallback?.Invoke(Clutch);
        }

        public void OnBrake(InputAction.CallbackContext context)
        {
            Brake = context.ReadValue<float>();
            BrakeCallback?.Invoke(Brake);
        }

        public void OnSteering_Steering(InputAction.CallbackContext context)
        {
            Steering = context.ReadValue<float>();
            SteeringCallback?.Invoke(Steering);
        }

        public void OnSteering_Stick(InputAction.CallbackContext context)
        {
            Steering = context.ReadValue<float>();
            SteeringCallback?.Invoke(Steering);
        }


        public void OnHandbrake(InputAction.CallbackContext context)
        {
            Handbrake = context.ReadValue<float>();
            HandbrakeCallback?.Invoke(Handbrake);
        }

        #endregion
    }
}