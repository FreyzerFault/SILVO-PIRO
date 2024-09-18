using System.IO;
using System.Linq;
using DavidUtils.ExtensionMethods;
using UnityEditor;
using UnityEngine;

namespace SILVO.Asset_Importers
{
    public class SHP_AssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets) Debug.Log($"<color=lime>Imported Asset:</color> {str}");
            foreach (string str in deletedAssets) Debug.Log($"<color=red>Deleted Asset:</color> {str}");

            for (var i = 0; i < movedFromAssetPaths.Length; i++)
            {
                string moved = movedAssets[i], movedFrom = movedFromAssetPaths[i];
                Debug.Log($"<color=teal>Moved Asset:</color> {movedFrom} to {moved}");
            }
            
            // if (didDomainReload)
            // {
            //     Debug.Log("Domain Reloaded");
            // }
            
            string[] shpAssetPaths = importedAssets.Where(asset => Path.GetExtension(asset) == ".shp").ToArray();
            shpAssetPaths.ForEach(PostProcessSHP);
        }

        private static void PostProcessSHP(string path)
        {
            Debug.Log($"Processing SHP file: {path}");
        }
    }
}
