using Assets.VehicleController;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace Assets.VehicleControllerEditor
{
    public class ControllerSuspensionSettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        #region Suspension Fields
        private SuspensionSO _frontSuspensionSO;

        private ObjectField _frontSuspensionObjectField;
        private FloatField _frontSuspensionStiffnessField;
        private FloatField _frontSuspensionDamperField;
        private Slider _frontSuspensionHeightSlider;
        private FloatField _frontSuspTravelField;
        private FloatField _frontAntiRollBarField;
        private TextField _frontSuspensionNameField;

        private SuspensionSO _rearSuspensionSO;

        private ObjectField _rearSuspensionObjectField;
        private FloatField _rearSuspensionStiffnessField;
        private FloatField _rearSuspensionDamperField;
        private Slider _rearSuspensionHeightSlider;
        private FloatField _rearSuspTravelField;
        private FloatField _rearAntiRollBarField;
        private TextField _rearSuspensionNameField;
        #endregion

        #region SuspensionFieldNames
        private const string FRONT_SUSPENSION_OBJECT_FIELD = "FrontSuspensionObjectField";
        private const string FRONT_SUSPENSION_STIFFNESS_FIELD = "FrontSuspensionStiffnessField";
        private const string FRONT_SUSPENSION_DAMPER_FIELD = "FrontSuspensionDamperField";
        private const string FRONT_SUSPENSION_HEIGHT_SLIDER_FIELD = "FrontSuspensionHeightSliderField";
        private const string FRONT_SUSPENSTION_TRAVEL_FIELD = "FrontSuspensionTravelDistance";
        private const string FRONT_ANTIROLL_BAR_FIELD = "FrontAntiRollField";
        private const string FRONT_SUSPENSION_NAME_FIELD = "FrontSuspensionNameField";
        private const string FRONT_SUSPENSION_SAVE_BUTTON_NAME = "FrontSuspensionSaveButton";

        private const string REAR_SUSPENSION_OBJECT_FIELD = "RearSuspensionObjectField";
        private const string REAR_SUSPENSION_STIFFNESS_FIELD = "RearSuspensionStiffnessField";
        private const string REAR_SUSPENSION_DAMPER_FIELD = "RearSuspensionDamperField";
        private const string REAR_SUSPENSION_HEIGHT_SLIDER_FIELD = "RearSuspensionHeightSliderField";
        private const string REAR_SUSPENSTION_TRAVEL_FIELD = "RearSuspensionTravelDistance";
        private const string REAR_ANTIROLL_BAR_FIELD = "RearAntiRollField";
        private const string REAR_SUSPENSION_NAME_FIELD = "RearSuspensionNameField";
        private const string REAR_SUSPENSION_SAVE_BUTTON_NAME = "RearSuspensionSaveButton";
        #endregion
        public const string SUSPENSION_FOLDER_NAME = "Suspensions";

        public ControllerSuspensionSettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;
            FindFrontSuspensionFields();
            FindRearSuspensionFields();

            BindFrontSuspensionSOField();
            BindRearSuspensionSOField();

            SubscribeToFrontSuspensionSaveButtonClick();
            SubscribeToRearSuspensionSaveButtonClick();

            _mainEditor.OnWindowClosed += _mainEditor_OnWindowClosed;
            SetTooltips();
        }
        private void SetTooltips()
        {
            StringBuilder sb1 = new StringBuilder();
            sb1.AppendLine("Defines the amount of force that is applied to the suspension.");
            sb1.AppendLine("");
            sb1.AppendLine("If the value is too low, the suspension won't be able to keep the car at a desired height above the ground.");
            sb1.AppendLine("");
            sb1.AppendLine("Vehicle mass has an influence on the amount of force required.");
            sb1.AppendLine("");
            sb1.AppendLine("Recommended values: [50000: 200000].");

            _frontSuspensionStiffnessField.tooltip = sb1.ToString();
            _rearSuspensionStiffnessField.tooltip = sb1.ToString();

            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine("Damper stiffness defines how much the suspension wants to travel.");
            sb2.AppendLine("");
            sb2.AppendLine("The lower the value, the less resistance the suspension has to movement.");
            sb2.AppendLine("");
            sb2.AppendLine("Recommended values: [500: 3000].");

            _frontSuspensionDamperField.tooltip = sb2.ToString();
            _rearSuspensionDamperField.tooltip = sb2.ToString();

            StringBuilder sb3 = new StringBuilder();
            sb3.AppendLine("AntilRoll bar is an essential component in the vehicle's suspension system.");
            sb3.AppendLine("");
            sb3.AppendLine("It provides the most value to vehicle resistance to rolling over, thus stabilizing the vehicle when turning.");
            sb3.AppendLine("");
            sb3.AppendLine("Recommended value: 50% of the suspension stiffness.");
            _frontAntiRollBarField.tooltip = sb3.ToString();
            _rearAntiRollBarField.tooltip = sb3.ToString();
        }

        private void _mainEditor_OnWindowClosed()
        {
            var button1 = root.Q<Button>(name: FRONT_SUSPENSION_SAVE_BUTTON_NAME);
            button1.clicked -= FrontSuspensionCreateAssetButton_onClick;

            var button2 = root.Q<Button>(name: REAR_SUSPENSION_SAVE_BUTTON_NAME);
            button2.clicked -= RearSuspensionCreateAssetButton_onClick;
            _mainEditor.OnWindowClosed -= _mainEditor_OnWindowClosed;
        }

        private void FindFrontSuspensionFields()
        {
            _frontSuspensionStiffnessField = root.Q<FloatField>(FRONT_SUSPENSION_STIFFNESS_FIELD);
            _frontSuspensionStiffnessField.RegisterValueChangedCallback(evt =>
            {
                _frontSuspensionStiffnessField.value =
                Mathf.Max(0, _frontSuspensionStiffnessField.value);
            });

            _frontSuspensionDamperField = root.Q<FloatField>(FRONT_SUSPENSION_DAMPER_FIELD);
            _frontSuspensionDamperField.RegisterValueChangedCallback(evt =>
            {
                _frontSuspensionDamperField.value =
                Mathf.Max(0, _frontSuspensionDamperField.value);
            });

            _frontAntiRollBarField = root.Q<FloatField>(FRONT_ANTIROLL_BAR_FIELD);
            _frontAntiRollBarField.RegisterValueChangedCallback(evt => {
                _frontAntiRollBarField.value = Mathf.Clamp(_frontAntiRollBarField.value, 0, _frontSuspensionStiffnessField.value);
            });

            _frontSuspensionHeightSlider = root.Q<Slider>(FRONT_SUSPENSION_HEIGHT_SLIDER_FIELD);
            _frontSuspensionHeightSlider.RegisterValueChangedCallback(evt => { _frontSuspTravelField.value = Mathf.Clamp(_frontSuspTravelField.value, 0, _frontSuspensionHeightSlider.value); });
            _frontSuspensionNameField = root.Q<TextField>(FRONT_SUSPENSION_NAME_FIELD);

            _frontSuspTravelField = root.Q<FloatField>(FRONT_SUSPENSTION_TRAVEL_FIELD);
            _frontSuspTravelField.RegisterValueChangedCallback(evt => { _frontSuspTravelField.value = Mathf.Clamp(_frontSuspTravelField.value, 0 , _frontSuspensionHeightSlider.value); });
        }
        private void FindRearSuspensionFields()
        {
            _rearSuspensionStiffnessField = root.Q<FloatField>(REAR_SUSPENSION_STIFFNESS_FIELD);
            _rearSuspensionStiffnessField.RegisterValueChangedCallback(evt =>
            {
                _rearSuspensionStiffnessField.value =
                Mathf.Max(0, _rearSuspensionStiffnessField.value);
            });
            _rearSuspensionDamperField = root.Q<FloatField>(REAR_SUSPENSION_DAMPER_FIELD);
            _rearSuspensionDamperField.RegisterValueChangedCallback(evt =>
            {
                _rearSuspensionDamperField.value =
                Mathf.Max(0, _rearSuspensionDamperField.value);
            });


            _rearAntiRollBarField = root.Q<FloatField>(REAR_ANTIROLL_BAR_FIELD);
            _rearAntiRollBarField.RegisterValueChangedCallback(evt => { 
                _rearAntiRollBarField.value = Mathf.Clamp(_rearAntiRollBarField.value, 0, _rearSuspensionStiffnessField.value); 
            });

            _rearSuspensionHeightSlider = root.Q<Slider>(REAR_SUSPENSION_HEIGHT_SLIDER_FIELD);
            _rearSuspensionHeightSlider.RegisterValueChangedCallback(evt => { _rearSuspTravelField.value = Mathf.Clamp(_rearSuspTravelField.value, 0, _rearSuspensionHeightSlider.value); });
            _rearSuspensionNameField = root.Q<TextField>(REAR_SUSPENSION_NAME_FIELD);

            _rearSuspTravelField = root.Q<FloatField>(REAR_SUSPENSTION_TRAVEL_FIELD);
            _rearSuspTravelField.RegisterValueChangedCallback(evt => { _rearSuspTravelField.value = Mathf.Clamp(_rearSuspTravelField.value, 0, _rearSuspensionHeightSlider.value); });
        }

        private void BindFrontSuspensionSOField()
        {
            _frontSuspensionObjectField = root.Q<ObjectField>(FRONT_SUSPENSION_OBJECT_FIELD);

            _frontSuspensionObjectField.RegisterValueChangedCallback(x => RebindFrontSuspensionSettings(_frontSuspensionObjectField.value as SuspensionSO));

            if (_frontSuspensionObjectField.value == null)
            {
                _frontSuspensionSO = SuspensionSO.CreateDefaultSuspensionSO();
            }
            else
            {
                _frontSuspensionSO = _frontSuspensionObjectField.value as SuspensionSO;
            }
        }

        private void RebindFrontSuspensionSettings(SuspensionSO loadedFrontSuspensionSO)
        {
            _frontSuspensionSO = loadedFrontSuspensionSO;

            if (_frontSuspensionSO == null)
                _frontSuspensionSO = SuspensionSO.CreateDefaultSuspensionSO();

            SerializedObject so = new (_frontSuspensionSO);
            BindFrontSuspensionStiffnessField(so);
            BindFrontdSuspensionDamperField(so);
            BindFrontSuspensionHeightField(so);
            BindFrontAntirollField(so);
            BindFrontTravel(so);
        }

        private void BindFrontSuspensionStiffnessField(SerializedObject so)
        {
            _frontSuspensionStiffnessField.bindingPath = nameof(_frontSuspensionSO.SpringStiffness);
            _frontSuspensionStiffnessField.Bind(so);
        }
        private void BindFrontdSuspensionDamperField(SerializedObject so)
        {
            _frontSuspensionDamperField.bindingPath = nameof(_frontSuspensionSO.SpringDampingStiffness);
            _frontSuspensionDamperField.Bind(so);
        }
        private void BindFrontSuspensionHeightField(SerializedObject so)
        {
            _frontSuspensionHeightSlider.bindingPath = nameof(_frontSuspensionSO.SpringRestDistance);
            _frontSuspensionHeightSlider.Bind(so);
        }

        private void BindFrontAntirollField(SerializedObject so)
        {
            _frontAntiRollBarField.bindingPath = nameof(_frontSuspensionSO.AntiRollForce);
            _frontAntiRollBarField.Bind(so);
        }

        private void BindFrontTravel(SerializedObject so)
        {
            _frontSuspTravelField.bindingPath = nameof(_frontSuspensionSO.SpringTravelLength);
            _frontSuspTravelField.Bind(so);
        }

        private void BindRearSuspensionSOField()
        {
            _rearSuspensionObjectField = root.Q<ObjectField>(REAR_SUSPENSION_OBJECT_FIELD);

            _rearSuspensionObjectField.RegisterValueChangedCallback(x => RebindRearSuspensionSettings(_rearSuspensionObjectField.value as SuspensionSO));

            if (_rearSuspensionObjectField.value == null)
            {
                _rearSuspensionSO = SuspensionSO.CreateDefaultSuspensionSO();
            }
            else
            {
                _rearSuspensionSO = _rearSuspensionObjectField.value as SuspensionSO;
            }
        }

        private void RebindRearSuspensionSettings(SuspensionSO loadedFrontSuspensionSO)
        {
            _rearSuspensionSO = loadedFrontSuspensionSO;

            if (_rearSuspensionSO == null)
                _rearSuspensionSO = SuspensionSO.CreateDefaultSuspensionSO();

            SerializedObject so = new (_rearSuspensionSO);
            BindRearSuspensionStiffnessField(so);
            BindRearSuspensionDamperField(so);
            BindRearSuspensionHeightField(so);
            BindRearAntirollField(so);
            BindRearTravel(so);
        }

        private void BindRearSuspensionStiffnessField(SerializedObject so)
        {
            _rearSuspensionStiffnessField.bindingPath = nameof(_rearSuspensionSO.SpringStiffness);
            _rearSuspensionStiffnessField.Bind(so);
        }
        private void BindRearSuspensionDamperField(SerializedObject so)
        {
            _rearSuspensionDamperField.bindingPath = nameof(_rearSuspensionSO.SpringDampingStiffness);
            _rearSuspensionDamperField.Bind(so);
        }
        private void BindRearSuspensionHeightField(SerializedObject so)
        {
            _rearSuspensionHeightSlider.bindingPath = nameof(_rearSuspensionSO.SpringRestDistance);
            _rearSuspensionHeightSlider.Bind(so);
        }
        private void BindRearAntirollField(SerializedObject so)
        {
            _rearAntiRollBarField.bindingPath = nameof(_rearSuspensionSO.AntiRollForce);
            _rearAntiRollBarField.Bind(so);
        }
        private void BindRearTravel(SerializedObject so)
        {
            _rearSuspTravelField.bindingPath = nameof(_rearSuspensionSO.SpringTravelLength);
            _rearSuspTravelField.Bind(so);
        }

        private void SubscribeToFrontSuspensionSaveButtonClick()
        {
            var button = root.Q<Button>(name: FRONT_SUSPENSION_SAVE_BUTTON_NAME);
            button.clicked += FrontSuspensionCreateAssetButton_onClick;
        }
        private void FrontSuspensionCreateAssetButton_onClick()
        {
            string folderPath = LocalPathFinder.Instance.GetVehiclePartsFolderPathForAsset(SUSPENSION_FOLDER_NAME);

            SuspensionSO newSusp = SuspensionSO.CreateDefaultSuspensionSO();

            if (AssetSaver.TrySaveAsset(folderPath, newSusp, _frontSuspensionNameField.text, "asset"))
            {
                _frontSuspensionSO = newSusp;
                _frontSuspensionObjectField.value = _frontSuspensionSO;
            }
        }
        private void SubscribeToRearSuspensionSaveButtonClick()
        {
            var button = root.Q<Button>(name: REAR_SUSPENSION_SAVE_BUTTON_NAME);
            button.clicked += RearSuspensionCreateAssetButton_onClick;
        }
        private void RearSuspensionCreateAssetButton_onClick()
        {
            string folderPath = LocalPathFinder.Instance.GetVehiclePartsFolderPathForAsset(SUSPENSION_FOLDER_NAME);

            SuspensionSO newSusp = SuspensionSO.CreateDefaultSuspensionSO();

            if (AssetSaver.TrySaveAsset(folderPath, newSusp, _rearSuspensionNameField.text, "asset"))
            {
                _rearSuspensionSO = newSusp;
                _rearSuspensionObjectField.value = _rearSuspensionSO;
            }
        }

        public void SetVehicleController(SuspensionSO frontSusp, SuspensionSO rearSusp)
        {
            _frontSuspensionObjectField.value = frontSusp;
            _rearSuspensionObjectField.value = rearSusp;
        }

        public void BindVehicleController(SerializedProperty frontSuspProperty, SerializedProperty rearSuspProperty)
        {
            _frontSuspensionObjectField.Unbind();
            if(frontSuspProperty != null)
                _frontSuspensionObjectField.BindProperty(frontSuspProperty);

            _rearSuspensionObjectField.Unbind();
            if (rearSuspProperty != null)
                _rearSuspensionObjectField.BindProperty(rearSuspProperty);
        }

        public void BindPreset(SerializedObject preset)
        {
            _frontSuspensionObjectField.Unbind();
            if(preset != null)
                _frontSuspensionObjectField.BindProperty(preset.FindProperty("FrontSuspension"));

            _rearSuspensionObjectField.Unbind();
            if (preset != null)
                _rearSuspensionObjectField.BindProperty(preset.FindProperty("RearSuspension"));
        }

        public void Unbind()
        {
            _frontSuspensionObjectField.Unbind();
            _rearSuspensionObjectField.Unbind();
        }
    }

}
