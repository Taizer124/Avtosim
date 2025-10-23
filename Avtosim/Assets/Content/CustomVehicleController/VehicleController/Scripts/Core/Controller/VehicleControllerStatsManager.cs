using UnityEngine;

namespace Assets.VehicleController
{
    public class VehicleControllerStatsManager
    {
        private float _lastDriftTime;
        private float _lastSpeed;

        private Rigidbody _rb;
        private Transform _transform;
        private CurrentCarStats _currentCarStats;
        private VehicleAxle[] _axleArray;
        private WheelController[] _wheelControllerArray;
        private VehicleAxle[] _frontAxleArray;
        private VehicleAxle[] _rearAxleArray;

        private VehicleAxle[] _driveAxleArray;

        private ITransmission _transmission;
        private IShifter _shifter;
        private IEngine _engine;

        private VehiclePartsSetWrapper _partsPresetWrapper;
        private int _axleAmount;
        private int _wheelAmount;

        private const string REVERSE_GEAR_NAME = "R";
        private const string NEUTRAL_GEAR_NAME = "N";

        private string[] _gearsArray;

        public VehicleControllerStatsManager(VehicleAxle[] axleArray, VehicleAxle[] frontAxleArray, VehicleAxle[] rearAxleArray,
            CurrentCarStats currentCarStats, Rigidbody rb, Transform transform, IEngine engine, ITransmission transmission,
            IShifter shifter, VehiclePartsSetWrapper partsPresetWrapper)
        {
            _currentCarStats = currentCarStats;
            _axleArray = axleArray;
            _frontAxleArray = frontAxleArray;
            _rearAxleArray = rearAxleArray;
            _rb = rb;
            this._transform = transform;

            _wheelControllerArray = VehicleAxle.ExtractVehicleWheelControllerArray(_axleArray);

            _engine = engine;
            _transmission = transmission;
            _shifter = shifter;

            _axleAmount = axleArray.Length;
            _wheelAmount = _axleAmount * 2;
            _currentCarStats.WheelSlipArray = new bool[_axleAmount * 2];

            _partsPresetWrapper = partsPresetWrapper;

            CreateGearCharArray();
        }

        private void CreateGearCharArray()
        {
            _gearsArray = new string[_partsPresetWrapper.Transmission.GearRatiosList.Count];

            for (int i = 1; i <= _gearsArray.Length; i++)
                _gearsArray[i - 1] = i.ToString();
        }

        public void ManageStats(float gasInput, float brakeInput, bool handbrakeInput, float sideSlipThreshold, float fwdSlipThreshold, DrivetrainType drivetrainType)
        {
            UpdateDriveWheels(drivetrainType);
            Vector3 forward = _transform.forward;
            float speedMS = Vector3.Dot(_rb.linearVelocity, forward);

            _currentCarStats.SpeedInMsPerS = speedMS;
            _currentCarStats.SpeedPercent = Mathf.Clamp01(Mathf.Abs(_currentCarStats.SpeedInKMperH) / _partsPresetWrapper.Engine.MaxSpeed);

            float minRPM = _transmission.GetModifiedMinRPM();
            float maxRPM = _transmission.GetModifiedMaxRPM();

            _currentCarStats.MinRPM = minRPM;
            _currentCarStats.EngineRPM = _transmission.EvaluateRPM(gasInput, _driveAxleArray);

            _currentCarStats.EngineMaxRPMChangeMultiplier = maxRPM / _transmission.GetMaxRPM();

            _currentCarStats.EngineRPMPercent = (_currentCarStats.EngineRPM - minRPM) / (maxRPM - minRPM);

            _currentCarStats.CurrentEngineTorque = _engine.GetCurrentTorque();
            _currentCarStats.ForcedInductionBoostPercent = _engine.GetForcedInductionBoostPercent();
            _currentCarStats.ForcedInductionBoostPressureMax = _engine.GetForcedInductionBoostPressureMax();
            _currentCarStats.ForcedInductionBoostPressureCurrent = _currentCarStats.ForcedInductionBoostPercent * _currentCarStats.ForcedInductionBoostPressureMax;

            _currentCarStats.Accelerating = _transmission.DetermineGasInput(gasInput, brakeInput) != 0;
            _currentCarStats.Braking = _transmission.DetermineBrakeInput(gasInput, brakeInput) != 0;
            _currentCarStats.BrakingIntensity = _currentCarStats.Braking? brakeInput * Mathf.Abs(_currentCarStats.AccelerationForce / 20): 0;
            _currentCarStats.HandbrakePulled = handbrakeInput;

            _currentCarStats.AccelerationForce = (_currentCarStats.SpeedInMsPerS - _lastSpeed) / Time.deltaTime;
            _lastSpeed = speedMS;

            _currentCarStats.SidewaysForce = _rb.linearVelocity.x * _transform.right.x + _rb.linearVelocity.z * _transform.right.z;

            _currentCarStats.Reversing = _transmission.DetermineGasInput(gasInput, brakeInput) < 0 && speedMS <= 1;
            _currentCarStats.TCSworking = _driveAxleArray[0].LeftHalfShaft.WheelController.TCSWorked;
            CalculateDriftAngle(forward);
            CalculateDriftTime(sideSlipThreshold);
            IsCarInAir();
            HaveDriveWheelNoGroundContact();
            HasCarLostTraction(sideSlipThreshold, fwdSlipThreshold);
            UpdateCurrentGear();
        }

        private void UpdateDriveWheels(DrivetrainType drivetrainType)
        {
            switch (drivetrainType)
            {
                case DrivetrainType.FWD:
                    _driveAxleArray = _frontAxleArray;
                    break;
                case DrivetrainType.RWD:
                    _driveAxleArray = _rearAxleArray;
                    break;
                default:
                    _driveAxleArray = _axleArray;
                    break;
            }
            
        }

        private void CalculateDriftAngle(Vector3 fwd)
        {
            if (_currentCarStats.SpeedInMsPerS > 0.1f)
                _currentCarStats.DriftAngle = Vector3.Angle(fwd, _rb.linearVelocity);
            else
                _currentCarStats.DriftAngle = 0;
        }

        private void CalculateDriftTime(float sideSlipThreshold)
        {
            if (Mathf.Abs(Vector3.Dot(_rb.linearVelocity.normalized, _transform.right)) > sideSlipThreshold)
                _lastDriftTime = Time.time;

            _currentCarStats.DriftTime = Time.time < _lastDriftTime + 1 ? _currentCarStats.DriftTime + Time.deltaTime : 0;
        }

        private void UpdateCurrentGear()
        {
            if (_shifter.InReverseGear())
            {
                _currentCarStats.CurrentGear = REVERSE_GEAR_NAME;
                return;
            }

            if (_shifter.InNeutralGear())
            {
                _currentCarStats.CurrentGear = NEUTRAL_GEAR_NAME;
                return;
            }

            int gearID = _shifter.GetCurrentGearID();

            if (gearID >= _gearsArray.Length)
                CreateGearCharArray();

            _currentCarStats.CurrentGear = _gearsArray[gearID];
        }

        private void HasCarLostTraction(float sideSlipThres, float fwdSlipThres)
        {
            bool slip = false;

            for (int i = 0; i < _wheelAmount; i++)
            {
                if (_wheelControllerArray[i].LockedUp)
                {
                    _currentCarStats.WheelSlipArray[i] = true;
                    slip = true;
                }
                else
                {
                    if (!_wheelControllerArray[i].IsExceedingSlipThreshold(fwdSlipThres, sideSlipThres, _currentCarStats.Accelerating))
                        _currentCarStats.WheelSlipArray[i] = false;
                    else
                    {
                        slip = true;
                        _currentCarStats.WheelSlipArray[i] = true;
                    }
                }
            }
            _currentCarStats.IsCarSlipping = slip;
        }

        private void IsCarInAir()
        {
            int wheelsInAir = 0;
            for (int i = 0; i < _wheelAmount; i++)
            {
                if (!_wheelControllerArray[i].HasContactWithGround)
                    wheelsInAir++;
            }
            _currentCarStats.InAir = wheelsInAir == _wheelAmount;
            _currentCarStats.AirTime = _currentCarStats.InAir ? _currentCarStats.AirTime + Time.deltaTime : 0;
            _currentCarStats.AllWheelsGrounded = wheelsInAir == 0;
        }

        private void HaveDriveWheelNoGroundContact()
        {
            int wheelsInAir = 0;
            int size = _driveAxleArray.Length;
            for (int i = 0; i < size; i++)
            {
                if (!_driveAxleArray[i].LeftHalfShaft.WheelController.HasContactWithGround)
                    wheelsInAir++;

                if (!_driveAxleArray[i].RightHalfShaft.WheelController.HasContactWithGround)
                    wheelsInAir++;
            }

            _currentCarStats.DriveWheelsGrounded = wheelsInAir < size * 2;
        }

        //public void OnDestroy()
        //{
        //    if (_engineSO != null)
        //        _engineSO.OnEngineStatsChanged -= OnStatsChanged;

        //    if (_partsPresetWrapper == null)
        //        return;

        //    _partsPresetWrapper.OnPartsChanged -= _stats_OnPresetChanged;
        //}
    }
}