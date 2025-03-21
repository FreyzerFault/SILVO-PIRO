using System.Collections.Generic;
using System.Linq;
using DotSpatial.Data;
using NetTopologySuite.Geometries;
using UnityEngine;
using Polygon = DavidUtils.Geometry.Polygon;

namespace SILVO.GEO_Tools.DotSpatialExtensions
{
    public static class ShapeExtensions
    {
        public static Projecter GetImageProjecter(this Shape shape, Vector2Int texSize) => new(shape, texSize);

        public static Polygon GetPolygon(this Shape shape)
        {
            Polygon polygon = new(shape.GetPoints());
            
            // Dado la vuelta CW -> CCW y limpiar vertices duplicados y ejes superpuestos
            polygon = polygon.Revert();
            polygon.CleanDegeneratePolygon();

            return polygon;
        }
        
        public static Vector2[] GetPoints(this Shape shape)
        {
            int start = shape.Range.StartIndex;
            int numPoints = shape.Range.NumPoints;
            
            // double[] to Vector2[]
            double[] shpVertices = shape.Vertices;
            Vector2[] points = new Vector2[shpVertices.Length / 2];
            for (var i = 0; i < shpVertices.Length / 2; i++) 
                points[(i + start) % numPoints] = new Vector2((float)shpVertices[i * 2], (float)shpVertices[i * 2 + 1]);

            return points;
        }
        
        /// <summary>
        /// Join Single Point Shapes into 1 MultiPoint Shape
        /// </summary>
        /// <param name="shapes">All must be Point Shapes</param>
        /// <returns>One MultiPoint Shape</returns>
        public static Shape ToMultiPoint(this IEnumerable<Shape> shapes)
        {
            Point[] geom = shapes.Select(s => new Point(s.Vertices[0], s.Vertices[1])).ToArray();
            MultiPoint mp = new(geom);
            
            return new Shape(mp, FeatureType.MultiPoint);
        }
    }
}
