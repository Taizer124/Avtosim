namespace Assets.VehicleController
{
    public interface IHandling
    {
        public void Initialize(VehicleAxle[] steerAxleArray);
        public void SteerWheels(float input, float steeringAngle, float steerSpeed, float returnSpeed);
    }
}
