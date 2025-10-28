using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class RaceManagerDebug : MonoBehaviour
    {
        [SerializeField]
        private RaceManager _raceManager;
        public List<RacerProgress> _racersProgressList;
        public float CountdownTimeLeft;
        public float WaitForOthersTimeLeft;
        [TextArea]
        public string Logs = "This component is only for debugging. Remove it if you don't need it.";

        private void Awake()
        {
            Logs = "";
        }

        private void Start()
        {
            _racersProgressList = _raceManager.GetLeaderboard();
        }

        private void Update()
        {
            CountdownTimeLeft = _raceManager.StartCountdownTimeLeft;
            WaitForOthersTimeLeft = _raceManager.WaitForOtherRacersTimeLeft;
        }

        public void OnAllRacersFinished()
        {
            Logs += "All racers finished \n";
        }
        public void OnPlayerFinished(int pos)
        {
            Logs +=  "Player finished " + pos + "\n";
        }
        public void OnPlayerFinishedFirst()
        {
            Logs += "Player finished first \n";
        }
        public void OnRaceEnded()
        {
            Logs += "Race ended \n";
        }
        public void OnPodiumPlacesTaken()
        {
            Logs += "Podium places taken \n";
        }

        public void OnCountdownStarted()
        {
            Logs += "Countdown started \n";
        }

        public void OnRaceStarted()
        {
            Logs += "Race started \n";
        }
    }
}

