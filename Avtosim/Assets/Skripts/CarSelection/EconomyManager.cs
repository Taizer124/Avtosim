using System;
using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Единый персистентный менеджер экономики и прогрессии: монеты, владение
    /// машинами, каталог машин (имя/описание/цена) и таблица наград за места в
    /// гонке. Живёт между сценами (DontDestroyOnLoad, синглтон), состояние
    /// (монеты + владение) сохраняется в PlayerPrefs, а конфиг (каталог,
    /// награды) настраивается в инспекторе одного объекта — как и просил
    /// пользователь ("гибко в одном менеджере").
    ///
    /// Ставится ОДИН экземпляр в первую сцену (MainMenu). При возврате в меню
    /// дубликат самоуничтожается, оставляя исходный персистентный.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/Economy Manager")]
    public class EconomyManager : MonoBehaviour
    {
        [Header("Car Catalog")]
        [Tooltip("Единый каталог машин (имя/описание/цена/бесплатна-ли/гоночный префаб). Один ассет на весь проект.")]
        [SerializeField] private CarDatabase _database;

        [Header("Coins")]
        [SerializeField, Min(0)] private int _startingCoins = 0;

        [Header("Race Rewards")]
        [Tooltip("Награда монетами по месту: индекс 0 = 1-е место, 1 = 2-е и т.д. Места вне массива дают 0.")]
        [SerializeField] private int[] _placementRewards = { 500, 250, 100 };

        public event Action<int> OnCoinsChanged;   // новый баланс
        public event Action OnOwnershipChanged;

        private const string KEY_COINS = "Economy_Coins";
        private const string KEY_OWNED_PREFIX = "Economy_CarOwned_";

        private int _coins;
        private bool[] _owned;

        public static EconomyManager Instance { get; private set; }

        public int Coins => _coins;
        public int CarCount => _database != null ? _database.Count : 0;
        public CarDatabase Database => _database;

        // Гарантируем, что менеджер существует, даже если игрок запустил игровую
        // сцену напрямую (без прохода через меню, где стоит настроенный
        // экземпляр). Авто-созданный не имеет каталога машин, но монеты и
        // награды за гонку работают и сохраняются в PlayerPrefs. Настроенный в
        // меню экземпляр стартует раньше и остаётся главным (синглтон в Awake).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (Instance == null)
                new GameObject("EconomyManager (auto)").AddComponent<EconomyManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
        }

        private void Load()
        {
            _coins = PlayerPrefs.GetInt(KEY_COINS, _startingCoins);

            int count = CarCount;
            _owned = new bool[count];
            for (int i = 0; i < count; i++)
            {
                int def = _database.IsOwnedByDefault(i) ? 1 : 0;
                _owned[i] = PlayerPrefs.GetInt(KEY_OWNED_PREFIX + i, def) == 1;
            }
        }

        private void Save()
        {
            PlayerPrefs.SetInt(KEY_COINS, _coins);
            if (_owned != null)
                for (int i = 0; i < _owned.Length; i++)
                    PlayerPrefs.SetInt(KEY_OWNED_PREFIX + i, _owned[i] ? 1 : 0);
            PlayerPrefs.Save();
        }

        // --- Монеты ---
        public void AddCoins(int amount)
        {
            if (amount == 0)
                return;

            _coins = Mathf.Max(0, _coins + amount);
            Save();
            OnCoinsChanged?.Invoke(_coins);
        }

        /// <summary>Списать монеты, если хватает. true — успех.</summary>
        public bool TrySpend(int amount)
        {
            if (amount < 0 || _coins < amount)
                return false;

            _coins -= amount;
            Save();
            OnCoinsChanged?.Invoke(_coins);
            return true;
        }

        // --- Каталог (проксируем в CarDatabase) ---
        public string GetName(int index) => _database != null ? _database.GetName(index) : "";
        public string GetDescription(int index) => _database != null ? _database.GetDescription(index) : "";
        public int GetPrice(int index) => _database != null ? _database.GetPrice(index) : 0;

        // --- Владение / покупка ---
        public bool IsOwned(int index) => IsValid(index) && _owned[index];

        /// <summary>
        /// Купить машину: списывает цену, если хватает монет и она ещё не
        /// куплена. Возвращает true, если после вызова машина во владении.
        /// </summary>
        public bool TryPurchase(int index)
        {
            if (!IsValid(index))
                return false;

            if (_owned[index])
                return true; // уже куплена

            if (!TrySpend(_database.GetPrice(index)))
                return false; // не хватает монет

            _owned[index] = true;
            Save();
            OnOwnershipChanged?.Invoke();
            return true;
        }

        // --- Награда за гонку ---
        /// <summary>
        /// Начислить награду за место игрока (1 = первое). Место вне таблицы
        /// _placementRewards даёт 0. Вызывается из RaceRewardGranter по событию
        /// RaceManager.OnPlayerFinished.
        /// </summary>
        public void AwardRacePlacement(int placement)
        {
            int idx = placement - 1; // 1-е место = индекс 0
            if (_placementRewards != null && idx >= 0 && idx < _placementRewards.Length)
                AddCoins(_placementRewards[idx]);
        }

        private bool IsValid(int index) => _database != null && index >= 0 && index < _database.Count;
    }
}
