using System;
using System.Collections;
using _2DOF;
using UnityEngine;

public class CarTelemetryHandler : MonoBehaviour
{
    private const float WAIT_TIME = SendingData.WAIT_TIME / 1000f;

    [SerializeField] private Transform vehicleTransform;
    [SerializeField] private Rigidbody rigidbody;

    [Header("Tilt Settings")]
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float tiltResponseSpeed = 2f;
    [SerializeField] private float accelerationThreshold = 1f;
    [SerializeField] private float deadZone = 0.1f; // Зона нечувствительности

    private ObjectTelemetryData _telemetryDataData;
    private SendingData _sendingData;
    private Vector3 _previousVelocity;
    private float _previousTime;
    private float _currentTiltAngle;
    private float _tiltVelocity; // Для SmoothDamp

    // Добавляем смещение для центра масс если нужно
    [SerializeField] private Vector3 centerOfMassOffset = Vector3.zero;

    private void Awake()
    {
        _sendingData = new SendingData();
        _telemetryDataData = _sendingData.ObjectTelemetryData;
        _previousVelocity = rigidbody.linearVelocity;
        _previousTime = Time.time;

        // Устанавливаем центр масс если нужно
        if (centerOfMassOffset != Vector3.zero)
        {
            rigidbody.centerOfMass = centerOfMassOffset;
        }
    }

    public void OnEnable()
    {
        StartCoroutine(TelemetryHandler());
        _sendingData.SendingStart();
    }

    public void OnDisable()
    {
        StopCoroutine(TelemetryHandler());
        _sendingData.SendingStop();
    }

    private IEnumerator TelemetryHandler()
    {
        while (true)
        {
            if (_telemetryDataData == null)
            {
                yield return new WaitForSeconds(WAIT_TIME * 10f);
                continue;
            }

            UpdateAngles();
            UpdateVelocity();
            UpdatePlatformTilt();
            Debug.Log(_telemetryDataData.ToString());
            yield return new WaitForSeconds(WAIT_TIME);
        }
    }

    private void UpdateVelocity()
    {
        _telemetryDataData.Velocity = rigidbody.linearVelocity;
    }

    private void UpdateAngles()
    {
        var euler = vehicleTransform.eulerAngles;

        // Нормализуем углы в диапазон [-180, 180]
        euler.x = NormalizeAngle(euler.x);
        euler.y = NormalizeAngle(euler.y);
        euler.z = NormalizeAngle(euler.z);

        // Меняем оси местами согласно вашим требованиям
        // Unity X -> Platform Z, Unity Y -> Platform Y, Unity Z -> Platform X
        _telemetryDataData.Angles = new Vector3(euler.z, euler.y, euler.x);
    }

    private float NormalizeAngle(float angle)
    {
        angle = Mathf.Approximately(angle, 180) ? 0 : angle;
        angle = angle > 180 ? angle - 360 : angle;
        return angle;
    }

    private void UpdatePlatformTilt()
    {
        Vector3 currentVelocity = rigidbody.linearVelocity;
        float currentTime = Time.time;
        float deltaTime = currentTime - _previousTime;

        if (deltaTime > 0)
        {
            Vector3 acceleration = (currentVelocity - _previousVelocity) / deltaTime;

            // Получаем продольное ускорение (вперед/назад)
            float forwardAcceleration = Vector3.Dot(acceleration, vehicleTransform.forward);

            // Вычисляем целевой угол наклона
            float targetTiltAngle = 0f;

            // Добавляем зону нечувствительности
            if (Mathf.Abs(forwardAcceleration) > accelerationThreshold)
            {
                if (forwardAcceleration > accelerationThreshold)
                {
                    targetTiltAngle = -Mathf.Clamp(forwardAcceleration / 10f * maxTiltAngle, 0, maxTiltAngle);
                }
                else if (forwardAcceleration < -accelerationThreshold)
                {
                    targetTiltAngle = Mathf.Clamp(-forwardAcceleration / 10f * maxTiltAngle, 0, maxTiltAngle);
                }
            }

            // Используем SmoothDamp для плавного изменения угла
            _currentTiltAngle = Mathf.SmoothDamp(_currentTiltAngle, targetTiltAngle, ref _tiltVelocity, tiltResponseSpeed, Mathf.Infinity, deltaTime);

            // Добавляем мертвую зону для устранения микроколебаний
            if (Mathf.Abs(_currentTiltAngle) < deadZone)
            {
                _currentTiltAngle = 0f;
            }

            // Применяем наклон через Rigidbody для совместимости с физикой
            ApplyTiltToRigidbody();
        }

        _previousVelocity = currentVelocity;
        _previousTime = currentTime;
    }

    private void ApplyTiltToRigidbody()
    {
        // Получаем текущее вращение
        Quaternion currentRotation = rigidbody.rotation;

        // Создаем целевое вращение с наклоном по оси Z (в Unity это крен)
        // В вашем случае: наклон вперед/назад - это вращение вокруг оси X в Unity
        Quaternion targetRotation = Quaternion.Euler(_currentTiltAngle, currentRotation.eulerAngles.y, currentRotation.eulerAngles.z);

        // Применяем вращение через Rigidbody
        rigidbody.MoveRotation(targetRotation);
    }

    // Метод для принудительной установки угла наклона
    public void SetTiltAngle(float angle)
    {
        _currentTiltAngle = Mathf.Clamp(angle, -maxTiltAngle, maxTiltAngle);
        ApplyTiltToRigidbody();
    }

    // Метод для сброса наклона
    public void ResetTilt()
    {
        _currentTiltAngle = 0f;
        ApplyTiltToRigidbody();
    }

    // Метод для получения текущего угла наклона
    public float GetCurrentTiltAngle()
    {
        return _currentTiltAngle;
    }

    // Визуализация центра масс в редакторе
    private void OnDrawGizmosSelected()
    {
        if (rigidbody != null)
        {
            Gizmos.color = Color.red;
            Vector3 centerOfMass = rigidbody.centerOfMass + (centerOfMassOffset != Vector3.zero ? centerOfMassOffset : Vector3.zero);
            Gizmos.DrawWireSphere(rigidbody.position + rigidbody.rotation * centerOfMass, 0.1f);
        }
    }
}