using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using Assets.VehicleController;

namespace Assets.VehicleControllerEditor
{
    public class EnginePartCreatorWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private const string ENGINE_PART_TYPES_LIST_NAME = "EnginePartsIsProjectList";
        private const string DOCS_LINK_BUTTON_NAME = "DocsLinkButton";

        private ListView _enginePartTypesList;

        public const string ENGINE_PARTS_FOLDER_NAME = "EngineParts";

        private List<Type> _enginePartTypeList;
        private List<string> _enginePartTypeNameList;

        private EnginePartSettingsEditor _enginePartSettingsEditor;
        private NewEnginePartTypeCreatorEditor _newEnginePartTypeCreatorEditor;
        private EnginePartContainerEditor _enginePartContainerEditor;

        private UnityEngine.Color _docsMouseOverColor = new Color(100 / 255f, 200 / 255f, 255 / 255f);
        private UnityEngine.Color _docsMouseExitColor = new Color(0, 165 / 255f, 240 / 255f);

        [MenuItem("Tools/CustomVehicleController/Engine Part Creator Window")]
        public static void OpenWindow()
        {
            EnginePartCreatorWindow wnd = GetWindow<EnginePartCreatorWindow>();
            wnd.titleContent = new GUIContent("Engine Part Creator Window");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            VisualElement tree = m_VisualTreeAsset.Instantiate();
            root.Add(tree);

            Initialize();
            TryBindContainer();
        }

        public Type GetTypeFromName(string value)
        {
            return _enginePartTypeList.Find(x => x.Name == value);
        }

        private void Initialize()
        {
            CreateClassInstances();
            FindElements();
            FindEnginePartTypes();
            Button button = rootVisualElement.Q<Button>(DOCS_LINK_BUTTON_NAME);

            button.clicked += () => {
                Application.OpenURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/workflow/engine-performance-customization");
            };
            Label label = rootVisualElement.Q<Label>("DocsLabel");
            button.RegisterCallback<MouseOverEvent>(evt => { label.style.color = _docsMouseOverColor; });
            button.RegisterCallback<MouseOutEvent>(evt => { label.style.color = _docsMouseExitColor; });
        }

        private void CreateClassInstances()
        {
            _enginePartSettingsEditor = new(rootVisualElement, this);
            _newEnginePartTypeCreatorEditor = new(rootVisualElement, this);
            _enginePartContainerEditor = new(rootVisualElement, this);
        }

        private void FindElements()
        {
            _enginePartTypesList = rootVisualElement.Q<ListView>(ENGINE_PART_TYPES_LIST_NAME);
        }

        private void OnSelectionChange()
        {
            TryBindContainer();
        }

        private void TryBindContainer()
        {
            if (_enginePartContainerEditor == null)
                CreateClassInstances();
            if (Selection.activeGameObject == null)
            {
                _enginePartContainerEditor.BindController(null);
                return;
            }

            _enginePartContainerEditor.BindController(Selection.activeGameObject.GetComponent<CustomVehicleController>());
        }

        public List<string> GetPartTypeChoices() => new List<string>(_enginePartTypeNameList);

        public void UpdatePartTypes() => FindEnginePartTypes();

        private void FindEnginePartTypes()
        {
            _enginePartTypeList = new();

            // Get all types in the assembly
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                // Filter types that implement IEnginePart interface
                Type[] types = assembly.GetTypes()
                    .Where(t => typeof(CustomEnginePart).IsAssignableFrom(t) && t != typeof(CustomEnginePart))
                    .ToArray();
                _enginePartTypeList.AddRange(types);
            }
            _enginePartTypeNameList = new();
            foreach (var type in _enginePartTypeList)
            {
                _enginePartTypeNameList.Add(type.Name);
            }

            FillTypeFields();
        }

        private void FillTypeFields()
        { 
            _enginePartTypesList.itemsSource = _enginePartTypeNameList;

            _newEnginePartTypeCreatorEditor.ResetChoices(_enginePartTypeNameList);
            _enginePartSettingsEditor.ResetChoices(_enginePartTypeNameList);
        }
    }
}
