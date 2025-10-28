using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Physics/Tire Controller")]
    public class TireController
    {
        private Transform _transform;
        private Transform _visualTransform;

        private float _handbrakeGripMultiplier = 1;
        private float _lockedGripMultiplier = 1;

        private float _staticTireLoad;
        public float StaticTireLoad
        {
            get { return _staticTireLoad; }
        }

        private float _wheelBaseLen;

        private float _forwardSlip = 0;
        public float ForwardSlip
        {
            get { return _forwardSlip; }
        }

        private float _sidewaysSlip;
        public float SidewaysSlip
        {
            get { return _sidewaysSlip; }
        }

        private float _visualSideSlip;
        public float VisualSideSlip
        {
            get => _visualSideSlip;
        }

        private float _sidewaysDot;
        public float SidewaysDot
        {
            get { return _sidewaysDot; }
        }

        private float _returnGripTime = 2;

        private VehiclePartsSetWrapper _partsPresetWrapper;

        private bool _isFrontTire;
        public bool IsFrontTire => _isFrontTire;
        private Rigidbody _rb;

        private float _currentLoad;

        private float _minLoad;
        private float _maxLoad;

        public TireController(Transform transform, Transform visualTransform, VehiclePartsSetWrapper partsPresetWrapper, bool front, float axelLen,
            float wheelBaseLen, Rigidbody rb)
        {
            _transform = transform;
            _visualTransform = visualTransform;
            _isFrontTire = front;
            _partsPresetWrapper = partsPresetWrapper;
            _staticTireLoad = (axelLen / wheelBaseLen) * partsPresetWrapper.Body.Mass * 9.81f;
            _minLoad = _staticTireLoad - _staticTireLoad / 2;
            _maxLoad = _staticTireLoad * 3;
            _wheelBaseLen = wheelBaseLen;
            _rb = rb;
        }

        private float CalculateTireLoad(float accel, float distanceToGround)
        {
            if (_isFrontTire)
                return CalculateFrontTireLoad(accel, distanceToGround);
            else
                return CalculateRearTireLoad(accel, distanceToGround);
        }
        public float CalculateFrontTireLoad(float accel, float distanceToGround)
        {
            float weightTransfer = (distanceToGround / _wheelBaseLen) * _partsPresetWrapper.Body.Mass * accel / 2;
            float totalLoad = _staticTireLoad - weightTransfer;
            _currentLoad = (_staticTireLoad - weightTransfer) / 3000;
            return Mathf.Clamp(totalLoad, _minLoad, _maxLoad);
        }
        public float CalculateRearTireLoad(float accel, float distanceToGround)
        {
            float weightTransfer = (distanceToGround / _wheelBaseLen) * _partsPresetWrapper.Body.Mass * accel / 2;
            float totalLoad = _staticTireLoad + weightTransfer;
            _currentLoad = (_staticTireLoad + weightTransfer) / 3000;
            return Mathf.Clamp(totalLoad, _minLoad, _maxLoad);
        }

        public void CalculateForwardSlip(float accelForce, float wheelLoad, float speed)
        {
            accelForce = Mathf.Abs(accelForce);
            speed = Mathf.Abs(speed);

            float relativeTorque = accelForce / _rb.mass;

            if (_isFrontTire)
                CalculateTireForwardSlip(accelForce, relativeTorque, wheelLoad, speed, _partsPresetWrapper.FrontTires.ForwardGrip);
            else
                CalculateTireForwardSlip(accelForce, relativeTorque, wheelLoad, speed, _partsPresetWrapper.RearTires.ForwardGrip);
        }

        public float GetMaxWheelLoad(float accel, float distanceToGround)
        {
            float wheelLoad = CalculateTireLoad(accel, distanceToGround);
            float maxGrip = wheelLoad
                * (_isFrontTire ? _partsPresetWrapper.FrontTires.ForwardGrip : _partsPresetWrapper.RearTires.ForwardGrip) 
                * _lockedGripMultiplier;

            return maxGrip;
        }

        private void CalculateTireForwardSlip(float accelForce, float relativeTorque, float maxLoad, float speed, float tireGrip)
        {
            if (speed < 1)
                speed = 1;

            float burnOutSpeed = speed * tireGrip * _lockedGripMultiplier;
            if (relativeTorque > burnOutSpeed)
            {
                _forwardSlip = relativeTorque / burnOutSpeed;
                return;
            }

            if (accelForce < maxLoad)
                _forwardSlip = 0;
            else
                _forwardSlip = accelForce / maxLoad;
        }

        public Vector3 CalculateSidewaysForce(bool accelerating, float speed, float speedPercent)
        {
            Vector3 steeringDir = _transform.right;
            Vector3 tireWorldVel = _rb.GetPointVelocity(_transform.position);

            float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);

            _sidewaysDot = Vector3.Dot(_rb.linearVelocity.normalized, steeringDir);

            if (_isFrontTire)
                return CalculateTiresSidewaysForce(accelerating, speed, speedPercent, steeringVel, steeringDir,
                                                    _partsPresetWrapper.FrontTires.SteeringStiffness,
                                                    _partsPresetWrapper.FrontTires.SidewaysGripCurve,
                                                    _partsPresetWrapper.FrontTires.SidewaysSlipCurve);
            else
                return CalculateTiresSidewaysForce(accelerating, speed, speedPercent, steeringVel, steeringDir,
                                                    _partsPresetWrapper.RearTires.SteeringStiffness,
                                                    _partsPresetWrapper.RearTires.SidewaysGripCurve,
                                                    _partsPresetWrapper.RearTires.SidewaysSlipCurve);
        }

        private Vector3 CalculateTiresSidewaysForce(bool accelerating, float speed, float speedPercent, float steeringVel, Vector3 steeringDir,
            float tireCorneringStiffnessMax, AnimationCurve gripCurve, AnimationCurve slipCurve)
        {
            if (Mathf.Abs(speed) < 5)
            {
                _sidewaysSlip = 0;
                _visualSideSlip = 0;
            }
            else
            {
                _sidewaysSlip = 1 - slipCurve.Evaluate(Mathf.Abs(_sidewaysDot));

                float dif =  1 - Mathf.Abs(Vector3.Dot(_visualTransform.right, _rb.transform.right));
                _visualSideSlip = Mathf.Abs(_sidewaysDot) - dif;
            }

            float tireCorneringStiffness = tireCorneringStiffnessMax * gripCurve.Evaluate(speedPercent);

            float desiredVelocityChange = -steeringVel * (1 - _sidewaysSlip) * (accelerating ? _handbrakeGripMultiplier : 1);

            float desiredAccel = desiredVelocityChange / 0.02f;

            return tireCorneringStiffness * desiredAccel * steeringDir;
        }

        public void ApplyHandbrake(float force, float gasInput, float tractionPercent)
        {
            if (force != 0)
                _handbrakeGripMultiplier = Mathf.Clamp(_handbrakeGripMultiplier - Time.deltaTime * 2 * (gasInput + 1), tractionPercent, 1);
            else
                _handbrakeGripMultiplier = Mathf.Clamp(_handbrakeGripMultiplier + Time.deltaTime / (_returnGripTime / _partsPresetWrapper.RearTires.ForwardGrip) * (1 - _sidewaysSlip), tractionPercent, 1);
        }

        public void DecreaseFriction(bool locked, float effectStrength)
        {
            if (locked)
                _lockedGripMultiplier = Mathf.Clamp(_lockedGripMultiplier - Time.deltaTime * effectStrength, 0.1f, 1);
            else
                _lockedGripMultiplier = Mathf.Clamp(_lockedGripMultiplier + Time.deltaTime / _returnGripTime * (1 - _sidewaysSlip), 0.1f, 1);
        }
    }
}

