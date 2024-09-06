using System;
using System.Linq;
using BitMiracle.LibTiff.Classic;
using DavidUtils.ExtensionMethods;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UI;

namespace SILVO.Asset_Importers
{
    [ScriptedImporter(1, "tifdem")]
    public class DEM_Importer : ScriptedImporter
    {
        public float[] heightMap = Array.Empty<float>();
        
        public Vector2Int mapSize = new Vector2Int(0, 0);
        public int bitsPerSample = 0;
        public string format = "not defined";
        
        public float maxHeight = 0;
        public float minHeight = 0;
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            // Create PREFAB with Sprite
            var obj = new GameObject();
            var image = obj.AddComponent<Image>();
            Texture2D texture = GetDefaultTexture();
            image.sprite = CreateSprite(texture);
            
            // Read TIFF file directly to Tiff Object
            string assetPath = ctx.assetPath;
            
            // OPEN TIFF
            Tiff tiff = Tiff.Open(assetPath, "r");
            if (tiff == null)
                Debug.LogError($"Failed to open TIFF file in {assetPath}");
            else
            {
                // READ TIFF 
                tiff.ReadDirectory();
                int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                int height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                mapSize = new Vector2Int(width, height);
                
                bitsPerSample = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
                format = tiff.GetField(TiffTag.SAMPLEFORMAT)[0].ToString();
                
                Debug.Log($"TIFF loaded from {assetPath}\nSize: {width} x {height}; Format: {format} {bitsPerSample} bits");
                
                heightMap = new float[height * width];
                
                for (var y = 0; y < height; y++)
                {
                    var scanline = new byte[tiff.ScanlineSize()];
                    tiff.ReadScanline(scanline, y);
                    
                    // Convert to 32 bits (float)
                    for (var x = 0; x < width; x++)
                    {
                        var value = BitConverter.ToSingle(scanline, x * sizeof(float));
                        
                        // Check if it's NaN
                        if (float.IsNaN(value)) value = 0;
                        
                        heightMap[y * width + x] = value;
                    }
                }
                
                tiff.Close();
                
                // Map values Min-Max to 0-1
                maxHeight = heightMap.Max();
                minHeight = heightMap.Min();
                NormalizeFromMinMax();
                
                // Data to Texture
                texture = CreateGreyTexture(heightMap, width, height);
                image.sprite = CreateSprite(texture);
            }
            
            // Add Main OBJECT and secondary ASSETS
            ctx.AddObjectToAsset("main obj", obj);
            ctx.AddObjectToAsset("texture", texture, texture);
            ctx.AddObjectToAsset("sprite", image.sprite, texture);
            ctx.SetMainObject(obj);
        }
        
        private Texture2D GetDefaultTexture() => Texture2D.grayTexture;
        
        private Sprite CreateSprite(Texture2D texture) => 
            Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

        private Texture2D CreateGreyTexture(float[] data, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height);
                
            tex.SetPixels(data.Select(f => new Color(f, f, f, 1)).ToArray());
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return tex;
        }

        private void NormalizeFromMinMax()
        {
            for (int y = 0; y < mapSize.y; y++)
            for (int x = 0; x < mapSize.x; x++)
                heightMap[y * mapSize.x + x] = Mathf.InverseLerp(minHeight, maxHeight, heightMap[y * mapSize.x + x]);
        }

        #region TERRAIN APPLICATION
        
        /// <summary>
        /// Aplica el mapa de alturas al terreno activo
        /// Asume que el terreno es CUADRADO
        /// TODO - Ajustar tamaño del terreno RECTANGULAR igualando ratio x:y mapa -> terreno
        /// </summary>
        /// <param name="terrainSize"></param>
        /// <param name="heightRange"></param>
        public void ApplyToTerrain(Vector2Int terrainSize, Vector2Int heightRange)
        {
            UnityEngine.Terrain terrain = UnityEngine.Terrain.activeTerrain;
            
            if (terrain == null)
            {
                Debug.LogError("No active terrain found");
                return;
            }
            
            Debug.Log($"Applying DEM to Terrain with size: {terrainSize}, resolution: {Mathf.Min(mapSize.x, mapSize.y)}, heightmap: {heightMap.Length}");

            // Real World Size
            // Min Height is Pos Y
            terrain.transform.position = terrain.transform.position.WithY(heightRange.x);
            terrain.terrainData.size = new Vector3(terrainSize.x, heightRange.y - heightRange.x, terrainSize.y);

            // Map Resolution. Min between X and Y
            int res = mapSize.x == mapSize.y ? mapSize.x : Mathf.Min(mapSize.x, mapSize.y);

            // Resolution must be 2^n
            if (!Mathf.IsPowerOfTwo(res)) res = Mathf.ClosestPowerOfTwo(res);
            
            terrain.terrainData.heightmapResolution = res;
            
            // Set Heights
            float[,] heights2D = new float[res, res];
            for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                // Map x,y to 0-1 -> to mapSize x,y
                // Busca los valores de altura mas cercanos
                float xt = (float)x / (res - 1);
                float yt = (float)y / (res - 1);

                float realX = Mathf.Lerp(0, mapSize.x - 1, xt);
                float realY = Mathf.Lerp(0, mapSize.y - 1, yt);
                int x0 = Mathf.FloorToInt(realX), x1 = Mathf.CeilToInt(realX);
                int y0 = Mathf.FloorToInt(realY), y1 = Mathf.CeilToInt(realY);
                
                if (x0 == x1 || y0 == y1) 
                {
                    heights2D[y, x] = heightMap[x0 + y0 * mapSize.x];
                    continue;
                }
                
                float denom = (x1 - x0) * (y1 - y0);
                float w00 = (x1 - realX) * (y1 - realY) / denom;
                float w01 = (x1 - realX) * (realY - y0) / denom;
                float w10 = (realX - x0) * (y1 - realY) / denom;
                float w11 = (realX - x0) * (realY - y0) / denom;
                
                // Interpolamos la altura entre esos valores mas cercanos
                float h00 = heightMap[x0 + y0 * mapSize.x], h01 = heightMap[x0 + y1 * mapSize.x];
                float h10 = heightMap[x1 + y0 * mapSize.x], h11 = heightMap[x1 + y1 * mapSize.x];
                float height = h00 * w00 + h01 * w01 + h10 * w10 + h11 * w11;
                
                heights2D[y, x] = height;
            }
            
            terrain.terrainData.SetHeights(0,0, heights2D);
            
        }

        #endregion
    }
}
