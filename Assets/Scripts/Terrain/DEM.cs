using System;
using System.Collections.Generic;
using System.Linq;
using DavidUtils.ExtensionMethods;
using SILVO.Misc_Utils;
using UnityEngine;

namespace SILVO.Terrain
{
    [Serializable]
    public class DEM
    {
        public string tiffPath;
        [HideInInspector] public float[] heightData;
        public int width, height;
        public Vector2 Size => new(width, height);
        
        public float minHeight, maxHeight;

        public TiffReader.TiffMetaData metaData;

        public bool IsEmpty => heightData.IsNullOrEmpty();

        public Vector2 WorldOrigin => metaData.originWorld;
        
        /// <summary>
        /// Get the Real Terrain Size by Sample Distance (between 2 samples)
        /// </summary>
        public Vector3 WorldSize => new(width * metaData.sampleScale.x, maxHeight - minHeight, height * metaData.sampleScale.y);
        public Vector2 WorldSize2D => new(width * metaData.sampleScale.x, height * metaData.sampleScale.y);
        
        public DEM(string tiffPath)
        {
            this.tiffPath = tiffPath;
            
            // READ TIFF
            (heightData, metaData) = TiffReader.ReadTiff(tiffPath);
            width = metaData.width;
            height = metaData.height;
            
            PostProcess();
            
            resPow2 = Mathf.ClosestPowerOfTwo(Mathf.Max(width, height));
            
            // Set MAX and MIN height and Normalize to use for Texture and Terrain
            minHeight = heightData.Min();
            maxHeight = heightData.Max();
            
            NormalizeHeightMap();

            PrepareHeightDataForTerrain();
        }
        
        
        public void NormalizeHeightMap()
        {
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                heightData[y * width + x] = Mathf.InverseLerp(minHeight, maxHeight, heightData[y * width + x]);
        }


        #region TEXTURE
        
        /// <summary>
        /// Crea una textura lowRes y samplea por Nearest
        /// </summary>
        public Texture2D CreateLowResGreyTexture(int res)
        {
            if (!Mathf.IsPowerOfTwo(res)) Debug.LogWarning("Texture Low Res must be 2^n to underscale correctly");
            
            var tex = new Texture2D(res, res)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            
            Color[] lowResHeightMap = new Color[res * res];
            for (int y = 0; y < res; y ++)
            for (var x = 0; x < res; x ++)
            {
                float xt = (float)x / res, yt = (float)y / res;
                float height = heightData[Mathf.FloorToInt(xt * width) + Mathf.FloorToInt(yt* this.height) * width];
                lowResHeightMap[x + y * res] = new Color(height , height, height);
            }

            tex.SetPixels(lowResHeightMap);
            tex.Apply();
            return tex;
        }

        #endregion


        #region TERRAIN
        
        // Size is 2^n x 2^n for Unity Terrain
        public float[,] heightDataForTerrain;
        public int resPow2;
        
        public void PrepareHeightDataForTerrain()
        {
            // Map Resolution -> Power of 2 Res
            resPow2 = Mathf.Min(width, height);

            // Resolution must be 2^n
            if (!Mathf.IsPowerOfTwo(resPow2)) resPow2 = Mathf.ClosestPowerOfTwo(resPow2);

            // Last row and column are not included
            resPow2 += 1;
            
            heightDataForTerrain = SampleTo2D(heightData, Mathf.Min(width, height), resPow2);
        }
        
        /// <summary>
        /// Sample and interpolate height values to adapt to another resolution
        /// And convert them to float[,] to apply to the terrain
        /// </summary>
        private static float[,] SampleTo2D(float[] heightMap, int origRes, int targetRes)
        {
            var rescaledHeightData = new float[targetRes, targetRes];
            for (int y = 0; y < targetRes; y++)
            for (int x = 0; x < targetRes; x++)
            {
                // Map x,y to 0-1 -> to mapSize x,y
                // Busca los valores de altura mas cercanos
                float xt = (float)x / (targetRes - 1);
                float yt = (float)y / (targetRes - 1);

                float realX = Mathf.Lerp(0, origRes - 1, xt);
                float realY = Mathf.Lerp(0, origRes - 1, yt);
                int x0 = Mathf.FloorToInt(realX), x1 = Mathf.CeilToInt(realX);
                int y0 = Mathf.FloorToInt(realY), y1 = Mathf.CeilToInt(realY);
                
                if (x0 == x1 || y0 == y1) 
                {
                    rescaledHeightData[y, x] = heightMap[x0 + y0 * origRes];
                    continue;
                }
                
                float denom = (x1 - x0) * (y1 - y0);
                float w00 = (x1 - realX) * (y1 - realY) / denom;
                float w01 = (x1 - realX) * (realY - y0) / denom;
                float w10 = (realX - x0) * (y1 - realY) / denom;
                float w11 = (realX - x0) * (realY - y0) / denom;
                
                // Interpolamos la altura entre esos valores mas cercanos
                float h00 = heightMap[x0 + y0 * origRes], h01 = heightMap[x0 + y1 * origRes];
                float h10 = heightMap[x1 + y0 * origRes], h11 = heightMap[x1 + y1 * origRes];
                float height = h00 * w00 + h01 * w01 + h10 * w10 + h11 * w11;
                
                rescaledHeightData[y, x] = height;
            }

            return rescaledHeightData;
        }
        
        #endregion


        #region POSTPROCESSING
        
        // Distancia maxima entre alturas de vertices vecinos
        static float maxValidDistance = 5;

        private void PostProcess()
        {
            CleanInvalidHeights();
            CleanAnomalousHeights();
            CleanInvalidHeights();
        }
        
        private void CleanInvalidHeights()
        {
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                float value = heightData[y * width + x];
                
                if (!InvalidHeight(value)) continue;
                
                heightData[y * width + x] = InterpolateValue(x,y);
            }
        }
        
        /// <summary>
        /// Limpia los datos de altura ANOMALOS (mayoria de los vecinos supera el maximo de dif de altura valido)
        /// Interpola el nuevo valor a partir de los vecinos
        /// </summary>
        private void CleanAnomalousHeights()
        {
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                float value = heightData[y * width + x];
                
                if (!AnomalousHeight(x, y) && !InvalidHeight(value)) continue;
                
                heightData[y * width + x] = InterpolateValue(x,y);
            }
        }
        
        private static bool InvalidHeight(float height) => 
            float.IsNaN(height) || height < 10;
        
        /// <summary>
        /// Detecta Alturas ANÓMALAS => Diferencia con vecinos > maxValidDistance
        /// Para evitar falsos positivos (vecinos de anomalos)
        /// Solo será anómalo si la mayoria de vecinos supera ese maximo de Distancia
        /// </summary>
        private bool AnomalousHeight(int x, int y)
        {
            float value = heightData[y * width + x];
            
            float[] difs = GetNeighbours3x3(x, y).Select(n => Mathf.Abs(n - value)).ToArray();
            
            // Si la MAYORIA de las diferencias son invalidas, la altura es anomala
            // En el area 3x3, 5 o + ya es mayoría
            return difs.Count(d => d > maxValidDistance) > 4;
        }

        #endregion
        
        
        
        /// <summary>
        /// Interpolate with adyacent neighbours
        /// </summary>
        private float InterpolateValue(int x, int y)
        {
            List<float> validNeighbours = new List<float>();
            for (int dy = -1; dy < 2; dy++)
            for (int dx = -1; dx < 2; dx++)
            {
                if ((dx == 0 && dy == 0) || x + dx < 0 || x + dx == width || y + dy < 0 || y + dy == height) continue;
                
                float n = heightData[(y + dy) * width + x + dx];
                if (InvalidHeight(n)) continue;
                if (AnomalousHeight(x + dx, y + dy)) continue;
                validNeighbours.Add(n);
            }
            
            if (validNeighbours.IsNullOrEmpty()) return -1;
            
            return validNeighbours.Average();
        }

        
        
        private float[] GetNeighbours(int x, int y)
        {
            int x0 = x - 1, x1 = x + 1, y0 = y - 1, y1 = y + 1;
                    
            float top = y1 == height ? -1 : heightData[y1 * width + x],
                bottom = y0 == -1 ? -1 : heightData[y0 * width + x],
                left = x0 == -1 ? -1 :  heightData[y * width + x0],
                right = x1 == width ? -1 :  heightData[y * width + x1];
            
            return new[] {top, bottom, left, right}.Where(n => n < 0).ToArray();
        }

        private float[] GetNeighbours3x3(int x, int y)
        {
            int x0 = x - 1, x1 = x + 1, y0 = y - 1, y1 = y + 1;
            float tl = y1 == height || x0 == -1 ? -1 : heightData[y1 * width + x0],
                tr = y1 == height || x1 == width ? -1 : heightData[y1 * width + x1],
                bl = y0 == -1 || x0 == -1 ? -1 : heightData[y0 * width + x0],
                br = y0 == -1 || x1 == width ? -1 : heightData[y0 * width + x1];
            return GetNeighbours(x, y).Concat(new[] { tl, tr, bl, br }.Where(n => n < 0).ToArray()).ToArray();
        }

        public override string ToString() => 
            $"DEM (loaded from {tiffPath})\n{metaData}; Min height: {minHeight}, Max height: {maxHeight}";
    }
}
