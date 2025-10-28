using Assets.VehicleController;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class CustomVehicleControllerEditor : EditorWindow
    {
        public static CustomVehicleControllerEditor Instance;

        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        public event Action OnWindowClosed;

        private Label _controllerSelectedLabel;
        private Toggle _saveChangedToggle;
        private Toggle _lockWindowToggle;
        private Toggle _usePresetsToggle;
        private VisualElement _usePreserHolder;

        private List<Button> _helpButtonList;
        private UnityEngine.Color _buttonMouseOverColor = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f);
        private UnityEngine.Color _buttonMouseExitColor = new Color(1f, 1f, 1f, 1f);

        private Button _docsLinkButton;
        private UnityEngine.Color _docsMouseOverColor = new Color(100 / 255f, 200 / 255f, 255 / 255f);
        private UnityEngine.Color _docsMouseExitColor = new Color(0, 165 / 255f, 240 / 255f);

        private const string CONTROLLER_SELECTED_LABEL = "ControllerSelectionLabel";
        private const string SAVE_CHANGES_TOGGLE_NAME = "SaveChangesToggle";
        private const string LOCK_WINDOW_TOGGLE = "LockWindowToggle";
        private const string DOCS_LINK_BUTTON = "DocumentationLinkButton";
        private const string USE_PRESETS_TOGGLE = "UsePresetToggle";
        private const string USE_PRESET_HOLDER = "UsePresetFooter";

        private CustomVehicleController _controller;

        private SerializedObject _serializedController;
        private SerializedObject _serializedCarVisuals;

        private VehiclePartsPresetSO _vehiclePartsPlayMode;
        private int _suspensionRaycastNumPlayMode;
        private Vector3 _comPlayMode;
        private TransmissionType _transTypePlayMode;

        #region Parts Editors
        private ControllerPresetSettingsEditor _presetEditor;
        private ControllerTransmissionSettingsEditor _transmissionSettingsEditor;
        private ControllerEngineSettingsEditor _engineSettingsEditor;
        private ControllerNitrousSettingsEditor _nitrousSettingsEditor;
        private ControllerForcedInductionSettingsEditor _fiSettingEditor;
        private ControllerSuspensionSettingsEditor _suspensionSettingsEditor;
        private ControllerBodySettingsEditor _bodySettingsEditor;
        private ControllerTiresSettingsEditor _tiresSettingsEditor;
        private ControllerBrakesSettingsEditor _brakesSettingsEditor;
        private ControllerSteeringSettingsEditor _steeringSettingsEditor;
        private ControllerInitializerEditor _initializerEditor;

        private ControllerExtraVisualsSettingsEditor _extraVisualsSettingsEditor;
        #endregion

        [MenuItem("Tools/CustomVehicleController/Vehicle Editor")]
        public static void OpenWindow()
        {
            CustomVehicleControllerEditor wnd = GetWindow<CustomVehicleControllerEditor>();
            wnd.titleContent = new GUIContent("Vehicle Editor");
        }

        public void CreateGUI()
        {
            if (Instance == null)
                Initialize();
        }

        private void Initialize()
        {
            VisualElement root = rootVisualElement;
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            _controllerSelectedLabel = root.Q<Label>(CONTROLLER_SELECTED_LABEL);

            _lockWindowToggle = root.Q<Toggle>(LOCK_WINDOW_TOGGLE);

            _saveChangedToggle = root.Q<Toggle>(SAVE_CHANGES_TOGGLE_NAME);

            _usePresetsToggle = root.Q<Toggle>(USE_PRESETS_TOGGLE);
            _usePresetsToggle.RegisterValueChangedCallback(evt => { 
                DisplayPresetVisualElements(_usePresetsToggle.value);
                SetVehicleControllerToSettingEditors();
            });
            _usePresetsToggle.bindingPath = nameof(CustomVehicleController.UsePreset);

            _usePreserHolder = root.Q<VisualElement>(USE_PRESET_HOLDER);

            _presetEditor = new(root, this);
            _transmissionSettingsEditor = new (root, this);
            _fiSettingEditor = new (root, this);
            _engineSettingsEditor = new (root, this);
            _nitrousSettingsEditor = new (root, this);
            _suspensionSettingsEditor = new (root, this);
            _bodySettingsEditor = new (root, this);
            _tiresSettingsEditor = new (root, this);
            _brakesSettingsEditor = new (root, this);
            _steeringSettingsEditor = new (root, this);
            _initializerEditor = new (root, this);
            _extraVisualsSettingsEditor = new (root, this);

            Instance = this;

            BindController(TryGetVehicleController());
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;

            CreateDocsLink();
            CreateLinksToDocs();
            Undo.undoRedoPerformed += UpdateAfterUndo;
        }

        private void UpdateAfterUndo()
        {
            if (_lockWindowToggle.value && _controller != null)
                return;

            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<CustomVehicleController>() != null)
                return;

            _initializerEditor.SetVehicleController(null);
            _controllerSelectedLabel.text = "CURRENTLY SELECTED VEHICLE CONTROLLER: NONE";
            _controllerSelectedLabel.style.color = Color.red;
        }

        private void CreateDocsLink()
        {
            _docsLinkButton = rootVisualElement.Q<Button>(DOCS_LINK_BUTTON);
            _docsLinkButton.clicked += CustomVehicleControllerEditor_onClick;
            _docsLinkButton.RegisterCallback<MouseOverEvent>(evt => { _docsLinkButton.style.color = _docsMouseOverColor; });
            _docsLinkButton.RegisterCallback<MouseOutEvent>(evt => { _docsLinkButton.style.color = _docsMouseExitColor; });
        }

        private void CreateLinksToDocs()
        {
            _helpButtonList = new List<Button>()
            {
                rootVisualElement.Q<Button>("EngineHelpButton"),
                rootVisualElement.Q<Button>("FIHelpButton"),
                rootVisualElement.Q<Button>("TransmissionHelpButton"),
                rootVisualElement.Q<Button>("NitrousHelpButton"),
                rootVisualElement.Q<Button>("WheelHelpButton"),
                rootVisualElement.Q<Button>("SuspHelpButton"),
                rootVisualElement.Q<Button>("BrakesHelpButton"),
                rootVisualElement.Q<Button>("BodyHelpButton")
            };

            for(int i = 0; i < _helpButtonList.Count; i++)
            {
                _helpButtonList[i].RegisterCallback<MouseOverEvent>(evt => { _helpButtonList.Find(x => evt.target == x).style.unityBackgroundImageTintColor = _buttonMouseOverColor; });
                _helpButtonList[i].RegisterCallback<MouseOutEvent>(evt => { _helpButtonList.Find(x => evt.target == x).style.unityBackgroundImageTintColor = _buttonMouseExitColor; });
            }

            //engine
            _helpButtonList[0].clicked += () => { Application.OpenURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/workflow/modifying-parts#torque-curve"); };
            //forced induction
            _helpButtonList[1].clicked += () => { Application.OpenURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/workflow/modifying-parts#forced-induction-type"); };
            //transmission
            _helpButtonList[2].clicked += () => { Application.OpenURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/workflow/modifying-parts#gear-ratios"); };
            //nitrous
            _helpButtonList[3].clicked += () => { Application.OpenURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/workflow/modifying-parts#boost-amount"); };
            //tires
            _helpButtonList[4].clicked += () => { Application.OpenURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/workflow/modifying-parts#steering-stiffness"); };
            //suspension
            _helpButtonList[5].clicked += () => { Application.OpenURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/workflow/modifying-parts#suspension-stiffness"); };
            //brakes
            _helpButtonList[6].clicked += () => { Application.OpenURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/workflow/modifying-parts#brakes-strength"); };
            //body
            _helpButtonList[7].clicked += () => { Application.OpenURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/workflow/modifying-parts#mass"); };
        }

        private void CustomVehicleControllerEditor_onClick() => Application.OpenURL("https://distubredone322.gitbook.io/custom-vehicle-controller/");

        private void OnDestroy()
        {
            OnWindowClosed?.Invoke();
            EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
            if(_docsLinkButton != null)
                _docsLinkButton.clicked -= CustomVehicleControllerEditor_onClick;

            Undo.undoRedoPerformed -= UpdateAfterUndo;
        }

        private void EditorApplication_playModeStateChanged(PlayModeStateChange newState)
        {
            if (newState == PlayModeStateChange.ExitingPlayMode)
            {
                if (!_saveChangedToggle.value)
                    return;

                CopyStats();
            }

            if (newState == PlayModeStateChange.EnteredEditMode)
            {
                if (_saveChangedToggle.value)
                {
                    PasteStats();
                    SaveController();
                }
                BindController(TryGetVehicleController());
            }
        }

        private void CopyStats()
        {
            if (_serializedController == null || _serializedController.targetObject == null || _serializedController.targetObjects.Length == 0)
                return;
            _serializedController.Update();

            _extraVisualsSettingsEditor.CopyStats(_serializedController);
            _bodySettingsEditor.CopyStats(_serializedController);
            _steeringSettingsEditor.CopyStats(_serializedController);

            _suspensionRaycastNumPlayMode = _serializedController.FindProperty("_suspensionSimulationPrecision").intValue;
            _comPlayMode = (_serializedController.FindProperty("_centerOfMass").objectReferenceValue as Transform).localPosition;
            _transTypePlayMode = (TransmissionType)_serializedController.FindProperty(nameof(CustomVehicleController.TransmissionType)).intValue;


            if (!_saveChangedToggle.value)
                return;

            if (!_usePresetsToggle.value)
            {
                _vehiclePartsPlayMode = ScriptableObject.CreateInstance<VehiclePartsPresetSO>();
                SerializedProperty customPartsSet = _serializedController.FindProperty("_customizableSet");
                _vehiclePartsPlayMode.Engine = customPartsSet.FindPropertyRelative("Engine").objectReferenceValue as EngineSO;
                _vehiclePartsPlayMode.Nitrous = customPartsSet.FindPropertyRelative("Nitrous").objectReferenceValue as NitrousSO;
                _vehiclePartsPlayMode.Transmission = customPartsSet.FindPropertyRelative("Transmission").objectReferenceValue as TransmissionSO;
                _vehiclePartsPlayMode.FrontSuspension = customPartsSet.FindPropertyRelative("FrontSuspension").objectReferenceValue as SuspensionSO;
                _vehiclePartsPlayMode.RearSuspension = customPartsSet.FindPropertyRelative("RearSuspension").objectReferenceValue as SuspensionSO;
                _vehiclePartsPlayMode.FrontTires = customPartsSet.FindPropertyRelative("FrontTires").objectReferenceValue as TiresSO;
                _vehiclePartsPlayMode.RearTires = customPartsSet.FindPropertyRelative("RearTires").objectReferenceValue as TiresSO;
                _vehiclePartsPlayMode.Brakes = customPartsSet.FindPropertyRelative("Brakes").objectReferenceValue as BrakesSO;
                _vehiclePartsPlayMode.Body = customPartsSet.FindPropertyRelative("Body").objectReferenceValue as VehicleBodySO;
            }
            else
                _vehiclePartsPlayMode = _serializedController.FindProperty("_vehiclePartsPreset").objectReferenceValue as VehiclePartsPresetSO;
        }

        private void PasteStats()
        {

            if (_serializedController == null || _serializedController.targetObject == null)
                return;

            _serializedController.Update();

            _extraVisualsSettingsEditor.PasteStats(_serializedController);
            _bodySettingsEditor.PasteStats(_serializedController);
            _steeringSettingsEditor.PasteStats(_serializedController);
            _serializedController.FindProperty("_suspensionSimulationPrecision").intValue = _suspensionRaycastNumPlayMode;
            (_serializedController.FindProperty("_centerOfMass").objectReferenceValue as Transform).localPosition = _comPlayMode;
            _serializedController.FindProperty(nameof(CustomVehicleController.TransmissionType)).intValue = (int)_transTypePlayMode;

            UnbindEditors();
            if(_saveChangedToggle.value)
            {
                if (!_usePresetsToggle.value)
                {
                    SerializedProperty customPartsSet = _serializedController.FindProperty("_customizableSet");
                    customPartsSet.FindPropertyRelative("Engine").objectReferenceValue = _vehiclePartsPlayMode.Engine;
                    customPartsSet.FindPropertyRelative("Transmission").objectReferenceValue = _vehiclePartsPlayMode.Transmission;
                    customPartsSet.FindPropertyRelative("Nitrous").objectReferenceValue = _vehiclePartsPlayMode.Nitrous;
                    customPartsSet.FindPropertyRelative("FrontSuspension").objectReferenceValue = _vehiclePartsPlayMode.FrontSuspension;
                    customPartsSet.FindPropertyRelative("RearSuspension").objectReferenceValue = _vehiclePartsPlayMode.RearSuspension;
                    customPartsSet.FindPropertyRelative("FrontTires").objectReferenceValue = _vehiclePartsPlayMode.FrontTires;
                    customPartsSet.FindPropertyRelative("RearTires").objectReferenceValue = _vehiclePartsPlayMode.RearTires;
                    customPartsSet.FindPropertyRelative("Brakes").objectReferenceValue = _vehiclePartsPlayMode.Brakes;
                    customPartsSet.FindPropertyRelative("Body").objectReferenceValue = _vehiclePartsPlayMode.Body;
                }
                else
                    _serializedController.FindProperty("_vehiclePartsPreset").objectReferenceValue = _vehiclePartsPlayMode;
            }

            SaveController();
            SetVehicleControllerToSettingEditors();
        }

        private void OnSelectionChange()
        {
            if (_lockWindowToggle.value)
                return;
                
            BindController(TryGetVehicleController());
        }

        private void OnBecameVisible()
        {
            if (Instance == null)
                Initialize();

            if (_lockWindowToggle == null || _lockWindowToggle.value)
                return;

            BindController(TryGetVehicleController());
        }

        public void RequestUpdate() => BindController(TryGetVehicleController());

        private CustomVehicleController TryGetVehicleController()
        {
            if (Selection.activeGameObject != null)
            {
                if(Selection.activeGameObject.TryGetComponent(out _controller) || 
                    Selection.activeGameObject.transform.root.TryGetComponent(out _controller))
                {
                    if (Selection.objects.Length > 1)
                    {
                        Debug.Log("Multiobject editing isn't supported");
                    }

                    _serializedController = new SerializedObject(_controller);
                    _usePresetsToggle.Bind(_serializedController);
                    _serializedCarVisuals = new SerializedObject(_controller.GetComponent<CarVisualsEssentials>());

                    _usePreserHolder.style.display = DisplayStyle.Flex;

                    return _controller;
                }
            }

            _usePresetsToggle.Unbind();
            _controller = null;
            _usePresetsToggle.value = false;

            _usePreserHolder.style.display = DisplayStyle.None;

            return _controller;
        }

        public SerializedObject GetSerializedController() => _serializedController;
       
        public SerializedObject GetSerializedCarVisuals() => _serializedCarVisuals;

        public CustomVehicleController GetController() => _controller;

        public void BindEditorPartFields(VehiclePartsPresetSO partsSO)
        {
            if (partsSO == null)
                return;

            SerializedObject serializedPreset = new SerializedObject(partsSO);

            _engineSettingsEditor.BindPreset(serializedPreset);
            _fiSettingEditor.BindPreset(serializedPreset);
            _nitrousSettingsEditor.BindPreset(serializedPreset);
            _transmissionSettingsEditor.BindPreset(serializedPreset);
            _suspensionSettingsEditor.BindPreset(serializedPreset);
            _bodySettingsEditor.BindPreset(_serializedController, serializedPreset);
            _tiresSettingsEditor.BindPreset(serializedPreset);
            _brakesSettingsEditor.BindPreset(serializedPreset);
        }

        public void SaveController()
        {
            if (_serializedController == null || _serializedController.targetObject == null || _serializedController.targetObjects.Length == 0)
                return;
            _serializedController.ApplyModifiedProperties();
            _serializedController.Update();
        }

        private void BindController(CustomVehicleController controller)
        {
            if(controller == null)
                UnbindEditors();
            else
                SetVehicleControllerToSettingEditors();

            _initializerEditor.SetVehicleController(controller);

            if (controller != null)
            {
                _controllerSelectedLabel.text = "CURRENTLY SELECTED VEHICLE CONTROLLER: " + controller.name.ToUpper();
                _controllerSelectedLabel.style.color = Color.green;
                return;
            }

            _controllerSelectedLabel.text = "CURRENTLY SELECTED VEHICLE CONTROLLER: NONE";
            _controllerSelectedLabel.style.color = Color.red;
        }
        private void SetVehicleControllerToSettingEditors()
        {
            if(_usePresetsToggle.value)
                _presetEditor.BindVehicleController(_serializedController);
            else
                BindPartsToController();

            _steeringSettingsEditor.BindVehicleController(_serializedController);

            _extraVisualsSettingsEditor.SetVehicleController(_serializedController);
        }

        private void BindPartsToController()
        {
            if (_serializedController == null)
                return;
            if (_serializedController.FindProperty("_customizableSet") == null)
                return;

            _engineSettingsEditor.BindVehicleController(_serializedController.FindProperty("_customizableSet").FindPropertyRelative("Engine"));
            _fiSettingEditor.BindVehicleController(_serializedController.FindProperty("_customizableSet").FindPropertyRelative("ForcedInduction"));
            _nitrousSettingsEditor.BindVehicleController(_serializedController.FindProperty("_customizableSet").FindPropertyRelative("Nitrous"));
            _transmissionSettingsEditor.BindVehicleController(_serializedController.FindProperty("_customizableSet").FindPropertyRelative("Transmission"));
            _suspensionSettingsEditor.BindVehicleController(_serializedController.FindProperty("_customizableSet").FindPropertyRelative("FrontSuspension"),
                                                           _serializedController.FindProperty("_customizableSet").FindPropertyRelative("RearSuspension"));
            _bodySettingsEditor.BindVehicleController(_serializedController, _serializedController.FindProperty("_customizableSet").FindPropertyRelative("Body"));
            _tiresSettingsEditor.BindVehicleController(_serializedController.FindProperty("_customizableSet").FindPropertyRelative("FrontTires"),
                                                      _serializedController.FindProperty("_customizableSet").FindPropertyRelative("RearTires"));
            _brakesSettingsEditor.BindVehicleController(_serializedController.FindProperty("_customizableSet").FindPropertyRelative("Brakes"));
        }

        private void UnbindEditors()
        {
            _steeringSettingsEditor.Unbind();

            if (_usePresetsToggle.value)
                _presetEditor.Unbind();
            else
                UnbindPartsToController();
        }

        private void UnbindPartsToController()
        {
            _engineSettingsEditor.Unbind();
            _fiSettingEditor.Unbind();
            _nitrousSettingsEditor.Unbind();
            _transmissionSettingsEditor.Unbind();
            _suspensionSettingsEditor.Unbind();
            _bodySettingsEditor.Unbind();
            _tiresSettingsEditor.Unbind();
            _brakesSettingsEditor.Unbind();
        }

        private void DisplayPresetVisualElements(bool display)
        {
            rootVisualElement.Q<Foldout>("PresetSettingsFoldout").style.display = display? DisplayStyle.Flex : DisplayStyle.None;
            rootVisualElement.Q<VisualElement>("PresetLabel").style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
