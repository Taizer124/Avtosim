using UnityEngine;

namespace Assets.VehicleController
{
    [HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/ai-racers-setup")]
    public class RaycastSensor : MonoBehaviour
    {
        private Rigidbody _rigidBody;
        private RaceParticipant _raceParticipant;

        [SerializeField, Min(3)]
        private int _raycastAmount = 13;

        public int RaycastAmount => _raycastAmount;

        [SerializeField, Min(1)]
        private float _maxAngleBetweenRays = 5;
        public float MaxAngleBetweenRays => _maxAngleBetweenRays;
        [SerializeField, Min(1), Space]
        private float _rayLength = 50;
        public float RayLength => _rayLength;

        [SerializeField, Min(1)]
        private float _rearRayLength = 5;
        public float RearRayLength => _rearRayLength;

        [SerializeField, Min(1), Space]
        private float _maxHorizontalDistance = 30;
        public float MaxHorizontalDistance => _maxHorizontalDistance;

        [SerializeField]
        private LayerMask _collisionLayer;

        private RaycastHitInfo[] _hitInfoArray;
        private RaycastHitInfo _rearHitInfo;
        private float[] _rayAngleArray;

        private RaycastHit[] _raycastHitResults;

        public void Initialize(RaceParticipant raceParticipant, Rigidbody rigidbody)
        {
            _rigidBody = rigidbody;
            _raceParticipant = raceParticipant;

            InitializeHitInfoArray();
        }

        private void InitializeHitInfoArray()
        {
            _hitInfoArray = new RaycastHitInfo[_raycastAmount];
            _rearHitInfo = new();
            _rayAngleArray = new float[_raycastAmount];
            _raycastHitResults = new RaycastHit[_raycastAmount];

            Vector3 forward = transform.forward;

            for (int i = 0; i < _hitInfoArray.Length; i++)
            {
                _hitInfoArray[i] = new RaycastHitInfo();
                if (i <= _raycastAmount / 2)
                {
                    _rayAngleArray[i] = i * _maxAngleBetweenRays;
                }
                else
                {
                    _rayAngleArray[i] = (i - _raycastAmount / 2) * -_maxAngleBetweenRays;
                }

                float cLen = _rayLength;
                float aLen = cLen * Mathf.Sin(Mathf.Abs(_rayAngleArray[i]) * Mathf.Deg2Rad);
                if (aLen > _maxHorizontalDistance)
                {
                    aLen = _maxHorizontalDistance;
                    cLen = aLen / Mathf.Sin(Mathf.Abs(_rayAngleArray[i]) * Mathf.Deg2Rad);
                }

                _hitInfoArray[i].RayLength = cLen;
                _hitInfoArray[i].DotToControllerForward = Vector3.Dot(forward, Quaternion.Euler(0, _rayAngleArray[i], 0) * forward);
            }
        }

        public RaycastHitInfo[] GetRaycastHitInfoArray()
        {
            if (_hitInfoArray == null || _hitInfoArray.Length != _raycastAmount)
                InitializeHitInfoArray();

            Vector3 fwd = transform.forward;
            Vector3 pos = transform.position;

            for (int i = 0; i < _raycastAmount; i++)
            {
                _hitInfoArray[i].Direction = Quaternion.Euler(0, _rayAngleArray[i], 0) * fwd;

                _hitInfoArray[i].Hit = Physics.Raycast(pos,
                     _hitInfoArray[i].Direction, out _raycastHitResults[i], _hitInfoArray[i].RayLength,
                    _collisionLayer);


                _hitInfoArray[i].HitDistance = _raycastHitResults[i].distance;
                if (_hitInfoArray[i].Hit)
                {
                    Rigidbody hitRB = _raceParticipant.TryFindRaceParticipant(_raycastHitResults[i].collider);
                    if (hitRB != null)
                    {
                        Vector3 velocityDif = hitRB.linearVelocity - _rigidBody.linearVelocity;
                        _hitInfoArray[i].VelocityDifference = velocityDif.x * fwd.x
                                                              + velocityDif.z *
                                                              fwd.z;

                        _hitInfoArray[i].HitVelocity = hitRB.linearVelocity;
                    }
                    else
                    {
                        _hitInfoArray[i].VelocityDifference = _rigidBody.linearVelocity.x * fwd.x + _rigidBody.linearVelocity.z * fwd.z;
                        _hitInfoArray[i].HitVelocity = Vector3.zero;
                    }
                }
                else
                {
                    _hitInfoArray[i].VelocityDifference = _rigidBody.linearVelocity.x * fwd.x + _rigidBody.linearVelocity.z * fwd.z;
                    _hitInfoArray[i].HitVelocity = Vector3.zero;
                }
            }
            return _hitInfoArray;
        }

        public RaycastHitInfo GetRearRaycastHitInfo()
        {
            Vector3 fwd = transform.forward;
            Vector3 pos = transform.position;

            _rearHitInfo.RayLength = _rearRayLength;
            _rearHitInfo.Direction = -fwd;
            _rearHitInfo.Hit = Physics.Raycast(pos, _rearHitInfo.Direction, out RaycastHit hit, _rearHitInfo.RayLength, _collisionLayer);
            if (_rearHitInfo.Hit)
                _rearHitInfo.HitDistance = hit.distance;
            else
                _rearHitInfo.HitDistance = 0;
            return _rearHitInfo;
        }
    }
}