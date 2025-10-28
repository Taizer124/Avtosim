using UnityEngine;

namespace Assets.VehicleController
{

    public class Handling : IHandling
    {
        private VehicleAxle[] _steerAxle;
        private float _steerAxlesAmount;

        private float _targetAngle = 0;

        public void Initialize(VehicleAxle[] steerAxles)
        {
            _steerAxle = steerAxles;
            _steerAxlesAmount = _steerAxle.Length;
        }

        public void SteerWheels(float input, float maxSteerAngle, float steerSpeed, float returnSpeed)
        {
            _targetAngle = input * maxSteerAngle;

            float t = 0;
            if(_targetAngle == 0)
            {
                if(returnSpeed > 0)
                    t = Time.deltaTime / returnSpeed;
                else
                    t = 0;
            }
            else
            {
                if(steerSpeed > 0)
                    t = Time.deltaTime / steerSpeed;
                else
                    t = 0;
            }
            
            float angle = Mathf.LerpAngle(_steerAxle[0].LeftHalfShaft.WheelController.SteerAngle, _targetAngle, t);

            for (int i = 0; i < _steerAxlesAmount; i++)
            {
                _steerAxle[i].ApplySteering(angle);
            }
        }
    }
}
