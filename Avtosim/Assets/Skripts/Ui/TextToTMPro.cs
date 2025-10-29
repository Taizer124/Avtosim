using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextToTMPConverter : MonoBehaviour
{
    [Header("Source Text Component")]
    [SerializeField] private Text sourceText;

    [Header("Target Components")]
    [SerializeField] private TextMeshProUGUI targetTMP;
    [SerializeField] private TextMesh targetTM;

    [Header("Search by Tag (if components not assigned)")]
    [SerializeField] private string sourceTextTag = "";
    [SerializeField] private string targetTMPTag = "";
    [SerializeField] private string targetTMTag = "";

    [Header("Update Settings")]
    [SerializeField] private bool updateContinuously = false;
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private bool updateBothTargets = true;

    private string lastText = "";
    private float timer = 0f;

    private void Start()
    {
        FindComponentsByTagIfNeeded();
        InitializeComponents();
    }

    private void FindComponentsByTagIfNeeded()
    {
        // Поиск sourceText по тегу если не назначен
        if (sourceText == null && !string.IsNullOrEmpty(sourceTextTag))
        {
            GameObject sourceObject = GameObject.FindGameObjectWithTag(sourceTextTag);
            if (sourceObject != null)
            {
                sourceText = sourceObject.GetComponent<Text>();
                if (sourceText != null)
                {
                    Debug.Log($"Found source Text by tag '{sourceTextTag}': {sourceObject.name}", this);
                }
                else
                {
                    Debug.LogWarning($"Found object with tag '{sourceTextTag}' but no Text component: {sourceObject.name}", this);
                }
            }
            else
            {
                Debug.LogWarning($"No object found with tag '{sourceTextTag}' for source Text", this);
            }
        }

        // Поиск targetTMP по тегу если не назначен
        if (targetTMP == null && !string.IsNullOrEmpty(targetTMPTag))
        {
            GameObject targetObject = GameObject.FindGameObjectWithTag(targetTMPTag);
            if (targetObject != null)
            {
                targetTMP = targetObject.GetComponent<TextMeshProUGUI>();
                if (targetTMP != null)
                {
                    Debug.Log($"Found target TMP by tag '{targetTMPTag}': {targetObject.name}", this);
                }
                else
                {
                    Debug.LogWarning($"Found object with tag '{targetTMPTag}' but no TextMeshProUGUI component: {targetObject.name}", this);
                }
            }
            else
            {
                Debug.LogWarning($"No object found with tag '{targetTMPTag}' for target TMP", this);
            }
        }

        // Поиск targetTM по тегу если не назначен
        if (targetTM == null && !string.IsNullOrEmpty(targetTMTag))
        {
            GameObject targetObject = GameObject.FindGameObjectWithTag(targetTMTag);
            if (targetObject != null)
            {
                targetTM = targetObject.GetComponent<TextMesh>();
                if (targetTM != null)
                {
                    Debug.Log($"Found target TextMesh by tag '{targetTMTag}': {targetObject.name}", this);
                }
                else
                {
                    Debug.LogWarning($"Found object with tag '{targetTMTag}' but no TextMesh component: {targetObject.name}", this);
                }
            }
            else
            {
                Debug.LogWarning($"No object found with tag '{targetTMTag}' for target TextMesh", this);
            }
        }
    }

    private void InitializeComponents()
    {
        // Автоматически находим компоненты если не назначены (старая логика)
        if (sourceText == null)
        {
            sourceText = GetComponent<Text>();
            if (sourceText == null)
            {
                // Ищем среди дочерних объектов
                sourceText = GetComponentInChildren<Text>();
                if (sourceText == null)
                {
                    Debug.LogError("No Text component found!", this);
                    return;
                }
            }
        }

        // Инициализация targetTMP (если нужен)
        if (targetTMP == null)
        {
            targetTMP = GetComponent<TextMeshProUGUI>();
            if (targetTMP == null)
            {
                // Ищем среди дочерних объектов
                targetTMP = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        // Инициализация targetTM (если нужен)
        if (targetTM == null)
        {
            targetTM = GetComponent<TextMesh>();
            if (targetTM == null)
            {
                // Ищем среди дочерних объектов
                targetTM = GetComponentInChildren<TextMesh>();
            }
        }

        // Проверяем что есть хотя бы один целевой компонент
        if (targetTMP == null && targetTM == null)
        {
            Debug.LogError("No target component found! Assign either TextMeshProUGUI or TextMesh.", this);
            return;
        }

        // Первоначальное копирование
        CopyTextToTargets();
    }

    private void Update()
    {
        if (!updateContinuously || sourceText == null)
            return;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            CopyTextToTargets();
        }
    }

    public void CopyTextToTargets()
    {
        if (sourceText == null)
            return;

        // Копируем только если текст изменился
        if (sourceText.text != lastText)
        {
            lastText = sourceText.text;

            // Копируем в TMP если назначен
            if (targetTMP != null)
            {
                targetTMP.text = sourceText.text;
                targetTMP.color = sourceText.color;
                targetTMP.alignment = ConvertTextAlignmentToTMP(sourceText.alignment);
            }

            // Копируем в TextMesh если назначен
            if (targetTM != null)
            {
                targetTM.text = sourceText.text;
                targetTM.color = sourceText.color;
                targetTM.anchor = ConvertTextAnchorToTextMesh(sourceText.alignment);
                targetTM.alignment = ConvertTextAlignmentToTextMesh(sourceText.alignment);
            }
        }
    }

    private TextAlignmentOptions ConvertTextAlignmentToTMP(TextAnchor textAnchor)
    {
        // Конвертируем TextAnchor в TextAlignmentOptions
        switch (textAnchor)
        {
            case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
            case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
            default: return TextAlignmentOptions.Center;
        }
    }

    private TextAnchor ConvertTextAnchorToTextMesh(TextAnchor textAnchor)
    {
        // TextMesh использует тот же TextAnchor что и UI Text
        return textAnchor;
    }

    private TextAlignment ConvertTextAlignmentToTextMesh(TextAnchor textAnchor)
    {
        // Конвертируем TextAnchor в TextAlignment для TextMesh
        switch (textAnchor)
        {
            case TextAnchor.UpperLeft: return TextAlignment.Left;
            case TextAnchor.UpperCenter: return TextAlignment.Center;
            case TextAnchor.UpperRight: return TextAlignment.Right;
            case TextAnchor.MiddleLeft: return TextAlignment.Left;
            case TextAnchor.MiddleCenter: return TextAlignment.Center;
            case TextAnchor.MiddleRight: return TextAlignment.Right;
            case TextAnchor.LowerLeft: return TextAlignment.Left;
            case TextAnchor.LowerCenter: return TextAlignment.Center;
            case TextAnchor.LowerRight: return TextAlignment.Right;
            default: return TextAlignment.Center;
        }
    }

    // Метод для ручного обновления из других скриптов
    [ContextMenu("Update All Targets")]
    public void ManualUpdate()
    {
        CopyTextToTargets();
    }

    [ContextMenu("Update TMP Only")]
    public void ManualUpdateTMP()
    {
        if (sourceText != null && targetTMP != null)
        {
            targetTMP.text = sourceText.text;
            targetTMP.color = sourceText.color;
            targetTMP.alignment = ConvertTextAlignmentToTMP(sourceText.alignment);
        }
    }

    [ContextMenu("Update TextMesh Only")]
    public void ManualUpdateTextMesh()
    {
        if (sourceText != null && targetTM != null)
        {
            targetTM.text = sourceText.text;
            targetTM.color = sourceText.color;
            targetTM.anchor = ConvertTextAnchorToTextMesh(sourceText.alignment);
            targetTM.alignment = ConvertTextAlignmentToTextMesh(sourceText.alignment);
        }
    }

    // Метод для установки нового источника Text
    public void SetSourceText(Text newSourceText)
    {
        sourceText = newSourceText;
        CopyTextToTargets();
    }

    // Метод для установки нового целевого TMP
    public void SetTargetTMP(TextMeshProUGUI newTargetTMP)
    {
        targetTMP = newTargetTMP;
        CopyTextToTargets();
    }

    // Метод для установки нового целевого TextMesh
    public void SetTargetTM(TextMesh newTargetTM)
    {
        targetTM = newTargetTM;
        CopyTextToTargets();
    }

    // Метод для поиска компонентов по тегам вручную
    [ContextMenu("Find Components by Tags")]
    public void FindComponentsByTags()
    {
        FindComponentsByTagIfNeeded();
        bool hasAnyTarget = targetTMP != null || targetTM != null;

        if (sourceText != null && hasAnyTarget)
        {
            CopyTextToTargets();
            Debug.Log("Components found and updated successfully!", this);
        }
        else
        {
            Debug.LogWarning("Could not find all required components!", this);
        }
    }

    // Метод для включения/выключения обновления определенных целей
    public void SetUpdateTMP(bool update)
    {
        if (targetTMP != null)
        {
            // Можно добавить логику для временного отключения TMP
        }
    }

    public void SetUpdateTextMesh(bool update)
    {
        if (targetTM != null)
        {
            // Можно добавить логику для временного отключения TextMesh
        }
    }

    // В редакторе автоматически обновляем при изменении в инспекторе
    private void OnValidate()
    {
        if (sourceText != null && (targetTMP != null || targetTM != null))
        {
            CopyTextToTargets();
        }
    }

    // Получение статуса компонентов
    public bool HasSourceText => sourceText != null;
    public bool HasTargetTMP => targetTMP != null;
    public bool HasTargetTextMesh => targetTM != null;
    public bool HasAnyTarget => targetTMP != null || targetTM != null;
}