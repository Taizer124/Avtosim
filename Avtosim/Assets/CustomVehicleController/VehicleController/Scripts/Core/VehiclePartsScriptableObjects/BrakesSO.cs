using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "BreaksSO", menuName = "CustomVehicleController/VehicleParts/Breaks")]
    public class BrakesSO : ScriptableObject, IVehiclePart
    {
        [Min(0)]
        public float BrakesStrength;
        [Min(0)]
        public float HandbrakeForce;
        [Range(0, 1f)]
        public float HandbrakeTractionPercent;

        public static BrakesSO CreateDefaultBrakesSO()
        {
            BrakesSO defaultBrakes = ScriptableObject.CreateInstance<BrakesSO>();
            defaultBrakes.BrakesStrength = 30000;
            defaultBrakes.HandbrakeForce = 12000;
            defaultBrakes.HandbrakeTractionPercent = 0.1f;
            defaultBrakes.name = "DefaultBrakes";
            return defaultBrakes;
        }
    }
}