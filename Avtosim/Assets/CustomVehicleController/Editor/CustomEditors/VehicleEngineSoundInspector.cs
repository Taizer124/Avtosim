using Assets.VehicleController;
using UnityEditor;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(VehicleEngineSoundManager))]
    public class VehicleEngineSoundInspector : Editor
    {
        private SerializedProperty _3DSound;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_vehicleController"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_engineSoundsSO"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_vehicleSoundAudioMixerGroup"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EngineSoundPitch"));

            EditorGUILayout.Space();
            _3DSound = serializedObject.FindProperty("_3DSound");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("EngineModificationsAffectPitch"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_optimizeAudioPerformance"));
            EditorGUILayout.PropertyField(_3DSound);

            if (_3DSound.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_spatialBlend"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_dopplerLevel"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_spread"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_volumeRolloff"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_minDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxDistance"));
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
