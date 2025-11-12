using UnityEngine;

public class DestructibleInfrastructure : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float destroyDelay = 13f; // Время перед удалением
    [SerializeField] private bool debugLog = false;     // Для отладки

    private bool _isTriggered = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Если уже запущен таймер — не реагируем повторно
        if (_isTriggered) return;

        // Проверяем наличие Rigidbody на этом объекте
        if (TryGetComponent<Rigidbody>(out _) && collision.relativeVelocity.magnitude > 1f)
        {
            StartDestruction();
            return;
        }

        // Проверяем наличие Rigidbody у дочерних объектов
        Rigidbody childRb = GetComponentInChildren<Rigidbody>();
        if (childRb != null && collision.relativeVelocity.magnitude > 1f)
        {
            StartDestruction();
        }
    }

    private void StartDestruction()
    {
        _isTriggered = true;

        if (debugLog)
            Debug.Log($"[Destructible] {name} будет удалён через {destroyDelay} секунд.");

        StartCoroutine(DestroyAfterDelay());
    }

    private System.Collections.IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        if (debugLog)
            Debug.Log($"[Destructible] {name} удалён.");

        Destroy(gameObject);
    }
}
