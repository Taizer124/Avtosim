using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Input/Vehicle Controller Input Provider"),
    HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/vehicle-controller-input-provider")]
    public class VehicleControllerInputProvider : MonoBehaviour, IVehicleControllerInputProvider
    {
        #region Control field
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
        #endregion

        private bool _enabled = true;

        private void Update()
        {
            if(!_enabled)
            {
                _gasInput = Input.GetKey("joystick button 1") || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
                if (_gasInput > 0)
                    _brakeInput = 1;
                else
                    _brakeInput = 0;
                _horizontalInput = 0;

                _handbrakeInput = _nitroBoostInput = false;
                _pitchInput = _yawInput = _rollInput = 0;
                return;
            }

            _gasInput = Input.GetKey("joystick button 1") || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0;

            _brakeInput = Input.GetKey("joystick button 0") || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0;

            _handbrakeInput = Input.GetKey(KeyCode.Space) || Input.GetKey("joystick button 2");

            _horizontalInput = Input.GetAxis("Horizontal");
            
            _nitroBoostInput = Input.GetKey(KeyCode.N) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey("joystick button 3");

            _pitchInput = Input.GetAxis("Vertical");

            float yawLeftInput = Input.GetKey("joystick button 4") || Input.GetKey(KeyCode.Q) ? 1 : 0;
            float yawRightInput = Input.GetKey("joystick button 5") || Input.GetKey(KeyCode.E) ? 1 : 0;
            _yawInput = yawRightInput - yawLeftInput;

            _rollInput = Input.GetAxis("Horizontal");

            _shiftedUp = Input.GetKeyDown(KeyCode.LeftShift);
            _shiftedDown = Input.GetKeyDown(KeyCode.LeftControl);
        }

        public void EnableInput(bool enable)
        {
            _enabled = enable;
        }

        public float GetGasInput() => _gasInput;

        public float GetBrakeInput() => _brakeInput;

        public bool GetHandbrakeInput() => _handbrakeInput;

        public float GetHorizontalInput() => _horizontalInput;

        public bool GetGearUpInput() => _shiftedUp;

        public bool GetGearDownInput() => _shiftedDown;

        public float GetPitchInput() => _pitchInput;

        public float GetYawInput() => _yawInput;

        public float GetRollInput() => _rollInput;

        public bool GetNitroBoostInput() => _nitroBoostInput;


    }
}

