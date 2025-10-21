using UnityEditor;
using UnityEngine;

namespace Assets.VehicleController
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DetachmentParameters))]
    public class DetachmentParametersPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate rect for the RearLightMeshes field
            Rect detachBooleanRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            SerializedProperty detachAtSpeedBool = property.FindPropertyRelative("DetachAtHighSpeed");
            EditorGUI.PropertyField(detachBooleanRect, detachAtSpeedBool);

            SerializedProperty detachAtHpBool = property.FindPropertyRelative("DetachWhenHPDepleted");

            float offset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            Rect detachWhenDepleted = new Rect(position.x, position.y + offset, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(detachWhenDepleted, property.FindPropertyRelative("DetachWhenHPDepleted"));

            if (detachAtSpeedBool.boolValue)
            {
                Rect detachSpeed = new Rect(position.x, position.y + offset * 3, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(detachSpeed, property.FindPropertyRelative("DetachSpeed"));
            }

            if(detachAtSpeedBool.boolValue || detachAtHpBool.boolValue)
            {
                Rect addBox = new Rect(position.x, position.y + offset * 2, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(addBox, property.FindPropertyRelative("AddBoxColliderWhenDetached"));
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float totalHeight = lineHeight * 2;

            SerializedProperty detachSpeedBool = property.FindPropertyRelative("DetachAtHighSpeed");
            SerializedProperty detachHPBool = property.FindPropertyRelative("DetachWhenHPDepleted");
            if (detachSpeedBool.boolValue || detachHPBool.boolValue)
                totalHeight += lineHeight;
            
            if (detachSpeedBool.boolValue)
                totalHeight += lineHeight;

            return totalHeight;
        }
    }
#endif
}
