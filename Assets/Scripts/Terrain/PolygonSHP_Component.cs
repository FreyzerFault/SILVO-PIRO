using System;
using System.Linq;
using DavidUtils.Geometry.Bounding_Box;
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
        
        public bool renderOnTerrain = true;
        
        private void Awake()
        {
            renderer = GetComponent<PolygonRenderer>();
            renderer.generateSubPolygons = true;
            
            transform.Rotate(Vector3.right, 90);
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
            base.UpdateShape();
            
            _worldPolygon = shape.GetPolygon();
            
            UpdateTerrainProjection();
            UpdateNormalizedPolygon();
            UpdateTexture();
            UpdateRenderer();
        }
        
        
        #region NORMALIZED
        
        private Polygon normalizedPolygon;
        
        private void UpdateNormalizedPolygon()
        {
            var normalizer = new Projecter(shape, new Vector2(1,1));
            normalizedPolygon = normalizer.ReprojectPolygon(_worldPolygon);
        }

        #endregion
        
        
        #region TERRAIN REPROJECTION

        private Polygon _terrainPolygon;
        private Projecter _terrainProjecter;
        public Polygon TerrainPolygon => _terrainPolygon;
        
        private void UpdateTerrainProjection()
        {
            if (TerrainManager.Instance?.Terrain == null) return;
            
            _terrainProjecter = new Projecter(TerrainManager.Instance.WorldExtents, TerrainManager.Instance.TerrainRectangle);
            RemapWorldPolygonToTerrain();
        }
        

        private void RemapWorldPolygonToTerrain()
        {
            if (_worldPolygon.IsEmpty || UnityEngine.Terrain.activeTerrain == null) return;
            
            _terrainPolygon = _terrainProjecter.ReprojectPolygon(_worldPolygon);

            var terrainPolygonVerticesStr = $"{string.Join(", ", _terrainPolygon.Vertices.Take(10))} {(_terrainPolygon.Vertices.Length > 10 ? "..." : "")}";
            var terrainPolygonAABB = new AABB_2D(_terrainPolygon);
            
            Debug.Log("Reprojected Polygon:\n" +
                      $"Reprojected Vertices: <color=teal>{terrainPolygonVerticesStr}</color>\n" +
                      $"Reprojected Centroid: <color=cyan>{_terrainPolygon.centroid}</color>\n" +
                      $"Reprojected AABB: <color=orange>{terrainPolygonAABB}</color>\n");
        }

        #endregion
        

        #region RENDERING
        
        public PolygonRenderer renderer;
        
        private void UpdateRenderer()
        {
            renderer.maxSubPolygonCount = maxSubPolygonCount;
            renderer.Polygon =
                renderOnTerrain && UnityEngine.Terrain.activeTerrain != null
                    ? _terrainPolygon
                    : normalizedPolygon;
        }

        #endregion
        
        
        #region TEXTURE
        
        protected override void UpdateTexture() => texture = GetTexture();

        public override Texture2D GetTexture() => 
            shape.GetImageProjecter(texSize).ReprojectPolygon(_worldPolygon).ToTexture(texSize);

        #endregion
    }
}
