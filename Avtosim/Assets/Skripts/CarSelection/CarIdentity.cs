using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Вешается на сцен-объект машины (городская машина в MVP, превью в меню) и
    /// объявляет, какой машине из CarDatabase он соответствует — по индексу.
    /// Благодаря этому порядок массивов (_cars у PlayerCarSelector, _carPreviews
    /// у меню) больше не важен: каждый объект сам знает свой индекс, и рассинхрон
    /// выбора становится невозможен.
    /// </summary>
    [AddComponentMenu("CustomVehicleController/CarSelection/Car Identity")]
    public class CarIdentity : MonoBehaviour
    {
        [Tooltip("Индекс этой машины в CarDatabase (0,1,2...). Должен совпадать с записью в каталоге.")]
        [SerializeField] private int _carIndex;

        public int CarIndex => _carIndex;
    }
}
