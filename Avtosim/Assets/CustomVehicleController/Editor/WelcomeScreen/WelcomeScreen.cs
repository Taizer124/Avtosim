using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class WelcomeScreen : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [SerializeField]
        private TextAsset _textAsset;

        private const string DOCS_URL = "https://distubredone322.gitbook.io/custom-vehicle-controller";
        private const string YT_URL = "https://www.youtube.com/watch?v=qjfbUDkL3EU";
        private const string WEBSITE_URL = "https://sites.google.com/view/shirepact/home";
        private const string AI_DOCS_URL = "https://distubredone322.gitbook.io/custom-vehicle-controller/guides/ai-racers-setup";

        private UnityEngine.Color _defaultTextColor = new UnityEngine.Color(150 / 255f, 150 / 255f, 150 / 255f, 1f);
        private UnityEngine.Color _highlightTextColor = new UnityEngine.Color(225 / 255f, 225 / 255f, 225 / 255f, 1f);

        private const int DEFAULT_TEXT_SIZE = 16;
        private const int HIGHLIGHT_TEXT_SIZE = 17;

        private const int DEFAULT_ICON_SIZE = 30;
        private const int HIGHLIGHT_ICON_SIZE = 32;

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            VisualElement tree = m_VisualTreeAsset.Instantiate();

            root.Add(tree);
            FindElements();
        }

        private void FindElements()
        {
            SetUpClickableLink(rootVisualElement.Q<Button>("DocsButton"), DOCS_URL);
            SetUpClickableLink(rootVisualElement.Q<Button>("YTButton"), YT_URL);
            SetUpClickableLink(rootVisualElement.Q<Button>("WebsiteButton"), WEBSITE_URL);
            var button = rootVisualElement.Q<Button>("AIDocsLinkButton");
            button.clicked += () =>
            {
                Application.OpenURL(AI_DOCS_URL);
            };
            button.RegisterCallback<MouseEnterEvent>(evt => {
                button.style.color = _highlightTextColor;
            });

            button.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                button.style.color = _defaultTextColor;
            });
#if INPUT_SYSTEM_INSTALLED
#else
            DisplayInputSystemNotification();
#endif
        }

        private void DisplayInputSystemNotification()
        {
            rootVisualElement.Q<Label>("InputSystemNotInstalledLabel").style.display = DisplayStyle.Flex;
            var button = rootVisualElement.Q<Button>("InputSystemDocsLinkButton");
            button.clicked += () =>
            {
                Application.OpenURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/vehicle-controller-input-provider#version-1.1.2-added-input-provider-script-using-input-system");
            };
            button.RegisterCallback<MouseEnterEvent>(evt => {
                button.style.color = _highlightTextColor;
            });

            button.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                button.style.color = _defaultTextColor;
            });
        }

        private void SetUpClickableLink(Button button, string link)
        {
            button.clicked += () =>
            {
                Application.OpenURL(link);
            };

            button.RegisterCallback<MouseEnterEvent>(evt => {
                Label label = button.Q<Label>();
                label.style.color = _highlightTextColor;
                label.style.fontSize = HIGHLIGHT_TEXT_SIZE;

                VisualElement icon = button.Q<VisualElement>("Icon");
                icon.style.width = icon.style.height = HIGHLIGHT_ICON_SIZE;
            });

            button.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                Label label = button.Q<Label>();
                label.style.color = _defaultTextColor;
                label.style.fontSize = DEFAULT_TEXT_SIZE;

                VisualElement icon = button.Q<VisualElement>("Icon");
                icon.style.width = icon.style.height = DEFAULT_ICON_SIZE;
            });
        }

        [MenuItem("Tools/CustomVehicleController/Welcome Window")]
        public static void OpenWindow()
        {
            WelcomeScreen wnd = GetWindow<WelcomeScreen>();
            wnd.maxSize = new Vector2(600, 300);
            wnd.minSize = new Vector2(600, 300);
            wnd.titleContent = new GUIContent("Welcome Window");
        }


        public static void DisplayWelcomeWindow()
        {
            WelcomeScreen wnd = GetWindow<WelcomeScreen>();
            wnd.maxSize = new Vector2(600, 300);
            wnd.minSize = new Vector2(600, 300);
            wnd.titleContent = new GUIContent("Welcome Window");

            // Check if the welcome screen has already been shown
            if (!wnd.AlreadyShowedWelcomeScreen())
            {
                wnd.OverwriteLine(0, "true");
                wnd.Show();
            }
            else
            {
                wnd.Close();
            }
        }

        private bool AlreadyShowedWelcomeScreen()
        {
            return ReadTextFileAtLine(0);
        }

        bool ReadTextFileAtLine(int line)
        {
            string filePath = AssetDatabase.GetAssetPath(_textAsset);

            try
            {
                // Read all lines from the text file
                string[] lines = File.ReadAllLines(filePath);

                if (lines.Length >= line)
                {
                    // Parse the first and second lines as boolean values
                    return bool.Parse(lines[line]);
                }
                else
                {
                    //write line
                    lines = new string[1];
                    lines[1] = "false";
                    File.WriteAllLines(filePath, lines);
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }


        void OverwriteLine(int lineNumber, string newText)
        {
            try
            {
                // Read all lines from the text file
                string filePath = AssetDatabase.GetAssetPath(_textAsset);
                string[] lines = File.ReadAllLines(filePath);

                if (lineNumber >= 0 && lineNumber < lines.Length)
                {
                    // Overwrite the specified line with the new text
                    lines[lineNumber] = newText;

                    // Write all lines back to the text file
                    File.WriteAllLines(filePath, lines);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error overwriting text file: " + e.Message);
            }
        }
    }

}
