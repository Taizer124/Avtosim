using System.Collections;
using UnityEngine;

namespace Assets.VehicleController
{
    public class Shifter : IShifter
    {
        private int _currentGear = 0;

        // Последнее "сырое" значение gearId, переданное в SetGear (0 = нейтраль,
        // -1 = задний ход, 1..N = передача) — отдельно от _currentGear, который
        // является 0-based индексом в GearRatiosList и не различает нейтраль/
        // передачу 1 (оба дают индекс 0). Нужен только для сравнения "изменилась
        // ли передача" в Transmission.SetGear().
        private int _lastSetGearId = 0;

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
            // ShifterState.Drive — это C#-дефолт для неинициализированного enum
            // (Drive объявлен первым = значение 0). Без явной установки машина
            // считалась бы "уже на 1-й передаче" с самого создания, до того как
            // игрок вообще коснулся ввода.
            _shifterState = ShifterStates.ShifterState.Neutral;
        }

        public bool InNeutralGear() => _shifterState == ShifterStates.ShifterState.Neutral;

        public bool InReverseGear() => _shifterState == ShifterStates.ShifterState.Reverse;

        public void SetInNeutral() => _shifterState = ShifterStates.ShifterState.Neutral;

        // Мгновенная установка передачи для H-паттерн (механика): в отличие от
        // TryChangeGear (секвентальный +1/-1 с задержкой корутины, имитирующей
        // время переключения паддла), здесь передача включается сразу — как
        // физически происходит при вставке рычага в положение с зажатым
        // сцеплением. Никакой задержки/кулдауна.
        public void SetGear(int gearId)
        {
            _lastSetGearId = gearId;

            if (gearId == 0) // Нейтраль
            {
                _shifterState = ShifterStates.ShifterState.Neutral;
                _currentGear = 0;
            }
            else if (gearId == -1) // Задний ход
            {
                _shifterState = ShifterStates.ShifterState.Reverse;
                _currentGear = 0;
            }
            else // Drive (1-я передача и выше)
            {
                int maxGearIndex = _partsPresetWrapper.Transmission.GearRatiosList.Count - 1;
                _currentGear = Mathf.Clamp(gearId - 1, 0, maxGearIndex);
                _shifterState = ShifterStates.ShifterState.Drive;
            }
        }

        // "Сырое" значение gearId, которое в последний раз передали в SetGear
        // (0 = нейтраль, -1 = задний ход, 1..N = передача). Используется
        // Transmission.SetGear() для определения реальной смены передачи.
        public int GetRawGearId() => _lastSetGearId;

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
