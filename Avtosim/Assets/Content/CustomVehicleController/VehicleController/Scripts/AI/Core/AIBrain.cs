using UnityEngine;

namespace Assets.VehicleController
{
    [RequireComponent(typeof(RaycastSensor)), RequireComponent(typeof(AIVehicleInputProvider)), RequireComponent(typeof(RaceParticipant)), HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/ai-racers-setup")]
    public class AIBrain : MonoBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;

        [SerializeField]
        private RaycastSensor _raycastSensor;

        [SerializeField, Space]
        private RaceParticipant _raceParticipant;

        [SerializeField, Space]
        private AIVehicleInputProvider _inputProvider;

        private AIBehaviorProcessor _aiBehaviorProcessor;
        private AIDangerProcessor _aiDangerProcessor;

        [Separator, SerializeField, Range(1, 10), Tooltip("Defines how close the car wants to stay to the track. Higher values make the AI choose the closest path to the followed spline.")]
        private int _AIInterestModifier = 7;
        [SerializeField, Range(1, 10), Tooltip("Defines how the AI treats the obstacles that it sees. At higher values it ignores the obstacles that are not in the chosen movement direction more.")]
        private int _AIDangerModifier = 3;
        [SerializeField, Range(0.9f, 0.999f), Tooltip("Defines how dangerous the possible collision must be before a car will take serious action to avoid it")]
        private float _riskiness;

        private RacerAIBreakInput _racerAIBrakes;
        private RacerAIGasInput _racerAIGas;
        private RacerAIHorizontalInput _racerAISteering;

        private AIRacerState _state;

        private float _stateChangeCooldown = 0.33f;
        private float _lastStateChangeTime = -1f;

        private void Awake()
        {
            _aiBehaviorProcessor = new(_raycastSensor.RaycastAmount);
            _aiDangerProcessor = new();
            _racerAIBrakes = new();
            _racerAIGas = new();
            _racerAISteering = new();

            _state = AIRacerState.FollowingTrack;
        }

        private void Start()
        {
            _raycastSensor.Initialize(_raceParticipant, _vehicleController.GetRigidbody());
        }

        private void Update()
        {
            switch(_state)
            {
                case AIRacerState.ReturningToTrack:
                    HandleReturningToTrack();
                    break;
                case AIRacerState.Reversing:
                    HandleReversing(); 
                    break;
                case AIRacerState.FollowingTrack:
                    HandleTrackFollowing();
                    break;
            }
        }

        private void HandleReturningToTrack()
        {
            RaycastHitInfo rearRaycastHitInfo = _raycastSensor.GetRearRaycastHitInfo();

            if (!StateChangeInCooldown() && rearRaycastHitInfo.Hit)
            {
                _lastStateChangeTime = Time.time;
                _state = AIRacerState.FollowingTrack;
                return;
            }

            Vector3 reverseDirection = RaceManager.Instance.GetDirectionForRacer(_raceParticipant);
            float dotToReverse = Vector3.Dot(reverseDirection, -rearRaycastHitInfo.Direction);
            bool recovered = dotToReverse > 0.5f;

            if (!StateChangeInCooldown() && recovered)
            {
                _lastStateChangeTime = Time.time;
                _state = AIRacerState.FollowingTrack;
            }

            EvaluateInput(rearRaycastHitInfo, reverseDirection, dotToReverse);
        }

        private void HandleTrackFollowing()
        {
            RaycastHitInfo[] raycastHitInfoArray = _raycastSensor.GetRaycastHitInfoArray();

            Vector3 trackDirection = RaceManager.Instance.GetDirectionForRacer(_raceParticipant);
            float dotToTrack = Vector3.Dot(trackDirection, raycastHitInfoArray[0].Direction);

            Vector3 targetDir = _aiBehaviorProcessor.GetDirectionalVector(raycastHitInfoArray, trackDirection, _AIInterestModifier, _AIDangerModifier);

            float reverseDot = -0.5f;
            bool needsReversing = dotToTrack < reverseDot;

            if (!StateChangeInCooldown() && needsReversing)
            {
                _lastStateChangeTime = Time.time;
                _state = AIRacerState.ReturningToTrack;
            }

            int dangerDirID = _aiDangerProcessor.FindMovingDirectionID(raycastHitInfoArray[_aiBehaviorProcessor.HighestInterestID].Direction, 
                _aiBehaviorProcessor.GetDangerArray(), _aiBehaviorProcessor.LowestDanger, raycastHitInfoArray);

            if(!StateChangeInCooldown())
            {
                for (int i = 0; i < raycastHitInfoArray.Length; i++)
                {
                    if (!raycastHitInfoArray[i].Hit)
                        continue;
                    if (_vehicleController.GetCurrentCarStats().SpeedInMsPerS < 7 && raycastHitInfoArray[i].HitVelocity.sqrMagnitude < 50 && raycastHitInfoArray[i].HitDistance < 7)
                    {
                        _state = AIRacerState.Reversing;
                        _lastStateChangeTime = Time.time;
                        break;
                    }
                }
            }



            EvaluateInput(raycastHitInfoArray, dangerDirID, targetDir, dotToTrack);
        }

        private void HandleReversing()
        {
            RaycastHitInfo[] raycastHitInfoArray = _raycastSensor.GetRaycastHitInfoArray();
            Vector3 trackDirection = RaceManager.Instance.GetDirectionForRacer(_raceParticipant);
            Vector3 targetDir = _aiBehaviorProcessor.GetDirectionalVector(raycastHitInfoArray, trackDirection, _AIInterestModifier, _AIDangerModifier);

            RaycastHitInfo rearRaycastHitInfo = _raycastSensor.GetRearRaycastHitInfo();

            int dangerDirID = _aiDangerProcessor.FindMovingDirectionID(raycastHitInfoArray[_aiBehaviorProcessor.HighestInterestID].Direction, _aiBehaviorProcessor.GetDangerArray(), _aiBehaviorProcessor.LowestDanger, raycastHitInfoArray);
            if(!StateChangeInCooldown())
            {
                if (dangerDirID != -1)
                {
                    if ((raycastHitInfoArray[_aiBehaviorProcessor.HighestInterestID].DotToControllerForward >
                            raycastHitInfoArray[dangerDirID].DotToControllerForward)
                             || rearRaycastHitInfo.Hit)
                    {
                        _state = AIRacerState.FollowingTrack;
                        _lastStateChangeTime = Time.time;
                    }

                }
                else
                {
                    _state = AIRacerState.FollowingTrack;
                    _lastStateChangeTime = Time.time;
                }
            }


            EvaluateInput(rearRaycastHitInfo, targetDir, 0);
        }

        private void EvaluateInput(RaycastHitInfo[] raycastHitInfoArray, int dangerID, Vector3 targetDir, float dot)
        {
            Vector3 right = transform.right;

            float steerInput = _racerAISteering.GetHorizontalInput(right, targetDir, dot, _state);

            bool movingInDangerDir = false;

            if (dangerID != -1)
                movingInDangerDir = _aiDangerProcessor.MovingInDangerDirection(raycastHitInfoArray[0].Direction, raycastHitInfoArray[dangerID].Direction, _riskiness, raycastHitInfoArray[dangerID]);

            float dotToDanger = 0;

            if (dangerID != -1)
                dotToDanger = Vector3.Dot(raycastHitInfoArray[0].Direction, raycastHitInfoArray[dangerID].Direction);

            if (movingInDangerDir)
                steerInput = _aiDangerProcessor.CorrectSteeringFromFutureCollision(dotToDanger, steerInput);

            float brakeInput = _racerAIBrakes.GetInput(raycastHitInfoArray[_aiBehaviorProcessor.HighestInterestID], dot, dangerID != -1 ? raycastHitInfoArray[dangerID] : null, dotToDanger, _riskiness, _state);
            float gasInput = _racerAIGas.GetInput(raycastHitInfoArray[_aiBehaviorProcessor.HighestInterestID], dot, _state, brakeInput);
            _inputProvider.SetInput(Mathf.Clamp01(brakeInput), Mathf.Clamp01(gasInput), Mathf.Clamp(steerInput, -1, 1));
        }

        private void EvaluateInput(RaycastHitInfo raycastHitInfo, Vector3 targetDir, float dot)
        {
            float steerInput = _racerAISteering.GetHorizontalInput(transform.right, targetDir, dot, _state);
            
            float brakeInput = _racerAIBrakes.GetInput(raycastHitInfo, dot, null, 0, _riskiness, _state);
            float gasInput = _racerAIGas.GetInput(raycastHitInfo, dot, _state, brakeInput);

            _inputProvider.SetInput(Mathf.Clamp01(brakeInput), Mathf.Clamp01(gasInput), Mathf.Clamp(steerInput, -1, 1));
        }

        private bool StateChangeInCooldown() => _lastStateChangeTime + _stateChangeCooldown > Time.time;
    }
    public enum AIRacerState
    {
        FollowingTrack,
        ReturningToTrack,
        Reversing
    }
}
