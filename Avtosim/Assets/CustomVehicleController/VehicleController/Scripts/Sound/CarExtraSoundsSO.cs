using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "ExtraSoundsSO", menuName = "CustomVehicleController/Sound/ExtraSoundsSO")]
    public class CarExtraSoundsSO : ScriptableObject
    {
        public AudioClip TireSlipSound;
        public AudioClip WindNoise;
        public AudioClip NitroStart;
        public AudioClip NitroContinuous;
        public AudioClip MetalScraping;
        public AudioClip[] CrashImpact;
    }
}

