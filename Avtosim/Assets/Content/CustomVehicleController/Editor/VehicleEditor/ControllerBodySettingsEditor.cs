using Assets.VehicleController;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class ControllerBodySettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        #region body fields
        private VehicleBodySO _bodySO;

        private ObjectField _bodyObjectField;
        private FloatField _massField;
        private FloatField _forwardDragField;
        private FloatField _downforceField;
        private FloatField _corneringResistanceField;
        private CurveField _corneringResistanceCurve;
        private TextField _bodyNameField;
        #endregion

        #region body field names
        private const string BODY_OBJECT_FIELD = "BodyObjectField";
        private const string BODY_MASS_FIELD = "MassField";
        private const string BODY_FORWARD_DRAG_FIELD = "ForwardDragField";
        private const string BODY_DOWNFORCE_FIELD = "DownforceField";
        private const string BODY_CORNERING_RES_FIELD = "CorneringResistanceField";
        private const string BODY_CORNERING_RES_CURVE = "CorneringResistanceCurve";


        private const string BODY_NAME_FIELD = "BodyNameField";
        private const string BODY_CREATE_BUTTON_NAME = "CreateNewBodyButton";
        #endregion

        private EnumField _drivetrainTypeEnum;
        private const string DRIVETRAIN_TYPE_ENUM_NAME = "DrivetrainTypeEnum";
        private DrivetrainType _drivetrainTypePlayMode;

        public const string BODY_FOLDER_NAME = "VehicleBodies";
        public ControllerBodySettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;
            FindBodyFields();
            BindBodySOField();
            SubscribeToBodySaveButtonClick();
            _mainEditor.OnWindowClosed += Editor_OnWindowClosed;
            SetTooltips();
        }


        private void SetTooltips()
        {
            StringBuilder sb1 = new StringBuilder();
            sb1.AppendLine("Forward drag of a rigidbody. Works like air resistance that slows down the acceleration.");
            sb1.AppendLine("");
            sb1.AppendLine("Recommended values [0:0.2].");

            _forwardDragField.tooltip = sb1.ToString();

            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine("Defines the amount of force applied to the vehicle in the downward direction.");
            sb2.AppendLine("");
            sb2.AppendLine("Helps stabilize the vehicle and prevents rolling over at turns.");
            sb2.AppendLine("");
            sb2.AppendLine("Recommended values[10:100].");

            _downforceField.tooltip = sb2.ToString();

            StringBuilder sb3 = new StringBuilder();
            sb3.AppendLine("Cornering resistance simulates the vehicle control getting stiffer at high speeds.");
            sb3.AppendLine("");
            sb3.AppendLine("Also has the effect of wheels automatically recentering.");
            sb3.AppendLine("");
            sb3.AppendLine("Recommended values for drift [5:10].");
            sb3.AppendLine("");
            sb3.AppendLine("Recommended values for grip [10:30], but it highly depends on the cornering stiffness of the tires.");
            _corneringResistanceField.tooltip = sb3.ToString();

            StringBuilder sb4 = new StringBuilder();
            sb4.AppendLine("A multiplier to the cornering resistance strength.");
            sb4.AppendLine("");
            sb4.AppendLine("The X-axis is the current vehicle speed divided by the maximum speed of the engine. Y-axis - effect multiplier.");

            _corneringResistanceCurve.tooltip = sb4.ToString();
        }

        private void Editor_OnWindowClosed()
        {
            var button = root.Q<Button>(name: BODY_CREATE_BUTTON_NAME);
            button.clicked -= BodyCreateAssetButton_onClick;
            _mainEditor.OnWindowClosed -= Editor_OnWindowClosed;
        }

        public void PasteStats(SerializedObject serializedObject)
        {
            serializedObject.FindProperty(nameof(CustomVehicleController.DrivetrainType)).intValue = (int)_drivetrainTypePlayMode;
        }

        public void CopyStats(SerializedObject serializedObject)
        {
            _drivetrainTypePlayMode = (DrivetrainType)serializedObject.FindProperty(nameof(CustomVehicleController.DrivetrainType)).intValue;
        }

        private void FindBodyFields()
        {
            _massField = root.Q<FloatField>(BODY_MASS_FIELD);
            _massField.RegisterValueChangedCallback(evt => { _massField.value = Mathf.Max(1, _massField.value); });

            _forwardDragField = root.Q<FloatField>(BODY_FORWARD_DRAG_FIELD);
            _forwardDragField.RegisterValueChangedCallback(evt => { _forwardDragField.value = Mathf.Max(0, _forwardDragField.value); });

            _downforceField = root.Q<FloatField>(BODY_DOWNFORCE_FIELD);
            _downforceField.RegisterValueChangedCallback(evt => { _downforceField.value = Mathf.Max(0, _downforceField.value); });

            _corneringResistanceField = root.Q<FloatField>(BODY_CORNERING_RES_FIELD);
            _corneringResistanceField.RegisterValueChangedCallback(evt => { _corneringResistanceField.value = Mathf.Max(0, _corneringResistanceField.value); });

            _corneringResistanceCurve = root.Q<CurveField>(BODY_CORNERING_RES_CURVE);

            _bodyNameField = root.Q<TextField>(BODY_NAME_FIELD);

            _drivetrainTypeEnum = root.Q<EnumField>(DRIVETRAIN_TYPE_ENUM_NAME);
            _drivetrainTypeEnum.RegisterValueChangedCallback(val => { UpdateDrivetrainType((DrivetrainType)_drivetrainTypeEnum.value); });
        }

        private void UpdateDrivetrainType(DrivetrainType type)
        {
            SerializedObject serializedObject = _mainEditor.GetSerializedController();

            if (serializedObject != null)
            {
                serializedObject.FindProperty(nameof(CustomVehicleController.DrivetrainType)).intValue = (int)type;
                _mainEditor.SaveController();
            }
        }

        private void BindBodySOField()
        {
            _bodyObjectField = root.Q<ObjectField>(BODY_OBJECT_FIELD);

            _bodyObjectField.RegisterValueChangedCallback(x => RebindBodySettings(_bodyObjectField.value as VehicleBodySO));

            if (_bodyObjectField.value == null)
            {
                _bodySO = VehicleBodySO.CreateDefaultBodySO();
            }
            else
            {
                _bodySO = _bodyObjectField.value as VehicleBodySO;
            }
        }
        private void RebindBodySettings(VehicleBodySO loadedBodySO)
        {
            _bodySO = loadedBodySO;

            if (_bodySO == null)
                _bodySO = VehicleBodySO.CreateDefaultBodySO();

            SerializedObject so = new (_bodySO);
            BindBodyMasFields(so);
            BindBodyDownforceField(so);
            BindBodyCorneringResField(so);
            BindResCurve(so);
            BindForwardDragField(so);
        }

        private void BindBodyMasFields(SerializedObject so)
        {
            _massField.bindingPath = nameof(_bodySO.Mass);
            _massField.Bind(so);
        }
        private void BindBodyDownforceField(SerializedObject so)
        {
            _downforceField.bindingPath = nameof(_bodySO.Downforce);
            _downforceField.Bind(so);
        }
        private void BindBodyCorneringResField(SerializedObject so)
        {
            _corneringResistanceField.bindingPath = nameof(_bodySO.CorneringResistanceStrength);
            _corneringResistanceField.Bind(so);
        }
        private void BindForwardDragField(SerializedObject so)
        {
            _forwardDragField.bindingPath = nameof(_bodySO.ForwardDrag);
            _forwardDragField.Bind(so);
        }

        private void BindResCurve(SerializedObject so)
        {
            _corneringResistanceCurve.bindingPath = nameof(_bodySO.CorneringResistanceCurve);
            _corneringResistanceCurve.Bind(so);
        }

        private void SubscribeToBodySaveButtonClick()
        {
            var button = root.Q<Button>(name: BODY_CREATE_BUTTON_NAME);
            button.clicked += BodyCreateAssetButton_onClick;
        }

        private void BodyCreateAssetButton_onClick()
        {
            string folderPath = LocalPathFinder.Instance.GetVehiclePartsFolderPathForAsset(BODY_FOLDER_NAME);

            VehicleBodySO newBody = VehicleBodySO.CreateDefaultBodySO();

            if(AssetSaver.TrySaveAsset(folderPath, newBody, _bodyNameField.text, "asset"))
            {
                _bodySO = newBody;
                _bodyObjectField.value = _bodySO;
            }
            else
                Debug.LogError($"Error occured while trying to save {_bodyNameField.text} at {folderPath}");
        }

        public void BindVehicleController(SerializedObject controller, SerializedProperty bodyProperty)
        {
            if(controller != null)
                _drivetrainTypeEnum.value = (DrivetrainType)controller.FindProperty(nameof(CustomVehicleController.DrivetrainType)).intValue;

            _bodyObjectField.Unbind();

            if(bodyProperty != null)
                _bodyObjectField.BindProperty(bodyProperty);           
        }

        public void BindPreset(SerializedObject controller, SerializedObject preset)
        {
            if (controller != null)
                _drivetrainTypeEnum.value = (DrivetrainType)controller.FindProperty(nameof(CustomVehicleController.DrivetrainType)).intValue;

            _bodyObjectField.Unbind();

            if(preset != null)
                _bodyObjectField.BindProperty(preset.FindProperty("Body"));
        }

        public void Unbind() => _bodyObjectField.Unbind();
    }

}
