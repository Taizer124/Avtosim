using Assets.VehicleController;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class EnginePartContainerEditor
    {
        private const string SELECTED_CONTROLLER_LABEL = "VehicleControllerLable";
        private const string ENGINE_PARTS_LIST_VIEW = "EnginePartsContainerListView";
        private const string PART_HOLDER_NAME = "PartsHolder";

        private Label _selectedControllerLabel;
        private ListView _enginePartsContainerListView;
        private VisualElement _partsHolder;

        private UnityEngine.Color _selectedControllerColor = UnityEngine.Color.green;
        private UnityEngine.Color _nullControllerColor = UnityEngine.Color.red;

        private VisualElement rootVisualElement;
        private EnginePartCreatorWindow _editorWindow;

        private List<CustomEnginePart> _customEngineParts;
        private List<string> _possibleTypes;

        private SerializedObject _serializedController;

        public EnginePartContainerEditor(VisualElement root, EnginePartCreatorWindow editorWindow)
        {
            rootVisualElement = root;
            _editorWindow = editorWindow;
            FindFields();
            MakeEnginePartsList();
        }

        private void MakeEnginePartsList()
        {
            // The ListView calls this to add visible items to the scroller.
            Func<VisualElement> makeItem = () =>
            {
                var enginePartVisualElement = new EnginePartVisualElement();
                var objField = enginePartVisualElement.Q<ObjectField>();

                enginePartVisualElement.Q<Label>().text = _possibleTypes.First();
                _possibleTypes.RemoveAt(0);

                objField.RegisterValueChangedCallback(evt =>
                {
                    var i = (int)objField.userData;
                    _serializedController.FindProperty("_enginePartsContainer").FindPropertyRelative("EnginePartsList").GetArrayElementAtIndex(i).objectReferenceValue = evt.newValue as CustomEnginePart;
                    _serializedController.ApplyModifiedProperties();
                });
                return enginePartVisualElement;
            };

            Action<VisualElement, int> bindItem = (e, i) => BindItem(e as EnginePartVisualElement, i);

            int itemHeight = 27;
            _enginePartsContainerListView = rootVisualElement.Q<ListView>(ENGINE_PARTS_LIST_VIEW);
            _enginePartsContainerListView.fixedItemHeight = itemHeight;
            _enginePartsContainerListView.makeItem = makeItem;
            _enginePartsContainerListView.bindItem = bindItem;

            _enginePartsContainerListView.reorderable = false;
            _enginePartsContainerListView.style.flexGrow = 1f;
            _enginePartsContainerListView.showBorder = true;
            _enginePartsContainerListView.showAddRemoveFooter = false;
        }

        private void BindItem(EnginePartVisualElement elem, int i)
        {
            ObjectField objectField = elem.Q<ObjectField>();
            Label label = elem.Q<Label>();
            objectField.userData = i;
            objectField.objectType = _editorWindow.GetTypeFromName(label.text);
            objectField.value = _customEngineParts[i];
        }

        private void FindFields()
        {
            _selectedControllerLabel = rootVisualElement.Q<Label>(SELECTED_CONTROLLER_LABEL);
            _partsHolder = rootVisualElement.Q<VisualElement>(PART_HOLDER_NAME);
        }

        public void BindController(CustomVehicleController controller)
        {
            _enginePartsContainerListView.Unbind();

            if(_serializedController != null)
                _serializedController.Dispose();

            _partsHolder.style.display = controller == null ? DisplayStyle.None : DisplayStyle.Flex;

            if (controller == null)
            {
                _customEngineParts = null;
                _selectedControllerLabel.style.color = _nullControllerColor;
                _selectedControllerLabel.text = "None";
                return;
            }

            _customEngineParts = controller.GetEnginePartsContainer().EnginePartsList;

            RemoveExcessiveElements();
            FillMissingFields();

            _serializedController = new SerializedObject(controller);
            _enginePartsContainerListView.BindProperty(_serializedController.FindProperty("_enginePartsContainer").FindPropertyRelative("EnginePartsList"));
            
            _selectedControllerLabel.style.color = _selectedControllerColor;
            _selectedControllerLabel.text = controller.gameObject.name;
        }  

        private void RemoveExcessiveElements()
        {
            if (_customEngineParts.Count > _editorWindow.GetPartTypeChoices().Count)
                _customEngineParts.RemoveAll(x => x == null);
        }

        private void FillMissingFields()
        {
            _possibleTypes = _editorWindow.GetPartTypeChoices();
            int maxNum = _possibleTypes.Count;

            for (int i = _customEngineParts.Count; i < maxNum; i++)
            {
                _customEngineParts.Add(null);
            }
        }

        public class EnginePartVisualElement : VisualElement
        {
            public Label PartTypeLabel;
            public ObjectField PartObjectField;

            public EnginePartVisualElement()
            {
                var root = new VisualElement();
                root.style.flexDirection = FlexDirection.Row;

                root.style.marginBottom = 3f;
                root.style.paddingBottom = 3f;
                root.style.paddingLeft = 3f;

                root.style.borderBottomColor = UnityEngine.Color.gray;
                root.style.borderBottomWidth = 2f;
                PartTypeLabel = new Label();
                PartTypeLabel.style.fontSize = 12;

                PartObjectField = new ObjectField();
                
               
                PartObjectField.style.fontSize = 12;
                PartObjectField.style.flexGrow = 1f;

                root.Add(PartTypeLabel);
                root.Add(PartObjectField);
                Add(root);
            }

            public void SetData(List<string> choices, Type customEnginePartType)
            {
                PartTypeLabel.text = choices[0];
                PartObjectField.objectType = customEnginePartType;
            }
        }
    }
}
