using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRestartTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneName = "mcp_day";
    public bool useSceneName = true;

    [Header("Trigger Settings")]
    public bool requirePlayerTag = true;
    public string playerTag = "Player";

    [Header("Restart Settings")]
    public float restartDelay = 0f;
    public bool showDebugMessages = true;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnRestartTriggered;

    private bool _isRestarting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isRestarting)
            return;

        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;

        StartRestartSequence();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isRestarting)
            return;

        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;

        StartRestartSequence();
    }

    private void StartRestartSequence()
    {
        if (_isRestarting)
            return;

        _isRestarting = true;

        if (showDebugMessages)
            Debug.Log($"Player entered trigger. Restarting scene in {restartDelay} seconds...");

        OnRestartTriggered?.Invoke();

        if (restartDelay > 0)
        {
            Invoke("RestartScene", restartDelay);
        }
        else
        {
            RestartScene();
        }
    }

    private void RestartScene()
    {
        if (useSceneName)
        {
            if (showDebugMessages)
                Debug.Log($"Loading scene: {sceneName}");

            SceneManager.LoadScene(sceneName);
        }
        else
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (showDebugMessages)
                Debug.Log($"Reloading current scene: {currentSceneName}");

            SceneManager.LoadScene(currentSceneName);
        }
    }

    private void OnValidate()
    {
        if (requirePlayerTag && string.IsNullOrEmpty(playerTag))
        {
            playerTag = "Player";
        }

        if (useSceneName && string.IsNullOrEmpty(sceneName))
        {
            sceneName = "mcp_day";
        }

        restartDelay = Mathf.Max(0, restartDelay);
    }
}