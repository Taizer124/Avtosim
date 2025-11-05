using UnityEngine;
using TMPro; // подключаем TextMeshPro

namespace Assets.VehicleController
{
    public class RaceLeaderboard : MonoBehaviour
    {
        [SerializeField]
        private RaceManager _raceManager;

        [SerializeField]
        private TMP_Text _lapsProgress; // заменили Text TMP_Text

        private TMP_Text[] _racerProgressTextArray; // заменили Text TMP_Text

        [SerializeField]
        private GameObject _parent;

        [SerializeField]
        private GameObject _racerInfoPrefab;

        [SerializeField]
        private float _distanceBetweenLines;

        [SerializeField]
        private TMP_Text _countdownText; // заменили Text TMP_Text

        private void CreateLeaderboard()
        {
            if (_racerProgressTextArray != null && _racerProgressTextArray.Length > 0)
            {
                for (int i = 0; i < _racerProgressTextArray.Length; i++)
                {
                    Destroy(_racerProgressTextArray[i].gameObject);
                }
            }

            var leaderboard = _raceManager.GetLeaderboard();
            _racerProgressTextArray = new TMP_Text[leaderboard.Count];
            float currentDist = 0;

            for (int i = 0; i < leaderboard.Count; i++)
            {
                GameObject racerProgressInfo = Instantiate(_racerInfoPrefab, _parent.transform);
                racerProgressInfo.transform.localPosition -= new Vector3(0, currentDist, 0);
                currentDist += _distanceBetweenLines;

                // Получаем компонент TMP_Text (на объекте должен быть TextMeshProUGUI)
                _racerProgressTextArray[i] = racerProgressInfo.GetComponent<TMP_Text>();
            }
        }

        private void Update()
        {
            int countdown = Mathf.CeilToInt(_raceManager.StartCountdownTimeLeft);
            if (countdown > 0)
            {
                _countdownText.text = countdown.ToString();
            }
            else
            {
                _countdownText.gameObject.SetActive(false);
            }

            RacerProgress playerProgress = null;
            var list = _raceManager.GetLeaderboard();
            int size = list.Count;

            if (_racerProgressTextArray == null || size != _racerProgressTextArray.Length)
            {
                CreateLeaderboard();
            }

            for (int i = 0; i < size; i++)
            {
                var racerProgress = list[i];
                if (racerProgress.IsPlayer)
                    playerProgress = racerProgress;

                _racerProgressTextArray[i].text = $"{i + 1} - {racerProgress.RacerName}";
            }

            if (playerProgress == null)
                return;

            _lapsProgress.text = $"{playerProgress.LapsPassed}/{_raceManager.LapCount}";
        }
    }
}
