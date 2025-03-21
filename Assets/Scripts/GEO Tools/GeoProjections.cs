using System.Collections.Generic;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DotSpatial.Projections;
using UnityEngine;

namespace SILVO.GEO_Tools
{
    public static class GeoProjections
    {
        public static readonly ProjectionInfo WgsProjInfo = ProjectionInfo.FromEpsgCode(4326);
        public static readonly ProjectionInfo Utm30NProjInfo = ProjectionInfo.FromEpsgCode(25830);

        private static Vector2[] ProjectToUTM(Vector2[] lonLat) => 
            GeoProject(lonLat, WgsProjInfo, Utm30NProjInfo);

        private static Vector2 ProjectToUTM(Vector2 lonLat) =>
            GeoProject(lonLat.ToSingleArray(), WgsProjInfo, Utm30NProjInfo)[0];

        public static Vector2[] GeoProject(IEnumerable<Vector2> points, ProjectionInfo from, ProjectionInfo to)
        {
            Vector2[] array = points.ToArray();
            double[] z = ((double)0).ToFilledArray(array.Length).ToArray();
            double[] xy = array.SelectMany(v => new double[] { v.x, v.y }).ToArray();
            Reproject.ReprojectPoints(xy, z, from, to, 0, array.Length);
            return xy.ToVector2Array();
        }
        public static Vector2 GeoProject(Vector2 point, ProjectionInfo from, ProjectionInfo to)
        {
            Debug.Log($"Geoprojecting point {point} from {from} to {to}: {GeoProject(point.ToSingleArray(), from, to)[0]}");;
            return GeoProject(point.ToSingleArray(), from, to)[0];
        }
    }
}
