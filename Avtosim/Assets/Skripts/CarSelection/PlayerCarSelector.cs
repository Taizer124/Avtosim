using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Ставится одним экземпляром на игровую сцену (MVP). В инспектор кладутся
    /// три корневых объекта машин (player / player (1) / player (2)) в том же
    /// порядке, что и индексы выбора в меню.
    ///
    /// В Awake включает выбранную в меню машину и выключает остальные, а
    /// выбранной проставляет тег Player. Всё остальное (камера, UI, пауза,
    /// DemoManager, гонка) находит игрока уже сама — по тегу через
    /// PlayerLocator. RaceParticipant регистрируется в RaceManager только у
    /// активного объекта, поэтому выключенные машины автоматически не
    /// становятся игроком/участником.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/Player Car Selector")]
    [DefaultExecutionOrder(-100)] // раньше Start/Awake потребителей, чтобы тег уже стоял
    public class PlayerCarSelector : MonoBehaviour
    {
        [Tooltip("Корневые объекты машин в порядке индексов выбора из меню (0,1,2).")]
        [SerializeField] private GameObject[] _cars;

        [Tooltip("Тег, которым помечается выбранная машина. Должен существовать в Tags & Layers.")]
        [SerializeField] private string _playerTag = "Player";

        private void Awake()
        {
            if (_cars == null || _cars.Length == 0)
            {
                Debug.LogError("[PlayerCarSelector] Не назначены машины (_cars).");
                return;
            }

            int selected = CarSelection.SelectedIndex;

            // Ищем машину, чей индекс (из CarIdentity, иначе позиция в массиве)
            // совпадает с выбранным. Благодаря CarIdentity порядок массива _cars
            // больше не важен — рассинхрон выбора невозможен.
            int activePos = -1;
            for (int i = 0; i < _cars.Length; i++)
            {
                if (_cars[i] != null && ResolveIndex(_cars[i], i) == selected)
                {
                    activePos = i;
                    break;
                }
            }

            // Никто не подошёл по индексу (например CarIdentity не проставлены) —
            // откатываемся на позицию в массиве.
            if (activePos < 0)
                activePos = Mathf.Clamp(selected, 0, _cars.Length - 1);

            for (int i = 0; i < _cars.Length; i++)
            {
                if (_cars[i] == null)
                {
                    Debug.LogWarning($"[PlayerCarSelector] Ячейка машины {i} не назначена.");
                    continue;
                }

                bool isSelected = (i == activePos);
                _cars[i].SetActive(isSelected);

                if (isSelected)
                    _cars[i].tag = _playerTag;
            }

            // На случай, если PlayerLocator закэшировал ссылку в прошлой сцене:
            // заставляем его перерезолвить активного игрока заново.
            PlayerLocator.Invalidate();

            Debug.Log($"[PlayerCarSelector] Выбор #{selected} → активна '{_cars[activePos]?.name}', тег '{_playerTag}'.");
        }

        // Индекс машины: приоритет — компонент CarIdentity, иначе позиция в массиве.
        private int ResolveIndex(GameObject car, int fallbackPosition)
        {
            var identity = car.GetComponent<CarIdentity>();
            return identity != null ? identity.CarIndex : fallbackPosition;
        }
    }
}
