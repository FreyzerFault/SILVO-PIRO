using DavidUtils.ExtensionMethods;
using DavidUtils.Geometry.Bounding_Box;
using DotSpatial.Data;
using UnityEngine;

namespace SILVO.DotSpatialExtensions
{
    public static class DataExtensions
    {
        public static Extent ToExtent(this Vector2 origin, Vector2 size) =>
            new(origin.x, origin.y, origin.x + size.x, origin.y + size.y);
        
        public static Extent ToExtent(this Vector2 origin, Vector2Int size) => ToExtent(origin, size.ToVector2());
        public static Extent ToExtent(this Vector2Int origin, Vector2Int size) => ToExtent(origin.ToVector2(), size.ToVector2());
        public static Extent ToExtent(this Vector2Int origin, Vector2 size) =>ToExtent(origin.ToVector2(), size);
    }
}
