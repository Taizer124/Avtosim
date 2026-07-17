namespace Assets.VehicleController
{
    /// <summary>
    /// Мостик pre-race меню → заспавненная гоночная машина. Меню записывает сюда
    /// выбранные коробку и привод, а RaceCarSetupApplier на префабе гоночной
    /// машины читает их в своём Start (после спавна) и применяет к своему
    /// CustomVehicleController + AllInOneInputProvider.
    ///
    /// Статик, потому что меню и заспавненная машина — разные объекты без прямой
    /// ссылки, а значение живёт в пределах одного заезда (после финиша чистится).
    /// HasValue=false означает «сетап не выбирали» (гонка без pre-race меню) —
    /// тогда апплаер оставляет дефолты префаба.
    /// </summary>
    public static class RaceSetupSelection
    {
        public static bool HasValue { get; private set; }
        public static TransmissionType Transmission { get; private set; }
        public static DrivetrainType Drivetrain { get; private set; }

        public static void Set(TransmissionType transmission, DrivetrainType drivetrain)
        {
            Transmission = transmission;
            Drivetrain = drivetrain;
            HasValue = true;
        }

        public static void Clear() => HasValue = false;
    }
}
