using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    public class LocalPathFinder: ScriptableObject
    {
        [SerializeField]
        private PackageSettingsSO _packageSettingsSO;
        private const string VEHICLE_PARTS_FOLDER_PATH = "\\VehicleController\\VehicleParts\\";

        private static LocalPathFinder instance;

        public static LocalPathFinder Instance
        {
            get
            {
                if (instance == null)
                    InitializeInstance();
                return instance;
            }
        }

        [InitializeOnLoadMethod]
        private static void InitializeInstance()
        {
            instance = ScriptableObject.CreateInstance<LocalPathFinder>();
        }

        private string FindPath()
        {
            MonoScript ms = MonoScript.FromScriptableObject(this);
            string scriptFilePath = AssetDatabase.GetAssetPath(ms);

            FileInfo fi = new FileInfo(scriptFilePath);
            string scriptFolder = fi.Directory.ToString();
            scriptFolder = Path.GetFullPath(Path.Combine(scriptFolder, "../..")) + VEHICLE_PARTS_FOLDER_PATH;
            return scriptFolder.Substring(scriptFolder.IndexOf("Assets"));
        }

        public string GetDefaultVehiclePartsFolder()
        {
            return FindPath();
        }

        public string GetEnginePartsFolderOrDefault()
        {
            string result = "";
            if (!_packageSettingsSO.DefaultSavePaths)
            {
                result = _packageSettingsSO.GetEnginePartsPathFromGuid();
                if (result == null || result == "" || result == PackageSettingsSO.NO_PATH_PROVIDED_TEXT)
                {
                    result = FindPath() + EnginePartCreatorWindow.ENGINE_PARTS_FOLDER_NAME;
                }
            }
            else
                result = FindPath() + EnginePartCreatorWindow.ENGINE_PARTS_FOLDER_NAME;

            return result;
        }

        public string GetEnginePartsFolder()
        {
            if(_packageSettingsSO.DefaultSavePaths)
                return FindPath() + EnginePartCreatorWindow.ENGINE_PARTS_FOLDER_NAME;

            string path = _packageSettingsSO.GetEnginePartsPathFromGuid();

            if (!AssetDatabase.IsValidFolder(path))
            {
                Debug.LogError("Trying to save engine part asset at the custom path " + path + ", but folder was not found. Update the path to the folder from the Tools -> CustomVehicleController -> Package Settings window");
            }
            return path;
        }

        public string GetEngineTypePath(string name)
        {
            return FindPath() + EnginePartCreatorWindow.ENGINE_PARTS_FOLDER_NAME + "\\" + name;
        }

        public string GetVehiclePartsFolderPathForAsset(string folderName)
        {
            string path = "";
            if(!_packageSettingsSO.DefaultSavePaths)
            {
                switch(folderName)
                {
                    case (ControllerEngineSettingsEditor.ENGINE_FOLDER_NAME):
                        path = _packageSettingsSO.GetEnginePathFromGuid();
                        break;
                    case (ControllerTransmissionSettingsEditor.TRANSMISSION_FOLDER_NAME):
                        path = _packageSettingsSO.GetTransmissionPathFromGuid();
                        break;
                    case (ControllerForcedInductionSettingsEditor.FORCED_INDUCTION_FOLDER_NAME):
                        path = _packageSettingsSO.GetFIPathFromGuid();
                        break;
                    case (ControllerSuspensionSettingsEditor.SUSPENSION_FOLDER_NAME):
                        path = _packageSettingsSO.GetSuspensionPathFromGuid();
                        break;
                    case (ControllerTiresSettingsEditor.TIRES_FOLDER_NAME):
                        path = _packageSettingsSO.GetTiresPathFromGuid();
                        break;
                    case (ControllerBrakesSettingsEditor.BRAKES_FOLDER_NAME):
                        path = _packageSettingsSO.GetBrakesPathFromGuid();
                        break;
                    case (ControllerBodySettingsEditor.BODY_FOLDER_NAME):
                        path = _packageSettingsSO.GetBodiesPathFromGuid();
                        break;
                    case (ControllerNitrousSettingsEditor.NITRO_FOLDER_NAME):
                        path = _packageSettingsSO.GetNitroPathFromGuid();
                        break;
                    case (ControllerPresetSettingsEditor.PRESET_FOLDER_NAME):
                        path = _packageSettingsSO.GetPresetPathFromGuid();
                        break;
                    case (CollisionAreaPartitionerEditor.COLLISION_PARTS_FOLDER_NAME):
                        path = _packageSettingsSO.GetCollisionAreasPathFromGuid();
                        break;
                }

                if (!AssetDatabase.IsValidFolder(path))
                {
                    Debug.LogError("Trying to save asset at the custom path " + path + ", but folder was not found. Update the path to the folder from the Tools -> CustomVehicleController -> Package Settings window");
                }
                return path;
            }

            path = FindPath() + folderName;
            if (!AssetDatabase.IsValidFolder(path))
            {
                Debug.LogError("Trying to save asset at the default path " + path + ", but folder was not found.");
            }
            return path;
        }
    }

}
