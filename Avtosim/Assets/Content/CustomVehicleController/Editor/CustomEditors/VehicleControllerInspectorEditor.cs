using Assets.VehicleController;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(CustomVehicleController))]
    public class VehicleControllerInspectorEditor : Editor
    {
        private SerializedProperty UsePreset;
        private SerializedProperty _vehiclePartsPreset;
        private SerializedProperty _customizableSet;

        private void OnEnable()
        {
            UsePreset = serializedObject.FindProperty("UsePreset");
            _vehiclePartsPreset = serializedObject.FindProperty("_vehiclePartsPreset");
            _customizableSet = serializedObject.FindProperty("_customizableSet");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(UsePreset);

            if (UsePreset.boolValue)
                EditorGUILayout.PropertyField(_vehiclePartsPreset);
            else
                EditorGUILayout.PropertyField(_customizableSet);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_enginePartsContainer"));

            if (GUILayout.Button("Change Engine Parts"))
                EnginePartCreatorWindow.OpenWindow();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("DrivetrainType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TransmissionType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_steerAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_steerSpeed"));
            serializedObject.FindProperty("_centeringSpeed").floatValue = Math.Clamp(serializedObject.FindProperty("_centeringSpeed").floatValue, 0, serializedObject.FindProperty("_steerSpeed").floatValue);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_centeringSpeed"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_forwardSlippingThreshold"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_sidewaysSlippingThreshold"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_tcsEnabled"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_absEnabled"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("AerialControlsEnabled"));
            if (serializedObject.FindProperty("AerialControlsEnabled").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AerialControlsSensitivity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RecoveryHelp"));
                if (serializedObject.FindProperty("RecoveryHelp").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CenterOfMassOffset"));
            }


            EditorGUILayout.PropertyField(serializedObject.FindProperty("_rigidbody"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_centerOfMass"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_suspensionSimulationPrecision"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_ignoreLayers"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_frontAxles"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_rearAxles"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_steerAxles"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentCarStats"));

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
