using Assets.VehicleController;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    public class ControllerSteeringSettingsEditor
    {
        private VisualElement root;
        private CustomVehicleControllerEditor _editor;

        private FloatField _steerAngleField;
        private FloatField _steerSpeedField;
        private FloatField _recenterSpeedField;

        private const string STEER_ANGLE_FIELD_NAME = "SteerAngleInput";
        private const string STEER_SPEED_FIELD_NAME = "SteerSpeedInput";
        private const string RECENTER_SPEED_FIELD_NAME = "WheelsRecenterTime";

        private float _steerAnglePlayMode;
        private float _steerSpeedPlayMode;
        private float _recenterSpeedPlayMode;

        public ControllerSteeringSettingsEditor(VisualElement root, CustomVehicleControllerEditor editor)
        {
            this.root = root;
            _editor = editor;
            FindSteeringFields();
        }


        public void PasteStats(SerializedObject controller)
        {
            controller.FindProperty("_steerAngle").floatValue = _steerAnglePlayMode;
            controller.FindProperty("_steerSpeed").floatValue = _steerSpeedPlayMode;
            controller.FindProperty("_centeringSpeed").floatValue = _recenterSpeedPlayMode;
        }

        public void CopyStats(SerializedObject controller)
        {
            _steerAnglePlayMode = controller.FindProperty("_steerAngle").floatValue;
            _steerSpeedPlayMode = controller.FindProperty("_steerSpeed").floatValue;
            _recenterSpeedPlayMode = controller.FindProperty("_centeringSpeed").floatValue;
        }

        private void FindSteeringFields()
        {
            _steerAngleField = root.Q<FloatField>(STEER_ANGLE_FIELD_NAME);
            _steerAngleField.RegisterValueChangedCallback(evt => { _steerAngleField.value = Mathf.Clamp(_steerAngleField.value, 0, 90); });
            _steerAngleField.bindingPath = "_steerAngle";

            _steerSpeedField = root.Q<FloatField>(STEER_SPEED_FIELD_NAME);
            _steerSpeedField.RegisterValueChangedCallback(evt => { _steerSpeedField.value = Mathf.Clamp(_steerSpeedField.value, 0, 100); });
            _steerSpeedField.bindingPath = "_steerSpeed";

            _recenterSpeedField = root.Q<FloatField>(RECENTER_SPEED_FIELD_NAME);
            _recenterSpeedField.RegisterValueChangedCallback(evt => { _recenterSpeedField.value = Mathf.Clamp(_recenterSpeedField.value, 0, _steerSpeedField.value); });
            _recenterSpeedField.bindingPath = "_centeringSpeed";
        }

        public void BindVehicleController(SerializedObject controller)
        {
            Unbind();
            if (controller == null)
                return;

            _steerAngleField.Bind(controller);
            _steerSpeedField.Bind(controller);
            _recenterSpeedField.Bind(controller);
        }

        public void Unbind()
        {
            _steerAngleField.Unbind();
            _steerSpeedField.Unbind();
            _recenterSpeedField.Unbind();
        }
    }
}
