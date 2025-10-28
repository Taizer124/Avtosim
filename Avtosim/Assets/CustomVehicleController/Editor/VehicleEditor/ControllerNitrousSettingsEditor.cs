using Assets.VehicleController;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class ControllerNitrousSettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        #region NITRO fields
        private NitrousSO _nitroSO;

        private ObjectField _nitroObjectField;
        private FloatField _boostAmountField;
        private IntegerField _bottlesAmountField;
        private FloatField _boostIntensityField;
        private FloatField _boostWarmUpField;
        private Toggle _boostDuringWarmUpToggle;
        private FloatField _rechargeRateField;
        private FloatField _rechargeDelayField;
        private Slider _nitroMinUse;
        private EnumField _boostTypeEnum;
        private TextField _nitroNameField;
        #endregion

        #region NITRO field names
        private const string NITRO_OBJECT_FIELD = "NitrousSOField";
        private const string NITRO_AMOUNT_FIELD = "NitroBoostAmount";
        private const string NITRO_BOTTLES_AMOUNT_FIELD = "NitroBottlesAmount";
        private const string NITRO_INTENSITY_FIELD = "NitroBoostIntensity";
        private const string NITRO_WARM_UP_FIELD = "NitrousBoostWarmup";
        private const string NITRO_BOOST_DURING_WARMUP_TOGGLE = "BoostDuringWarmupToggle";
        private const string NITRO_RECHARGE_FIELD = "NitroRechargeRate";
        private const string NITRO_DELAY_FIELD = "NitroRechargeDelay";
        private const string NITRO_MIN_USE_FIELD = "NitroMinUse";
        private const string NITRO_TYPE_FIELD = "NitroBoostType";


        private const string NITRO_NAME_FIELD = "NitrousAssetName";
        private const string NITRO_CREATE_BUTTON_NAME = "CreateNitrousAssetButton";
        #endregion

        public const string NITRO_FOLDER_NAME = "Nitrous";

        public ControllerNitrousSettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;
            FindNitroFields();
            BindNitroSOField();
            SubscribeToNitroSaveButtonClick();
            _mainEditor.OnWindowClosed += Editor_OnWindowClosed;
        }

        private void Editor_OnWindowClosed()
        {
            var button = root.Q<Button>(name: NITRO_CREATE_BUTTON_NAME);
            button.clicked -= NitroCreateAssetButton_onClick;
            _mainEditor.OnWindowClosed -= Editor_OnWindowClosed;
        }

        private void FindNitroFields()
        {
            _boostAmountField = root.Q<FloatField>(NITRO_AMOUNT_FIELD);
            _boostAmountField.RegisterValueChangedCallback(evt => { _boostAmountField.value = Mathf.Max(0, _boostAmountField.value); });

            _bottlesAmountField = root.Q<IntegerField>(NITRO_BOTTLES_AMOUNT_FIELD);
            _bottlesAmountField.RegisterValueChangedCallback(evt => { _bottlesAmountField.value = Mathf.Max(1, evt.newValue); });

            _boostIntensityField = root.Q<FloatField>(NITRO_INTENSITY_FIELD);
            _boostIntensityField.RegisterValueChangedCallback(evt => { _boostIntensityField.value = Mathf.Max(0, _boostIntensityField.value); });

            _boostWarmUpField = root.Q<FloatField>(NITRO_WARM_UP_FIELD);
            _boostWarmUpField.RegisterValueChangedCallback(evt => { _boostWarmUpField.value = Mathf.Max(0, evt.newValue); });

            _boostDuringWarmUpToggle = root.Q<Toggle>(NITRO_BOOST_DURING_WARMUP_TOGGLE);

            _rechargeRateField = root.Q<FloatField>(NITRO_RECHARGE_FIELD);
            _rechargeRateField.RegisterValueChangedCallback(evt => { _rechargeRateField.value = Mathf.Max(0, _rechargeRateField.value); });

            _rechargeDelayField = root.Q<FloatField>(NITRO_DELAY_FIELD);
            _rechargeDelayField.RegisterValueChangedCallback(evt => { _rechargeDelayField.value = Mathf.Max(0, _rechargeDelayField.value); });

            _nitroMinUse = root.Q<Slider>(NITRO_MIN_USE_FIELD);

            _boostTypeEnum = root.Q<EnumField>(NITRO_TYPE_FIELD);

            _nitroNameField = root.Q<TextField>(NITRO_NAME_FIELD);
        }

        private void BindNitroSOField()
        {
            _nitroObjectField = root.Q<ObjectField>(NITRO_OBJECT_FIELD);

            _nitroObjectField.RegisterValueChangedCallback(x => RebindNitroSettings(_nitroObjectField.value as NitrousSO));

            if (_nitroObjectField.value == null)
                _nitroSO = NitrousSO.CreateDefaultNitroSO();
            else
                _nitroSO = _nitroObjectField.value as NitrousSO;
        }
        private void RebindNitroSettings(NitrousSO loadedNitroSO)
        {
            _nitroSO = loadedNitroSO;


            if (_nitroSO == null)
                _nitroSO = NitrousSO.CreateDefaultNitroSO();

            SerializedObject so = new(_nitroSO);
            BindBoostAmountFields(so);
            BindRechargeRateField(so);
            BindRechargeDelayField(so);
            BindBoostIntensityField(so);
            BindBoostTypeField(so);
            BindNitroMinUseField(so);
            BindBottlesAmountField(so);
            BindWarmUpField(so);
            BindBoostDuringWarmupToggle(so);
        }

        private void BindBoostAmountFields(SerializedObject so)
        {
            _boostAmountField.bindingPath = nameof(_nitroSO.BoostAmount);
            _boostAmountField.Bind(so);
        }
        private void BindRechargeRateField(SerializedObject so)
        {
            _rechargeRateField.bindingPath = nameof(_nitroSO.RechargeRate);
            _rechargeRateField.Bind(so);
        }
        private void BindRechargeDelayField(SerializedObject so)
        {
            _rechargeDelayField.bindingPath = nameof(_nitroSO.RechargeDelay);
            _rechargeDelayField.Bind(so);
        }
        private void BindBoostIntensityField(SerializedObject so)
        {
            _boostIntensityField.bindingPath = nameof(_nitroSO.BoostIntensity);
            _boostIntensityField.Bind(so);
        }

        private void BindBoostDuringWarmupToggle(SerializedObject so)
        {
            _boostDuringWarmUpToggle.bindingPath = nameof(_nitroSO.BoostDuringWarmUp);
            _boostDuringWarmUpToggle.Bind(so);
        }

        private void BindBoostTypeField(SerializedObject so)
        {
            _boostTypeEnum.bindingPath = nameof(_nitroSO.BoostType);
            _boostTypeEnum.Bind(so);
        }
        private void BindBottlesAmountField(SerializedObject so)
        {
            _bottlesAmountField.bindingPath = nameof(_nitroSO.BottlesAmount);
            _bottlesAmountField.Bind(so);
        }
        private void BindWarmUpField(SerializedObject so)
        {
            _boostWarmUpField.bindingPath = nameof(_nitroSO.BoostWarmUpTime);
            _boostWarmUpField.Bind(so);
        }

        private void BindNitroMinUseField(SerializedObject so)
        {
            _nitroMinUse.bindingPath = nameof(_nitroSO.MinAmountPercentToUse);
            _nitroMinUse.Bind(so);
        }

        private void SubscribeToNitroSaveButtonClick()
        {
            var button = root.Q<Button>(name: NITRO_CREATE_BUTTON_NAME);
            button.clicked += NitroCreateAssetButton_onClick;
        }

        private void NitroCreateAssetButton_onClick()
        {
            string folderPath = LocalPathFinder.Instance.GetVehiclePartsFolderPathForAsset(NITRO_FOLDER_NAME);

            NitrousSO newNitro = NitrousSO.CreateDefaultNitroSO();

            if (AssetSaver.TrySaveAsset(folderPath, newNitro, _nitroNameField.text, "asset"))
            {
                _nitroSO = newNitro;
                _nitroObjectField.value = _nitroSO;
            }
        }

        public void BindVehicleController(SerializedProperty nitrousProperty)
        {
            _nitroObjectField.Unbind();
            if(nitrousProperty != null)
                _nitroObjectField.BindProperty(nitrousProperty);
        }

        public void BindPreset(SerializedObject preset)
        {
            _nitroObjectField.Unbind();
            if(preset != null)
                _nitroObjectField.BindProperty(preset.FindProperty("Nitrous"));
        }

        public void Unbind() => _nitroObjectField.Unbind();
    }
}
