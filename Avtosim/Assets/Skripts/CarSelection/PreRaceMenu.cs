using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Сетап-меню перед гонкой. Открывается при въезде в RaceStartZone: замирает
    /// игра, показывается world-space Canvas (с затемняющей оболочкой), где игрок
    /// выбирает коробку и привод. По умолчанию подставляется рекомендация из
    /// RaceProfile гонки, но её можно сменить. По кнопке подтверждения сетап
    /// записывается в RaceSetupSelection (его подхватит RaceCarSetupApplier на
    /// заспавненной машине), тюнинг DemoManager блокируется на время заезда, и
    /// стартует обратный отсчёт старт-зоны. На финише — разблокировка и очистка.
    ///
    /// Одно меню обслуживает все старт-зоны: конкретную гонку задаёт RaceProfile,
    /// который передаёт вызывающая зона.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/Pre-Race Menu")]
    public class PreRaceMenu : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("Корень меню (world-space Canvas + затемняющая оболочка). Прячется в Awake, показывается при въезде в старт-зону.")]
        [SerializeField] private GameObject _menuRoot;
        [SerializeField] private TMP_Text _raceNameLabel;
        [SerializeField] private TMP_Text _transmissionLabel;
        [SerializeField] private TMP_Text _drivetrainLabel;
        [Tooltip("Строка рекомендации, напр. 'Рекомендуется: Секвентал, AWD'.")]
        [SerializeField] private TMP_Text _recommendationLabel;

        [Header("Placement")]
        [Tooltip("Ставит меню снимком перед камерой при открытии (чтобы не тряслось за головой в VR). Пусто — меню остаётся там, где стоит в сцене.")]
        [SerializeField] private WorldSpaceMenuPlacer _placer;

        [Header("Behaviour")]
        [Tooltip("Замораживать игру (timeScale=0), пока открыто меню.")]
        [SerializeField] private bool _pauseWhileOpen = true;
        [Tooltip("DemoManager, у которого блокируется переключение режимов на время гонки. Пусто — найдётся автоматически.")]
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
        private RaceStartZone _pendingZone;
        private bool _isOpen;

        private void Awake()
        {
            if (_menuRoot != null)
                _menuRoot.SetActive(false);
        }

        /// <summary>Вызывает RaceStartZone при въезде игрока. profile может быть null (тогда дефолты).</summary>
        public void Open(RaceStartZone zone, RaceProfile profile)
        {
            if (_isOpen)
                return;

            _pendingZone = zone;

            // Дефолт — рекомендация под гонку (сменяемая игроком).
            _transmissionIndex = profile != null ? IndexOf(_transmissions, profile.RecommendedTransmission) : 0;
            _drivetrainIndex = profile != null ? IndexOf(_drivetrains, profile.RecommendedDrivetrain) : 0;

            if (_raceNameLabel != null)
                _raceNameLabel.text = profile != null ? profile.RaceName : "Гонка";
            if (_recommendationLabel != null && profile != null)
                _recommendationLabel.text = $"Рекомендуется: {Name(profile.RecommendedTransmission)}, {profile.RecommendedDrivetrain}";

            if (_menuRoot != null)
                _menuRoot.SetActive(true);
            if (_placer != null)
                _placer.PlaceInFrontOfCamera();

            // Блокируем тюнинг сразу — чтобы кнопки руля не меняли коробку
            // городской машины, пока открыт сетап.
            SetDemoTuningLocked(true);

            _isOpen = true;
            if (_pauseWhileOpen)
                Time.timeScale = 0f;

            RefreshUI();
        }

        /// <summary>Кнопка ▶ у коробки: следующий тип трансмиссии.</summary>
        public void CycleTransmission()
        {
            _transmissionIndex = (_transmissionIndex + 1) % _transmissions.Length;
            RefreshUI();
        }

        /// <summary>Кнопка ▶ у привода: следующий тип привода.</summary>
        public void CycleDrivetrain()
        {
            _drivetrainIndex = (_drivetrainIndex + 1) % _drivetrains.Length;
            RefreshUI();
        }

        /// <summary>Кнопка «К старту»: фиксирует сетап и запускает отсчёт/спавн.</summary>
        public void Confirm()
        {
            if (!_isOpen)
                return;

            // Записываем выбор — RaceCarSetupApplier на заспавненной машине
            // подхватит его в своём Start (спавн будет уже после отсчёта).
            RaceSetupSelection.Set(_transmissions[_transmissionIndex], _drivetrains[_drivetrainIndex]);

            HookFinishUnlock();

            if (_pauseWhileOpen)
                Time.timeScale = 1f;
            if (_menuRoot != null)
                _menuRoot.SetActive(false);
            _isOpen = false;

            if (_pendingZone != null)
                _pendingZone.StartCountdown();
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

        private void SetDemoTuningLocked(bool locked)
        {
            if (_demoManager == null)
                _demoManager = FindAnyObjectByType<DemoManager>();
            if (_demoManager != null)
                _demoManager.SetTuningLocked(locked);
        }

        private void RefreshUI()
        {
            if (_transmissionLabel != null)
                _transmissionLabel.text = Name(_transmissions[_transmissionIndex]);
            if (_drivetrainLabel != null)
                _drivetrainLabel.text = _drivetrains[_drivetrainIndex].ToString();
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
