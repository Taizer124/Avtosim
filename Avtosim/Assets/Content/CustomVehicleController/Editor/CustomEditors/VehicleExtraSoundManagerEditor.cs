using Assets.VehicleController;
using UnityEditor;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(CustomVehicleControllerExtraSoundManager))]
    public class VehicleExtraSoundManagerEditor : Editor
    {
        private SerializedProperty _vehicleController;

        private SerializedProperty _extraSoundSO;
        private CarExtraSoundsSO _extraSound;

        private SerializedProperty _forcedInductionSoundSO;
        private CarForcedInductionSoundSO _forcedInductionSound;

        private void OnEnable()
        {
            _vehicleController = serializedObject.FindProperty("_vehicleController");

            _extraSoundSO = serializedObject.FindProperty("_extraSoundSO");
            _extraSound = _extraSoundSO.objectReferenceValue as CarExtraSoundsSO;

            _forcedInductionSoundSO = serializedObject.FindProperty("_forcedInductionSoundSO");
            _forcedInductionSound = _forcedInductionSoundSO.objectReferenceValue as CarForcedInductionSoundSO;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_vehicleController);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_collisionHandler"));
            EditorGUILayout.PropertyField(_forcedInductionSoundSO);
            EditorGUILayout.PropertyField(_extraSoundSO);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_vehicleSoundAudioMixerGroup"));

            if(_forcedInductionSound != null)
            {
                if (_forcedInductionSound.AntiLagMildSounds.Length != 0 || _forcedInductionSound.AntiLagSound.Length != 0)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_antiLagSoundCooldown"));

                if (_forcedInductionSound.ForcedInductionSound.length != 0)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_forcedInductionMaxPitch"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_forcedInductionMaxVolume"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_turboFlutterVolumeMultiplier"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_antiLagVolumeMultiplier"));
                }
            }

            if (_extraSound != null)
            {
                if(_extraSound.TireSlipSound != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_tireVolumeIncreaseTime"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxTireSlipVolume"));
                }

                if (_extraSound.WindNoise != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxWindVolume"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_speedForMaxWindVolume"));
                }

                if (serializedObject.FindProperty("_collisionHandler").objectReferenceValue != null)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_collisionSoundParameters"));

                SerializedProperty _reverbZone = serializedObject.FindProperty("_reverbZone");

                if (_extraSound.NitroContinuous != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_nitroVolumeGainSpeedInSeconds"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_nitroMaxVolume"));
                    EditorGUILayout.PropertyField(_reverbZone);
                }

                

                if (_reverbZone.objectReferenceValue != null)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_reverbDuringNitroPreset"));
            }

            _forcedInductionSound = _forcedInductionSoundSO.objectReferenceValue as CarForcedInductionSoundSO;
            _extraSound = _extraSoundSO.objectReferenceValue as CarExtraSoundsSO;

            EditorGUILayout.Space();
            SerializedProperty _3DSound = serializedObject.FindProperty("_3DSound");

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
        }
    }
}
