using System;
using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Единый источник данных о машинах (ScriptableObject-ассет). Порядок
    /// записей = канонический индекс машины (0,1,2...). Всё остальное
    /// (EconomyManager, RacePlayerCarBinder, меню) читает отсюда, а сцен-объекты
    /// (городские машины, превью в меню) объявляют свой индекс компонентом
    /// CarIdentity — поэтому порядок массивов в инспекторе больше не может
    /// разъехаться.
    ///
    /// Создать ассет: правый клик в Project → Create → CustomVehicleController →
    /// Car Database.
    /// </summary>
    [CreateAssetMenu(fileName = "CarDatabase", menuName = "CustomVehicleController/Car Database")]
    public class CarDatabase : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string DisplayName = "Car";
            [TextArea] public string Description = "";
            [Min(0)] public int Price = 0;
            [Tooltip("Доступна с начала бесплатно (например стартовая машина).")]
            public bool OwnedByDefault = false;
            [Tooltip("Префаб, который спавнится в гонке для этой машины (RaceParticipant вкл, _isPlayer=1, тег Player).")]
            public GameObject RacePrefab;
        }

        [SerializeField] private Entry[] _cars;

        public int Count => _cars != null ? _cars.Length : 0;

        public Entry Get(int index) => IsValid(index) ? _cars[index] : null;

        public string GetName(int index) => IsValid(index) ? _cars[index].DisplayName : "";
        public string GetDescription(int index) => IsValid(index) ? _cars[index].Description : "";
        public int GetPrice(int index) => IsValid(index) ? _cars[index].Price : 0;
        public bool IsOwnedByDefault(int index) => IsValid(index) && _cars[index].OwnedByDefault;
        public GameObject GetRacePrefab(int index) => IsValid(index) ? _cars[index].RacePrefab : null;

        private bool IsValid(int index) => _cars != null && index >= 0 && index < _cars.Length;
    }
}
