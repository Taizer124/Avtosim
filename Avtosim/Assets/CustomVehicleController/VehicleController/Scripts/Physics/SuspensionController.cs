using UnityEngine;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Physics/Suspension Controller")]
    public class SuspensionController : MonoBehaviour
    {
        private float _wheelRadius;
        private VehiclePartsSetWrapper _partsPresetWrapper;

        private float _minSpringLength;
        public float MinSpringLength
        {
            get => _minSpringLength;
        }
        private float _maxSpringLength;
        public float MaxSpringLength
        {
            get { return _maxSpringLength; }
        }
        private float _currentSpringLength;

        private float _currentSpringLengthPlusRadiusOffset;
        public float CurrentSpringLengthPlusGroundOffset
        {
            get => _currentSpringLengthPlusRadiusOffset;
        }
        private float _springRestLength;
        public float SpringRestLength
        {
            get => _springRestLength;
        }
        private float _springTravelLength;
        public float SpringTravelLength
        {
            get => _springTravelLength;
        }
        private float _damperStiffness;
        public float DamperStiffness
        {
            get => _damperStiffness;
        }

        private HitInformation _hitInfo;
        public HitInformation HitInfo
        {
            get => _hitInfo;
        }

        private float _lastSpringLength;

        private float _springVelocity;

        private bool _isFrontSusp;

        private SuspensionSO _frontSuspensionSO;
        private SuspensionSO _rearSuspensionSO;

        private const float FIXED_TIME_STEP = 0.02f;

        public void Initialize(VehiclePartsSetWrapper partsPresetWrapper, bool front, float wheelRadius)
        {
            _partsPresetWrapper = partsPresetWrapper;
            _isFrontSusp = front;
            _wheelRadius = wheelRadius;
            _hitInfo = new();


            _partsPresetWrapper.OnPartsChanged += OnPresetChanged;
            _frontSuspensionSO = _partsPresetWrapper.FrontSuspension;
            _rearSuspensionSO = _partsPresetWrapper.RearSuspension;
            _frontSuspensionSO.OnSuspensionStatsChanged += OnSuspensionStatsChanged;
            _rearSuspensionSO.OnSuspensionStatsChanged += OnSuspensionStatsChanged;

            UpdateSpringStats();
        }

        private void OnSuspensionStatsChanged()
        {
            UpdateSpringStats();
        }

        private void OnPresetChanged()
        {
            _frontSuspensionSO.OnSuspensionStatsChanged -= OnSuspensionStatsChanged;
            _rearSuspensionSO.OnSuspensionStatsChanged -= OnSuspensionStatsChanged;

            _frontSuspensionSO = _partsPresetWrapper.FrontSuspension;
            _rearSuspensionSO = _partsPresetWrapper.RearSuspension;
            _frontSuspensionSO.OnSuspensionStatsChanged += OnSuspensionStatsChanged;
            _rearSuspensionSO.OnSuspensionStatsChanged += OnSuspensionStatsChanged;
            UpdateSpringStats();
        }

        private void UpdateSpringStats()
        {
            _springRestLength = _isFrontSusp ? _frontSuspensionSO.SpringRestDistance : _rearSuspensionSO.SpringRestDistance;

            _springTravelLength = _isFrontSusp ? _frontSuspensionSO.SpringTravelLength : _rearSuspensionSO.SpringTravelLength;

            _damperStiffness = _isFrontSusp ? _frontSuspensionSO.SpringDampingStiffness : _rearSuspensionSO.SpringDampingStiffness;

            _minSpringLength = _springRestLength - _springTravelLength;
            _maxSpringLength = _springRestLength + _springTravelLength;
        }

        public void CalculateSpringForceAndHitPoint(int suspensionSimulationPrecision, LayerMask ignoreLayers)
        {
            FindAverageWheelContactPointAndHighestPoint(suspensionSimulationPrecision, ignoreLayers);

            if (_hitInfo.Hit)
            {
                _lastSpringLength = _currentSpringLength;
                _currentSpringLength = Mathf.Clamp(_hitInfo.Distance - _wheelRadius, _minSpringLength, _maxSpringLength);

                _springVelocity = (_lastSpringLength - _currentSpringLength) / FIXED_TIME_STEP;
            }
        }

        private void FindAverageWheelContactPointAndHighestPoint(int suspensionSimulationPrecision, LayerMask ignoreLayer)
        {
            if (suspensionSimulationPrecision == 1)
            {
                if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, _maxSpringLength + _wheelRadius, ~ignoreLayer))
                {
#if UNITY_EDITOR
                    Debug.DrawLine(transform.position, hit.point);
#endif              
                    _currentSpringLengthPlusRadiusOffset = hit.distance;
                    _hitInfo.SetHitInfo(true, hit.point, hit.normal, hit.distance);
                    return;
                }

                _hitInfo.SetHitInfo(false, Vector3.zero, Vector3.zero, 0);
                return;
            }

            int hits = 0;

            float step = _wheelRadius / (suspensionSimulationPrecision - 1) * 2;
            Vector3 hitPoint = Vector3.zero;
            Vector3 hitNormal = Vector3.zero;
            float hitDistance = 0;

            float lowestDistance = float.MaxValue;

            //cache forward and up vectors
            Vector3 forward = transform.forward;
            Vector3 up = transform.up;

            Vector3 position = transform.position;

            float rayLen = _maxSpringLength + _wheelRadius;

            for (int i = 0; i < suspensionSimulationPrecision; i++)
            {
                float offsetZ = -_wheelRadius + i * step;

                Ray ray = new Ray(position + forward * offsetZ, -up);

                if (Physics.Raycast(ray, out RaycastHit hit, rayLen, ~ignoreLayer))
                {
#if UNITY_EDITOR
                    Debug.DrawLine(position + forward * offsetZ, hit.point);
#endif
                    float distanceMultiplierFromRadius = (Mathf.Abs(offsetZ) / _wheelRadius);

                    float distance = hit.distance + hit.distance * (distanceMultiplierFromRadius * _wheelRadius);

                    if (distance < lowestDistance)
                    {
                        hitPoint = hit.point;
                        hitNormal = hit.normal;
                        _currentSpringLengthPlusRadiusOffset = hitDistance = lowestDistance = distance;
                    }
                    hits++;
                }
            }

            if (hits == 0)
            {
                _hitInfo.SetHitInfo(false, Vector3.zero, Vector3.zero, 0);
                return;
            }

            _hitInfo.SetHitInfo(true, hitPoint, hitNormal, hitDistance);
        }


        public float GetSuspForce()
        {
            float stiffness = _isFrontSusp ? _partsPresetWrapper.FrontSuspension.SpringStiffness : _partsPresetWrapper.RearSuspension.SpringStiffness;
            float restDistance = _isFrontSusp ? _partsPresetWrapper.FrontSuspension.SpringRestDistance : _partsPresetWrapper.RearSuspension.SpringRestDistance;
            float damper = _isFrontSusp ? _partsPresetWrapper.FrontSuspension.SpringDampingStiffness : _partsPresetWrapper.RearSuspension.SpringDampingStiffness;

            float springForce = stiffness * (restDistance - _currentSpringLength);
            float damperForce = damper * _springVelocity;

            return springForce + damperForce;
        }

        public class HitInformation
        {
            public bool Hit = false;
            public Vector3 Position = Vector3.zero;
            public Vector3 HitNormal = Vector3.zero;
            public float Distance = 0;

            public void SetHitInfo(bool hit, Vector3 pos, Vector3 normal, float dist)
            {
                Hit = hit;
                Position = pos;
                HitNormal = normal;
                Distance = dist;
            }
        }
    }
}

