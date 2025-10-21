using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
namespace Assets.VehicleController
{
    [CustomPropertyDrawer(typeof(EnginePartsContainer))]
    public class EnginePartsContainerDrawer : PropertyDrawer
    {
        private static List<Type> _enginePartTypeList;
        private static List<string> _enginePartTypeNameList;

        [InitializeOnLoadMethod]
        private static void FindEnginePartTypesOnLoad()
        {
            _enginePartTypeList = new List<Type>();
            _enginePartTypeNameList = new List<string>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                // Filter types that implement IEnginePart interface
                Type[] types = assembly.GetTypes()
                    .Where(t => typeof(CustomEnginePart).IsAssignableFrom(t) && t != typeof(CustomEnginePart))
                    .ToArray();
                _enginePartTypeList.AddRange(types);
            }
            _enginePartTypeNameList = new();
            foreach (var type in _enginePartTypeList)
            {
                _enginePartTypeNameList.Add(type.Name);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(_enginePartTypeList == null)
                FindEnginePartTypesOnLoad();
           
            EditorGUI.BeginProperty(position, label, property);

            // Get the list property
            SerializedProperty enginePartsListProperty = property.FindPropertyRelative("EnginePartsList");

            // Handle mouse events to toggle foldout
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && position.Contains(currentEvent.mousePosition))
            {
                property.isExpanded = !property.isExpanded;
                currentEvent.Use();
            }

            // Foldout for the list
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            if (property.isExpanded)
            {
                // Begin black box
                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 1));
                boxStyle.normal.textColor = Color.white;
                GUI.Box(new Rect(position.x, position.y + 20, position.width, position.height - 20), GUIContent.none, boxStyle);

                // Calculate rect for the content
                Rect contentRect = EditorGUI.IndentedRect(position);
                contentRect.y += EditorGUIUtility.singleLineHeight + 5;

                // Draw vertical white line
                Rect lineRect = new Rect(contentRect.x + contentRect.width / 2 - 1, contentRect.y + 3, 2, contentRect.height - 30);
                EditorGUI.DrawRect(lineRect, Color.white);

                //remove excessive elements
                for(int i = _enginePartTypeNameList.Count; i < enginePartsListProperty.arraySize; i++)
                {
                    enginePartsListProperty.DeleteArrayElementAtIndex(i);
                }

                // Iterate through the list elements
                for (int i = 0; i < _enginePartTypeNameList.Count; i++)
                {
                    string objectName = "";
                    string typeName = _enginePartTypeNameList[i];

                    //the types are the same
                    if (i < enginePartsListProperty.arraySize)
                    {
                        //same types but no reference
                        if(enginePartsListProperty.GetArrayElementAtIndex(i).objectReferenceValue != null)
                            objectName = enginePartsListProperty.GetArrayElementAtIndex(i).objectReferenceValue.name;
                        else
                            objectName = "None";
                    }

                    // Display the type name and object name
                    Rect typeRect = new Rect(contentRect.x + 5, contentRect.y, contentRect.width * 0.5f, EditorGUIUtility.singleLineHeight);
                    Rect objectRect = new Rect(contentRect.x + contentRect.width * 0.5f + 5, contentRect.y, contentRect.width * 0.5f, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(typeRect, "Part Type: " + typeName);
                    EditorGUI.LabelField(objectRect, "Part Asset: " + objectName);

                    // Move to the next line
                    contentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + 5;

            if (property.isExpanded)
            {
                height += _enginePartTypeList.Count * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }

            return height;
        }

        // Create texture for drawing the box background
        private Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = color;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
#endif