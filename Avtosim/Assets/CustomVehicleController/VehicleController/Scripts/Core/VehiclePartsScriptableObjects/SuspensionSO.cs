using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "SuspensionSO", menuName = "CustomVehicleController/VehicleParts/Suspension")]
    public class SuspensionSO : ScriptableObject, IVehiclePart
    {
        [Range(0, 2f)]
        public float SpringRestDistance = 0.5f;
        [Min(0)]
        public float SpringTravelLength = 0.25f;
        [Min(0)]
        public float SpringStiffness = 70000;
        [Min(0)]
        public float SpringDampingStiffness = 1500;
        [Min(0)]
        public float AntiRollForce = 35000;

        public static SuspensionSO CreateDefaultSuspensionSO()
        {
            SuspensionSO defaultSuspension = ScriptableObject.CreateInstance<SuspensionSO>();
            defaultSuspension.SpringStiffness = 90000f;
            defaultSuspension.SpringDampingStiffness = 2000;
            defaultSuspension.SpringRestDistance = 0.37f;
            defaultSuspension.AntiRollForce = 45000f;
            defaultSuspension.SpringTravelLength = 0.2f;
            defaultSuspension.name = "DefaultSuspension";

            return defaultSuspension;
        }

        public event Action OnSuspensionStatsChanged;

        private void OnValidate()
        {
            OnSuspensionStatsChanged?.Invoke();
        }
    }
}