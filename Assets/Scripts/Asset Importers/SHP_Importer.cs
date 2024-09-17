using System.Linq;
using DavidUtils.Geometry;
using DavidUtils.Rendering;
using DotSpatial.Data;
using DotSpatial.Projections;
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

            FeatureList features = shp.Features as FeatureList;
            Debug.Log($"Imported {shp}: {shp.Features.Count} features");
            
            
            Debug.Log($"Feature Type: {shp.Features[0].FeatureType}");
            
            Debug.Log("Fields: " + string.Join(", ", shp.DataTable.Columns));
            Debug.Log($"{shp.DataTable.Rows.Count} rows");
            Debug.Log($"{shp.DataTable.Columns.Count} columns");
            Debug.Log($"Bounds: {shp.Extent}");
            
            // SHAPE
            Shape shape = shp.GetShape(0, true);
            Debug.Log($"Vertices: {string.Join(", ", shape.Vertices)}");
            
            // EXTENT
            Extent extent = shape.Range.CalculateExtents();
            var min = new Vector2((float)extent.MinX, (float)extent.MinY);
            var max = new Vector2((float)extent.MaxX, (float)extent.MaxY);
            var width = (float)extent.Width;
            var height = (float)extent.Height;
            Debug.Log($"Extent: [min {min}, max {max}]. Width: {width}, Height: {height}");
            
            // POLYGON
            Polygon poly = CreatePolygon(shape);
            Debug.Log($"Vertices: {string.Join(", ", poly.Vertices)}");
            
            poly.NormalizeMinMax(min, max);
            Debug.Log($"Centroid: {poly.centroid}");
            
            Debug.Log($"Vertices Normalized: {string.Join(", ", poly.Vertices)}");
            
            
            // OBJECT
            var obj = new GameObject();
            var polyRenderer = obj.AddComponent<PolygonRenderer>();
            polyRenderer.Polygon = poly;
            // polyRenderer.RenderMode = PolygonRenderer.PolygonRenderMode.OutlinedMesh;
            // polyRenderer.Color = Color.gray;
            // polyRenderer.Thickness = 0.1f;
            // polyRenderer.OutlineColor = Color.magenta;
            
            ctx.AddObjectToAsset("Main Obj", obj);
            ctx.AddObjectToAsset("Renderer", polyRenderer);
            ctx.AddObjectToAsset("Mesh", polyRenderer.Mesh);
            ctx.SetMainObject(obj);
        }
        
        private Polygon CreatePolygon(Shape shape)
        {
            Vector2[] vertices = new Vector2[shape.Vertices.Length / 2];
            for (int i = 0; i < shape.Vertices.Length / 2; i++) 
                vertices[i] = new Vector2((float)shape.Vertices[i*2], (float)shape.Vertices[i*2 + 1]);
            return new Polygon(vertices);
        }
    }
}
