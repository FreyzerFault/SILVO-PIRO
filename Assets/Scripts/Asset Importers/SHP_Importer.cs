using System;
using System.Data;
using System.Linq;
using DavidUtils.Geometry;
using DavidUtils.Rendering;
using DavidUtils.ExtensionMethods;
using DotSpatial.Data;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SILVO.Asset_Importers
{
    [ScriptedImporter(1, "shp")]
    public class SHP_Importer: ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string path = ctx.assetPath;

            if (Shapefile.OpenFile(path) is not PolygonShapefile shp)
            {
                Debug.LogError($"Failed to open SHP file in {path}");
                return;
            }

            // SHAPEFILE INFO
            Debug.Log(ParseShapeFile(shp));

            // DATA TABLE
            Debug.Log(ParseTable(shp.DataTable));
            
            // SHAPE
            Shape shape = shp.GetShape(0, true);
            Debug.Log(ParseShape(shape));
            
            // POLYGON
            Polygon poly = CreatePolygon(shape, true);
            
            // RENDERER OBJECT
            PolygonRenderer polyRenderer = CreatePolygonRenderer(poly, shp);
            
            
            ctx.AddObjectToAsset("Main Obj", polyRenderer.gameObject);
            ctx.AddObjectToAsset("MeshFilter", polyRenderer.Mesh);
            ctx.SetMainObject(polyRenderer.gameObject);
        }
        


        #region OBJECT

        private PolygonRenderer CreatePolygonRenderer(Polygon polygon, Shapefile shp)
        {
            var obj = new GameObject();
            obj.AddComponent<MeshRenderer>();
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<LineRenderer>();
            
            var polyRenderer = obj.AddComponent<PolygonRenderer>();
            polyRenderer.Polygon = polygon;

            return polyRenderer;
        }

        #endregion
        

        #region POLYGON
        
        private Polygon CreatePolygon(Shape shape, bool normalize = false)
        {
            Vector2[] vertices = new Vector2[shape.Vertices.Length / 2];
            for (int i = 0; i < shape.Vertices.Length / 2; i++) 
                vertices[i] = new Vector2((float)shape.Vertices[i*2], (float)shape.Vertices[i*2 + 1]);
            
            Polygon poly = new Polygon(vertices).Revert();
            
            if (normalize)
                poly.NormalizeMinMax(
                    new Vector2((float)shape.Range.Extent.MinX, (float)shape.Range.Extent.MinY), 
                    new Vector2((float)shape.Range.Extent.MaxX, (float)shape.Range.Extent.MaxY)
                );
            
            Debug.Log($"<b>Polygon Extracted: ({poly.VertexCount} vertices)</b>\n" +
                      $"Vertices: <color=teal>{string.Join(", ", poly.Vertices.Take(10))} {(poly.Vertices.Length > 10 ? "..." : "")}</color>\n" +
                      $"Centroid: <color=cyan>{poly.centroid}\n</color>");
            
            return poly;
        }

        #endregion


        #region SHAPEFILE INFO

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

        #endregion


        #region DATA TABLE
        
        struct DataTableLogConfig
        {
            public int maxColLog, maxRowLog, maxItemChars;
            public string colSeparator;
        }
        DataTableLogConfig dtConfig = new()
        {
            maxColLog = 8, maxRowLog = 20, maxItemChars = 10, colSeparator = " | "
        };
        
        private string ParseTable(DataTable table)
        {
            DataRowCollection rows = table.Rows;
            DataColumnCollection cols = table.Columns;
            
            return "<color=cyan>DATA TABLE:</color>\n" +
                   $"<b>{ParseColumnCollection(cols)} {(cols.Count > dtConfig.maxColLog ? "..." : "")}</b>\n" +
                   $"{string.Join("\n", rows.Cast<DataRow>().Select(ParseRow))} {(cols.Count > dtConfig.maxColLog ? "..." : "")}";
        }
        
        private string ParseColumnCollection(DataColumnCollection col) =>
            string.Join(dtConfig.colSeparator, col.Cast<DataColumn>().Take(dtConfig.maxColLog).Select(c => c.ColumnName.TruncateFixedSize(dtConfig.maxItemChars)));

        private string ParseRow(DataRow row) =>
            string.Join(dtConfig.colSeparator, row.ItemArray.Take(dtConfig.maxColLog).Select(item => item.ToString().TruncateFixedSize(dtConfig. maxItemChars)));
        

        #endregion


        #region SHAPE

        private static string ParseShape(Shape shape)
        {
            // EXTENT
            Extent extent = shape.Range.CalculateExtents();
            var min = new Vector2((float)extent.MinX, (float)extent.MinY);
            var max = new Vector2((float)extent.MaxX, (float)extent.MaxY);
            var width = (float)extent.Width;
            var height = (float)extent.Height;

            return $"<b>Shape: ({shape.Vertices.Length} vertices)</b>\n" +
                      $"Extent: <color=orange>[MIN {min}, MAX {max}]. Width: {width}, Height: {height}</color>\n" +
                      $"Vertices: <color=teal>{string.Join(", ", shape.Vertices.Take(10))} {(shape.Vertices.Length > 10 ? "..." : "")}</color>\n";
        }

        #endregion
    }
}
