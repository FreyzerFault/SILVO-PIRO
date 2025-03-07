using System;
using DavidUtils.Rendering;
using UnityEngine;

namespace SILVO.Terrain
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
        
        

    }
}
