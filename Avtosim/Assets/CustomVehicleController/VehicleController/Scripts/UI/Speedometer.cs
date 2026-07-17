using UnityEngine;
using UnityEngine.UI;

namespace Assets.VehicleController
{
    public class Speedometer : MonoBehaviour
    {
        [SerializeField]
        private CustomVehicleController _vehicleController;

        [SerializeField]
        private Text _speedText;
        [SerializeField]
        private Slider _rpmSlider;

        [SerializeField]
        private Text _currentGearText;
        [SerializeField]
        private Text _nitroBottlesLeft;

        [SerializeField]
        private Slider _nitroSlider;
        [SerializeField]
        private Slider _boostSlider;

        private void Start()
        {
            if (_vehicleController != null)
                return;

            ResolveVehicle();
        }

        // PlayerLocator живёт в Assembly-CSharp и недоступен из этой (vendor)
        // ассембли CustomVehicleController, поэтому здесь оставлен локальный
        // поиск активного игрока: сначала объект с тегом Player (выключенные
        // машины Unity игнорирует), иначе — любая машина в сцене.
        private void ResolveVehicle()
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
                _vehicleController = go.GetComponent<CustomVehicleController>();

            if (_vehicleController == null)
                _vehicleController = FindAnyObjectByType<CustomVehicleController>();
        }

        private void Update()
        {
            if (_vehicleController == null)
            {
                ResolveVehicle();
                if (_vehicleController == null)
                    return; // раньше здесь был NRE, если игрок ещё не найден
            }
            _currentGearText.text = _vehicleController.GetCurrentCarStats().CurrentGear;
            _nitroBottlesLeft.text = _vehicleController.GetCurrentCarStats().NitroBottlesLeft.ToString();

            _speedText.text = ((int)Mathf.Abs(_vehicleController.GetCurrentCarStats().SpeedInKMperH)).ToString();
            _nitroSlider.value = _vehicleController.GetCurrentCarStats().NitroPercentLeft;
            _rpmSlider.value = _vehicleController.GetCurrentCarStats().EngineRPMPercent;
            _boostSlider.value = _vehicleController.GetCurrentCarStats().ForcedInductionBoostPercent;
        }

    }
}