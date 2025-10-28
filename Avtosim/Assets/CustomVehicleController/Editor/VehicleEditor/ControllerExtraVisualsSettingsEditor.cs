using Assets.VehicleController;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


namespace Assets.VehicleControllerEditor
{
    public class ControllerExtraVisualsSettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _editor;

        #region fields
        private FloatField _forwardSlipField;
        private FloatField _sidewaysSlipField;

        private Toggle _aerialControlsToggle;
        private FloatField _aerialSensitivityField;
        #endregion

        #region field names
        private const string FORWARD_SLIP_FIELD_NAME = "ForwardSlipThresholdField";
        private const string SIDEWAYS_SLIP_FIELD_NAME = "SidewaysSlipThresholdField";
        private const string AERIAL_CONTROLS_TOGGLE_NAME = "AerialControlsToggle";
        private const string AERIAL_CONTROLS_SENSITIVITY_NAME = "AerialControlsSensitivityField";
        #endregion

        #region values changed during play mode

        private float _forwardSlipPlayMode;
        private float _sidewaysSlipPlayMode;

        private bool _aerialControlsPlayMode;
        private float _aerialSensitivityPlayMode;
        #endregion

        public ControllerExtraVisualsSettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _editor = editor;
            FindFields();
        }
        public void PasteStats(SerializedObject controller)
        {
            controller.FindProperty("_forwardSlippingThreshold").floatValue = _forwardSlipPlayMode;
            controller.FindProperty("_sidewaysSlippingThreshold").floatValue = _sidewaysSlipPlayMode;
            controller.FindProperty(nameof(CustomVehicleController.AerialControlsEnabled)).boolValue = _aerialControlsPlayMode;
            controller.FindProperty(nameof(CustomVehicleController.AerialControlsSensitivity)).floatValue = _aerialSensitivityPlayMode;
        }

        public void CopyStats(SerializedObject serializedObject)
        {
            _forwardSlipPlayMode = serializedObject.FindProperty("_forwardSlippingThreshold").floatValue;
            _sidewaysSlipPlayMode = serializedObject.FindProperty("_sidewaysSlippingThreshold").floatValue;

            _aerialControlsPlayMode = serializedObject.FindProperty(nameof(CustomVehicleController.AerialControlsEnabled)).boolValue;
            _aerialSensitivityPlayMode = serializedObject.FindProperty(nameof(CustomVehicleController.AerialControlsSensitivity)).floatValue;
        }

        private void FindFields()
        {
            _forwardSlipField = root.Q<FloatField>(FORWARD_SLIP_FIELD_NAME);
            _forwardSlipField.RegisterValueChangedCallback(evt =>
            {
                _forwardSlipField.value = Mathf.Clamp(_forwardSlipField.value, 0, 100);
                SerializedObject serializedObject = _editor.GetSerializedController();
                if (serializedObject != null)
                {
                    serializedObject.FindProperty("_forwardSlippingThreshold").floatValue = _forwardSlipField.value;
                    _editor.SaveController();
                }
            });

            _sidewaysSlipField = root.Q<FloatField>(SIDEWAYS_SLIP_FIELD_NAME);
            _sidewaysSlipField.RegisterValueChangedCallback(evt =>
            {
                _sidewaysSlipField.value = Mathf.Clamp(_sidewaysSlipField.value, 0, 1);
                SerializedObject serializedObject = _editor.GetSerializedController();
                if (serializedObject != null)
                {
                    serializedObject.FindProperty("_sidewaysSlippingThreshold").floatValue = _sidewaysSlipField.value;
                    _editor.SaveController();
                }
            });

            _aerialControlsToggle = root.Q<Toggle>(AERIAL_CONTROLS_TOGGLE_NAME);
            _aerialControlsToggle.RegisterValueChangedCallback(evt =>
            {
                _aerialSensitivityField.style.display = _aerialControlsToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
                SerializedObject serializedObject = _editor.GetSerializedController();
                if (serializedObject != null)
                {
                    serializedObject.FindProperty(nameof(CustomVehicleController.AerialControlsEnabled)).boolValue = _aerialControlsToggle.value;
                    _editor.SaveController();
                }
            });

            _aerialSensitivityField = root.Q<FloatField>(AERIAL_CONTROLS_SENSITIVITY_NAME);
            _aerialSensitivityField.RegisterValueChangedCallback(evt =>
            {
                SerializedObject serializedObject = _editor.GetSerializedController();
                if (serializedObject != null)
                {
                    serializedObject.FindProperty(nameof(CustomVehicleController.AerialControlsSensitivity)).floatValue = _aerialSensitivityField.value;
                    _editor.SaveController();
                }
            });
        }

        public void SetVehicleController(SerializedObject vehicleController)
        {
            if (vehicleController == null)
                return;

            if (vehicleController.FindProperty(nameof(CustomVehicleController.AerialControlsEnabled)) == null)
                return;

            _aerialControlsToggle.value = vehicleController == null ? true : vehicleController.FindProperty(nameof(CustomVehicleController.AerialControlsEnabled)).boolValue;
            _aerialSensitivityField.value = vehicleController == null ? 7500 : vehicleController.FindProperty(nameof(CustomVehicleController.AerialControlsSensitivity)).floatValue;

            _forwardSlipField.value = vehicleController == null ? 0.1f : vehicleController.FindProperty("_forwardSlippingThreshold").floatValue;
            _sidewaysSlipField.value = vehicleController == null ? 0.3f : vehicleController.FindProperty("_sidewaysSlippingThreshold").floatValue;
        }
    }

}
