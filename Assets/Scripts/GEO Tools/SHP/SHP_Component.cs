using System;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DavidUtils.Geometry;
using DavidUtils.Geometry.Bounding_Box;
using DotSpatial.Data;
using SILVO.GEO_Tools.DEM;
using SILVO.GEO_Tools.DotSpatialExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace SILVO.GEO_Tools.SHP
{
    [ExecuteAlways] [Serializable]
    public abstract class SHP_Component: MonoBehaviour
    {
        [SerializeField] protected Vector2[] worldPoints;
        public Vector2[] WorldPoints => worldPoints;
        public int PointCount => worldPoints.Length;

        public AABB_2D aabb; // Shape.Range.Extent
        
        private AABB_2D _parentAABB; // Used to Normalize
        public void SetParentExtent(AABB_2D parentAABB) => _parentAABB = parentAABB;
        public void SetParentExtent(Extent parentExtent) => _parentAABB = parentExtent.ToAABB();

        // Shape is not Serializable, so it must be set in his creation
        // Instead we get and use the points and the extent that are truly Serializable
        public void SetShape(Shape shape)
        {
            worldPoints = shape.GetPoints();
            aabb = shape.Range.Extent.ToAABB();
            OnUpdateShape();
        }
        
        [ContextMenu("Force Update Shape")]
        protected virtual void OnUpdateShape()
        {
            UpdateTerrainProjection();
            UpdateNormalizedPolygon();
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
            int width = Mathf.RoundToInt(_parentAABB.Width / underScaleRatio);
            int height = Mathf.RoundToInt(_parentAABB.Height / underScaleRatio);
            Projecter underScaler = new(new Extent(_parentAABB.min.x, _parentAABB.min.y, _parentAABB.max.x, _parentAABB.max.y), new Vector2(width, height));
            
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
        
        private Projecter GetWorldToImgProjecter(int texRes = 128) => new(aabb, GetTexProportionalSize(aabb, texRes));
        
        public Texture2D GetTexture(int texRes = 128, Color fillColor = default, Color backgroundColor = default)
        {
            fillColor = fillColor == backgroundColor ? backgroundColor.Invert() : fillColor;
            Vector2Int texSize = GetTexProportionalSize(aabb, texRes);
            
            Projecter worldToImgProjecter = new(aabb, texSize);
            
            return GetTexture(texSize, worldToImgProjecter, fillColor, backgroundColor);
        }

        protected abstract Texture2D GetTexture(Vector2Int texSize, Projecter worldToImgProjecter,
            Color fillColor, Color backgroundColor);

        
        
        public static Vector2Int GetTexProportionalSize(AABB_2D aabb, int texRes = 128) => new(
            Mathf.CeilToInt(aabb.Width > aabb.Height ? texRes : aabb.Width / aabb.Height * texRes),
            Mathf.CeilToInt(aabb.Height > aabb.Width ? texRes : aabb.Height / aabb.Width * texRes)
        );
        
        public static AABB_2D GetAABB(SHP_Component[] shpComps)
        {
            AABB_2D[] aabbs = shpComps.Select(shpComp => shpComp.aabb).ToArray();

            float maxPointX = aabbs.Select(aabb => aabb.max.x).ToArray().Max();
            float maxPointY = aabbs.Select(aabb => aabb.max.y).ToArray().Max();
            float minPointX = aabbs.Select(aabb => aabb.min.x).ToArray().Min();
            float minPointY = aabbs.Select(aabb => aabb.min.y).ToArray().Min();

            return new AABB_2D(new Vector2(minPointX, minPointY), new Vector2(maxPointX, maxPointY));
        }
        

        public static Texture2D GetTexture(SHP_Component[] shpComps, int texRes = 128, Color fillColor = default, Color backgroundColor = default)
        {
            fillColor = fillColor == backgroundColor ? backgroundColor.Invert() : fillColor;
            
            AABB_2D aabb = GetAABB(shpComps);
            Vector2Int texSize = GetTexProportionalSize(aabb, texRes); 
            Debug.Log($"Tex Size: {texSize}");
            
            Projecter worldToImgProjecter = new(aabb, texSize);
            
            Texture2D[] textures = shpComps.Select(shpComp => shpComp.GetTexture(texSize, worldToImgProjecter, fillColor, backgroundColor)).ToArray();
            
            // Mezcla las texturas sumándolas pixel a pixel
            Texture2D texture = textures[0];
            for (var i = 1; i < textures.Length; i++)
            {
                Color[] currentPixels = texture.GetPixels();
                Color[] otherTexPixels = textures[i].GetPixels();
                texture.SetPixels(currentPixels.Select((pixel, pixIndex) => 
                    pixel + otherTexPixels[pixIndex]).ToArray());
            }
            
            return texture;
        }
        
        #endregion
    }
}
