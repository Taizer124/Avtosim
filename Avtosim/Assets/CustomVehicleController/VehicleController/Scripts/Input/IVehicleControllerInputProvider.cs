namespace Assets.VehicleController
{
    public interface IVehicleControllerInputProvider
    {
        public void EnableInput(bool enable);
        public float GetGasInput();
        public float GetBrakeInput();

        public bool GetNitroBoostInput();

        public bool GetHandbrakeInput();

        public float GetHorizontalInput();

        public bool GetGearUpInput();

        public bool GetGearDownInput();

        public float GetPitchInput();

        public float GetYawInput();

        public float GetRollInput();
    }
}