using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Подставляет в RaceSpawner префаб гоночной машины, соответствующий
    /// выбранной игроком машине (CarSelection.SelectedIndex) — чтобы в гонке
    /// спавнилась та же машина, на которой игрок приехал, а не всегда первая.
    ///
    /// Существует отдельным компонентом, потому что RaceSpawner лежит в
    /// ассембли CustomVehicleController и не может видеть CarSelection из
    /// Assembly-CSharp. Здесь же (Assembly-CSharp) обе стороны доступны:
    /// CarSelection напрямую, а RaceSpawner.PlayerVehiclePrefab — как public
    /// поле.
    ///
    /// Выполняется РАНЬШЕ спавна (DefaultExecutionOrder ниже), чтобы успеть
    /// подменить префаб до RaceSpawner.Spawn() даже при SpawnCondition=OnAwake.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/Race Player Car Binder")]
    [DefaultExecutionOrder(-200)]
    public class RacePlayerCarBinder : MonoBehaviour
    {
        [SerializeField] private RaceSpawner _raceSpawner;

        [Tooltip("Единый каталог машин. Гоночный префаб берётся из него по CarSelection.SelectedIndex — отдельного массива больше нет, рассинхрон невозможен. Если не задан — берётся из EconomyManager.")]
        [SerializeField] private CarDatabase _database;

        private void Awake()
        {
            if (_raceSpawner == null)
                _raceSpawner = GetComponent<RaceSpawner>();
            if (_raceSpawner == null)
                _raceSpawner = FindAnyObjectByType<RaceSpawner>();

            if (_raceSpawner == null)
            {
                Debug.LogWarning("[RacePlayerCarBinder] RaceSpawner не найден — префаб гонки не подменён.");
                return;
            }

            CarDatabase db = _database != null ? _database
                : (EconomyManager.Instance != null ? EconomyManager.Instance.Database : null);

            if (db == null || db.Count == 0)
            {
                Debug.LogWarning("[RacePlayerCarBinder] Нет CarDatabase — используется префаб по умолчанию из RaceSpawner.");
                return;
            }

            int idx = Mathf.Clamp(CarSelection.SelectedIndex, 0, db.Count - 1);
            GameObject prefab = db.GetRacePrefab(idx);
            if (prefab != null)
            {
                _raceSpawner.PlayerVehiclePrefab = prefab;
                Debug.Log($"[RacePlayerCarBinder] Гоночный префаб игрока подставлен по индексу {idx}: '{prefab.name}'.");
            }
            else
            {
                Debug.LogWarning($"[RacePlayerCarBinder] В CarDatabase у машины #{idx} не задан RacePrefab.");
            }
        }
    }
}
