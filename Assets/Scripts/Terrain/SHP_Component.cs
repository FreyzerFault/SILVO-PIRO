using System;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DavidUtils.Geometry.Bounding_Box;
using DotSpatial.Data;
using SILVO.DotSpatialExtensions;
using UnityEngine;

namespace SILVO.Terrain
{
    [ExecuteAlways] [Serializable]
    public class SHP_Component: MonoBehaviour
    {
        public Extent parentExtent;
        public Extent Extent => shape.Range.Extent;

        protected Shape shape;

        public virtual Shape Shape
        {
            get => shape;
            set
            {
                shape = value;
                UpdateShape();
            }
        }
        
        
        [SerializeField] protected Vector2[] _worldPoints;
        public Vector2[] WorldPoints => _worldPoints;
        public int PointCount => _worldPoints.Length;
        
        protected virtual void UpdateShape()
        {
            if (useTexture) UpdateTexture();
            
            _worldPoints = shape.GetPoints();
            UpdateTerrainProjection();
            UpdateNormalizedPolygon();
            UpdateTexture();
            UpdateRenderer();
        }

        
        protected virtual void Awake() { }

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
        
        
        protected virtual void UpdateRenderer() {}
        
        
        
        #region NORMALIZED
        
        protected Vector2[] underScaledPoints;
        protected int underScaleRatio = 10; // 1 pixel : 10 m
        
        protected virtual void UpdateNormalizedPolygon()
        {
            int width = Mathf.RoundToInt((float) parentExtent.Width / underScaleRatio);
            int height = Mathf.RoundToInt((float) parentExtent.Height / underScaleRatio);
            var underScaler = new Projecter(parentExtent, new Vector2(width, height));
            
            underScaledPoints = _worldPoints.Select(p => underScaler.ReprojectPoint(p)).ToArray();
        }

        #endregion
        
        
        #region TERRAIN REPROJECTION

        public bool renderOnTerrain = true;
        
        protected Vector2[] _terrainPoints;
        protected Projecter _terrainProjecter;
        public Vector2[] TerrainPoints => _terrainPoints;
        
        protected void UpdateTerrainProjection()
        {
            if (TerrainManager.Instance?.Terrain == null) return;
            
            _terrainProjecter = TerrainManager.Instance.GetWorldToTerrainProjecter();
            RemapWorldPointsToTerrain();
        }
        

        protected virtual void RemapWorldPointsToTerrain()
        {
            if (_worldPoints.IsNullOrEmpty() || UnityEngine.Terrain.activeTerrain == null) return;
            
            _terrainPoints = _worldPoints.Select(p => _terrainProjecter.ReprojectPoint(p)).ToArray();

            var terrainPointsStr = $"{string.Join(", ", _terrainPoints.Take(10))} {(_terrainPoints.Length > 10 ? "..." : "")}";
            var terrainPointsAABB = new AABB_2D(_terrainPoints);
            
            Debug.Log("Reprojected Points:\n" +
                      $"Reprojected: <color=teal>{terrainPointsStr}</color>\n" +
                      $"Reprojected Centroid: <color=cyan>{_terrainPoints.Center()}</color>\n" +
                      $"Reprojected AABB: <color=orange>{terrainPointsAABB}</color>\n");
        }

        #endregion


        
        

        #region TEXTURE
        
        public bool useTexture = false;

        public Vector2Int TexSize
        {
            get => texSize;
            set
            {
                texSize = value;
                UpdateTexture();
            }
        }

        public Texture2D texture;
        protected Vector2Int texSize = new(128, 128);
        protected virtual void UpdateTexture() => texture = GetTexture();
        public virtual Texture2D GetTexture()
        {
            Texture2D tex = new Texture2D(texSize.x, texSize.y);
            
            //Fill texture in Black
            tex.SetPixels(Color.black.ToFilledArray(texSize.x * texSize.y).ToArray());
            
            // Paint White Points
            var imagePoints = _worldPoints.Select(p => shape.GetImageProjecter(texSize).ReprojectPoint(p));
            foreach (Vector2 point in imagePoints) tex.SetPixel(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), Color.white);
            
            tex.Apply();
            return tex;
        }

        #endregion
    }
}
