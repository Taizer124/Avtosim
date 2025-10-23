using UnityEngine;

namespace Assets.VehicleController
{
    [HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/ai-racers-setup")]
    public class AIVehicleInputProvider : MonoBehaviour, IVehicleControllerInputProvider
    {
        private float _brakeInput;
        private float _gasInput;
        private float _steerInput;
        private bool _enabled = true;
        public void SetInput(float brakeInput, float gasInput, float steerInput)
        {
            if(!_enabled)
            {
                if (gasInput > 0)
                    brakeInput = 1;
                else
                    brakeInput = 0;
                steerInput = 0;
            }
            _brakeInput = brakeInput;
            _gasInput = gasInput;
            _steerInput = steerInput;
        }

        public float GetBrakeInput() => _brakeInput;

        public float GetGasInput() => _gasInput;

        public bool GetGearDownInput() => false;

        public bool GetGearUpInput() => false;

        public bool GetHandbrakeInput() => false;

        public float GetHorizontalInput() => _steerInput;

        public bool GetNitroBoostInput() => false;

        public float GetPitchInput() => 0;

        public float GetRollInput() => 0;

        public float GetYawInput() => 0;

        public void EnableInput(bool enable)
        {
            _enabled = enable;
        }
    }
}
