using System;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DavidUtils.Geometry;
using DavidUtils.Rendering;
using DavidUtils.Rendering.Extensions;
using SILVO.DotSpatialExtensions;
using UnityEngine;

namespace SILVO.Terrain
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
                    ? _terrainPoints
                    : underScaledPoints);
        }

        #endregion
        
        #region TEXTURE

        public override Texture2D GetTexture()
        {
            Texture2D tex = new Texture2D(texSize.x, texSize.y);
            var imagePoints = _worldPoints.Select(p => shape.GetImageProjecter(texSize).ReprojectPoint(p));
            
            Edge[] edges = imagePoints.IterateByPairs_NoLoop((a, b) => new Edge(a,b)).ToArray();

            var precision = 1f;
            
            for (var y = 0; y < texSize.y; y++)
            for (var x = 0; x < texSize.x; x++)
            {
                Vector2 pixel = new Vector2(x, y);
                bool nearEdge = edges.Any(e => e.DistanceTo(pixel) < precision);
                tex.SetPixel(x,y, nearEdge ? Color.white : Color.black);
            }
            tex.Apply();
            
            return tex;
        }

        #endregion
    }
}
