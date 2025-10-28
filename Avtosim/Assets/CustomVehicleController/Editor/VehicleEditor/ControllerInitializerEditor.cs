using Assets.VehicleController;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class ControllerInitializerEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        private Foldout _foldout;

        private VisualElement _addComponentMenu;
        private Toggle _addColliderToggle;


        private ObjectField _frontLeftWheelObjField;
        private ObjectField _frontRightWheelObjField;
        private ObjectField _rearLeftWheelObjField;
        private ObjectField _rearRightWheelObjField;

        private ObjectField _frontLeftBrakesObjField;
        private ObjectField _frontRightBrakesObjField;
        private ObjectField _rearLeftBrakesObjField;
        private ObjectField _rearRightBrakesObjField;

        private ObjectField _bodyMeshField;

        private Label _notInitializedMessageLabel;

        private const string FOLDOUT_NAME = "DrivetrainFoldout";

        private const string ADD_COMPONENT_MENU_NAME = "AddComponentMenu";
        private const string ADD_COLLIDER_TOGGLE_NAME = "AddColliderToggle";
        private const string ADD_COMPONENT_BUTTON_NAME = "AddComponentButton";

        private const string FRONT_LEFT_WHEEL_FIELD_NAME = "FrontLeftWheelObjField";
        private const string FRONT_LEFT_BRAKES_FIELD_NAME = "FrontLeftBrakesObjField";
        private const string FRONT_RIGHT_WHEEL_FIELD_NAME = "FrontRightWheelObjField";
        private const string FRONT_RIGHT_BRAKES_FIELD_NAME = "FrontRightBrakesObjField";
        private const string REAR_LEFT_WHEEL_FIELD_NAME = "RearLeftWheelObjField";
        private const string REAR_LEFT_BRAKES_FIELD_NAME = "RearLeftBrakesObjField";
        private const string REAR_RIGHT_WHEEL_FIELD_NAME = "RearRightWheelObjField";
        private const string REAR_RIGHT_BRAKES_FIELD_NAME = "RearRightBrakesObjField";
        private const string BODY_FIELD_NAME = "BodyField";
        private const string INIT_BUTTON_NAME = "InitializeController";
        private const string NOT_INITIALIZED_LABEL_NAME = "ControllerNotInitializedLabel";

        #region prefab unpack
        private VisualElement _warningWindow;
        private Button _closeWindowsButton;
        private Button _confirmButton;
        private Button _denyButton;

        private const string WINDOW_NAME = "WarningWindowWrapper";
        private const string CONFIRM_BUTTON_NAME = "ConfirmButton";
        private const string DENY_BUTTON_NAME = "DenyButton";
        private const string CLOSE_WINDOWS_BUTTON_NAME = "CloseWindowButton";
        #endregion

        public ControllerInitializerEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;
            FindAddComponentMenuFields();
            FindDrivetrainFields();
            FindWarningWindowFields();
            SubscribeToInitializeButtonClickEvent();

            _mainEditor.OnWindowClosed += _editor_OnWindowClosed;
            SetTooltips();
        }


        private void SetTooltips()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Drag and drop appropriate game objects which represent wheels into transform fields, ideally the ones with MeshRenderer components (this will allow automatic wheel radius and suspension position calculation).");
            sb.AppendLine();
            sb.AppendLine("This button will create a hierarchy of game objects, add needed scripts, populate scripts with appropriate references, and position game objects at correct location if mesh renderer is provided.");
            sb.AppendLine("");
            sb.AppendLine("Since hierarchy will be changed, the root game object can't be a prefab.");
            root.Q<Button>(INIT_BUTTON_NAME).tooltip = sb.ToString();
        }

        private void _editor_OnWindowClosed()
        {
            Button initializeButton = root.Q<Button>(INIT_BUTTON_NAME);
            initializeButton.clicked -= InitializeButton_clicked;
            root.Q<Button>(ADD_COMPONENT_BUTTON_NAME).clicked -= ControllerDrivetrainSettingsEditor_onClick;
            _mainEditor.OnWindowClosed -= _editor_OnWindowClosed;
        }

        private void FindAddComponentMenuFields()
        {
            _addComponentMenu = root.Q<VisualElement>(ADD_COMPONENT_MENU_NAME);
            _addColliderToggle = root.Q<Toggle>(ADD_COLLIDER_TOGGLE_NAME);

            root.Q<Button>(ADD_COMPONENT_BUTTON_NAME).clicked += ControllerDrivetrainSettingsEditor_onClick;
        }

        private void ControllerDrivetrainSettingsEditor_onClick()
        {
            AddComponents();
        }

        private void FindDrivetrainFields()
        {
            _foldout = root.Q<Foldout>(FOLDOUT_NAME);


            _frontLeftWheelObjField = root.Q<ObjectField>(FRONT_LEFT_WHEEL_FIELD_NAME);
            _frontRightWheelObjField = root.Q<ObjectField>(FRONT_RIGHT_WHEEL_FIELD_NAME);
            _rearLeftWheelObjField = root.Q<ObjectField>(REAR_LEFT_WHEEL_FIELD_NAME);
            _rearRightWheelObjField = root.Q<ObjectField>(REAR_RIGHT_WHEEL_FIELD_NAME);

            _frontLeftBrakesObjField = root.Q<ObjectField>(FRONT_LEFT_BRAKES_FIELD_NAME);
            _frontRightBrakesObjField = root.Q<ObjectField>(FRONT_RIGHT_BRAKES_FIELD_NAME);
            _rearLeftBrakesObjField = root.Q<ObjectField>(REAR_LEFT_BRAKES_FIELD_NAME);
            _rearRightBrakesObjField = root.Q<ObjectField>(REAR_RIGHT_BRAKES_FIELD_NAME);

            _bodyMeshField = root.Q<ObjectField>(BODY_FIELD_NAME);
            _notInitializedMessageLabel = root.Q<Label>(NOT_INITIALIZED_LABEL_NAME);
        }

        private void FindWarningWindowFields()
        {
            _warningWindow = root.Q<VisualElement>(WINDOW_NAME);
            _closeWindowsButton = root.Q<Button>(CLOSE_WINDOWS_BUTTON_NAME);
            _confirmButton = root.Q<Button>(CONFIRM_BUTTON_NAME);
            _denyButton = root.Q<Button>(DENY_BUTTON_NAME);


            _closeWindowsButton.clicked += () => { _warningWindow.style.display = DisplayStyle.None; };
            _denyButton.clicked += () => { _warningWindow.style.display = DisplayStyle.None; };
            _confirmButton.clicked += () => { _warningWindow.style.display = DisplayStyle.None; UnpackPrefab(Selection.activeGameObject); InitializeController(); };
        }
        private void SubscribeToInitializeButtonClickEvent()
        {
            root.Q<Button>(INIT_BUTTON_NAME).clicked += InitializeButton_clicked;
        }

        private void InitializeButton_clicked()
        {
            if (_mainEditor.GetController() == null)
            {
                Debug.LogError("CustomVehicleController script is missing");
                return;
            }
            if (_frontLeftWheelObjField.value == null)
            {
                Debug.LogError("Front left transform is missing");
                return;
            }
            if (_frontRightWheelObjField.value == null)
            {
                Debug.LogError("Front right transform is missing");
                return;
            }
            if (_rearLeftWheelObjField.value == null)
            {
                Debug.LogError("Rear left transform is missing");
                return;
            }
            if (_rearRightWheelObjField.value == null)
            {
                Debug.LogError("Rear right transform is missing");
                return;
            }

            if (PrefabUtility.GetPrefabAssetType(_mainEditor.GetController().gameObject) != PrefabAssetType.NotAPrefab)
            {
                _warningWindow.style.display = DisplayStyle.Flex;
                return;
            }

            InitializeController();
        }


        private void UnpackPrefab(GameObject selectedGO)
        {
            GameObject rootGO = selectedGO.transform.root.gameObject;
            PrefabUtility.UnpackPrefabInstance(rootGO, PrefabUnpackMode.Completely, InteractionMode.UserAction);        
        }

        private void AddComponents()
        {
            GameObject gameObject = Selection.activeGameObject;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Add Controller Component");
            int undoID = Undo.GetCurrentGroup();

            Undo.AddComponent<CustomVehicleController>(gameObject);

            if (_addColliderToggle.value)
                Undo.AddComponent<BoxCollider>(gameObject);

            _addColliderToggle.value = false;

            _addComponentMenu.style.display = DisplayStyle.None;

            _mainEditor.RequestUpdate();
            Undo.CollapseUndoOperations(undoID);
        }

        private void InitializeController()
        {
            ControllerHierarchyInitializer init = new ();
            Transform[] wheels = new Transform[4];
            wheels[0] = _frontLeftWheelObjField.value as Transform;
            wheels[1] = _frontRightWheelObjField.value as Transform;
            wheels[2] = _rearLeftWheelObjField.value as Transform;
            wheels[3] = _rearRightWheelObjField.value as Transform;

            Transform[] brakes = new Transform[4];
            brakes[0] = _frontLeftBrakesObjField.value as Transform;
            brakes[1] = _frontRightBrakesObjField.value as Transform;
            brakes[2] = _rearLeftBrakesObjField.value as Transform;
            brakes[3] = _rearRightBrakesObjField.value as Transform;

            Transform[] steerWheels = new Transform[2];
            steerWheels[0] = _frontLeftWheelObjField.value as Transform;
            steerWheels[1] = _frontRightWheelObjField.value as Transform;

            init.SetSteerWheelTransforms(steerWheels);
            init.SetWheelTransforms(wheels);
            init.SetBrakes(brakes);
            init.CreateHierarchyAndInitializeController(_mainEditor.GetSerializedController(),
                _mainEditor.GetSerializedCarVisuals(), _mainEditor.GetController(), _bodyMeshField.value as MeshRenderer);

            DisplayControllerNotInitializedMessage(false);

            _frontLeftWheelObjField.value = null;
            _frontRightWheelObjField.value = null;
            _rearLeftWheelObjField.value = null;
            _rearRightWheelObjField.value = null;
        }



        public void SetVehicleController(CustomVehicleController controller)
        {
            if (controller != null)
            {
                _addComponentMenu.style.display = DisplayStyle.None;

                var frontAxleArray = new SerializedObject(controller).FindProperty("_frontAxles");

                if (frontAxleArray == null || frontAxleArray.arraySize < 1)
                {
                    DisplayControllerNotInitializedMessage(true);
                    return;
                }

                var rearAxleArray = new SerializedObject(controller).FindProperty("_rearAxles");

                if (rearAxleArray == null || rearAxleArray.arraySize < 1)
                {
                    DisplayControllerNotInitializedMessage(true);
                    return;
                }

                DisplayControllerNotInitializedMessage(false);
                return;
            }

            if (Selection.activeGameObject == null)
            {
                _foldout.value = false;
                _addComponentMenu.style.display = DisplayStyle.None;
                _notInitializedMessageLabel.style.display = DisplayStyle.None;
                return;
            }

            _foldout.value = true;
            _addComponentMenu.style.display = DisplayStyle.Flex;
            _notInitializedMessageLabel.style.display = DisplayStyle.None;
        }

        private void DisplayControllerNotInitializedMessage(bool notInitialized)
        {
            _foldout.value = notInitialized;
            _notInitializedMessageLabel.style.display = notInitialized ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
