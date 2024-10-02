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
            // double[] to Coordinate[]
            double[] shpVertices = shape.Vertices;
            Coordinate[] vertices = new Coordinate[shpVertices.Length / 2];
            for (var i = 0; i < shpVertices.Length / 2; i++) 
                vertices[i] = new Coordinate(shpVertices[i * 2], shpVertices[i * 2 + 1]);
            
            Polygon polygon = new Polygon(vertices.Select(c => new Vector2((float)c.X, (float)c.Y)).ToArray());
            
            // Dado la vuelta CW -> CCW y limpiar vertices duplicados y ejes superpuestos
            polygon = polygon.Revert();
            polygon.CleanDegeneratePolygon();

            return polygon;
        }
    }
}
