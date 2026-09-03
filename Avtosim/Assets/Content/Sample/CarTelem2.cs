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

    // === Контракт телеметрии (6 слотов общей памяти) ===
    //   [0] Angles.x   Pitch — угол тангажа, град
    //   [1] Angles.y   Roll  — угол крена,   град
    //   [2] Angles.z   Yaw   — скорость рыскания, град/с
    //   [3] Velocity.x Surge — продольное ускорение, м/с^2 (+вперёд)
    //   [4] Velocity.y Sway  — БОКОВОЕ ускорение,   м/с^2 (+вправо)
    //   [5] Velocity.z Heave — вертикальное ускорение, м/с^2 (+вверх)
    // Раньше боковое ускорение не считалось вообще (в Sway ехала скорость
    // рыскания), а Yaw и Heave были нулями — поэтому крена, рыскания и тряски
    // не было ни на платформе, ни в эмуляторе.

    [Header("Platform Limits")]
    private const float maxPlatformAngle = 20f;    // град
    private const float maxAccel = 20f;            // м/с^2 (~2g)
    private const float maxYawRate = 120f;         // град/с

    [Header("Motion Gains (подстройка силы каналов)")]
    [SerializeField] private float surgeGain = 1f;
    [SerializeField] private float swayGain = 1f;
    [SerializeField] private float heaveGain = 1f;
    [SerializeField] private float yawGain = 1f;
    [SerializeField] private float angleGain = 1f;

    [Header("Response (с): атака — как быстро нарастает, спад — как затухает")]
    [Tooltip("Почти мгновенная атака нужна, чтобы резкое торможение и удар в стену пробивались")]
    [SerializeField] private float accelAttackTau = 0.012f;
    [SerializeField] private float accelReleaseTau = 0.12f;
    [SerializeField] private float heaveAttackTau = 0.008f;
    [SerializeField] private float heaveReleaseTau = 0.09f;
    [SerializeField] private float yawTau = 0.06f;
    [SerializeField] private float angleTau = 0.10f;

    private float currentPitch = 0f;
    private float currentRoll = 0f;
    private float currentSurge = 0f;
    private float currentSway = 0f;
    private float currentHeave = 0f;
    private float currentYawRate = 0f;
    private Vector3 lastVelocity = Vector3.zero;

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

        // --- �������� � ����� (��������� �����) ---
        if (forwardAccel > accelThreshold && forwardSpeed > 1f)
        {
            float intensity = Mathf.Clamp01(forwardAccel / 10f);
            BhapticsLibrary.Play("davlenie_kovsha", 0, intensity, 1, 0, 0);
            if (debugHaptics)
                Debug.Log($"[HAPTICS] �������� ������: {intensity:F2}");
        }

        // --- ������ ���������� ������ ��� �������� ����� ---
        if (forwardSpeed > 2f && forwardAccel < brakeThreshold && brakeInput > 0.5f)
        {
            float intensity = Mathf.Clamp01(Mathf.Abs(forwardAccel) / 8f);
            BhapticsLibrary.Play("brake_attack", 0, intensity, 1, 0, 0);
            if (debugHaptics)
                Debug.Log($"[HAPTICS] ������ ���������� (brake_attack): {intensity:F2}");
        }

        // --- ������� ����� ---
        if (lateralAccel < -lateralThreshold)
        {
            float intensity = Mathf.Clamp01(Mathf.Abs(lateralAccel) / 10f);
            BhapticsLibrary.Play("left_povorot", 0, intensity, 1, 0, 0);
            if (debugHaptics)
                Debug.Log($"[HAPTICS] ������� �����: {intensity:F2}");
        }

        // --- ������� ������ ---
        if (lateralAccel > lateralThreshold)
        {
            float intensity = Mathf.Clamp01(Mathf.Abs(lateralAccel) / 10f);
            BhapticsLibrary.Play("right_povorot", 0, intensity, 1, 0, 0);
            if (debugHaptics)
                Debug.Log($"[HAPTICS] ������� ������: {intensity:F2}");
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

    /// <summary>
    /// Считает полный набор 6 DOF в СИСТЕМЕ КООРДИНАТ МАШИНЫ и пишет его в
    /// общую память. Ускорения берём один раз и раскладываем на продольное /
    /// боковое / вертикальное — отсюда сразу и разгон-торможение, и крен в
    /// повороте, и тряска на неровностях.
    /// </summary>
    // Считаем в ФИЗИЧЕСКОМ такте: столкновение и резкое торможение живут
    // 1-2 шага физики, и при опросе из корутины по дельте кадра такой спайк
    // просто не попадал в выборку — поэтому удар в стену не доходил.
    private void FixedUpdate()
    {
        if (telemetryDataData == null || rb == null || vehicleTransform == null)
        {
            return;
        }

        UpdatePlatformTelemetry(Time.fixedDeltaTime);
    }

    private void UpdatePlatformTelemetry(float dt)
    {
        dt = Mathf.Max(dt, 0.0001f);

        // --- линейные ускорения в осях машины ---
        Vector3 velocity = rb.linearVelocity;
        Vector3 accelWorld = (velocity - lastVelocity) / dt;
        lastVelocity = velocity;

        Vector3 accelLocal = vehicleTransform.InverseTransformDirection(accelWorld);

        float surgeRaw = Mathf.Clamp(accelLocal.z, -maxAccel, maxAccel);
        float swayRaw = Mathf.Clamp(accelLocal.x, -maxAccel, maxAccel);
        float heaveRaw = Mathf.Clamp(accelLocal.y, -maxAccel, maxAccel);

        // --- скорость рыскания в град/с ---
        Vector3 angVelLocal = vehicleTransform.InverseTransformDirection(rb.angularVelocity);
        float yawRateRaw = Mathf.Clamp(angVelLocal.y * Mathf.Rad2Deg, -maxYawRate, maxYawRate);

        // --- углы кузова ---
        float pitchRaw = Mathf.Clamp(NormalizeAngle(vehicleTransform.eulerAngles.x), -maxPlatformAngle, maxPlatformAngle);
        float rollRaw = Mathf.Clamp(NormalizeAngle(vehicleTransform.eulerAngles.z), -maxPlatformAngle, maxPlatformAngle);

        // Сглаживание, не зависящее от частоты кадров (раньше был Lerp с
        // постоянным коэффициентом ~1 с — из-за него резкие ускорения
        // размазывались и отклик был ватным).
        currentSurge = SmoothAsym(currentSurge, surgeRaw, dt, accelAttackTau, accelReleaseTau);
        currentSway = SmoothAsym(currentSway, swayRaw, dt, accelAttackTau, accelReleaseTau);
        currentHeave = SmoothAsym(currentHeave, heaveRaw, dt, heaveAttackTau, heaveReleaseTau);
        currentYawRate = Smooth(currentYawRate, yawRateRaw, dt, yawTau);
        currentPitch = Smooth(currentPitch, pitchRaw, dt, angleTau);
        currentRoll = Smooth(currentRoll, rollRaw, dt, angleTau);

        telemetryDataData.Angles = new Vector3(
            currentPitch * angleGain,
            currentRoll * angleGain,
            currentYawRate * yawGain);

        telemetryDataData.Velocity = new Vector3(
            currentSurge * surgeGain,
            currentSway * swayGain,
            currentHeave * heaveGain);
    }

    /// <summary>
    /// Быстрая атака, медленный спад: пик (удар, резкий тормоз) проходит почти
    /// мгновенно, а возврат остаётся плавным.
    /// </summary>
    private static float SmoothAsym(float current, float target, float dt, float tauAttack, float tauRelease)
    {
        var rising = Mathf.Abs(target) > Mathf.Abs(current);
        return Smooth(current, target, dt, rising ? tauAttack : tauRelease);
    }

    private static float Smooth(float current, float target, float dt, float tau)
    {
        if (tau <= 0f)
        {
            return target;
        }

        return Mathf.Lerp(current, target, 1f - Mathf.Exp(-dt / tau));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!enableHaptics) return;

        float intensity = Mathf.Clamp01(Mathf.Abs(lastVelocity.magnitude * collisionIntensityScale) / 100f);
        BhapticsLibrary.Play("remen_bezopasnosti", 0, intensity, 1, 0, 0);

        if (debugHaptics)
            Debug.Log($"[HAPTICS] ������������ (������): {intensity:F2}");
    }
}
