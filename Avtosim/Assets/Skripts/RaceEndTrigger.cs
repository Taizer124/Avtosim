using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;

public class RaceFinishZone : MonoBehaviour
{
    [Header("Timer Settings")]
    [Min(0)]
    public float TimerDuration = 10f;
    private float _currentTimerTime;
    private TextMeshProUGUI _timerText;

    [Header("Objects to Manage")]
    public List<GameObject> objectsToEnable = new List<GameObject>();
    public List<GameObject> objectsToDisable = new List<GameObject>();

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
        // Автоматически находим таймер по тегу
        FindTimerByTag();
    }

    private void FindTimerByTag()
    {
        GameObject timerObject = GameObject.FindGameObjectWithTag("FinishTimer");
        if (timerObject != null)
        {
            _timerText = timerObject.GetComponent<TextMeshProUGUI>();
            if (_timerText != null)
            {
                Debug.Log($"Timer found by tag 'FinishTimer': {timerObject.name}");
                // Скрываем таймер до старта
                _timerText.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"Object with tag 'FinishTimer' found but no TextMeshProUGUI component: {timerObject.name}");
            }
        }
        else
        {
            Debug.LogWarning($"No object found with tag 'FinishTimer'");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasPlayerEntered) return; // Игрок уже вошел, игнорируем повторные входы

        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;

        // Отмечаем что игрок вошел в триггер
        _hasPlayerEntered = true;

        // Запускаем таймер если он еще не запущен
        if (!_isTimerRunning)
            StartTimer();
    }

    // Убрали OnTriggerExit - таймер продолжает работать даже если игрок вышел

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

        // Скрываем таймер
        if (_timerText != null)
        {
            _timerText.gameObject.SetActive(false);
        }
    }

    private IEnumerator TimerRoutine()
    {
        _isTimerRunning = true;
        _currentTimerTime = TimerDuration;

        // Показываем UI таймер
        if (_timerText != null)
        {
            _timerText.gameObject.SetActive(true);
        }

        // Запуск событий
        OnTimerStarted?.Invoke();

        // Основной цикл отсчета
        while (_currentTimerTime >= 0)
        {
            // Обновляем отображение таймера
            UpdateTimerDisplay();

            _currentTimerTime -= Time.deltaTime;
            yield return null;
        }

        // Завершение таймера
        OnTimerFinished?.Invoke();

        // Управление объектами
        ManageObjects();

        _isTimerRunning = false;
        _timerCoroutine = null;

        // Скрываем таймер после завершения
        if (_timerText != null)
        {
            _timerText.gameObject.SetActive(false);
        }
    }

    private void UpdateTimerDisplay()
    {
        if (_timerText != null)
        {
            // Форматируем время: 00:00
            int minutes = Mathf.FloorToInt(_currentTimerTime / 60f);
            int seconds = Mathf.FloorToInt(_currentTimerTime % 60f);
            _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Меняем цвет при малом времени (опционально)
            if (_currentTimerTime <= 5f)
            {
                _timerText.color = Color.red;
            }
            else if (_currentTimerTime <= 10f)
            {
                _timerText.color = Color.yellow;
            }
            else
            {
                _timerText.color = Color.white;
            }
        }
    }

    private void ManageObjects()
    {
        // Включаем необходимые объекты
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // Выключаем указанные объекты
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Выключаем саму зону (опционально)
        gameObject.SetActive(false);
    }

    public void ResetTimer()
    {
        StopTimer();
        _currentTimerTime = TimerDuration;
        _hasPlayerEntered = false;

        // Включаем зону обратно если она была выключена
        gameObject.SetActive(true);
    }

    // Метод для принудительного запуска таймера из других скриптов
    public void ForceStartTimer()
    {
        _hasPlayerEntered = true;
        StartTimer();
    }

    // Метод для установки нового времени таймера
    public void SetTimerDuration(float newDuration)
    {
        TimerDuration = Mathf.Max(0, newDuration);
        if (_isTimerRunning)
        {
            _currentTimerTime = TimerDuration;
        }
    }

    // Метод для поиска таймера по тегу вручную
    [ContextMenu("Find Timer by Tag")]
    public void FindTimerManually()
    {
        FindTimerByTag();
    }

    private void OnValidate()
    {
        if (requirePlayerTag && string.IsNullOrEmpty(playerTag))
        {
            playerTag = "Player";
        }

        TimerDuration = Mathf.Max(0, TimerDuration);
    }

    // Визуализация в редакторе
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