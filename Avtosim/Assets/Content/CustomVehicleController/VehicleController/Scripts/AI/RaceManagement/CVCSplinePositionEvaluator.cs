using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
#if MATH_PACKAGE_INSTALLED
using Unity.Mathematics;
#endif
using UnityEngine;
#if SPLINE_PACKAGE_INSTALLED
using UnityEngine.Splines;
#endif

namespace Assets.VehicleController
{
    [HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/ai-racers-setup")]
    public class CVCSplinePositionEvaluator : PositionEvaluator
    {
#if SPLINE_PACKAGE_INSTALLED
        [SerializeField]
        private SplineContainer _splineContainer;
        [SerializeField, Min(10)]
        private float _lookAheadDistance = 15;
        private float _raceProgressCached = 0;

        private float _trackLength = 0;
        private float _segmentsLength = 0;

        private NativeArray<float3> _splinePointsWorldPosNativeArray;
        private NativeArray<float> _splinePointTimesNativeArray;
        private NativeArray<float> _splineSegmentLengthNativeArray;

        private void Awake()
        {
            int pointCount = _splineContainer[0].Count;

            _splinePointsWorldPosNativeArray = new NativeArray<float3>(pointCount, Allocator.Persistent);
            _splinePointTimesNativeArray = new NativeArray<float>(pointCount, Allocator.Persistent);
            _splineSegmentLengthNativeArray = new NativeArray<float>(pointCount - 1, Allocator.Persistent);

            for (int i = 0; i < pointCount; i++)
            {
                _splinePointsWorldPosNativeArray[i] = _splineContainer.transform.TransformPoint(_splineContainer[0][i].Position);
                SplineUtility.GetNearestPoint(_splineContainer[0], _splineContainer[0][i].Position, out float3 nearestPoint, out float t);
                _splinePointTimesNativeArray[i] = t;
            };

            for(int i = 0; i < pointCount - 1; i++)
            {
                _splineSegmentLengthNativeArray[i] = Vector3.Distance(_splinePointsWorldPosNativeArray[i], _splinePointsWorldPosNativeArray[i + 1]);
                _segmentsLength += _splineSegmentLengthNativeArray[i];
            }

            _trackLength = _splineContainer.CalculateLength();
        }

        private void OnDestroy()
        {
            if (_splinePointsWorldPosNativeArray.IsCreated)
                _splinePointsWorldPosNativeArray.Dispose();
            if(_splinePointTimesNativeArray.IsCreated)
                _splinePointTimesNativeArray.Dispose();
            if(_splineSegmentLengthNativeArray.IsCreated)
                _splineSegmentLengthNativeArray.Dispose();
        }

        public override Vector3 GetFollowTrackDirection(Vector3 position, float speedMS)
        {
            NativeArray<float3> resultDir = new NativeArray<float3>(1, Allocator.TempJob);

            GetFollowTrackDirectionJob dirJob = new GetFollowTrackDirectionJob
            {
                VehicleWorldPos = position,
                VehicleSpeedMs = speedMS,
                TrackLength = _trackLength,
                LookAheadDistance = _lookAheadDistance,
                SplinePoints = _splinePointsWorldPosNativeArray,
                SplinePointTimes = _splinePointTimesNativeArray,
                resultDirection = resultDir,
            };

            JobHandle handle1 = dirJob.Schedule();
            handle1.Complete();

            Vector3 direction = resultDir[0];

            resultDir.Dispose();

            return direction;
        }

        public override void CalculateProgress(Vector3 position)
        {
            NativeArray<float> resultTime = new NativeArray<float>(1, Allocator.TempJob);
            GetNormalizedTimeJob timeJob = new GetNormalizedTimeJob
            {
                VehicleWorldPos = position,
                Closed = _splineContainer[0].Closed,
                SegmentsLength = _segmentsLength,
                SplinePoints = _splinePointsWorldPosNativeArray,
                SplinePointTimes = _splinePointTimesNativeArray,
                SplineSegmentLengths = _splineSegmentLengthNativeArray,
                resultTime = resultTime,
            };

            JobHandle handle1 = timeJob.Schedule();
            handle1.Complete();
            _raceProgressCached = resultTime[0];
            resultTime.Dispose();
        }

        public override float GetProgress() => _raceProgressCached;

        [BurstCompile]
        private struct GetFollowTrackDirectionJob : IJob
        {
            public float3 VehicleWorldPos;
            public float VehicleSpeedMs;
            public float TrackLength;
            public float LookAheadDistance;
            public NativeArray<float3> SplinePoints;
            public NativeArray<float> SplinePointTimes;

            public NativeArray<float3> resultDirection;

            public void Execute()
            {
                int size = SplinePoints.Length;

                int closestPointID = 0;
                int targetPointID = 0;
                float lookAheadTime = (math.abs(VehicleSpeedMs) + LookAheadDistance) / TrackLength;

                float closestDistance = float.MaxValue;
                for (int i = 0; i < size; i++)
                {
                    float distance = math.distance(VehicleWorldPos, SplinePoints[i]);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPointID = i;
                    }
                }

                float targetTime = (SplinePointTimes[closestPointID] + lookAheadTime) % 1;
                float minTimeDifference = float.MaxValue;
                for (int i = 0; i < size; i++)
                {
                    float diff = math.abs(targetTime - SplinePointTimes[i]);
                    if (diff < minTimeDifference)
                    {
                        minTimeDifference = diff;
                        targetPointID = i;
                    }
                }

                if (targetPointID == closestPointID)
                    targetPointID++;

                if (targetPointID == size)
                    targetPointID = 0;

                float3 evaluatedPos = SplinePoints[targetPointID];
                evaluatedPos.y = VehicleWorldPos.y;
                resultDirection[0] = math.normalize(evaluatedPos - VehicleWorldPos);
            }
        }
        [BurstCompile]
        private struct GetNormalizedTimeJob : IJob
        {
            public float3 VehicleWorldPos;
            public bool Closed;
            public float SegmentsLength;
            public NativeArray<float3> SplinePoints;
            public NativeArray<float> SplinePointTimes;
            public NativeArray<float> SplineSegmentLengths;

            public NativeArray<float> resultTime;

            public void Execute()
            {
                resultTime[0] = CalculateNormalizedTime();
            }

            private float CalculateNormalizedTime()
            {
                float closestDistance = float.MaxValue;
                float closestT = 0;
                int closestSegmentIndex = 0;

                int size = Closed ? SplineSegmentLengths.Length : SplinePointTimes.Length;

                for (int i = 0; i < size; i++)
                {
                    int nextPointIndex = i + 1;
                    if (nextPointIndex == size)
                    {
                        if (Closed)
                            nextPointIndex = 0;
                        else
                            nextPointIndex--;// For non-closed splines, exit the loop at the end
                    }

                    float3 segmentVector = SplinePoints[nextPointIndex] - SplinePoints[i];
                    float segmentLengthSquared = math.lengthsq(segmentVector);
                    if (segmentLengthSquared == 0)
                        continue; // Skip degenerate segments

                    float t = math.dot(VehicleWorldPos - SplinePoints[i], segmentVector) / segmentLengthSquared;
                    t = math.clamp(t, 0, 1);
                    float3 closestPoint = SplinePoints[i] + t * segmentVector;
                    float distance = math.distance(VehicleWorldPos, closestPoint);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestT = t;
                        closestSegmentIndex = i;
                    }
                }

                // Calculate the cumulative length up to the closest segment
                float cumulativeLength = 0;


                for (int i = 0; i < closestSegmentIndex; i++)
                    cumulativeLength += SplineSegmentLengths[i];

                cumulativeLength += closestT * SplineSegmentLengths[closestSegmentIndex];
                float normalizedTime = cumulativeLength / SegmentsLength;

                return normalizedTime;
            }
        }
#else
        [TextArea]
        public string Warning = "To use this component, install Unity's Splines package";
        public override Vector3 GetFollowTrackDirection(Vector3 position, float speedABS)
        {
            return Vector3.zero;
        }
        public override void CalculateProgress(Vector3 position)
        {

        }
        public override float GetProgress() => 0;
#endif
    }
}
