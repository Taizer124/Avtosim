using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Единая точка поиска активного игрока в сцене. Заменяет разбросанные по
    /// скриптам вызовы FindGameObjectWithTag("Player") / FindAnyObjectByType,
    /// чтобы вся логика "кто сейчас игрок" была в одном месте и одинаковая.
    ///
    /// Приоритет — объект с тегом Player (его проставляет PlayerCarSelector на
    /// выбранную машину). Fallback на FindAnyObjectByType нужен для сцен, где
    /// селектора нет (демо/тесты) — там просто берётся единственная машина.
    /// Результат кэшируется и перепроверяется при null/Destroyed, чтобы не
    /// вызывать поиск каждый кадр у каждого потребителя.
    /// </summary>
    public static class PlayerLocator
    {
        private static CustomVehicleController _cached;

        /// <summary>
        /// Возвращает CVC активного игрока или null, если в сцене нет ни одной
        /// машины. null-безопасно: потребитель обязан проверять результат.
        /// </summary>
        public static CustomVehicleController GetActivePlayer()
        {
            // Unity-объект после Destroy сравнивается с null как true —
            // поэтому обычная проверка _cached != null валидна и ловит
            // уничтоженную/выключенную при пересоздании сцены машину.
            if (_cached != null)
                return _cached;

            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null)
            {
                var cvc = tagged.GetComponent<CustomVehicleController>();
                if (cvc == null)
                    cvc = tagged.GetComponentInChildren<CustomVehicleController>();

                if (cvc != null)
                {
                    _cached = cvc;
                    return _cached;
                }
            }

            // Тега нет (демо-сцена без селектора) — берём любую машину.
            _cached = Object.FindAnyObjectByType<CustomVehicleController>();
            return _cached;
        }

        /// <summary>
        /// Сбросить кэш — вызывать, если игрок сменился в рантайме (например
        /// после переспавна). При обычном флоу выбора машины не требуется:
        /// селектор отрабатывает один раз в Awake до первого GetActivePlayer.
        /// </summary>
        public static void Invalidate() => _cached = null;
    }
}
