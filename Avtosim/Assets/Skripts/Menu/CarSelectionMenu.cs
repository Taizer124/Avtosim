using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Assets.VehicleController
{
    /// <summary>
    /// Логика Canvas выбора машины в меню. Физически переключает превью-машину
    /// стрелками ◀▶, показывает имя/описание/цену и баланс монет, даёт купить
    /// машину за монеты и запустить игру только на купленной. Все данные
    /// (имена, описания, цены, владение, монеты) берутся из EconomyManager —
    /// единого персистентного менеджера. Массивы имён ниже — только fallback,
    /// если менеджера в сцене нет.
    ///
    /// PlayerCarSelector в игровой сцене потом включит машину с выбранным
    /// индексом.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/Car Selection Menu")]
    public class CarSelectionMenu : MonoBehaviour
    {
        [Header("Cars (fallback, если нет EconomyManager)")]
        [SerializeField] private string[] _carNames = { "Car 1", "Car 2", "Car 3" };

        [Tooltip("Физические превью-объекты машин: показывается только выбранный (индекс = индекс выбора).")]
        [SerializeField] private GameObject[] _carPreviews;

        [Header("UI: инфо о машине (опционально)")]
        [SerializeField] private TMP_Text _carNameLabel;
        [SerializeField] private TMP_Text _carDescriptionLabel;
        [SerializeField] private TMP_Text _priceLabel;
        [SerializeField] private TMP_Text _coinsLabel;

        [Header("UI: покупка/запуск (опционально)")]
        [Tooltip("Кнопка покупки — активна/интерактивна только когда машина не куплена и хватает монет.")]
        [SerializeField] private Button _buyButton;
        [Tooltip("Индикатор 'куплено' — показывается для купленной машины.")]
        [SerializeField] private GameObject _ownedIndicator;
        [Tooltip("Кнопка Play — интерактивна только когда текущая машина куплена.")]
        [SerializeField] private Button _playButton;

        [Header("Scene")]
        [Tooltip("Build index игровой сцены (по умолчанию 1 = MVP).")]
        [SerializeField] private int _gameSceneBuildIndex = 1;

        private int _currentIndex;

        private EconomyManager Econ => EconomyManager.Instance;

        private void OnEnable()
        {
            if (Econ != null)
            {
                Econ.OnCoinsChanged += HandleCoinsChanged;
                Econ.OnOwnershipChanged += RefreshUI;
            }
        }

        private void OnDisable()
        {
            if (Econ != null)
            {
                Econ.OnCoinsChanged -= HandleCoinsChanged;
                Econ.OnOwnershipChanged -= RefreshUI;
            }
        }

        private void Start()
        {
            _currentIndex = Mathf.Clamp(CarSelection.SelectedIndex, 0, CarCount() - 1);
            RefreshUI();
        }

        private int CarCount()
        {
            if (Econ != null && Econ.CarCount > 0)
                return Econ.CarCount;

            int count = Mathf.Max(_carNames != null ? _carNames.Length : 0,
                                  _carPreviews != null ? _carPreviews.Length : 0);
            return Mathf.Max(count, 1);
        }

        /// <summary>Следующая машина (кнопка ▶).</summary>
        public void NextCar()
        {
            int n = CarCount();
            _currentIndex = (_currentIndex + 1) % n;
            RefreshUI();
        }

        /// <summary>Предыдущая машина (кнопка ◀).</summary>
        public void PrevCar()
        {
            int n = CarCount();
            _currentIndex = (_currentIndex - 1 + n) % n;
            RefreshUI();
        }

        /// <summary>Прямой выбор машины по индексу (для трёх отдельных кнопок).</summary>
        public void SelectCar(int index)
        {
            _currentIndex = Mathf.Clamp(index, 0, CarCount() - 1);
            RefreshUI();
        }

        /// <summary>Купить текущую машину за монеты (кнопка Buy).</summary>
        public void BuyCurrent()
        {
            if (Econ == null)
            {
                Debug.LogWarning("[CarSelectionMenu] EconomyManager отсутствует — покупка невозможна.");
                return;
            }

            Econ.TryPurchase(_currentIndex); // событие OnOwnershipChanged сам вызовет RefreshUI
        }

        /// <summary>
        /// Подтвердить выбор и загрузить игру (кнопка Play). Пускает только если
        /// машина куплена (или менеджера нет — тогда без ограничений).
        /// </summary>
        public void ConfirmAndPlay()
        {
            if (Econ != null && !Econ.IsOwned(_currentIndex))
            {
                Debug.Log("[CarSelectionMenu] Машина не куплена — сначала купи её.");
                return;
            }

            CarSelection.SelectedIndex = _currentIndex;
            SceneManager.LoadScene(_gameSceneBuildIndex);
        }

        // Индекс превью: приоритет — CarIdentity, иначе позиция в массиве.
        private int ResolveIndex(GameObject preview, int fallbackPosition)
        {
            var identity = preview.GetComponent<CarIdentity>();
            return identity != null ? identity.CarIndex : fallbackPosition;
        }

        private void HandleCoinsChanged(int _) => RefreshUI();

        private void RefreshUI()
        {
            // Физический свап превью-машины. Индекс каждого превью берётся из
            // CarIdentity (если есть), иначе — позиция в массиве. Благодаря
            // CarIdentity порядок _carPreviews не важен и не может разъехаться
            // с каталогом.
            if (_carPreviews != null)
                for (int i = 0; i < _carPreviews.Length; i++)
                    if (_carPreviews[i] != null)
                        _carPreviews[i].SetActive(ResolveIndex(_carPreviews[i], i) == _currentIndex);

            bool owned = Econ != null && Econ.IsOwned(_currentIndex);
            int price = Econ != null ? Econ.GetPrice(_currentIndex) : 0;

            // Имя (из менеджера, иначе fallback-массив).
            if (_carNameLabel != null)
            {
                string name = Econ != null ? Econ.GetName(_currentIndex) : null;
                if (string.IsNullOrEmpty(name) && _carNames != null && _currentIndex < _carNames.Length)
                    name = _carNames[_currentIndex];
                _carNameLabel.text = name;
            }

            if (_carDescriptionLabel != null && Econ != null)
                _carDescriptionLabel.text = Econ.GetDescription(_currentIndex);

            if (_priceLabel != null)
                _priceLabel.text = owned ? "Owned" : price.ToString();

            if (_coinsLabel != null && Econ != null)
                _coinsLabel.text = Econ.Coins.ToString();

            // Кнопка покупки: только для не купленной, интерактивна если хватает монет.
            if (_buyButton != null)
            {
                _buyButton.gameObject.SetActive(!owned);
                _buyButton.interactable = Econ != null && !owned && Econ.Coins >= price;
            }

            if (_ownedIndicator != null)
                _ownedIndicator.SetActive(owned);

            // Play доступен только для купленной (если менеджера нет — всегда).
            if (_playButton != null)
                _playButton.interactable = (Econ == null) || owned;
        }
    }
}
