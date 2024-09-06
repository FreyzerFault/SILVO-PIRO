
using System.Linq;
using DavidUtils.ExtensionMethods;
using UnityEngine;

namespace SILVO.Terrain
{
    public class TerrainAnalizer : MonoBehaviour
    {
        private UnityEngine.Terrain terrain = null;
        
        private void Awake()
        {
            terrain = GetComponent<UnityEngine.Terrain>();
        }

        void Start()
        {
            ScaleMap(10);
            
            // SIZE in real world units
            Vector3 size = terrain.terrainData.size;
            
            // HEIGHTMAP distance between each sample point
            Vector3 scale = terrain.terrainData.heightmapScale;
            
            // HEIGHTMAP original size
            int res = terrain.terrainData.heightmapResolution;
            
            float zeroHeight = terrain.terrainData.GetHeight(0, 0);
            float farCornerHeight = terrain.terrainData.GetHeight(res - 1, res - 1);
            
            Debug.Log("Terrain size: " + size);
            Debug.Log($"Terrain heightmap size: {res} x {res}");
            Debug.Log($"Distance between samples in world units: {scale.x} x {scale.z}");
            
            Debug.Log("Terrain height at (0, 0): " + zeroHeight);
            Debug.Log($"Terrain height at ({res - 1}, {res - 1}): {farCornerHeight}");
            
            // Max Min Height
            float[,] heights2D = terrain.terrainData.GetHeights(0, 0, res, res);
            float[] heights = heights2D.Flatten();
            float maxHeight = heights.Max();
            float minHeight = heights.Min();
            Debug.Log($"Terrain MAX height: {maxHeight}");
            Debug.Log($"Terrain MIN height: {minHeight}");
            
            // Height average
            float averageHeight = heights.Average();
            Debug.Log("Terrain average height: " + averageHeight);
        }

        void ScaleMap(float scale)
        {
            int res = terrain.terrainData.heightmapResolution;
            float[,] heights2D = terrain.terrainData.GetHeights(0, 0, res, res);
            
            for (int x = 0; x < res; x++)
            for (int z = 0; z < res; z++)
                heights2D[x, z] *= scale;
            
            terrain.terrainData.SetHeights(0, 0, heights2D);
        }
    }
}
