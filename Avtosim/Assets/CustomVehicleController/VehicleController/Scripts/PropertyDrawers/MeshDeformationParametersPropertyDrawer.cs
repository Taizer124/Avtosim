using UnityEditor;
using UnityEngine;

#if MATH_PACKAGE_INSTALLED

namespace Assets.VehicleController
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MeshDeformationParameters))]
    public class MeshDeformationParametersPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate rect for the RearLightMeshes field
            Rect foldout = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            float offset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded = EditorGUI.Foldout(foldout, property.isExpanded, "Properties:"))
            {
                Rect MeshFilter = new Rect(position.x, position.y + offset, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(MeshFilter, property.FindPropertyRelative("MeshFilter"));

                if (property.FindPropertyRelative("MeshFilter").objectReferenceValue != null)
                {
                    Rect BodyStrength = new Rect(position.x, position.y + offset * 2, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(BodyStrength, property.FindPropertyRelative("BodyStrength"));

                    Rect DamageRadiusMultiplier = new Rect(position.x, position.y + offset * 3, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(DamageRadiusMultiplier, property.FindPropertyRelative("DamageRadiusMultiplier"));

                    Rect AdditionalDamageRadius = new Rect(position.x, position.y + offset * 4, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(AdditionalDamageRadius, property.FindPropertyRelative("AdditionalDamageRadius"));

                    Rect MaxDeformDepth = new Rect(position.x, position.y + offset * 5, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(MaxDeformDepth, property.FindPropertyRelative("MaxDeformDepth"));

                    Rect CollisionAreasDataSO = new Rect(position.x, position.y + offset * 6, position.width, EditorGUIUtility.singleLineHeight * 2.5f);
                    EditorGUI.PropertyField(CollisionAreasDataSO, property.FindPropertyRelative("CollisionAreasDataSO"));
                    if (property.FindPropertyRelative("CollisionAreasDataSO").objectReferenceValue != null)
                    {
                        Rect CollisionAreasOptimization = new Rect(position.x, position.y + offset * 8.5f, position.width, EditorGUIUtility.singleLineHeight);
                        EditorGUI.PropertyField(CollisionAreasOptimization, property.FindPropertyRelative("CollisionAreasOptimization"));

                        Rect DeformAffectedCollisionAreasNearby = new Rect(position.x, position.y + offset * 9.5f, position.width, EditorGUIUtility.singleLineHeight);
                        EditorGUI.PropertyField(DeformAffectedCollisionAreasNearby, property.FindPropertyRelative("DeformAffectedCollisionAreasNearby"));
                    }
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float totalHeight = lineHeight;

            if(property.isExpanded)
            {
                if (property.FindPropertyRelative("MeshFilter").objectReferenceValue != null)
                {
                    totalHeight += lineHeight * 7;
                    if (property.FindPropertyRelative("CollisionAreasDataSO").objectReferenceValue != null)
                        totalHeight += lineHeight * 2;
                }
            }

            return totalHeight;
        }
    }
#endif
}
#endif