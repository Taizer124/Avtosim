#if INPUT_SYSTEM_INSTALLED
using UnityEngine;
namespace Assets.VehicleController
{
    [HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/vehicle-controller-input-provider")]
    public class VehicleInputProviderDemo : MonoBehaviour, IVehicleControllerInputProvider
    {
        private PlayerVehicleInputActions _inputActions;

        public bool ForceGasInputDuringNitrous = false;

        private float _gasInput;
        private float _brakeInput;
        private bool _handbrakeInput;
        private float _horizontalInput;

        private bool _nitroBoostInput;

        private float _pitchInput;
        private float _yawInput;
        private float _rollInput;

        private bool _shiftedUp;
        private bool _shiftedDown;

        private bool _enabled = true;

        private void Awake()
        {
            _inputActions = new PlayerVehicleInputActions();
            _inputActions.Enable();
        }

        private void Update()
        {
            _gasInput = _inputActions.Vehicle.GasInput.ReadValue<float>();

            if (!_enabled)
            {
                if (_gasInput > 0)
                    _brakeInput = 1;
                else
                    _brakeInput = 0;

                _horizontalInput = 0;

                _handbrakeInput = _nitroBoostInput = false;

                _pitchInput = _yawInput = _rollInput = 0;

                _shiftedUp = _shiftedDown = false;
                return;
            }

            _gasInput = _inputActions.Vehicle.GasInput.ReadValue<float>();
            _brakeInput = _inputActions.Vehicle.BrakeInput.ReadValue<float>();
            _horizontalInput = _inputActions.Vehicle.HorizontalInput.ReadValue<float>();

            _handbrakeInput = _inputActions.Vehicle.HandbrakeInput.ReadValue<float>() != 0;
            _nitroBoostInput = _inputActions.Vehicle.NitroBoostInput.ReadValue<float>() != 0;

            if (ForceGasInputDuringNitrous && _nitroBoostInput && _gasInput == 0)
                _gasInput = 1;
                

            _pitchInput = _inputActions.Vehicle.PitchInput.ReadValue<float>();
            _yawInput = _inputActions.Vehicle.YawInput.ReadValue<float>();
            _rollInput = _inputActions.Vehicle.RollInput.ReadValue<float>();

            _shiftedUp = _inputActions.Vehicle.GearUpInput.WasPerformedThisFrame();
            _shiftedDown = _inputActions.Vehicle.GearDownInput.WasPerformedThisFrame();
        }

        //brake input from 0 to 1
        public float GetBrakeInput() => _brakeInput;
        //gas input from 0 to 1
        public float GetGasInput() => _gasInput;
        //player shifted down this frame
        public bool GetGearDownInput() => _shiftedDown;
        //player shifted up this frame
        public bool GetGearUpInput() => _shiftedUp;
        //player is holding handbrake button
        public bool GetHandbrakeInput() => _handbrakeInput;
        //steer input from -1 to 1
        public float GetHorizontalInput() => _horizontalInput;
        //player is holding boost button
        public bool GetNitroBoostInput() => _nitroBoostInput;
        //pitch input from -1 to 1
        public float GetPitchInput() => _pitchInput;
        //roll input from -1 to 1
        public float GetRollInput() => _rollInput;
        //yaw input from -1 to 1
        public float GetYawInput() => _yawInput;

        public void EnableInput(bool enable)
        {
            _enabled = enable;
        }
    }
}

#endif
