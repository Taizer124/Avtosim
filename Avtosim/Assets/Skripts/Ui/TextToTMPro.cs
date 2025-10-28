using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextToTMPConverter : MonoBehaviour
{
    [Header("Source Text Component")]
    [SerializeField] private Text sourceText;

    [Header("Target TMP Component")]
    [SerializeField] private TextMeshProUGUI targetTMP;

    [Header("Search by Tag (if components not assigned)")]
    [SerializeField] private string sourceTextTag = "";
    [SerializeField] private string targetTMPTag = "";

    [Header("Update Settings")]
    [SerializeField] private bool updateContinuously = false;
    [SerializeField] private float updateInterval = 0.1f;

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

        if (targetTMP == null)
        {
            targetTMP = GetComponent<TextMeshProUGUI>();
            if (targetTMP == null)
            {
                // Ищем среди дочерних объектов
                targetTMP = GetComponentInChildren<TextMeshProUGUI>();
                if (targetTMP == null)
                {
                    Debug.LogError("No TextMeshProUGUI component found!", this);
                    return;
                }
            }
        }

        // Первоначальное копирование
        CopyTextToTMP();
    }

    private void Update()
    {
        if (!updateContinuously || sourceText == null || targetTMP == null)
            return;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            CopyTextToTMP();
        }
    }

    public void CopyTextToTMP()
    {
        if (sourceText == null || targetTMP == null)
            return;

        // Копируем только если текст изменился
        if (sourceText.text != lastText)
        {
            targetTMP.text = sourceText.text;
            lastText = sourceText.text;

            // Дополнительно копируем некоторые свойства
            targetTMP.color = sourceText.color;
            targetTMP.alignment = ConvertTextAlignmentToTMP(sourceText.alignment);
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

    // Метод для ручного обновления из других скриптов
    [ContextMenu("Update TMP Text")]
    public void ManualUpdate()
    {
        CopyTextToTMP();
    }

    // Метод для установки нового источника Text
    public void SetSourceText(Text newSourceText)
    {
        sourceText = newSourceText;
        CopyTextToTMP();
    }

    // Метод для установки нового целевого TMP
    public void SetTargetTMP(TextMeshProUGUI newTargetTMP)
    {
        targetTMP = newTargetTMP;
        CopyTextToTMP();
    }

    // Метод для поиска компонентов по тегам вручную
    [ContextMenu("Find Components by Tags")]
    public void FindComponentsByTags()
    {
        FindComponentsByTagIfNeeded();
        if (sourceText != null && targetTMP != null)
        {
            CopyTextToTMP();
            Debug.Log("Components found and updated successfully!", this);
        }
    }

    // В редакторе автоматически обновляем при изменении в инспекторе
    private void OnValidate()
    {
        if (sourceText != null && targetTMP != null)
        {
            CopyTextToTMP();
        }
    }
}