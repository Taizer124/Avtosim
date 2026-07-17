using System.Collections;
using UnityEngine;
using TMPro;

namespace Assets.VehicleController
{
    /// <summary>
    /// Всплывающее уведомление о начислении монет ("+500 монет"). Показывается
    /// с плавным появлением и авто-скрытием. Объект должен оставаться
    /// АКТИВНЫМ (иначе корутина не запустится) — прячется через прозрачность
    /// CanvasGroup, а не SetActive.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/Coin Reward Notification")]
    public class CoinRewardNotification : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TMP_Text _label;
        [Tooltip("{0} — сумма. Напр.: \"+{0} монет\"")]
        [SerializeField] private string _format = "+{0} монет";
        [SerializeField] private float _visibleTime = 2.5f;
        [SerializeField] private float _fadeTime = 0.4f;

        private Coroutine _routine;

        private void Awake()
        {
            if (_group == null)
                _group = GetComponent<CanvasGroup>();
            if (_group != null)
                _group.alpha = 0f;
        }

        /// <summary>Показать уведомление о начислении amount монет.</summary>
        public void Show(int amount)
        {
            if (_label != null)
                _label.text = string.Format(_format, amount);

            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            yield return Fade(0f, 1f);
            yield return new WaitForSeconds(_visibleTime);
            yield return Fade(1f, 0f);
            _routine = null;
        }

        private IEnumerator Fade(float from, float to)
        {
            if (_group == null)
                yield break;

            float t = 0f;
            while (t < _fadeTime)
            {
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(from, to, t / _fadeTime);
                yield return null;
            }
            _group.alpha = to;
        }
    }
}
