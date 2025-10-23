using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "TiresSO", menuName = "CustomVehicleController/VehicleParts/Tires")]
    public class TiresSO : ScriptableObject, IVehiclePart
    {
        [Min(0)]
        public float SteeringStiffness = 45f;
        public AnimationCurve SidewaysGripCurve;
        public AnimationCurve SidewaysSlipCurve;
        [Min(0)]
        public float ForwardGrip = 2.5f;

        public static TiresSO CreateDefaultTiresSO()
        {
            TiresSO defaultTires = ScriptableObject.CreateInstance<TiresSO>();
            defaultTires.SteeringStiffness = 45;
            defaultTires.ForwardGrip = 1.5f;

            AnimationCurve sidewaysGripCurve = new();
            sidewaysGripCurve.AddKey(0, 0.33f);
            sidewaysGripCurve.AddKey(1, 1);
            defaultTires.SidewaysGripCurve = sidewaysGripCurve;


            AnimationCurve sidewaysSlipCurve = new();
            Keyframe slip1 = new(0, 1, 0, 0.2f);
            Keyframe slip2 = new(0.8f, 0.4f, 0, 0);
            Keyframe slip3 = new(1, 1.1f, 1, 0);
            sidewaysSlipCurve.AddKey(slip1);
            sidewaysSlipCurve.AddKey(slip2);
            sidewaysSlipCurve.AddKey(slip3);

            defaultTires.SidewaysSlipCurve = sidewaysSlipCurve;

            defaultTires.name = "DefaultTires";

            return defaultTires;
        }
    }
}