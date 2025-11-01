using UnityEngine;
using Assets.VehicleController;

public class FinalLapIndicator : MonoBehaviour
{
    [Header("Race Manager")]
    [SerializeField] private RaceManager raceManager;

    [Header("Final Lap Settings")]
    [SerializeField] private GameObject objectToEnable;
    [SerializeField] private GameObject secondObjectToEnable; // Новый объект для включения
    [SerializeField] private bool enableOnFinalLap = true;
    [SerializeField] private bool disableOnRaceEnd = true;
    [SerializeField] private bool showDebugMessages = true;

    [Header("Audio/Visual Feedback")]
    [SerializeField] private AudioSource finalLapSound;
    [SerializeField] private ParticleSystem finalLapParticles;

    private RacerProgress playerProgress;
    private bool isFinalLap = false;
    private bool wasEnabled = false;

    private void Start()
    {
        // Автоматически находим RaceManager если не назначен
        if (raceManager == null)
        {
            raceManager = FindAnyObjectByType<RaceManager>();
        }

        // Отключаем объекты при старте
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(false);
        }

        // Второй объект тоже отключаем при старте
        if (secondObjectToEnable != null)
        {
            secondObjectToEnable.SetActive(false);
        }
    }

    private void Update()
    {
        if (raceManager == null || (objectToEnable == null && secondObjectToEnable == null)) return;

        // Получаем прогресс игрока
        UpdatePlayerProgress();

        // Проверяем финальный круг
        CheckFinalLap();
    }

    private void UpdatePlayerProgress()
    {
        var leaderboard = raceManager.GetLeaderboard();
        if (leaderboard == null || leaderboard.Count == 0) return;

        // Ищем прогресс игрока в лидерборде
        foreach (var progress in leaderboard)
        {
            if (progress.IsPlayer)
            {
                playerProgress = progress;
                break;
            }
        }
    }

    private void CheckFinalLap()
    {
        if (playerProgress == null) return;

        // Проверяем, является ли текущий круг финальным
        bool nowFinalLap = (playerProgress.LapsPassed == raceManager.LapCount - 1) && !playerProgress.FinishedRace;

        // Если начался финальный круг
        if (nowFinalLap && !isFinalLap)
        {
            isFinalLap = true;
            OnFinalLapStart();
        }
        // Если гонка завершена
        else if (playerProgress.FinishedRace && isFinalLap)
        {
            isFinalLap = false;
            OnRaceEnd();
        }
    }

    private void OnFinalLapStart()
    {
        // Включаем первый объект
        if (enableOnFinalLap && objectToEnable != null)
        {
            objectToEnable.SetActive(true);
            wasEnabled = true;

            if (showDebugMessages)
                Debug.Log("Final lap! Enabling object: " + objectToEnable.name);
        }

        // Включаем второй объект (всегда включается, не отключается)
        if (secondObjectToEnable != null)
        {
            secondObjectToEnable.SetActive(true);

            if (showDebugMessages)
                Debug.Log("Final lap! Enabling second object: " + secondObjectToEnable.name);
        }

        // Проигрываем звук
        if (finalLapSound != null)
        {
            finalLapSound.Play();
        }

        // Запускаем частицы
        if (finalLapParticles != null)
        {
            finalLapParticles.Play();
        }
    }

    private void OnRaceEnd()
    {
        // Отключаем только первый объект (если нужно)
        if (disableOnRaceEnd && wasEnabled && objectToEnable != null)
        {
            objectToEnable.SetActive(false);

            if (showDebugMessages)
                Debug.Log("Race ended. Disabling object: " + objectToEnable.name);
        }

        // ВТОРОЙ ОБЪЕКТ НЕ ОТКЛЮЧАЕМ - остается включенным

        // Останавливаем частицы
        if (finalLapParticles != null)
        {
            finalLapParticles.Stop();
        }
    }

    // Метод для принудительного включения/выключения
    public void ForceEnableObject()
    {
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
            wasEnabled = true;
        }

        if (secondObjectToEnable != null)
        {
            secondObjectToEnable.SetActive(true);
        }
    }

    public void ForceDisableObject()
    {
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(false);
            wasEnabled = false;
        }

        // Второй объект не отключаем принудительно, если не нужно
        // if (secondObjectToEnable != null)
        // {
        //     secondObjectToEnable.SetActive(false);
        // }
    }

    // Отдельные методы для управления вторым объектом
    public void EnableSecondObject()
    {
        if (secondObjectToEnable != null)
        {
            secondObjectToEnable.SetActive(true);
        }
    }

    public void DisableSecondObject()
    {
        if (secondObjectToEnable != null)
        {
            secondObjectToEnable.SetActive(false);
        }
    }

    // Метод для проверки текущего состояния
    public bool IsFinalLapActive()
    {
        return isFinalLap;
    }

    public int GetPlayerCurrentLap()
    {
        return playerProgress != null ? playerProgress.LapsPassed + 1 : 0;
    }

    public int GetTotalLaps()
    {
        return raceManager != null ? raceManager.LapCount : 0;
    }

    // Метод для проверки включен ли второй объект
    public bool IsSecondObjectEnabled()
    {
        return secondObjectToEnable != null && secondObjectToEnable.activeInHierarchy;
    }

    // В редакторе добавляем кнопку для тестирования
    [ContextMenu("Test Final Lap")]
    private void TestFinalLap()
    {
        OnFinalLapStart();
    }

    [ContextMenu("Test Race End")]
    private void TestRaceEnd()
    {
        OnRaceEnd();
    }

    [ContextMenu("Enable Second Object Only")]
    private void TestEnableSecondObject()
    {
        EnableSecondObject();
    }
}