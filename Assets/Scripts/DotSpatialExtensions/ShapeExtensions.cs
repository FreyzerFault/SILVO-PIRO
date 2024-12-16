using System.Linq;
using DotSpatial.Data;
using NetTopologySuite.Geometries;
using SILVO.Terrain;
using UnityEngine;
using Polygon = DavidUtils.Geometry.Polygon;

namespace SILVO.DotSpatialExtensions
{
    public static class ShapeExtensions
    {
        public static Projecter GetImageProjecter(this Shape shape, Vector2Int texSize) => new(shape, texSize);

        public static Polygon GetPolygon(this Shape shape)
        {
            Polygon polygon = new Polygon(shape.GetPoints());
            
            // Dado la vuelta CW -> CCW y limpiar vertices duplicados y ejes superpuestos
            polygon = polygon.Revert();
            polygon.CleanDegeneratePolygon();

            return polygon;
        }
        
        public static Vector2[] GetPoints(this Shape shape)
        {
            // double[] to Vector2[]
            double[] shpVertices = shape.Vertices;
            Vector2[] points = new Vector2[shpVertices.Length / 2];
            for (var i = 0; i < shpVertices.Length / 2; i++) 
                points[i] = new Vector2((float)shpVertices[i * 2], (float)shpVertices[i * 2 + 1]);

            return points;
        }
    }
}
