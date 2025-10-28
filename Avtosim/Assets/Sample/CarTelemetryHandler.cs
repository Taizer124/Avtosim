/*
using System;
using System.Collections;
using _2DOF;
using UnityEngine;

public class CarTelemetryHandler : MonoBehaviour
{
    private const float WAIT_TIME = SendingData.WAIT_TIME / 1000f;

    [Header("References")]
    [SerializeField] private Transform vehicleTransform;
    [SerializeField] private Rigidbody rigidbody1;

    [Header("Settings")]
    [SerializeField] private bool debugMode = true;

    private ObjectTelemetryData _telemetryDataData;
    private SendingData _sendingData;
    private Coroutine _telemetryCoroutine;

    private void Awake()
    {
        _sendingData = new SendingData();
        _telemetryDataData = _sendingData.ObjectTelemetryData;

        // Проверка обязательных компонентов
        if (vehicleTransform == null)
        {
            vehicleTransform = transform;
            Debug.LogWarning("CarTelemetryHandler: vehicleTransform not assigned, using self transform");
        }

        if (rigidbody1 == null)
        {
            rigidbody1 = GetComponent<Rigidbody>();
            if (rigidbody1 == null)
                Debug.LogError("CarTelemetryHandler: No Rigidbody found!");
        }
    }

    public void OnEnable()
    {
        if (_sendingData != null)
        {
            _sendingData.SendingStart();
        }

        _telemetryCoroutine = StartCoroutine(TelemetryHandler());
    }

    public void OnDisable()
    {
        if (_telemetryCoroutine != null)
        {
            StopCoroutine(_telemetryCoroutine);
            _telemetryCoroutine = null;
        }

        if (_sendingData != null)
        {
            _sendingData.SendingStop();
        }
    }

    private IEnumerator TelemetryHandler()
    {
        while (true)
        {
            // Проверка всех необходимых компонентов
            if (_telemetryDataData == null || vehicleTransform == null || rigidbody1 == null)
            {
                Debug.LogWarning("TelemetryHandler: Missing required components, waiting...");
                yield return new WaitForSeconds(WAIT_TIME * 2f);
                continue;
            }

            try
            {
                UpdateAngles();
                UpdateVelocity();

                if (debugMode)
                {
                    Debug.Log($"Telemetry - Angles: {_telemetryDataData.Angles}, Velocity: {_telemetryDataData.Velocity}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"TelemetryHandler error: {e.Message}");
            }

            yield return new WaitForSeconds(WAIT_TIME);
        }
    }

    private void UpdateVelocity()
    {
        if (rigidbody1 != null)
        {
            // Используем правильное свойство для velocity
#if UNITY_6000_0_OR_NEWER
            _telemetryDataData.Velocity = rigidbody1.linearVelocity;
#else
            _telemetryDataData.Velocity = rigidbody.velocity;
#endif
        }
    }

    private void UpdateAngles()
    {
        Vector3 euler = vehicleTransform.eulerAngles;

        // Нормализация углов в диапазон [-180, 180]
        _telemetryDataData.Angles = new Vector3(
            NormalizeAngle(euler.x),
            NormalizeAngle(euler.y),
            NormalizeAngle(euler.z)
        );
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f)
            return angle - 360f;
        return angle;
    }

    // Дополнительный метод для сброса при необходимости
    public void ResetTelemetry()
    {
        if (_telemetryDataData != null)
        {
            _telemetryDataData.Angles = Vector3.zero;
            _telemetryDataData.Velocity = Vector3.zero;
        }
    }
}
*/
using System;
using System.Collections;
using _2DOF;
using UnityEngine;

public class CarTelemetryHandler : MonoBehaviour
{
    private const float WAIT_TIME = SendingData.WAIT_TIME / 1000f;

    [SerializeField] private Transform vehicleTransform;
    [SerializeField] private Rigidbody rigidbody;

    private ObjectTelemetryData _telemetryDataData;
    private SendingData _sendingData;

    private void Awake()
    {
        _sendingData = new SendingData();
        _telemetryDataData = _sendingData.ObjectTelemetryData;
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

        euler.x = Mathf.Approximately(euler.x, 180) ? 0 : euler.x;
        euler.z = Mathf.Approximately(euler.z, 180) ? 0 : euler.z;
        euler.y = Mathf.Approximately(euler.y, 180) ? 0 : euler.y;

        euler.x = euler.x > 180 ? euler.x - 360 : euler.x;
        euler.z = euler.z > 180 ? euler.z - 360 : euler.z;
        euler.y = euler.y > 180 ? euler.y - 360 : euler.y;

        _telemetryDataData.Angles = euler;
    }
}
