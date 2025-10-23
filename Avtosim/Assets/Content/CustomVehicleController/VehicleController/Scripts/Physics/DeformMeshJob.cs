#if MATH_PACKAGE_INSTALLED && BURST_PACKAGE_INSTALLED

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.VehicleController
{
    [BurstCompile]
    public struct DeformMeshJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float3> OriginalVertices;
        [ReadOnly]
        public float3 CollisionPoint;
        [ReadOnly]
        public float3 CollisionNormal;

        [ReadOnly]
        public float MaxDeformDepth;
        [ReadOnly]
        public float CollisionAreaSQ;
        [ReadOnly]
        public float TotalAreaSQ;
        [ReadOnly]
        public float CollisionStrength;

        public NativeArray<float3> DeformedVertices;

        public NativeArray<float> DeformationDepthArray;

        public void Execute(int index)
        {
            if (DeformationDepthArray[index] >= MaxDeformDepth)
                return;

            float distanceBetweenPointsSQ = math.distancesq(CollisionPoint, OriginalVertices[index]);
            if (distanceBetweenPointsSQ > TotalAreaSQ)
                return;

            float fallOffStrength = 1 - distanceBetweenPointsSQ / TotalAreaSQ;
            float deformationStrength = math.clamp(fallOffStrength * CollisionStrength, 0, MaxDeformDepth);

            if(distanceBetweenPointsSQ > CollisionAreaSQ)
            {
                float noise = Mathf.PerlinNoise(OriginalVertices[index].z, OriginalVertices[index].x);
                deformationStrength *= noise;
            }

            DeformationDepthArray[index] += deformationStrength;
            DeformedVertices[index] += deformationStrength * CollisionNormal;
        }
    }

    [BurstCompile]
    public struct DeformMeshAreaJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<int> AreasVerticesIndexes;
        [ReadOnly, NativeDisableParallelForRestriction]
        public NativeArray<float3> OriginalVertices;
        [ReadOnly]
        public float3 CollisionPoint;
        [ReadOnly]
        public float3 CollisionNormal;

        [ReadOnly]
        public float MaxDeformDepth;
        [ReadOnly]
        public float CollisionAreaSQ;
        [ReadOnly]
        public float TotalAreaSQ;
        [ReadOnly]
        public float CollisionStrength;

        [NativeDisableParallelForRestriction]
        public NativeArray<float3> DeformedVertices;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> DeformationDepthArray;

        public void Execute(int index)
        {
            int verticeID = AreasVerticesIndexes[index];
            if (DeformationDepthArray[verticeID] >= MaxDeformDepth)
                return;

            float distanceBetweenPointsSQ = math.distancesq(CollisionPoint, OriginalVertices[verticeID]);
            if (distanceBetweenPointsSQ > TotalAreaSQ)
                return;

            float fallOffStrength = 1 - distanceBetweenPointsSQ / TotalAreaSQ;
            float deformationStrength = math.clamp(fallOffStrength * CollisionStrength, 0, MaxDeformDepth);

            if (distanceBetweenPointsSQ > CollisionAreaSQ)
            {
                float noise = Mathf.PerlinNoise(OriginalVertices[verticeID].z, OriginalVertices[verticeID].x);
                deformationStrength *= noise;
            }

            DeformationDepthArray[verticeID] += deformationStrength;
            DeformedVertices[verticeID] += deformationStrength * CollisionNormal;
        }
    }
}
#endif