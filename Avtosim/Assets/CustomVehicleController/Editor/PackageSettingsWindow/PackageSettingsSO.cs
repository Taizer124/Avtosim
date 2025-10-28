using System;
using UnityEditor;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    public class PackageSettingsSO : ScriptableObject
    {
        public bool DefaultSavePaths = true;

        public const string NO_PATH_PROVIDED_TEXT = "No path provided";

        public void UpdateDisplayInfo()
        {
            UpdateDisplayName(ref EnginePathDisplay, _enginePathGUID);
            UpdateDisplayName(ref TransmissionPathDisplay, _transmissionPathGUID);
            UpdateDisplayName(ref FIPathDisplay, _fiPathGUID);
            UpdateDisplayName(ref SuspensionPathDisplay, _suspensionPathGUID);
            UpdateDisplayName(ref TiresPathDisplay, _tiresPathGUID);
            UpdateDisplayName(ref BrakesPathDisplay, _brakesPathGUID);
            UpdateDisplayName(ref BodiesPathDisplay, _bodiesPathGUID);
            UpdateDisplayName(ref NitroPathDisplay, _nitroPathGUID);
            UpdateDisplayName(ref PresetPathDisplay, _presetPathGUID);
            UpdateDisplayName(ref EnginePartsPathDisplay, _enginePartsPathGUID);
            UpdateDisplayName(ref CollisionAreasPathDisplay, _collisionAreasPathGUID);
        }

        public void ResetInfo()
        {
            DefaultSavePaths = true;
            _enginePathGUID = "";
            _transmissionPathGUID = "";
            _fiPathGUID = "";
            _suspensionPathGUID = "";
            _tiresPathGUID = "";
            _brakesPathGUID = "";
            _bodiesPathGUID = "";
            _nitroPathGUID = "";
            _presetPathGUID = "";
            _enginePartsPathGUID = "";
            _collisionAreasPathGUID = "";
            UpdateDisplayInfo();
        }

        public string EnginePathDisplay;
        [SerializeField]
        private string _enginePathGUID;
        public void SetEngineGuid(string guid)
        {
            UpdateDisplayName(ref EnginePathDisplay, guid);
            _enginePathGUID = guid;
        }
        public string GetEnginePathFromGuid() => AssetDatabase.GUIDToAssetPath(_enginePathGUID);

        public string FIPathDisplay;
        [SerializeField]
        private string _fiPathGUID;
        public void SetFIGuid(string guid)
        {
            UpdateDisplayName(ref FIPathDisplay, guid);
            _fiPathGUID = guid;
        }
        public string GetFIPathFromGuid() => AssetDatabase.GUIDToAssetPath(_fiPathGUID);

        public string TransmissionPathDisplay;
        [SerializeField]
        private string _transmissionPathGUID;
        public void SetTransmissionGuid(string guid)
        {
            UpdateDisplayName(ref TransmissionPathDisplay, guid);
            _transmissionPathGUID = guid;
        }
        public string GetTransmissionPathFromGuid() => AssetDatabase.GUIDToAssetPath(_transmissionPathGUID);

        public string TiresPathDisplay;
        [SerializeField]
        private string _tiresPathGUID;
        public void SetTiresGuid(string guid)
        {
            UpdateDisplayName(ref TiresPathDisplay, guid);
            _tiresPathGUID = guid; 
        }
        public string GetTiresPathFromGuid() => AssetDatabase.GUIDToAssetPath(_tiresPathGUID);

        public string BrakesPathDisplay;
        [SerializeField]
        private string _brakesPathGUID;
        public void SetBrakesGuid(string guid) 
        {
            UpdateDisplayName(ref BrakesPathDisplay, guid);
            _brakesPathGUID = guid; 
        }
        public string GetBrakesPathFromGuid() => AssetDatabase.GUIDToAssetPath(_brakesPathGUID);

        public string SuspensionPathDisplay;
        [SerializeField]
        private string _suspensionPathGUID;
        public void SetSuspensionGuid(string guid)
        {
            UpdateDisplayName(ref SuspensionPathDisplay, guid);
            _suspensionPathGUID = guid;
        }
        public string GetSuspensionPathFromGuid() => AssetDatabase.GUIDToAssetPath(_suspensionPathGUID);

        public string BodiesPathDisplay;
        [SerializeField]
        private string _bodiesPathGUID;
        public void SetBodiesGuid(string guid)
        {
            UpdateDisplayName(ref BodiesPathDisplay, guid);
            _bodiesPathGUID = guid;
        }
        public string GetBodiesPathFromGuid() => AssetDatabase.GUIDToAssetPath(_bodiesPathGUID);

        public string NitroPathDisplay;
        [SerializeField]
        private string _nitroPathGUID;
        public void SetNitroGuid(string guid)
        {
            UpdateDisplayName(ref NitroPathDisplay, guid);
            _nitroPathGUID = guid;
        }
        public string GetNitroPathFromGuid() => AssetDatabase.GUIDToAssetPath(_nitroPathGUID);

        public string PresetPathDisplay;
        [SerializeField]
        private string _presetPathGUID;
        public void SetPresetGuid(string guid)
        {
            UpdateDisplayName(ref PresetPathDisplay, guid);
            _presetPathGUID = guid;
        }
        public string GetPresetPathFromGuid() => AssetDatabase.GUIDToAssetPath(_presetPathGUID);

        public string EnginePartsPathDisplay;
        [SerializeField]
        private string _enginePartsPathGUID;
        public void SetEnginePartsGuid(string guid)
        {
            UpdateDisplayName(ref EnginePartsPathDisplay, guid);
            _enginePartsPathGUID = guid;
        }
        public string GetEnginePartsPathFromGuid() => AssetDatabase.GUIDToAssetPath(_enginePartsPathGUID);

        public string CollisionAreasPathDisplay;
        [SerializeField]
        private string _collisionAreasPathGUID;
        public void SetCollisionAreasGuid(string guid)
        {
            UpdateDisplayName(ref CollisionAreasPathDisplay, guid);
            _collisionAreasPathGUID = guid;
        }
        public string GetCollisionAreasPathFromGuid() => AssetDatabase.GUIDToAssetPath(_collisionAreasPathGUID);

        private void UpdateDisplayName(ref string displayField, string guid)
        {
            if(guid == null || guid == "")
            {
                displayField = NO_PATH_PROVIDED_TEXT;
                return;
            }

            displayField = AssetDatabase.GUIDToAssetPath(guid);
        }
    }
}
