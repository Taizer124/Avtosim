using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class RaceStartZone : MonoBehaviour
{
    [Header("Countdown Settings")]
    [Min(0)]
    public float CountdownTime = 3f;
    private float _currentCountdownTime;

    [Header("Objects to Manage")]
    public List<GameObject> objectsToEnable = new List<GameObject>();
    public List<GameObject> objectsToDisable = new List<GameObject>();

    [Header("Zone Settings")]
    public bool requirePlayerTag = true;
    public string playerTag = "Player";

    [Header("Events")]
    public UnityEvent OnCountdownStarted = new UnityEvent();    // ДОБАВЛЕНО
    public UnityEvent OnCountdownFinished = new UnityEvent();   // ДОБАВЛЕНО

    private bool _isCountdownRunning = false;
    private Coroutine _countdownCoroutine;

    public float CurrentCountdownTime => _currentCountdownTime;
    public bool IsCountdownRunning => _isCountdownRunning;

    private void OnTriggerEnter(Collider other)
    {
        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;

        if (!_isCountdownRunning)
            StartCountdown();
    }

    private void OnTriggerExit(Collider other)
    {
        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;

        if (_isCountdownRunning)
            StopCountdown();
    }

    public void StartCountdown()
    {
        if (_isCountdownRunning)
            return;

        _countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    public void StopCountdown()
    {
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
            OnCountdownFinished?.Invoke();
        }
        _isCountdownRunning = false;
    }

    private IEnumerator CountdownRoutine()
    {
        _isCountdownRunning = true;
        _currentCountdownTime = CountdownTime;

        // Запуск UI таймера
        OnCountdownStarted?.Invoke();

        // Основной цикл отсчета
        while (_currentCountdownTime >= 0)
        {
            _currentCountdownTime -= Time.deltaTime;
            yield return null;
        }

        // Завершение таймера
        OnCountdownFinished?.Invoke();

        // Управление объектами
        ManageObjects();

        _isCountdownRunning = false;
        _countdownCoroutine = null;
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

        // Выключаем саму зону
        gameObject.SetActive(false);
    }

    public void ResetCountdown()
    {
        StopCountdown();
        _currentCountdownTime = CountdownTime;
    }
}