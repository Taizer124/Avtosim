using UnityEngine;

namespace Assets.VehicleController
{
    public class CustomEnginePart : ScriptableObject
    {
        public string Name;
        public int Torque;
        public bool NonLinearBoost;
        public AnimationCurve EffectCurve;
        public bool ChangeWorkingRPM;
        public int IdleRPMChange;
        public int MaxRPMChange;

        public void SetDefaultData(string name)
        { 
            Name = name;
            Torque = 100;
            NonLinearBoost = false;

            Keyframe[] keyframes = { new Keyframe(0, 0), new Keyframe(1, 1) };
            EffectCurve = new AnimationCurve(keyframes);

            ChangeWorkingRPM = false;

            IdleRPMChange = 0;
            MaxRPMChange = 0;
        }
    }
}
