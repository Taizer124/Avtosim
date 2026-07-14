using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.VehicleController;

public class SimpleSpeedDisplay : MonoBehaviour
{
    public enum NeedleDisplayMode { Rotation, Fill, PhysicalRotation }


    [Header("Vehicle Reference")]
    [SerializeField] private CustomVehicleController _vehicleController;

    [Header("Text Components")]
    [SerializeField] private Text _speedText;
    [SerializeField] private TextMeshProUGUI _speedTextTMP;
    [SerializeField] private TextMeshProUGUI _gearTextTMP;
    [SerializeField] private TextMeshProUGUI _rpmTextTMP;

    [Header("UI Sliders")]
    [SerializeField] private Slider _rpmSlider;

    [Header("Needle Mode")]
    [SerializeField] private NeedleDisplayMode _speedNeedleMode = NeedleDisplayMode.Rotation;
    [SerializeField] private NeedleDisplayMode _rpmNeedleMode = NeedleDisplayMode.Rotation;

    [Header("Needle References (Rotation)")]
    [SerializeField] private Image _speedNeedle;
    [SerializeField] private Image _rpmNeedle;

    [Header("Needle References (Fill)")]
    [SerializeField] private Image _speedNeedleFill;
    [SerializeField] private Image _rpmNeedleFill;

    // Отдельный физический GameObject (обычный Transform, не UI Image) —
    // например стрелка на 3D-модели приборной панели в кабине, а не на
    // Canvas. Поворачивается через Transform.localRotation, а не
    // localEulerAngles на RectTransform, и может крутиться вокруг любой оси
    // (задаётся _speedPhysicalRotationAxis), а не только вокруг Z как у UI.
    [Header("Needle Reference (Physical GameObject)")]
    [SerializeField] private Transform _speedNeedlePhysical;
    [SerializeField] private Transform _rpmNeedlePhysical;

    [Header("Needle Settings")]
    [SerializeField] private float _speedMinAngle = 45f;
    [SerializeField] private float _speedMaxAngle = -225f;
    [SerializeField] private float _rpmMinAngle = 45f;
    [SerializeField] private float _rpmMaxAngle = -225f;
    [SerializeField] private float _maxSpeed = 200f;

    [Header("Needle Settings (Fill)")]
    [SerializeField] private float _speedFillMin = 0f;
    [SerializeField] private float _speedFillMax = 1f;
    [SerializeField] private float _rpmFillMin = 0f;
    [SerializeField] private float _rpmFillMax = 1f;

    [Header("Needle Settings (Physical GameObject)")]
    [SerializeField] private float _speedPhysicalMinAngle = 0f;
    [SerializeField] private float _speedPhysicalMaxAngle = -270f;
    [SerializeField] private Vector3 _speedPhysicalRotationAxis = Vector3.forward;
    [SerializeField] private float _rpmPhysicalMinAngle = 0f;
    [SerializeField] private float _rpmPhysicalMaxAngle = -270f;
    [SerializeField] private Vector3 _rpmPhysicalRotationAxis = Vector3.forward;

    [Header("Display Settings")]
    [SerializeField] private bool _roundToInteger = true;
    [SerializeField] private bool _showGear = true;
    [SerializeField] private bool _showRPM = true;

    private void Start()
    {
        // ������������� ������� ���������� ���� �� ��������
        if (_vehicleController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _vehicleController = player.GetComponent<CustomVehicleController>();

            if (_vehicleController == null)
                _vehicleController = FindAnyObjectByType<CustomVehicleController>();
        }

        // �������������� ���� ��� ��������� �����������
        if (_speedText == null && _speedTextTMP == null)
        {
            Debug.LogWarning("SimpleSpeedDisplay: No text components assigned!");
        }
    }

    private void Update()
    {
        if (_vehicleController == null)
        {
            // ���������� ����� ���������� ���� �� �����
            _vehicleController = FindAnyObjectByType<CustomVehicleController>();
            if (_vehicleController == null) return;
        }

        var carStats = _vehicleController.GetCurrentCarStats();

        // ��������� ��������
        UpdateSpeedDisplay(carStats.SpeedInKMperH);

        // ��������� �������������� ����������
        UpdateAdditionalDisplays(carStats);

        // ��������� �������
        UpdateSlider(carStats);

        // ��������� �������
        UpdateNeedles(carStats);
    }

    private void UpdateSpeedDisplay(float speed)
    {
        string speedText = _roundToInteger ?
            Mathf.Abs((int)speed).ToString() :
            Mathf.Abs(speed).ToString("F1");

        // ��������� ��������� ���������� (��� ����)
        if (_speedTextTMP != null)
            _speedTextTMP.text = speedText;

        if (_speedText != null)
            _speedText.text = speedText;
    }

    private void UpdateAdditionalDisplays(CurrentCarStats carStats)
    {
        // ��������� ��������
        if (_showGear && _gearTextTMP != null)
            _gearTextTMP.text = carStats.CurrentGear;

        // ��������� RPM �����
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
        // ��������� ������� RPM
        if (_rpmSlider != null)
            _rpmSlider.value = carStats.EngineRPMPercent;
    }


    private void UpdateNeedles(CurrentCarStats carStats)
    {
        float speedNormalized = Mathf.Clamp01(carStats.SpeedInKMperH / _maxSpeed);
        float rpmNormalized = Mathf.Clamp01(carStats.EngineRPMPercent / 100f);

        // Спидометр: либо поворот физической стрелки, либо Fill Amount
        // графической (Image Type = Filled) — выбирается через _speedNeedleMode,
        // работает независимо от тахометра.
        if (_speedNeedleMode == NeedleDisplayMode.Rotation && _speedNeedle != null)
        {
            float speedAngle = Mathf.Lerp(_speedMinAngle, _speedMaxAngle, speedNormalized);
            _speedNeedle.transform.localEulerAngles = new Vector3(0, 0, speedAngle);
        }
        else if (_speedNeedleMode == NeedleDisplayMode.Fill && _speedNeedleFill != null)
        {
            _speedNeedleFill.fillAmount = Mathf.Lerp(_speedFillMin, _speedFillMax, speedNormalized);
        }
        else if (_speedNeedleMode == NeedleDisplayMode.PhysicalRotation && _speedNeedlePhysical != null)
        {
            float speedPhysicalAngle = Mathf.Lerp(_speedPhysicalMinAngle, _speedPhysicalMaxAngle, speedNormalized);
            _speedNeedlePhysical.localRotation = Quaternion.AngleAxis(speedPhysicalAngle, _speedPhysicalRotationAxis);
        }

        // Тахометр: та же логика выбора режима, отдельная от спидометра.
        if (_rpmNeedleMode == NeedleDisplayMode.Rotation && _rpmNeedle != null)
        {
            float rpmAngle = Mathf.Lerp(_rpmMinAngle, _rpmMaxAngle, rpmNormalized);
            _rpmNeedle.transform.localEulerAngles = new Vector3(0, 0, rpmAngle);
        }
        else if (_rpmNeedleMode == NeedleDisplayMode.Fill && _rpmNeedleFill != null)
        {
            _rpmNeedleFill.fillAmount = Mathf.Lerp(_rpmFillMin, _rpmFillMax, rpmNormalized);
        }
        else if (_rpmNeedleMode == NeedleDisplayMode.PhysicalRotation && _rpmNeedlePhysical != null)
        {
            float rpmPhysicalAngle = Mathf.Lerp(_rpmPhysicalMinAngle, _rpmPhysicalMaxAngle, rpmNormalized);
            _rpmNeedlePhysical.localRotation = Quaternion.AngleAxis(rpmPhysicalAngle, _rpmPhysicalRotationAxis);
        }
    }

    // ������ ��� ��������� �� ������ ��������
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
        if (_rpmNeedleFill != null)
            _rpmNeedleFill.gameObject.SetActive(show);
        if (_rpmNeedlePhysical != null)
            _rpmNeedlePhysical.gameObject.SetActive(show);
    }
}