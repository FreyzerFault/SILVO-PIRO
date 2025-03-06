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
        [SerializeField] protected Vector2[] worldPoints;
        public Vector2[] WorldPoints => worldPoints;
        public int PointCount => worldPoints.Length;
        
        protected Extent extent; // Shape.Range.Extent
        
        private Extent _parentExtent; // Used to Normalize
        public void SetParentExtent(Extent parentExtent) => _parentExtent = parentExtent;

        // Shape is not Serializable, so it must be set in his creation
        // Instead we get and use the points and the extent that are truly Serializable
        public void SetShape(Shape shape)
        {
            worldPoints = shape.GetPoints();
            extent = shape.Range.Extent;
            OnUpdateShape();
        }
        
        [ContextMenu("Force Update Shape")]
        protected virtual void OnUpdateShape()
        {
            if (useTexture) UpdateTexture();
            UpdateTerrainProjection();
            UpdateNormalizedPolygon();
            UpdateTexture();
            UpdateRenderer();
        }

        
        protected virtual void Awake() { }

        protected virtual void OnEnable()
        {
            if (TerrainManager.Instance == null) return;
            TerrainManager.Instance.onTerrainSizeChanged += UpdateTerrainProjection;
            TerrainManager.Instance.onTerrainSizeChanged += UpdateRenderer;
        }
        
        protected virtual void OnDisable()
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
            int width = Mathf.RoundToInt((float) _parentExtent.Width / underScaleRatio);
            int height = Mathf.RoundToInt((float) _parentExtent.Height / underScaleRatio);
            Projecter underScaler = new(_parentExtent, new Vector2(width, height));
            
            underScaledPoints = worldPoints.Select(p => underScaler.ReprojectPoint(p)).ToArray();
        }

        #endregion
        
        
        #region TERRAIN REPROJECTION

        public bool renderOnTerrain = true;
        
        protected Vector2[] terrainPoints;
        protected Projecter terrainProjecter;
        
        protected virtual void UpdateTerrainProjection()
        {
            if (TerrainManager.Instance?.Terrain == null) return;
            
            terrainProjecter = TerrainManager.Instance.GetWorldToTerrainProjecter();
            RemapWorldPointsToTerrain();
        }
        

        protected virtual void RemapWorldPointsToTerrain()
        {
            if (worldPoints.IsNullOrEmpty() || UnityEngine.Terrain.activeTerrain == null) return;
            
            terrainPoints = worldPoints.Select(p => terrainProjecter.ReprojectPoint(p)).ToArray();

            var terrainPointsStr = $"{string.Join(", ", terrainPoints.Take(10))} {(terrainPoints.Length > 10 ? "..." : "")}";
            AABB_2D terrainPointsAABB = new(terrainPoints);
            
            Debug.Log("Reprojected Points:\n" +
                      $"Reprojected: <color=teal>{terrainPointsStr}</color>\n" +
                      $"Reprojected Centroid: <color=cyan>{terrainPoints.Center()}</color>\n" +
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
        protected Projecter WorldToImgProjecter => new(extent, texSize);
        protected virtual void UpdateTexture() => texture = GetTexture();
        public virtual Texture2D GetTexture()
        {
            Texture2D tex = new(texSize.x, texSize.y);
            
            //Fill texture in Black
            tex.SetPixels(Color.black.ToFilledArray(texSize.x * texSize.y).ToArray());
            
            // Raster White Points
            var imagePoints = worldPoints.Select(p => WorldToImgProjecter.ReprojectPoint(p));
            foreach (Vector2 point in imagePoints) tex.SetPixel(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), Color.white);
            
            tex.Apply();
            return tex;
        }

        #endregion
    }
}
