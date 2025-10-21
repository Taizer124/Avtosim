using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "TransmissionSO", menuName = "CustomVehicleController/VehicleParts/Transmission")]
    public class TransmissionSO : ScriptableObject, IVehiclePart
    {
        public List<float> GearRatiosList;
        [Min(0.1f)]
        public float FinalDriveRatio = 3.3f;
        [Min(0f)]
        public float ShiftCooldown = 0.15f;
        [Range(0f, 0.99f)]
        public float UpShiftRPMPercent = 0.95f;
        [Range(0f, 0.94f)]
        public float DownShiftRPMPercent = 0.75f;

        public static TransmissionSO CreateDefaultTransmissionSO()
        {
            TransmissionSO defaultTransmissionSO = ScriptableObject.CreateInstance<TransmissionSO>();
            List<float> gearList = new List<float>
            {
                3.45f,
                2.633659f,
                2.010481f,
                1.53476f,
                1.171605f
            };
            defaultTransmissionSO.GearRatiosList = gearList;
            defaultTransmissionSO.FinalDriveRatio = 3;
            defaultTransmissionSO.ShiftCooldown = 0.2f;
            defaultTransmissionSO.UpShiftRPMPercent = 0.98f;
            defaultTransmissionSO.DownShiftRPMPercent = 0.8f;

            defaultTransmissionSO.name = "DefaultTransmission";

            return defaultTransmissionSO;
        }

        public event Action OnTransmissionStatsChanged;

        private void OnValidate()
        {
            OnTransmissionStatsChanged?.Invoke();
        }
    }
}