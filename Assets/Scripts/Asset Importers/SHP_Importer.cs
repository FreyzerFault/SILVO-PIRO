using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using DavidUtils.Rendering;
using DavidUtils.ExtensionMethods;
using DavidUtils.Geometry.Bounding_Box;
using DotSpatial.Data;
using NetTopologySuite.Geometries;
using UnityEditor.AssetImporters;
using UnityEngine;
using Point = System.Drawing.Point;
using Polygon = DavidUtils.Geometry.Polygon;

namespace SILVO.Asset_Importers
{
    [ScriptedImporter(1, "shp")]
    public class SHP_Importer: ScriptedImporter
    {
        class Projecter : IProj
        {
            public Rectangle ImageRectangle { get; }
            public Extent GeographicExtents { get; }
             
            public Vector2 RectSize => new Vector2(ImageRectangle.Size.Width,  ImageRectangle.Size.Height);
            public Vector2 ExtentSize => new Vector2((float)GeographicExtents.Width, (float)GeographicExtents.Height);
            
            public Projecter(Rectangle imageRectangle, Extent geographicExtents)
            {
                ImageRectangle = imageRectangle;
                GeographicExtents = geographicExtents;
            }
            
            public Projecter(Extent geographicExtents)
            {
                Vector3 terrainSize = UnityEngine.Terrain.activeTerrain.terrainData.size;
                ImageRectangle = new Rectangle(0, 0, (int)terrainSize.x, (int)terrainSize.z);
                GeographicExtents = geographicExtents;
            }
            public Projecter(Shape shape) : this(shape.Range.Extent) { }
            
            public Vector2 ReprojectPoint(Vector2 p)
            {
                Point drawPoint = this.ProjToPixel(new Coordinate(p.x, p.y));
                // Y Starts from top
                return new Vector2(drawPoint.X, ImageRectangle.Height - drawPoint.Y);
            }
            
            public Polygon ReprojectPolygon(Polygon p) => 
                new Polygon(p.Vertices.Select(ReprojectPoint).ToArray());
        }
        
        [SerializeField] public Texture2D texture;
        [SerializeField] public Polygon polygon;
        
        public Vector2 texSize = new Vector2(128, 128);
            
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
            polygon = CreatePolygonReprojected(shape);

            Polygon distinctVerticesPoly = new Polygon(polygon.Vertices.Distinct().ToArray());
            Debug.Log($"Polygon Created. {polygon.VertexCount} -> {distinctVerticesPoly.VertexCount} sin duplicados");
            
            Polygon normPoly = distinctVerticesPoly.NormalizeMinMax(Vector2.zero, new Projecter(shape.Range.Extent).RectSize);
            
            // TEXTURE
            texture = normPoly.ToTexture(texSize);
            Debug.Log($"<color=cyan>Texture Created. Size: {new Vector2(texture.width, texture.height)}</color>");
            
            // RENDERER OBJECT
            PolygonRenderer polyRenderer = CreatePolygonRenderer(distinctVerticesPoly, shp);
            
            ctx.AddObjectToAsset("Main Obj", polyRenderer.gameObject);
            ctx.AddObjectToAsset("MeshFilter", polyRenderer.Mesh);
            ctx.AddObjectToAsset("Texture", texture);
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
            polyRenderer.generateSubPolygons = true;
            polyRenderer.Polygon = polygon;

            return polyRenderer;
        }

        #endregion
        

        #region POLYGON

        private Polygon CreatePolygon(Shape shape, bool normalize = false)
        {
            Coordinate[] vertices = new Coordinate[shape.Vertices.Length / 2];
            for (var i = 0; i < shape.Vertices.Length / 2; i++) 
                vertices[i] = new Coordinate(shape.Vertices[i * 2], shape.Vertices[i * 2 + 1]);
            
            Polygon poly = new Polygon(vertices.Select(c => new Vector2((float)c.X, (float)c.Y)).ToArray());
            
            // Dado la vuelta CW -> CCW y limpiar vertices duplicados y ejes superpuestos
            poly = poly.Revert();
            poly.CleanDegeneratePolygon();
            
            if (normalize)
            {
                poly = poly.NormalizeMinMax(
                    new Vector2((float)shape.Range.Extent.MinX, (float)shape.Range.Extent.MinY),
                    new Vector2((float)shape.Range.Extent.MaxX, (float)shape.Range.Extent.MaxY)
                );
            }

            var polyVerticesStr = $"{string.Join(", ", poly.Vertices.Take(10))} {(poly.Vertices.Length > 10 ? "..." : "")}";
            var polyAABB = new AABB_2D(poly);

            Debug.Log($"<b>Polygon Extracted: ({poly.VertexCount} vertices)</b>\n" +
                      $"Vertices: <color=teal>{polyVerticesStr}</color>\n" +
                      $"Centroid: <color=cyan>{poly.centroid}\n</color>" +
                      $"AABB: <color=orange>{polyAABB}</color>\n");
            
            return poly;
        }
        
        private Polygon CreatePolygonReprojected(Shape shape, bool normalize = false)
        {
            // PROJECTION
            var projecter = new Projecter(shape);
            
            Coordinate[] vertices = new Coordinate[shape.Vertices.Length / 2];
            for (var i = 0; i < shape.Vertices.Length / 2; i++) 
                vertices[i] = new Coordinate(shape.Vertices[i * 2], shape.Vertices[i * 2 + 1]);
            
            Polygon poly = new Polygon(vertices.Select(c => new Vector2((float)c.X, (float)c.Y)).ToArray());
            
            // Dado la vuelta CW -> CCW y limpiar vertices duplicados y ejes superpuestos
            poly = poly.Revert();
            poly.CleanDegeneratePolygon();
            
            // REPROJECTION to TERRAIN
            var reprojectedPoly = projecter.ReprojectPolygon(poly);
            
            if (normalize)
            {
                poly = poly.NormalizeMinMax(
                    new Vector2((float)shape.Range.Extent.MinX, (float)shape.Range.Extent.MinY),
                    new Vector2((float)shape.Range.Extent.MaxX, (float)shape.Range.Extent.MaxY)
                );
                reprojectedPoly = reprojectedPoly.NormalizeMinMax(
                    Vector2.zero,
                    projecter.RectSize
                );
            }

            var reprojPolyVerticesStr = $"{string.Join(", ", reprojectedPoly.Vertices.Take(10))} {(reprojectedPoly.Vertices.Length > 10 ? "..." : "")}";
            var polyVerticesStr = $"{string.Join(", ", poly.Vertices.Take(10))} {(poly.Vertices.Length > 10 ? "..." : "")}";
            var polyAABB = new AABB_2D(poly);
            var reprojPolyAABB = new AABB_2D(reprojectedPoly);
            
            Debug.Log($"<b>Polygon Extracted: ({poly.VertexCount} vertices)</b>\n" +
                      $"Vertices: <color=teal>{polyVerticesStr}</color>\n" +
                      $"Centroid: <color=cyan>{poly.centroid}\n</color>" +
                      $"AABB: <color=orange>{polyAABB}</color>\n" +
                      
                      $"Reprojected Polygon:\n" +
                      $"Reprojected Vertices: <color=teal>{reprojPolyVerticesStr}</color>\n" +
                      $"Reprojected Centroid: <color=cyan>{reprojectedPoly.centroid}</color>\n" +
                      $"Reprojected AABB: <color=orange>{reprojPolyAABB}</color>\n");
            
            return reprojectedPoly;
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

            return $"<b>Shape: ({shape.Vertices.Length / 2} vertices)</b>\n" +
                      $"Extent: <color=orange>[MIN {min}, MAX {max}]. Width: {width}, Height: {height}</color>\n" +
                      $"Vertices: <color=teal>{string.Join(", ", shape.Vertices.Take(10))} {(shape.Vertices.Length > 10 ? "..." : "")}</color>\n";
        }

        #endregion
    }
}
