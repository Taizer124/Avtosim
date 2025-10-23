using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "ForcedInductionSO", menuName = "CustomVehicleController/VehicleParts/ForcedInduction")]
    public class ForcedInductionSO : ScriptableObject, IVehiclePart
    {
        public ForcedInductionType ForcedInductionType;
        [Min(0)]
        public float MaxBoostPressure = 10;
        [Range(0, 1f)]
        public float TurboRPMPercentDelay = 0.2f;
        [Min(0.1f)]
        public float TurboSpoolUpTime = 0.33f;
        [Min(0.1f)]
        public float TurboSpoolDownTime = 0.5f;
        public bool AntiLagSystemInstalled = false;
        [Range(0f, 1f)]
        public float AntiLagEffect = 0.5f;
        [Range(0, 1f)]
        public float AntiLagVisualEffectChance = 0.7f;

        public static ForcedInductionSO CreateDefaultForcedInductionSO()
        {
            ForcedInductionSO defaultFISO = ScriptableObject.CreateInstance<ForcedInductionSO>();
            defaultFISO.ForcedInductionType = ForcedInductionType.Turbocharger;
            defaultFISO.MaxBoostPressure = 30;
            defaultFISO.TurboRPMPercentDelay = 0.2f;
            defaultFISO.TurboSpoolUpTime = 0.5f;
            defaultFISO.AntiLagSystemInstalled = false;
            defaultFISO.AntiLagEffect = 0.7f;
            defaultFISO.AntiLagVisualEffectChance = 0.5f;
            defaultFISO.TurboSpoolDownTime = 1f;

            defaultFISO.name = "DefaultForcedInduction";

            return defaultFISO;
        }
    }
}