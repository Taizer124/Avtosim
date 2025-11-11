using UnityEngine;
using Assets.VehicleController;
using System.Collections;

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
    [SerializeField] private int delayBeforeEnableSeconds = 0; // новая настройка задержки

    [Header("Audio/Visual Feedback")]
    [SerializeField] private AudioSource finalLapSound;
    [SerializeField] private ParticleSystem finalLapParticles;

    private RacerProgress playerProgress;
    private bool isFinalLap = false;
    private bool wasEnabled = false;

    private void Start()
    {
        if (raceManager == null)
        {
            raceManager = FindAnyObjectByType<RaceManager>();
        }

        if (objectToEnable != null)
            objectToEnable.SetActive(false);

        if (secondObjectToEnable != null)
            secondObjectToEnable.SetActive(false);
    }

    private void Update()
    {
        if (raceManager == null || (objectToEnable == null && secondObjectToEnable == null)) return;

        UpdatePlayerProgress();
        CheckFinalLap();
    }

    private void UpdatePlayerProgress()
    {
        var leaderboard = raceManager.GetLeaderboard();
        if (leaderboard == null || leaderboard.Count == 0) return;

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

        bool nowFinalLap = (playerProgress.LapsPassed == raceManager.LapCount - 1) && !playerProgress.FinishedRace;

        if (nowFinalLap && !isFinalLap)
        {
            isFinalLap = true;
            StartCoroutine(OnFinalLapStartDelayed());
        }
        else if (playerProgress.FinishedRace && isFinalLap)
        {
            isFinalLap = false;
            OnRaceEnd();
        }
    }

    // Новый метод с задержкой
    private IEnumerator OnFinalLapStartDelayed()
    {
        if (delayBeforeEnableSeconds > 0)
        {
            if (showDebugMessages)
                Debug.Log($"Final lap detected! Waiting {delayBeforeEnableSeconds} seconds before activation...");
            yield return new WaitForSeconds(delayBeforeEnableSeconds);
        }

        OnFinalLapStart();
    }

    private void OnFinalLapStart()
    {
        if (enableOnFinalLap && objectToEnable != null)
        {
            objectToEnable.SetActive(true);
            wasEnabled = true;

            if (showDebugMessages)
                Debug.Log("Final lap! Enabling object: " + objectToEnable.name);
        }

        if (secondObjectToEnable != null)
        {
            secondObjectToEnable.SetActive(true);

            if (showDebugMessages)
                Debug.Log("Final lap! Enabling second object: " + secondObjectToEnable.name);
        }

        if (finalLapSound != null)
            finalLapSound.Play();

        if (finalLapParticles != null)
            finalLapParticles.Play();
    }

    private void OnRaceEnd()
    {
        if (disableOnRaceEnd && wasEnabled && objectToEnable != null)
        {
            objectToEnable.SetActive(false);

            if (showDebugMessages)
                Debug.Log("Race ended. Disabling object: " + objectToEnable.name);
        }

        if (finalLapParticles != null)
            finalLapParticles.Stop();
    }

    public void ForceEnableObject()
    {
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
            wasEnabled = true;
        }

        if (secondObjectToEnable != null)
            secondObjectToEnable.SetActive(true);
    }

    public void ForceDisableObject()
    {
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(false);
            wasEnabled = false;
        }
    }

    public void EnableSecondObject()
    {
        if (secondObjectToEnable != null)
            secondObjectToEnable.SetActive(true);
    }

    public void DisableSecondObject()
    {
        if (secondObjectToEnable != null)
            secondObjectToEnable.SetActive(false);
    }

    public bool IsFinalLapActive() => isFinalLap;
    public int GetPlayerCurrentLap() => playerProgress != null ? playerProgress.LapsPassed + 1 : 0;
    public int GetTotalLaps() => raceManager != null ? raceManager.LapCount : 0;
    public bool IsSecondObjectEnabled() => secondObjectToEnable != null && secondObjectToEnable.activeInHierarchy;

    [ContextMenu("Test Final Lap")]
    private void TestFinalLap() => OnFinalLapStart();

    [ContextMenu("Test Race End")]
    private void TestRaceEnd() => OnRaceEnd();

    [ContextMenu("Enable Second Object Only")]
    private void TestEnableSecondObject() => EnableSecondObject();
}
