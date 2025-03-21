using System;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DavidUtils.Geometry;
using DavidUtils.Rendering.Extensions;
using UnityEngine;

namespace SILVO.GEO_Tools.SHP
{
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    [Serializable]
    public class LineSHP_Component: SHP_Component
    {
        
        protected override void Awake()
        {
            renderer = GetComponent<LineRenderer>();
        }

        
        #region RENDERING
        
        public new LineRenderer renderer;

        protected override void UpdateRenderer()
        {
            base.UpdateRenderer();
            renderer.SetPoints(
                renderOnTerrain && UnityEngine.Terrain.activeTerrain != null
                    ? terrainPoints
                    : underScaledPoints);
        }

        #endregion
        
        #region TEXTURE
        
        protected override Texture2D GetTexture(Vector2Int texSize, Projecter worldToImgProjecter,
            Color backgroundColor, Color fillColor)
        {
            Texture2D tex = new Texture2D(texSize.x, texSize.y);
            var imagePoints = worldPoints.Select(worldToImgProjecter.ReprojectPoint);
            
            Edge[] edges = imagePoints.IterateByPairs_NoLoop((a, b) => new Edge(a,b)).ToArray();

            
            // Raster all Texture and paint White Pixels near enough to edges
            var precision = 1f;
            for (var y = 0; y < texSize.y; y++)
            for (var x = 0; x < texSize.x; x++)
            {
                Vector2 pixel = new(x, y);
                bool nearEdge = edges.Any(e => e.DistanceTo(pixel) < precision);
                tex.SetPixel(x,y, nearEdge ? fillColor : backgroundColor);
            }
            tex.Apply();
            
            return tex;
        }

        #endregion
    }
}
