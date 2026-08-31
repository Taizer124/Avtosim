using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// ВРЕМЕННЫЙ диагностический компонент. Каждый FixedUpdate снимает состояние
    /// коробки активного игрока и записывает СОБЫТИЯ (смена передачи, вход в
    /// нейтраль, залипание на отсечке) в статический буфер, который читается
    /// снаружи. Ставится на время отладки и потом удаляется вместе с файлом.
    /// </summary>
    public class GearTelemetry : MonoBehaviour
    {
        public static readonly List<string> Events = new List<string>();
        private const int MAX_EVENTS = 400;

        private CustomVehicleController _cvc;
        private object _transmission;
        private object _shifter;

        private FieldInfo _fPartsManager, _fTransmission, _fShifter;
        private FieldInfo _fUpShift, _fLastShiftTime, _fMaxRPM, _fCurRPM;
        private PropertyInfo _pInCooldown;
        private MethodInfo _mGearId, _mNeutral, _mReverse;
        private Rigidbody _rb;

        private int _lastGear = -999;
        private bool _lastNeutral, _lastReverse;
        private int _redlineFrames;
        private float _lastRedlineReport;

        // Ловля NaN в колёсах: сообщаем ТОЛЬКО про первый случай на машину,
        // иначе лог зальёт каждым кадром.
        private WheelController[] _wheels;
        private bool _nanReported;
        private static readonly System.Type WheelType = typeof(WheelController);

        private void Add(string s)
        {
            Events.Add(s);
            if (Events.Count > MAX_EVENTS) Events.RemoveAt(0);
        }

        // Игрок меняется на гонку и обратно (городская машина выключается,
        // спавнится гоночная). Выключенный объект в Unity НЕ равен null,
        // поэтому кэш надо сбрасывать по activeInHierarchy, иначе телеметрия
        // так и продолжит читать заглушенную городскую машину и гонку не увидит.
        private CustomVehicleController FindActivePlayer()
        {
            var p = PlayerLocator.GetActivePlayer();
            if (p != null && p.gameObject.activeInHierarchy)
                return p;

            // Кэш PlayerLocator протух — ищем напрямую (только в момент подмены).
            foreach (var c in FindObjectsByType<CustomVehicleController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (c.gameObject.activeInHierarchy && c.CompareTag("Player"))
                    return c;
            return null;
        }

        private bool Resolve()
        {
            bool stale = _cvc == null || !_cvc.gameObject.activeInHierarchy;
            if (stale)
            {
                var found = FindActivePlayer();
                if (found == null) return false;
                if (!ReferenceEquals(found, _cvc))
                {
                    _cvc = found;
                    _transmission = null;
                    _shifter = null;
                    _rb = _cvc.GetComponent<Rigidbody>();
                    _wheels = _cvc.GetComponentsInChildren<WheelController>(true);
                    _nanReported = false;
                    _lastGear = -999;
                    Add($"[{Time.time:F2}] === СМЕНА МАШИНЫ: {_cvc.gameObject.name}, тип={_cvc.TransmissionType}, привод={_cvc.DrivetrainType}, UsePreset={_cvc.UsePreset} ===");
                }
            }

            if (_transmission == null)
            {
                var bf = BindingFlags.NonPublic | BindingFlags.Instance;
                _fPartsManager = typeof(CustomVehicleController).GetField("_partsManager", bf);
                var pm = _fPartsManager?.GetValue(_cvc);
                if (pm == null) return false;
                _fTransmission = pm.GetType().GetField("_transmission", bf);
                _transmission = _fTransmission?.GetValue(pm);
                if (_transmission == null) return false;

                var tt = _transmission.GetType();
                _fShifter = tt.GetField("_shifter", bf);
                _fUpShift = tt.GetField("_upShiftRPM", bf);
                _fLastShiftTime = tt.GetField("_lastShiftTime", bf);
                _fMaxRPM = tt.GetField("_modifiedEngineMaxRPM", bf);
                _fCurRPM = tt.GetField("_currentEngineRPM", bf);
                _pInCooldown = tt.GetProperty("InCooldown", bf);
                _shifter = _fShifter?.GetValue(_transmission);
                if (_shifter == null) return false;
                var st = _shifter.GetType();
                _mGearId = st.GetMethod("GetCurrentGearID");
                _mNeutral = st.GetMethod("InNeutralGear");
                _mReverse = st.GetMethod("InReverseGear");
            }
            return _transmission != null && _shifter != null;
        }

        private static bool Bad(float v) => float.IsNaN(v) || float.IsInfinity(v);

        /// <summary>
        /// Ищет момент, когда состояние колеса впервые становится NaN/Infinity.
        /// Именно отсюда потом растёт и дым (флаг пробуксовки), и залипший
        /// кватернион поворота колеса, и NaN-сила на Rigidbody.
        /// </summary>
        private void CheckWheelsForNaN()
        {
            if (_nanReported || _wheels == null) return;

            var bf = BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (var w in _wheels)
            {
                if (w == null) continue;

                float speedRpm = w.WheelRPM;
                float visualRpm = w.VisualRPM;
                if (!Bad(speedRpm) && !Bad(visualRpm)) continue;

                float radius = (float)(WheelType.GetField("_wheelRadius", bf)?.GetValue(w) ?? 0f);
                float slip = (float)(WheelType.GetField("_slipWheelRPM", bf)?.GetValue(w) ?? 0f);
                Vector3 vel = _rb != null ? _rb.linearVelocity : Vector3.zero;

                Add($"[{Time.time:F2}] !!! NaN В КОЛЕСЕ {w.gameObject.name}: " +
                    $"speedRPM={speedRpm} visualRPM={visualRpm} slipRPM={slip} radius={radius} " +
                    $"torque={w.Torque} fwdSlip={w.ForwardSlip} наЗемле={w.HasContactWithGround} " +
                    $"| скорость rb={vel} (NaN={Bad(vel.x) || Bad(vel.y) || Bad(vel.z)})");
                _nanReported = true;
                return;
            }
        }

        private void FixedUpdate()
        {
            if (!Resolve()) return;
            CheckWheelsForNaN();

            int gear = (int)_mGearId.Invoke(_shifter, null);
            bool neutral = (bool)_mNeutral.Invoke(_shifter, null);
            bool reverse = (bool)_mReverse.Invoke(_shifter, null);
            float curRPM = (float)_fCurRPM.GetValue(_transmission);
            float maxRPM = (float)_fMaxRPM.GetValue(_transmission);
            float upShift = (float)_fUpShift.GetValue(_transmission);
            float lastShift = (float)_fLastShiftTime.GetValue(_transmission);
            bool cooldown = _pInCooldown != null && (bool)_pInCooldown.GetValue(_transmission);
            float kmh = _rb != null ? _rb.linearVelocity.magnitude * 3.6f : 0f;

            // Событие: сменилось состояние коробки
            if (gear != _lastGear || neutral != _lastNeutral || reverse != _lastReverse)
            {
                string state = reverse ? "ЗАДНЯЯ" : (neutral ? "НЕЙТРАЛЬ" : "передача " + (gear + 1));
                Add($"[{Time.time:F2}] {state} (idx={gear}) | {kmh:F0} км/ч | RPM {curRPM:F0}/{maxRPM:F0} | порог {upShift:F0} | cooldown={cooldown} | сЛастШифта {(Time.time - lastShift):F2}с");
                _lastGear = gear;
                _lastNeutral = neutral;
                _lastReverse = reverse;
            }

            // Событие: залипание на отсечке
            if (curRPM >= maxRPM * 0.97f)
            {
                _redlineFrames++;
                if (_redlineFrames > 25 && Time.time - _lastRedlineReport > 1f)
                {
                    string state = reverse ? "ЗАДНЯЯ" : (neutral ? "НЕЙТРАЛЬ" : "передача " + (gear + 1));
                    Add($"[{Time.time:F2}] !! ОТСЕЧКА {_redlineFrames} кадров на {state} | {kmh:F0} км/ч | RPM {curRPM:F0}/{maxRPM:F0} | порог {upShift:F0} | RPM>порог={(curRPM > upShift)} | cooldown={cooldown}");
                    _lastRedlineReport = Time.time;
                }
            }
            else _redlineFrames = 0;
        }
    }
}
