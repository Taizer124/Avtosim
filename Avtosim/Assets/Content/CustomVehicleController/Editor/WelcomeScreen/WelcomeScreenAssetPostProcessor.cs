using UnityEditor;
using UnityEngine;
namespace Assets.VehicleControllerEditor
{
    public class WelcomeScreenAssetPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (!didDomainReload)
                return;

            for (int i = 0; i < importedAssets.Length; i++)
            {
                if (importedAssets[i].Contains("CustomVehicleController"))
                {
                    WelcomeScreen.DisplayWelcomeWindow();
                    break;
                }
            }
        }
    }
}
