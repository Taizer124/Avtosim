using Assets.VehicleController;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class RaceFinishZone : MonoBehaviour
{
    [Header("Timer Settings")]
    [Min(0)]
    public float TimerDuration = 5f;
    private float _currentTimerTime;
    private TextMeshProUGUI _timerText;
    private GameObject _timerObject;
    [Tooltip("Куда выводить отсчёт до возврата в город. Пусто — возьмётся дисплей стартового отсчёта активной машины.")]
    [SerializeField] private TextMeshProUGUI _timerTextOverride;

    [Header("Objects to Manage")]
    public List<GameObject> objectsToEnable = new List<GameObject>();
    public List<GameObject> objectsToDisable = new List<GameObject>();
    [SerializeField] private RaceSpawner _raceSpawner;

    [Header("Old Vehicle Settings")]
    [SerializeField] private GameObject _oldVehicle; // ������ �� ������ ������
    [SerializeField] private bool _enableOldVehicleOnFinish = true;

    [Header("Zone Settings")]
    public bool requirePlayerTag = true;
    public string playerTag = "Player";

    [Header("Events")]
    public UnityEvent OnTimerStarted = new UnityEvent();
    public UnityEvent OnTimerFinished = new UnityEvent();

    private bool _isTimerRunning = false;
    private bool _hasPlayerEntered = false;
    private Coroutine _timerCoroutine;

    public float CurrentTimerTime => _currentTimerTime;
    public bool IsTimerRunning => _isTimerRunning;
    public bool HasPlayerEntered => _hasPlayerEntered;

    private void Start()
    {
        // Ранний резолв — необязательный: на этот момент гоночной машины ещё
        // может не быть. Настоящий резолв происходит в StartTimer().
        ResolveTimerText();

        // ������������� ������� RaceSpawner ���� �� ��������
        if (_raceSpawner == null)
        {
            _raceSpawner = FindAnyObjectByType<RaceSpawner>();
        }

        // ������������� ������� ������ ������ �� ���� ���� �� ���������
        if (_oldVehicle == null)
        {
            GameObject oldVehicleObj = GameObject.FindGameObjectWithTag("OldVehicle");
            if (oldVehicleObj != null)
            {
                _oldVehicle = oldVehicleObj;
            }
        }
    }

    /// <summary>
    /// Ищет дисплей для отсчёта до возврата в город. Старый вариант искал ТОЛЬКО
    /// объект с тегом "FinishTimer", которого в проекте нет ни одного — поэтому
    /// цифры после финиша не появлялись вообще. Теперь порядок такой:
    ///   1) ручная ссылка в инспекторе (главнее всего);
    ///   2) объект с тегом "FinishTimer" (если когда-нибудь появится);
    ///   3) дисплей стартового отсчёта у АКТИВНОЙ машины — он живёт в её UI.
    /// Пункт 3 — рабочий путь: искать надо в момент запуска таймера, потому что
    /// на финише активна заспавненная гоночная машина, которой в Start() зоны
    /// ещё не существовало.
    /// </summary>
    private void ResolveTimerText()
    {
        if (_timerText != null)
            return;

        if (_timerTextOverride != null)
        {
            _timerText = _timerTextOverride;
            _timerObject = _timerText.gameObject;
            return;
        }

        _timerObject = GameObject.FindGameObjectWithTag("FinishTimer");
        if (_timerObject != null)
        {
            _timerText = _timerObject.GetComponent<TextMeshProUGUI>();
            if (_timerText != null)
            {
                ShowTimerText(false);
                return;
            }
        }

        // Берём дисплей стартового отсчёта у активного игрока.
        CustomVehicleController player = PlayerLocator.GetActivePlayer();
        RaceStartCountdown countdown = player != null
            ? player.GetComponentInChildren<RaceStartCountdown>(true)
            : FindAnyObjectByType<RaceStartCountdown>();

        if (countdown != null && countdown.CountdownText != null)
        {
            _timerText = countdown.CountdownText;
            _timerObject = _timerText.gameObject;
            ShowTimerText(false);
            return;
        }

        Debug.LogWarning("[RaceFinishZone] Дисплей отсчёта не найден: ни ручной ссылки, ни тега 'FinishTimer', ни RaceStartCountdown у игрока.");
    }

    /// <summary>
    /// Показать/скрыть цифру. Важно дёргать И GameObject, И компонент:
    /// RaceStartCountdown прячет свой текст через SetActive(false), поэтому
    /// одного .enabled = true недостаточно — цифра осталась бы невидимой.
    /// </summary>
    private void ShowTimerText(bool show)
    {
        if (_timerText == null)
            return;

        _timerText.gameObject.SetActive(show);
        _timerText.enabled = show;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasPlayerEntered) return; // ����� ��� �����, ���������� ��������� �����

        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;

        // �������� ��� ����� ����� � �������
        _hasPlayerEntered = true;

        // ��������� ������ ���� �� ��� �� �������
        if (!_isTimerRunning)
            StartTimer();
    }

    // ������ OnTriggerExit - ������ ���������� �������� ���� ���� ����� �����

    public void StartTimer()
    {
        if (_isTimerRunning)
            return;

        // Резолвим ИМЕННО здесь: на финише активна заспавненная гоночная машина,
        // её UI и надо использовать.
        ResolveTimerText();

        _timerCoroutine = StartCoroutine(TimerRoutine());
    }

    public void StopTimer()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
        _isTimerRunning = false;

        ShowTimerText(false);
    }

    private IEnumerator TimerRoutine()
    {
        _isTimerRunning = true;
        _currentTimerTime = TimerDuration;

        ShowTimerText(true);

        // ������ �������
        OnTimerStarted?.Invoke();

        // �������� ���� �������
        while (_currentTimerTime >= 0)
        {
            // ��������� ����������� �������
            UpdateTimerDisplay();

            _currentTimerTime -= Time.deltaTime;
            yield return null;
        }

        // Гасим цифру ДО ManageObjects: там DestroyAllVehicles уничтожает
        // гоночную машину вместе с её UI, а дисплей живёт именно в нём —
        // после уничтожения обращаться к нему уже нельзя.
        ShowTimerText(false);
        // Ссылку сбрасываем, чтобы следующий заезд нашёл дисплей заново
        // (у новой машины он будет свой).
        _timerText = null;
        _timerObject = null;

        // ���������� �������
        OnTimerFinished?.Invoke();

        // ���������� ���������
        ManageObjects();

        _isTimerRunning = false;
        _timerCoroutine = null;
    }

    private void UpdateTimerDisplay()
    {
        if (_timerText != null)
        {
            // �������� ������ ����� ������ (5, 4, 3, 2, 1)
            int displayNumber = Mathf.CeilToInt(_currentTimerTime);

            // ���������� ������ ����� ����� �� 1 �� TimerDuration
            if (displayNumber >= 1 && displayNumber <= TimerDuration)
            {
                _timerText.text = displayNumber.ToString();
            }
            else if (_currentTimerTime > 0 && _currentTimerTime < 1)
            {
                // ��������� ������� - ���������� 1
                _timerText.text = "1";
            }
            else
            {
                // ����� ����� ��� ��� �� ������� �������� ������
                _timerText.text = "";
            }

            // ������ ���� ��� ����� �������
            if (_currentTimerTime <= 3f)
            {
                _timerText.color = Color.red;
            }
            else if (_currentTimerTime <= 5f)
            {
                _timerText.color = Color.yellow;
            }
            else
            {
                _timerText.color = Color.white;
            }

            // ��������� �������� ��� ��������� ������ (�����������)
            if (_currentTimerTime <= 3f)
            {
                // ����� �������� ��������� ��� ������ ��������
                float scale = 1f + Mathf.PingPong(Time.time * 2f, 0.3f);
                _timerText.transform.localScale = Vector3.one * scale;
            }
            else
            {
                _timerText.transform.localScale = Vector3.one;
            }
        }
    }

    private void ManageObjects()
    {
        // ������� ������������ �������� ��� ���������� �������
        if (_raceSpawner != null)
        {
            // ������� ���� �����
            //_raceSpawner.DestroyBotVehicles();

            // ��� ������� ���� ������� ������
            _raceSpawner.DestroyAllVehicles();
        }

        // �������� ������ ������
        if (_enableOldVehicleOnFinish)
        {
            EnableOldVehicle();
        }

        // �������� ����������� �������
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // ��������� ��������� �������
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // ��������� ���� ���� (�����������)
        gameObject.SetActive(false);
    }

    // ����� ��� ��������� ������ ������
    public void EnableOldVehicle()
    {
        // Приоритет — та машина, на которой игрок реально приехал к старту и
        // которую RaceStartZone выключила (выбранная в меню, стоит на линии
        // старта). Только если её нет — откатываемся на фиксированный
        // _oldVehicle. Это чинит и «не та машина» (мерседес вместо порша), и
        // «не то место» (респавн уровня вместо линии старта).
        GameObject target = RaceReturnState.ArrivalVehicle != null
            ? RaceReturnState.ArrivalVehicle
            : _oldVehicle;

        if (target == null)
        {
            Debug.LogWarning("Restore vehicle reference is null (нет ни ArrivalVehicle, ни _oldVehicle)");
            return;
        }

        // �������� GameObject
        target.SetActive(true);

        // Машину выключили ещё на ходу (игрок въехал в старт-зону на скорости),
        // и Unity сохранил её линейную/угловую скорость. При включении корпус
        // «летит» вперёд, а колёса сброшены в ноль → бешеная пробуксовка:
        // VisualRPM ведущих колёс скачет к десяткам тысяч, Transmission считает
        // из него обороты двигателя → отсечка, шинный дым и неверные передачи.
        // Обнуляем скорость — машина возвращается стоящей на линии старта.
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb == null)
            targetRb = target.GetComponentInChildren<Rigidbody>();
        if (targetRb != null)
        {
            targetRb.linearVelocity = Vector3.zero;
            targetRb.angularVelocity = Vector3.zero;
        }

        // Тег Player мог «уехать» на заспавненную гоночную машину (её уже
        // уничтожил DestroyAllVehicles) — возвращаем его восстановленному авто
        // и сбрасываем кэш, иначе PlayerLocator продолжит искать уничтоженного.
        target.tag = playerTag;
        PlayerLocator.Invalidate();

        // ���������������� ������� �����
        AllInOneInputProvider inputProvider = target.GetComponent<AllInOneInputProvider>();
        if (inputProvider == null)
            inputProvider = target.GetComponentInChildren<AllInOneInputProvider>();
        if (inputProvider != null)
        {
            inputProvider.ReinitializeInputSystem();
            inputProvider.EnableInput(true); // �������� ����
            Debug.Log("Restored vehicle input system reinitialized and enabled");
        }
        else
        {
            Debug.LogWarning("No AllInOneInputProvider found on restored vehicle");
        }

        Debug.Log("Restored vehicle enabled: " + target.name);

        // Потребили ссылку — очищаем, чтобы следующий заезд не восстановил
        // устаревшую/чужую машину.
        RaceReturnState.Clear();
    }

    // ����� ��� ��������������� ��������� ������ ������ �� ������ ��������
    public void ForceEnableOldVehicle()
    {
        EnableOldVehicle();
    }

    // ����� ��� ��������� ������ �� ������ ������
    public void SetOldVehicle(GameObject oldVehicle)
    {
        _oldVehicle = oldVehicle;
    }

    // ����� ��� ������ ������ ������ �� ����
    [ContextMenu("Find Old Vehicle by Tag")]
    public void FindOldVehicleByTag()
    {
        GameObject oldVehicleObj = GameObject.FindGameObjectWithTag("OldVehicle");
        if (oldVehicleObj != null)
        {
            _oldVehicle = oldVehicleObj;
            Debug.Log($"Found old vehicle by tag 'OldVehicle': {_oldVehicle.name}");
        }
        else
        {
            Debug.LogWarning("No object found with tag 'OldVehicle'");
        }
    }

    public void ResetTimer()
    {
        StopTimer();
        _currentTimerTime = TimerDuration;
        _hasPlayerEntered = false;

        // ���������� ������� ������
        if (_timerText != null)
        {
            _timerText.transform.localScale = Vector3.one;
        }

        // �������� ���� ������� ���� ��� ���� ���������
        gameObject.SetActive(true);
    }

    // ����� ��� ��������������� ������� ������� �� ������ ��������
    public void ForceStartTimer()
    {
        _hasPlayerEntered = true;
        StartTimer();
    }

    // ����� ��� ��������� ������ ������� �������
    public void SetTimerDuration(float newDuration)
    {
        TimerDuration = Mathf.Max(0, newDuration);
        if (_isTimerRunning)
        {
            _currentTimerTime = TimerDuration;
        }
    }

    // ������ ����� ������� �������
    [ContextMenu("Find Timer Text")]
    public void FindTimerManually()
    {
        _timerText = null;
        ResolveTimerText();
    }

    // ����� ��� ��������������� ���������/���������� �������
    public void EnableTimerDisplay(bool enable)
    {
        if (_timerText != null)
        {
            _timerText.enabled = enable;
        }
    }

    // ����� ��� �������� ������ �����
    public void RemoveBotsOnly()
    {
        if (_raceSpawner != null)
        {
            _raceSpawner.DestroyBotVehicles();
        }
    }

    // ����� ��� �������� ���� ������������ �������
    public void RemoveAllVehicles()
    {
        if (_raceSpawner != null)
        {
            _raceSpawner.DestroyAllVehicles();
        }
    }

    private void OnValidate()
    {
        if (requirePlayerTag && string.IsNullOrEmpty(playerTag))
        {
            playerTag = "Player";
        }

        TimerDuration = Mathf.Max(0, TimerDuration);
    }

    // ������������ � ���������
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
    }
}