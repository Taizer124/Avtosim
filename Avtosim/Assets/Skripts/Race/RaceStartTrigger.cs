using Assets.VehicleController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [Header("Leaderboard Object")]
    [Tooltip("������, �� ������� ��������� ������ RaceLeaderboard")]
    public GameObject leaderboardObject;

    [Header("Zone Settings")]
    public bool requirePlayerTag = true;
    public string playerTag = "Player";

    [Header("Arrival Vehicle")]
    [Tooltip("Выключать машину, на которой игрок реально приехал к старту (её сам объект, а не фиксированный список). Убирает дубль-игрока и десинхрон ввода с заспавненной гоночной машиной.")]
    public bool disableArrivalVehicleOnStart = true;
    private GameObject _arrivalVehicle;

    [Header("Pre-Race Menu (опционально)")]
    [Tooltip("Если задано — при въезде откроется сетап-меню, а обратный отсчёт стартует только после подтверждения (меню вызовет StartCountdown). Пусто — гонка стартует сразу, как раньше.")]
    public PreRaceMenu preRaceMenu;
    [Tooltip("Профиль этой гонки: имя + рекомендованные коробка/привод для pre-race меню.")]
    public RaceProfile raceProfile;

    [Header("Events")]
    public UnityEvent OnCountdownStarted = new UnityEvent();
    public UnityEvent OnCountdownFinished = new UnityEvent();

    private bool _isCountdownRunning = false;
    private Coroutine _countdownCoroutine;

    public float CurrentCountdownTime => _currentCountdownTime;
    public bool IsCountdownRunning => _isCountdownRunning;

    private void OnTriggerEnter(Collider other)
    {
        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;

        // Уже идёт отсчёт (игрок подтвердил сетап и заезд стартует) — повторный
        // въезд не должен ни открывать меню заново, ни рестартовать отсчёт.
        if (_isCountdownRunning)
            return;

        // Запоминаем корневой объект именно той машины, что въехала в зону —
        // её и выключим при старте гонки (какая бы из выбираемых машин это ни
        // была), вместо заранее вбитого в инспектор списка.
        _arrivalVehicle = other.transform.root.gameObject;

        // Если назначено pre-race меню — сперва сетап заезда, а отсчёт запустит
        // само меню по кнопке подтверждения (PreRaceMenu.Confirm → StartCountdown).
        if (preRaceMenu != null)
        {
            preRaceMenu.Open(this, raceProfile);
            return;
        }

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

        OnCountdownStarted?.Invoke();

        while (_currentCountdownTime >= 0)
        {
            _currentCountdownTime -= Time.deltaTime;
            yield return null;
        }

        OnCountdownFinished?.Invoke();
        ManageObjects();

        _isCountdownRunning = false;
        _countdownCoroutine = null;
    }

    private void ManageObjects()
    {
        // �������� ����������� �������
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // ���������� RaceLeaderboard, ���� �� ������
        if (leaderboardObject != null)
        {
            leaderboardObject.SetActive(true);

            var leaderboard = leaderboardObject.GetComponent<RaceLeaderboard>();
            if (leaderboard != null)
                leaderboard.enabled = true;
            else
                Debug.LogWarning($"�� ������� {leaderboardObject.name} �� ������ ������ RaceLeaderboard!");
        }

        // ��������� ��������� �������
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Выключаем именно ту машину, на которой игрок приехал к старту —
        // иначе она остаётся активной параллельно с заспавненной гоночной,
        // обе читают ввод, и получается рассинхрон (передачи меняются на
        // одной машине, едет другая).
        if (disableArrivalVehicleOnStart && _arrivalVehicle != null)
        {
            // Передаём приехавшую машину финиш-зоне, чтобы после гонки включить
            // обратно ИМЕННО её (выбранное авто) и ИМЕННО тут, на линии старта,
            // где она сейчас стоит выключенной. Кладём ДО SetActive(false).
            RaceReturnState.ArrivalVehicle = _arrivalVehicle;

            _arrivalVehicle.SetActive(false);

            // Игрок сменился (приехавшая машина выключена, активна заспавненная
            // гоночная) — сбрасываем кэш, иначе PlayerLocator продолжит отдавать
            // выключенную машину (её CVC не уничтожен, а лишь неактивен).
            PlayerLocator.Invalidate();
        }

        // ��������� ���� ���� ����� ������
        gameObject.SetActive(false);
    }

    public void ResetCountdown()
    {
        StopCountdown();
        _currentCountdownTime = CountdownTime;
    }
}
