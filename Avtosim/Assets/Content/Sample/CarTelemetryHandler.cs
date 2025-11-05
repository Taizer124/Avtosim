using System;
using System.Collections;
using _2DOF;
using UnityEngine;
using Bhaptics.SDK2; // Добавь, если используешь Bhaptics SDK

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

    [SerializeField] private Vector3 centerOfMassOffset = Vector3.zero;

    private void Awake()
    {
        _sendingData = new SendingData();
        _telemetryDataData = _sendingData.ObjectTelemetryData;
        _previousVelocity = rigidbody.linearVelocity;
        _previousTime = Time.time;

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
            //Debug.Log(_telemetryDataData.ToString());
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
        euler.x = NormalizeAngle(euler.x);
        euler.y = NormalizeAngle(euler.y);
        euler.z = NormalizeAngle(euler.z);
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
            float forwardAcceleration = Vector3.Dot(acceleration, vehicleTransform.forward);
            float forwardSpeed = Vector3.Dot(currentVelocity, vehicleTransform.forward);

            // Реалистичная g-сила
            float gForce = forwardAcceleration / 9.81f; // 1g = ускорение свободного падения

            float targetTiltAngle = 0f;

            // Эффект ремня безопасности (резкое торможение)
            if (forwardSpeed > 2f && gForce < -0.3f)
            {
                // Преобразуем силу торможения в интенсивность (3g = максимум)
                float intensityValue = Mathf.Clamp01(Mathf.Abs(gForce) / 3f);

                BhapticsLibrary.Play(
                    eventId: "remen_bez",
                    startMillis: 0,
                    intensity: intensityValue,
                    duration: 1,
                    angleX: 0,
                    offsetY: 0
                );

                //Debug.Log("Haptic effect: remen_bez | Intensity: " + intensityValue.ToString("F2") + " | gForce: " + gForce.ToString("F2"));
            }

            // Эффект давления в спину (ускорение вперёд)
            if (forwardSpeed > 1f && gForce > 0.3f)
            {
                // Чем сильнее ускорение, тем выше интенсивность (до 2g)
                float intensityValue = Mathf.Clamp01(gForce / 2f);

                BhapticsLibrary.Play(
                    eventId: "davlenie_kovsha",
                    startMillis: 0,
                    intensity: intensityValue,
                    duration: 1,
                    angleX: 0,
                    offsetY: 0
                );

                //Debug.Log("Haptic effect: davlenie_kovsha | Intensity: " + intensityValue.ToString("F2") + " | gForce: " + gForce.ToString("F2"));
            }

            // Наклон платформы
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

            _currentTiltAngle = Mathf.SmoothDamp(
                _currentTiltAngle,
                targetTiltAngle,
                ref _tiltVelocity,
                tiltResponseSpeed,
                Mathf.Infinity,
                deltaTime
            );

            if (Mathf.Abs(_currentTiltAngle) < deadZone)
                _currentTiltAngle = 0f;

            ApplyTiltToRigidbody();
        }

        _previousVelocity = currentVelocity;
        _previousTime = currentTime;
    }

    private void ApplyTiltToRigidbody()
    {
        Quaternion currentRotation = rigidbody.rotation;
        Quaternion targetRotation = Quaternion.Euler(_currentTiltAngle, currentRotation.eulerAngles.y, currentRotation.eulerAngles.z);
        rigidbody.MoveRotation(targetRotation);
    }

    public void SetTiltAngle(float angle)
    {
        _currentTiltAngle = Mathf.Clamp(angle, -maxTiltAngle, maxTiltAngle);
        ApplyTiltToRigidbody();
    }

    public void ResetTilt()
    {
        _currentTiltAngle = 0f;
        ApplyTiltToRigidbody();
    }

    public float GetCurrentTiltAngle()
    {
        return _currentTiltAngle;
    }

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
