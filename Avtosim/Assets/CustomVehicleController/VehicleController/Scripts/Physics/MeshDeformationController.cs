#if MATH_PACKAGE_INSTALLED

using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.VehicleController
{
    public class MeshDeformationController
    {
        private Transform _transform;
        private MeshDeformationParameters _params;

        private AreaDamageSystem _areaDamageSystem;
        private PartDamageHandler _partDamageHandler;
        private VehicleAttachmentsAligner _attachmentsAligner;

        private NativeArray<float3> _originalVerticesNativeArray;
        private NativeArray<float3> _deformedVerticesNativeArray;
        private NativeArray<float> _deformationDepthNativeArray;

        private Stack<JobHandle> _scheduledDeformationJobs;

        private List<NativeArray<int>> _areaVerticesIDs;

        // Start is called before the first frame update
        public MeshDeformationController(MeshDeformationParameters parameters, Transform transform, PartDamageHandler partDamageHandler, VehicleAttachmentsAligner aligner)
        {
            _params = parameters;
            _transform = transform;
            _partDamageHandler = partDamageHandler;
            _attachmentsAligner = aligner;

            _areaDamageSystem = new();
            _scheduledDeformationJobs = new();

            _params.MeshFilter.sharedMesh.MarkDynamic();

            int size = _params.MeshFilter.sharedMesh.vertices.Length;

            _originalVerticesNativeArray = new NativeArray<float3>(size, Allocator.Persistent);
            _deformedVerticesNativeArray = new NativeArray<float3>(size, Allocator.Persistent);
            _deformationDepthNativeArray = new NativeArray<float>(size, Allocator.Persistent);

            using (var dataArray = Mesh.AcquireReadOnlyMeshData(_params.MeshFilter.sharedMesh))
            {
                dataArray[0].GetVertices(_originalVerticesNativeArray.Reinterpret<Vector3>());
                dataArray[0].GetVertices(_deformedVerticesNativeArray.Reinterpret<Vector3>());
            }

            ConvertAreasVerticeIDsToNative();
        }

        public void ProcessDeformationJobsStack()
        {
            bool deformationCompleted = false;
            while (_scheduledDeformationJobs.Count > 0)
            {
                _scheduledDeformationJobs.Pop().Complete();
                deformationCompleted = true;
            }

            if (deformationCompleted)
                ApplyDeformation();
        }

        private void ConvertAreasVerticeIDsToNative()
        {
            _areaVerticesIDs = new();
            for (int i = 0; i < _params.CollisionAreasDataSO.AreasVertices.Length; i++)
            {
                var areaVerticesIds = new NativeArray<int>(_params.CollisionAreasDataSO.AreasVertices[i].AreaVerticesIndexes.Length, Allocator.Persistent);
                NativeArray<int>.Copy(_params.CollisionAreasDataSO.AreasVertices[i].AreaVerticesIndexes, areaVerticesIds);
                _areaVerticesIDs.Add(areaVerticesIds);
            }
        }

        public void ClearNativeArrays()
        {
            _originalVerticesNativeArray.Dispose();
            _deformedVerticesNativeArray.Dispose();
            _deformationDepthNativeArray.Dispose();

            foreach (var a in _areaVerticesIDs)
                a.Dispose();
            _areaVerticesIDs.Clear();
        }

        public void Repair()
        {
            NativeArray<float3>.Copy(_originalVerticesNativeArray, _deformedVerticesNativeArray);
            ApplyDeformation();
        }

        public void ProcessCollisionImpact(CollisionImpactInfo info)
        {
            if (info.Side == CollisionSide.Bottom)
                return;

            Vector3 localCollPoint = _params.MeshFilter.transform.InverseTransformPoint(info.Point);
            Vector3 normal = _params.MeshFilter.transform.InverseTransformDirection(info.Normal);
            float collStr = CalculateCollisionStrength(info);
            float collisionArea = info.CollisionMagnitude / 20 * info.DistanceToPreviousCollisionPoint * _params.DamageRadiusMultiplier;
            //damageArea *= (1 - _params.BodyStrength);

            if (_params.CollisionAreasOptimization)
            {
                if (_params.DeformAffectedCollisionAreasNearby)
                    CalculateDeformationToAffectedCollisionAreas(info.Point, localCollPoint, normal, collStr, collisionArea);
                else
                    CalculateDeformationToCollisionArea(info.Point, localCollPoint, normal, collStr, collisionArea);
            }
            else
                CalculateDeformationToWholeBody(localCollPoint, normal, collStr, collisionArea);

            _partDamageHandler?.ProcessCollision(info.Point, collisionArea, collStr);
            _attachmentsAligner?.ProcessCollision(info.Point, info.Normal, collisionArea, _params.BodyStrength);
        }

        private JobHandle ScheduleMeshDeformationJob(Vector3 point, Vector3 normal, float collStr, float damageArea)
        {
            DeformMeshJob deformMeshJob = new DeformMeshJob()
            {
                OriginalVertices = _originalVerticesNativeArray,
                DeformedVertices = _deformedVerticesNativeArray,
                DeformationDepthArray = _deformationDepthNativeArray,
                CollisionPoint = point,
                CollisionNormal = normal,
                CollisionStrength = collStr,
                MaxDeformDepth = _params.MaxDeformDepth,
                CollisionAreaSQ = damageArea * damageArea,
                TotalAreaSQ = Mathf.Pow(damageArea + _params.AdditionalDamageRadius, 2)
            };

            JobHandle deformMeshJobHandle;
            if (_scheduledDeformationJobs.Count == 0)
                deformMeshJobHandle = deformMeshJob.Schedule(_originalVerticesNativeArray.Length, 128);
            else
                deformMeshJobHandle = deformMeshJob.Schedule(_originalVerticesNativeArray.Length, 128, _scheduledDeformationJobs.Peek());

            return deformMeshJobHandle;
        }

        private JobHandle ScheduleMeshAreaDeformationJob(Vector3 point, Vector3 normal, float collStr, float damageArea, NativeArray<int> areasVerticesIndexes)
        {
            DeformMeshAreaJob deformMeshAreaJob = new DeformMeshAreaJob()
            {
                AreasVerticesIndexes = areasVerticesIndexes,
                OriginalVertices = _originalVerticesNativeArray,
                DeformedVertices = _deformedVerticesNativeArray,
                DeformationDepthArray = _deformationDepthNativeArray,
                CollisionPoint = point,
                CollisionNormal = normal,
                CollisionStrength = collStr,
                MaxDeformDepth = _params.MaxDeformDepth,
                CollisionAreaSQ = damageArea * damageArea,
                TotalAreaSQ = Mathf.Pow(damageArea + _params.AdditionalDamageRadius, 2)
            };

            JobHandle deformMeshAreaJobHandle;
            if (_scheduledDeformationJobs.Count == 0)
                deformMeshAreaJobHandle = deformMeshAreaJob.Schedule(areasVerticesIndexes.Length, 128);
            else
                deformMeshAreaJobHandle = deformMeshAreaJob.Schedule(areasVerticesIndexes.Length, 128, _scheduledDeformationJobs.Peek());

            return deformMeshAreaJobHandle;
        }

        private float CalculateCollisionStrength(CollisionImpactInfo info)
        {
            float deformationStrength = info.CollisionMagnitude * info.WeightRario;
            return deformationStrength / info.CollisionsCount * (1 - _params.BodyStrength);
        }

        private void CalculateDeformationToAffectedCollisionAreas(Vector3 point, Vector3 localCollPoint, Vector3 normal, float collStr, float damageArea)
        {
            List<int> collisionAreaIDsList = _areaDamageSystem.FindAffectedCollisionAreasID(_transform, point, damageArea, _params.CollisionAreasDataSO);

            for (int j = 0; j < collisionAreaIDsList.Count; j++)
            {
                int collisionAreaID = collisionAreaIDsList[j];
                _scheduledDeformationJobs.Push(ScheduleMeshAreaDeformationJob(localCollPoint, normal, collStr, damageArea, _areaVerticesIDs[collisionAreaID]));
            }
        }

        private void CalculateDeformationToCollisionArea(Vector3 point, Vector3 localCollPoint, Vector3 normal, float collStr, float damageArea)
        {
            int collisionAreaID = _areaDamageSystem.FindCollisionAreaID(_transform, point, _params.CollisionAreasDataSO);
            _scheduledDeformationJobs.Push(ScheduleMeshAreaDeformationJob(localCollPoint, normal, collStr, damageArea, _areaVerticesIDs[collisionAreaID]));
        }

        private void CalculateDeformationToWholeBody(Vector3 localCollPoint, Vector3 normal, float collSpeed, float damageArea)
        {
            _scheduledDeformationJobs.Push(ScheduleMeshDeformationJob(localCollPoint, normal, collSpeed, damageArea));
        }

        private void ApplyDeformation()
        {
            _params.MeshFilter.sharedMesh.SetVertices(_deformedVerticesNativeArray);
            _params.MeshFilter.sharedMesh.RecalculateNormals();
        }
    }
}
#endif