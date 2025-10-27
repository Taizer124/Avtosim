using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.VehicleController;

public class SimpleSpeedDisplay : MonoBehaviour
{
    [Header("Vehicle Reference")]
    [SerializeField] private CustomVehicleController _vehicleController;

    [Header("Text Components")]
    [SerializeField] private Text _speedText;
    [SerializeField] private TextMeshProUGUI _speedTextTMP;
    [SerializeField] private TextMeshProUGUI _gearTextTMP;
    [SerializeField] private TextMeshProUGUI _rpmTextTMP;

    [Header("UI Sliders")]
    [SerializeField] private Slider _rpmSlider;

    [Header("Needle References")]
    [SerializeField] private Image _speedNeedle;
    [SerializeField] private Image _rpmNeedle;

    [Header("Needle Settings")]
    [SerializeField] private float _speedMinAngle = 45f;
    [SerializeField] private float _speedMaxAngle = -225f;
    [SerializeField] private float _rpmMinAngle = 45f;
    [SerializeField] private float _rpmMaxAngle = -225f;
    [SerializeField] private float _maxSpeed = 200f;

    [Header("Display Settings")]
    [SerializeField] private bool _showKMH = true;
    [SerializeField] private bool _roundToInteger = true;
    [SerializeField] private bool _showGear = true;
    [SerializeField] private bool _showRPM = true;

    private void Start()
    {
        // Автоматически находим автомобиль если не назначен
        if (_vehicleController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _vehicleController = player.GetComponent<CustomVehicleController>();

            if (_vehicleController == null)
                _vehicleController = FindAnyObjectByType<CustomVehicleController>();
        }

        // Предупреждение если нет текстовых компонентов
        if (_speedText == null && _speedTextTMP == null)
        {
            Debug.LogWarning("SimpleSpeedDisplay: No text components assigned!");
        }
    }

    private void Update()
    {
        if (_vehicleController == null)
        {
            // Продолжаем поиск автомобиля если не нашли
            _vehicleController = FindAnyObjectByType<CustomVehicleController>();
            if (_vehicleController == null) return;
        }

        var carStats = _vehicleController.GetCurrentCarStats();

        // Обновляем скорость
        UpdateSpeedDisplay(carStats.SpeedInKMperH);

        // Обновляем дополнительные показатели
        UpdateAdditionalDisplays(carStats);

        // Обновляем слайдер
        UpdateSlider(carStats);

        // Обновляем стрелки
        UpdateNeedles(carStats);
    }

    private void UpdateSpeedDisplay(float speed)
    {
        string speedText = _roundToInteger ?
            Mathf.Abs((int)speed).ToString() :
            Mathf.Abs(speed).ToString("F1");

        // Обновляем текстовые компоненты (оба типа)
        if (_speedTextTMP != null)
            _speedTextTMP.text = speedText;

        if (_speedText != null)
            _speedText.text = speedText;
    }

    private void UpdateAdditionalDisplays(CurrentCarStats carStats)
    {
        // Обновляем передачу
        if (_showGear && _gearTextTMP != null)
            _gearTextTMP.text = carStats.CurrentGear;

        // Обновляем RPM текст
        if (_showRPM && _rpmTextTMP != null)
        {
            string rpmText = _roundToInteger ?
                ((int)carStats.EngineRPMPercent).ToString() :
                carStats.EngineRPMPercent.ToString("F1");

            _rpmTextTMP.text = rpmText;
        }
    }

    private void UpdateSlider(CurrentCarStats carStats)
    {
        // Обновляем слайдер RPM
        if (_rpmSlider != null)
            _rpmSlider.value = carStats.EngineRPMPercent;
    }


    private void UpdateNeedles(CurrentCarStats carStats)
    {
        // Обновляем стрелку скорости
        if (_speedNeedle != null)
        {
            float speedNormalized = Mathf.Clamp01(carStats.SpeedInKMperH / _maxSpeed);
            float speedAngle = Mathf.Lerp(_speedMinAngle, _speedMaxAngle, speedNormalized);
            _speedNeedle.transform.localEulerAngles = new Vector3(0, 0, speedAngle);
        }

        // Обновляем стрелку RPM
        if (_rpmNeedle != null)
        {
            float rpmNormalized = Mathf.Clamp01(carStats.EngineRPMPercent / 100f);
            float rpmAngle = Mathf.Lerp(_rpmMinAngle, _rpmMaxAngle, rpmNormalized);
            _rpmNeedle.transform.localEulerAngles = new Vector3(0, 0, rpmAngle);
        }
    }

    // Методы для настройки из других скриптов
    public void SetVehicle(CustomVehicleController vehicle)
    {
        _vehicleController = vehicle;
    }

    public void SetTextColor(Color color)
    {
        if (_speedTextTMP != null)
            _speedTextTMP.color = color;

        if (_speedText != null)
            _speedText.color = color;

        if (_gearTextTMP != null)
            _gearTextTMP.color = color;

        if (_rpmTextTMP != null)
            _rpmTextTMP.color = color;
    }

    public void ShowGear(bool show)
    {
        _showGear = show;
        if (_gearTextTMP != null)
            _gearTextTMP.gameObject.SetActive(show);
    }

    public void ShowRPM(bool show)
    {
        _showRPM = show;
        if (_rpmTextTMP != null)
            _rpmTextTMP.gameObject.SetActive(show);
        if (_rpmSlider != null)
            _rpmSlider.gameObject.SetActive(show);
        if (_rpmNeedle != null)
            _rpmNeedle.gameObject.SetActive(show);
    }
}