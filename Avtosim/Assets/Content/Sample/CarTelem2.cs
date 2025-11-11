using _2DOF;
using Assets.VehicleController;
using Bhaptics.SDK2;
using System.Collections;
using TMPro;
using UnityEngine;

public class CarTelemetryHandler1 : MonoBehaviour
{
    private const float WAIT_TIME = SendingData.WAIT_TIME / 1000f;

    private ObjectTelemetryData telemetryDataData;
    private SendingData _sendingData;

    [Header("Vehicle References")]
    [SerializeField] private Transform vehicleTransform;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AllInOneInputProvider inputProvider;

    [Header("Platform Settings")]
    private const float maxPlatformAngle = 15f;
    private const float maxPlatformVelocity = 100f;
    private float currentPitch = 0f;
    private float currentRoll = 0f;
    private float currentLinearAcceleration = 0f;
    private float lastLinearVelocity = 0f;
    private float currentAngularVelocity = 0f;

    [Header("bHaptics Feedback")]
    [SerializeField] private bool enableHaptics = true;
    [SerializeField] private bool debugHaptics = false;
    [SerializeField] private float accelThreshold = 2.5f;
    [SerializeField] private float brakeThreshold = -3.5f;
    [SerializeField] private float lateralThreshold = 2.5f;
    [SerializeField] private float collisionIntensityScale = 3.6f;

    private Vector3 _previousVelocity;
    private float _previousTime;
    private float _lastBrakeInput = 0f;

    private void Awake()
    {
        _sendingData = new SendingData();
        telemetryDataData = _sendingData.ObjectTelemetryData;
        _previousVelocity = rb.linearVelocity;
        _previousTime = Time.time;
    }

    private void OnEnable()
    {
        StartCoroutine(TelemetryHandler());
        _sendingData.SendingStart();
    }

    private void OnDisable()
    {
        StopCoroutine(TelemetryHandler());
        _sendingData.SendingStop();
    }

    private IEnumerator TelemetryHandler()
    {
        while (true)
        {
            if (telemetryDataData == null)
            {
                yield return new WaitForSeconds(WAIT_TIME * 10f);
                continue;
            }

            UpdatePlatformVelocity();
            UpdatePlatformAngles();

            if (enableHaptics)
                HandleHaptics();

            yield return new WaitForSeconds(WAIT_TIME);
        }
    }

    private void HandleHaptics()
    {
        Vector3 velocity = rb.linearVelocity;
        Vector3 accel = (velocity - _previousVelocity) / Mathf.Max(Time.deltaTime, 0.001f);

        float forwardAccel = Vector3.Dot(accel, vehicleTransform.forward);
        float lateralAccel = Vector3.Dot(accel, vehicleTransform.right);
        float forwardSpeed = Vector3.Dot(velocity, vehicleTransform.forward);

        float brakeInput = inputProvider != null ? inputProvider.GetBrakeInput() : 0f;

        // --- Давление в спину (ускорение вперёд) ---
        if (forwardAccel > accelThreshold && forwardSpeed > 1f)
        {
            float intensity = Mathf.Clamp01(forwardAccel / 10f);
            BhapticsLibrary.Play("davlenie_kovsha", 0, intensity, 1, 0, 0);
            if (debugHaptics)
                Debug.Log($"[HAPTICS] Давление кресла: {intensity:F2}");
        }

        // --- Резкое торможение только при движении вперёд ---
        if (forwardSpeed > 2f && forwardAccel < brakeThreshold && brakeInput > 0.5f)
        {
            float intensity = Mathf.Clamp01(Mathf.Abs(forwardAccel) / 8f);
            BhapticsLibrary.Play("brake_attack", 0, intensity, 1, 0, 0);
            if (debugHaptics)
                Debug.Log($"[HAPTICS] Резкое торможение (brake_attack): {intensity:F2}");
        }

        // --- Поворот влево ---
        if (lateralAccel < -lateralThreshold)
        {
            float intensity = Mathf.Clamp01(Mathf.Abs(lateralAccel) / 10f);
            BhapticsLibrary.Play("left_povorot", 0, intensity, 1, 0, 0);
            if (debugHaptics)
                Debug.Log($"[HAPTICS] Поворот влево: {intensity:F2}");
        }

        // --- Поворот вправо ---
        if (lateralAccel > lateralThreshold)
        {
            float intensity = Mathf.Clamp01(Mathf.Abs(lateralAccel) / 10f);
            BhapticsLibrary.Play("right_povorot", 0, intensity, 1, 0, 0);
            if (debugHaptics)
                Debug.Log($"[HAPTICS] Поворот вправо: {intensity:F2}");
        }

        _previousVelocity = velocity;
        _previousTime = Time.time;
        _lastBrakeInput = brakeInput;
    }

    private float NormalizeAngle(float angle)
    {
        angle = angle > 180 ? angle - 360 : angle;
        return angle;
    }

    private void UpdatePlatformVelocity()
    {
        Vector3 globalLinearVelocity = rb.linearVelocity;
        Vector3 localLinearVelocity = transform.InverseTransformVector(globalLinearVelocity);

        float linearAcceleration = (localLinearVelocity.z - lastLinearVelocity) / Time.deltaTime;
        lastLinearVelocity = localLinearVelocity.z;

        linearAcceleration = Mathf.Clamp(linearAcceleration, -maxPlatformVelocity, maxPlatformVelocity);
        currentLinearAcceleration = Mathf.Lerp(currentLinearAcceleration, linearAcceleration, 0.02f);

        Vector3 globalAngularVelocity = rb.angularVelocity;
        Vector3 localAngularVelocity = transform.InverseTransformVector(globalAngularVelocity);

        currentAngularVelocity = Mathf.Lerp(currentAngularVelocity, Mathf.Clamp(localAngularVelocity.y, -maxPlatformVelocity, maxPlatformVelocity), 0.03f);

        telemetryDataData.Angles = transform.eulerAngles;
        telemetryDataData.Velocity = new Vector3(currentLinearAcceleration * 50, currentAngularVelocity * 160, 0);
    }

    private void UpdatePlatformAngles()
    {
        float targetPitch = Mathf.Clamp(NormalizeAngle(vehicleTransform.eulerAngles.x), -maxPlatformAngle, maxPlatformAngle);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, 0.04f);

        float targetRoll = Mathf.Clamp(NormalizeAngle(vehicleTransform.eulerAngles.z), -maxPlatformAngle, maxPlatformAngle);
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, 0.04f);

        telemetryDataData.Angles = new Vector3(currentPitch, currentRoll, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!enableHaptics) return;

        float intensity = Mathf.Clamp01(Mathf.Abs(lastLinearVelocity * collisionIntensityScale) / 100f);
        BhapticsLibrary.Play("remen_bezopasnosti", 0, intensity, 1, 0, 0);

        if (debugHaptics)
            Debug.Log($"[HAPTICS] Столкновение (ремень): {intensity:F2}");
    }
}
