using UnityEngine;
using UnityEngine.UI;

namespace Assets.VehicleController
{
    public class RaceLeaderboard : MonoBehaviour
    {
        [SerializeField]
        private RaceManager _raceManager;
        [SerializeField]
        private UnityEngine.UI.Text _lapsProgress;
        private UnityEngine.UI.Text[] _racerProgressTextArray;
        [SerializeField]
        private GameObject _parent;
        [SerializeField]
        private GameObject _racerInfoPrefab;
        [SerializeField]
        private float _distanceBetweenLines;
        [SerializeField]
        private UnityEngine.UI.Text _countdownText;
        [SerializeField]
        private GameObject _countdownTMP;

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

            _racerProgressTextArray = new UnityEngine.UI.Text[_raceManager.GetLeaderboard().Count];
            float currentDist = 0;
            for (int i = 0; i < _raceManager.GetLeaderboard().Count; i++)
            {
                GameObject racerProgressInfo = Instantiate(_racerInfoPrefab);
                racerProgressInfo.transform.SetParent(_parent.transform, false);
                racerProgressInfo.transform.localPosition -= new Vector3(0, currentDist, 0);
                currentDist += _distanceBetweenLines;
                _racerProgressTextArray[i] = racerProgressInfo.GetComponent<UnityEngine.UI.Text>();

                // Если не нашли компонент на основном объекте, ищем в дочерних
                if (_racerProgressTextArray[i] == null)
                {
                    _racerProgressTextArray[i] = racerProgressInfo.GetComponentInChildren<UnityEngine.UI.Text>();
                }
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

            if (playerProgress == null)
                return;

            if (_lapsProgress != null)
            {
                _lapsProgress.text = playerProgress.LapsPassed + "/" + _raceManager.LapCount;
            }
        }
    }
}