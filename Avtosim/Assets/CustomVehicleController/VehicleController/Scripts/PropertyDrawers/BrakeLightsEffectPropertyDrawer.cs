using UnityEditor;
using UnityEngine;

namespace Assets.VehicleController
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(BrakeLightsParameters))]
    public class BrakeLightsEffectPropertyDrawer : PropertyDrawer
    {
        private float _rectYPosOffsetBelowMeshesArray = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate rect for the RearLightMeshes field
            Rect rearLightMeshesRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rearLightMeshesRect, property.FindPropertyRelative("RearLightMeshes"));

            // Calculate rect for the BrakeColor field
            Rect brakeColorRect = new Rect(position.x, rearLightMeshesRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + _rectYPosOffsetBelowMeshesArray, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(brakeColorRect, property.FindPropertyRelative("BrakeColor"));

            // Calculate rect for the MaterialAtSpecificIndex field
            Rect materialAtSpecificIndexRect = new Rect(position.x, brakeColorRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
            SerializedProperty materialAtSpecificIndexProperty = property.FindPropertyRelative("MaterialsAtSpecificIndex");
            EditorGUI.PropertyField(materialAtSpecificIndexRect, materialAtSpecificIndexProperty);

            // If MaterialAtSpecificIndex is true, show the MaterialIndex field
            if (materialAtSpecificIndexProperty.boolValue)
            {
                Rect materialIndexRect = new Rect(position.x, materialAtSpecificIndexRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(materialIndexRect, property.FindPropertyRelative("MaterialIndexArray"));
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float totalHeight = lineHeight * 3;

            SerializedProperty rearLightMeshesProperty = property.FindPropertyRelative("RearLightMeshes");
            if (rearLightMeshesProperty.isExpanded)
            {
                int rearLightMeshesSize = rearLightMeshesProperty.arraySize;
                if(rearLightMeshesSize == 0)
                    rearLightMeshesSize = 1;
                _rectYPosOffsetBelowMeshesArray = (rearLightMeshesSize + 2) * lineHeight;
                totalHeight += (rearLightMeshesSize + 2) * lineHeight;
            }
            else
                _rectYPosOffsetBelowMeshesArray = 0;

            SerializedProperty materialAtSpecificIndexProperty = property.FindPropertyRelative("MaterialsAtSpecificIndex");
            if (materialAtSpecificIndexProperty.boolValue)
            {
                totalHeight += lineHeight;

                SerializedProperty indexArray = property.FindPropertyRelative("MaterialIndexArray");
                if (indexArray.isExpanded)
                {
                    totalHeight += lineHeight;
                    int size = indexArray.arraySize;
                    if (size == 0)
                        size = 1;
                    totalHeight += size * lineHeight;
                }
            }

            return totalHeight;
        }
    }
#endif
}
