using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [RequireComponent(typeof(SuspensionController)), AddComponentMenu("CustomVehicleController/Physics/Wheel Controller")]
    public class WheelController : MonoBehaviour
    {
        private Rigidbody _rb;
        private SuspensionController _suspensionController;
        private TireController _tireController;

        #region Forces
        public float Torque { get; set; }
        public float BrakeForce { get; set; }
        #endregion

        #region Variables

        [Header("Wheel radius can be calculated automatically from the mesh renderer. \n" +
            "In case the radius is wrong, set it manually.\n")]
        [SerializeField]
        private float _wheelRadius;
        public float WheelRadius
        {
            get
            {
                if (_wheelRadius == 0)
                    TryFindWheelRadius();
                return _wheelRadius;
            }
            set => _wheelRadius = value;
        }
        private float _speedRpm = 0;
        public float WheelRPM
        {
            get { return _speedRpm; }
        }
        private float _visualRpm = 0;
        public float VisualRPM
        {
            get { return _visualRpm; }
        }
        private float _steerAngle;
        public float SteerAngle
        {
            get { return _steerAngle; }
            set { _steerAngle = value; }
        }

        private const float MIN_SPEED_TO_LOCK = 5;
        private const float MAX_LOCK_EFFECT_SPEED = 80;

        private bool _lockedUp;
        public bool LockedUp => _lockedUp;

        private float _lockEffectStrength = 0;
        private const float LOCK_EFFECT_MAX_STR = 10;
        private float _lockEffect;
        public float LockEffectStrength => _lockEffect;

        private bool _tcsWorked = false;
        public bool TCSWorked => _tcsWorked;

        private float _slipWheelRPM = 0;
        private const float MAX_SLIP_WHEEL_RPM = 50000;

        public float SidewaysSlip
        {
            get { return _tireController.SidewaysSlip; }
        }
        public float VisualSidewaysSlip
        {
            get => _tireController.VisualSideSlip;
        }

        public float ForwardSlip
        {
            get { return _tireController.ForwardSlip; }
        }
        public bool HasContactWithGround
        {
            get { return _suspensionController.HitInfo.Hit; }
        }
        #endregion

        #region Visuals
        [SerializeField]
        private Transform _wheelMeshTransform;
        private Vector3 _wheelPosition;
        public Vector3 WheelPosition
        {
            get => _wheelPosition;
        }
        private Vector3 _wheelInitialPosition;
        private float _distanceFromSuspensionToWheelTopPoint;
        public float DistanceFromSuspensionToWheelTopPoint
        {
            get => _distanceFromSuspensionToWheelTopPoint;
        }
        private Transform _childrenToNotRotateParentTransform;
        private Transform _wheelMeshParent;
        #endregion

        public void Initialize(SuspensionController suspensionController, Transform wheelMeshTransform, Transform wheelMeshParent, Transform childrenToNotRotateParentTransform, VehiclePartsSetWrapper partsPresetWrapper, Rigidbody rb, float wheelBaseLen, float axelLen, bool front)
        {
            _rb = rb;
            _suspensionController = suspensionController;

            _wheelMeshTransform = wheelMeshTransform;
            _wheelMeshParent = wheelMeshParent;

            _tireController = new(transform, _wheelMeshTransform, partsPresetWrapper, front, axelLen, wheelBaseLen, _rb);
            _wheelInitialPosition = _wheelMeshTransform.transform.localPosition;
            _wheelPosition = _wheelInitialPosition;
            _childrenToNotRotateParentTransform = childrenToNotRotateParentTransform;
            if (_wheelRadius == 0)
                TryFindWheelRadius();

            _distanceFromSuspensionToWheelTopPoint = Vector3.Distance(transform.position, _wheelMeshTransform.position + transform.up * _wheelRadius);
            if (transform.position == wheelMeshTransform.position)
                Debug.LogWarning($"GameObjects with WheelController script must be positioned above the wheel, at the point where the top of suspension is supposed to be.");
        }

        private void TryFindWheelRadius()
        {
            if (_wheelMeshTransform.TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
            {
                _wheelRadius = mesh.bounds.size.y / 2;
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"Wheel Controller couldn't automatically calculate wheel's radius as mesh renderer was not found on {_wheelMeshTransform.gameObject.name}. " +
                    "\n Radius has been set to default value (0.25). Set the radius to correct value manually in edit mode.");
#endif
                _wheelRadius = 0.25f;
            }
        }

        public void UpdateWheelRadiusFromMesh()
        {
            if (_wheelMeshTransform.TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
                _wheelRadius = mesh.bounds.size.y / 2;
        }

        public void TryLockWheel(bool ABS, float brakeInput, float speed)
        {
            if(ABS)
            {
                _lockedUp = false;
                return;
            }

            float _lockThreshold = 0.92f;
            _lockedUp = brakeInput >= _lockThreshold && speed >= MIN_SPEED_TO_LOCK;
            _lockEffectStrength = (brakeInput - _lockThreshold) / (1 - _lockThreshold);
        }

        public void ControlWheel(float speed, float speedPercent, float acceleration, float distanceToGround, bool TCS)
        {
            float actualSteerAngle = _steerAngle;
            if (_lockedUp)
            {
                _lockEffect = Mathf.Clamp01((Mathf.Abs(speed) - MIN_SPEED_TO_LOCK) / MAX_LOCK_EFFECT_SPEED) * _lockEffectStrength;
                actualSteerAngle /= _lockEffect + LOCK_EFFECT_MAX_STR;
            }
            else
                _lockEffect = 0;

            transform.localRotation = Quaternion.Euler(new Vector3(0, actualSteerAngle, 0));

            float maxWheelLoad = _tireController.GetMaxWheelLoad(acceleration, distanceToGround);

            if (TCS)
                (_tcsWorked, Torque) = ApplyTCS(maxWheelLoad);
            else
                _tcsWorked = false;

            _tireController.CalculateForwardSlip(Torque, maxWheelLoad, Mathf.Abs(speed));
            CalculateWheelRPM(speed);

            if (!HasContactWithGround)
                return;

            Vector3 pos = _wheelMeshTransform.position;

            ApplyBraking(pos);
            ApplySteering(pos, speed, speedPercent);
            ApplyTorque(pos);
        }

        private (bool,float) ApplyTCS(float maxWheelLoad)
        {
            float modifiedTorque = Torque;

            if (modifiedTorque == 0)
                return (false, 0);

            float torqueStep = Mathf.Sqrt(modifiedTorque);
            bool TCSworked = false;

            while(modifiedTorque >= maxWheelLoad) 
            {
                modifiedTorque -= torqueStep;
                TCSworked = true;
            }

            if(TCSworked)
            {
                modifiedTorque += torqueStep;
                torqueStep /= 10;
                while (modifiedTorque >= maxWheelLoad)
                    modifiedTorque -= torqueStep;
            }

            return (TCSworked, modifiedTorque);
        }
        

        private void ApplyTorque(Vector3 pos)
        {
            if (Torque == 0)
                return;

            float slipMultiplier = (_tireController.ForwardSlip / 2) + 1;
            float force = Torque / slipMultiplier;
            _rb.AddForceAtPosition(force * transform.forward, pos);

            Torque = 0;
        }
        private void ApplySteering(Vector3 pos, float speed, float speedPercent)
        {
            _rb.AddForceAtPosition(_tireController.CalculateSidewaysForce(Mathf.Abs(Torque) > 1, speed, speedPercent), pos);
        }
        private void ApplyBraking(Vector3 pos)
        {
            if (BrakeForce == 0)
                return;
            _rb.AddForceAtPosition(BrakeForce * transform.forward, pos);
        }

        public void ApplyHandbrake(float force, float gasInput, float tractionPercent)
        {
            _tireController.ApplyHandbrake(force, gasInput, tractionPercent);
            //if (force != 0)
            //{
            //    Vector3 angularVelocity = _rb.angularVelocity;
            //    float angularVelocityY = angularVelocity.y;
            //    float side = Mathf.Sign(angularVelocityY);

            //    Vector3 resultForce = angularVelocity.magnitude * force * transform.right;
            //    resultForce *= side;

            //    _rb.AddForceAtPosition(resultForce, _wheelMeshTransform.position, ForceMode.Force);
            //}
        }

        public void DecreaseFriction(bool locked, float effectStrength)
        {
            _tireController.DecreaseFriction(locked, effectStrength);
        }

        private void CalculateWheelRPM(float speed)
        {
            _speedRpm = speed / _wheelRadius;
            if (HasContactWithGround)
            {
                // this rpm is used to shift gears
                if (Torque == 0)
                {
                    _visualRpm = speed / _wheelRadius;
                    _slipWheelRPM = 0;
                    return;
                }

                //slip rpm is additive to visual rpm, if the wheel continues slipping, slip rpm increases
                _slipWheelRPM += (Torque / 100 * _tireController.ForwardSlip - speed) * Time.deltaTime;

                if (Torque > 0)
                    _slipWheelRPM = Mathf.Clamp(_slipWheelRPM, 0, MAX_SLIP_WHEEL_RPM);
                else
                    _slipWheelRPM = Mathf.Clamp(_slipWheelRPM, -MAX_SLIP_WHEEL_RPM, 0);

                //this rpm is used for engine rpm calculations and wheel rotation
                _visualRpm = speed / _wheelRadius + _slipWheelRPM;
                if (_tireController.ForwardSlip == 0)
                    _slipWheelRPM = 0;

                return;
            }

            _slipWheelRPM = 0;
            if (BrakeForce == 0)
            {
                if (Torque == 0)
                {
                    _visualRpm -= _visualRpm * Time.fixedDeltaTime * 20;
                }
                else
                {
                    _visualRpm += Torque * Time.fixedDeltaTime;
                }
            }
            else
            {
                _visualRpm -= _visualRpm * Time.fixedDeltaTime * 80;
            }
        }

        public void UpdateWheelPosition(bool restrainPosition)
        {
            if (HasContactWithGround)
                UpdateGroundPosition(restrainPosition);
            else
                UpdateWheelAirPosition();

            UpdateChildrenTransform();
        }


        private void UpdateGroundPosition(bool restrainPosition)
        {
            if (_suspensionController.CurrentSpringLengthPlusGroundOffset > _suspensionController.MaxSpringLength + _wheelRadius)
                return;

            float targetY = _wheelInitialPosition.y + _distanceFromSuspensionToWheelTopPoint - (_suspensionController.CurrentSpringLengthPlusGroundOffset - _wheelRadius * 2);
            if (restrainPosition)
            {
                //don't allow the wheel go beyond suspension top point
                if (targetY > _wheelInitialPosition.y + _distanceFromSuspensionToWheelTopPoint)
                    targetY = _wheelInitialPosition.y + _distanceFromSuspensionToWheelTopPoint;

                //don't allow wheel go below ground
                if (_suspensionController.HitInfo.Distance > _suspensionController.SpringRestLength + _wheelRadius)
                    targetY += _suspensionController.HitInfo.Distance - (_suspensionController.SpringRestLength + _wheelRadius);
            }

            _wheelPosition = new Vector3(_wheelInitialPosition.x,
                                 targetY,
                                 _wheelInitialPosition.z);
        }

        private void UpdateWheelAirPosition()
        {
            _wheelPosition.y += Vector3.Dot(transform.up, Vector3.up) * Physics.gravity.y / (_suspensionController.DamperStiffness / 50) * Time.deltaTime;

            float defaultPos = _wheelInitialPosition.y + _distanceFromSuspensionToWheelTopPoint + _wheelRadius;

            _wheelPosition.y = Mathf.Clamp(_wheelPosition.y,
                defaultPos - _suspensionController.MaxSpringLength,
                defaultPos - _suspensionController.SpringRestLength
                );
        }

        private void UpdateChildrenTransform()
        {
            if (_childrenToNotRotateParentTransform == null)
                return;

            _childrenToNotRotateParentTransform.transform.position = _wheelMeshTransform.position;

            if (_wheelMeshParent == null)
                return;

            _childrenToNotRotateParentTransform.transform.rotation = _wheelMeshParent.rotation;
        }

        public void SetWheelMeshTransform(Transform transform) => _wheelMeshTransform = transform;

        public bool IsExceedingSlipThreshold(float forwardSlip, float sidewaysSlip, bool accelerating)
        {
            if (!HasContactWithGround)
                return false;


            if (VisualSidewaysSlip > sidewaysSlip || (accelerating && ForwardSlip > forwardSlip))
                return true;

            return false;
        }

        //for the editor, it uses it to define if the controller has been initialized
        public Transform GetWheelTransform() => _wheelMeshTransform;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 centerPos = (transform.position + _wheelMeshTransform.position) / 2;
            Gizmos.DrawCube(centerPos, new Vector3(0.07f, Vector3.Distance(transform.position, _wheelMeshTransform.position), 0.07f));
            Gizmos.DrawWireSphere(_wheelMeshTransform.position, _wheelRadius);
        }
#endif
    }
}
