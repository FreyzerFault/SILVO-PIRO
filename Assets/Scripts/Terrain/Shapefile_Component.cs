using System;
using System.Collections.Generic;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DavidUtils.Rendering;
using DotSpatial.Data;
using UnityEngine;

namespace SILVO.Terrain
{
    [Serializable]
    public struct ShapefileMetaData
    {
        public int ShapeCount;
        public FeatureType featureType;
        public string projectionName;
        public Vector2 min;
        public Vector2 max;
        
        public ShapefileMetaData(Shapefile shp)
        {
            ShapeCount = shp.ShapeIndices?.Count ?? 0;
            featureType = shp.FeatureType;
            projectionName = shp.Projection.Name.Replace("_", " ");
            min = new Vector2((float)shp.Extent.MinX, (float)shp.Extent.MinY);
            max = new Vector2((float)shp.Extent.MaxX, (float)shp.Extent.MaxY);
        }

        public override string ToString()
            => $"{ShapeCount} shapes. Type: {featureType} shapes. Projection: {projectionName}\n" +
               $"AABB: {min} - {max}";
    }
    
    [ExecuteAlways]
    public class Shapefile_Component: MonoBehaviour
    {
        private int maxSubPolygonCount;
        
        private List<SHP_Component> shpComponents = new();
        public SHP_Component[] ShpComponents => shpComponents.ToArray();
        
        [SerializeField]
        public ShapefileMetaData metaData;
        public FeatureType FeatureType => metaData.featureType;
        public int ShapeCount => metaData.ShapeCount;
        public string ProjectionName => metaData.projectionName;
        public Vector2 Min => metaData.min;
        public Vector2 Max => metaData.max;
        
        private Shapefile _shpfile;
        public Shapefile Shpfile
        {
            get => _shpfile;
            set
            {
                _shpfile = value;
                if (_shpfile == null)
                    Debug.LogError("Shapefile is null", this);
                metaData = new ShapefileMetaData(_shpfile);
                InstantiateShapes();
                UpdateTexture();
            }
        }
        

        private void InstantiateShapes()
        {
            List<ShapeRange> shpIndices = _shpfile.ShapeIndices;
            shpComponents = shpIndices
                .Select((s, i) =>
                    InstantiateShape(_shpfile.GetShape(i, true), _shpfile.FeatureType, shpIndices[i].Parts))
                .ToList();
            
            shpComponents.ForEach((sc, i) => sc.name = $"{FeatureType.ToString()} {i}");
        }

        private SHP_Component InstantiateShape(Shape shp, FeatureType type, List<PartRange> parts = null)
        {
            GameObject obj = Instantiate(new GameObject(), transform);
            SHP_Component shpComp = null;
            
            switch (type)
            {
                case FeatureType.Polygon:
                    obj.AddComponent<LineRenderer>();
                    obj.AddComponent<MeshRenderer>();
                    obj.AddComponent<MeshFilter>();
                    obj.AddComponent<PolygonRenderer>();
                    shpComp = obj.AddComponent<PolygonSHP_Component>();
                    ((PolygonSHP_Component)shpComp).maxSubPolygonCount = maxSubPolygonCount;
                    break;
                case FeatureType.MultiPoint:
                case FeatureType.Line:
                case FeatureType.Point:
                case FeatureType.Unspecified:
                default:
                    throw new NotImplementedException();
            }
            shpComp.Shape = shp;
            return shpComp;
        }

        public static Shapefile_Component InstantiateShapefile(Shapefile shpfile, int maxSubPolygonCount = 500)
        {
            var shpfileComp = new GameObject().AddComponent<Shapefile_Component>();
            shpfileComp.maxSubPolygonCount = maxSubPolygonCount;
            shpfileComp.Shpfile = shpfile;
            return shpfileComp;
        }
        
        
        #region TEXTURE

        public Vector2Int TexSize
        {
            get { return texSize; }
            set
            {
                texSize = value;
                UpdateTexture();
            }
        }

        public Texture2D[] ShapeTextures => shpComponents.Select(sc => sc.texture).ToArray();
        public Texture2D texture;
        protected Vector2Int texSize = new(128, 128);
        protected virtual void UpdateTexture() => texture = GetTexture();

        public virtual Texture2D GetTexture() => 
            shpComponents.IsNullOrEmpty() ? null : shpComponents[0].texture ?? shpComponents[0].GetTexture();

        #endregion


        #region MESHES

        public Mesh[] Meshes =>
            FeatureType == FeatureType.Polygon
                ? shpComponents.Select(sc => ((PolygonSHP_Component)sc).renderer.Mesh).ToArray()
                : null;

        #endregion
    }
}
