using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.VehicleController
{
    [RequireComponent(typeof(RaceSpawner))]
    public class RaceManager : MonoBehaviour
    {
        [SerializeField]
        private PositionEvaluator _positionEvaluator;

        [Min(1)]
        public int LapCount = 1;

        [Range(0, 1f), Tooltip("Adjust this value if you want to change where the track ends. You can change it only if the amount of laps equals to 1.")]
        public float RaceEndNormilizedTime = 1;

        [Min(1), Tooltip("After this amount of racers finish the race, the countdown to race finish will start.")]
        public int PodiumPlaces = 3;

        [Min(0), Tooltip("The amount of time that must pass before the racers can control the vehicle.")]
        public float StartCountdownTime = 3;
        private float _startCountdownTimeLeft;
        public float StartCountdownTimeLeft => _startCountdownTimeLeft;

        [Tooltip("Defines what causes the countdown to start, which enables the vehicle controls after it ends.")]
        public RaceStartConditions RaceStartCondition;

        [Min(0), Tooltip("The amount of time given to other racers after the podium places have been taken before ending the race.")]
        public float WaitForOtherRacersTime = 10;
        private float _waitForOtherRacersTimeLeft = 10;
        public float WaitForOtherRacersTimeLeft => _waitForOtherRacersTimeLeft;

        private bool _endCountDownStarted = false;
        private bool _raceEnded = false;

        [Space, Separator()]
        public UnityEvent OnCountdownStarted;
        public UnityEvent OnRaceStarted;
        public UnityEvent OnPlayerFinishedFirst;
        public UnityEvent<int> OnPlayerFinished;
        public UnityEvent OnPodiumPlacedTaken;
        public UnityEvent OnAllRacersFinished;
        public UnityEvent OnRaceEnd;

        private List<RacerProgress> _racersProgressList;
        private List<RaceParticipant> _raceParticipantsList;
        private int _finishedRacersAmount;
        public int FinishedRacersAmount => _finishedRacersAmount;
        private Dictionary<RaceParticipant, RaceInfoForPlayer> _trackFollowDirectionForRacerDict;
        
        private static RaceManager _instance;
        public static RaceManager Instance => _instance;

        private void Awake()
        {
            ResetRaceData();
        }

        private void Start()
        {
            if (RaceStartCondition != RaceStartConditions.InStartMethod)
                return;

            StartCoroutine(StartRace());
        }

        public void BeginCountdown()
        {
            if (RaceStartCondition != RaceStartConditions.AfterExternalCall)
                return;

            StartCoroutine(StartRace());
        }

        private IEnumerator StartRace()
        {
            _startCountdownTimeLeft = StartCountdownTime;
            OnCountdownStarted?.Invoke();
            while (_startCountdownTimeLeft >= 0)
            {
                _startCountdownTimeLeft -= Time.deltaTime;
                yield return null;
            }
            OnRaceStarted?.Invoke();
            for (int i = 0; i < _raceParticipantsList.Count; i++)
                _raceParticipantsList[i].EnableInput(true);
        }

        private void OnValidate()
        {
            if (LapCount > 1)
                RaceEndNormilizedTime = 1;
        }

        public void ResetRaceData()
        {
            _racersProgressList = new();
            _raceParticipantsList = new();
            _trackFollowDirectionForRacerDict = new();

            if (_instance == null)
                _instance = this;

            _endCountDownStarted = false;
            _raceEnded = false;
            _finishedRacersAmount = 0;
            _waitForOtherRacersTimeLeft = WaitForOtherRacersTime;
        }

        public void RegisterInRace(RaceParticipant raceParticipant, bool isPlayer)
        {
            var racer = new RacerProgress(raceParticipant.transform.root.name, isPlayer);
            _raceParticipantsList.Add(raceParticipant);
            _racersProgressList.Add(racer);
            _trackFollowDirectionForRacerDict.Add(raceParticipant, new(racer));
            racer.OnRacerFinished += Racer_OnRacerFinished;

            raceParticipant.EnableInput(false);
        }

        private void Racer_OnRacerFinished(RacerProgress racer)
        {
            _finishedRacersAmount++;

            if (racer.IsPlayer)
            {
                OnPlayerFinished.Invoke(_finishedRacersAmount);
                if (_finishedRacersAmount == 1)
                    OnPlayerFinishedFirst.Invoke();
            }

            if(_finishedRacersAmount == PodiumPlaces)
            {
                OnPodiumPlacedTaken?.Invoke();

                if (WaitForOtherRacersTime == 0)
                {
                    _endCountDownStarted = false;
                    OnRaceEnd?.Invoke();
                }
                else
                    _endCountDownStarted = true;
            }

            if (_finishedRacersAmount == _racersProgressList.Count)
            {
                OnAllRacersFinished?.Invoke();
                if(!_raceEnded)
                    OnRaceEnd?.Invoke();
                _endCountDownStarted = false;
                _raceEnded = true;
            }

            racer.OnRacerFinished -= Racer_OnRacerFinished;
        }

        private void Update()
        {
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            if (_raceEnded)
                return;

            if (_racersProgressList == null)
                return;

            SortActiveParticipantsByProgress();
            HandleCountdown();
        }

        private void HandleCountdown()
        {
            if (!_endCountDownStarted)
                return;

            if(_finishedRacersAmount == _racersProgressList.Count)
            {
                _endCountDownStarted = false;
                OnRaceEnd?.Invoke();
            }

            if (_finishedRacersAmount != PodiumPlaces)
                return;

            _waitForOtherRacersTimeLeft -= Time.deltaTime;
            if(_waitForOtherRacersTimeLeft <= 0)
            {
                _raceEnded = true;
                OnRaceEnd?.Invoke();
                return;
            }
        }

        public void CalculateDirectionForRacer(RaceParticipant racer, Vector3 worldPos, float speed)
        {
            var racerInfo = _trackFollowDirectionForRacerDict[racer];

            if (!racer.IsPlayer)
                racerInfo.DirectionToFollow = _positionEvaluator.GetFollowTrackDirection(worldPos, speed);

            _positionEvaluator.CalculateProgress(worldPos);
            racerInfo.RacerProgress.UpdateProgress(LapCount, _positionEvaluator.GetProgress(), RaceEndNormilizedTime);
        }

        public Vector3 GetDirectionForRacer(RaceParticipant racer) => _trackFollowDirectionForRacerDict[racer].DirectionToFollow;

        public List<RacerProgress> GetLeaderboard() => _racersProgressList;

        private void SortActiveParticipantsByProgress()
        {
            QuickSort.Sort(_racersProgressList, _finishedRacersAmount, _racersProgressList.Count - 1);
        }
    }

    [Serializable]
    public class RacerProgress
    {
        public event Action<RacerProgress> OnRacerFinished;

        private bool _isPlayer;
        public bool IsPlayer => _isPlayer;

        private string _racerName;
        public string RacerName => _racerName;
        public string RacerNameDebug;

        private float _raceProgressNormalized;
        public float RacerProgressNormalized => _raceProgressNormalized;
        public float RacerProgressDebug;

        private int _lapsPassed;
        public int LapsPassed => _lapsPassed;
        public int LapsPasedDebug;

        private bool _finishedRace = false;
        public bool FinishedRace => _finishedRace;
        public bool FinishedRaceDebug;

        private float _lastProgress;

        public RacerProgress(string name, bool player)
        {
            RacerNameDebug = name;
            _racerName = name;
            _isPlayer = player;
            _raceProgressNormalized = 0;
            _lapsPassed = 0;
        }

        public void UpdateProgress(int lapCount, float progress, float modifiedRaceEndNormalizedTime)
        {
            if (_finishedRace)
                return;

            RacerProgressDebug = _raceProgressNormalized = progress;

            if(modifiedRaceEndNormalizedTime != 1 && progress > modifiedRaceEndNormalizedTime)
            {
                _lapsPassed++;
                OnRacerFinished?.Invoke(this);
                _finishedRace = true;
            }

            if (_raceProgressNormalized < 0.5f && _lastProgress > 0.5f)
                _lapsPassed++;

            LapsPasedDebug = _lapsPassed;

            if (_lapsPassed >= lapCount)
            {
                OnRacerFinished?.Invoke(this);
                FinishedRaceDebug = _finishedRace = true;
            }

            _lastProgress = _raceProgressNormalized;
        }
    }

    public class RaceInfoForPlayer
    {
        public RaceInfoForPlayer(RacerProgress racerProgress)
        {
            RacerProgress = racerProgress;
        }
        public Vector3 DirectionToFollow;
        public RacerProgress RacerProgress;
    }

    public enum RaceStartConditions
    {
        InStartMethod,
        AfterExternalCall
    }
}
