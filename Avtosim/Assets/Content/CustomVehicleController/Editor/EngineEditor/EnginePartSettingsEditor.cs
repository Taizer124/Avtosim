using Assets.VehicleController;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class EnginePartSettingsEditor
    {
        private const string NEW_ASSET_TEXT_FIELD_NAME = "NewAssetTextField";

        private const string PART_SETTINGS_HOLDER_NAME = "PartSettingsHolder";

        private const string ENGINE_PART_TYPE_FIELD_NAME = "EnginePartTypeField";
        private const string ENGINE_PART_OBJECT_FIELD_NAME = "EnginePartField";
        private const string CREATE_NEW_ASSET_BUTTON_NAME = "CreateNewAssetButton";

        private const string TORQUE_INCREASE_FIELD_NAME = "TorqueIncreaseField";
        private const string NONLINEAR_BOOST_BOOL_FIELD_NAME = "NonLinearBoostBoolField";
        private const string EFFECT_CURVE_FIELD_NAME = "EffectCurveField";
        private const string CHANGLE_RPM_BOOL_FIELD_NAME = "ChangeRPMField";
        private const string IDLE_RPM_CHANGE_FIELD_NAME = "IdleRPMChangeField";
        private const string MAX_RPM_CHANGE_NAME = "MaxRPMChange";

        private DropdownField _enginePartTypeField;
        private ObjectField _enginePartObjectField;

        private IntegerField _torqueField;
        private Toggle _nonLinearBoostField;
        private CurveField _effectCurveField;
        private Toggle _changeRPMBoolField;
        private IntegerField _idleRPMIncreaseField;
        private IntegerField _maxRPMIncreaseField;
        private TextField _newAssetNameField;

        private VisualElement _partsSettingsHolder;

        private VisualElement rootVisualElement;
        private EnginePartCreatorWindow _editorWindow;

        public EnginePartSettingsEditor(VisualElement rootVisualElement, EnginePartCreatorWindow editorWindow)
        {
            this.rootVisualElement = rootVisualElement;
            _editorWindow = editorWindow;
            FindFields();
        }

        private void FindFields()
        {
            _partsSettingsHolder = rootVisualElement.Q<VisualElement>(PART_SETTINGS_HOLDER_NAME);

            _torqueField = rootVisualElement.Q<IntegerField>(TORQUE_INCREASE_FIELD_NAME);
            _torqueField.bindingPath = nameof(CustomEnginePart.Torque);

            _nonLinearBoostField = rootVisualElement.Q<Toggle>(NONLINEAR_BOOST_BOOL_FIELD_NAME);
            _nonLinearBoostField.bindingPath = nameof(CustomEnginePart.NonLinearBoost);

            _nonLinearBoostField.RegisterValueChangedCallback(evt =>
            {
                _effectCurveField.style.display = _nonLinearBoostField.value ? DisplayStyle.Flex : DisplayStyle.None;
            });

            _effectCurveField = rootVisualElement.Q<CurveField>(EFFECT_CURVE_FIELD_NAME);
            _effectCurveField.bindingPath = nameof(CustomEnginePart.EffectCurve);

            _changeRPMBoolField = rootVisualElement.Q<Toggle>(CHANGLE_RPM_BOOL_FIELD_NAME);
            _changeRPMBoolField.bindingPath = nameof(CustomEnginePart.ChangeWorkingRPM);
            _changeRPMBoolField.RegisterValueChangedCallback(evt => {
                _idleRPMIncreaseField.style.display = _maxRPMIncreaseField.style.display = _changeRPMBoolField.value ? DisplayStyle.Flex : DisplayStyle.None;
            });

            _idleRPMIncreaseField = rootVisualElement.Q<IntegerField>(IDLE_RPM_CHANGE_FIELD_NAME);
            _idleRPMIncreaseField.bindingPath = nameof(CustomEnginePart.IdleRPMChange);

            _maxRPMIncreaseField = rootVisualElement.Q<IntegerField>(MAX_RPM_CHANGE_NAME);
            _maxRPMIncreaseField.bindingPath = nameof(CustomEnginePart.MaxRPMChange);

            _newAssetNameField = rootVisualElement.Q<TextField>(NEW_ASSET_TEXT_FIELD_NAME);

            _enginePartObjectField = rootVisualElement.Q<ObjectField>(ENGINE_PART_OBJECT_FIELD_NAME);
            _enginePartObjectField.RegisterValueChangedCallback(evt => BindObjectToSO());

            _enginePartTypeField = rootVisualElement.Q<DropdownField>(ENGINE_PART_TYPE_FIELD_NAME);
            _enginePartTypeField.RegisterValueChangedCallback(evt => {
                if (_enginePartTypeField.value == null)
                {
                    _enginePartObjectField.value = null;
                    return;
                }

                _enginePartObjectField.objectType = _editorWindow.GetTypeFromName(_enginePartTypeField.value);
                _enginePartObjectField.label = _enginePartTypeField.value + " SO";
                _enginePartObjectField.value = null;
            });

            rootVisualElement.Q<Button>(CREATE_NEW_ASSET_BUTTON_NAME).clicked += () =>
            {
                CreateNewAsset();
            };
        }

        public void ResetChoices(List<string> choices)
        {
            _enginePartTypeField.choices = choices;
            if (String.IsNullOrEmpty(_enginePartTypeField.value))
                _enginePartTypeField.value = choices[0];

            _partsSettingsHolder.style.display = choices.Count == 0 ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void BindObjectToSO()
        {
            if (_enginePartObjectField.value == null)
            {
                _torqueField.Unbind();
                _nonLinearBoostField.Unbind();
                _effectCurveField.Unbind();
                _changeRPMBoolField.Unbind();
                _idleRPMIncreaseField.Unbind();
                _maxRPMIncreaseField.Unbind();
                return;
            }
            SerializedObject customEnginePart = new SerializedObject(_enginePartObjectField.value as CustomEnginePart);
            _torqueField.Bind(customEnginePart);
            _nonLinearBoostField.Bind(customEnginePart);
            _effectCurveField.Bind(customEnginePart);
            _changeRPMBoolField.Bind(customEnginePart);
            _idleRPMIncreaseField.Bind(customEnginePart);
            _maxRPMIncreaseField.Bind(customEnginePart);
        }

        private void CreateNewAsset()
        {
            if (!PartTypeNameValidator.CheckClassNameIsValid(_newAssetNameField.text))
                return;

            string rootFolder = LocalPathFinder.Instance.GetEnginePartsFolder();
            if (!AssetDatabase.IsValidFolder(rootFolder))
            {
                Debug.LogWarning($"A folder at path {rootFolder} doesn't exist. If you set custom paths, update the one for the engine parts folder!");
                return;
            }


            string folderPath = rootFolder + "\\" + _enginePartTypeField.value;

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"A folder at path {folderPath} doesn't exist. It was created automatically.");
                CreateFolder(rootFolder, _enginePartTypeField.value);
            }

            string filePath = folderPath + "\\" + _newAssetNameField.text + ".asset";

            var selectedType = _editorWindow.GetTypeFromName(_enginePartTypeField.value);

            var newAsset = ScriptableObject.CreateInstance(selectedType);

            newAsset.name = _newAssetNameField.text;
            (newAsset as CustomEnginePart).SetDefaultData(_newAssetNameField.text);
            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(filePath);

            AssetDatabase.CreateAsset(newAsset, uniqueFileName);
            AssetDatabase.SaveAssets();
            Undo.RegisterCreatedObjectUndo(newAsset, "Created New Engine Part");

            _enginePartObjectField.value = newAsset;
        }

        private void CreateFolder(string folderPath, string folderName)
        {
            AssetDatabase.CreateFolder(folderPath, folderName);
            AssetDatabase.SaveAssets();
        }
    }
}
