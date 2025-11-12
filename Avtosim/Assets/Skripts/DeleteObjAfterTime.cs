using UnityEngine;

#if UNITY_EDITOR
[RequireComponent(typeof(Collider))]
#endif
public class DestructibleInfrastructure : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float destroyDelay = 13f; // Время перед удалением
    [SerializeField] private bool debugLog = false;     // Для отладки
    [SerializeField] private float minImpactSpeed = 1f; // минимальная relativeVelocity для срабатывания

    private bool _isTriggered = false;

    private void Awake()
    {
        // Пройдём по всем дочерним Rigidbody и при необходимости добавим прокси,
        // который будет пересылать столкновения сюда.
        var allRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in allRigidbodies)
        {
            if (rb == null) continue;

            // Если Rigidbody стоит на том же GameObject, где висит скрипт — прокси не нужен.
            if (rb.gameObject == this.gameObject)
                continue;

            // Если прокси уже есть, просто установим ссылку parent (на случай копирования/пересборки)
            var existing = rb.gameObject.GetComponent<CollisionProxy>();
            if (existing != null)
            {
                existing.Init(this);
            }
            else
            {
                var proxy = rb.gameObject.AddComponent<CollisionProxy>();
                proxy.Init(this);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Обработка столкновения с Rigidbody на самом объекте (если он есть)
        if (_isTriggered) return;

        if (TryGetComponent<Rigidbody>(out Rigidbody selfRb))
        {
            // если на корне есть Rigidbody — реагируем на столкновения с достаточной силой
            if (collision.relativeVelocity.magnitude > minImpactSpeed)
            {
                StartDestruction();
            }
        }
        // Если на корне Rigidbody нет — дочерние прокси вызовут NotifyCollision
    }

    // Этот метод вызывают прокси-компоненты на дочерних rigidbody
    public void NotifyCollision(Collision collisionFromChild)
    {
        if (_isTriggered) return;

        if (collisionFromChild == null) return;

        if (collisionFromChild.relativeVelocity.magnitude > minImpactSpeed)
        {
            if (debugLog)
                Debug.Log($"[Destructible] Collision detected on child '{collisionFromChild.gameObject.name}' with relVel {collisionFromChild.relativeVelocity.magnitude:F2}");
            StartDestruction();
        }
    }

    private void StartDestruction()
    {
        if (_isTriggered) return;

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

    // Вложенный прокси-компонент — очень лёгкий, только пересылает OnCollisionEnter сюда
    private class CollisionProxy : MonoBehaviour
    {
        private DestructibleInfrastructure _parent;

        // Инициализация/повторная инициализация
        public void Init(DestructibleInfrastructure parent)
        {
            _parent = parent;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Если родитель уже уничтожается либо не установлен — ничего не делаем
            if (_parent == null || _parent._isTriggered) return;

            _parent.NotifyCollision(collision);
        }

        private void OnDestroy()
        {
            _parent = null;
        }
    }
}
