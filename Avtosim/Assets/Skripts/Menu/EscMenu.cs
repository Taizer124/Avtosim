using UnityEngine;
using UnityEngine.Audio;
using LogitechG29.Sample.Input; // добавлено для InputControllerReader

public class MenuToggle : MonoBehaviour
{
    [Header("Menu Settings")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private bool startHidden = true;
    [SerializeField] private MenuController1 GameManager;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";

    [Header("Input Settings")]
    [SerializeField] private InputControllerReader inputController; // добавлено поле для Logitech G29

    [SerializeField] private Assets.VehicleController.DemoManager demoManager;

    private float savedMasterVolume = 0f;
    private bool isAudioPaused = false;

    private void Start()
    {
        if (GameManager == null)
            GameManager = FindAnyObjectByType<MenuController1>();
        if (demoManager == null)
            demoManager = FindAnyObjectByType<Assets.VehicleController.DemoManager>();
        if (menuCanvas != null)
            menuCanvas.enabled = !startHidden;
        if (startHidden)
            LockCursor();

        SaveCurrentVolume();
    }

    private void OnEnable()
    {
        // Подписываемся на событие кнопки Options на руле
        if (inputController != null)
        {
            inputController.OnOptionsCallback += OnOptionsPressed;
        }
    }

    private void OnDisable()
    {
        // Отписываемся, чтобы не было утечек
        if (inputController != null)
        {
            inputController.OnOptionsCallback -= OnOptionsPressed;
        }
    }

    private void Update()
    {
        // ESC — стандартное открытие меню с клавиатуры
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    private void OnOptionsPressed(bool isPressed)
    {
        // Срабатывает при нажатии кнопки "Options" на руле
        if (isPressed)
        {
            Debug.Log("MenuToggle: Options button pressed on Logitech G29");
            ToggleMenu();
        }
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

        Debug.Log("MenuToggle: Menu state changed. Menu active = " + menuCanvas.enabled);
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
