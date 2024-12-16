using System;
using System.Data;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DotSpatial.Data;
using SILVO.DotSpatialExtensions;
using SILVO.Terrain;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SILVO.Asset_Importers
{
    [ScriptedImporter(1, "shp")]
    public class SHP_Importer: ScriptedImporter
    {
        public Shapefile_Component shpfileComponent;
        
        public int maxSubPolygonCount = 10;
        public int terrainOffset = 200;
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string path = ctx.assetPath;
            
            shpfileComponent = Shapefile_Component.InstantiateShapefile(path, maxSubPolygonCount);
            
            ShapefileExtensions.DebugAllSHPInfo(shpfileComponent.Shpfile);
            
            ctx.AddObjectToAsset("Main Obj", shpfileComponent.gameObject);
            ctx.AddObjectToAsset("SHP", shpfileComponent);
            
            shpfileComponent.ShapeTextures.ForEach((t,i) => ctx.AddObjectToAsset($"Texture {i}", t));

            Mesh[] allMeshes = shpfileComponent.Meshes;
            allMeshes?.ForEach((m,i) => ctx.AddObjectToAsset($"Mesh{i}", m));
            
            ctx.SetMainObject(shpfileComponent.gameObject);
        }
    }
}
