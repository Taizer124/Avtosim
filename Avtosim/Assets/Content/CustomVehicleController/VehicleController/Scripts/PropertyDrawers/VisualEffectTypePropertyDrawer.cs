using UnityEditor;
using UnityEngine;

namespace Assets.VehicleController
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EffectTypeParameters))]
    public class VisualEffectTypePropertyDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var typeRect = new Rect(position.x, position.y, 120, position.height);
            var psRect = new Rect(position.x + 125, position.y, position.size.x - 125, position.height);
            var vfxRect = new Rect(position.x + 125, position.y, position.size.x - 125, position.height);

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("VisualEffectType"), GUIContent.none);

#if VISUAL_EFFECT_GRAPH_INSTALLED
            if(property.FindPropertyRelative("VisualEffectType").intValue == (int)VisualEffectAssetType.Type.VisualEffect)
                EditorGUI.PropertyField(vfxRect, property.FindPropertyRelative("VFXAsset"), GUIContent.none);
#endif
            if (property.FindPropertyRelative("VisualEffectType").intValue == (int)VisualEffectAssetType.Type.ParticleSystem)
                EditorGUI.PropertyField(psRect, property.FindPropertyRelative("ParticleSystem"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
        }
    }
#endif
}
