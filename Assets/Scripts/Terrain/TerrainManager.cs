using System;
using System.Drawing;
using DavidUtils;
using DavidUtils.ExtensionMethods;
using DotSpatial.Data;
using SILVO.DotSpatialExtensions;
using UnityEngine;
using DavidUtils.ExtensionMethods;
using DotSpatial.Projections;

namespace SILVO.Terrain
{
    [ExecuteAlways]
    public class TerrainManager : SingletonExecuteAlways<TerrainManager>
    {
        [SerializeField] private DEM dem;

        public DEM DEM
        {
            get => dem;
            set
            {
                dem = value;
                UpdateDEM();
            }
        }

        private UnityEngine.Terrain _terrain = null;
        public UnityEngine.Terrain Terrain => _terrain == null ? _terrain = UnityEngine.Terrain.activeTerrain : _terrain;
        public Vector3 TerrainSize => Terrain.terrainData.size;
        public Vector2 TerrainSize2D => new(TerrainSize.x, TerrainSize.z);
        
        public Rectangle TerrainRectangle => new(0, 0, (int)TerrainSize.x, (int)TerrainSize.z);
        public Rectangle WorldRectangle => new(dem.WorldOrigin.ToPoint(), dem.WorldSize2D.ToSize());
        
        public Extent TerrainExtents => new(Terrain.GetPosition().x, Terrain.GetPosition().y, TerrainSize.x, TerrainSize.z);
        public Extent WorldExtents => WorldOrigin.ToExtent(dem.WorldSize2D);
        
        public Vector2 WorldOrigin => dem.WorldOrigin;
        public Vector2 WorldSize => dem.WorldSize2D;
        
        public ProjectionInfo WorldProjection => dem.metaData.Projection;

        public Action onTerrainSizeChanged;
        public Action onTerrainHeightsChanged;

        protected override void Awake()
        {
            base.Awake();
            _terrain = GetComponent<UnityEngine.Terrain>();
        }

        #region DEM

        /// <summary>
        /// Create TerrainData from DEM and apply to Terrain & TerrainCollider
        /// </summary>
        private void UpdateDEM()
        {
            var terrain = UnityEngine.Terrain.activeTerrain;
            
            if (dem.IsEmpty)
                throw new Exception("No DEM data found. Try to reimport it");
            if (terrain?.terrainData == null)
                throw new Exception("No active terrain found. Create or enable it");
            
            TerrainData tData = terrain.terrainData;
            
            tData.heightmapResolution = dem.resPow2;

            if (dem.heightDataForTerrain == null)
                dem.PrepareHeightDataForTerrain();
            
            tData.SetHeights(0,0,  dem.heightDataForTerrain);
            
            // Collider
            terrain.GetComponent<TerrainCollider>().terrainData = tData;

            // Terrain Real Size
            Vector3 worldSize = dem.WorldSize;
            if (tData.size != worldSize)
            {
                tData.size = worldSize;
                onTerrainSizeChanged?.Invoke();
            }
            
            onTerrainHeightsChanged?.Invoke();
        }

        

        #endregion


        #region WORLD - TERRAIN CONVERSION
        
        public Projecter GetWorldToTerrainProjecter() => new(WorldExtents, TerrainRectangle);
        public Projecter GetTerrainToWorldProjecter() => new(TerrainExtents, WorldRectangle);
        
        public Vector2 GetNormalizedPosition(Vector2 pos) => pos / TerrainSize2D;
        public Vector2 GetNormalizedPosition_World(Vector2 worldPosition) => worldPosition / WorldSize;

        public Vector2 WorldToTerrainPosition(Vector2 worldPosition) => worldPosition - WorldOrigin;
        public Vector2 TerrainToWorldPosition(Vector2 terrainPos) => terrainPos + WorldOrigin;

        #endregion


        #region HEIGHTS

        public Vector3 WorldToTerrain3D(Vector2 worldPosition) => 
            AddHeight(WorldToTerrainPosition(worldPosition));

        public Vector3 AddHeight(Vector2 pos) =>
            pos.ToV3xz().WithY(GetInterpolatedHeight(pos));

        public float GetInterpolatedHeight(Vector2 pos) => 
            Terrain.GetInterpolatedHeight(GetNormalizedPosition(pos));

        #endregion
    }
}
