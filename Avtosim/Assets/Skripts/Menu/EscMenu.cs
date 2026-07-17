using UnityEngine;
using UnityEngine.Audio;
using LogitechG29.Sample.Input; // ��������� ��� InputControllerReader
using Assets.VehicleController;
public class MenuToggle : MonoBehaviour
{
    [Header("Menu Settings")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private bool startHidden = true;
    [SerializeField] private MenuController1 GameManager;
    [SerializeField] private CustomVehicleController CustomVehicleController1;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";

    [Header("Input Settings")]
    [SerializeField] private InputControllerReader inputController; // ��������� ���� ��� Logitech G29

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
        if (CustomVehicleController1 ==  null)
        {
            CustomVehicleController1 = PlayerLocator.GetActivePlayer();
        }
        if (startHidden)
            //LockCursor();

        SaveCurrentVolume();
    }

    private void OnEnable()
    {
        // ������������� �� ������� ������ Options �� ����
        if (inputController != null)
        {
            inputController.OnOptionsCallback += OnOptionsPressed;
        }
    }

    private void OnDisable()
    {
        // ������������, ����� �� ���� ������
        if (inputController != null)
        {
            inputController.OnOptionsCallback -= OnOptionsPressed;
        }
    }

    private void Update()
    {
        // ESC � ����������� �������� ���� � ����������
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    private void OnOptionsPressed(bool isPressed)
    {
        // ����������� ��� ������� ������ "Options" �� ����
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

        // Игрок мог смениться (выбор машины в меню) — если ссылка пустая,
        // берём активного игрока по тегу. Плюс null-guard: раньше обращение
        // к .enabled на несуществующей машине давало NullReferenceException.
        if (CustomVehicleController1 == null)
            CustomVehicleController1 = PlayerLocator.GetActivePlayer();

        if (isMenuOpening)
        {
            PauseAudio();
            //UnlockCursor();
            if (CustomVehicleController1 != null)
                CustomVehicleController1.enabled = false;
            if (GameManager != null)
                GameManager.Pause();

            if (demoManager != null)
                demoManager.SetPauseState(true);
        }
        else
        {
            ResumeAudio();
            //LockCursor();
            if (CustomVehicleController1 != null)
                CustomVehicleController1.enabled = true;
            if (GameManager != null)
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

    //private void UnlockCursor()
    //{
    //    Cursor.visible = true;
    //    Cursor.lockState = CursorLockMode.None;
    //}

    //private void LockCursor()
    //{
    //    Cursor.visible = false;
    //    Cursor.lockState = CursorLockMode.Locked;
    //}

    private void OnDestroy()
    {
        if (isAudioPaused)
            ResumeAudio();
    }
}
