using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Начисляет монеты за гонку и показывает уведомление ровно в момент, когда
    /// игрока после финиша возвращает в городскую машину. Вешается на объект в
    /// игровой сцене и подписывается на RaceFinishZone.OnTimerFinished — это
    /// событие срабатывает прямо перед ManageObjects()/EnableOldVehicle().
    ///
    /// Место игрока берётся из RaceManager (если он ведёт позиции), иначе
    /// используется _fallbackPlacement (по умолчанию 1 — считаем, что финиш =
    /// победа). Награда по месту задаётся в EconomyManager.
    ///
    /// ВАЖНО: используй ЛИБО этот компонент, ЛИБО RaceRewardGranter, но не оба
    /// сразу — иначе монеты начислятся дважды. Для триггерного финиша
    /// (RaceFinishZone) правильный именно этот.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/Race Finish Reward")]
    public class RaceFinishReward : MonoBehaviour
    {
        [SerializeField] private RaceFinishZone _finishZone;
        [SerializeField] private CoinRewardNotification _notification;

        [Tooltip("Если RaceManager не найден — какое место считать (1 = первое).")]
        [Min(1)]
        [SerializeField] private int _fallbackPlacement = 1;

        private bool _granted;

        private void Start()
        {
            if (_finishZone == null)
                _finishZone = FindAnyObjectByType<RaceFinishZone>();

            if (_finishZone != null)
                _finishZone.OnTimerFinished.AddListener(GrantReward);
            else
                Debug.LogWarning("[RaceFinishReward] RaceFinishZone не найдена — награда за гонку не начислится.");
        }

        private void OnDestroy()
        {
            if (_finishZone != null)
                _finishZone.OnTimerFinished.RemoveListener(GrantReward);
        }

        private void GrantReward()
        {
            if (_granted)
                return; // один раз за заезд
            _granted = true;

            if (EconomyManager.Instance == null)
            {
                Debug.LogWarning("[RaceFinishReward] EconomyManager отсутствует — награда не начислена.");
                return;
            }

            int placement = GetPlayerPlacement();

            // Считаем фактически начисленное по разнице баланса — чтобы
            // уведомление показало ровно ту сумму, что дала таблица наград.
            int before = EconomyManager.Instance.Coins;
            EconomyManager.Instance.AwardRacePlacement(placement);
            int awarded = EconomyManager.Instance.Coins - before;

            if (awarded > 0 && _notification != null)
                _notification.Show(awarded);

            Debug.Log($"[RaceFinishReward] Место {placement}, начислено {awarded} монет.");
        }

        private int GetPlayerPlacement()
        {
            RaceManager rm = RaceManager.Instance;
            if (rm != null)
            {
                var board = rm.GetLeaderboard();
                if (board != null)
                {
                    for (int i = 0; i < board.Count; i++)
                        if (board[i] != null && board[i].IsPlayer)
                            return i + 1;
                }
            }
            return _fallbackPlacement;
        }
    }
}
