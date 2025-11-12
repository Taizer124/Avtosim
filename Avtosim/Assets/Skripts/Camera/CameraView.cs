using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Управляет VR- и spectator-камерами, добавляя плавное затемнение при смене режимов.
/// </summary>
public class SplitScreenVRManager : MonoBehaviour
{
    public enum ViewMode
    {
        VR_Only,
        SplitScreen,
        Spectator_Only
    }

    [Header("Camera References")]
    [SerializeField] private Camera vrCamera;          // XR Origin или VR Camera
    [SerializeField] private Camera spectatorCamera;   // Внешняя камера (например, за машиной)

    [Header("Fade Settings")]
    [SerializeField, Range(0.1f, 2f)] private float fadeDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("Key Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.V;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private ViewMode _currentMode = ViewMode.VR_Only;
    private Image _fadeImage;
    private Canvas _fadeCanvas;
    private bool _isFading = false;

    private void Start()
    {
        if (vrCamera == null)
        {
            vrCamera = Camera.main;
            if (debugLogs)
                Debug.LogWarning("[SplitScreenVR] VR Camera not assigned. Using Camera.main");
        }

        if (spectatorCamera == null)
        {
            Debug.LogError("[SplitScreenVR] Spectator Camera not assigned!");
            enabled = false;
            return;
        }

        SetupFadeCanvas();
        ApplyMode(_currentMode);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && !_isFading)
        {
            StartCoroutine(ChangeModeWithFade());
        }
    }

    private IEnumerator ChangeModeWithFade()
    {
        _isFading = true;

        yield return Fade(1f); // затемнение

        CycleMode();
        ApplyMode(_currentMode);

        yield return new WaitForSeconds(0.05f);
        yield return Fade(0f); // плавное появление

        _isFading = false;
    }

    private void CycleMode()
    {
        _currentMode = (ViewMode)(((int)_currentMode + 1) % 3);
        if (debugLogs)
            Debug.Log($"[SplitScreenVR] Mode changed to: {_currentMode}");
    }

    private void ApplyMode(ViewMode mode)
    {
        switch (mode)
        {
            case ViewMode.VR_Only:
                vrCamera.rect = new Rect(0f, 0f, 1f, 1f);
                spectatorCamera.enabled = false;
                break;

            case ViewMode.SplitScreen:
                vrCamera.rect = new Rect(0f, 0f, 0.5f, 1f);
                spectatorCamera.enabled = true;
                spectatorCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
                break;

            case ViewMode.Spectator_Only:
                spectatorCamera.enabled = true;
                spectatorCamera.rect = new Rect(0f, 0f, 1f, 1f);
                vrCamera.rect = new Rect(0f, 0f, 0f, 0f);
                break;
        }
    }

    private void SetupFadeCanvas()
    {
        GameObject canvasObj = new GameObject("ScreenFadeCanvas");
        _fadeCanvas = canvasObj.AddComponent<Canvas>();
        _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _fadeCanvas.sortingOrder = 9999;

        GameObject imgObj = new GameObject("FadeImage");
        imgObj.transform.SetParent(_fadeCanvas.transform, false);
        _fadeImage = imgObj.AddComponent<Image>();
        _fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        RectTransform rect = _fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = _fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            _fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, newAlpha);
            yield return null;
        }

        _fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, targetAlpha);
    }
}
