using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(PackageSettingsSO))]
    public class PackageSettingsSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var script = (PackageSettingsSO)target;

            if (GUILayout.Button("Update Path Display Information", GUILayout.Height(20)))
            {
                script.UpdateDisplayInfo();
            }
            if (GUILayout.Button("Reset All Information", GUILayout.Height(20)))
            {
                script.ResetInfo();
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.DefaultSavePaths)));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.PresetPathDisplay)));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.EnginePathDisplay)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.TransmissionPathDisplay)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.FIPathDisplay)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.SuspensionPathDisplay)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.TiresPathDisplay)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.BrakesPathDisplay)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.BodiesPathDisplay)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.NitroPathDisplay)));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.EnginePartsPathDisplay)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(script.CollisionAreasPathDisplay)));

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
