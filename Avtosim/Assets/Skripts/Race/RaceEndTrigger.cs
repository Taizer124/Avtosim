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
        // ������������� ������� ������ �� ����
        FindTimerByTag();

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

    private void FindTimerByTag()
    {
        _timerObject = GameObject.FindGameObjectWithTag("FinishTimer");
        if (_timerObject != null)
        {
            _timerText = _timerObject.GetComponent<TextMeshProUGUI>();
            if (_timerText != null)
            {
                Debug.Log($"Timer found by tag 'FinishTimer': {_timerObject.name}");
                // ��������� ��������� TMPro �� ������
                _timerText.enabled = false;
            }
            else
            {
                Debug.LogWarning($"Object with tag 'FinishTimer' found but no TextMeshProUGUI component: {_timerObject.name}");
            }
        }
        else
        {
            Debug.LogWarning($"No object found with tag 'FinishTimer'");
        }
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

        // ��������� ��������� TMPro
        if (_timerText != null)
        {
            _timerText.enabled = false;
        }
    }

    private IEnumerator TimerRoutine()
    {
        _isTimerRunning = true;
        _currentTimerTime = TimerDuration;

        // �������� ��������� TMPro
        if (_timerText != null)
        {
            _timerText.enabled = true;
        }

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

        // ���������� �������
        OnTimerFinished?.Invoke();

        // ���������� ���������
        ManageObjects();

        _isTimerRunning = false;
        _timerCoroutine = null;

        // ��������� ��������� TMPro ����� ����������
        if (_timerText != null)
        {
            _timerText.enabled = false;
        }
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

    // ����� ��� ������ ������� �� ���� �������
    [ContextMenu("Find Timer by Tag")]
    public void FindTimerManually()
    {
        FindTimerByTag();
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