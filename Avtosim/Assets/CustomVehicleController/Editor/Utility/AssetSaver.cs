using Assets.VehicleController;
using UnityEditor;
using UnityEngine;

namespace Assets.VehicleControllerEditor
{
    public static class AssetSaver
    {
        public static bool TrySaveAsset(string folderPath, IVehiclePart vehiclePart, string fileName, string fileExtension)
        {
            if (!PartTypeNameValidator.CheckClassNameIsValid(fileName))
                return false;

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"A folder at path {folderPath} doesn't exist. DO NOT move or delete the folders within CustomVehicleController folder! If you use custom folder path, update it!");
                return false;
            }

            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(folderPath + "\\" + fileName + "." + fileExtension);
            AssetDatabase.CreateAsset(vehiclePart as UnityEngine.Object, uniqueFileName);
            AssetDatabase.SaveAssets();
            Undo.RegisterCreatedObjectUndo(vehiclePart as UnityEngine.Object, "Created Body Asset");

            return true;
        }

        public static bool TrySavePreset(string folderPath, VehiclePartsPresetSO preset, string fileName, string fileExtension)
        {
            if (!PartTypeNameValidator.CheckClassNameIsValid(fileName))
                return false;

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"A folder at path {folderPath} doesn't exist. DO NOT move or delete the folders within CustomVehicleController folder! If you use custom folder path, update it!");
                return false;
            }

            try
            {
                var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(folderPath + "\\" + fileName + "." + fileExtension);
                AssetDatabase.CreateAsset(preset as UnityEngine.Object, uniqueFileName);
                AssetDatabase.SaveAssets();
                Undo.RegisterCreatedObjectUndo(preset as UnityEngine.Object, "Created Body Asset");
                return true;
            }
            catch
            {
                Debug.LogError("An error occured while saving the asset!");
                return false;
            }
        }


        public static CollisionAreasDataSO TryCreateCollisionAreasSO(string folderPath, string fileName, string fileExtension)
        {
            if (!PartTypeNameValidator.CheckClassNameIsValid(fileName))
                return null;

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"A folder at path {folderPath} doesn't exist. DO NOT move or delete the folders within CustomVehicleController folder! If you use custom folder path, update it!");
                return null;
            }

            try
            {
                var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(folderPath + "\\" + fileName + "." + fileExtension);
                CollisionAreasDataSO collisionAreasDataSO = ScriptableObject.CreateInstance<CollisionAreasDataSO>();
                AssetDatabase.CreateAsset(collisionAreasDataSO as UnityEngine.Object, uniqueFileName);
                AssetDatabase.SaveAssets();
                Undo.RegisterCreatedObjectUndo(collisionAreasDataSO as UnityEngine.Object, "Created Collision Area SO");
                return collisionAreasDataSO;
            }
            catch
            {
                Debug.LogError("An error occured while saving the asset!");
                return null;
            }
        }
    }
}
