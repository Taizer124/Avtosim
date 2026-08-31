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
    private bool _awaitingSetup = false;
    private bool _raceStarted = false;
    private Coroutine _countdownCoroutine;

    public float CurrentCountdownTime => _currentCountdownTime;
    public bool IsCountdownRunning => _isCountdownRunning;

    private void OnTriggerEnter(Collider other)
    {
        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;

        // Идёт отсчёт, открыто сетап-меню или заезд уже стартовал — повторный
        // въезд ничего не перезапускает.
        if (_isCountdownRunning || _awaitingSetup || _raceStarted)
            return;

        // Запоминаем корневой объект именно той машины, что въехала в зону —
        // её и выключим при старте гонки (какая бы из выбираемых машин это ни
        // была), вместо заранее вбитого в инспектор списка.
        _arrivalVehicle = other.transform.root.gameObject;

        // Порядок: сперва отсчёт 3-2-1-GO (TimerBefore). Если назначено pre-race
        // меню — оно откроется ПО ЗАВЕРШЕНИИ отсчёта (см. CountdownRoutine), а
        // сам матч (спавн) запустит подтверждение в меню (Confirm → StartRaceNow).
        // Без меню матч стартует сразу по концу отсчёта.
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

        _isCountdownRunning = false;
        _countdownCoroutine = null;
        OnCountdownFinished?.Invoke();

        if (preRaceMenu != null)
        {
            // Отсчёт прошёл — ставим паузу и открываем сетап-меню. Спавн матча
            // запустит PreRaceMenu.Confirm → StartRaceNow (не здесь).
            _awaitingSetup = true;
            preRaceMenu.Open(this, raceProfile);
        }
        else
        {
            // Без pre-race меню — прежнее поведение: матч сразу по концу отсчёта.
            StartRaceNow();
        }
    }

    // Запускает сам матч (спавн гоночных машин, включение объектов гонки).
    // Вызывается концом отсчёта (без меню) или подтверждением сетапа в меню.
    public void StartRaceNow()
    {
        if (_raceStarted)
            return;
        _awaitingSetup = false;
        _raceStarted = true;
        ManageObjects();
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
