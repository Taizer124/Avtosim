using System.Collections;
using UnityEngine;

namespace Assets.VehicleController
{
    public class Shifter : IShifter
    {
        private int _currentGear = 0;

        private VehiclePartsSetWrapper _partsPresetWrapper;

        private ShifterStates.ShifterState _shifterState;

        public bool CheckIsClutchEngaged() => false;

        public int GetCurrentGearID()
        {
            if (_currentGear < _partsPresetWrapper.Transmission.GearRatiosList.Count)
                return _currentGear;

            return _partsPresetWrapper.Transmission.GearRatiosList.Count - 1;
        }


        public int GetGearAmount() => _partsPresetWrapper.Transmission.GearRatiosList.Count;

        public void Initialize(VehiclePartsSetWrapper partsPresetWrapper)
        {
            _partsPresetWrapper = partsPresetWrapper;
        }

        public bool InNeutralGear() => _shifterState == ShifterStates.ShifterState.Neutral;

        public bool InReverseGear() => _shifterState == ShifterStates.ShifterState.Reverse;

        public void SetInNeutral() => _shifterState = ShifterStates.ShifterState.Neutral;

        public bool TryChangeGear(int i, float delay)
        {
            (bool success, int nextGearID, ShifterStates.ShifterState nextShifterState) = WrapGear(_currentGear + i);
            if (success)
            {
                SetInNeutral();
                _partsPresetWrapper.Owner.StartCoroutine(DelayGearSwitch(delay, nextGearID, nextShifterState));
            }
            return success;
        }

        private IEnumerator DelayGearSwitch(float delay, int nextID, ShifterStates.ShifterState nextShifterState)
        {
            yield return new WaitForSeconds(delay);

            _currentGear = nextID;
            _shifterState = nextShifterState;
        }


        private (bool, int, ShifterStates.ShifterState) WrapGear(int newGearID)
        {
            //trying to downshift from the lowest gear
            if (newGearID < 0)
            {
                //already in reverse
                if (_shifterState == ShifterStates.ShifterState.Reverse)
                    return (false, 0, _shifterState);

                //get into reverse
                return (true, 0, ShifterStates.ShifterState.Reverse);
            }

            //going from reverse into first gear (id 0)
            if (_shifterState == ShifterStates.ShifterState.Reverse)
                return (true, 0, ShifterStates.ShifterState.Drive);

            if (newGearID >= _partsPresetWrapper.Transmission.GearRatiosList.Count)
                return (false, _partsPresetWrapper.Transmission.GearRatiosList.Count - 1, ShifterStates.ShifterState.Drive);

            return (true, newGearID, ShifterStates.ShifterState.Drive);
        }
    }
}
