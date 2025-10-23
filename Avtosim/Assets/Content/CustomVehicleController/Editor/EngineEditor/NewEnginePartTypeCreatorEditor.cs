using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class NewEnginePartTypeCreatorEditor
    {
        private const string ENGINE_PART_TYPE_NAME_FIELD_NAME = "NewEnginePartTypeName";
        private const string CREATE_ENGINE_TYPE_BUTTON_NAME = "CreateNewEnginePartTypeButton";

        private const string DELETE_PART_DROPDOWN_FIELD_NAME = "DeletePartTypeDropdown";
        private const string DELETE_PART_BUTTON_NAME = "DeleteEnginePartTypeButton";

        private const string CONFIRM_WINDOW_NAME = "ConfirmScreen";
        private const string FOLDER_PATH_LABEL_NAME = "FolderPathLabel";
        private const string TYPE_NAME_LABEL_NAME = "TypeNameLabel";
        private const string DONT_ASK_AGAIN_TOGGLE_NAME = "DontAskToggle";
        private const string CONFIRM_BUTTON_NAME = "ConfirmDeleteButton";
        private const string DENY_BUTTON_NAME = "DenyDeleteButton";

        private DropdownField _deletePartTypeDropdown;
        private TextField _enginePartTypeNameField;

        private VisualElement _confirmWindow;
        private Label _folderPathLabel;
        private Label _typeNameLabel;
        private Toggle _dontAskToggle;

        private VisualElement rootVisualElement;
        private EnginePartCreatorWindow _editorWindow;

        private string _deleteAssetPath;

        private const string KEY_NAME = "CustomVehicleControllerDontAskWhenDeletingEnginePartType";

        public NewEnginePartTypeCreatorEditor(VisualElement root, EnginePartCreatorWindow editorWindow)
        {
            rootVisualElement = root;
            _editorWindow = editorWindow;
            FindElements();
        }

        private void FindElements()
        {
            _enginePartTypeNameField = rootVisualElement.Q<TextField>(ENGINE_PART_TYPE_NAME_FIELD_NAME);
            rootVisualElement.Q<Button>(CREATE_ENGINE_TYPE_BUTTON_NAME).clicked += () =>
            {
                CreateNewEngineType();
            };

            _deletePartTypeDropdown = rootVisualElement.Q<DropdownField>(DELETE_PART_DROPDOWN_FIELD_NAME);

            rootVisualElement.Q<Button>(DELETE_PART_BUTTON_NAME).clicked += () =>
            {
                DeleteTypeAndAssets();
            };

            _confirmWindow = rootVisualElement.Q<VisualElement>(CONFIRM_WINDOW_NAME);
            _folderPathLabel = rootVisualElement.Q<Label>(FOLDER_PATH_LABEL_NAME);
            _typeNameLabel = rootVisualElement.Q<Label>(TYPE_NAME_LABEL_NAME);
            _dontAskToggle = rootVisualElement.Q<Toggle>(DONT_ASK_AGAIN_TOGGLE_NAME);

            rootVisualElement.Q<Button>(CONFIRM_BUTTON_NAME).clicked += () =>
            {
                EditorPrefs.SetBool(KEY_NAME, _dontAskToggle.value);
                AssetDatabase.DeleteAsset(_deleteAssetPath);
                _confirmWindow.style.display = DisplayStyle.None;
            };

            rootVisualElement.Q<Button>(DENY_BUTTON_NAME).clicked += () =>
            {
                _confirmWindow.style.display = DisplayStyle.None;
            };
        }

        private void CreateNewEngineType()
        {
            string newTypeName = _enginePartTypeNameField.text;

            if (!PartTypeNameValidator.CheckClassNameIsValid(newTypeName))
                return;

            string templateContent = LoadTemplate();
            if (templateContent == "")
                return;

            string folderPath = LocalPathFinder.Instance.GetEnginePartsFolder();

            CreateFolder(folderPath, newTypeName);
            CreateClass(templateContent, folderPath, newTypeName);
            _editorWindow.UpdatePartTypes();
        }

        private void CreateFolder(string folderPath, string folderName)
        {
            AssetDatabase.CreateFolder(folderPath, folderName);
            AssetDatabase.SaveAssets();
        }

        private void CreateClass(string template, string folderPath, string newTypeName)
        {
            string classContent = template.Replace("#CLASSNAME#", newTypeName);

            File.WriteAllBytes(folderPath + "\\" + newTypeName + ".cs", Encoding.ASCII.GetBytes(classContent));
            AssetDatabase.Refresh();
        }

        private string LoadTemplate()
        {
            // Load template file
            TextAsset templateFile = Resources.Load<TextAsset>("NewEnginePartTypeTemplate");

            if (templateFile != null)
            {
                return templateFile.text;
            }
            else
            {
                Debug.LogError("Template file not found!");
                return "";
            }
        }

        private void DeleteTypeAndAssets()
        {
            if (String.IsNullOrEmpty(_deletePartTypeDropdown.value))
            {
                Debug.Log("No part type chosen.");
                return;
            }

            _deleteAssetPath = LocalPathFinder.Instance.GetEnginePartsFolder() + "\\" + _deletePartTypeDropdown.value;

            if (!AssetDatabase.IsValidFolder(_deleteAssetPath))
            {
                Debug.LogWarning($"A folder at path {_deleteAssetPath} doesn't exist. DO NOT move or delete the folders within CustomVehicleController folder! If you set custom path to the engine parts folder, this means the folder no longer exists.");
                return;
            }

            if(!EditorPrefs.HasKey(KEY_NAME) || !EditorPrefs.GetBool(KEY_NAME))
            {
                _confirmWindow.style.display = DisplayStyle.Flex;
                _dontAskToggle.value = false;
                _folderPathLabel.text = _deleteAssetPath;
                _typeNameLabel.text = $"You are about to delete {_deletePartTypeDropdown.value} at location:";
                return;
            }

            AssetDatabase.DeleteAsset(_deleteAssetPath);
            AssetDatabase.DeleteAsset(LocalPathFinder.Instance.GetEngineTypePath(_deletePartTypeDropdown.value + ".cs"));
            AssetDatabase.SaveAssets();
        }

        public void ResetChoices(List<string> choices)
        {
            _deletePartTypeDropdown.choices = choices;
        }
    }
}
