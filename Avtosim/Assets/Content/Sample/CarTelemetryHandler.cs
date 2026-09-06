using System;
using System.Collections;
using _2DOF;
using UnityEngine;
using Bhaptics.SDK2; // ���������� Bhaptics SDK

public class CarTelemetryHandler : MonoBehaviour
{
    private const float WAIT_TIME = SendingData.WAIT_TIME / 1000f;

    [SerializeField] private Transform vehicleTransform;
    [SerializeField] private Rigidbody rigidbody;

    [Header("Tilt Settings")]
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float tiltResponseSpeed = 2f;
    [SerializeField] private float accelerationThreshold = 1f;
    [SerializeField] private float deadZone = 0.1f; // ���� ������������������

    [Header("Debug")]
    [SerializeField] private bool debugTilt = false;

    private ObjectTelemetryData _telemetryDataData;
    private SendingData _sendingData;
    private Vector3 _previousVelocity;
    private float _previousTime;
    private float _currentTiltAngle;
    private float _tiltVelocity;

    private const float MaxAccel = 20f;
    private const float MaxYawRate = 120f;
    private const float MaxAngleDeg = 20f;
    private const float AccelPrefilterTau = 0.02f;
    private const float HeavePrefilterTau = 0.03f;
    private const float AccelTau = 0.025f;
    private const float HeaveTau = 0.025f;
    private const float YawTau = 0.05f;
    private const float AngleTau = 0.10f;
    private const float AccelDeadzone = 0.25f;

    // Неровности: значения совпадают с CarTelemetryHandler1, иначе платформа
    // вела бы себя по-разному на разных машинах.
    private const float CurbRollDegrees = 9f;
    private const float CurbPitchDegrees = 5f;
    private const float CurbHeaveGain = 7f;
    private const float CurbJoltGain = 1.2f;

    private readonly SuspensionMotionSampler _suspension = new SuspensionMotionSampler();
    private Vector3 _lastVelocity;
    private float _filteredSurge;
    private float _filteredSway;
    private float _filteredHeave;
    private float _currentSurge;
    private float _currentSway;
    private float _currentHeave;
    private float _currentYawRate;
    private float _currentPitch;
    private float _currentRoll;

    [SerializeField] private Vector3 centerOfMassOffset = Vector3.zero;

    private void Awake()
    {
        _sendingData = new SendingData();
        _suspension.Initialize(vehicleTransform != null ? vehicleTransform : transform);
        _telemetryDataData = _sendingData.ObjectTelemetryData;
        _previousVelocity = rigidbody.linearVelocity;
        _previousTime = Time.time;

        if (centerOfMassOffset != Vector3.zero)
            rigidbody.centerOfMass = centerOfMassOffset;
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
            yield return new WaitForSeconds(WAIT_TIME);
        }
    }

    // Контракт совпадает с CarTelemetryHandler1, иначе платформа вела бы себя
    // по-разному на разных машинах:
    //   Angles   = (тангаж град, крен град, скорость рыскания град/с)
    //   Velocity = (surge, sway, heave) - ускорения в осях машины, м/с^2
    private void FixedUpdate()
    {
        if (_telemetryDataData == null || rigidbody == null || vehicleTransform == null)
        {
            return;
        }

        float dt = Mathf.Max(Time.fixedDeltaTime, 0.0001f);

        Vector3 velocity = rigidbody.linearVelocity;
        Vector3 accelLocal = vehicleTransform.InverseTransformDirection((velocity - _lastVelocity) / dt);
        _lastVelocity = velocity;

        _filteredSurge = Smooth(_filteredSurge, Mathf.Clamp(accelLocal.z, -MaxAccel, MaxAccel), dt, AccelPrefilterTau);
        _filteredSway = Smooth(_filteredSway, Mathf.Clamp(accelLocal.x, -MaxAccel, MaxAccel), dt, AccelPrefilterTau);
        _filteredHeave = Smooth(_filteredHeave, Mathf.Clamp(accelLocal.y, -MaxAccel, MaxAccel), dt, HeavePrefilterTau);

        // Перекос стоек: наезд одним колесом на бордюр по ускорению кузова
        // не виден, его показывает только ход отдельных стоек.
        _suspension.Sample(dt);

        float heaveTarget = Mathf.Clamp(
            Deadzone(_filteredHeave)
            + _suspension.Heave * CurbHeaveGain
            + _suspension.HeaveRate * CurbJoltGain,
            -MaxAccel, MaxAccel);

        _currentSurge = Smooth(_currentSurge, Deadzone(_filteredSurge), dt, AccelTau);
        _currentSway = Smooth(_currentSway, Deadzone(_filteredSway), dt, AccelTau);
        _currentHeave = Smooth(_currentHeave, heaveTarget, dt, HeaveTau);

        Vector3 angVelLocal = vehicleTransform.InverseTransformDirection(rigidbody.angularVelocity);
        _currentYawRate = Smooth(_currentYawRate,
            Mathf.Clamp(angVelLocal.y * Mathf.Rad2Deg, -MaxYawRate, MaxYawRate), dt, YawTau);

        float pitchTarget = Mathf.Clamp(
            NormalizeAngle(vehicleTransform.eulerAngles.x) + _suspension.Pitch * CurbPitchDegrees,
            -MaxAngleDeg, MaxAngleDeg);
        float rollTarget = Mathf.Clamp(
            NormalizeAngle(vehicleTransform.eulerAngles.z) + _suspension.Roll * CurbRollDegrees,
            -MaxAngleDeg, MaxAngleDeg);

        _currentPitch = Smooth(_currentPitch, pitchTarget, dt, AngleTau);
        _currentRoll = Smooth(_currentRoll, rollTarget, dt, AngleTau);

        _telemetryDataData.Angles = new Vector3(_currentPitch, _currentRoll, _currentYawRate);
        _telemetryDataData.Velocity = new Vector3(_currentSurge, _currentSway, _currentHeave);
    }

    private static float Deadzone(float value)
    {
        var magnitude = Mathf.Abs(value);
        return magnitude <= AccelDeadzone ? 0f : Mathf.Sign(value) * (magnitude - AccelDeadzone);
    }

    private static float Smooth(float current, float target, float dt, float tau)
    {
        return tau <= 0f ? target : Mathf.Lerp(current, target, 1f - Mathf.Exp(-dt / tau));
    }

    private void UpdateVelocity()
    {
    }

    private void UpdateAngles()
    {
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

            // G-����
            float gForce = forwardAcceleration / 9.81f;

            float targetTiltAngle = 0f;

            // --- ������ ����� ---
            if (forwardSpeed > 2f && gForce < -0.3f)
            {
                float intensityValue = Mathf.Clamp01(Mathf.Abs(gForce) / 3f);
                BhapticsLibrary.Play(
                    eventId: "remen_bez",
                    startMillis: 0,
                    intensity: intensityValue,
                    duration: 1,
                    angleX: 0,
                    offsetY: 0
                );
                //Debug.Log("Bhaptics: remen_bez, intensity=" + intensityValue);
            }

            // --- ������ �������� � ����� ---
            if (forwardSpeed > 1f && gForce > 0.3f)
            {
                float intensityValue = Mathf.Clamp01(gForce / 2f);
                BhapticsLibrary.Play(
                    eventId: "davlenie_kovsha",
                    startMillis: 0,
                    intensity: intensityValue,
                    duration: 1,
                    angleX: 0,
                    offsetY: 0
                );
                //Debug.Log("Bhaptics: davlenie_kovsha, intensity=" + intensityValue);
            }

            // --- ������������ ������ ---
            // ������ ��� ��������� ����� � ������ �����, ��� ���������� � ���Ш�
            if (Mathf.Abs(forwardAcceleration) > accelerationThreshold)
            {
                if (forwardAcceleration > accelerationThreshold)
                {
                    // ������: ������ �����
                    targetTiltAngle = Mathf.Clamp(forwardAcceleration / 10f * maxTiltAngle, 0f, maxTiltAngle);
                }
                else if (forwardAcceleration < -accelerationThreshold)
                {
                    // ����������/������: ������ �����
                    targetTiltAngle = -Mathf.Clamp(-forwardAcceleration / 10f * maxTiltAngle, 0f, maxTiltAngle);
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


            if (debugTilt)
            {
                Debug.Log($"Accel={forwardAcceleration:F2}, G={gForce:F2}, Tilt={_currentTiltAngle:F2}");
            }
        }

        _previousVelocity = currentVelocity;
        _previousTime = currentTime;
    }

    /// <summary>
    /// БОЛЬШЕ НЕ ТРОГАЕТ ФИЗИКУ. Раньше здесь каждый тик вызывался
    /// rigidbody.MoveRotation(), который принудительно доворачивал кузов и
    /// воевал с подвеской, причём из корутины, а не из FixedUpdate.
    /// Из-за этого машины подпрыгивали и трясли капсулу.
    /// Наклон кузова должна задавать подвеска, а не телеметрия.
    /// </summary>
    private void ApplyTiltToRigidbody()
    {
        // намеренно пусто
    }

    public void SetTiltAngle(float angle)
    {
        _currentTiltAngle = Mathf.Clamp(angle, -maxTiltAngle, maxTiltAngle);
    }

    public void ResetTilt()
    {
        _currentTiltAngle = 0f;
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
