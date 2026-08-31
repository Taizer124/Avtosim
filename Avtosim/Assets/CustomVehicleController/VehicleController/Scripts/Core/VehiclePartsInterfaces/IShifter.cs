namespace Assets.VehicleController
{
    public interface IShifter
    {
        public void Initialize(VehiclePartsSetWrapper partsPresetWrapper);
        public bool TryChangeGear(int i, float delay);
        public void SetGear(int gearId);
        public int GetRawGearId();
        public bool InNeutralGear();
        public bool InReverseGear();
        // Идёт ли переключение прямо сейчас: TryChangeGear уже увёл шифтер в
        // нейтраль, а целевая передача воткнётся корутиной позже. Нужен, чтобы
        // автомат не принял эту техническую нейтраль за настоящую.
        public bool IsShifting();
        public bool CheckIsClutchEngaged();
        public void SetInNeutral();
        public int GetCurrentGearID();
        public int GetGearAmount();
    }
}
