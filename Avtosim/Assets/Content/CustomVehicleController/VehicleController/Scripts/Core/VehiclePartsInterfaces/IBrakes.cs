using UnityEngine;

namespace Assets.VehicleController
{
    public interface IBrakes
    {
        public void Initialize(VehiclePartsSetWrapper partsPresetWrapper, VehicleAxle[] axleArray,
            VehicleAxle[] rearAxles, CurrentCarStats currentCarStats, Rigidbody rb, ITransmission transmission);
        public void Break(float gasInput, float breakInput, bool handbrakePulled, bool ABS);
    }
}
