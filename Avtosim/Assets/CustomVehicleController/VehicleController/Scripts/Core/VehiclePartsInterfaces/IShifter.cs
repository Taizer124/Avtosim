namespace Assets.VehicleController
{
    public interface IShifter
    {
        public void Initialize(VehiclePartsSetWrapper partsPresetWrapper);
        public bool TryChangeGear(int i, float delay);
        public bool InNeutralGear();
        public bool InReverseGear();
        public bool CheckIsClutchEngaged();
        public void SetInNeutral();
        public int GetCurrentGearID();
        public int GetGearAmount();
    }
}
