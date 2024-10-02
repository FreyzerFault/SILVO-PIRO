using System;
using System.Data;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DavidUtils.Rendering;
using DotSpatial.Data;
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
            
            Shapefile shp = Shapefile.OpenFile(path);
            if (shp == null)
            {
                Debug.LogError($"Failed to open SHP file in {path}");
                return;
            }
            
            DebugAllSHPInfo(shp);

            shpfileComponent = Shapefile_Component.InstantiateShapefile(shp, maxSubPolygonCount);
            shpfileComponent.ShpComponents.ForEach(shpComp => shpComp.transform.localPosition += Vector3.up * terrainOffset);
            
            
            ctx.AddObjectToAsset("Main Obj", shpfileComponent.gameObject);
            ctx.AddObjectToAsset("SHP", shpfileComponent);
            
            shpfileComponent.ShapeTextures.ForEach((t,i) => ctx.AddObjectToAsset($"Texture {i}", t));

            Mesh[] allMeshes = shpfileComponent.Meshes;
            allMeshes?.ForEach((m,i) => ctx.AddObjectToAsset($"Mesh{i}", m));
            
            ctx.SetMainObject(shpfileComponent.gameObject);
        }
        
        
        #region INFO
        
        private static void DebugAllSHPInfo(Shapefile shp)
        {
            // SHAPEFILE INFO
            Debug.Log(ParseShapeFile(shp));

            // DATA TABLE
            Debug.Log(ParseTable(shp.DataTable));
            
            // SHAPE
            Shape shape = shp.GetShape(0, true);
        }
        
        private static string ParseShapeFile(Shapefile shp)
        {
            if (shp.Features is not FeatureList features) 
                throw new Exception("No features in SHP file. Can't work with it");
            
            FeatureType[] types = features.Select(f => f.FeatureType).ToArray();
            
            return $"<color=lime>Imported <i><b>{shp.Name}</b></i>:</color> <b>{shp.Features.Count} features</b>\n" +
                   $"Types: <color=cyan>{(types.Length == 1 ? shp.FeatureType : string.Join(", ", types))}</color>\n" +
                   $"Projection: <color=yellow>{shp.ProjectionString}</color>\n" +
                   $"Bounds: <color=orange>{shp.Extent}</color>";
        }
        
        private static string ParseShape(Shape shape)
        {
            // EXTENT
            Extent extent = shape.Range.CalculateExtents();
            var min = new Vector2((float)extent.MinX, (float)extent.MinY);
            var max = new Vector2((float)extent.MaxX, (float)extent.MaxY);
            var width = (float)extent.Width;
            var height = (float)extent.Height;

            return $"<b>Shape: ({shape.Vertices.Length / 2} vertices)</b>\n" +
                   $"Extent: <color=orange>[MIN {min}, MAX {max}]. Width: {width}, Height: {height}</color>\n" +
                   $"Vertices: <color=teal>{string.Join(", ", shape.Vertices.Take(10))} {(shape.Vertices.Length > 10 ? "..." : "")}</color>\n";
        }
        
        
        #region DATA TABLE
        
        private struct DataTableLogConfig
        {
            public int maxColLog, maxRowLog, maxItemChars;
            public string colSeparator;
        }
        private static DataTableLogConfig dtConfig = new()
        {
            maxColLog = 8, maxRowLog = 20, maxItemChars = 10, colSeparator = " | "
        };
        
        private static string ParseTable(DataTable table)
        {
            DataRowCollection rows = table.Rows;
            DataColumnCollection cols = table.Columns;
            
            return "<color=cyan>DATA TABLE:</color>\n" +
                   $"<b>{ParseColumnCollection(cols)} {(cols.Count > dtConfig.maxColLog ? "..." : "")}</b>\n" +
                   $"{string.Join("\n", rows.Cast<DataRow>().Select(ParseRow))} {(cols.Count > dtConfig.maxColLog ? "..." : "")}";
        }
        
        private static string ParseColumnCollection(DataColumnCollection col) =>
            string.Join(dtConfig.colSeparator, col.Cast<DataColumn>().Take(dtConfig.maxColLog).Select(c => c.ColumnName.TruncateFixedSize(dtConfig.maxItemChars)));

        private static string ParseRow(DataRow row) =>
            string.Join(dtConfig.colSeparator, row.ItemArray.Take(dtConfig.maxColLog).Select(item => item.ToString().TruncateFixedSize(dtConfig. maxItemChars)));
        

        #endregion

        #endregion
    }
}
