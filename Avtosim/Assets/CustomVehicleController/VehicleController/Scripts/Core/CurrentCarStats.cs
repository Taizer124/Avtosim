using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "CurrentCarStatsSO", menuName = "CustomVehicleController/CurrentCarStats")]
    public class CurrentCarStats : ScriptableObject
    {
        public List<GameObject> ScriptableObjectOwners;
        public void Reset()
        {
            SpeedInMsPerS = 0;
            SpeedPercent = 0;
            EngineRPM = 0;
            EngineRPMPercent = 0;
            ForcedInductionBoostPercent = 0;
            ForcedInductionBoostPressureCurrent = 0;
            ForcedInductionBoostPressureMax = 0;
            CurrentGear = "N";
            Accelerating = false;
            Braking = false;
            BrakingIntensity = 0;
            NitroBoosting = false;
            NitroPercentLeft = 1f;
            NitroBottlesLeft = 1;
            NitroIntensity = 0;
            AccelerationForce = 0;
            SidewaysForce = 0;
            Reversing = false;
            InAir = false;
            DriveWheelsGrounded = false;
            DriftAngle = 0;
            DriftTime = 0;
            AirTime = 0;
            HandbrakePulled = false;
            IsCarSlipping = false;
            TCSworking = false;

            if (ScriptableObjectOwners != null)
                ScriptableObjectOwners.Clear();
        }

        public event Action OnAntiLag;
        public void AntiLagHappened() => OnAntiLag?.Invoke();
        public event Action OnShiftedAntiLag;
        public void ShiftedAntiLagHappened() => OnShiftedAntiLag?.Invoke();
        public event Action<string> OnGearShifted;
        public void TransmissionShiftedGear() => OnGearShifted?.Invoke(CurrentGear);

        public float SpeedInMsPerS;
        public float SpeedInKMperH => SpeedInMsPerS * 3.6f;
        public float SpeedInMilesPerH => SpeedInMsPerS * 2.23693629f;
        public float SpeedPercent;
        public string CurrentGear;
        public float MinRPM;
        public float EngineRPM;
        public float EngineRPMPercent;
        public float ForcedInductionBoostPercent;
        public float ForcedInductionBoostPressureMax;
        public float ForcedInductionBoostPressureCurrent;
        public float CurrentEngineTorque;
        public float CurrentEngineHorsepower => CurrentEngineTorque * EngineRPM / 5252f;
        public bool Accelerating;
        public bool Braking;
        public float BrakingIntensity;
        public bool NitroBoosting;
        public int NitroBottlesLeft;
        public float NitroPercentLeft;
        public float EngineMaxRPMChangeMultiplier;
        public float NitroIntensity;
        public float AccelerationForce;
        public float SidewaysForce;
        public bool Reversing;
        public bool AllWheelsGrounded;
        public bool DriveWheelsGrounded;
        public bool InAir;
        public float DriftAngle;
        public float DriftTime;
        public float AirTime;
        public bool HandbrakePulled;
        public bool IsCarSlipping;
        public bool[] WheelSlipArray;
        public bool TCSworking;
        
    }
}