using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.InputSystem.Controls;

public class SceneStarterVR : MonoBehaviour
{
    [Header("VR Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float activationDistance = 2f;

    [Header("UI References")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingProgressBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName = "MainGameScene";
    [SerializeField] private float minLoadTime = 3f;

    private bool playerInRange = false;
    private Transform playerTransform;
    private bool isLoading = false;

    // Input System действия
    private InputAction anyButtonAction;

    private void Start()
    {
        // Скрываем UI элементы при старте
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (loadingScreen != null) loadingScreen.SetActive(false);

        // Находим игрока по тегу
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }

        SetupInputActions();
    }

    private void SetupInputActions()
    {
        // Создаем действие для любой кнопки VR контроллеров
        anyButtonAction = new InputAction("AnyVRButton", InputActionType.Button);

        // Добавляем привязки для контроллеров Quest 2
        // Правая рука
        anyButtonAction.AddBinding("<XRController>{RightHand}/{PrimaryButton}"); // A
        anyButtonAction.AddBinding("<XRController>{RightHand}/{SecondaryButton}"); // B
        anyButtonAction.AddBinding("<XRController>{RightHand}/trigger");
        anyButtonAction.AddBinding("<XRController>{RightHand}/grip");

        // Левая рука
        anyButtonAction.AddBinding("<XRController>{LeftHand}/{PrimaryButton}"); // X
        anyButtonAction.AddBinding("<XRController>{LeftHand}/{SecondaryButton}"); // Y
        anyButtonAction.AddBinding("<XRController>{LeftHand}/trigger");
        anyButtonAction.AddBinding("<XRController>{LeftHand}/grip");

        // Меню кнопки
        anyButtonAction.AddBinding("<XRController>{RightHand}/menu");
        anyButtonAction.AddBinding("<XRController>{LeftHand}/menu");

        // Подписываемся на событие
        anyButtonAction.performed += OnAnyButtonPressed;
        anyButtonAction.Enable();

        Debug.Log("VR Input actions initialized");
    }

    private void OnAnyButtonPressed(InputAction.CallbackContext context)
    {
        if (playerInRange && !isLoading)
        {
            StartGame();
        }
    }

    private void OnEnable()
    {
        anyButtonAction?.Enable();
    }

    private void OnDisable()
    {
        anyButtonAction?.Disable();
    }

    private void OnDestroy()
    {
        if (anyButtonAction != null)
        {
            anyButtonAction.performed -= OnAnyButtonPressed;
            anyButtonAction.Dispose();
        }
    }

    private void Update()
    {
        if (isLoading) return;

        CheckPlayerDistance();

        // Также проверяем клавиатуру для тестирования в редакторе
        if (playerInRange && (Keyboard.current.spaceKey.wasPressedThisFrame ||
                             Keyboard.current.enterKey.wasPressedThisFrame))
        {
            StartGame();
        }
    }

    private void CheckPlayerDistance()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool nowInRange = distance <= activationDistance;

        if (nowInRange != playerInRange)
        {
            playerInRange = nowInRange;
            UpdateInstructionUI();
        }
    }

    private void UpdateInstructionUI()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(playerInRange);
        }

        if (instructionText != null && playerInRange)
        {
            instructionText.text = "Нажми любую кнопку, чтобы начать";
        }
    }

    private void StartGame()
    {
        if (isLoading) return;

        isLoading = true;

        // Показываем экран загрузки
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (loadingScreen != null) loadingScreen.SetActive(true);

        // Запускаем асинхронную загрузку
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;

        float timer = 0f;
        float progress = 0f;

        while (!asyncLoad.isDone)
        {
            timer += Time.deltaTime;

            // Расчет прогресса с минимальным временем загрузки
            progress = Mathf.Clamp01(timer / minLoadTime);

            // Обновляем UI
            if (loadingProgressBar != null)
                loadingProgressBar.value = progress;

            if (loadingText != null)
                loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";

            // Когда загрузка почти завершена и прошло минимальное время
            if (asyncLoad.progress >= 0.9f && timer >= minLoadTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    // Визуализация зоны активации в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }

    // Для отладки - выводим информацию о доступных устройствах
    [ContextMenu("Debug Input Devices")]
    private void DebugInputDevices()
    {
        Debug.Log("Available Input Devices:");
        foreach (var device in InputSystem.devices)
        {
            Debug.Log($"- {device.name} ({device.layout})");

            // Выводим доступные контролы для VR устройств
            if (device.name.Contains("XR") || device.name.Contains("Oculus"))
            {
                foreach (var control in device.allControls)
                {
                    if (control is ButtonControl)
                    {
                        Debug.Log($"  Button: {control.displayName} -> {control.path}");
                    }
                }
            }
        }
    }
}