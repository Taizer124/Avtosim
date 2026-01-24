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
    private bool isLoading = false;

    // Input System действия
    private InputAction anyButtonAction;

    private void Start()
    {
        // Скрываем UI элементы при старте
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (loadingScreen != null) loadingScreen.SetActive(false);

        SetupInputActions();

        // Проверяем наличие коллайдера
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogError("SceneStarterVR: No collider found on this GameObject!");
        }
        else if (!collider.isTrigger)
        {
            Debug.LogWarning("SceneStarterVR: Collider is not set as trigger! Please enable 'Is Trigger' in the collider component.");
        }
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

        // Проверяем клавиатуру для тестирования в редакторе
        if (playerInRange && (Keyboard.current.spaceKey.wasPressedThisFrame ||
                             Keyboard.current.enterKey.wasPressedThisFrame))
        {
            StartGame();
        }
    }

    // Обработка входа в триггер
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            UpdateInstructionUI();
            Debug.Log("Player entered trigger zone");
        }
    }

    // Обработка выхода из триггера
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            UpdateInstructionUI();
            Debug.Log("Player exited trigger zone");
        }
    }

    private void UpdateInstructionUI()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(playerInRange);
        }

        if (instructionText != null)
        {
            instructionText.text = playerInRange ? "Нажми любую кнопку, чтобы начать" : "";
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
                loadingText.text = $"Загрузка... {Mathf.RoundToInt(progress * 100)}%";

            // Когда загрузка почти завершена и прошло минимальное время
            if (asyncLoad.progress >= 0.9f && timer >= minLoadTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    // Визуализация триггерной зоны в редакторе
    private void OnDrawGizmos()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.color = playerInRange ? new Color(0, 1, 0, 0.3f) : new Color(1, 1, 0, 0.3f);

            if (collider is CapsuleCollider capsuleCollider)
            {
                // Рисуем капсулу
                Vector3 center = transform.TransformPoint(capsuleCollider.center);
                float radius = capsuleCollider.radius * Mathf.Max(
                    transform.lossyScale.x,
                    transform.lossyScale.y,
                    transform.lossyScale.z
                );
                float height = capsuleCollider.height * transform.lossyScale.y;

                // Упрощенная визуализация капсулы как сферы
                Gizmos.DrawWireSphere(center, radius);

                // Подпись
                GUIStyle style = new GUIStyle();
                style.normal.textColor = playerInRange ? Color.green : Color.yellow;
#if UNITY_EDITOR
                UnityEditor.Handles.Label(center, "Trigger Zone", style);
#endif
            }
            else
            {
                // Для других типов коллайдеров
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
            }
        }
    }
}