using Assets.VehicleController;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.Text;

namespace Assets.VehicleControllerEditor
{
    public class ControllerTransmissionSettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _mainEditor;

        #region Transmission
        private TransmissionSO _transmissionSO;
        private TextField _transmissionTextField;
        private Slider _transmissionFinalDriveField;
        private FloatField _transmissionShiftCDField;

        private Slider _upshiftSlider;
        private Slider _downshiftSlider;

        private ObjectField _transmissionSOObjectField;

        private ListView _gearRatiosSlidersListView;

        private const string TRANSMISSION_GEAR_RATIOS_SLIDERS_LIST_NAME = "GearRatiosSlidersList";
        private const string TRANSMISSION_TEXT_FIELD_NAME = "TransmissionAssetName";
        private const string TRANSMISSION_FINAL_DRIVE_FIELD_NAME = "FinalDriveInput";
        private const string TRANSMISSION_SHIFT_CD_NAME = "ShiftCooldownInput";
        private const string TRANSMISSION_CREATE_BUTTON_NAME = "CreateTransmissionAssetButton";
        private const string TRANSMISSION_SO_NAME = "TransmissionSOField";

        private const string TRANSMISSION_UPSHIFT_SLIDER_NAME = "UpshiftRPMSlider";
        private const string TRANSMISSION_DONWSHIFT_SLIDER_NAME = "DownshiftRPMSlider";

        private const string CALCULATE_BUTTON_NAME = "CulculateGearRatiosButton";
        #endregion

        private ControllerAutoGearCalculatorEditor _calculator;

        public const string TRANSMISSION_FOLDER_NAME = "Transmissions";

        private const float MIN_RPM_DIFFERENCE = 0.05f;

        public ControllerTransmissionSettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _mainEditor = editor;

            FindTransmissionFields();

            BindTransmissionSOField();
            SubscribeToTransmissionSaveButtonClick();

            _mainEditor.OnWindowClosed += _mainEditor_OnWindowClosed;
            SetTooltips();

            _calculator = new ControllerAutoGearCalculatorEditor(root);
        }

        private void SetTooltips()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Gear ratios of the transmission. The reverse gear has the same ratio as the first gear.");
            sb.AppendLine();
            sb.AppendLine("The higher the ratio the more torque the gear produces, but the top speed gets lower.");
            sb.AppendLine();
            sb.AppendLine("Gear ratios must decrease as the gear number increases.");


            _gearRatiosSlidersListView.tooltip = sb.ToString();
        }

        private void _mainEditor_OnWindowClosed()
        {
            root.Q<Button>(name: TRANSMISSION_CREATE_BUTTON_NAME).clicked -= TransmissionCreateAssetButton_onClick;
            root.Q<Button>(name: CALCULATE_BUTTON_NAME).clicked -= CalculateGearRatiosArray;
            _mainEditor.OnWindowClosed -= _mainEditor_OnWindowClosed;
        }

        private void MakeGearList()
        {
            // The ListView calls this to add visible items to the scroller.
            Func<VisualElement> makeItem = () =>
            {
                var gearInfoVisualElement = new GearInfoVisualElement();
                var slider = gearInfoVisualElement.Q<Slider>(name: "Ratio");

                var name = gearInfoVisualElement.Q<Label>(name: "nameLabel");
                slider.RegisterValueChangedCallback(evt =>
                {
                    var i = (int)slider.userData;
                    slider.value = evt.newValue;
                    _transmissionSO.GearRatiosList[i] = evt.newValue;
                });
                return gearInfoVisualElement;
            };

            Action<VisualElement, int> bindItem = (e, i) => BindItem(e as GearInfoVisualElement, i);

            int itemHeight = 27;
            _gearRatiosSlidersListView = root.Q<ListView>(TRANSMISSION_GEAR_RATIOS_SLIDERS_LIST_NAME);
            _gearRatiosSlidersListView.fixedItemHeight = itemHeight;
            _gearRatiosSlidersListView.makeItem = makeItem;
            _gearRatiosSlidersListView.bindItem = bindItem;

            _gearRatiosSlidersListView.reorderable = false;
            _gearRatiosSlidersListView.style.flexGrow = 1f;
            _gearRatiosSlidersListView.showBorder = true;
            _gearRatiosSlidersListView.itemsAdded += _gearRatiosSlidersListView_itemsAdded;
        }

        private void _gearRatiosSlidersListView_itemsAdded(IEnumerable<int> obj)
        {
            int indexLast = obj.Last();
            if (indexLast == 0)
            {
                _transmissionSO.GearRatiosList[indexLast] = 3.45f;
                return;
            }
            float changeRate = _calculator.GetChangeRate();

            if (changeRate == 0)
                _transmissionSO.GearRatiosList[indexLast] = _transmissionSO.GearRatiosList[indexLast - 1];
            else
                _transmissionSO.GearRatiosList[indexLast] = _transmissionSO.GearRatiosList[0] * Mathf.Exp(_calculator.GetChangeRate() * indexLast);   
        }

        private void FindTransmissionFields()
        {
            _transmissionTextField = root.Q<TextField>(name: TRANSMISSION_TEXT_FIELD_NAME);
            _transmissionFinalDriveField = root.Q<Slider>(name: TRANSMISSION_FINAL_DRIVE_FIELD_NAME);
            _transmissionFinalDriveField.RegisterValueChangedCallback(evt => { _transmissionFinalDriveField.value = Mathf.Max(0.1f, _transmissionFinalDriveField.value); });
            _transmissionShiftCDField = root.Q<FloatField>(name: TRANSMISSION_SHIFT_CD_NAME);
            _transmissionShiftCDField.RegisterValueChangedCallback(evt => { _transmissionShiftCDField.value = Mathf.Max(0f, _transmissionShiftCDField.value); });

            MakeGearList();

            _upshiftSlider = root.Q<Slider>(TRANSMISSION_UPSHIFT_SLIDER_NAME);
            _upshiftSlider.RegisterValueChangedCallback(evt =>
            {
                if (_downshiftSlider.value > _upshiftSlider.value - MIN_RPM_DIFFERENCE)
                {
                    _downshiftSlider.value = _upshiftSlider.value - MIN_RPM_DIFFERENCE;
                }
            });
            _downshiftSlider = root.Q<Slider>(TRANSMISSION_DONWSHIFT_SLIDER_NAME);
            _downshiftSlider.RegisterValueChangedCallback(evt => { _downshiftSlider.value = Mathf.Clamp(_downshiftSlider.value, 0, _upshiftSlider.value - MIN_RPM_DIFFERENCE); });
        }

        private void BindTransmissionSOField()
        {
            _transmissionSOObjectField = root.Q<ObjectField>(TRANSMISSION_SO_NAME);

            _transmissionSOObjectField.RegisterValueChangedCallback(x => RebindTransmissionSettings(_transmissionSOObjectField.value as TransmissionSO));

            if (_transmissionSOObjectField.value == null)
                _transmissionSO = TransmissionSO.CreateDefaultTransmissionSO();
            else
                _transmissionSO = _transmissionSOObjectField.value as TransmissionSO;
        }

        private void RebindTransmissionSettings(TransmissionSO loadedTransmissionSO)
        {
            _transmissionSO = loadedTransmissionSO;

            if (_transmissionSO == null)
                _transmissionSO = TransmissionSO.CreateDefaultTransmissionSO();

            SerializedObject so = new (_transmissionSO);
            BindFinalDrive(so);
            BindShiftCD(so);
            BindUpshiftSlider(so);
            BindDownshiftSlider(so);
            BindGearList(so);
        }

        private void BindFinalDrive(SerializedObject so)
        {
            _transmissionFinalDriveField.bindingPath = nameof(_transmissionSO.FinalDriveRatio);
            _transmissionFinalDriveField.Bind(so);
        }
        private void BindShiftCD(SerializedObject so)
        {
            _transmissionShiftCDField.bindingPath = nameof(_transmissionSO.ShiftCooldown);
            _transmissionShiftCDField.Bind(so);
        }
        private void BindUpshiftSlider(SerializedObject so)
        {
            _upshiftSlider.bindingPath = nameof(_transmissionSO.UpShiftRPMPercent);
            _upshiftSlider.Bind(so);
        }
        private void BindDownshiftSlider(SerializedObject so)
        {
            _downshiftSlider.bindingPath = nameof(_transmissionSO.DownShiftRPMPercent);
            _downshiftSlider.Bind(so);
        }
        private void BindGearList(SerializedObject so)
        {
            _gearRatiosSlidersListView.bindingPath = nameof(_transmissionSO.GearRatiosList);
            _gearRatiosSlidersListView.Bind(so);     
        }

        private void SubscribeToTransmissionSaveButtonClick()
        {
            root.Q<Button>(name: TRANSMISSION_CREATE_BUTTON_NAME).clicked += TransmissionCreateAssetButton_onClick;
            root.Q<Button>(name: CALCULATE_BUTTON_NAME).clicked += CalculateGearRatiosArray;
        }

        private void CalculateGearRatiosArray()
        {
            _transmissionSO.GearRatiosList = _calculator.CalculateGearArray().ToList();
            _gearRatiosSlidersListView.Rebuild();
        }

        private void TransmissionCreateAssetButton_onClick()
        {
            string folderPath = LocalPathFinder.Instance.GetVehiclePartsFolderPathForAsset(TRANSMISSION_FOLDER_NAME);

            TransmissionSO newTransmission = TransmissionSO.CreateDefaultTransmissionSO();

            if (AssetSaver.TrySaveAsset(folderPath, newTransmission, _transmissionTextField.text, "asset"))
            {
                _transmissionSO = newTransmission;
                _transmissionSOObjectField.value = _transmissionSO;
            }
        }

        public void BindVehicleController(SerializedProperty transmissionProperty)
        {
            _transmissionSOObjectField.Unbind();
            if(transmissionProperty != null)
                _transmissionSOObjectField.BindProperty(transmissionProperty);
        }

        public void BindPreset(SerializedObject preset)
        {
            _transmissionSOObjectField.Unbind();
            if(preset != null)
                _transmissionSOObjectField.BindProperty(preset.FindProperty("Transmission"));
        }

        public void Unbind() => _transmissionSOObjectField.Unbind();

        public class GearRatioVisualElement : VisualElement
        {
            public GearRatioVisualElement()
            {
                var root = new VisualElement();

                var gearSlider = new Slider { name = "Ratio", lowValue = 0.1f, highValue = 10f };
                gearSlider.style.flexGrow = 1f;
                Add(root);
            }
        }

        private class GearInfoVisualElement : VisualElement
        {
            public GearInfoVisualElement()
            {
                var root = new VisualElement();
                root.style.flexDirection = FlexDirection.Row;

                root.style.marginBottom = 3f;
                root.style.paddingBottom = 3f;
                root.style.paddingLeft = 3f;

                root.style.borderBottomColor = Color.gray;
                root.style.borderBottomWidth = 2f;
                var nameLabel = new Label() { name = "nameLabel" };
                nameLabel.style.fontSize = 14f;
                nameLabel.style.width = 75f;
                var gearContainer = new VisualElement();
                gearContainer.style.flexDirection = FlexDirection.Row;
                gearContainer.style.flexGrow = 1;
                gearContainer.style.paddingLeft = 15f;
                gearContainer.style.paddingRight = 15f;

                var gearRatioSlider = new Slider { name = "Ratio", lowValue = 0.1f, highValue = 10 };
                gearRatioSlider.showInputField = true;
                gearRatioSlider.style.fontSize = 14;
                gearRatioSlider.style.flexGrow = 1f;
                gearContainer.Add(gearRatioSlider);

                root.Add(nameLabel);
                root.Add(gearContainer);
                Add(root);
            }
        }

        private void BindItem(GearInfoVisualElement elem, int i)
        {
            if (i >= _transmissionSO.GearRatiosList.Count)
                return;

            var label = elem.Q<Label>(name: "nameLabel");
            var slider = elem.Q<Slider>(name: "Ratio");
            slider.userData = i;
            label.text = GetGearName(i);
            slider.value = _transmissionSO.GearRatiosList[i];
        }

        private string GetGearName(int i)
        {
            if (i == 0)
                return "Gear 1 / R";

            return $"Gear {i + 1}";
        }
    }

}
