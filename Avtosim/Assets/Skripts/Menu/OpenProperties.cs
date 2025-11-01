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

    [Header("Navigation")]
    [SerializeField] private WheelUINavigation wheelUINavigation;

    private Stack<GameObject> panelHistory = new Stack<GameObject>();

    private void Awake()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        settingsButton.onClick.AddListener(() => OpenPanel(settingsPanel));
        backButtonSettings.onClick.AddListener(GoBack);

        panelHistory.Push(mainMenuPanel);

        if (wheelUINavigation != null)
            wheelUINavigation.SetActivePanel(mainMenuPanel.transform);
    }

    private void OpenPanel(GameObject targetPanel)
    {
        panelHistory.Push(GetActivePanel());
        ShowOnlyPanel(targetPanel);

        if (wheelUINavigation != null)
            wheelUINavigation.SetActivePanel(targetPanel.transform);
    }

    private void GoBack()
    {
        if (panelHistory.Count > 1)
        {
            panelHistory.Pop();
            var previousPanel = panelHistory.Peek();
            ShowOnlyPanel(previousPanel);

            if (wheelUINavigation != null)
                wheelUINavigation.SetActivePanel(previousPanel.transform);
        }
        else
        {
            ShowOnlyPanel(mainMenuPanel);
            if (wheelUINavigation != null)
                wheelUINavigation.SetActivePanel(mainMenuPanel.transform);
        }
    }

    public void GoToMainMenu()
    {
        panelHistory.Clear();
        panelHistory.Push(mainMenuPanel);
        ShowOnlyPanel(mainMenuPanel);

        if (wheelUINavigation != null)
            wheelUINavigation.SetActivePanel(mainMenuPanel.transform);
    }

    private void ShowOnlyPanel(GameObject panelToShow)
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        panelToShow.SetActive(true);
    }

    private GameObject GetActivePanel()
    {
        if (settingsPanel.activeSelf) return settingsPanel;
        return mainMenuPanel;
    }
}
