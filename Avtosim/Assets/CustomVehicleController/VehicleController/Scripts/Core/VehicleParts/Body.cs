using UnityEngine;

namespace Assets.VehicleController
{

    public class Body : IBody
    {
        private Rigidbody _rb;
        private Transform _transform;
        private Vector3 _cachedCOMLocalPosition;

        private VehiclePartsSetWrapper _partsPresetWrapper;
        private CurrentCarStats _currentCarStats;


        private const float MAX_SENSITIVITY_AFTER_AERIAL_TIME = 5f;

        private float _handbrakeEffect = 0;
        private bool _recovering = false;

        public void Initialize(Rigidbody rb, VehiclePartsSetWrapper partsPresetWrapper, CurrentCarStats currentCarStats, Transform transform, Transform centerOfMass)
        {
            _rb = rb;
            _rb.centerOfMass = centerOfMass.localPosition;
            _partsPresetWrapper = partsPresetWrapper;
            _currentCarStats = currentCarStats;

            _cachedCOMLocalPosition = centerOfMass.localPosition;

            this._transform = transform;
        }

        public void AddDownforce()
        {
            _rb.mass = _partsPresetWrapper.Body.Mass;
            _rb.AddForce(_partsPresetWrapper.Body.Downforce * Mathf.Abs(_currentCarStats.SpeedInMsPerS) * -Vector3.up);
        }

        public void AddCorneringForce()
        {
            _handbrakeEffect = _currentCarStats.HandbrakePulled ? _partsPresetWrapper.Brakes.HandbrakeTractionPercent : 1;
            //lower effect at higher speed
            _handbrakeEffect += _currentCarStats.SpeedPercent * (1 - _handbrakeEffect);
            //and low speed when vehicle is moving perpendicularly
            _handbrakeEffect += (1 - _currentCarStats.SpeedPercent) * Mathf.Abs(Vector3.Dot(_transform.right, _rb.linearVelocity.normalized));
            _handbrakeEffect = Mathf.Clamp01(_handbrakeEffect);

            //car control becoming stiffer and higher speed effect
            _rb.angularDamping = _partsPresetWrapper.Body.CorneringResistanceCurve.Evaluate(_currentCarStats.SpeedPercent) *
                              _partsPresetWrapper.Body.CorneringResistanceStrength * _handbrakeEffect;

            if (_currentCarStats.InAir)
                return;

            float slipAngle = Vector3.SignedAngle(_transform.forward, _rb.linearVelocity, _transform.up);

            ////in case of reversing
            if (slipAngle >= 90)
                slipAngle = -slipAngle + 180;
            else if (slipAngle <= -90)
                slipAngle = -slipAngle - 180;

            //car self centering effect
            _rb.AddTorque(slipAngle * _currentCarStats.SpeedInMsPerS * _partsPresetWrapper.Body.CorneringResistanceCurve.Evaluate(_currentCarStats.SpeedPercent) *
                              _partsPresetWrapper.Body.CorneringResistanceStrength * (_rb.angularVelocity.magnitude + 1) *
                              _handbrakeEffect * _transform.up, ForceMode.Force);
        }

        public void AutoRecover(bool enabled, float offset)
        {
            if (!enabled)
                return;

            if(_recovering)
            {
                _rb.centerOfMass = _cachedCOMLocalPosition + Vector3.up * offset;
                if (Vector3.Dot(_transform.up, Vector3.up) > 0.5f)
                    _recovering = false;
                return;
            }

            if (_currentCarStats.AllWheelsGrounded)
            {
                _rb.centerOfMass = _cachedCOMLocalPosition;
                return;
            }

            //not completely turned over
            if (Vector3.Dot(_transform.up, Vector3.up) > -0.9f)
            {
                _rb.centerOfMass = _cachedCOMLocalPosition;
                return;
            }

            _recovering = true;
        }


        public void PerformAerialControls(float sensitivity, float pitchInput, float yawInput, float rollInput)
        {
            if (!_currentCarStats.InAir)
                return;

            float airTime = Mathf.Clamp(_currentCarStats.AirTime, 0, MAX_SENSITIVITY_AFTER_AERIAL_TIME);

            float sensMultiplier = airTime / MAX_SENSITIVITY_AFTER_AERIAL_TIME;

            Vector3 rotatePitchForce = pitchInput * sensMultiplier * sensitivity * _transform.right;
            Vector3 rotateYawForce = yawInput * sensMultiplier * sensitivity * _transform.up;
            Vector3 rotateRollForce = -rollInput * sensitivity * _transform.forward;
            _rb.AddTorque(rotatePitchForce + rotateYawForce + rotateRollForce, ForceMode.Acceleration);
        }
    }
}
