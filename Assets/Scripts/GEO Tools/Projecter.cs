using System;
using System.Drawing;
using System.Linq;
using DavidUtils.Geometry.Bounding_Box;
using DotSpatial.Data;
using DotSpatial.Projections.Transforms;
using NetTopologySuite.Geometries;
using SILVO.GEO_Tools.DotSpatialExtensions;
using UnityEngine;
using Point = System.Drawing.Point;
using Polygon = DavidUtils.Geometry.Polygon;

namespace SILVO.GEO_Tools
{
    [Serializable]
    public class Projecter : IProj
    {
        public Rectangle ImageRectangle { get; }
        public Extent GeographicExtents { get; }
         
        public Vector2 RectSize => new(ImageRectangle.Size.Width,  ImageRectangle.Size.Height);
        public Vector2 ExtentSize => new((float)GeographicExtents.Width, (float)GeographicExtents.Height);


        public Projecter(AABB_2D aabb) : this(aabb.ToExtent()) { }
        public Projecter(AABB_2D aabb, Vector2 targetSize) : this(aabb.ToExtent(), targetSize) { }        
        public Projecter(AABB_2D aabb, Rectangle imageRectangle) : this(aabb.ToExtent(), imageRectangle) { }
        
        public Projecter(Extent geographicExtents, Rectangle imageRectangle)
        {
            GeographicExtents = geographicExtents;
            ImageRectangle = imageRectangle;
        }

        public Projecter(Extent geographicExtents, Vector2 targetSize) 
            : this(geographicExtents, new Rectangle(0, 0, (int)targetSize.x, (int)targetSize.y)) { }
        
        public Projecter(Extent geographicExtents)
            : this(geographicExtents, new Rectangle(0, 0,
                (int)Terrain.activeTerrain.terrainData.size.x,
                (int)Terrain.activeTerrain.terrainData.size.z)) 
        { }
        
        public Projecter(Shape shape, Vector2 targetSize) : this(shape.Range.Extent, targetSize) { }
        public Projecter(Shape shape) : this(shape.Range.Extent) { }
        
        public Vector2 ReprojectPoint(Vector2 p)
        {
            Point drawPoint = this.ProjToPixel(new Coordinate(p.x, p.y));
            // Y Starts from top
            return new Vector2(drawPoint.X, ImageRectangle.Height - drawPoint.Y);
        }
        
        public Polygon ReprojectPolygon(Polygon p) => 
            p.IsEmpty
                ? p
                : new Polygon(p.Vertices.Select(ReprojectPoint).ToArray());
    }
}
