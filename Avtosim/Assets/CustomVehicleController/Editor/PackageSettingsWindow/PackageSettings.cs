using Assets.VehicleController;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.VehicleControllerEditor
{
    public class PackageSettings : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField]
        private PackageSettingsSO _packageSettingSO;

        #region field names
        private const string DEFAULT_PATHS_TOGGLE = "UseDefaultPathsToggle";
        private const string VEHICLE_PARTS_HOLDER_NAME = "PathsHolder";
        private const string PRESET_PARTS_HOLDER_NAME = "PresetAndPartsHolder";

        private const string ENGINE_PATH_LABEL = "EnginePathLabel";
        private const string TRANS_PATH_LABEL = "TransmissionPathLabel";
        private const string FI_PATH_LABEL = "FIPathLabel";
        private const string SUSP_PATH_LABEL = "SuspensionPathLabel";
        private const string TIRES_PATH_LABEL = "TiresPathLabel";
        private const string BRAKES_PATH_LABEL = "BrakesPathLabel";
        private const string BODIES_PATH_LABEL = "BodiesPathLabel";
        private const string NITRO_PATH_LABEL = "NitrousPathLabel";

        private const string PRESET_PATH_LABEL = "PresetsPathLabel";
        private const string ENGINE_PARTS_PATH_LABEL = "EnginePartsPathLabel";
        private const string COLLISION_AREAS_PATH_LABEL = "CollisionAreasPathLabel";

        private const string ENGINE_PATH_BUTTON = "EnginePathButton";
        private const string TRANS_PATH_BUTTON = "TransmissionPathButton";
        private const string FI_PATH_BUTTON = "FIPathButton";
        private const string SUSP_PATH_BUTTON = "SuspensionPathButton";
        private const string TIRES_PATH_BUTTON = "TiresPathButton";
        private const string BRAKES_PATH_BUTTON = "BrakesPathButton";
        private const string BODIES_PATH_BUTTON = "BodiesPathButton";
        private const string NITRO_PATH_BUTTON = "NitrousPathButton";


        private const string PRESET_PATH_BUTTON = "PresetsPathButton";
        private const string ENGINE_PARTS_PATH_BUTTON = "EnginePartsPathButton";
        private const string COLLISION_AREAS_PATH_BUTTON = "CollisionAreasPathButton";
        #endregion

        #region
        private VisualElement _vehiclePartsHolder;
        private VisualElement _presetPartsHolder;
        private Toggle _useDefPathsToggle;

        private Label _enginePathLabel;
        private Label _transmissionPathLabel;
        private Label _fiPathLabel;
        private Label _suspPathLabel;
        private Label _tiresPathLabel;
        private Label _brakesPathLabel;
        private Label _bodiesPathLabel;
        private Label _nitroPathLabel;

        private Label _presetsPathLabel;
        private Label _enginePartsPathLabel;
        private Label _collisionAreasPathLabel;
        #endregion

        #region confirm move
        private const string CONFIRM_BG_NAME = "MoveAssetsHolder";
        private const string CONFIRM_BUTTON_NAME = "ConfirmMoveButton";
        private const string DENY_BUTTON_NAME = "DenyMoveButton";
        private const string SUCCESS_LABEL_NAME = "SuccessLabel";
        private const string ASK_LABEL_NAME = "AskLabel";

        private VisualElement _confirmBg;
        private Label _successLabel;
        private Label _askLabel;
        #endregion

        private AssetType _lastPathChangedAssetType;
        private string _lastChangedPath;
        private string _oldRootEnginePartsFolder;

        private const string DOCS_LINK_BUTTON_NAME = "DocsLinkButton";
        private UnityEngine.Color _docsMouseOverColor = new Color(100 / 255f, 200 / 255f, 255 / 255f);
        private UnityEngine.Color _docsMouseExitColor = new Color(0, 165 / 255f, 240 / 255f);

        [MenuItem("Tools/CustomVehicleController/Package Settings")]
        public static void ShowExample()
        {
            PackageSettings wnd = GetWindow<PackageSettings>();
            wnd.titleContent = new GUIContent("PackageSettings");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            _packageSettingSO.UpdateDisplayInfo();

            FindFields();
            BindFields(new SerializedObject(_packageSettingSO));

            Button button = rootVisualElement.Q<Button>(DOCS_LINK_BUTTON_NAME);

            button.clicked += () => {
                Application.OpenURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/package-settings");
            };
            Label label = rootVisualElement.Q<Label>("DocsLabel");
            button.RegisterCallback<MouseOverEvent>(evt => { label.style.color = _docsMouseOverColor; });
            button.RegisterCallback<MouseOutEvent>(evt => { label.style.color = _docsMouseExitColor; });
        }
       
        private void UpdatePathsHolder()
        {
            _presetPartsHolder.style.opacity = _useDefPathsToggle.value ? 0.5f : 1;
            _vehiclePartsHolder.style.opacity = _useDefPathsToggle.value ? 0.5f : 1;

            _presetPartsHolder.pickingMode = _useDefPathsToggle.value ? PickingMode.Ignore : PickingMode.Position;
            _vehiclePartsHolder.pickingMode = _useDefPathsToggle.value ? PickingMode.Ignore : PickingMode.Position;
            foreach(var child in _vehiclePartsHolder.Children())
            {
                HandleVisualElementClicking(child);
            }
            foreach (var child in _presetPartsHolder.Children())
            {
                HandleVisualElementClicking(child);
            }
        }

        private void HandleVisualElementClicking(VisualElement child)
        {
            if (child == null)
                return;

            child.pickingMode = _useDefPathsToggle.value ? PickingMode.Ignore : PickingMode.Position;
            foreach (var subChild in child.Children())
            {
                HandleVisualElementClicking(subChild);
            }
        }

        private void FindFields()
        {
            _useDefPathsToggle = rootVisualElement.Q<Toggle>(DEFAULT_PATHS_TOGGLE);
            _useDefPathsToggle.value = _packageSettingSO.DefaultSavePaths;
            _vehiclePartsHolder = rootVisualElement.Q<VisualElement>(VEHICLE_PARTS_HOLDER_NAME);
            _presetPartsHolder = rootVisualElement.Q<VisualElement>(PRESET_PARTS_HOLDER_NAME);
            UpdatePathsHolder();

            _enginePathLabel = rootVisualElement.Q<Label>(ENGINE_PATH_LABEL);
            _transmissionPathLabel = rootVisualElement.Q<Label>(TRANS_PATH_LABEL);
            _fiPathLabel = rootVisualElement.Q<Label>(FI_PATH_LABEL);
            _suspPathLabel = rootVisualElement.Q<Label>(SUSP_PATH_LABEL);
            _tiresPathLabel = rootVisualElement.Q<Label>(TIRES_PATH_LABEL);
            _brakesPathLabel = rootVisualElement.Q<Label>(BRAKES_PATH_LABEL);
            _bodiesPathLabel = rootVisualElement.Q<Label>(BODIES_PATH_LABEL);
            _nitroPathLabel = rootVisualElement.Q<Label>(NITRO_PATH_LABEL);

            _presetsPathLabel = rootVisualElement.Q<Label>(PRESET_PATH_LABEL);
            _enginePartsPathLabel = rootVisualElement.Q<Label>(ENGINE_PARTS_PATH_LABEL);
            _collisionAreasPathLabel = rootVisualElement.Q<Label>(COLLISION_AREAS_PATH_LABEL);

            rootVisualElement.Q<Button>(ENGINE_PATH_BUTTON).clicked += () => SetNewPathOnClick(AssetType.Engine);
            rootVisualElement.Q<Button>(TRANS_PATH_BUTTON).clicked += () => SetNewPathOnClick(AssetType.Transmission);
            rootVisualElement.Q<Button>(FI_PATH_BUTTON).clicked += () => SetNewPathOnClick(AssetType.ForcedInduction);
            rootVisualElement.Q<Button>(SUSP_PATH_BUTTON).clicked += () => SetNewPathOnClick(AssetType.Suspension);
            rootVisualElement.Q<Button>(TIRES_PATH_BUTTON).clicked += () => SetNewPathOnClick(AssetType.Tires);
            rootVisualElement.Q<Button>(BRAKES_PATH_BUTTON).clicked += () => SetNewPathOnClick(AssetType.Brakes);
            rootVisualElement.Q<Button>(BODIES_PATH_BUTTON).clicked += () => SetNewPathOnClick(AssetType.Bodies);
            rootVisualElement.Q<Button>(NITRO_PATH_BUTTON).clicked += () => SetNewPathOnClick(AssetType.Nitro);
            rootVisualElement.Q<Button>(PRESET_PATH_BUTTON).clicked += () => SetNewPathOnClick(AssetType.Preset);
            rootVisualElement.Q<Button>(ENGINE_PARTS_PATH_BUTTON).clicked += () => SetNewPathOnClick(AssetType.EnginePart);
            rootVisualElement.Q<Button>(COLLISION_AREAS_PATH_BUTTON).clicked += () => SetNewPathOnClick(AssetType.CollisionAreas);

            _confirmBg = rootVisualElement.Q<VisualElement>(CONFIRM_BG_NAME);
            _successLabel = rootVisualElement.Q<Label>(SUCCESS_LABEL_NAME);
            _askLabel = rootVisualElement.Q<Label>(ASK_LABEL_NAME);
            rootVisualElement.Q<Button>(CONFIRM_BUTTON_NAME).clicked += MoveAllAssetsOfTypeOnClick;
            rootVisualElement.Q<Button>(DENY_BUTTON_NAME).clicked += () =>
            {
                _confirmBg.style.display = DisplayStyle.None;
            };
        }

        private void MoveAllAssetsOfTypeOnClick()
        {
            if(_lastPathChangedAssetType == AssetType.EnginePart)
            {
                try
                {
                    string[] subfolders = AssetDatabase.GetSubFolders(_oldRootEnginePartsFolder);
                    foreach (string subfolder in subfolders)
                    {
                        string folderName = System.IO.Path.GetFileName(subfolder);
                        string destinationPath = System.IO.Path.Combine(_lastChangedPath, folderName);

                        AssetDatabase.MoveAsset(subfolder, destinationPath);
                    }
                    AssetDatabase.SaveAssets();
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error when moving engine parts folder");
                    Debug.LogError(ex);
                }
                finally
                {
                    _confirmBg.style.display = DisplayStyle.None;
                }
                return;
            }


            string typeName = "";
            switch(_lastPathChangedAssetType)
            {
                case AssetType.Engine:
                    typeName = nameof(EngineSO); break;
                case AssetType.Transmission:
                    typeName = nameof(TransmissionSO); break;
                case AssetType.ForcedInduction:
                    typeName = nameof(ForcedInductionSO); break;
                case AssetType.Suspension:
                    typeName = nameof(SuspensionSO); break;
                case AssetType.Tires:
                    typeName = nameof(TiresSO); break;
                case AssetType.Brakes:
                    typeName = nameof(BrakesSO); break;
                case AssetType.Bodies:
                    typeName = nameof(VehicleBodySO); break;
                case AssetType.Nitro:
                    typeName = nameof(NitrousSO); break;
                case AssetType.Preset:
                    typeName = nameof(VehiclePartsPresetSO); break;
                case AssetType.CollisionAreas:
                    typeName = nameof(CollisionAreasDataSO); break;
            }

            string[] result= AssetDatabase.FindAssets($"t:{typeName}");
            try
            {
                foreach (string asset in result)
                {
                    string oldPath = AssetDatabase.GUIDToAssetPath(asset);
                    string assetName = oldPath.Substring(oldPath.LastIndexOf("/"));
                    AssetDatabase.MoveAsset(oldPath, _lastChangedPath + assetName);
                }
                AssetDatabase.SaveAssets();
            }
            catch (Exception ex)
            {
                Debug.LogError("An error occured while movign assets");
                Debug.LogError(ex);
            }
            finally
            {
                _confirmBg.style.display = DisplayStyle.None;
            }
        }

        private void SetNewPathOnClick(AssetType type)
        {
            string path = GetCustomPath(type);
            if (path == null)
                return;

            string newGUID = AssetDatabase.AssetPathToGUID(path);

            switch (type)
            {
                case AssetType.Engine:
                    _packageSettingSO.SetEngineGuid(newGUID); break;
                case AssetType.Transmission:
                    _packageSettingSO.SetTransmissionGuid(newGUID); break;
                case AssetType.ForcedInduction:
                    _packageSettingSO.SetFIGuid(newGUID); break;
                case AssetType.Suspension:
                    _packageSettingSO.SetSuspensionGuid(newGUID); break;
                case AssetType.Tires:
                    _packageSettingSO.SetTiresGuid(newGUID); break;
                case AssetType.Brakes:
                    _packageSettingSO.SetBrakesGuid(newGUID); break;
                case AssetType.Bodies:
                    _packageSettingSO.SetBodiesGuid(newGUID); break;
                case AssetType.Nitro:
                    _packageSettingSO.SetNitroGuid(newGUID); break;
                case AssetType.Preset:
                    _packageSettingSO.SetPresetGuid(newGUID); break;
                case AssetType.CollisionAreas:
                    _packageSettingSO.SetCollisionAreasGuid(newGUID); break;
                case AssetType.EnginePart:
                    _oldRootEnginePartsFolder = LocalPathFinder.Instance.GetEnginePartsFolderOrDefault();
                    _packageSettingSO.SetEnginePartsGuid(newGUID);break;
            }


            _lastPathChangedAssetType = type;
            _confirmBg.style.display = DisplayStyle.Flex;
            _lastChangedPath = path;
            _packageSettingSO.UpdateDisplayInfo();
        }

        private string GetCustomPath(AssetType type)
        {
            var setPath = TryGetSetPath(type);

            var path = EditorUtility.OpenFolderPanel(
                "Select a folder for " + type.ToString() +" assets",
                setPath == null ? LocalPathFinder.Instance.GetDefaultVehiclePartsFolder() : setPath,
                "");

            if (path == null ||path == "" || path == PackageSettingsSO.NO_PATH_PROVIDED_TEXT)
                return null;

            if(!path.Contains("Assets"))
            {
                Debug.LogError("The provided path must be inside the Assets folder of the current project.");
                return null;
            }

            return path.Substring(path.IndexOf("Assets"));
        }

        private string TryGetSetPath(AssetType type)
        {
            string path = "";
            switch (type)
            {
                case AssetType.Engine:
                    path = _packageSettingSO.GetEnginePathFromGuid();
                    break;
                case AssetType.Transmission:
                    path = _packageSettingSO.GetTransmissionPathFromGuid();
                    break;
                case AssetType.ForcedInduction:
                    path = _packageSettingSO.GetFIPathFromGuid();
                    break;
                case AssetType.Suspension:
                    path = _packageSettingSO.GetSuspensionPathFromGuid();
                    break;
                case AssetType.Tires:
                    path = _packageSettingSO.GetTiresPathFromGuid();
                    break;
                case AssetType.Brakes:
                    path = _packageSettingSO.GetBrakesPathFromGuid();
                    break;
                case AssetType.Bodies:
                    path = _packageSettingSO.GetBodiesPathFromGuid();
                    break;
                case AssetType.Nitro:
                    path = _packageSettingSO.GetNitroPathFromGuid();
                    break;
                case AssetType.Preset:
                    path = _packageSettingSO.GetPresetPathFromGuid();
                    break;
                case AssetType.EnginePart:
                    path = _packageSettingSO.GetEnginePartsPathFromGuid();
                    break;
                case AssetType.CollisionAreas:
                    path = _packageSettingSO.GetCollisionAreasPathFromGuid();
                    break;
            }

            if (path == null || path == "" || path == PackageSettingsSO.NO_PATH_PROVIDED_TEXT)
                return null;

            if (!AssetDatabase.IsValidFolder(path))
                return null;

            return path;
        }

        private void BindFields(SerializedObject so)
        {
            _useDefPathsToggle.bindingPath = nameof(_packageSettingSO.DefaultSavePaths);
            _useDefPathsToggle.Bind(so);

            _enginePathLabel.bindingPath = nameof(_packageSettingSO.EnginePathDisplay);
            _transmissionPathLabel.bindingPath = nameof(_packageSettingSO.TransmissionPathDisplay);
            _fiPathLabel.bindingPath = nameof(_packageSettingSO.FIPathDisplay);
            _suspPathLabel.bindingPath = nameof(_packageSettingSO.SuspensionPathDisplay);
            _tiresPathLabel.bindingPath = nameof(_packageSettingSO.TiresPathDisplay);
            _brakesPathLabel.bindingPath = nameof(_packageSettingSO.BrakesPathDisplay);
            _bodiesPathLabel.bindingPath = nameof(_packageSettingSO.BodiesPathDisplay);
            _nitroPathLabel.bindingPath = nameof(_packageSettingSO.NitroPathDisplay);
            _presetsPathLabel.bindingPath = nameof(_packageSettingSO.PresetPathDisplay);
            _enginePartsPathLabel.bindingPath = nameof(_packageSettingSO.EnginePartsPathDisplay);
            _collisionAreasPathLabel.bindingPath = nameof(_packageSettingSO.CollisionAreasPathDisplay);

            _enginePathLabel.Bind(so);
            _transmissionPathLabel.Bind(so);
            _fiPathLabel.Bind(so);
            _suspPathLabel.Bind(so);
            _tiresPathLabel.Bind(so);
            _brakesPathLabel.Bind(so);
            _bodiesPathLabel.Bind(so);
            _nitroPathLabel.Bind(so);
            _presetsPathLabel.Bind(so);
            _enginePartsPathLabel.Bind(so);
            _collisionAreasPathLabel.Bind(so);

            _useDefPathsToggle.RegisterValueChangedCallback(evt => {
                UpdatePathsHolder();
            });
        }

        private enum AssetType
        {
            Engine,
            Transmission,
            Suspension,
            ForcedInduction,
            Tires,
            Brakes,
            Bodies,
            Nitro,
            Preset,
            EnginePart,
            CollisionAreas
        }
    }

}
