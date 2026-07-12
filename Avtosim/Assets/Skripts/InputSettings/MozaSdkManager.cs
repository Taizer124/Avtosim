using UnityEngine;
using System;
using mozaAPI;

namespace Assets.VehicleController
{
    public class MozaSdkManager : MonoBehaviour
    {
        private static MozaSdkManager _instance;
        public static MozaSdkManager Instance => _instance;

        private bool _isSdkInstalled = false;
        private AllInOneInputProvider _inputProvider;
        private int _limitAngle = 900;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            GameObject sdkGO = new GameObject("MOZA_SDK_Manager");
            sdkGO.AddComponent<MozaSdkManager>();
            DontDestroyOnLoad(sdkGO);
            Debug.Log("[MOZA SDK] Runtime Manager auto-created.");
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            try
            {
                mozaAPI.mozaAPI.installMozaSDK();
                _isSdkInstalled = true;
                Debug.Log("[MOZA SDK] SDK successfully installed.");

                ERRORCODE err = ERRORCODE.NORMAL;
                var limit = mozaAPI.mozaAPI.getMotorLimitAngle(ref err);
                if (err == ERRORCODE.NORMAL && limit != null)
                {
                    _limitAngle = Math.Abs(limit.Item2);
                    if (_limitAngle <= 0) _limitAngle = 900;
                    Debug.Log($"[MOZA SDK] Steering Wheel Limit Angle: {_limitAngle} degrees.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MOZA SDK] Failed to install SDK: {ex.Message}");
            }

            FindInputProvider();
        }

        private void FindInputProvider()
        {
            _inputProvider = FindFirstObjectByType<AllInOneInputProvider>();
        }

        private void Update()
        {
            if (!_isSdkInstalled) return;

            if (_inputProvider == null)
            {
                FindInputProvider();
                if (_inputProvider == null) return;
            }

            try
            {
                ERRORCODE err = ERRORCODE.NORMAL;
                HIDData data = mozaAPI.mozaAPI.getHIDData(ref err);

                if (err == ERRORCODE.NORMAL)
                {
                    // 1. Steering Wheel Angle
                    float steer = 0f;
                    if (!float.IsNaN(data.fSteeringWheelAngle))
                    {
                        float halfLimit = _limitAngle / 2f;
                        steer = Mathf.Clamp(data.fSteeringWheelAngle / halfLimit, -1f, 1f);
                    }
                    else
                    {
                        steer = (float)data.steeringWheelAxle / 32767f;
                    }

                    // 2. Pedals: throttle, brake, clutch (Int16 to 0..1f)
                    float gas = Mathf.Clamp01((float)(data.throttle - (-32768)) / 65535f);
                    float brake = Mathf.Clamp01((float)(data.brake - (-32768)) / 65535f);
                    float clutch = Mathf.Clamp01((float)(data.clutch - (-32768)) / 65535f);

                    // 3. Handbrake
                    bool handbrake = data.buttonHandbrake;
                    if (!handbrake)
                    {
                        float hbVal = Mathf.Clamp01((float)(data.handbrake - (-32767)) / 65534f);
                        handbrake = hbVal > 0.5f;
                    }

                    // 4. Shifter Gear
                    int gear = 0;
                    switch (data.shift)
                    {
                        case GEAR.GEAR0th:
                            gear = 0;
                            break;
                        case GEAR.GEAR1st:
                            gear = 1;
                            break;
                        case GEAR.GEAR2nd:
                            gear = 2;
                            break;
                        case GEAR.GEAR3rd:
                            gear = 3;
                            break;
                        case GEAR.GEAR4th:
                            gear = 4;
                            break;
                        case GEAR.GEAR5th:
                            gear = 5;
                            break;
                        case GEAR.GEAR6th:
                            gear = 6;
                            break;
                        case GEAR.GEAR7th:
                            gear = 7;
                            break;
                        case GEAR.R:
                            gear = -1;
                            break;
                    }

                    _inputProvider.SetMozaInputs(gas, brake, clutch, steer, handbrake, gear);

                    // 5. Кнопки Y/X/B/A — соответствуют North/West/East/South у
                    // Logitech G29 (та же Xbox-style раскладка, физически то же
                    // место на руле). Номера взяты из официального MOZA Button
                    // Numbering Guide для раскладки ES (support.mozaracing.com):
                    // 1=A(низ/Юг), 2=B(право/Восток), 3=Y(верх/Север), 4=X(лево/Запад),
                    // переведены в 0-based индекс data.buttons[номер-1].
                    // НЕ проверено на реальном железе — подтверди после подключения.
                    bool north = false, south = false, east = false, west = false;
                    if (data.buttons != null)
                    {
                        if (data.buttons.Length > 0) south = data.buttons[0].startValue; // 1 = A
                        if (data.buttons.Length > 1) east = data.buttons[1].startValue;  // 2 = B
                        if (data.buttons.Length > 2) north = data.buttons[2].startValue; // 3 = Y
                        if (data.buttons.Length > 3) west = data.buttons[3].startValue;  // 4 = X
                    }
                    _inputProvider.SetMozaButtons(north, south, east, west);
                }
                else
                {
                    _inputProvider.SetMozaDisconnected();
                }
            }
            catch (Exception)
            {
                if (_inputProvider != null)
                    _inputProvider.SetMozaDisconnected();
            }
        }

        private void OnDestroy()
        {
            if (_isSdkInstalled)
            {
                try
                {
                    mozaAPI.mozaAPI.removeMozaSDK();
                    Debug.Log("[MOZA SDK] SDK successfully removed.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MOZA SDK] Failed to remove SDK: {ex.Message}");
                }
            }
        }
    }
}
