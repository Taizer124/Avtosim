using UnityEngine;

// Добавляем директиву для TextMeshPro
#if TMP_PRESENT
using TMPro;
#endif

namespace Assets.VehicleController
{
    public class RaceLeaderboard : MonoBehaviour
    {
        [SerializeField]
        private RaceManager _raceManager;

        // Объявляем поля с условной компиляцией
#if TMP_PRESENT
        [SerializeField] private TextMeshProUGUI _lapsProgress;
        [SerializeField] private TextMeshProUGUI _countdownText;
        private TextMeshProUGUI[] _racerProgressTextArray;
#else
        [SerializeField] private UnityEngine.UI.Text _lapsProgress;
        [SerializeField] private UnityEngine.UI.Text _countdownText;
        private UnityEngine.UI.Text[] _racerProgressTextArray;
#endif

        [SerializeField] private GameObject _parent;
        [SerializeField] private GameObject _racerInfoPrefab;
        [SerializeField] private float _distanceBetweenLines;
        [SerializeField] private GameObject _countdownTMP;

        private void CreateLeaderboard()
        {
            if (_racerProgressTextArray != null && _racerProgressTextArray.Length > 0)
            {
                for (int i = 0; i < _racerProgressTextArray.Length; i++)
                {
                    if (_racerProgressTextArray[i] != null)
                        Destroy(_racerProgressTextArray[i].gameObject);
                }
            }

#if TMP_PRESENT
            _racerProgressTextArray = new TextMeshProUGUI[_raceManager.GetLeaderboard().Count];
#else
            _racerProgressTextArray = new UnityEngine.UI.Text[_raceManager.GetLeaderboard().Count];
#endif

            float currentDist = 0;
            for (int i = 0; i < _raceManager.GetLeaderboard().Count; i++)
            {
                GameObject racerProgressInfo = Instantiate(_racerInfoPrefab);
                racerProgressInfo.transform.SetParent(_parent.transform, false);
                racerProgressInfo.transform.localPosition -= new Vector3(0, currentDist, 0);
                currentDist += _distanceBetweenLines;

#if TMP_PRESENT
                _racerProgressTextArray[i] = racerProgressInfo.GetComponent<TextMeshProUGUI>();
                if (_racerProgressTextArray[i] == null)
                    _racerProgressTextArray[i] = racerProgressInfo.GetComponentInChildren<TextMeshProUGUI>();
#else
                _racerProgressTextArray[i] = racerProgressInfo.GetComponent<UnityEngine.UI.Text>();
                if (_racerProgressTextArray[i] == null)
                    _racerProgressTextArray[i] = racerProgressInfo.GetComponentInChildren<UnityEngine.UI.Text>();
#endif
            }
        }

        void Update()
        {
            int countdown = Mathf.CeilToInt(_raceManager.StartCountdownTimeLeft);
            if (countdown > 0)
            {
                _countdownText.text = countdown.ToString();
            }
            else
            {
                _countdownText.gameObject.SetActive(false);
                if (_countdownTMP != null)
                    _countdownTMP.SetActive(false);
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

                if (_racerProgressTextArray[i] != null)
                {
                    _racerProgressTextArray[i].text = (i + 1) + " - " + racerProgress.RacerName;
                }
            }

            if (playerProgress == null) return;

            if (_lapsProgress != null)
            {
                _lapsProgress.text = playerProgress.LapsPassed + "/" + _raceManager.LapCount;
            }
        }
    }
}