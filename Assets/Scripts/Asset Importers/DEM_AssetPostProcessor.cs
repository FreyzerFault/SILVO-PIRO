using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DavidUtils.ExtensionMethods;
using DotSpatial.Data;
using Color = UnityEngine.Color;

namespace SILVO.Asset_Importers
{
    public class DEM_AssetPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            string[] tifAssetPath = importedAssets.Where(asset => Path.GetExtension(asset) == ".tif").ToArray();
            tifAssetPath.ForEach(PostProcessTIFF);
        }

        private static void PostProcessTIFF(string path)
        {
            Debug.Log($"<color=lime>Processing TIF file:</color> <b>{path}</b>");
            //
            //
            // WorldFile worldFile = new WorldFile(path);
            // Debug.Log($"World File Loaded: {worldFile.Filename}\n" +
            //           $"{worldFile.CellWidth} x {worldFile.CellHeight} - (0,0) = {worldFile.TopLeftX}, {worldFile.TopLeftY}");;
            //
            // if (ImageData.Open(path) is not TiledImage raster)
            //     throw new FileLoadException($"Failed to open TIFF file", path);
            
            // Debug.Log($"TIFF type: {raster?.GetType()}");
        }
    }
}
