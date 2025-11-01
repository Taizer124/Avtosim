using UnityEngine;
using UnityEngine.Audio;

public class MenuToggle : MonoBehaviour
{
    [Header("Menu Settings")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private bool startHidden = true;
    [SerializeField] private MenuController1 GameManager;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";

    // ссылка на DemoManager дл€ синхронизации паузы
    [SerializeField] private Assets.VehicleController.DemoManager demoManager;

    private float savedMasterVolume = 0f;
    private bool isAudioPaused = false;

    private void Start()
    {
        if (menuCanvas != null)
            menuCanvas.enabled = !startHidden;

        if (startHidden)
            LockCursor();

        SaveCurrentVolume();
    }

    private void Update()
    {
        // ESC Ч стандартное открытие меню
        // "Options" Ч кнопка на руле Logitech G29 (эмулируетс€ через KeyCode.JoystickButton9)
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton9))
            ToggleMenu();
    }

    public void ToggleMenu()
    {
        if (menuCanvas == null) return;

        bool isMenuOpening = !menuCanvas.enabled;
        menuCanvas.enabled = isMenuOpening;

        if (isMenuOpening)
        {
            PauseAudio();
            UnlockCursor();
            GameManager.Pause();

            if (demoManager != null)
                demoManager.SetPauseState(true);
        }
        else
        {
            ResumeAudio();
            LockCursor();
            GameManager.Resume();

            if (demoManager != null)
                demoManager.SetPauseState(false);
        }
    }

    private void PauseAudio()
    {
        if (audioMixer != null && !isAudioPaused)
        {
            SaveCurrentVolume();
            audioMixer.SetFloat(masterVolumeParameter, -80f);
            isAudioPaused = true;
        }
    }

    private void ResumeAudio()
    {
        if (audioMixer != null && isAudioPaused)
        {
            audioMixer.SetFloat(masterVolumeParameter, savedMasterVolume);
            isAudioPaused = false;
        }
    }

    private void SaveCurrentVolume()
    {
        if (audioMixer != null && audioMixer.GetFloat(masterVolumeParameter, out float currentVolume))
            savedMasterVolume = currentVolume;
        else
            savedMasterVolume = 0f;
    }

    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        if (isAudioPaused)
            ResumeAudio();
    }
}
