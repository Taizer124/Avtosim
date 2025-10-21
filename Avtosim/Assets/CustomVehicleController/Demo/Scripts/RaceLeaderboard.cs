using UnityEngine;
using UnityEngine.UI;

namespace Assets.VehicleController
{
    public class RaceLeaderboard : MonoBehaviour
    {
        [SerializeField]
        private RaceManager _raceManager;
        [SerializeField]
        private Text _lapsProgress;
        private Text[] _racerProgressTextArray;
        [SerializeField]
        private GameObject _parent;
        [SerializeField]
        private GameObject _racerInfoPrefab;
        [SerializeField]
        private float _distanceBetweenLines;
        [SerializeField]
        private Text _countdownText;

        private void CreateLeaderboard()
        {
            if(_racerProgressTextArray != null && _racerProgressTextArray.Length > 0)
            {
                for (int i = 0; i < _racerProgressTextArray.Length; i++)
                {
                    Destroy(_racerProgressTextArray[i]);
                }
            }

            _racerProgressTextArray = new Text[_raceManager.GetLeaderboard().Count];
            float currentDist = 0;
            for (int i = 0; i < _raceManager.GetLeaderboard().Count; i++)
            {
                GameObject racerProgressInfo = Instantiate(_racerInfoPrefab);
                racerProgressInfo.transform.SetParent(_parent.transform, false);
                racerProgressInfo.transform.localPosition -= new Vector3(0, currentDist, 0);
                currentDist += _distanceBetweenLines;
                _racerProgressTextArray[i] = racerProgressInfo.GetComponent<Text>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            int countdown = Mathf.CeilToInt(_raceManager.StartCountdownTimeLeft);
            if(countdown > 0)
            {
                _countdownText.text = countdown.ToString();
            }
            else
                _countdownText.gameObject.SetActive(false);




            RacerProgress playerProgress = null;

            var list = _raceManager.GetLeaderboard();
            int size = list.Count;

            if(_racerProgressTextArray == null || size != _racerProgressTextArray.Length)
            {
                CreateLeaderboard();
            }

            for (int i = 0; i < size; i++)
            {
                var racerProgress = list[i];

                if (racerProgress.IsPlayer)
                    playerProgress = racerProgress;

                _racerProgressTextArray[i].text = (i + 1) + " - " + racerProgress.RacerName;
            }

            if (playerProgress == null)
                return;

            _lapsProgress.text = playerProgress.LapsPassed + "/" + _raceManager.LapCount;
        }
    }
}

