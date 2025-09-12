#region

using LogitechG29.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Editor;

#endregion

namespace LogitechG29.Editor
{
    public class ExtensionProcessorEditor : InputParameterEditor<ExtensionProcessor>
    {
        public override void OnGUI()
        {
            var style = new GUIStyle
            {
                normal = new GUIStyleState
                {
                    textColor = Color.grey
                },
                fontStyle = FontStyle.Italic,
                wordWrap = true
            };

            EditorGUILayout.LabelField("Примечание: Для работы тип действия должен быть сквозным", style);
            EditorGUILayout.Space(10f);

            target.SensitivitySpeed = EditorGUILayout.FloatField("Чувствительность Скорость", target.SensitivitySpeed);
            target.GravitySpeed = EditorGUILayout.FloatField("Скорость гравитации", target.GravitySpeed);
        }
    }
}