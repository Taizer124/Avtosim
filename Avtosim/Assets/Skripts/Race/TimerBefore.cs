using UnityEngine;
using TMPro;

public class RaceStartCountdown : MonoBehaviour
{
    [SerializeField]
    private RaceStartZone _raceStartZone;
    [SerializeField]
    private TextMeshProUGUI _countdownText;

    /// <summary>
    /// Тот же цифровой дисплей переиспользует финиш-зона для своего отсчёта до
    /// возврата в город — отдельного объекта под это в проекте нет.
    /// </summary>
    public TextMeshProUGUI CountdownText => _countdownText;

    private void Start()
    {
        // Цифру гасим ПЕРВЫМ делом. Раньше это стояло после проверки
        // _raceStartZone, а в префабе гоночной машины ссылка на зону сцены
        // всегда null (префаб не может ссылаться на объект сцены) — Start
        // выходил по LogError, и цифра оставалась висеть включённой с начала
        // гонки.
        if (_countdownText != null)
            _countdownText.gameObject.SetActive(false);
        else
            Debug.LogError("Countdown Text not assigned in RaceStartCountdown!");

        // Зона живёт в сцене, поэтому у заспавненного префаба ссылки нет —
        // находим её сами.
        if (_raceStartZone == null)
            _raceStartZone = FindAnyObjectByType<RaceStartZone>();

        if (_raceStartZone == null)
        {
            Debug.LogWarning("RaceStartZone не найдена — отсчёт старта показан не будет.");
            return;
        }

        // OnEnable мог отработать раньше, чем зона была найдена — подпишемся сейчас.
        _raceStartZone.OnCountdownStarted.RemoveListener(ShowCountdown);
        _raceStartZone.OnCountdownFinished.RemoveListener(HideCountdown);
        _raceStartZone.OnCountdownStarted.AddListener(ShowCountdown);
        _raceStartZone.OnCountdownFinished.AddListener(HideCountdown);
    }

    void Update()
    {
        // ��������� ����������� �������, ���� �� �������
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

    // ������ ��� �������� �� ������� RaceStartZone
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
        // ������������� �� ������� ��� ���������
        if (_raceStartZone != null)
        {
            _raceStartZone.OnCountdownStarted.AddListener(ShowCountdown);
            _raceStartZone.OnCountdownFinished.AddListener(HideCountdown);
        }
    }

    private void OnDisable()
    {
        // ������������ �� ������� ��� ����������
        if (_raceStartZone != null)
        {
            _raceStartZone.OnCountdownStarted.RemoveListener(ShowCountdown);
            _raceStartZone.OnCountdownFinished.RemoveListener(HideCountdown);
        }
    }
}