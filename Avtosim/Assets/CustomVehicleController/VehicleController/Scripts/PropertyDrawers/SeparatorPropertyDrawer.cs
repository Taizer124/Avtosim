using UnityEditor;
using UnityEngine;

namespace Assets.VehicleController
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SeparatorAttribute))]
    public class SeparatorPropertyDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            SeparatorAttribute separator = attribute as SeparatorAttribute;
            return EditorGUIUtility.singleLineHeight + separator.Height;
        }

        public override void OnGUI(Rect position)
        {
            SeparatorAttribute separator = attribute as SeparatorAttribute;

            Rect rect = EditorGUI.IndentedRect(position);
            rect.y -= separator.VerticalOFfset;
            rect.x += separator.LeftPadding;
            rect.height = separator.Height;
            EditorGUI.DrawRect(rect, separator.Color);
        }
    }
#endif
}
