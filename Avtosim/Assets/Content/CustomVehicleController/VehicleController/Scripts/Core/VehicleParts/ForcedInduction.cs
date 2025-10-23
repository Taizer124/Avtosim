using UnityEngine;

namespace Assets.VehicleController
{
    public class ForcedInduction
    {
        private VehiclePartsSetWrapper _partsPresetWrapper;
        private CurrentCarStats _currentCarStats;

        private float _boostPercent;
        private float _lastShiftTime;

        private bool _antiLagHappened;

        public ForcedInduction(VehiclePartsSetWrapper partsPresetWrapper, CurrentCarStats currentCarStats, ITransmission transmission)
        {
            _partsPresetWrapper = partsPresetWrapper;
            _currentCarStats = currentCarStats;
            transmission.OnShifted += _transmission_OnShifted;
        }

        //simulate the driver leaving the foot off the throttle when shifting, thus decreasing the forced induction boost
        private void _transmission_OnShifted()
        {
            if (_partsPresetWrapper.ForcedInduction == null)
                return;

            if (_partsPresetWrapper.ForcedInduction.ForcedInductionType != ForcedInductionType.Turbocharger)
                return;

            HandleAntiLag();
            _lastShiftTime = Time.time;
        }

        private void HandleAntiLag()
        {
            if (_boostPercent >= 1)
            {
                if (_partsPresetWrapper.ForcedInduction.AntiLagSystemInstalled)
                {
                    _antiLagHappened = true;

                    if (_partsPresetWrapper.ForcedInduction.AntiLagVisualEffectChance == 0)
                        return;
                    if (Random.Range(0, 1f) < _partsPresetWrapper.ForcedInduction.AntiLagVisualEffectChance)
                    {
                        _currentCarStats.AntiLagHappened();
                    }
                }
                else
                    _antiLagHappened = false;
            }
        }

        public float GetForcedInductionBoost(float gasInput)
        {
            if (_partsPresetWrapper.ForcedInduction == null)
            {
                _boostPercent = 0;
                return 0;
            }

            switch (_partsPresetWrapper.ForcedInduction.ForcedInductionType)
            {
                case ForcedInductionType.Centrifugal:
                    return GetCentrifugalBoost();
                case ForcedInductionType.Supercharger:
                    return GetSuperchargerBoost();
                case ForcedInductionType.Turbocharger:
                    return GetTurbochargerBoost(gasInput);
                default:
                    _boostPercent = 0;
                    return 0;
            }
        }

        public float GetForcedInductionBoostPercent() => Mathf.Clamp01(_boostPercent);
        public float GetForcedInductionBoostPressureMax() => _partsPresetWrapper.ForcedInduction != null ? _partsPresetWrapper.ForcedInduction.MaxBoostPressure : 0;

        //even though supercharger provides boost at all times, set the forced induction boost percent to the engine RPM percent 
        //so that the supercharger sound pitch depends on the engine rpm
        private float GetSuperchargerBoost()
        {
            _boostPercent = _currentCarStats.EngineRPMPercent;
            return _partsPresetWrapper.ForcedInduction.MaxBoostPressure;
        }

        //centrifugal supercharger provides boost corresponding to the engine RPM
        private float GetCentrifugalBoost()
        {
            float percent = _currentCarStats.EngineRPMPercent;
            _boostPercent = percent;
            return percent * _partsPresetWrapper.ForcedInduction.MaxBoostPressure;
        }

        //turbocharger provides boost based on gas input
        private float GetTurbochargerBoost(float gasInput)
        {
            float dropSpeed = _partsPresetWrapper.ForcedInduction.TurboSpoolDownTime;

            if (_antiLagHappened)
            {
                dropSpeed /= 1 - _partsPresetWrapper.ForcedInduction.AntiLagEffect;
            }

            if (Time.time <= _lastShiftTime + _partsPresetWrapper.Transmission.ShiftCooldown)
            {
                _boostPercent -= Time.deltaTime / dropSpeed * _boostPercent;

                if (_boostPercent < 0)
                    _boostPercent = 0;

                return 0;
            }

            if (gasInput > 0)
            {
                _antiLagHappened = false;
                if (_currentCarStats.EngineRPMPercent > _partsPresetWrapper.ForcedInduction.TurboRPMPercentDelay)
                {
                    _boostPercent += gasInput * Time.deltaTime / _partsPresetWrapper.ForcedInduction.TurboSpoolUpTime;
                    _boostPercent = Mathf.Clamp(_boostPercent, 0, gasInput);
                }
                else
                {
                    _boostPercent -= Time.deltaTime / dropSpeed * _boostPercent;
                }
            }
            else
            {
                HandleAntiLag();
                _boostPercent -= Time.deltaTime / dropSpeed * _boostPercent;
            }

            _boostPercent = Mathf.Clamp01(_boostPercent);

            return _boostPercent * _partsPresetWrapper.ForcedInduction.MaxBoostPressure;
        }
    }
}

