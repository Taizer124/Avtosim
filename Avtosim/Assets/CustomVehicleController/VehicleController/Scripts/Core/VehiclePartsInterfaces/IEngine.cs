using System.Collections.Generic;

namespace Assets.VehicleController
{
    public interface IEngine
    {
        public void Initialize(CurrentCarStats currentCarStats, VehiclePartsSetWrapper partsPresetWrapper, List<CustomEnginePart> customEngineParts, IShifter shifter, ITransmission transmission);
        public void Accelerate(VehicleAxle[] driveAxleArray, float gasInput, float breakInput, bool nitroBoostInput, float rpm);
        public float GetCurrentTorque();
        public float GetForcedInductionBoostPercent();
        public float GetForcedInductionBoostPressureMax();
        public void AddNitro(float amount);
    }
}
