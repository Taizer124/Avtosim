using Assets.VehicleController;
using UnityEngine;
using UnityEngine.UI;

public class SimpleSpeedDisplay : MonoBehaviour
{
    [Header("Vehicle Reference")]
    [SerializeField] private CustomVehicleController _vehicleController;

    [Header("Text Components - Use One or Both")]
    [SerializeField] private Text _speedText;
    [SerializeField] private TMPro.TextMeshProUGUI _speedTextTMP;

    [Header("Display Settings")]
    [SerializeField] private bool _showKMH = true;
    [SerializeField] private bool _roundToInteger = true;
    [SerializeField] private string _suffix = "";

    private void Start()
    {
        // Автоматически находим автомобиль если не назначен
        if (_vehicleController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _vehicleController = player.GetComponent<CustomVehicleController>();

            if (_vehicleController == null)
                _vehicleController = FindObjectOfType<CustomVehicleController>();
        }
    }

    private void Update()
    {
        if (_vehicleController == null) return;

        // Получаем скорость
        float speed = _vehicleController.GetCurrentCarStats().SpeedInKMperH;

        // Форматируем отображение
        string speedText = _roundToInteger ?
            Mathf.Abs((int)speed).ToString() :
            Mathf.Abs(speed).ToString("F1");

        if (!string.IsNullOrEmpty(_suffix))
            speedText += _suffix;

        // Обновляем текстовые компоненты
        if (_speedTextTMP != null)
            _speedTextTMP.text = speedText;

        if (_speedText != null)
            _speedText.text = speedText;
    }

    // Методы для настройки из других скриптов
    public void SetVehicle(CustomVehicleController vehicle)
    {
        _vehicleController = vehicle;
    }

    public void SetSuffix(string suffix)
    {
        _suffix = suffix;
    }

    public void SetTextColor(Color color)
    {
        if (_speedTextTMP != null)
            _speedTextTMP.color = color;

        if (_speedText != null)
            _speedText.color = color;
    }
}