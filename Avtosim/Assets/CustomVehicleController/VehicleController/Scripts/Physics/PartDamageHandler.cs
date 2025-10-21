using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.VehicleController
{
    [HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/vehicle-damage-system/part-damage-handler")]
    public class PartDamageHandler : MonoBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;
        private CurrentCarStats _currentCarStats;

        [SerializeField]
        private DamagableVehiclePart[] _damagableVehicleParts;
        private Queue<Transform> _detachedPartsToDeactivate;

        private float _time;
        private float _clearTimer;
        private const float CLEAR_TIME_TOTAL = 5;

        private void Start()
        {
            _currentCarStats = _vehicleController.GetCurrentCarStats();
            _detachedPartsToDeactivate = new();
            _clearTimer = CLEAR_TIME_TOTAL;
        }

        private void Update()
        {
            UpdateParts();
            ClearDetachedParts();
        }

        private void ClearDetachedParts()
        {
            if (_detachedPartsToDeactivate.Count == 0)
                return;

            _clearTimer -= Time.deltaTime;

            if(_clearTimer < 0)
            {
                _clearTimer = CLEAR_TIME_TOTAL;
                _detachedPartsToDeactivate.Dequeue().gameObject.SetActive(false);
            }
        }

        public void RepairParts()
        {
            foreach (var part in _damagableVehicleParts)
            {
                part.Repair();
                part.PartTransform.gameObject.SetActive(true);
            }

            _detachedPartsToDeactivate = new();
        }

        private void UpdateParts()
        {
            _time += Time.deltaTime;

            float carSpeedAbs = Mathf.Abs(_currentCarStats.SpeedInMsPerS);

            foreach (var part in _damagableVehicleParts)
            {
                if (part.IsDetached())
                    continue;

                if (!part.IsDamaged())
                    continue;

                if (part.DetachmentParameters.DetachAtHighSpeed && carSpeedAbs > part.DetachmentParameters.DetachSpeed)
                {
                    part.DetachSelf(_vehicleController.GetRigidbody().linearVelocity, carSpeedAbs);
                    _detachedPartsToDeactivate.Enqueue(part.PartTransform);
                    continue;            
                }

                if (part.DanglingParameters.DangleFromSpeed)
                    DanglePart(part, carSpeedAbs);
            }
        }

        private void DanglePart(DamagableVehiclePart part, float carSpeedAbs)
        {
            float effect = CalculateDanglingEffect(part, carSpeedAbs);
            float lerpFactor = Mathf.PingPong(_time * part.DanglingParameters.DangleRate, 1f) * effect;
            Vector3 currentRotation = Vector3.Lerp(part.MaxRotation(), Vector3.zero, lerpFactor);

            part.PartTransform.parent.localRotation = Quaternion.Euler(currentRotation);
        }

        private float CalculateDanglingEffect(DamagableVehiclePart part, float speedAbs)
        {
            float effect = Mathf.Clamp01((speedAbs - part.DanglingParameters.MinSpeedForDangling) / (part.DanglingParameters.DangleMaxEffectSpeed - part.DanglingParameters.MinSpeedForDangling));
            return Mathf.Pow(effect, 3); 
        }

        public void ProcessCollision(Vector3 point, float damageArea, float damage)
        {
            foreach(var part in _damagableVehicleParts)
            {
                if (part.IsDetached() || part.IsDamaged())
                    continue;
                if(IsPartAffectedByCollision(part, point, damageArea))
                {
                    part.AddDamage(damage);
                    part.SetPivotIfDamaged(point);
                    if (part.TryDetachIfHPDepleted(_vehicleController.GetRigidbody().linearVelocity, _currentCarStats.SpeedInMsPerS))
                        _detachedPartsToDeactivate.Enqueue(part.PartTransform.parent);
                }
            }
        }

        private bool IsPartAffectedByCollision(DamagableVehiclePart part, Vector3 point, float damageArea)
        {
            return part.PartDamagePoint == PartDamagePoint.PartPivotPoints ? 
                IsAffectedPartPivotPoints(part, point, damageArea) : 
                IsAffectedPartTransform(part, point, damageArea);
        }

        private bool IsAffectedPartTransform(DamagableVehiclePart part, Vector3 point, float damageArea)
        {
            return (point - part.PartTransform.position).sqrMagnitude <= damageArea * damageArea;
        }

        private bool IsAffectedPartPivotPoints(DamagableVehiclePart part, Vector3 point, float damageArea)
        {
            for (int i = 0; i < part.PartPivotPoints.Length; i++)
            {
                if ((point - part.PartPivotPoints[i].PivotPoint.position).sqrMagnitude <= damageArea * damageArea)
                    return true;
            }
            return false;
        }

        private void DestroyDetachedPart(DamagableVehiclePart part)
        {
            Destroy(part.PartTransform);
        }
    }

    [Serializable]
    public class DamagableVehiclePart
    {
        public string Name;
        public Transform PartTransform;
        public PivotPointParameters[] PartPivotPoints;
        public PartDamagePoint PartDamagePoint;
        [Min(1)]
        public float HealthPoints = 100;

        private int _pivotID = 0;
        private Vector3 _defaultLocalPosition;
        private Quaternion _defaultLocalRotation;
        private Transform _defaultParent;

        [Space]
        public DanglingParameters DanglingParameters;
        [Separator]
        public DetachmentParameters DetachmentParameters;

        [Space]
        public UnityEvent OnHealthPointsDepteted;
        public UnityEvent OnDetached;

        private float _damageSustained = 0;

        private bool _detached = false;
        private bool _dangling = false;

        public bool IsDetached() => _detached;
        public bool IsDamaged() => _damageSustained >= HealthPoints;
        public Vector3 MaxRotation() => PartPivotPoints[_pivotID].MaxRotationAround;

        public void Repair()
        {
            if (_damageSustained == 0 || !_dangling || !_detached)
                return;

            PartTransform.parent = _defaultParent;
            PartTransform.localPosition = _defaultLocalPosition;
            PartTransform.localRotation = _defaultLocalRotation;
            _damageSustained = 0;
            _dangling = false;
            _detached = false;
            _pivotID = 0;
        }

        public void AddDamage(float damage)
        {       
            _damageSustained += damage;
            
            if (_damageSustained >= HealthPoints)
                OnHealthPointsDepteted?.Invoke();
        }
        public void SetPivotIfDamaged(Vector3 point)
        {
            if (!DanglingParameters.DangleFromSpeed)
                return;

            if (_dangling)
                return;

            if (_damageSustained < HealthPoints)
                return;

            _defaultParent = PartTransform.parent;
            _defaultLocalPosition = PartTransform.localPosition;
            _defaultLocalRotation = PartTransform.localRotation;

            _pivotID = FindFurthestPivot(point);
            PartTransform.parent = PartPivotPoints[_pivotID].PivotPoint;
            _dangling = true;
        }

        public bool TryDetachIfHPDepleted(Vector3 velocity, float speed)
        {
            if (!DetachmentParameters.DetachWhenHPDepleted)
                return false;

            if (IsDamaged())
            {
                DetachSelf(velocity, speed);
                return true;
            }

            return false;
        }

        private int FindFurthestPivot(Vector3 point)
        {
            int furthestPivotID = 0;
            float minLen = float.MinValue;
            for (int i = 0; i < PartPivotPoints.Length; i++)
            {
                float dist = Vector3.Distance(point, PartPivotPoints[i].PivotPoint.position);
                if (dist > minLen)
                {
                    minLen = dist;
                    furthestPivotID = i;
                }
            }
            return furthestPivotID;
        }

        public void DetachSelf(Vector3 velocity, float speed)
        {
            Rigidbody rb = PartTransform.parent.gameObject.AddComponent<Rigidbody>();
            if(DetachmentParameters.AddBoxColliderWhenDetached)
                PartTransform.gameObject.AddComponent<BoxCollider>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.linearVelocity = velocity * 0.85f;
            rb.angularVelocity = PartTransform.parent.TransformDirection(new Vector3(-speed / 2, 0 , 0));
            rb.AddForce(Vector3.up * (1 + speed / 30), ForceMode.Impulse);
            _detached = true;
            PartTransform.parent.parent = null;
            OnDetached?.Invoke();
        }
    }

    public enum PartDamagePoint
    {
        PartTransform,
        PartPivotPoints
    }

    [Serializable]
    public class PivotPointParameters
    {
        public Transform PivotPoint;
        public Vector3 MaxRotationAround;
    }

    [Serializable]
    public class DanglingParameters
    {
        public bool DangleFromSpeed = false;
        public float MinSpeedForDangling = 60;
        public float DangleMaxEffectSpeed = 120;
        public float DangleRate = 1;
    }

    [Serializable]
    public class DetachmentParameters
    {
        public bool DetachAtHighSpeed = false;
        public bool DetachWhenHPDepleted = false;
        public bool AddBoxColliderWhenDetached = true;
        public float DetachSpeed = 120;
    }
}
