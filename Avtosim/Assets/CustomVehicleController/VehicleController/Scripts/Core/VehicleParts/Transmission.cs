using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class Transmission : ITransmission
    {
        public event Action OnShifted;

        private TransmissionType _transmissionType;

        private CurrentCarStats _currentCarStats;
        private VehiclePartsSetWrapper _partsPresetWrapper;
        private IShifter _shifter;

        private float _downShiftRPM;
        private float _upShiftRPM;

        private float _lastShiftTime;

        private bool _redlining = false;

        private bool _inCooldown;

        private float _engineMinRPM;
        private float _engineMaxRPM;

        private float _modifiedEngineMinRPM;
        private float _modifiedEngineMaxRPM;

        private float _currentEngineRPM = 0;
        private float smDampVelocity;
        private const float SM_DAMP_SPEED = 0.15f;

        private EngineSO _engineSO;
        private TransmissionSO _transmissionSO;

        private List<CustomEnginePart> _customEngineParts;

        public void Initialize(VehiclePartsSetWrapper partsPresetWrapper, CurrentCarStats currentCarStats, List<CustomEnginePart> customEngineParts, IShifter shifter)
        {
            _currentCarStats = currentCarStats;
            _partsPresetWrapper = partsPresetWrapper;
            _shifter = shifter;

            _customEngineParts = customEngineParts;

            _lastShiftTime = Time.time;

            _engineMinRPM = _modifiedEngineMinRPM = _partsPresetWrapper.Engine.MinRPM;
            _engineMaxRPM = _modifiedEngineMaxRPM = _partsPresetWrapper.Engine.MaxRPM;

            UpdateShiftRPMs();

            //this adds support to field changes during runtime
            _transmissionSO = _partsPresetWrapper.Transmission;
            _engineSO = _partsPresetWrapper.Engine;
            _transmissionSO.OnTransmissionStatsChanged += OnStatsChanged;
            _engineSO.OnEngineStatsChanged += OnStatsChanged;
            _partsPresetWrapper.OnPartsChanged += _stats_OnPresetChanged;
        }

        private void UpdateShiftRPMs()
        {
            _upShiftRPM = _modifiedEngineMaxRPM * _partsPresetWrapper.Transmission.UpShiftRPMPercent;
            _downShiftRPM = _modifiedEngineMaxRPM * _partsPresetWrapper.Transmission.DownShiftRPMPercent;
        }

        public float GetMinRPM() => _engineMinRPM;
        public float GetMaxRPM() => _engineMaxRPM;

        public float GetModifiedMinRPM() => _modifiedEngineMinRPM;
        public float GetModifiedMaxRPM() => _modifiedEngineMaxRPM;

        private void OnStatsChanged()
        {
            UpdateRPMValues();
        }

        private void UpdateRPMValues()
        {
            _engineMinRPM = _engineSO.MinRPM;
            _engineMaxRPM = _engineSO.MaxRPM;
            UpdateShiftRPMs();
        }

        private void _stats_OnPresetChanged()
        {
            _transmissionSO.OnTransmissionStatsChanged -= OnStatsChanged;
            _engineSO.OnEngineStatsChanged -= OnStatsChanged;

            _transmissionSO = _partsPresetWrapper.Transmission;
            _engineSO = _partsPresetWrapper.Engine;

            _transmissionSO.OnTransmissionStatsChanged += OnStatsChanged;
            _engineSO.OnEngineStatsChanged += OnStatsChanged;
            UpdateRPMValues();
        }

        public void HandleGearChanges(TransmissionType transmissionType, VehicleAxle[] axleArray)
        {
            if (_inCooldown)
                return;

            _transmissionType = transmissionType;

            float wheelRPM = 0;

            int size = axleArray.Length;
            for (int i = 0; i < size; i++)
            {
                wheelRPM += axleArray[i].LeftHalfShaft.WheelController.WheelRPM;
                wheelRPM += axleArray[i].RightHalfShaft.WheelController.WheelRPM;
            }


            wheelRPM /= size * 2;

            float temp = Mathf.Abs(wheelRPM) * _partsPresetWrapper.Transmission.FinalDriveRatio * 60 / 6.28f;

            float RPMfromSpeed = CalculateRealRPM(temp);
            (float imaginaryRPM, int gearDown) = CalculateImaginaryRPMAndGearSkip(temp);

            _redlining = RPMfromSpeed == _modifiedEngineMaxRPM;

            if (transmissionType != TransmissionType.Automatic)
                return;

            if (_currentCarStats.InAir)
                return;

            SwitchGearsAutomatically(RPMfromSpeed, imaginaryRPM, gearDown);
        }

        private void UpdateEngineRPMRange()
        {
            float idleRPMChange = 0;
            float maxRPMChange = 0;

            int size = _customEngineParts.Count;

            for(int i = 0; i < size; i++)
            {
                if (_customEngineParts[i] == null)
                    continue;

                if (!_customEngineParts[i].ChangeWorkingRPM)
                    continue;

                idleRPMChange += _customEngineParts[i].IdleRPMChange;
                maxRPMChange += _customEngineParts[i].MaxRPMChange;
            }

            _modifiedEngineMaxRPM = _engineMaxRPM + maxRPMChange;
            _modifiedEngineMinRPM = _engineMinRPM + idleRPMChange;

            UpdateShiftRPMs();
        }

        public float EvaluateRPM(float gasInput, VehicleAxle[] driveAxleArray)
        {
            UpdateEngineRPMRange();

            bool isDisconnected = _shifter.InNeutralGear();
            float clutch = 0f;
            CustomVehicleController owner = _partsPresetWrapper.Owner;
            if (owner != null)
            {
                clutch = owner.GetClutchInput();
            }
            if (clutch > 0.1f)
            {
                isDisconnected = true;
            }

            if (isDisconnected)
            {
                float targetRPM = _modifiedEngineMinRPM;
                if (gasInput > 0.05f)
                {
                    targetRPM = Mathf.Lerp(_currentEngineRPM, _modifiedEngineMaxRPM, gasInput * Time.deltaTime * 6.0f);
                }
                else
                {
                    targetRPM = Mathf.Lerp(_currentEngineRPM, _modifiedEngineMinRPM, Time.deltaTime * 3.0f);
                }
                _currentEngineRPM = targetRPM;
            }
            else
            {
                float highestRPM = 0;
                int size = driveAxleArray.Length;

                for (int i = 0; i < size; i++)
                {
                    if (Mathf.Abs(driveAxleArray[i].LeftHalfShaft.WheelController.VisualRPM) > highestRPM)
                        highestRPM = Mathf.Abs(driveAxleArray[i].LeftHalfShaft.WheelController.VisualRPM);

                    if (Mathf.Abs(driveAxleArray[i].RightHalfShaft.WheelController.VisualRPM) > highestRPM)
                        highestRPM = Mathf.Abs(driveAxleArray[i].RightHalfShaft.WheelController.VisualRPM);
                }

                float imaginaryEngineRPM = CalculateRealRPM(Mathf.Abs(highestRPM) * _partsPresetWrapper.Transmission.FinalDriveRatio * 60 / 6.28f);

                _currentEngineRPM = Mathf.SmoothDamp(_currentEngineRPM, imaginaryEngineRPM, ref smDampVelocity, SM_DAMP_SPEED);
            }

            _inCooldown = Time.time < _lastShiftTime + _partsPresetWrapper.Transmission.ShiftCooldown;
            PerformRedliningEffect(gasInput);

            return _currentEngineRPM;
        }

        private float CalculateRealRPM(float temp)
        {
            float nextRPM = temp * _partsPresetWrapper.Transmission.GearRatiosList[_shifter.GetCurrentGearID()];
            return Mathf.Clamp(nextRPM, _modifiedEngineMinRPM, _modifiedEngineMaxRPM);
        }

        private (float, int) CalculateImaginaryRPMAndGearSkip(float temp)
        {
            int currentGear = _shifter.GetCurrentGearID();

            int gearSkip = -1;

            int bestGear = Mathf.Clamp(currentGear - 1, 0, _shifter.GetGearAmount());
            float bestRPM = Mathf.Clamp(temp * _partsPresetWrapper.Transmission.GearRatiosList[bestGear],
                _modifiedEngineMinRPM, _modifiedEngineMaxRPM);

            //find the gear that will give us the highest possible RPM when downshifting.
            //for example, in case of high speed crash, the best one would be the first gear, without the need to downshift multiple times
            for (int i = bestGear - 1; i >= 0; i--)
            {
                float imaginaryRPM = temp * _partsPresetWrapper.Transmission.GearRatiosList[i];

                if (imaginaryRPM > _modifiedEngineMaxRPM)
                    break;

                if (imaginaryRPM < _downShiftRPM)
                {
                    bestRPM = imaginaryRPM;
                    gearSkip = i - currentGear;
                }
            }
            return (bestRPM, gearSkip);
        }

        private void SwitchGearsAutomatically(float rpmFromSpeed, float imaginaryRPM, int gearDown)
        {
            if (_inCooldown)
                return;

            if (_currentCarStats.Reversing && _currentCarStats.Accelerating)
            {
                ShiftGear(-1);
                return;
            }

            TryUpShift(rpmFromSpeed);
            TryDownShift(imaginaryRPM, gearDown);
        }

        private void PerformRedliningEffect(float gasInput)
        {
            if (_currentEngineRPM < _modifiedEngineMaxRPM * 0.99f)
                return;

            _currentEngineRPM -= (_currentEngineRPM * UnityEngine.Random.Range(0.02f, 0.11f)) * gasInput;
        }

        private void TryUpShift(float currentRPM)
        {
            if (!_currentCarStats.Accelerating)
                return;

            if (_shifter.InReverseGear())
                ShiftGear(+1);

            if (currentRPM > _upShiftRPM)
                ShiftGear(+1);
        }

        private void TryDownShift(float imaginaryRPM, int gearDown)
        {
            if (imaginaryRPM < _downShiftRPM && _shifter.GetCurrentGearID() > 0)
                ShiftGear(gearDown);
        }

        public void ShiftUpManually() => ShiftGear(+1);

        public void ShiftDownManually() => ShiftGear(-1);

        public void ShiftGear(int i)
        {
            if (_inCooldown)
                return;

            if (!_shifter.TryChangeGear(i, _partsPresetWrapper.Transmission.ShiftCooldown))
                return;

            _lastShiftTime = Time.time;
            OnShifted?.Invoke();
        }

        // Прямая установка передачи для механики (H-паттерн). В отличие от
        // ShiftGear(+1/-1) — без проверки _inCooldown и без задержки: водитель
        // сам решает, когда сцепление/рычаг позволяют переключиться, это уже
        // отражено в TrySelectGear (AllInOneInputProvider) через сцепление.
        // Кулдаун здесь имитирует время механического переключения
        // секвентальной/автоматической коробки — к H-паттерну неприменим.
        //
        // CustomVehicleController вызывает SetGear() КАЖДЫЙ кадр, пока активна
        // механика (не только при реальной смене передачи) — поэтому _lastShiftTime
        // обновляем и OnShifted шлём ТОЛЬКО когда передача действительно изменилась.
        // Иначе _inCooldown держится постоянно true (ShiftCooldown "не успевает"
        // истечь между кадрами), а Engine.CalculateAccelerationForce() при активном
        // _inCooldown всегда возвращает нулевой крутящий момент — газ перестаёт
        // работать полностью, хотя передача формально включена.
        public void SetGear(int gearId)
        {
            bool changed = gearId != _shifter.GetRawGearId();
            _shifter.SetGear(gearId);

            if (changed)
            {
                _lastShiftTime = Time.time;
                OnShifted?.Invoke();
            }
        }

        public bool InShiftingCooldown() => _inCooldown;

        public bool Redlining() => _redlining;

        public float DetermineGasInput(float gasInput, float brakeInput)
        {
            if (_transmissionType == TransmissionType.Automatic)
                if (_currentCarStats.SpeedInMsPerS > 1)
                    return gasInput;
                else
                {
                    return brakeInput > gasInput ? -brakeInput : gasInput;
                }

            return _shifter.InReverseGear() ? -gasInput : gasInput;
        }

        public float DetermineBrakeInput(float gasInput, float brakeInput)
        {
            if (_transmissionType == TransmissionType.Automatic)
                return _currentCarStats.Reversing ? gasInput : brakeInput;

            return brakeInput;
        }
    }
}
