using _2DOF;
using System.Collections;
using TMPro;
using UnityEngine;
public class CarTelemetryHandler1 : MonoBehaviour
{
    private const float WAIT_TIME = SendingData.WAIT_TIME / 1000f;

    private ObjectTelemetryData telemetryDataData;
    private SendingData _sendingData;

    [SerializeField] private Transform vehicleTransform;
    [SerializeField] private Rigidbody rb;

    private const float maxPlatformAngle = 15f; // Максимальные Angles платформы 2DOF (влияет на статичные наклоны, зависящие от поверхности)
    private const float maxPlatformVelocity = 100f; // Максимальная Velocity платформы 2DOF (влияет на наклоны в зависимости от линейного ускорения/угловой скорости)
    private float currentPitch = 0f; // текущий наклон платформы 2DOF по x (учет наклона поверхности)
    private float currentRoll = 0f; // текущий наклон платформы 2DOF по z (учет наклона поверхности)
    private float currentLinearAcceleration = 0f; // текущий наклон платформы 2DOF по x (учет линейного ускорения)
    private float lastLinearVelocity = 0f;
    private float currentAngularVelocity = 0f; // текущий наклон платформы 2DOF по z (учет угловой скорости)

    private void Awake()
    {
        _sendingData = new SendingData();
        telemetryDataData = _sendingData.ObjectTelemetryData;
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
            if (telemetryDataData == null)
            {
                yield return new WaitForSeconds(WAIT_TIME * 10f);
                continue;
            }

            UpdatePlatformVelocity();
            UpdatePlatformAngles();

            yield return new WaitForSeconds(WAIT_TIME);
        }
    }
    private float NormalizeAngle(float angle) // нормализуем угол в диапазон -180 до 180
    {
        angle = angle > 180 ? angle - 360 : angle;
        return angle;
    }

    private void UpdatePlatformVelocity() // отправка данных о скорости на платформу
    {
        Vector3 globalLinearVelocity = rb.linearVelocity;
        Vector3 localLinearVelocity = transform.InverseTransformVector(globalLinearVelocity); // считаем линейную скорость относительно локальных координат

        // считаем линейное ускорение
        float linearAcceleration = (localLinearVelocity.z - lastLinearVelocity) / Time.deltaTime;
        lastLinearVelocity = localLinearVelocity.z;

        linearAcceleration = Mathf.Clamp(linearAcceleration, -maxPlatformVelocity, maxPlatformVelocity);

        currentLinearAcceleration = Mathf.Lerp(currentLinearAcceleration, linearAcceleration, 0.02f);

        Vector3 globalAngularVelocity = rb.angularVelocity;
        Vector3 localAngularVelocity = transform.InverseTransformVector(globalAngularVelocity); // считаем угловую скорость относительно локальных координат

        currentAngularVelocity = Mathf.Lerp(currentAngularVelocity, Mathf.Clamp(localAngularVelocity.y, -maxPlatformVelocity, maxPlatformVelocity), 0.03f);


        telemetryDataData.Angles = gameObject.transform.eulerAngles;
        telemetryDataData.Velocity = new Vector3(currentLinearAcceleration * 50, currentAngularVelocity * 160, 0);



    }

    private void UpdatePlatformAngles()
    {
        float targetPitch = 0;

        targetPitch = NormalizeAngle(vehicleTransform.eulerAngles.x); // учет наклона поверхности
        targetPitch = Mathf.Clamp(targetPitch, -maxPlatformAngle, maxPlatformAngle);

        currentPitch = Mathf.Lerp(currentPitch, targetPitch, 0.04f);

        //---------------------------------------------------------------------------
        float targetRoll = 0;

        targetRoll = NormalizeAngle(vehicleTransform.eulerAngles.z);
        targetRoll = Mathf.Clamp(targetRoll, -maxPlatformAngle, maxPlatformAngle);

        currentRoll = Mathf.Lerp(currentRoll, targetRoll, 0.04f);

        Vector3 resultAngles = new Vector3(currentPitch, currentRoll, 0); // конечный возврат углов для передачи данных в платформу
        telemetryDataData.Angles = resultAngles;
    }

    private void OnCollisionEnter(Collision collision)
    {
        float CollisionIntensity = Mathf.Abs(lastLinearVelocity * 3.6f) / 100;

        CollisionIntensity = Mathf.Clamp01(CollisionIntensity);
    }
}

