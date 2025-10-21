using Assets.VehicleController;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class ControllerPresetSettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        private VehiclePartsPresetSO _presetSO;

        #region field names
        private const string SELECTED_PRESET_LABEL_NAME = "PresetSelectionLabel";
        private const string PRESET_OBJECT_FIELD_NAME = "PresetObjectField";
        private const string NO_PRESET_LABEL_NAME = "NoPresetSelectedLabel";
        private const string PRESET_PARTS_HOLDER = "PresetPartsHolder";

        private const string PRESET_ENGINE_NAME = "EnginePresetField";
        private const string PRESET_FI_NAME = "FIPresetField";
        private const string PRESET_NITROUS_NAME = "NitrousPresetField";
        private const string PRESET_TRANSMISSION_NAME = "TransmissionPresetField";
        private const string PRESET_FR_TIRES_NAME = "FrontTiresPresetField";
        private const string PRESET_R_TIRES_NAME = "RearTiresPresetField";
        private const string PRESET_F_SUSP_NAME = "FrontSuspensionPresetField";
        private const string PRESET_R_SUSP_NAME = "RearSuspensionPresetField";
        private const string PRESET_BRAKES_NAME = "BrakesPresetField";
        private const string PRESET_BODY_NAME = "BodyPresetField";

        private const string PRESET_NAME_FIELD = "PresetSONameTextField";
        private const string PRESET_CREATE_BUTTON_NAME = "PresetSaveButton";
        #endregion

        #region fields
        private Label _selectedPresetLabel;
        private ObjectField _presetSOObjectField;
        private Label _noPresetLabel;
        private VisualElement _presetPartsHolder;

        private ObjectField _presetEngineField;
        private ObjectField _presetFIField;
        private ObjectField _presetNitroField;
        private ObjectField _presetTransField;
        private ObjectField _presetFrTiresField;
        private ObjectField _presetRTiresField;
        private ObjectField _presetFrSuspField;
        private ObjectField _presetRSuspField;
        private ObjectField _presetBrakesField;
        private ObjectField _presetBodyField;

        private TextField _presetNameField;
        #endregion

        public const string PRESET_FOLDER_NAME = "Presets";

        public ControllerPresetSettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;

            FindFields();
            BindPresetSOField();
            SubscribeToCreateButtonClick();
            _mainEditor.OnWindowClosed += _mainEditor_OnWindowClosed;
        }

        private void _mainEditor_OnWindowClosed()
        {
            root.Q<Button>(PRESET_CREATE_BUTTON_NAME).clicked -= CreatePresetAsset;
        }

        private void FindFields()
        {
            _selectedPresetLabel = root.Q<Label>(SELECTED_PRESET_LABEL_NAME);
            _noPresetLabel = root.Q<Label>(NO_PRESET_LABEL_NAME);
            _presetPartsHolder = root.Q<VisualElement>(PRESET_PARTS_HOLDER);

            _presetEngineField = root.Q<ObjectField>(PRESET_ENGINE_NAME);

            _presetFIField = root.Q<ObjectField>(PRESET_FI_NAME);

            _presetNitroField = root.Q<ObjectField>(PRESET_NITROUS_NAME);

            _presetTransField = root.Q<ObjectField>(PRESET_TRANSMISSION_NAME);

            _presetFrTiresField = root.Q<ObjectField>(PRESET_FR_TIRES_NAME);

            _presetRTiresField = root.Q<ObjectField>(PRESET_R_TIRES_NAME);


            _presetFrSuspField = root.Q<ObjectField>(PRESET_F_SUSP_NAME);
            _presetRSuspField = root.Q<ObjectField>(PRESET_R_SUSP_NAME);

            _presetBrakesField = root.Q<ObjectField>(PRESET_BRAKES_NAME);

            _presetBodyField = root.Q<ObjectField>(PRESET_BODY_NAME);

            _presetNameField = root.Q<TextField>(PRESET_NAME_FIELD);
        }

        private void BindPresetSOField()
        {
            _presetSOObjectField = root.Q<ObjectField>(PRESET_OBJECT_FIELD_NAME);
            _presetSOObjectField.RegisterValueChangedCallback(x => RebindPresetSettings(_presetSOObjectField.value as VehiclePartsPresetSO));
        }
        private void RebindPresetSettings(VehiclePartsPresetSO loadedPresetSO)
        {
            _presetSO = loadedPresetSO;
            _mainEditor.BindEditorPartFields(_presetSO);

            if (_presetSO == null)
            {
                _selectedPresetLabel.text = "None";
                _selectedPresetLabel.style.color = Color.red;
                _noPresetLabel.style.display = DisplayStyle.Flex;
                _presetPartsHolder.style.display = DisplayStyle.None;

                return;
            }
            else
            {
                _selectedPresetLabel.text = _presetSO.name.ToUpper();

                _selectedPresetLabel.style.color = Color.green;

                _presetPartsHolder.style.display = DisplayStyle.Flex;
                _noPresetLabel.style.display = DisplayStyle.None;
            }

            SerializedObject so = new(_presetSO);
            BindEngineField(so);
            BindFIField(so);
            BindNitrousField(so);
            BindTransField(so);
            BindFrTiresField(so);
            BindRTiresField(so);
            BindFrSuspField(so);
            BindRSuspField(so);
            BindBrakesField(so);
            BindBodyField(so);
        }


        private void BindEngineField(SerializedObject so)
        {
            _presetEngineField.bindingPath = nameof(_presetSO.Engine);
            _presetEngineField.Bind(so);
        }

        private void BindFIField(SerializedObject so)
        {
            _presetFIField.bindingPath = nameof(_presetSO.ForcedInduction);
            _presetFIField.Bind(so);
        }

        private void BindNitrousField(SerializedObject so)
        {
            _presetNitroField.bindingPath = nameof(_presetSO.Nitrous);
            _presetNitroField.Bind(so);
        }
        private void BindTransField(SerializedObject so)
        {
            _presetTransField.bindingPath = nameof(_presetSO.Transmission);
            _presetTransField.Bind(so);
        }
        private void BindFrTiresField(SerializedObject so)
        {
            _presetFrTiresField.bindingPath = nameof(_presetSO.FrontTires);
            _presetFrTiresField.Bind(so);
        }
        private void BindRTiresField(SerializedObject so)
        {
            _presetRTiresField.bindingPath = nameof(_presetSO.RearTires);
            _presetRTiresField.Bind(so);
        }
        private void BindFrSuspField(SerializedObject so)
        {
            _presetFrSuspField.bindingPath = nameof(_presetSO.FrontSuspension);
            _presetFrSuspField.Bind(so);
        }
        private void BindRSuspField(SerializedObject so)
        {
            _presetRSuspField.bindingPath = nameof(_presetSO.RearSuspension);
            _presetRSuspField.Bind(so);
        }
        private void BindBrakesField(SerializedObject so)
        {
            _presetBrakesField.bindingPath = nameof(_presetSO.Brakes);
            _presetBrakesField.Bind(so);
        }
        private void BindBodyField(SerializedObject so)
        {
            _presetBodyField.bindingPath = nameof(_presetSO.Body);
            _presetBodyField.Bind(so);
        }

        private void SubscribeToCreateButtonClick()
        {
            root.Q<Button>(PRESET_CREATE_BUTTON_NAME).clicked += CreatePresetAsset;
        }

        private void CreatePresetAsset()
        {
            string folderPath = LocalPathFinder.Instance.GetVehiclePartsFolderPathForAsset(PRESET_FOLDER_NAME);

            VehiclePartsPresetSO newPreset = ScriptableObject.CreateInstance<VehiclePartsPresetSO>();

            if (AssetSaver.TrySavePreset(folderPath, newPreset, _presetNameField.text, "asset"))
            {
                _presetSO = newPreset;
                _presetSOObjectField.value = _presetSO;
            }
        }

        public void BindVehicleController(SerializedObject controller)
        {
            if (_mainEditor.GetController() == null)
            {
                _presetSOObjectField.Unbind();
                return;
            }

            if(controller != null)
                _presetSOObjectField.BindProperty(controller.FindProperty("_vehiclePartsPreset"));
        }
        public void Unbind() => _presetSOObjectField.Unbind();

        public VehiclePartsPresetSO GetCurrentySelectedVehiclePreset() => _presetSOObjectField.value as VehiclePartsPresetSO;
    }
}
