namespace Assets.VehicleController
{

    public enum DrivetrainType
    {
        RWD = 0,
        FWD = 1,
        AWD = 2
    }

    public enum TransmissionType
    {
        Automatic = 0,
        Manual = 1
    }

    public enum ForcedInductionType
    {
        None = 0,
        Turbocharger = 1,
        Supercharger = 2,
        Centrifugal = 3
    }

    public enum NitroBoostType
    {
        Continuous,
        OneShot,
    }
}