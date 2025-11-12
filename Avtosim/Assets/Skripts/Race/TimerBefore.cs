using UnityEngine;
using TMPro;

public class RaceStartCountdown : MonoBehaviour
{
    [SerializeField]
    private RaceStartZone _raceStartZone;
    [SerializeField]
    private TextMeshProUGUI _countdownText;

    private void Start()
    {
        // Проверяем ссылки
        if (_raceStartZone == null)
        {
            Debug.LogError("RaceStartZone not assigned in RaceStartCountdown!");
            return;
        }

        if (_countdownText == null)
        {
            Debug.LogError("Countdown Text not assigned in RaceStartCountdown!");
            return;
        }

        // Скрываем текст при старте
        _countdownText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Обновляем отображение таймера, если он активен
        if (_raceStartZone != null && _raceStartZone.IsCountdownRunning)
        {
            UpdateCountdownDisplay();
        }
    }

    private void UpdateCountdownDisplay()
    {
        float timeLeft = _raceStartZone.CurrentCountdownTime;
        int countdown = Mathf.CeilToInt(timeLeft);

        if (countdown > 0)
        {
            _countdownText.text = countdown.ToString();
        }
        else
        {
            _countdownText.text = "GO!";
        }
    }

    // Методы для подписки на события RaceStartZone
    public void ShowCountdown()
    {
        _countdownText.gameObject.SetActive(true);
    }

    public void HideCountdown()
    {
        _countdownText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Подписываемся на события при включении
        if (_raceStartZone != null)
        {
            _raceStartZone.OnCountdownStarted.AddListener(ShowCountdown);
            _raceStartZone.OnCountdownFinished.AddListener(HideCountdown);
        }
    }

    private void OnDisable()
    {
        // Отписываемся от событий при выключении
        if (_raceStartZone != null)
        {
            _raceStartZone.OnCountdownStarted.RemoveListener(ShowCountdown);
            _raceStartZone.OnCountdownFinished.RemoveListener(HideCountdown);
        }
    }
}