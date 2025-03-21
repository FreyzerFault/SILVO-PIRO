using System;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DavidUtils.Rendering;
using UnityEngine;

namespace SILVO.GEO_Tools.SHP
{
    [ExecuteAlways]
    [RequireComponent(typeof(PointsRenderer))]
    [Serializable]
    public class PointSHP_Component : SHP_Component
    {
        
        protected override void Awake()
        {
            renderer = GetComponent<PointsRenderer>();
        }

        
        #region RENDERING
        
        public new PointsRenderer renderer;

        protected override void UpdateRenderer()
        {
            base.UpdateRenderer();
            
            renderer.UpdateAllObj(
                renderOnTerrain && UnityEngine.Terrain.activeTerrain != null
                    ? terrainPoints
                    : underScaledPoints);
        }

        #endregion



        #region TEXTURE

        /// <summary>
        /// Dibuja los puntos como pixeles blancos
        /// </summary>
        protected override Texture2D GetTexture(Vector2Int texSize, Projecter worldToImgProjecter,
            Color backgroundColor, Color fillColor)
        {
            Texture2D tex = new(texSize.x, texSize.y);
            
            //Fill texture in Black
            tex.SetPixels(backgroundColor.ToFilledArray(texSize.x * texSize.y).ToArray());
            
            // Raster White Points
            var imagePoints = worldPoints.Select(worldToImgProjecter.ReprojectPoint);
            foreach (Vector2 point in imagePoints) tex.SetPixel(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), fillColor);
            
            tex.Apply();
            return tex;
        }

        #endregion
    }
}
