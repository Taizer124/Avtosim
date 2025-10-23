using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "VehicleBodySO", menuName = "CustomVehicleController/VehicleParts/VehicleBody")]
    public class VehicleBodySO : ScriptableObject, IVehiclePart
    {
        [Min(0)]
        public float Mass = 1500;
        [Min(0)]
        public float ForwardDrag = 0.07f;
        [Min(0)]
        public float Downforce = 10;
        [Min(0)]
        public float CorneringResistanceStrength = 12;
        public AnimationCurve CorneringResistanceCurve;

        public static VehicleBodySO CreateDefaultBodySO()
        {
            VehicleBodySO defaultBodySO = ScriptableObject.CreateInstance<VehicleBodySO>();
            defaultBodySO.Mass = 1500;
            defaultBodySO.ForwardDrag = 0.05f;
            defaultBodySO.CorneringResistanceStrength = 8f;
            defaultBodySO.Downforce = 35f;

            AnimationCurve curve = new();
            curve.AddKey(0f, 0.1f);
            curve.AddKey(1f, 1f);
            defaultBodySO.CorneringResistanceCurve = curve;

            defaultBodySO.name = "DefaultBody";

            return defaultBodySO;
        }
    }
}