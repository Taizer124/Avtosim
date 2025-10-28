using UnityEngine;
using UnityEditor;
using Assets.VehicleController;

namespace Assets.VehicleControllerEditor
{
    [CustomEditor(typeof(CarVisualsExtra))]
    public class CarVisualsExtraEditor : Editor
    {
        #region smoke
        private SerializedProperty EnableTireSmoke;
        private SerializedProperty _tireSmokeParameters;
        #endregion
        #region trails
        private SerializedProperty EnableTireTrails;
        private SerializedProperty _tireTrailParameters;
        #endregion
        #region brake lights
        private SerializedProperty EnableBrakeLightsEffect;
        private SerializedProperty _brakeLightsParameters;
        #endregion
        #region body aero
        private SerializedProperty EnableBodyAeroEffect;
        private SerializedProperty _bodyEffectParameters;
        #endregion
        #region wing aero
        private SerializedProperty EnableWingAeroEffect;
        private SerializedProperty _wingAeroParameters;
        #endregion
        #region anti lag
        private SerializedProperty EnableAntiLagEffect;
        private SerializedProperty _antiLagParameters;
        private SerializedProperty _antiLagMinCount;
        private SerializedProperty _antiLagMaxCount;
        #endregion
        #region
        private SerializedProperty EnableBrakeDisksGlowEffect;
        private SerializedProperty _brakeDisksGlowParameters;
        #endregion
        #region nitro
        private SerializedProperty EnableNitroEffect;
        private SerializedProperty _nitroParameters;
        #endregion

        private void OnEnable()
        {
            FindTireSmoke();
            FindTireTrail();
            FindBrakeLights();
            FindBrakeDisksGlow();
            FindBodyAero();
            FindWingAero();
            FindAntiLagProperties();
            FindNitro();
        }


        private void FindTireSmoke()
        {
            EnableTireSmoke = serializedObject.FindProperty("EnableTireSmoke");
            _tireSmokeParameters = serializedObject.FindProperty("_tireSmokeParameters");
        }

        private void FindTireTrail()
        {
            EnableTireTrails = serializedObject.FindProperty("EnableTireTrails");
            _tireTrailParameters = serializedObject.FindProperty("_tireTrailParameters");
        }

        private void FindBrakeLights()
        {
            EnableBrakeLightsEffect = serializedObject.FindProperty("EnableBrakeLightsEffect");
            _brakeLightsParameters = serializedObject.FindProperty("_brakeLightsParameters");
        }

        private void FindBrakeDisksGlow()
        {
            EnableBrakeDisksGlowEffect = serializedObject.FindProperty("EnableBrakeDisksGlowEffect");
            _brakeDisksGlowParameters = serializedObject.FindProperty("_brakeDisksGlowParameters");
        }

        private void FindWingAero()
        {
            EnableWingAeroEffect = serializedObject.FindProperty("EnableWingAeroEffect");
            _wingAeroParameters = serializedObject.FindProperty("_wingAeroParameters");
        }

        private void FindBodyAero()
        {
            EnableBodyAeroEffect = serializedObject.FindProperty("EnableBodyAeroEffect");
            _bodyEffectParameters = serializedObject.FindProperty("_bodyEffectParameters");
        }

        private void FindNitro()
        {
            EnableNitroEffect = serializedObject.FindProperty("EnableNitroEffect");
            _nitroParameters = serializedObject.FindProperty("_nitroParameters"); 
        }

        private void FindAntiLagProperties()
        {
            EnableAntiLagEffect = serializedObject.FindProperty("EnableAntiLagEffect");
            _antiLagParameters = serializedObject.FindProperty("_antiLagParameters");
            _antiLagMinCount = _antiLagParameters.FindPropertyRelative(nameof(AntiLagParameters.MinBackfireCount));
            _antiLagMaxCount = _antiLagParameters.FindPropertyRelative(nameof(AntiLagParameters.MaxBackfireCount));
        }

        public override void OnInspectorGUI()
        {
            CarVisualsExtra carVisualsExtra = (CarVisualsExtra)target;
            if (carVisualsExtra == null)
                return;

            if (GUILayout.Button("Copy values from CarVisualsEssentials script"))
            {
                carVisualsExtra.CopyValuesFromEssentials();
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_carVisualsEssentials"));
            
            DefaultInspector();

            DrawEffectSettings(EnableTireSmoke, _tireSmokeParameters);
            DrawEffectSettings(EnableTireTrails, _tireTrailParameters);
            DrawEffectSettings(EnableBrakeLightsEffect, _brakeLightsParameters);
            DrawEffectSettings(EnableBrakeDisksGlowEffect, _brakeDisksGlowParameters);
            HandleAntiLag();

            DrawEffectSettings(EnableNitroEffect, _nitroParameters);
            DrawEffectSettings(EnableBodyAeroEffect, _bodyEffectParameters);
            DrawEffectSettings(EnableWingAeroEffect, _wingAeroParameters);

            if(serializedObject.FindProperty("_collisionHandler").objectReferenceValue != null)
            {
                DrawEffectSettings(serializedObject.FindProperty("EnableCollisionEffects"), serializedObject.FindProperty("_collisionParameters"));
            }

            DrawEffectSettings(serializedObject.FindProperty("EnableEngineSmokeEffect"), serializedObject.FindProperty("_engineSmokeEffectParameters"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DefaultInspector()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_currentCarStats"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_rigidbody"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_collisionHandler"));
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_axleArray"));
            EditorGUI.indentLevel -= 1;
        }

        private void DrawEffectSettings(SerializedProperty boolProperty, SerializedProperty parametersProperty)
        {
            EditorGUILayout.PropertyField(boolProperty);

            if(boolProperty.boolValue)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(parametersProperty);
                EditorGUI.indentLevel -= 1;
                EditorGUILayout.Space();
            }
        }
       
        private void HandleAntiLag()
        {
            EditorGUILayout.PropertyField(EnableAntiLagEffect);

            if (EnableAntiLagEffect.boolValue)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(_antiLagParameters);
                EditorGUI.indentLevel -= 1;
                EditorGUILayout.Space();
            }

            if (_antiLagMaxCount.intValue < _antiLagMinCount.intValue)
            {
                _antiLagMaxCount.intValue = _antiLagMinCount.intValue;
            }
        }
    }
}
