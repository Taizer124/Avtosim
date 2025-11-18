using UnityEngine;

#if UNITY_EDITOR
[RequireComponent(typeof(Collider))]
#endif
public class DestructibleInfrastructure : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float destroyDelay = 13f;
    [SerializeField] private bool debugLog = false;
    [SerializeField] private float minImpactSpeed = 1f;

    private bool _isTriggered = false;
    private bool _hasRootRb = false;
    private Rigidbody _rootRb;

    private void Awake()
    {
        // Проверяем, есть ли Rigidbody на корне
        _hasRootRb = TryGetComponent<Rigidbody>(out _rootRb);

        // Сканируем дочерние Rigidbody и добавляем Proxy
        var allRigidbodies = GetComponentsInChildren<Rigidbody>(true);

        foreach (var rb in allRigidbodies)
        {
            if (rb == null) continue;
            if (rb.gameObject == this.gameObject) continue;

            var existing = rb.GetComponent<CollisionProxy>();
            if (existing != null)
                existing.Init(this);
            else
            {
                var proxy = rb.gameObject.AddComponent<CollisionProxy>();
                proxy.Init(this);
            }
        }
    }

    // Обрабатываем столкновения, но безопасно
    private void OnCollisionEnter(Collision collision)
    {
        if (_isTriggered) return;

        // если нет Rigidbody игнорируем (Unity любит кидать ошибки)
        if (!_hasRootRb || _rootRb == null) return;

        if (collision?.relativeVelocity.magnitude > minImpactSpeed)
            StartDestruction();
    }

    // Вызов от дочерних Proxy
    public void NotifyCollision(Collision collisionFromChild)
    {
        if (_isTriggered) return;
        if (collisionFromChild == null) return;

        float vel = collisionFromChild.relativeVelocity.magnitude;

        if (vel > minImpactSpeed)
        {
            if (debugLog)
                Debug.Log($"[Destructible] Child '{collisionFromChild.gameObject.name}' relVel={vel:F2}");

            StartDestruction();
        }
    }

    private void StartDestruction()
    {
        if (_isTriggered) return;

        _isTriggered = true;

        if (debugLog)
            Debug.Log($"[Destructible] {name} будет удалён через {destroyDelay} сек.");

        StartCoroutine(DestroyAfterDelay());
    }

    private System.Collections.IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        if (debugLog)
            Debug.Log($"[Destructible] {name} удалён.");

        Destroy(gameObject);
    }


    [RequireComponent(typeof(Collider))]
    private class CollisionProxy : MonoBehaviour
    {
        private DestructibleInfrastructure _parent;
        private bool _isQuitting = false;

        public void Init(DestructibleInfrastructure parent)
        {
            _parent = parent;
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isQuitting) return;
            if (_parent == null || _parent._isTriggered) return;
            if (collision == null) return;

            // Ошибок не будет, даже если Rigidbody не прикреплён
            _parent.NotifyCollision(collision);
        }

        private void OnDestroy()
        {
            _parent = null;
        }
    }
}
