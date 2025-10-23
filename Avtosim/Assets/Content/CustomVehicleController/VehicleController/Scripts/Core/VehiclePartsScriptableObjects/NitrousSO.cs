using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "NitrousSO", menuName = "CustomVehicleController/VehicleParts/Nitrous")]
    public class NitrousSO : ScriptableObject, IVehiclePart
    {
        [Min(0)]
        public float BoostAmount = 5000;
        [Min(1)]
        public int BottlesAmount = 1;
        [Min(0)]
        public float BoostIntensity = 5000;
        [Min(0)]
        public float BoostWarmUpTime = 0;
        public bool BoostDuringWarmUp = false;
        [Min(0)]
        public float RechargeRate = 2500;
        [Min(0)]
        public float RechargeDelay = 2;
        [Range(0f, 1f)]
        public float MinAmountPercentToUse = 0;
        public NitroBoostType BoostType;

        public static NitrousSO CreateDefaultNitroSO()
        {
            NitrousSO defaultNitroSO = ScriptableObject.CreateInstance<NitrousSO>();
            defaultNitroSO.BoostAmount = 3000;
            defaultNitroSO.BottlesAmount = 3;
            defaultNitroSO.BoostIntensity = 1500;
            defaultNitroSO.BoostWarmUpTime = 0.5f;
            defaultNitroSO.RechargeRate = 1000;
            defaultNitroSO.RechargeDelay = 2;
            defaultNitroSO.MinAmountPercentToUse = 0;
            defaultNitroSO.BoostType = NitroBoostType.Continuous;

            defaultNitroSO.name = "DefaultNitrosu";

            return defaultNitroSO;
        }
    }
}
