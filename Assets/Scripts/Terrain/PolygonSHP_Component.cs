using System;
using DavidUtils.Rendering;
using DotSpatial.Data;
using SILVO.DotSpatialExtensions;
using UnityEngine;
using UnityEngine.Serialization;
using Polygon = DavidUtils.Geometry.Polygon;

namespace SILVO.Terrain
{
    [ExecuteAlways]
    [RequireComponent(typeof(PolygonRenderer))]
    [Serializable]
    public class PolygonSHP_Component: SHP_Component
    {
        public int maxSubPolygonCount = 500;
        
        [SerializeField]
        private Polygon worldPolygon;
        
        public Polygon WorldPolygon => worldPolygon;
        public Vector2[] Vertices => worldPolygon.Vertices;
        public int VertexCount => worldPolygon.VertexCount;

        protected override void OnUpdateShape()
        {
            worldPolygon = new Polygon(worldPoints);
            
            // Dado la vuelta CW -> CCW y limpiar vertices duplicados y ejes superpuestos
            worldPolygon = worldPolygon.Revert();
            worldPolygon.CleanDegeneratePolygon();
            
            base.OnUpdateShape();
        }
        
        protected override void Awake()
        {
            renderer = GetComponent<PolygonRenderer>();
            renderer.generateSubPolygons = true;
        }

        
        
        #region NORMALIZED
        
        private Polygon _underScaledPolygon;

        protected override void UpdateNormalizedPolygon()
        {
            base.UpdateNormalizedPolygon();
            _underScaledPolygon = new Polygon(underScaledPoints).Revert();
        }

        #endregion
        
        
        #region TERRAIN REPROJECTION

        private Polygon _terrainPolygon;
        public Polygon TerrainPolygon => _terrainPolygon;
        
        protected override void RemapWorldPointsToTerrain()
        {
            base.RemapWorldPointsToTerrain();
            _terrainPolygon = new Polygon(terrainPoints).Revert();
        }

        #endregion
        

        #region RENDERING
        
        public new PolygonRenderer renderer;

        protected override void UpdateRenderer()
        {
            base.UpdateRenderer();
            
            renderer.maxSubPolygonCount = maxSubPolygonCount;
            renderer.Polygon =
                renderOnTerrain && UnityEngine.Terrain.activeTerrain != null
                    ? _terrainPolygon
                    : _underScaledPolygon;
        }

        #endregion
        
        
        #region TEXTURE

        public override Texture2D GetTexture() => WorldToImgProjecter.ReprojectPolygon(worldPolygon).ToTexture(texSize);

        #endregion
    }
}
