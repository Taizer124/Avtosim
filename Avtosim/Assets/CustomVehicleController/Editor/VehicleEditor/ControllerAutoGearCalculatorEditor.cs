using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Text;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    public class ControllerAutoGearCalculatorEditor
    {
        private VisualElement root;

        #region Auto Gear Ratio Calculator
        private Foldout _autoCalculateFoldout;
        private Slider _gearRatioChangeRateSlider;
        private FloatField _firstGearRatioField;
        private IntegerField _gearAmountField;

        private const string FOLDOUT_NAME = "AutoCalculateGearsFoldout";
        private const string GEAR_RATIO_CHANGE_SLIDER_NAME = "GearRatioChangeRateSlider";
        private const string FIRST_GEAR_FIELD_NAME = "GearRatioChangeRateSlider";
        private const string GEAR_AMOUNT_FIELD_NAME = "GearAmountField";
        #endregion

        public ControllerAutoGearCalculatorEditor(VisualElement _root)
        {
            root = _root;
            FindFields();
        }


        private void FindFields()
        {
            _autoCalculateFoldout = root.Q<Foldout>(FOLDOUT_NAME);

            _gearRatioChangeRateSlider = root.Q<Slider>(GEAR_RATIO_CHANGE_SLIDER_NAME);
            _firstGearRatioField = root.Q<FloatField>(FIRST_GEAR_FIELD_NAME);
            _firstGearRatioField.RegisterValueChangedCallback(evt => {
                _firstGearRatioField.value =
                evt.newValue < 1 ? 1 : evt.newValue; });
            _gearAmountField = root.Q<IntegerField>(GEAR_AMOUNT_FIELD_NAME);
            _gearAmountField.RegisterValueChangedCallback(evt =>
            {
                _gearAmountField.value = evt.newValue < 1? 1 : evt.newValue;
            });

            SetTooltip();
        }

        private void SetTooltip()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Controls the rate of change in gear ratios.");
            sb.AppendLine("");
            sb.AppendLine("A more negative value results in a faster decrease in gear ratios as the index increases.");
            sb.AppendLine("");
            sb.AppendLine("Recommended value [-0.25: -0.3]");

            _gearRatioChangeRateSlider.tooltip = sb.ToString();
        }

        public float[] CalculateGearArray()
        {
            int size = _gearAmountField.value;
            float[] array = new float[size];

            float changeRate = _gearRatioChangeRateSlider.value;

            float firstGearRatio = _firstGearRatioField.value;

            array[0] = _firstGearRatioField.value;

            for(int i = 1; i < size; i++)
            {
                array[i] = firstGearRatio * Mathf.Exp(changeRate * i);
            }

            return array;
        }

        public float GetChangeRate() => _autoCalculateFoldout.value ? _gearRatioChangeRateSlider.value : 0;
    }
}
