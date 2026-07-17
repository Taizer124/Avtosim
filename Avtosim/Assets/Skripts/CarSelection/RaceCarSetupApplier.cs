using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Ставится на префаб гоночной машины (тот, что спавнит RaceSpawner). В Start
    /// применяет выбранный в pre-race меню сетап (коробка/привод) из
    /// RaceSetupSelection к своему CustomVehicleController и синхронизирует режим
    /// коробки с AllInOneInputProvider (иначе передачи читались бы не так, как
    /// выставлен TransmissionType).
    ///
    /// Если сетап не выбирали (HasValue=false, гонка без pre-race меню) —
    /// оставляет дефолты префаба, ничего не трогая.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/Race Car Setup Applier")]
    public class RaceCarSetupApplier : MonoBehaviour
    {
        [Tooltip("Контроллер машины. Пусто — ищется в детях этого объекта.")]
        [SerializeField] private CustomVehicleController _vehicleController;
        [Tooltip("Провайдер ввода для синхронизации режима коробки. Пусто — ищется в детях.")]
        [SerializeField] private AllInOneInputProvider _inputProvider;

        private void Start()
        {
            if (!RaceSetupSelection.HasValue)
                return; // сетап не выбирали — оставляем дефолты префаба

            if (_vehicleController == null)
                _vehicleController = GetComponentInChildren<CustomVehicleController>();
            if (_inputProvider == null)
                _inputProvider = GetComponentInChildren<AllInOneInputProvider>();

            if (_vehicleController == null)
            {
                Debug.LogWarning("[RaceCarSetupApplier] Нет CustomVehicleController — сетап гонки не применён.");
                return;
            }

            _vehicleController.TransmissionType = RaceSetupSelection.Transmission;
            _vehicleController.DrivetrainType = RaceSetupSelection.Drivetrain;

            // Режим ввода коробки должен совпадать с типом трансмиссии, иначе
            // (напр. Manual выставлен, а провайдер в Automatic) переключения
            // передач читаются некорректно.
            if (_inputProvider != null)
                _inputProvider.SetTransmissionMode(ToInputMode(RaceSetupSelection.Transmission));

            Debug.Log($"[RaceCarSetupApplier] Сетап гонки применён: {RaceSetupSelection.Transmission} / {RaceSetupSelection.Drivetrain}.");
        }

        private static AllInOneInputProvider.TransmissionMode ToInputMode(TransmissionType t)
        {
            switch (t)
            {
                case TransmissionType.Sequential: return AllInOneInputProvider.TransmissionMode.Sequential;
                case TransmissionType.Manual: return AllInOneInputProvider.TransmissionMode.Manual;
                default: return AllInOneInputProvider.TransmissionMode.Automatic;
            }
        }
    }
}
