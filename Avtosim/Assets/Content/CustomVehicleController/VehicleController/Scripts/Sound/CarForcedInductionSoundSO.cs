using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "ForcedInductionSoundSO", menuName = "CustomVehicleController/Sound/ForcedInductionSoundSO")]
    public class CarForcedInductionSoundSO : ScriptableObject
    {
        public AudioClip ForcedInductionSound;
        public AudioClip[] TurboFlutterSound;
        public AudioClip[] TurboFlutterMildSound;
        public AudioClip[] AntiLagSound;
        public AudioClip[] AntiLagMildSounds;
    }
}
