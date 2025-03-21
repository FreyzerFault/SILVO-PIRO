using DavidUtils.ExtensionMethods;
using DavidUtils.Geometry.Bounding_Box;
using DotSpatial.Data;
using UnityEngine;

namespace SILVO.GEO_Tools.DotSpatialExtensions
{
    public static class DataExtensions
    {
        public static Extent ToExtent(this Vector2 origin, Vector2 size) =>
            new(origin.x, origin.y, origin.x + size.x, origin.y + size.y);
        
        public static Extent ToExtent(this Vector2 origin, Vector2Int size) => ToExtent(origin, size.ToVector2());
        public static Extent ToExtent(this Vector2Int origin, Vector2Int size) => ToExtent(origin.ToVector2(), size.ToVector2());
        public static Extent ToExtent(this Vector2Int origin, Vector2 size) =>ToExtent(origin.ToVector2(), size);
        
        public static Extent ToExtent(this AABB_2D aabb) => new(aabb.min.x, aabb.min.y, aabb.max.x, aabb.max.y);
        public static AABB_2D ToAABB(this Extent extent) => new(new Vector2((float)extent.MinX, (float)extent.MinY), new Vector2((float)extent.MaxX, (float)extent.MaxY));
    }
}
