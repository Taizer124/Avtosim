using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel = null;
    [SerializeField] private GameObject settingsPanel = null;

    [Header("Buttons")]
    [SerializeField] private Button settingsButton = null;

    [Header("Back Buttons")]
    [SerializeField] private Button backButtonSettings = null;

    private Stack<GameObject> panelHistory = new Stack<GameObject>();

    private void Awake()
    {
        // Главное меню всегда активно изначально
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        // Назначаем переходы на кнопки главного меню
        settingsButton.onClick.AddListener(() => OpenPanel(settingsPanel));

        // Назначаем обработчики на backButton
        backButtonSettings.onClick.AddListener(GoBack);

        // Добавляем главное меню в историю как начальную точку
        panelHistory.Push(mainMenuPanel);
    }

    private void OpenPanel(GameObject targetPanel)
    {
        // Добавляем текущую активную панель в историю
        panelHistory.Push(GetActivePanel());

        // Показываем целевую панель и скрываем остальные
        ShowOnlyPanel(targetPanel);
    }

    private void GoBack()
    {
        if (panelHistory.Count > 1) // Больше 1, потому что главное меню всегда там
        {
            // Убираем текущую панель из истории
            panelHistory.Pop();

            // Возвращаемся к предыдущей панели
            GameObject previousPanel = panelHistory.Peek();
            ShowOnlyPanel(previousPanel);
        }
        else
        {
            // Если в истории только главное меню, показываем его
            ShowOnlyPanel(mainMenuPanel);
        }
    }

    // Метод для принудительного возврата в главное меню
    public void GoToMainMenu()
    {
        // Очищаем историю и добавляем главное меню
        panelHistory.Clear();
        panelHistory.Push(mainMenuPanel);

        ShowOnlyPanel(mainMenuPanel);
    }

    private void ShowOnlyPanel(GameObject panelToShow)
    {
        // Скрываем все панели
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        // Показываем только нужную панель
        panelToShow.SetActive(true);
    }

    private GameObject GetActivePanel()
    {
        if (settingsPanel.activeSelf) return settingsPanel;
        return mainMenuPanel;
    }

    // Метод для получения текущей активной панели
    public GameObject GetCurrentPanel()
    {
        return GetActivePanel();
    }

    // Метод для проверки, активна ли главная панель
    public bool IsMainMenuActive()
    {
        return mainMenuPanel.activeSelf;
    }

    // Метод для принудительного открытия панели настроек
    public void OpenSettingsPanel()
    {
        OpenPanel(settingsPanel);
    }

    // Метод для принудительного закрытия всех панелей (кроме главной)
    public void CloseAllPanels()
    {
        ShowOnlyPanel(mainMenuPanel);
        panelHistory.Clear();
        panelHistory.Push(mainMenuPanel);
    }
}