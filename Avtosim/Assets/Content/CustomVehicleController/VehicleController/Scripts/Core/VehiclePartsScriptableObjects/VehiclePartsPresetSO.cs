using UnityEngine;

namespace Assets.VehicleController
{
    [CreateAssetMenu(fileName = "VehiclePartsPresetSO", menuName = "CustomVehicleController/VehicleParts/VehiclePartsPreset")]
    public class VehiclePartsPresetSO : ScriptableObject
    {
        public EngineSO Engine;
        public ForcedInductionSO ForcedInduction;
        public NitrousSO Nitrous;
        public TransmissionSO Transmission;
        public TiresSO FrontTires;
        public TiresSO RearTires;
        public SuspensionSO FrontSuspension;
        public SuspensionSO RearSuspension;
        public BrakesSO Brakes;
        public VehicleBodySO Body;

        public static VehiclePartsPresetSO CreateDefaultVehiclePartsPresetSO(string name = "DefaultPreset")
        {
            VehiclePartsPresetSO preset = ScriptableObject.CreateInstance<VehiclePartsPresetSO>();

            preset.Engine = EngineSO.CreateDefaultEngineSO();
            preset.ForcedInduction = ForcedInductionSO.CreateDefaultForcedInductionSO();
            preset.Nitrous = NitrousSO.CreateDefaultNitroSO();
            preset.Transmission = TransmissionSO.CreateDefaultTransmissionSO();
            preset.FrontTires = TiresSO.CreateDefaultTiresSO();
            preset.RearTires = TiresSO.CreateDefaultTiresSO();
            preset.FrontSuspension = SuspensionSO.CreateDefaultSuspensionSO();
            preset.RearSuspension = SuspensionSO.CreateDefaultSuspensionSO();
            preset.Brakes = BrakesSO.CreateDefaultBrakesSO();
            preset.Body = VehicleBodySO.CreateDefaultBodySO();
            preset.name = name;
            return preset;
        }
    }
}
