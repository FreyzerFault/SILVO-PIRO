using System;
using DavidUtils.Rendering;
using DotSpatial.Data;
using SILVO.DotSpatialExtensions;
using UnityEngine;
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
        private Polygon _worldPolygon;
        public Polygon WorldPolygon => _worldPolygon;
        public Vector2[] Vertices => _worldPolygon.Vertices;
        public int VertexCount => _worldPolygon.VertexCount;


        public override Shape Shape
        {
            get => shape;
            set
            {
                shape = value;
                UpdateShape();
            }
        }

        protected override void UpdateShape()
        {
            _worldPolygon = shape.GetPolygon();
            
            base.UpdateShape();
        }
        
        protected override void Awake()
        {
            renderer = GetComponent<PolygonRenderer>();
            renderer.generateSubPolygons = true;
        }


        private void OnEnable()
        {
            if (TerrainManager.Instance == null) return;
            TerrainManager.Instance.onTerrainSizeChanged += UpdateTerrainProjection;
            TerrainManager.Instance.onTerrainSizeChanged += UpdateRenderer;
        }

        private void OnDisable()
        {
            if (TerrainManager.Instance == null) return;
            TerrainManager.Instance.onTerrainSizeChanged -= UpdateTerrainProjection;
            TerrainManager.Instance.onTerrainSizeChanged -= UpdateRenderer;
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
            _terrainPolygon = new Polygon(_terrainPoints).Revert();
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

        public override Texture2D GetTexture() => shape.GetImageProjecter(texSize).ReprojectPolygon(_worldPolygon).ToTexture(texSize);

        #endregion
    }
}
