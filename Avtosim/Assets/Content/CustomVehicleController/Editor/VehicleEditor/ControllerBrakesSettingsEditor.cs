using Assets.VehicleController;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class ControllerBrakesSettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        #region fields
        private ObjectField _brakesSOObjectField;

        private BrakesSO _brakesSO;

        private FloatField _brakesForceField;
        private FloatField _handbrakeForceField;
        private Slider _handbrakeTractionSlider;

        private TextField _brakesNameTextField;
        #endregion

        #region FieldNames
        private const string BRAKES_SO_OBJECT_FIELD_NAME = "BrakesObjectField";
        private const string BRAKES_FORCE_FIELD_NAME = "BrakesForceField";
        private const string HANDBRAKE_FORCE_FIELD_NAME = "HandbrakeForceField";
        private const string HANDBRAKE_TRACTION_SLIDER_NAME = "HandbrakeTraction";
        private const string BRAKES_NAME_FIELD_NAME = "BrakesNameField";
        private const string BRAKES_SAVE_BUTTON_NAME = "BrakesCreateButton";
        #endregion

        public const string BRAKES_FOLDER_NAME = "Brakes";

        public ControllerBrakesSettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;

            FindBrakesFields();

            BindBrakesSOField();
            SubscribeToBrakesSaveButtonClick();

            _mainEditor.OnWindowClosed += _mainEditor_OnWindowClosed;
            SetTooltips();
        }

        private void SetTooltips()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Defines the stopping ability of a car.");
            sb.AppendLine("");
            sb.AppendLine("The car is slowed down by increasing drag.");
            sb.AppendLine("");
            sb.AppendLine("The weight of the vehicle affects the stopping ability");
            sb.AppendLine("");
            sb.AppendLine("Recommended values [15000:30000].");

            _brakesForceField.tooltip = sb.ToString();

            StringBuilder sb2 = new StringBuilder();
            sb2.AppendLine("Defines the amount of force that will be applied to the rear wheels.");
            sb2.AppendLine();
            sb2.AppendLine("Unlike brakes, the car is slowed down by adding force in the opposite direction to movement.");
            sb2.AppendLine();
            sb2.AppendLine("Recommended values [5000:15000]");

            _handbrakeForceField.tooltip = sb2.ToString();
        }

        private void _mainEditor_OnWindowClosed()
        {
            var button = root.Q<Button>(name: BRAKES_SAVE_BUTTON_NAME);
            button.clicked -= BrakesCreateAssetButton_onClick;
            _mainEditor.OnWindowClosed -= _mainEditor_OnWindowClosed;
        }

        private void FindBrakesFields()
        {
            _brakesForceField = root.Q<FloatField>(BRAKES_FORCE_FIELD_NAME);
            _brakesForceField.RegisterValueChangedCallback(evt => { _brakesForceField.value = Mathf.Max(0, _brakesForceField.value); });
            _handbrakeForceField = root.Q<FloatField>(HANDBRAKE_FORCE_FIELD_NAME);
            _handbrakeForceField.RegisterValueChangedCallback(evt => { _handbrakeForceField.value = Mathf.Max(0, _handbrakeForceField.value); });
            _handbrakeTractionSlider = root.Q<Slider>(HANDBRAKE_TRACTION_SLIDER_NAME);
            _brakesNameTextField = root.Q<TextField>(BRAKES_NAME_FIELD_NAME);
        }


        private void BindBrakesSOField()
        {
            _brakesSOObjectField = root.Q<ObjectField>(BRAKES_SO_OBJECT_FIELD_NAME);

            _brakesSOObjectField.RegisterValueChangedCallback(x => RebindBrakesSettings(_brakesSOObjectField.value as BrakesSO));

            if (_brakesSOObjectField.value == null)
                _brakesSO = BrakesSO.CreateDefaultBrakesSO();
            else
                _brakesSO = _brakesSOObjectField.value as BrakesSO;

        }
        private void RebindBrakesSettings(BrakesSO loadedBrakesSO)
        {
            _brakesSO = loadedBrakesSO;

            if (_brakesSO == null)
                _brakesSO = BrakesSO.CreateDefaultBrakesSO();

            SerializedObject so = new (_brakesSO);
            BindBrakesForceField(so);
            BindHandbrakesForceField(so);
            BindHandbrakeTractionField(so);
        }
        private void BindBrakesForceField(SerializedObject so)
        {
            _brakesForceField.bindingPath = nameof(_brakesSO.BrakesStrength);
            _brakesForceField.Bind(so);
        }
        private void BindHandbrakesForceField(SerializedObject so)
        {
            _handbrakeForceField.bindingPath = nameof(_brakesSO.HandbrakeForce);
            _handbrakeForceField.Bind(so);
        }
        private void BindHandbrakeTractionField(SerializedObject so)
        {
            _handbrakeTractionSlider.bindingPath = nameof(_brakesSO.HandbrakeTractionPercent);
            _handbrakeTractionSlider.Bind(so);
        }

        private void SubscribeToBrakesSaveButtonClick()
        {
            var button = root.Q<Button>(name: BRAKES_SAVE_BUTTON_NAME);
            button.clicked += BrakesCreateAssetButton_onClick;
        }

        private void BrakesCreateAssetButton_onClick()
        {
            string folderPath = LocalPathFinder.Instance.GetVehiclePartsFolderPathForAsset(BRAKES_FOLDER_NAME);

            BrakesSO newBrakes = BrakesSO.CreateDefaultBrakesSO();

            if (AssetSaver.TrySaveAsset(folderPath, newBrakes, _brakesNameTextField.text, "asset"))
            {
                _brakesSO = newBrakes;
                _brakesSOObjectField.value = _brakesSO;
            }
        }

        public void BindVehicleController(SerializedProperty brakesProperty)
        {
            _brakesSOObjectField.Unbind();
            if(brakesProperty != null)
                _brakesSOObjectField.BindProperty(brakesProperty);      
        }

        public void BindPreset(SerializedObject preset)
        {
            _brakesSOObjectField.Unbind();
            if(preset != null)
                _brakesSOObjectField.BindProperty(preset.FindProperty("Brakes"));
        }

        public void Unbind() => _brakesSOObjectField.Unbind();
    }

}
