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
    [SerializeField] private TextMeshProUGUI _nitroTextTMP;
    [SerializeField] private TextMeshProUGUI _rpmTextTMP;

    [Header("UI Sliders")]
    [SerializeField] private Slider _rpmSlider;
    [SerializeField] private Slider _nitroSlider;
    [SerializeField] private Slider _boostSlider;

    [Header("Display Settings")]
    [SerializeField] private bool _showKMH = true;
    [SerializeField] private bool _roundToInteger = true;
    [SerializeField] private string _speedSuffix = " km/h";
    [SerializeField] private string _rpmSuffix = " RPM";
    [SerializeField] private bool _showGear = true;
    [SerializeField] private bool _showNitro = true;
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

        // Обновляем дополнительные показатели как во втором коде
        UpdateAdditionalDisplays(carStats);

        // Обновляем слайдеры
        UpdateSliders(carStats);
    }

    private void UpdateSpeedDisplay(float speed)
    {
        string speedText = _roundToInteger ?
            Mathf.Abs((int)speed).ToString() :
            Mathf.Abs(speed).ToString("F1");

        if (!string.IsNullOrEmpty(_speedSuffix))
            speedText += _speedSuffix;

        // Обновляем текстовые компоненты (оба типа)
        if (_speedTextTMP != null)
            _speedTextTMP.text = speedText;

        if (_speedText != null)
            _speedText.text = speedText;
    }

    private void UpdateAdditionalDisplays(CurrentCarStats carStats)
    {
        // Обновляем передачу (как во втором коде)
        if (_showGear && _gearTextTMP != null)
            _gearTextTMP.text = carStats.CurrentGear;

        // Обновляем нитро (как во втором коде)
        if (_showNitro && _nitroTextTMP != null)
            _nitroTextTMP.text = carStats.NitroBottlesLeft.ToString();

        // Обновляем RPM текст
        if (_showRPM && _rpmTextTMP != null)
        {
            string rpmText = _roundToInteger ?
                ((int)carStats.EngineRPMPercent).ToString() :
                carStats.EngineRPMPercent.ToString("F1");

            if (!string.IsNullOrEmpty(_rpmSuffix))
                rpmText += _rpmSuffix;

            _rpmTextTMP.text = rpmText;
        }
    }

    private void UpdateSliders(CurrentCarStats carStats)
    {
        // Обновляем слайдеры как во втором коде
        if (_rpmSlider != null)
            _rpmSlider.value = carStats.EngineRPMPercent;

        if (_nitroSlider != null)
            _nitroSlider.value = carStats.NitroPercentLeft;
        if (_boostSlider != null)
            _boostSlider.value = carStats.ForcedInductionBoostPercent;
    }

    // Методы для настройки из других скриптов
    public void SetVehicle(CustomVehicleController vehicle)
    {
        _vehicleController = vehicle;
    }

    public void SetSpeedSuffix(string suffix)
    {
        _speedSuffix = suffix;
    }

    public void SetRPMSuffix(string suffix)
    {
        _rpmSuffix = suffix;
    }

    public void SetTextColor(Color color)
    {
        if (_speedTextTMP != null)
            _speedTextTMP.color = color;

        if (_speedText != null)
            _speedText.color = color;

        if (_gearTextTMP != null)
            _gearTextTMP.color = color;

        if (_nitroTextTMP != null)
            _nitroTextTMP.color = color;

        if (_rpmTextTMP != null)
            _rpmTextTMP.color = color;
    }

    public void ShowGear(bool show)
    {
        _showGear = show;
        if (_gearTextTMP != null)
            _gearTextTMP.gameObject.SetActive(show);
    }

    public void ShowNitro(bool show)
    {
        _showNitro = show;
        if (_nitroTextTMP != null)
            _nitroTextTMP.gameObject.SetActive(show);
    }

    public void ShowRPM(bool show)
    {
        _showRPM = show;
        if (_rpmTextTMP != null)
            _rpmTextTMP.gameObject.SetActive(show);
        if (_rpmSlider != null)
            _rpmSlider.gameObject.SetActive(show);
    }
}