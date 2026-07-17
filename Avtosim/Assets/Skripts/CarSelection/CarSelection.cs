using UnityEngine;

namespace Assets.VehicleController
{
    /// <summary>
    /// Носитель выбора машины между сценой меню и игровой сценой.
    ///
    /// static-поле переживает загрузку сцены в рамках одной сессии (статики
    /// не сбрасываются при SceneManager.LoadScene), а зеркалирование в
    /// PlayerPrefs делает выбор устойчивым к domain reload в редакторе и к
    /// повторному запуску игры. Меню пишет сюда SelectedIndex перед загрузкой
    /// MVP, а PlayerCarSelector читает его в Awake.
    /// </summary>
    public static class CarSelection
    {
        private const string PREF_KEY = "SelectedCarIndex";

        // Кол-во выбираемых машин (player / player (1) / player (2)).
        // Держим здесь как единый источник для клампа и в меню, и в селекторе.
        public const int CarCount = 3;

        private static int _selectedIndex = -1;

        public static int SelectedIndex
        {
            get
            {
                // Первое обращение за сессию — поднимаем сохранённое значение
                // из PlayerPrefs (после перезапуска static обнулён в -1).
                if (_selectedIndex < 0)
                    _selectedIndex = Mathf.Clamp(PlayerPrefs.GetInt(PREF_KEY, 0), 0, CarCount - 1);

                return _selectedIndex;
            }
            set
            {
                _selectedIndex = Mathf.Clamp(value, 0, CarCount - 1);
                PlayerPrefs.SetInt(PREF_KEY, _selectedIndex);
                PlayerPrefs.Save();
            }
        }
    }
}
