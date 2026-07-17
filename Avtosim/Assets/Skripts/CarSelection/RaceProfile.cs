using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Профиль отдельной гонки: имя + рекомендованный сетап (коробка/привод),
    /// который pre-race меню подставит по умолчанию (игрок может сменить).
    /// Отдельный ScriptableObject-ассет на каждую гонку — чтобы позже сюда же
    /// класть награды, число кругов и прочие параметры трассы, не трогая код.
    ///
    /// Живёт в Assembly-CSharp (Assets/Skripts), поэтому свободно ссылается на
    /// перечисления TransmissionType/DrivetrainType из ассембли
    /// CustomVehicleController.
    /// </summary>
    [CreateAssetMenu(fileName = "RaceProfile", menuName = "CustomVehicleController/Race Profile")]
    public class RaceProfile : ScriptableObject
    {
        [SerializeField] private string _raceName = "Гонка";

        [Header("Рекомендованный сетап (дефолт в pre-race меню, сменяемый)")]
        [SerializeField] private TransmissionType _recommendedTransmission = TransmissionType.Sequential;
        [SerializeField] private DrivetrainType _recommendedDrivetrain = DrivetrainType.AWD;

        public string RaceName => _raceName;
        public TransmissionType RecommendedTransmission => _recommendedTransmission;
        public DrivetrainType RecommendedDrivetrain => _recommendedDrivetrain;
    }
}
