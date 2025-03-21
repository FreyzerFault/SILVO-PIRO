using DavidUtils.ExtensionMethods;
using SILVO.GEO_Tools.DotSpatialExtensions;
using SILVO.GEO_Tools.SHP;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SILVO.GEO_Tools.Asset_Importers
{
    [ScriptedImporter(1, "shp")]
    public class SHP_Importer: ScriptedImporter
    {
        public Shapefile_Component shpfileComponent;

        public int maxSubPolygonCount = 10;
        public int previewTextureResolution = 128;
        public bool onTerrain = true;
        public int terrainOffset = 200;
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string path = ctx.assetPath;
            
            shpfileComponent = Shapefile_Component.InstantiateShapefile(path, maxSubPolygonCount, onTerrain, terrainOffset);
            
            ShapefileExtensions.DebugAllSHPInfo(shpfileComponent.Shpfile);
            
            ctx.AddObjectToAsset("Main Obj", shpfileComponent.gameObject);
            ctx.AddObjectToAsset("SHP", shpfileComponent);
            
            shpfileComponent.UpdateTexture(previewTextureResolution);
            ctx.AddObjectToAsset("Texture", shpfileComponent.previewTexture);
            
            // shpfileComponent.ShapeTextures.ForEach((t,i) => ctx.AddObjectToAsset($"Texture {i}", t));

            Mesh[] allMeshes = shpfileComponent.Meshes;
            allMeshes?.ForEach((m,i) => ctx.AddObjectToAsset($"Mesh{i}", m));
            
            ctx.SetMainObject(shpfileComponent.gameObject);
        }
    }
}
