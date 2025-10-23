using Assets.VehicleController;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace Assets.VehicleControllerEditor
{
    public class ControllerForcedInductionSettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        #region body fields
        private ForcedInductionSO _inductionSO;

        private ObjectField _fiObjectField;
        private EnumField _foTypeEnum;
        private FloatField _maxBoostField;
        private Slider _turboLagSlider;
        private FloatField _turboSpinSpeedField;
        private FloatField _turboSpooldownSpeedField;
        private Toggle _antiLagSystemToggle;
        private Slider _antiLagEffect;
        private Slider _antiLagChance;

        private VisualElement _antiLagParamsHolder;
        private VisualElement _turboParamsHolder;

        private TextField _fiNameField;
        #endregion

        #region body field names
        private const string FI_OBJECT_FIELD_NAME = "ForcedInductionSOObjectField";
        private const string FI_TYPE_ENUM = "FITypeEnum";
        private const string FI_MAX_BOOST_FIELD_NAME = "FOMaxBoostField";
        private const string FI_TURBO_LAG_SLIDER_NAME = "TurboLagSlider";
        private const string FI_TURBO_SPEED_FIELD_NAME = "TurboSpinSpeedField";
        private const string FI_TURBO_SPEED_DOWN_FIELD_NAME = "TurboSpooldownSpeed";
        private const string FI_ANTILAG_INSTALLED = "AntiLagSystem";
        private const string FI_ANTILAG_EFFECT = "AntiLagEffectSlider";
        private const string FI_ANTILAG_CHANCE = "AntiLagChanceSlider";

        private const string FI_ANTILAG_PARAMS_HOLDER_NAME = "AntiLagParams";
        private const string FI_TURBO_PARAMS_HOLDER_NAME = "TurboParams";

        private const string FI_NAME_FIELD_NAME = "FISONameTextField";
        private const string FI_CREATE_BUTTON_NAME = "FOSaveButton";
        #endregion

        public const string FORCED_INDUCTION_FOLDER_NAME = "ForcedInductions";

        public ControllerForcedInductionSettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;
            FindFIFields();
            BindFISOField();
            SubscribeToFISaveButtonClick();
            _mainEditor.OnWindowClosed += _mainEditor_OnWindowClosed;


            SetTooltips();
        }


        private void SetTooltips()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Forced induction provides an additional boost. The way boost is provided depends on the forced induction type.");
            sb.AppendLine("");
            sb.AppendLine("Turbocharger: isn't working until certain engine rpm. Takes time to spin to full speed. Boost depends on gas input.");
            sb.AppendLine("");
            sb.AppendLine("Supercharger: constant boost.");
            sb.AppendLine("");
            sb.AppendLine("Centrifugal: boost is directly tied to engine rpm.");
            _foTypeEnum.tooltip = sb.ToString();
        }

        private void _mainEditor_OnWindowClosed()
        {
            var button = root.Q<Button>(name: FI_CREATE_BUTTON_NAME);
            button.clicked -= FICreateAssetButton_onClick;
            _mainEditor.OnWindowClosed -= _mainEditor_OnWindowClosed;
        }

        private void FindFIFields()
        {
            _foTypeEnum = root.Q<EnumField>(FI_TYPE_ENUM);
            _foTypeEnum.RegisterValueChangedCallback(evt => { DisplayForcedInductionTypeField((ForcedInductionType)_foTypeEnum.value); });

            _maxBoostField = root.Q<FloatField>(FI_MAX_BOOST_FIELD_NAME);
            _maxBoostField.RegisterValueChangedCallback(evt => { _maxBoostField.value = Mathf.Max(0, _maxBoostField.value); });

            _turboLagSlider = root.Q<Slider>(FI_TURBO_LAG_SLIDER_NAME);

            _turboSpinSpeedField = root.Q<FloatField>(FI_TURBO_SPEED_FIELD_NAME);
            _turboSpinSpeedField.RegisterValueChangedCallback(evt => { _turboSpinSpeedField.value = Mathf.Max(0.1f, _turboSpinSpeedField.value); });
            _turboSpooldownSpeedField = root.Q<FloatField>(FI_TURBO_SPEED_DOWN_FIELD_NAME);
            _turboSpooldownSpeedField.RegisterValueChangedCallback(evt => { _turboSpooldownSpeedField.value = Mathf.Max(0.1f, _turboSpooldownSpeedField.value); });


            _fiNameField = root.Q<TextField>(FI_NAME_FIELD_NAME);

            _antiLagParamsHolder = root.Q<VisualElement>(FI_ANTILAG_PARAMS_HOLDER_NAME);
            _turboParamsHolder = root.Q<VisualElement>(FI_TURBO_PARAMS_HOLDER_NAME);

            _antiLagSystemToggle = root.Q<Toggle>(FI_ANTILAG_INSTALLED);
            _antiLagSystemToggle.RegisterValueChangedCallback(evt => {
                _antiLagParamsHolder.style.display = _antiLagSystemToggle.value ? DisplayStyle.Flex: DisplayStyle.None;
            });

            _antiLagEffect = root.Q<Slider>(FI_ANTILAG_EFFECT);
            _antiLagChance = root.Q<Slider>(FI_ANTILAG_CHANCE);
        }

        private void DisplayForcedInductionTypeField(ForcedInductionType type)
        {
            switch(type)
            {
                case ForcedInductionType.None:
                    _maxBoostField.style.display = DisplayStyle.None;
                    _turboParamsHolder.style.display = DisplayStyle.None;
                    break;
                case ForcedInductionType.Supercharger:
                case ForcedInductionType.Centrifugal:
                    _maxBoostField.style.display = DisplayStyle.Flex;
                    _turboParamsHolder.style.display = DisplayStyle.None;
                    break;
                case ForcedInductionType.Turbocharger:
                    _maxBoostField.style.display = DisplayStyle.Flex;
                    _turboParamsHolder.style.display = DisplayStyle.Flex;
                    break;
            }
        }

        private void BindFISOField()
        {
            _fiObjectField = root.Q<ObjectField>(FI_OBJECT_FIELD_NAME);

            _fiObjectField.RegisterValueChangedCallback(x => RebindFISettings(_fiObjectField.value as ForcedInductionSO));

            if (_fiObjectField.value == null)
            {
                _inductionSO = ForcedInductionSO.CreateDefaultForcedInductionSO();
            }
            else
            {
                _inductionSO = _fiObjectField.value as ForcedInductionSO;
            }
        }
        private void RebindFISettings(ForcedInductionSO loadedFISO)
        {
            _inductionSO = loadedFISO;

            if (_inductionSO == null)
            {
                _inductionSO = ForcedInductionSO.CreateDefaultForcedInductionSO();
            }

            DisplayForcedInductionTypeField(_inductionSO.ForcedInductionType);

            SerializedObject so = new(_inductionSO);
            BindFIType(so);
            BindMaxBoostField(so);
            BindTurboLagField(so);
            BindTurboSpinSpeedField(so);
            BindAntiLagField(so);
            BindAntiLagEffectField(so);
            BindAntiLagChanceField(so);
            BindSpoolDownField(so);
        }

        public void UpdateForcedInduction(ForcedInductionSO loadedFISO)
        {
            if (loadedFISO == null)
                _fiObjectField.value = null;
            else
            {
                _inductionSO = loadedFISO;
                _fiObjectField.value = _inductionSO;
            }
        }

        private void BindFIType(SerializedObject so)
        {
            _foTypeEnum.bindingPath = nameof(_inductionSO.ForcedInductionType);
            _foTypeEnum.Bind(so);
        }
        private void BindMaxBoostField(SerializedObject so)
        {
            _maxBoostField.bindingPath = nameof(_inductionSO.MaxBoostPressure);
            _maxBoostField.Bind(so);
        }
        private void BindTurboLagField(SerializedObject so)
        {
            _turboLagSlider.bindingPath = nameof(_inductionSO.TurboRPMPercentDelay);
            _turboLagSlider.Bind(so);
        }
        private void BindTurboSpinSpeedField(SerializedObject so)
        {
            _turboSpinSpeedField.bindingPath = nameof(_inductionSO.TurboSpoolUpTime);
            _turboSpinSpeedField.Bind(so);
        }
        private void BindAntiLagField(SerializedObject so)
        {
            _antiLagSystemToggle.bindingPath = nameof(_inductionSO.AntiLagSystemInstalled);
            _antiLagSystemToggle.Bind(so);
        }
        private void BindAntiLagEffectField(SerializedObject so)
        {
            _antiLagEffect.bindingPath = nameof(_inductionSO.AntiLagEffect);
            _antiLagEffect.Bind(so);
        }
        private void BindAntiLagChanceField(SerializedObject so)
        {
            _antiLagChance.bindingPath = nameof(_inductionSO.AntiLagVisualEffectChance);
            _antiLagChance.Bind(so);
        }
        private void BindSpoolDownField(SerializedObject so)
        {
            _turboSpooldownSpeedField.bindingPath = nameof(_inductionSO.TurboSpoolDownTime);
            _turboSpooldownSpeedField.Bind(so);
        }

        private void SubscribeToFISaveButtonClick()
        {
            var button = root.Q<Button>(name: FI_CREATE_BUTTON_NAME);
            button.clicked += FICreateAssetButton_onClick;
        }

        private void FICreateAssetButton_onClick()
        {
            string folderPath = LocalPathFinder.Instance.GetVehiclePartsFolderPathForAsset(FORCED_INDUCTION_FOLDER_NAME);

            ForcedInductionSO newFI = ForcedInductionSO.CreateDefaultForcedInductionSO();

            if (AssetSaver.TrySaveAsset(folderPath, newFI, _fiNameField.text, "asset"))
            {
                _inductionSO = newFI;
                _fiObjectField.value = _inductionSO;
            }
        }

        public void BindVehicleController(SerializedProperty fiProperty)
        {
            _fiObjectField.Unbind();
            if (fiProperty != null)
                _fiObjectField.BindProperty(fiProperty);
        }

        public void BindPreset(SerializedObject preset)
        {
            _fiObjectField.Unbind();
            if (preset != null)
                _fiObjectField.BindProperty(preset.FindProperty("ForcedInduction"));
        }

        public void Unbind() => _fiObjectField.Unbind();
    }
}

