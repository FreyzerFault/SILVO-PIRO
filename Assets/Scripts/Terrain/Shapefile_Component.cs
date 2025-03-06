using System;
using System.Collections.Generic;
using System.Linq;
using DavidUtils;
using DavidUtils.ExtensionMethods;
using DavidUtils.Rendering;
using DavidUtils.Rendering.Extensions;
using DotSpatial.Data;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using SILVO.DotSpatialExtensions;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Serialization;

namespace SILVO.Terrain
{
    [Serializable]
    public struct ShapefileMetaData
    {
        public int shapeCount;
        public FeatureType featureType;
        public string projectionName;
        public Vector2 min;
        public Vector2 max;
        
        public ShapefileMetaData(Shapefile shp)
        {
            shapeCount = shp.ShapeIndices?.Count ?? 0;
            featureType = shp.FeatureType;
            projectionName = shp.Projection.Name.Replace("_", " ");
            min = new Vector2((float)shp.Extent.MinX, (float)shp.Extent.MinY);
            max = new Vector2((float)shp.Extent.MaxX, (float)shp.Extent.MaxY);
        }

        public override string ToString()
            => $"{shapeCount} shapes. Type: {featureType} shapes. Projection: {projectionName}\n" +
               $"AABB: {min} - {max}";
    }
    
    [ExecuteAlways]
    public class Shapefile_Component: MonoBehaviour
    {
        [Tooltip("To Render Concave Polygons divided in Convex SubPolygons")]
        [SerializeField] private int maxSubPolygonCount;
        private List<SHP_Component> _shpComponents = new();
        
        public SHP_Component[] ShpComponents => _shpComponents.ToArray();
        
        [SerializeField] public ShapefileMetaData metaData;
        
        public FeatureType FeatureType => metaData.featureType;
        public int ShapeCount => metaData.shapeCount;
        public string ProjectionName => metaData.projectionName;
        public Vector2 Min => metaData.min;
        public Vector2 Max => metaData.max;

        [SerializeField] private bool loaded = false;
        
        private Shapefile _shpfile;
        public Shapefile Shpfile
        {
            get => _shpfile;
            set
            {
                _shpfile = value;
                if (_shpfile == null)
                {
                    Debug.LogError($"Shapefile set to null", this);
                    return;
                }
                
                metaData = new ShapefileMetaData(_shpfile);
                InstantiateShapes();
                OverTerrain = overTerrain;
                UpdateTexture();
                
                filePath = _shpfile.Filename;
                loaded = true;
            }
        }

        private void Update()
        {
            if (_shpfile == null && filePath.NotNullOrEmpty() && !loaded)
                LoadShapeFile();
        }

        [ContextMenu("Reload Shapefile")]
        public void LoadShapeFile() => Shpfile = OpenFile(filePath);


        #region FILE HANDLING

        [SerializeField] private string filePath;

        public string FilePath
        {
            get => filePath;
            set
            {
                filePath = value;
                if (filePath.NotNullOrEmpty())
                    LoadShapeFile();
            }
        }
        
        
        public static Shapefile_Component InstantiateShapefile(Shapefile shpfile, int maxSubPolygonCount = PolygonRenderer.DEFAULT_MAX_SUBPOLYGONS_COUNT)
        {
            Shapefile_Component shpfileComp = new GameObject().AddComponent<Shapefile_Component>();
            shpfileComp.maxSubPolygonCount = maxSubPolygonCount;
            shpfileComp.Shpfile = shpfile;
            return shpfileComp;
        }
        
        public static Shapefile_Component InstantiateShapefile(string filePath, int maxSubPolygonCount = PolygonRenderer.DEFAULT_MAX_SUBPOLYGONS_COUNT)
        {
            Shapefile_Component shpfileComp = UnityUtils.InstantiateObject<Shapefile_Component>(null);
            shpfileComp.maxSubPolygonCount = maxSubPolygonCount;
            shpfileComp.FilePath = filePath;
            return shpfileComp;
        }
        
        public static Shapefile OpenFile(string path)
        {
            try
            {
                Shapefile shp = Shapefile.OpenFile(path);
                if (shp == null)
                    Debug.LogError($"Failed to open SHP file in {path}");
                return shp;
            }
            catch (Exception e)
            {
                Debug.LogError($"File Error: {path}\n" + e);
            }

            return null;
        }

        #endregion


        #region Featured SHAPES

        private void InstantiateShapes()
        {
            // Delete children to reinstantiate
            var children = GetComponentsInChildren<SHP_Component>();
            if (children is { Length: > 0 })
                children.ForEach(UnityUtils.DestroySafe);
            
            if (_shpfile == null)
            {
                Debug.LogError("Shapefile is null while Instiatiating Shapes", this);
                return;
            }
            
            List<ShapeRange> shpIndices = _shpfile.ShapeIndices;
            
            // If Features are Single Points, join them in a single Shape of type MultiPoint
            if (FeatureType is FeatureType.Point)
            {
                Shape mpShape = shpIndices.Select((_, i) => _shpfile.GetShape(i, true)).ToMultiPoint();
                _shpComponents = new List<SHP_Component> {InstantiateShape(mpShape)};
            }
            else
            {
                // Create a SHP_Component for each Shape
                _shpComponents = shpIndices
                    .Select((_, i) =>
                    {
                        Shape shape = _shpfile.GetShape(i, true);
                        List<PartRange> parts = shpIndices[i].Parts;
                        return InstantiateShape(shape, parts, i.ToString(), GetColor(i));
                    })
                    .ToList();
            }
        }

        
        private SHP_Component InstantiateShape(Shape shp, List<PartRange> parts = null, string label = "", Color color = default)
        {
            SHP_Component shpComp = null;
            
            switch (FeatureType)
            {
                case FeatureType.Polygon:
                    shpComp = InstantiatePolygon(label, color);
                    break;
                case FeatureType.Point:
                case FeatureType.MultiPoint:
                    shpComp = InstantiatePoints(label, color);
                    break;
                case FeatureType.Line:
                    shpComp = InstantiateLine(label, color);
                    break;
                case FeatureType.Unspecified:
                default:
                    Debug.LogError("Shapefile has an unsupported feature type", this);
                    return null;
            }
            
            shpComp.SetParentExtent(_shpfile.Extent);
            shpComp.SetShape(shp);
            return shpComp;
        }


        #region Each FeatureType Instantiation
        
        private SHP_Component InstantiatePolygon(string label = "", Color color = default)
        {
            PolygonRenderer polygonRenderer = UnityUtils.InstantiateObject<PolygonRenderer>(transform, $"Polygon {label}");
            polygonRenderer.Color = color == default ? firstColor : color;
            
            SHP_Component shpComp = polygonRenderer.gameObject.AddComponent<PolygonSHP_Component>();
            ((PolygonSHP_Component)shpComp).maxSubPolygonCount = maxSubPolygonCount;
            return shpComp;
        }
        
        private SHP_Component InstantiateLine(string label = "", Color color = default)
        {
            LineRenderer lr = UnityUtils.InstantiateObject<LineRenderer>(transform, $"Line {label}");
            lr.startColor = lr.endColor = color == default ? firstColor : color;
            lr.SetDefaultMaterial();
            lr.useWorldSpace = false;
            SHP_Component shpComp = lr.gameObject.AddComponent<LineSHP_Component>();
            return shpComp;
        }
        
        private SHP_Component InstantiatePoints(string label = "", Color color = default)
        {
            PointsRenderer pr = UnityUtils.InstantiateObject<PointsRenderer>(transform, $"Points {label}");
            SHP_Component shpComp = pr.gameObject.AddComponent<PointSHP_Component>();
            return shpComp;
        }
        
        #endregion

        #endregion


        #region COLOR

        private Color firstColor = Color.cyan;
        private Color[] _colors;

        private void GenerateColors(int count) => _colors = firstColor.GetRainBowColors(count);
        
        private Color GetColor(int i)
        {
            if (_colors.IsNullOrEmpty() || _colors.Length <= i)
                GenerateColors(i + 1);
            return _colors[i];
        }

        #endregion
        
        
        #region TERRAIN

        [SerializeField] private bool overTerrain = false;
        private int terrainOffset = 1000;

        public bool OverTerrain
        {
            get => overTerrain;
            set
            {
                overTerrain = value;
                UpdateOverTerrain();
            }
        }
        
        public void UpdateOverTerrain()
        {
            transform.localRotation = overTerrain ? Quaternion.Euler(90, 0, 0) : Quaternion.identity;
            transform.localPosition = transform.localPosition.WithY(overTerrain ? terrainOffset : 0);
        }

        #endregion
        
        
        
        #region TEXTURE

        public Vector2Int TexSize
        {
            get => texSize;
            set
            {
                texSize = value;
                UpdateTexture();
            }
        }

        public Texture2D[] ShapeTextures => _shpComponents.Select(sc => sc.texture).ToArray();
        public Texture2D texture;
        protected Vector2Int texSize = new(128, 128);
        public virtual void UpdateTexture() => texture = GetTexture();

        public virtual Texture2D GetTexture() => 
            _shpComponents.IsNullOrEmpty() ? null : _shpComponents[0].texture ?? _shpComponents[0].GetTexture();

        #endregion


        #region MESHES

        public Mesh[] Meshes =>
            FeatureType == FeatureType.Polygon
                ? _shpComponents.Select(sc => ((PolygonSHP_Component)sc).renderer.Mesh).ToArray()
                : null;

        #endregion
    }
}
