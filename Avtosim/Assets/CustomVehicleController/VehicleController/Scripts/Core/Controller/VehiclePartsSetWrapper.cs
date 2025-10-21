using System;

namespace Assets.VehicleController
{
    public class VehiclePartsSetWrapper
    {
        public static event Action OnAnyPresetChanged;
        public event Action OnPartsChanged;

        private CustomVehicleController _owner;
        public CustomVehicleController Owner
        {
            get => _owner;
        }

        private EngineSO _engine;
        public EngineSO Engine
        {
            private set
            {
                if (_engine == value)
                    return;

                _engine = value;
                OnAnyPresetChanged?.Invoke();
                OnPartsChanged?.Invoke();
            }
            get => _engine;
        }

        private ForcedInductionSO _forcedInduction;
        public ForcedInductionSO ForcedInduction
        {
            private set
            {
                if (_forcedInduction == value)
                    return;

                _forcedInduction = value;
                OnAnyPresetChanged?.Invoke();
                OnPartsChanged?.Invoke();
            }
            get => _forcedInduction;
        }

        private NitrousSO _nitrous;
        public NitrousSO Nitrous
        {
            private set
            {
                if (_nitrous == value)
                    return;

                _nitrous = value;
                OnAnyPresetChanged?.Invoke();
                OnPartsChanged?.Invoke();
            }
            get => _nitrous;
        }

        private TransmissionSO _transmission;
        public TransmissionSO Transmission
        {
            private set
            {
                if (_transmission == value)
                    return;

                _transmission = value;
                OnAnyPresetChanged?.Invoke();
                OnPartsChanged?.Invoke();
            }
            get => _transmission;
        }

        private TiresSO _frontTires;
        public TiresSO FrontTires
        {
            private set
            {
                if (_frontTires == value)
                    return;

                _frontTires = value;
                OnAnyPresetChanged?.Invoke();
                OnPartsChanged?.Invoke();
            }
            get => _frontTires;
        }

        private TiresSO _rearTires;
        public TiresSO RearTires
        {
            private set
            {
                if (_rearTires == value)
                    return;

                _rearTires = value;
                OnAnyPresetChanged?.Invoke();
                OnPartsChanged?.Invoke();
            }
            get => _rearTires;
        }

        private SuspensionSO _frontSuspension;
        public SuspensionSO FrontSuspension
        {
            private set
            {
                if (_frontSuspension == value)
                    return;

                _frontSuspension = value;
                OnAnyPresetChanged?.Invoke();
                OnPartsChanged?.Invoke();
            }
            get => _frontSuspension;
        }

        private SuspensionSO _rearSuspension;
        public SuspensionSO RearSuspension
        {
            private set
            {
                if (_rearSuspension == value)
                    return;

                _rearSuspension = value;
                OnAnyPresetChanged?.Invoke();
                OnPartsChanged?.Invoke();
            }
            get => _rearSuspension;
        }

        private BrakesSO _brakes;
        public BrakesSO Brakes
        {
            private set
            {
                if (_brakes == value)
                    return;

                _brakes = value;
                OnAnyPresetChanged?.Invoke();
                OnPartsChanged?.Invoke();
            }
            get => _brakes;
        }

        private VehicleBodySO _body;
        public VehicleBodySO Body
        {
            private set
            {
                if (_body == value)
                    return;

                _body = value;
                OnAnyPresetChanged?.Invoke();
                OnPartsChanged?.Invoke();
            }
            get => _body;
        }


        public VehiclePartsSetWrapper(VehiclePartsPresetSO vehiclePartsPresetSO, CustomVehicleController owner)
        {
            _owner = owner;

            if (vehiclePartsPresetSO == null)
                return;
            Engine = vehiclePartsPresetSO.Engine;
            ForcedInduction = vehiclePartsPresetSO.ForcedInduction;
            Nitrous = vehiclePartsPresetSO.Nitrous;
            Transmission = vehiclePartsPresetSO.Transmission;
            FrontTires = vehiclePartsPresetSO.FrontTires;
            RearTires = vehiclePartsPresetSO.RearTires;
            FrontSuspension = vehiclePartsPresetSO.FrontSuspension;
            RearSuspension = vehiclePartsPresetSO.RearSuspension;
            Brakes = vehiclePartsPresetSO.Brakes;
            Body = vehiclePartsPresetSO.Body;
        }
        public VehiclePartsSetWrapper(VehiclePartsCustomizableSet vehiclePartsSetSO, CustomVehicleController owner)
        {
            _owner = owner;

            Engine = vehiclePartsSetSO.Engine;
            ForcedInduction = vehiclePartsSetSO.ForcedInduction;
            Nitrous = vehiclePartsSetSO.Nitrous;
            Transmission = vehiclePartsSetSO.Transmission;
            FrontTires = vehiclePartsSetSO.FrontTires;
            RearTires = vehiclePartsSetSO.RearTires;
            FrontSuspension = vehiclePartsSetSO.FrontSuspension;
            RearSuspension = vehiclePartsSetSO.RearSuspension;
            Brakes = vehiclePartsSetSO.Brakes;
            Body = vehiclePartsSetSO.Body;
        }

        public void UpdateVehiclePartsPresetIfRequired(VehiclePartsPresetSO newVehiclePresetSO)
        {
            Engine = newVehiclePresetSO.Engine;
            ForcedInduction = newVehiclePresetSO.ForcedInduction;
            Nitrous = newVehiclePresetSO.Nitrous;
            Transmission = newVehiclePresetSO.Transmission;
            FrontTires = newVehiclePresetSO.FrontTires;
            RearTires = newVehiclePresetSO.RearTires;
            FrontSuspension = newVehiclePresetSO.FrontSuspension;
            RearSuspension = newVehiclePresetSO.RearSuspension;
            Brakes = newVehiclePresetSO.Brakes;
            Body = newVehiclePresetSO.Body;
        }

        public void UpdateVehiclePartsPresetIfRequired(VehiclePartsCustomizableSet newVehiclePresetSO)
        {
            Engine = newVehiclePresetSO.Engine;
            ForcedInduction = newVehiclePresetSO.ForcedInduction;
            Nitrous = newVehiclePresetSO.Nitrous;
            Transmission = newVehiclePresetSO.Transmission;
            FrontTires = newVehiclePresetSO.FrontTires;
            RearTires = newVehiclePresetSO.RearTires;
            FrontSuspension = newVehiclePresetSO.FrontSuspension;
            RearSuspension = newVehiclePresetSO.RearSuspension;
            Brakes = newVehiclePresetSO.Brakes;
            Body = newVehiclePresetSO.Body;
        }
    }
}
