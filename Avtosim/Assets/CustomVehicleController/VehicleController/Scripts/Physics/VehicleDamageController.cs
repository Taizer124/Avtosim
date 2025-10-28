#if MATH_PACKAGE_INSTALLED
using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/vehicle-damage-system/vehicle-damage-controller")]
    public class VehicleDamageController : MonoBehaviour
    {
        [SerializeField]
        private CollisionHandler _collisionHandler;
        [SerializeField]
        private bool _deformationPersists = false;
        [SerializeField]
        private MeshDeformationParameters _meshDeformationParameters;

        [SerializeField, Space, Header("    Optional: ")]
        private PartDamageHandler _partDamageHandler;

        [SerializeField]
        private VehicleAttachmentsAligner _attachmentsAligner;

        private MeshDeformationController _meshDeformator;

        // Start is called before the first frame update
        void Start()
        {
            _meshDeformator = new(_meshDeformationParameters, transform, _partDamageHandler, _attachmentsAligner);
            if(_collisionHandler!= null)
                _collisionHandler.OnCollisionImpact += OnCollisionImpact;
        }

        private void LateUpdate()
        {
            _meshDeformator.ProcessDeformationJobsStack();
        }

        private void OnDestroy()
        {
            if (!_deformationPersists)
                RepairAll();
            _meshDeformator.ClearNativeArrays();
            if (_collisionHandler != null)
                _collisionHandler.OnCollisionImpact -= OnCollisionImpact;
        }

        public void RepairAll()
        {
            RepairMainBody();
            RepairAttachments();
            RepairDamagableParts();
        }

        public void RepairMainBody()
        {
            _meshDeformator.Repair();
        }

        public void RepairDamagableParts()
        {
            _partDamageHandler?.RepairParts();
        }

        public void RepairAttachments()
        {
            _attachmentsAligner?.RepairAttachments();
        }

        private void OnCollisionImpact(CollisionImpactInfo info)
        {
            _meshDeformator.ProcessCollisionImpact(info);
        }       
    }

    [Serializable]
    public class MeshDeformationParameters
    {
        [SerializeField]
        public MeshFilter MeshFilter;
        [Header("   Optional: ")]
        public CollisionAreasDataSO CollisionAreasDataSO;
        [Range(0.001f, 0.999f)]
        public float BodyStrength = 0.99f;
        [Min(0.01f)]
        public float DamageRadiusMultiplier = 3f;
        [Min(0)]
        public float AdditionalDamageRadius = 1.5f;
        [Min(0.01f)]
        public float MaxDeformDepth = 0.5f;
        public bool CollisionAreasOptimization = true;
        public bool DeformAffectedCollisionAreasNearby = true;
    }
}
#endif