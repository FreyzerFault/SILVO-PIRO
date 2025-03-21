using UnityEngine;

namespace SILVO.GEO_Tools.Asset_Importers
{
    public class MyAssetPostprocessor : UnityEditor.AssetPostprocessor
    {
        /// <summary>
        /// Monitor imported files
        /// </summary>
        /// <param name="importedAssets">Imported</param>
        /// <param name="deletedAssets">Deleted</param>
        /// <param name="movedAssets">MovedTo</param>
        /// <param name="movedFromAssetPaths">MovedFrom</param>
        private static void OnPostprocessAllAssets(
            string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths
            )
        {
            foreach (string str in importedAssets) Debug.Log($"<color=lime>Imported Asset:</color> {str}");
            foreach (string str in deletedAssets) Debug.Log($"<color=red>Deleted Asset:</color> {str}");

            for (var i = 0; i < movedFromAssetPaths.Length; i++)
            {
                string moved = movedAssets[i], movedFrom = movedFromAssetPaths[i];
                Debug.Log($"<color=teal>Moved Asset:</color> {movedFrom} to {moved}");
            }
        }
    }
}
