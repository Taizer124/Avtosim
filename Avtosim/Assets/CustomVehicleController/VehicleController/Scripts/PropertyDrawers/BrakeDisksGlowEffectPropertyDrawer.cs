using UnityEditor;
using UnityEngine;

namespace Assets.VehicleController
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(BrakeDisksGlowParameters))]
    public class BrakeDisksGlowEffectPropertyDrawer : PropertyDrawer
    {
        private float _rectYPosOffsetBelowMeshesArray = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate rect for the RearLightMeshes field
            Rect brakesMeshesRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(brakesMeshesRect, property.FindPropertyRelative("BrakeDisksMeshes"));

            // Calculate rect for the BrakeColor field
            Rect brakeColorRect = new Rect(position.x, brakesMeshesRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + _rectYPosOffsetBelowMeshesArray, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(brakeColorRect, property.FindPropertyRelative("GlowColor"));

            Rect heatUpRect = new Rect(position.x, brakeColorRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
            SerializedProperty heatUpProperty = property.FindPropertyRelative("HeatUpTime");
            EditorGUI.PropertyField(heatUpRect, heatUpProperty);

            Rect coolDownRect = new Rect(position.x, brakeColorRect.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2, position.width, EditorGUIUtility.singleLineHeight);
            SerializedProperty coolDownProperty = property.FindPropertyRelative("CoolDownTime");
            EditorGUI.PropertyField(coolDownRect, coolDownProperty);

            // Calculate rect for the MaterialAtSpecificIndex field
            Rect materialAtSpecificIndexRect = new Rect(position.x, brakeColorRect.y + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3, position.width, EditorGUIUtility.singleLineHeight);
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
            float totalHeight = lineHeight * 5;

            SerializedProperty brakeDisksMeshesProperty = property.FindPropertyRelative("BrakeDisksMeshes");
            if (brakeDisksMeshesProperty.isExpanded)
            {
                int meshesSize = brakeDisksMeshesProperty.arraySize;
                if(meshesSize == 0)
                    meshesSize = 1;
                _rectYPosOffsetBelowMeshesArray = (meshesSize + 2) * lineHeight;
                totalHeight += (meshesSize + 2) * lineHeight;
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
