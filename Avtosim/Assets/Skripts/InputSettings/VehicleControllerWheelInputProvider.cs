using UnityEngine;
using LogitechG29.Sample.Input;

namespace Assets.VehicleController
{
    [AddComponentMenu("CustomVehicleController/Input/Vehicle Controller Wheel Input Provider")]
    public class VehicleControllerWheelInputProvider : MonoBehaviour, IVehicleControllerInputProvider
    {
        [Header("Wheel Input Settings")]
        [SerializeField] private InputControllerReader _wheelInput;

        [Header("Input Mapping")]
        [SerializeField] private bool _useSequentialShifting = false;

        // Для хранения состояний ввода
        private float _gasInput;
        private float _brakeInput;
        private float _steeringInput;
        private bool _handbrakeInput;
        private bool _gearUpInput;
        private bool _gearDownInput;
        private bool _nitroInput;

        // Для механической коробки передач
        private bool[] _gearInputs = new bool[8]; // 0-N, 1-7 передачи
        private int _currentGear = 0; // 0 - нейтраль, 1-7 - передачи
        private bool _clutchInput = false;

        private bool _enabled = true;

        private void OnEnable()
        {
            if (_wheelInput == null)
            {
                Debug.LogError("Wheel Input not assigned!");
                return;
            }

            // ПРАВИЛЬНЫЕ названия событий (с "On" в начале)
            _wheelInput.ThrottleCallback += OnThrottle;
            _wheelInput.BrakeCallback += OnBrake;
            _wheelInput.SteeringCallback += OnSteering;
            _wheelInput.HandbrakeCallback += OnHandbrake;
            _wheelInput.OnRightShiftCallback += OnGearUp;        // С "On"
            _wheelInput.OnLeftShiftCallback += OnGearDown;       // С "On"  
            _wheelInput.OnEastButtonCallback += OnNitro;         // С "On"

            // Подписываемся на события механической коробки
            SubscribeToManualGearEvents();
        }

        private void OnDisable()
        {
            if (_wheelInput == null) return;

            // Отписываемся от событий
            _wheelInput.ThrottleCallback -= OnThrottle;
            _wheelInput.BrakeCallback -= OnBrake;
            _wheelInput.SteeringCallback -= OnSteering;
            _wheelInput.HandbrakeCallback -= OnHandbrake;
            _wheelInput.OnRightShiftCallback -= OnGearUp;        // С "On"
            _wheelInput.OnLeftShiftCallback -= OnGearDown;       // С "On"
            _wheelInput.OnEastButtonCallback -= OnNitro;         // С "On"

            // Отписываемся от событий механической коробки
            UnsubscribeFromManualGearEvents();
        }

        private void SubscribeToManualGearEvents()
        {
            // Подписываемся на события передач механической коробки
            _wheelInput.Shifter1Callback += OnShifter1;
            _wheelInput.Shifter2Callback += OnShifter2;
            _wheelInput.Shifter3Callback += OnShifter3;
            _wheelInput.Shifter4Callback += OnShifter4;
            _wheelInput.Shifter5Callback += OnShifter5;
            _wheelInput.Shifter6Callback += OnShifter6;
            _wheelInput.Shifter7Callback += OnShifter7;
        }

        private void UnsubscribeFromManualGearEvents()
        {
            // Отписываемся от событий передач механической коробки
            _wheelInput.Shifter1Callback -= OnShifter1;
            _wheelInput.Shifter2Callback -= OnShifter2;
            _wheelInput.Shifter3Callback -= OnShifter3;
            _wheelInput.Shifter4Callback -= OnShifter4;
            _wheelInput.Shifter5Callback -= OnShifter5;
            _wheelInput.Shifter6Callback -= OnShifter6;
            _wheelInput.Shifter7Callback -= OnShifter7;
        }

        private void Update()
        {
            if (!_enabled || _wheelInput == null)
            {
                ResetInputs();
                return;
            }

            // Обрабатываем переключения передач в зависимости от режима
            ProcessGearChanges();

            //// Для отладки - выводим значения каждую секунду
            //if (Time.frameCount % 60 == 0)
            //{
            //    Debug.Log($"Wheel Input - Gas: {_gasInput}, Brake: {_brakeInput}, Steering: {_steeringInput}, " +
            //             $"GearUp: {_gearUpInput}, GearDown: {_gearDownInput}, Nitro: {_nitroInput}, " +
            //             $"Current Gear: {_currentGear}");
            //}
        }

        private void ProcessGearChanges()
        {
            if (_useSequentialShifting)
            {
                // Секвентальное переключение - старая логика
                ProcessSequentialShifting();
            }
            else
            {
                // Механическая коробка - новая логика
                ProcessManualShifting();
            }
        }

        private void ProcessSequentialShifting()
        {
            // Существующая логика секвентального переключения
            if (_gearUpInput)
            {
                // Логика переключения вверх
                Debug.Log("Sequential Gear Up");
                _gearUpInput = false;
            }

            if (_gearDownInput)
            {
                // Логика переключения вниз
                Debug.Log("Sequential Gear Down");
                _gearDownInput = false;
            }
        }

        private void ProcessManualShifting()
        {
            // Проверяем, какая передача нажата в механической коробке
            int selectedGear = -1;

            for (int i = 0; i < _gearInputs.Length; i++)
            {
                if (_gearInputs[i])
                {
                    selectedGear = i;
                    break;
                }
            }

            // Если нажата какая-то передача и она отличается от текущей
            if (selectedGear != -1 && selectedGear != _currentGear)
            {
                // Эмулируем выжим сцепления для переключения
                if (!_clutchInput)
                {
                    Debug.Log($"Manual gear change: {_currentGear} -> {selectedGear}");

                    // Здесь можно добавить логику выжима сцепления
                    // _clutchInput = true;
                    // StartCoroutine(ClutchAndChangeGear(selectedGear));

                    // Прямое переключение (упрощенное)
                    _currentGear = selectedGear;
                }
            }

            // Если не нажата ни одна передача - устанавливаем нейтраль
            if (selectedGear == -1 && _currentGear != 0)
            {
                _currentGear = 0;
                Debug.Log("Gear set to Neutral");
            }
        }

        #region Wheel Input Handlers
        private void OnThrottle(float value)
        {
            _gasInput = value;
        }

        private void OnBrake(float value)
        {
            _brakeInput = value;
        }

        private void OnSteering(float value)
        {
            _steeringInput = value;
        }

        private void OnHandbrake(float value)
        {
            _handbrakeInput = value > 0.5f;
        }

        private void OnGearUp(bool pressed)
        {
            if (_useSequentialShifting)
            {
                _gearUpInput = pressed;
            }
        }

        private void OnGearDown(bool pressed)
        {
            if (_useSequentialShifting)
            {
                _gearDownInput = pressed;
            }
        }

        private void OnNitro(bool pressed)
        {
            _nitroInput = pressed;
        }
        #endregion

        #region Manual Gear Handlers
        private void OnShifter1(bool pressed)
        {
            _gearInputs[1] = pressed;
            Debug.Log($"Shifter 1: {pressed}");
        }

        private void OnShifter2(bool pressed)
        {
            _gearInputs[2] = pressed;
            Debug.Log($"Shifter 2: {pressed}");
        }

        private void OnShifter3(bool pressed)
        {
            _gearInputs[3] = pressed;
            Debug.Log($"Shifter 3: {pressed}");
        }

        private void OnShifter4(bool pressed)
        {
            _gearInputs[4] = pressed;
            Debug.Log($"Shifter 4: {pressed}");
        }

        private void OnShifter5(bool pressed)
        {
            _gearInputs[5] = pressed;
            Debug.Log($"Shifter 5: {pressed}");
        }

        private void OnShifter6(bool pressed)
        {
            _gearInputs[6] = pressed;
            Debug.Log($"Shifter 6: {pressed}");
        }

        private void OnShifter7(bool pressed)
        {
            _gearInputs[7] = pressed;
            Debug.Log($"Shifter 7: {pressed}");
        }
        #endregion

        private void ResetInputs()
        {
            _gasInput = 0f;
            _brakeInput = 0f;
            _steeringInput = 0f;
            _handbrakeInput = false;
            _gearUpInput = false;
            _gearDownInput = false;
            _nitroInput = false;

            // Сбрасываем состояния передач механической коробки
            for (int i = 0; i < _gearInputs.Length; i++)
            {
                _gearInputs[i] = false;
            }
            _currentGear = 0;
        }

        #region IVehicleControllerInputProvider Implementation
        public void EnableInput(bool enable)
        {
            _enabled = enable;
            if (!enable) ResetInputs();
        }

        public float GetGasInput() => _gasInput;
        public float GetBrakeInput() => _brakeInput;
        public bool GetNitroBoostInput() => _nitroInput;
        public bool GetHandbrakeInput() => _handbrakeInput;
        public float GetHorizontalInput() => _steeringInput;

        public bool GetGearUpInput()
        {
            if (_useSequentialShifting)
            {
                bool result = _gearUpInput;
                _gearUpInput = false; // Сбрасываем после чтения
                return result;
            }
            return false;
        }

        public bool GetGearDownInput()
        {
            if (_useSequentialShifting)
            {
                bool result = _gearDownInput;
                _gearDownInput = false; // Сбрасываем после чтения
                return result;
            }
            return false;
        }

        // Новый метод для получения текущей передачи (для механической коробки)
        public int GetCurrentGear()
        {
            return _currentGear;
        }

        // Новый метод для проверки, включена ли механическая коробка
        public bool IsManualTransmission()
        {
            return !_useSequentialShifting;
        }

        public float GetPitchInput() => 0f;
        public float GetYawInput() => 0f;
        public float GetRollInput() => 0f;
        #endregion
    }
}