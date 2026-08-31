using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Сетап-меню перед гонкой. Порядок: игрок въезжает в RaceStartZone → зона
    /// проигрывает отсчёт 3-2-1-GO (TimerBefore) → по его завершении зона зовёт
    /// Open(): игра замирает, вокруг игрока встаёт тёмная оболочка и world-space
    /// Canvas, где выбираются коробка, «режим езды» (пресет частей) и привод.
    /// По умолчанию — рекомендация из RaceProfile. Подтверждение (Confirm)
    /// пишет выбор в RaceSetupSelection (его подхватит RaceCarSetupApplier на
    /// заспавненной машине), блокирует тюнинг DemoManager на заезд и запускает
    /// СПАВН матча (StartRaceNow) — отсчёт к этому моменту уже прошёл. На финише
    /// — разблокировка и очистка.
    ///
    /// Переключение делается ЗНАКОМЫМИ по вождению кнопками (T/Y/U и те же на
    /// руле — через AllInOneInputProvider), а не мышью: T=коробка (North),
    /// Y=режим/пресет (South), U=привод (East), R=подтвердить (West). Тюнинг
    /// DemoManager на это время залочен, поэтому те же нажатия не трогают машину.
    ///
    /// Одно меню обслуживает все старт-зоны: конкретную гонку задаёт RaceProfile,
    /// который передаёт вызывающая зона.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/Pre-Race Menu")]
    public class PreRaceMenu : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("Корень меню (world-space Canvas). Прячется в Awake, показывается при открытии.")]
        [SerializeField] private GameObject _menuRoot;
        [SerializeField] private TMP_Text _raceNameLabel;
        [SerializeField] private TMP_Text _transmissionLabel;
        [Tooltip("Строка «режима езды» (пресета частей машины). Y/South перебирает.")]
        [SerializeField] private TMP_Text _presetLabel;
        [SerializeField] private TMP_Text _drivetrainLabel;
        [Tooltip("Строка рекомендации, напр. 'Рекомендуется: Секвентал, AWD'.")]
        [SerializeField] private TMP_Text _recommendationLabel;

        [Header("Тёмная оболочка")]
        [Tooltip("Тёмная сфера-оболочка вокруг игрока (гасит вид города). Включается вместе с меню, ставится снимком у камеры. Пусто — без затемнения окружения.")]
        [SerializeField] private GameObject _darkShell;

        [Header("Placement")]
        [Tooltip("Ставит меню снимком перед камерой при открытии (чтобы не тряслось за головой в VR). Пусто — меню остаётся там, где стоит в сцене.")]
        [SerializeField] private WorldSpaceMenuPlacer _placer;
        [Tooltip("Камера, вокруг которой центрируется тёмная оболочка. Пусто — Camera.main.")]
        [SerializeField] private Camera _camera;

        [Header("Behaviour")]
        [Tooltip("Замораживать игру (timeScale=0), пока открыто меню.")]
        [SerializeField] private bool _pauseWhileOpen = true;
        [Tooltip("Антидребезг переключений кнопками, сек (нескалированное время).")]
        [SerializeField] private float _buttonCooldown = 0.25f;
        [Tooltip("DemoManager: источник списка пресетов + блокировка тюнинга на гонку. Пусто — найдётся автоматически.")]
        [SerializeField] private DemoManager _demoManager;
        [Tooltip("Финиш-зона: по её OnTimerFinished разблокируется DemoManager и очищается сетап. Пусто — найдётся автоматически при подтверждении.")]
        [SerializeField] private RaceFinishZone _finishZone;

        // Порядок циклов совпадает с DemoManager.SwapTransmissionType/Drivetrain.
        private static readonly TransmissionType[] _transmissions =
            { TransmissionType.Automatic, TransmissionType.Sequential, TransmissionType.Manual };
        private static readonly DrivetrainType[] _drivetrains =
            { DrivetrainType.RWD, DrivetrainType.AWD, DrivetrainType.FWD };

        private static readonly Dictionary<TransmissionType, string> _transmissionNames = new()
        {
            { TransmissionType.Automatic, "Автомат" },
            { TransmissionType.Sequential, "Секвентал" },
            { TransmissionType.Manual, "Механика" },
        };

        private int _transmissionIndex;
        private int _drivetrainIndex;

        // Пресеты («режимы езды»): индекс 0 = «Штатный» (без пресета, null),
        // далее — DemoManager.TuningPresets. Собирается при Open, т.к. список
        // живёт в DemoManager.
        private readonly List<VehiclePartsPresetSO> _presets = new();
        private int _presetIndex;

        private RaceStartZone _pendingZone;
        private bool _isOpen;

        // Кнопки берём с провайдера активного игрока (той машины, на которой
        // въехал) — он объединяет клавиатуру R/T/Y/U + руль + MOZA.
        private AllInOneInputProvider _inputProvider;
        private bool _prevNorth, _prevSouth, _prevEast, _prevWest;
        private float _lastButtonTime;

        private void Awake()
        {
            if (_menuRoot != null)
                _menuRoot.SetActive(false);
            if (_darkShell != null)
                _darkShell.SetActive(false);
        }

        /// <summary>Зовёт RaceStartZone ПОСЛЕ отсчёта. profile может быть null (тогда дефолты).</summary>
        public void Open(RaceStartZone zone, RaceProfile profile)
        {
            if (_isOpen)
                return;

            _pendingZone = zone;

            BuildPresetList();

            // Дефолт — рекомендация под гонку (сменяемая игроком). Пресет по
            // умолчанию «Штатный» (0): RaceProfile его пока не задаёт.
            _transmissionIndex = profile != null ? IndexOf(_transmissions, profile.RecommendedTransmission) : 0;
            _drivetrainIndex = profile != null ? IndexOf(_drivetrains, profile.RecommendedDrivetrain) : 0;
            _presetIndex = 0;

            if (_raceNameLabel != null)
                _raceNameLabel.text = profile != null ? profile.RaceName : "Гонка";
            if (_recommendationLabel != null && profile != null)
                _recommendationLabel.text = $"Рекомендуется: {Name(profile.RecommendedTransmission)}, {profile.RecommendedDrivetrain}";

            // Провайдер активного игрока (машина, на которой въехали).
            _inputProvider = ResolveActiveInputProvider();

            if (_menuRoot != null)
                _menuRoot.SetActive(true);
            if (_placer != null)
                _placer.PlaceInFrontOfCamera();
            PlaceDarkShell();

            // Блокируем тюнинг сразу — те же кнопки T/Y/U теперь крутят меню, а не
            // машину; DemoManager их гасит, пока залочен.
            SetDemoTuningLocked(true);

            _isOpen = true;
            // Сбрасываем фронты, чтобы удержание кнопки со времени отсчёта не
            // сработало сразу при открытии.
            _prevNorth = _prevSouth = _prevEast = _prevWest = true;
            _lastButtonTime = Time.unscaledTime;

            if (_pauseWhileOpen)
                Time.timeScale = 0f;

            RefreshUI();
        }

        private void Update()
        {
            if (!_isOpen || _inputProvider == null)
                return;

            bool north = _inputProvider.NorthButton; // T — коробка
            bool south = _inputProvider.SouthButton; // Y — режим (пресет)
            bool east = _inputProvider.EastButton;   // U — привод
            bool west = _inputProvider.WestButton;   // R — подтвердить

            bool canPress = Time.unscaledTime - _lastButtonTime > _buttonCooldown;

            if (canPress)
            {
                if (north && !_prevNorth) { CycleTransmission(); _lastButtonTime = Time.unscaledTime; }
                else if (south && !_prevSouth) { CyclePreset(); _lastButtonTime = Time.unscaledTime; }
                else if (east && !_prevEast) { CycleDrivetrain(); _lastButtonTime = Time.unscaledTime; }
                else if (west && !_prevWest) { Confirm(); return; } // после Confirm _isOpen=false
            }

            _prevNorth = north;
            _prevSouth = south;
            _prevEast = east;
            _prevWest = west;
        }

        /// <summary>T / North: следующий тип коробки.</summary>
        public void CycleTransmission()
        {
            _transmissionIndex = (_transmissionIndex + 1) % _transmissions.Length;
            RefreshUI();
        }

        /// <summary>U / East: следующий тип привода.</summary>
        public void CycleDrivetrain()
        {
            _drivetrainIndex = (_drivetrainIndex + 1) % _drivetrains.Length;
            RefreshUI();
        }

        /// <summary>Y / South: следующий «режим езды» (пресет частей машины).</summary>
        public void CyclePreset()
        {
            if (_presets.Count == 0)
                return;
            _presetIndex = (_presetIndex + 1) % _presets.Count;
            RefreshUI();
        }

        /// <summary>R / West (или кнопка «К старту»): фиксирует сетап и запускает спавн матча.</summary>
        public void Confirm()
        {
            if (!_isOpen)
                return;

            VehiclePartsPresetSO preset = _presetIndex >= 0 && _presetIndex < _presets.Count
                ? _presets[_presetIndex] : null;

            // Записываем выбор — RaceCarSetupApplier на заспавненной машине
            // подхватит его в своём Start.
            RaceSetupSelection.Set(_transmissions[_transmissionIndex], _drivetrains[_drivetrainIndex], preset);

            HookFinishUnlock();

            if (_pauseWhileOpen)
                Time.timeScale = 1f;
            if (_menuRoot != null)
                _menuRoot.SetActive(false);
            if (_darkShell != null)
                _darkShell.SetActive(false);
            _isOpen = false;
            _inputProvider = null;

            // Отсчёт уже прошёл ДО меню — теперь сразу запускаем матч (спавн).
            if (_pendingZone != null)
                _pendingZone.StartRaceNow();
        }

        private void BuildPresetList()
        {
            _presets.Clear();
            _presets.Add(null); // «Штатный» — без пресета
            EnsureDemoManager();
            if (_demoManager != null && _demoManager.TuningPresets != null)
            {
                foreach (var p in _demoManager.TuningPresets)
                    if (p != null)
                        _presets.Add(p);
            }
        }

        private AllInOneInputProvider ResolveActiveInputProvider()
        {
            var player = PlayerLocator.GetActivePlayer();
            if (player == null)
                return null;
            var provider = player.GetComponent<AllInOneInputProvider>();
            if (provider == null)
                provider = player.GetComponentInChildren<AllInOneInputProvider>();
            return provider;
        }

        private void PlaceDarkShell()
        {
            if (_darkShell == null)
                return;
            _darkShell.SetActive(true);
            Camera cam = _camera != null ? _camera : Camera.main;
            if (cam != null)
                _darkShell.transform.position = cam.transform.position;
        }

        private void HookFinishUnlock()
        {
            if (_finishZone == null)
                _finishZone = FindAnyObjectByType<RaceFinishZone>();

            if (_finishZone != null)
            {
                _finishZone.OnTimerFinished.RemoveListener(OnRaceFinished);
                _finishZone.OnTimerFinished.AddListener(OnRaceFinished);
            }
        }

        private void OnRaceFinished()
        {
            SetDemoTuningLocked(false);
            RaceSetupSelection.Clear();
            if (_finishZone != null)
                _finishZone.OnTimerFinished.RemoveListener(OnRaceFinished);
        }

        private void EnsureDemoManager()
        {
            if (_demoManager == null)
                _demoManager = FindAnyObjectByType<DemoManager>();
        }

        private void SetDemoTuningLocked(bool locked)
        {
            EnsureDemoManager();
            if (_demoManager != null)
                _demoManager.SetTuningLocked(locked);
        }

        private void RefreshUI()
        {
            if (_transmissionLabel != null)
                _transmissionLabel.text = Name(_transmissions[_transmissionIndex]);
            if (_drivetrainLabel != null)
                _drivetrainLabel.text = _drivetrains[_drivetrainIndex].ToString();
            if (_presetLabel != null)
                _presetLabel.text = PresetName(_presetIndex);
        }

        private string PresetName(int index)
        {
            if (index <= 0 || index >= _presets.Count || _presets[index] == null)
                return "Штатный";
            return _presets[index].name;
        }

        private static int IndexOf<T>(T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
                if (EqualityComparer<T>.Default.Equals(arr[i], value))
                    return i;
            return 0;
        }

        private static string Name(TransmissionType t) =>
            _transmissionNames.TryGetValue(t, out var n) ? n : t.ToString();
    }
}
