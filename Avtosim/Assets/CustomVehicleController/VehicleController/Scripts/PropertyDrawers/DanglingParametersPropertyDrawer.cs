using UnityEditor;
using UnityEngine;

namespace Assets.VehicleController
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DanglingParameters))]
    public class DanglingParametersPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate rect for the RearLightMeshes field
            Rect dangleBooleanRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            SerializedProperty dangleBool = property.FindPropertyRelative("DangleFromSpeed");
            EditorGUI.PropertyField(dangleBooleanRect, dangleBool);

            if(dangleBool.boolValue)
            {
                float offset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                Rect minSpeed = new Rect(position.x, position.y + offset, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(minSpeed, property.FindPropertyRelative("MinSpeedForDangling"));

                Rect maxEffectSpeed = new Rect(position.x, position.y + offset * 2, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(maxEffectSpeed, property.FindPropertyRelative("DangleMaxEffectSpeed"));

                Rect rate = new Rect(position.x, position.y + offset * 3, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(rate, property.FindPropertyRelative("DangleRate"));
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float totalHeight = lineHeight;

            SerializedProperty dangleBool = property.FindPropertyRelative("DangleFromSpeed");
            if (dangleBool.boolValue)
            {
                totalHeight += lineHeight * 4;
            }

            return totalHeight;
        }
    }
#endif
}
