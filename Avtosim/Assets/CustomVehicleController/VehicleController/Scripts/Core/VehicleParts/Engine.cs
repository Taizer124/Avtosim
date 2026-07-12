using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class Engine : IEngine
    {
        private VehiclePartsSetWrapper _partsPresetWrapper;

        private ForcedInduction _forcedInduction;
        private NitrousBoost _nitrousBoost;

        private IShifter _shifter;
        private ITransmission _transmission;

        private List<CustomEnginePart> _customEngineParts;

        private CurrentCarStats _currentCarStats;

        private float _totalTorque;

        public void Initialize(CurrentCarStats currentCarStats, VehiclePartsSetWrapper partsPresetWrapper, List<CustomEnginePart> customEngineParts, IShifter shifter, ITransmission transmission)
        {
            _partsPresetWrapper = partsPresetWrapper;
            _currentCarStats = currentCarStats;
            _shifter = shifter;
            _transmission = transmission;

            _customEngineParts = customEngineParts;

            _forcedInduction = new(_partsPresetWrapper, _currentCarStats, _transmission);
            _nitrousBoost = new(_partsPresetWrapper, _currentCarStats);
        }

        public void Accelerate(VehicleAxle[] driveAxleArray, float gasInput, float breakInput, bool nitroBoostInput, float rpm)
        {
            if (_shifter.InNeutralGear())
            {
                _totalTorque = 0;
                SetTorque(0, driveAxleArray);
                return;
            }

            float input = _transmission.DetermineGasInput(gasInput, breakInput);
            float forcedInductionBoost = _forcedInduction.GetForcedInductionBoost(_transmission.InShiftingCooldown() ? 0 : Mathf.Abs(input));
            float nitroBoost = _nitrousBoost.GetNitroBoost(nitroBoostInput);

            float defaultEngineTorque = CalculateAccelerationForce(input, rpm);
            float customEnginePartsTorque = GetBoostFromParts(Mathf.Sign(defaultEngineTorque), input);
            _totalTorque = defaultEngineTorque + customEnginePartsTorque;

            float clutch = 0f;
            if (_partsPresetWrapper.Owner != null)
            {
                clutch = _partsPresetWrapper.Owner.GetClutchInput();
            }
            float clutchFactor = Mathf.Clamp01(1.0f - clutch);

            SetTorque((_totalTorque * (1 + forcedInductionBoost * 0.07f) + nitroBoost) * clutchFactor, driveAxleArray);
        }

        public float GetForcedInductionBoostPressureMax()
        {
            return _forcedInduction.GetForcedInductionBoostPressureMax();
        }

        private float GetBoostFromParts(float sign, float input)
        {
            float boost = 0;
            float rpmPercent = _currentCarStats.EngineRPMPercent;
            int size = _customEngineParts.Count;
            for(int i = 0; i < size; i++)
            {
                if (_customEngineParts[i] == null)
                    continue;

                float addition = _customEngineParts[i].Torque;
                if (_customEngineParts[i].NonLinearBoost && _customEngineParts[i].EffectCurve != null)
                    addition *= _customEngineParts[i].EffectCurve.Evaluate(rpmPercent);

                boost += addition;
            }

            return boost * input * sign;
        }

        private float CalculateAccelerationForce(float input, float rpm)
        {
            if (input == 0 && _currentCarStats.AllWheelsGrounded)
            {
                //engine braking;
                float sign = (_shifter.InReverseGear() && _currentCarStats.SpeedInMsPerS < 0) || _currentCarStats.SpeedInMsPerS < 0 ? -1 : 1;
                float engineBrakingMultiplierFromGear = _partsPresetWrapper.Transmission.GearRatiosList[_shifter.GetCurrentGearID()];
                return -CalculateTorque(rpm) * engineBrakingMultiplierFromGear * sign * 0.1f * _currentCarStats.EngineRPMPercent;
            }

            if (_transmission.Redlining())
            {
                float mult = (_shifter.InReverseGear() && _currentCarStats.SpeedInMsPerS < 0) || _currentCarStats.SpeedInMsPerS < 0 ? -0.2f : 0.2f;
                return -CalculateTorque(rpm) * mult * Mathf.Abs(input);
            }

            if (_partsPresetWrapper.Engine.MaxSpeed < _currentCarStats.SpeedInKMperH)
                return -CalculateTorque(rpm) * input;

            if (_transmission.InShiftingCooldown())
                return 0;

            return CalculateTorque(rpm) * input;
        }

        private float CalculateTorque(float rpm)
        {
            float idleRpmChangeFromPerformanceModifications = _transmission.GetModifiedMinRPM() - _transmission.GetMinRPM();
            rpm -= idleRpmChangeFromPerformanceModifications;

            if (_currentCarStats.EngineMaxRPMChangeMultiplier > 0)
            {
                rpm /= _currentCarStats.EngineMaxRPMChangeMultiplier;
                rpm += idleRpmChangeFromPerformanceModifications;
            }

            return _partsPresetWrapper.Engine.TorqueCurve.Evaluate(rpm) * _partsPresetWrapper.Transmission.GearRatiosList[_shifter.GetCurrentGearID()] *
                _partsPresetWrapper.Transmission.FinalDriveRatio;
        }

        private void SetTorque(float torque, VehicleAxle[] driveAxleArray)
        {
            int size = driveAxleArray.Length;
            float torqueToApply = torque / (size * 2);
            for (int i = 0; i < size; i++)
            {
                driveAxleArray[i].ApplyTorque(torqueToApply / driveAxleArray[i].LeftHalfShaft.WheelController.WheelRadius);
            }
        }

        public float GetCurrentTorque() => Mathf.Abs(_totalTorque);
        public float GetForcedInductionBoostPercent() => _forcedInduction.GetForcedInductionBoostPercent();

        public void AddNitro(float amount) => _nitrousBoost.AddNitro(amount);
    }
}
