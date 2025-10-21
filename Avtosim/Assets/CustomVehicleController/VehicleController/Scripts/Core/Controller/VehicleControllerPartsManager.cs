using UnityEngine;

namespace Assets.VehicleController
{
    public class VehicleControllerPartsManager
    {
        private IBody _body;
        private IEngine _engine;
        private ITransmission _transmission;
        private IBrakes _breaks;
        private IHandling _handling;

        private CurrentCarStats _currentCarStats;

        private CustomEnginePart[] _customEngineParts;

        private VehicleAxle[] _axleArray;
        private VehicleAxle[] _frontAxleArray;
        private VehicleAxle[] _rearAxleArray;

        private VehicleAxle[] _driveAxleArray;

        private Transform _centerOfGeometry;
        private Transform _transform;

        public VehicleControllerPartsManager(IBody body, IEngine engine, ITransmission transmission, IBrakes brakes,
            IHandling handling, CurrentCarStats currentCarStats, Transform transform,
            VehicleAxle[] axleArray, VehicleAxle[] frontAxleArray
            , VehicleAxle[] rearAxleArray, Transform centerOfGeometry)
        {
            _body = body;
            _engine = engine;
            _transmission = transmission;
            _breaks = brakes;
            _handling = handling;
            _currentCarStats = currentCarStats;
            _transform = transform;
            _axleArray = axleArray;
            _frontAxleArray = frontAxleArray;
            _rearAxleArray = rearAxleArray;
            _centerOfGeometry = centerOfGeometry;
        }

        public void ManageCarParts(float gasInput, float breakInput, bool nitroBoostInput, float horizontalInput,
        bool handbrakeInput, float maxSteerAngle, float steerSpeed, float returnSpeed, TransmissionType transmissionType,
        DrivetrainType drivetrainType, int suspensionSimulationPrecision, LayerMask ignoreLayers, bool TCS, bool ABS, bool recover, float offset)
        {
            UpdateDriveWheels(drivetrainType);

            _body.AddDownforce();
            _body.AddCorneringForce();
            _body.AutoRecover(recover, offset);

            _engine.Accelerate(_driveAxleArray, gasInput, breakInput, nitroBoostInput, _currentCarStats.EngineRPM);

            _breaks.Break(gasInput, breakInput, handbrakeInput, ABS);
            _handling.SteerWheels(horizontalInput, maxSteerAngle, steerSpeed, returnSpeed);
            _transmission.HandleGearChanges(transmissionType, _driveAxleArray);
            ManageWheelsPhysics(suspensionSimulationPrecision, ignoreLayers, TCS);
        }

        public void AddNitro(float amount) => _engine.AddNitro(amount);

        private void UpdateDriveWheels(DrivetrainType drivetrainType)
        {
            switch (drivetrainType)
            {
                case DrivetrainType.RWD:
                    _driveAxleArray = _rearAxleArray;
                    break;
                case DrivetrainType.FWD:
                    _driveAxleArray = _frontAxleArray;
                    break;
                default:
                    _driveAxleArray = _axleArray;
                    break;
            }
        }

        public void ManageTransmissionUpShift(bool shiftUp)
        {
            if (shiftUp)
                _transmission.ShiftUpManually();
        }

        public void ManageTransmissionDownShift(bool shiftDown)
        {
            if (shiftDown)
                _transmission.ShiftDownManually();
        }

        private void ManageWheelsPhysics(int suspensionSimulationPrecision, LayerMask ignoreLayers, bool TCS)
        {
            float dist = GetDistanceToGroundFromCoG();
            int size = _axleArray.Length;
            for (int i = 0; i < size; i++)
            {
                _axleArray[i].HandleVehicleAxle(_currentCarStats.SpeedInMsPerS, _currentCarStats.SpeedPercent, _currentCarStats.AccelerationForce, dist, suspensionSimulationPrecision, ignoreLayers, TCS);
            }
        }
        private float GetDistanceToGroundFromCoG()
        {
            RaycastHit hit;
            if (Physics.Raycast(_centerOfGeometry.position, -_transform.up, out hit))
            {
                return hit.distance;
            }
            return 0;
        }

        public void PerformAirControls(bool enabled, float aerialControlsSensitivity,
            float pitchInput, float yawInput, float rollInput)
        {
            if (enabled)
            {
                _body.PerformAerialControls(aerialControlsSensitivity, pitchInput, yawInput, rollInput);
            }
        }
    }
}


