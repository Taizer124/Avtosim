using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/vehicle-damage-system/collision-handler")]
    public class CollisionHandler : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _minCollisionSpeed = 10;
        [SerializeField]
        private float _impactCollisionCooldown = 1.5f;

        private float _collisionStayAdditionalTime = 0.15f;

        private float _lastLeftSideStay;
        private float _lastRightSideStay;
        private float _nextCollTime;

        public event Action<CollisionImpactInfo> OnCollisionImpact;

        public event Action<CollisionStayInfo[]> OnCollisionSideStay;
        public event Action<CollisionSide> OnCollisionSideExit;

        public event Action OnAllCollisionsExit;

        private ContactPoint[] _contactPoints;

        private void Awake()
        {
            _contactPoints = new ContactPoint[6];
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Time.time < _nextCollTime)
                return;

            float mag = collision.relativeVelocity.magnitude;
            if (mag < _minCollisionSpeed)
            {
                OnAllCollisionsExit?.Invoke();
                return;
            }

            _nextCollTime = Time.time + _impactCollisionCooldown;
            int count = collision.GetContacts(_contactPoints);

            SendImpactEvent(0, count - 1, collision, mag);

            for (int i = 1; i < count; i++)
            {
                SendImpactEvent(i, i - 1, collision, mag);
            }
        }

        private void SendImpactEvent(int i, int j, Collision collision, float mag)
        {
            float dist = 1 + Vector3.Distance(_contactPoints[i].point, _contactPoints[j].point);

            float otherWeight = collision.rigidbody ? collision.rigidbody.mass : _rigidbody.mass;

            float weightRatio = otherWeight / _rigidbody.mass;


            CollisionImpactInfo info = new(GetCollisionSide(collision), collision.contactCount,
                _contactPoints[i].point, _contactPoints[i].normal, collision.relativeVelocity,
                mag, Mathf.Abs(Vector3.Dot(_rigidbody.linearVelocity.normalized, _contactPoints[i].normal)), dist, weightRatio);

            OnCollisionImpact?.Invoke(info);
        }

        private CollisionSide GetCollisionSide(Collision collision)
        {
            Vector3 contactNormal = collision.contacts[0].normal;
            Vector3 up = transform.up;
            Vector3 right = transform.right;
            Vector3 forward = transform.forward;

            float upDot = Vector3.Dot(contactNormal, up);
            float rightDot = Vector3.Dot(contactNormal, right);
            float forwardDot = Vector3.Dot(contactNormal, forward);

            float absUpDot = Mathf.Abs(upDot);
            float absRightDot = Mathf.Abs(rightDot);
            float absForwardDot = Mathf.Abs(forwardDot);

            if (absUpDot > absRightDot && absUpDot > absForwardDot)
            {
                return upDot > 0 ? CollisionSide.Bottom : CollisionSide.Top;
            }
            else if (absRightDot > absUpDot && absRightDot > absForwardDot)
            {
                return rightDot > 0 ? CollisionSide.Left : CollisionSide.Right;
            }
            else
            {
                return forwardDot > 0 ? CollisionSide.Rear : CollisionSide.Front;
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            float mag = collision.relativeVelocity.magnitude;
            if (mag < _minCollisionSpeed)
            {
                OnAllCollisionsExit?.Invoke();
                return;
            }

            ContactPoint[] contactPoints = new ContactPoint[6];

            int collAmount = collision.GetContacts(contactPoints);

            CollisionStayInfo[] collisionInfoArray = new CollisionStayInfo[collAmount];

            int rightColls = 0;
            int leftColls = 0;
            int topColls = 0;
            int bottomColls = 0;

            Vector3 right = transform.right;
            Vector3 up = transform.up;

            float dotAlignment = 0.3f;

            for (int i = 0; i < collAmount; i++)
            {
                ContactPoint contactPoint = contactPoints[i];

                Vector3 normal = contactPoint.normal;

                float leftDot = Vector3.Dot(-right, normal);
                float bottomDot = Vector3.Dot(-up, normal);

                if (bottomDot > dotAlignment)
                {
                    collisionInfoArray[i] = new CollisionStayInfo(CollisionSide.Top, contactPoint.point, mag);
                    topColls++;
                }
                else if (bottomDot < -dotAlignment)
                {
                    collisionInfoArray[i] = new CollisionStayInfo(CollisionSide.Bottom, contactPoint.point, mag);
                    bottomColls++;
                }

                if (leftDot > dotAlignment)
                {
                    collisionInfoArray[i] = new CollisionStayInfo(CollisionSide.Right, contactPoint.point, mag);
                    _lastRightSideStay = Time.time;
                    rightColls++;
                }
                else if (leftDot < -dotAlignment)
                {
                    collisionInfoArray[i] = new CollisionStayInfo(CollisionSide.Left, contactPoint.point, mag);
                    _lastLeftSideStay = Time.time;
                    leftColls++;
                }
            }

            OnCollisionSideStay?.Invoke(collisionInfoArray);

            if (bottomColls == 0)
                OnCollisionSideExit?.Invoke(CollisionSide.Bottom);
            if (topColls == 0)
                OnCollisionSideExit?.Invoke(CollisionSide.Top);
            if (rightColls == 0 && Time.time > _lastRightSideStay + _collisionStayAdditionalTime)
                OnCollisionSideExit?.Invoke(CollisionSide.Right);
            if (leftColls == 0 && Time.time > _lastLeftSideStay + _collisionStayAdditionalTime)
                OnCollisionSideExit?.Invoke(CollisionSide.Left);
        }

        private void OnCollisionExit(Collision collision)
        {
            OnAllCollisionsExit?.Invoke();
        }
    }

    public struct CollisionImpactInfo
    {
        public CollisionSide Side;
        public int CollisionsCount;
        public Vector3 Point;
        public Vector3 Normal;
        public Vector3 RelativeVelocity;
        public float CollisionMagnitude;
        public float DotToMyVelocity;
        public float DistanceToPreviousCollisionPoint;
        public float WeightRario;

        public CollisionImpactInfo(CollisionSide side, int count, Vector3 point, Vector3 normal, Vector3 relVel, float mag, float dot, float dist, float weightRatio)
        {
            Side = side;
            CollisionsCount = count;
            Point = point;
            Normal = normal;
            RelativeVelocity = relVel;
            CollisionMagnitude = mag;
            DotToMyVelocity = dot;
            DistanceToPreviousCollisionPoint = dist;
            WeightRario = weightRatio;
        }
    }


    [Serializable]
    public struct CollisionStayInfo
    {
        public CollisionSide CollisionSide;
        public Vector3 Position;
        public float RelativeMagnitude;

        public CollisionStayInfo(CollisionSide side, Vector3 pos, float mag)
        {
            CollisionSide = side;
            Position = pos;
            RelativeMagnitude = mag;
        }
    }

    public enum CollisionSide
    {
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Rear,
    }
}
