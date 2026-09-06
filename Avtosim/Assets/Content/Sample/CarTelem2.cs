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

    [Header("Фильтрация (защита приводов от дрожания)")]
    [Tooltip("Ускорение получается численным дифференцированием скорости и потому шумное. " +
             "Слишком малая постоянная времени пропускает этот шум прямо на приводы и капсулу трясёт.")]
    [SerializeField] private float accelPrefilterTau = 0.02f;
    [SerializeField] private float accelAttackTau = 0.025f;
    [SerializeField] private float accelReleaseTau = 0.12f;
    [Tooltip("Вертикальный канал самый шумный - подвеска и неровности. Фильтруется сильнее прочих.")]
    [SerializeField] private float heavePrefilterTau = 0.03f;
    [SerializeField] private float heaveAttackTau = 0.025f;
    [SerializeField] private float heaveReleaseTau = 0.12f;
    [SerializeField] private float yawTau = 0.05f;
    [SerializeField] private float angleTau = 0.10f;

    [Header("Неровности: бордюры, ямы, лежачие полицейские")]
    [Tooltip("Перекос платформы при наезде одним бортом на препятствие, град. " +
             "Считается по разности хода стоек левого и правого бортов, поэтому " +
             "срабатывает даже там, где кузов почти не наклоняется.")]
    [SerializeField] private float curbRollDegrees = 9f;
    [Tooltip("Клевок при въезде передней осью на препятствие и съезде с него, град.")]
    [SerializeField] private float curbPitchDegrees = 5f;
    [Tooltip("Подъём платформы при поджатии всех стоек. Условные м/с^2 " +
             "вертикального канала: капсула держится приподнятой, пока машина на бордюре.")]
    [SerializeField] private float curbHeaveGain = 7f;
    [Tooltip("Толчок на кромке препятствия: реакция на СКОРОСТЬ поджатия стоек, " +
             "а не на само поджатие. Даёт короткий удар в момент наезда.")]
    [SerializeField] private float curbJoltGain = 1.2f;

    [Header("Ограничители")]
    [Tooltip("Мёртвая зона по ускорению, м/с^2: мелкая дрожь ниже порога на платформу не идёт.")]
    [SerializeField] private float accelDeadzone = 0.25f;
    [Tooltip("Предел скорости изменения канала, единиц/с: не даёт скачком бросить привод в упор.")]
    [SerializeField] private float accelSlewPerSecond = 120f;

    private float currentPitch = 0f;
    private float currentRoll = 0f;
    private float currentSurge = 0f;
    private float currentSway = 0f;
    private float currentHeave = 0f;
    private float currentYawRate = 0f;
    private readonly SuspensionMotionSampler suspension = new SuspensionMotionSampler();
    private Vector3 lastVelocity = Vector3.zero;
    private float filteredSurge = 0f;
    private float filteredSway = 0f;
    private float filteredHeave = 0f;

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
        suspension.Initialize(vehicleTransform != null ? vehicleTransform : transform);
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

        // Предварительный фильтр СРАЗУ после дифференцирования: до него сигнал
        // несёт численный шум, который иначе уходит прямо на приводы.
        filteredSurge = Smooth(filteredSurge, Mathf.Clamp(accelLocal.z, -maxAccel, maxAccel), dt, accelPrefilterTau);
        filteredSway = Smooth(filteredSway, Mathf.Clamp(accelLocal.x, -maxAccel, maxAccel), dt, accelPrefilterTau);
        filteredHeave = Smooth(filteredHeave, Mathf.Clamp(accelLocal.y, -maxAccel, maxAccel), dt, heavePrefilterTau);

        // Мёртвая зона убирает постоянную мелкую дрожь на ровной дороге.
        float surgeRaw = ApplyDeadzone(filteredSurge, accelDeadzone);
        float swayRaw = ApplyDeadzone(filteredSway, accelDeadzone);
        float heaveRaw = ApplyDeadzone(filteredHeave, accelDeadzone);

        // --- скорость рыскания в град/с ---
        Vector3 angVelLocal = vehicleTransform.InverseTransformDirection(rb.angularVelocity);
        float yawRateRaw = Mathf.Clamp(angVelLocal.y * Mathf.Rad2Deg, -maxYawRate, maxYawRate);

        // --- перекос подвески: бордюры и прочие неровности ---
        // Ускорение центра масс наезд одним колесом на бордюр почти не видит:
        // кузов при этом вертикально почти не разгоняется. Перекос заметен
        // только по ходу отдельных стоек, поэтому берём его оттуда.
        suspension.Sample(dt);

        // --- углы кузова ---
        float pitchRaw = Mathf.Clamp(NormalizeAngle(vehicleTransform.eulerAngles.x), -maxPlatformAngle, maxPlatformAngle);
        float rollRaw = Mathf.Clamp(NormalizeAngle(vehicleTransform.eulerAngles.z), -maxPlatformAngle, maxPlatformAngle);

        // Левый борт поджат вверх -> тот же знак, что и крен кузова левым
        // бортом вверх, поэтому просто складываем.
        rollRaw = Mathf.Clamp(rollRaw + suspension.Roll * curbRollDegrees,
            -maxPlatformAngle, maxPlatformAngle);
        pitchRaw = Mathf.Clamp(pitchRaw + suspension.Pitch * curbPitchDegrees,
            -maxPlatformAngle, maxPlatformAngle);

        // Вертикальный канал: удержание на бордюре плюс удар на его кромке.
        // Мёртвую зону сюда НЕ применяем — она относится к шуму
        // дифференцирования скорости, а этот сигнал берётся прямо с подвески.
        heaveRaw = Mathf.Clamp(
            heaveRaw + suspension.Heave * curbHeaveGain + suspension.HeaveRate * curbJoltGain,
            -maxAccel, maxAccel);

        // Сглаживание, не зависящее от частоты кадров (раньше был Lerp с
        // постоянным коэффициентом ~1 с — из-за него резкие ускорения
        // размазывались и отклик был ватным).
        // Сглаживание + ограничение скорости изменения: привод не может быть
        // брошен в упор одним шагом, даже если ускорение скакнуло.
        currentSurge = Slew(currentSurge, SmoothAsym(currentSurge, surgeRaw, dt, accelAttackTau, accelReleaseTau), dt, accelSlewPerSecond);
        currentSway = Slew(currentSway, SmoothAsym(currentSway, swayRaw, dt, accelAttackTau, accelReleaseTau), dt, accelSlewPerSecond);
        currentHeave = Slew(currentHeave, SmoothAsym(currentHeave, heaveRaw, dt, heaveAttackTau, heaveReleaseTau), dt, accelSlewPerSecond);
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
    /// <summary>Мёртвая зона без ступеньки на границе.</summary>
    private static float ApplyDeadzone(float value, float deadzone)
    {
        if (deadzone <= 0f)
        {
            return value;
        }

        var magnitude = Mathf.Abs(value);

        return magnitude <= deadzone ? 0f : Mathf.Sign(value) * (magnitude - deadzone);
    }

    /// <summary>Ограничение скорости изменения канала.</summary>
    private static float Slew(float current, float target, float dt, float maxPerSecond)
    {
        if (maxPerSecond <= 0f)
        {
            return target;
        }

        var maxStep = maxPerSecond * dt;

        return Mathf.Clamp(target, current - maxStep, current + maxStep);
    }

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
