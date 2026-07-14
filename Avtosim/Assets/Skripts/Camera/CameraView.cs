using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class SplitScreenVRManager : MonoBehaviour
{
    public enum ViewMode { VR_Only, SplitScreen, Spectator_Only }

    [Header("Cameras")]
    public Camera vrCamera;           // XR camera
    public Camera spectatorCamera;    // back camera
    public Camera mirrorCamera;       // mono copy of VR camera

    [Header("Fade")]
    public float fadeDuration = 0.4f;
    public Color fadeColor = Color.black;

    [Header("Toggle Key")]
    public KeyCode toggleKey = KeyCode.V;

    private ViewMode mode = ViewMode.VR_Only;

    private Canvas canvas;
    private RawImage leftRaw;     // VR mirror
    private RawImage rightRaw;    // spectator

    private RenderTexture mirrorRT;
    private RenderTexture spectatorRT;

    private Image fadeImage;
    private Canvas fadeCanvas;

    private int lastW, lastH;
    private bool isFading = false;

    void Start()
    {
        if (vrCamera == null)
            vrCamera = Camera.main;

        if (spectatorCamera == null)
        {
            Debug.LogError("Spectator camera missing!");
            enabled = false;
            return;
        }

        // Disable XR on spectator
        var uadS = spectatorCamera.GetUniversalAdditionalCameraData();
        if (uadS != null) uadS.allowXRRendering = false;

        // MirrorCamera creation
        if (mirrorCamera == null)
        {
            var go = new GameObject("MirrorCamera");
            mirrorCamera = go.AddComponent<Camera>();
            mirrorCamera.enabled = true;
        }

        // Disable XR for mirrorCamera
        var uadM = mirrorCamera.GetUniversalAdditionalCameraData();
        if (uadM != null) uadM.allowXRRendering = false;

        CreateCanvas();
        CreateFadeCanvas();

        ApplyMode(ViewMode.VR_Only);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && !isFading)
            StartCoroutine(ChangeMode());

        // update mirror camera transform
        SyncMirrorCamera();

        // rebuild RT on resolution change
        if (Screen.width != lastW || Screen.height != lastH)
            RebuildRTs();
    }

    private void SyncMirrorCamera()
    {
        if (mirrorCamera == null || vrCamera == null) return;

        mirrorCamera.transform.position = vrCamera.transform.position;
        mirrorCamera.transform.rotation = vrCamera.transform.rotation;
        mirrorCamera.fieldOfView = vrCamera.fieldOfView;
        mirrorCamera.nearClipPlane = vrCamera.nearClipPlane;
        mirrorCamera.farClipPlane = vrCamera.farClipPlane;
        mirrorCamera.cullingMask = vrCamera.cullingMask;
    }

    IEnumerator ChangeMode()
    {
        isFading = true;
        yield return Fade(1f);

        mode = (ViewMode)(((int)mode + 1) % 3);
        ApplyMode(mode);

        yield return new WaitForSeconds(0.05f);
        yield return Fade(0f);
        isFading = false;
    }

    private void ApplyMode(ViewMode m)
    {
        mode = m;

        switch (m)
        {
            case ViewMode.VR_Only:
                leftRaw.gameObject.SetActive(false);
                rightRaw.gameObject.SetActive(false);
                spectatorCamera.enabled = false;
                mirrorCamera.enabled = false;
                break;

            case ViewMode.SplitScreen:
                RebuildRTs();

                leftRaw.gameObject.SetActive(true);
                rightRaw.gameObject.SetActive(true);

                leftRaw.texture = mirrorRT;
                rightRaw.texture = spectatorRT;

                mirrorCamera.targetTexture = mirrorRT;
                spectatorCamera.targetTexture = spectatorRT;

                spectatorCamera.enabled = true;
                mirrorCamera.enabled = true;
                break;

            case ViewMode.Spectator_Only:
                RebuildRTs(fullScreenSpectator: true);

                // show only spectator
                leftRaw.gameObject.SetActive(false);
                rightRaw.gameObject.SetActive(true);

                rightRaw.texture = spectatorRT;

                spectatorCamera.targetTexture = spectatorRT;
                spectatorCamera.enabled = true;

                mirrorCamera.enabled = false;
                break;
        }
    }

    private void RebuildRTs(bool fullScreenSpectator = false)
    {
        int w = Screen.width;
        int h = Screen.height;

        lastW = w; lastH = h;

        if (mirrorRT != null) { mirrorRT.Release(); Destroy(mirrorRT); }
        if (spectatorRT != null) { spectatorRT.Release(); Destroy(spectatorRT); }

        if (!fullScreenSpectator)
        {
            // split
            int halfW = w / 2;

            mirrorRT = new RenderTexture(halfW, h, 24);
            spectatorRT = new RenderTexture(halfW, h, 24);

            SetAnchors(leftRaw.rectTransform, 0, 0, 0.5f, 1);
            SetAnchors(rightRaw.rectTransform, 0.5f, 0, 1, 1);
        }
        else
        {
            // full screen spectator
            spectatorRT = new RenderTexture(w, h, 24);
            SetAnchors(rightRaw.rectTransform, 0, 0, 1, 1);
        }
    }

    private void CreateCanvas()
    {
        var canvasObj = new GameObject("SpectatorCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        // Left RawImage
        var leftGO = new GameObject("LeftRaw");
        leftGO.transform.SetParent(canvas.transform);
        leftRaw = leftGO.AddComponent<RawImage>();
        leftRaw.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Right RawImage
        var rightGO = new GameObject("RightRaw");
        rightGO.transform.SetParent(canvas.transform);
        rightRaw = rightGO.AddComponent<RawImage>();
        rightRaw.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        leftRaw.gameObject.SetActive(false);
        rightRaw.gameObject.SetActive(false);
    }

    private void CreateFadeCanvas()
    {
        var fadeObj = new GameObject("FadeCanvas");
        fadeCanvas = fadeObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999;

        var imgObj = new GameObject("FadeImage");
        imgObj.transform.SetParent(fadeCanvas.transform);
        fadeImage = imgObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);

        var rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void SetAnchors(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
    {
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    IEnumerator Fade(float target)
    {
        float start = fadeImage.color.a;
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(start, target, t / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, a);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, target);
    }
}
