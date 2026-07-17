using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Мост между гоночной системой и экономикой. Ставится в игровую сцену.
    /// Подписывается на RaceManager.OnPlayerFinished(place) и начисляет монеты
    /// через EconomyManager по занятому месту (1 = первое). Награды по местам
    /// настраиваются в EconomyManager, поэтому здесь только проброс события.
    ///
    /// Вынесено в отдельный компонент (а не в инспекторный UnityEvent), потому
    /// что EconomyManager — персистентный синглтон из другой сцены, и повесить
    /// ссылку на него в UnityEvent сцены нельзя.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/Race Reward Granter")]
    public class RaceRewardGranter : MonoBehaviour
    {
        [SerializeField] private RaceManager _raceManager;

        private void Start()
        {
            if (_raceManager == null)
                _raceManager = RaceManager.Instance;
            if (_raceManager == null)
                _raceManager = FindAnyObjectByType<RaceManager>();

            if (_raceManager != null)
                _raceManager.OnPlayerFinished.AddListener(HandlePlayerFinished);
            else
                Debug.LogWarning("[RaceRewardGranter] RaceManager не найден — награда за гонку не будет начислена.");
        }

        private void OnDestroy()
        {
            if (_raceManager != null)
                _raceManager.OnPlayerFinished.RemoveListener(HandlePlayerFinished);
        }

        private void HandlePlayerFinished(int placement)
        {
            if (EconomyManager.Instance != null)
                EconomyManager.Instance.AwardRacePlacement(placement);
            else
                Debug.LogWarning("[RaceRewardGranter] EconomyManager.Instance отсутствует — награда не начислена. Помести EconomyManager в стартовую сцену.");
        }
    }
}
