using System;
using System.Drawing;
using DavidUtils;
using DavidUtils.ExtensionMethods;
using DotSpatial.Data;
using SILVO.DotSpatialExtensions;
using UnityEngine;

namespace SILVO.Terrain
{
    [ExecuteAlways]
    public class TerrainManager : SingletonExecuteAlways<TerrainManager>
    {
        [SerializeField] private DEM _dem;

        public DEM DEM
        {
            get => _dem;
            set
            {
                _dem = value;
                UpdateDEM();
            }
        }

        private UnityEngine.Terrain _terrain = null;
        public UnityEngine.Terrain Terrain => _terrain == null ? _terrain = UnityEngine.Terrain.activeTerrain : _terrain;
        public Vector3 TerrainSize => Terrain.terrainData.size;
        public Vector2 TerrainSize2D => new(TerrainSize.x, TerrainSize.z);
        
        public Rectangle TerrainRectangle => new(0, 0, (int)TerrainSize.x, (int)TerrainSize.z);
        public Rectangle WorldRectangle => new(_dem.WorldOrigin.ToPoint(), _dem.WorldSize2D.ToSize());
        
        public Extent TerrainExtents => new(Terrain.GetPosition().x, Terrain.GetPosition().y, TerrainSize.x, TerrainSize.z);
        public Extent WorldExtents => _dem.WorldOrigin.ToExtent(_dem.WorldSize2D);
        
        public Vector2 WorldOrigin => _dem.WorldOrigin;
        public Vector2 WorldSize => _dem.WorldSize2D;

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
            
            if (_dem.IsEmpty)
                throw new Exception("No DEM data found. Try to reimport it");
            if (terrain?.terrainData == null)
                throw new Exception("No active terrain found. Create or enable it");
            
            TerrainData tData = terrain.terrainData;
            
            tData.heightmapResolution = _dem.resPow2;

            if (_dem.heightDataForTerrain == null)
                _dem.PrepareHeightDataForTerrain();
            
            tData.SetHeights(0,0,  _dem.heightDataForTerrain);
            
            // Collider
            terrain.GetComponent<TerrainCollider>().terrainData = tData;

            // Terrain Real Size
            Vector3 worldSize = _dem.WorldSize;
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

        public Vector2 GetRelativeTerrainPosition(Vector2 worldPosition) =>
            GetNormalizedPosition(worldPosition) * TerrainSize;
        
        public Vector2 GetNormalizedPosition(Vector2 worldPosition) =>
            (worldPosition - WorldOrigin) / WorldSize;

        public Vector3 GetRelativeTerrainPositionWithHeight(Vector2 worldPosition) =>
            AddHeight(GetRelativeTerrainPosition(worldPosition));

        #endregion


        #region HEIGHTS


        public Vector3 AddHeight(Vector2 terrainPos) =>
            terrainPos.ToV3xz().WithY(GetInterpolatedHeight(terrainPos));

        public float GetInterpolatedHeight(Vector2 worldPosition) =>
            Terrain.GetInterpolatedHeight(GetNormalizedPosition(worldPosition));

        #endregion
    }
}
