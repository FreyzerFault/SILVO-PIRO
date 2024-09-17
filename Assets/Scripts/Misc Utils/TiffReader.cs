using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using BitMiracle.LibTiff.Classic;
using UnityEngine;

namespace SILVO.Misc_Utils
{
    public static class TiffReader
    {
        [Serializable]
        public class TiffMetaData
        {
            public int width;
            public int height;
            public int bitsPerSample;
            public string format;

            public static TiffMetaData DefaultMetaData =>
                new TiffMetaData(0, 0, 0, "not defined");
            
            public TiffMetaData(int width, int height, int bitsPerSample, string format)
            {
                this.width = width;
                this.height = height;
                this.bitsPerSample = bitsPerSample;
                this.format = format;
            }

            public override string ToString() => 
                $"{width} x {height} - {format} {bitsPerSample} bits";
        }
        
        public static (float[], TiffMetaData) ReadTiff(string path)
        {
            // OPEN TIFF
            Tiff tiff = Tiff.Open(path, "r");
            if (tiff == null)
                return (Array.Empty<float>(), TiffMetaData.DefaultMetaData);
            
            // READ TIFF 
            tiff.ReadDirectory();
            
            // Meta Data
            int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            int bitsPerSample = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
            var format = tiff.GetField(TiffTag.SAMPLEFORMAT)[0].ToString();
            
            var metaData = new TiffMetaData(width, height, bitsPerSample, format);
            
            // Read Height Values
            var heightMap = new float[width * height];
            for (var y = 0; y < height; y++)
            {
                var scanline = new byte[tiff.ScanlineSize()];
                tiff.ReadScanline(scanline, y);
                
                // bytes -> 32 bits (float)
                float[] parsedScanline = BytesToFloat32(scanline);
                for (int x = 0; x < width; x++) 
                    heightMap[(height - 1 - y) * width + x] = parsedScanline[x];
            }
                
            tiff.Close();
            
            return (heightMap, metaData);
        }
        

        public static void WriteToTiff(string path, float[] heightMap)
        {
            // OPEN TIFF
            Tiff tiff = Tiff.Open(path, "w");
            if (tiff == null)
                throw new FileLoadException("Error WRITING to TIFF file", path);
            
            // WRITE TIFF 
            tiff.WriteDirectory();
            
            // Meta Data
            int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            int bitsPerSample = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
            var format = tiff.GetField(TiffTag.SAMPLEFORMAT)[0].ToString();
            
            var metaData = new TiffMetaData(width, height, bitsPerSample, format);
            
            if (heightMap.Length != width * height)
                throw new FileLoadException($"Map size mismatch. {width} x {height} != {heightMap.Length}", path);
            
            
            // Write Height Values
            for (var y = 0; y < height; y++)
            {
                float[] scanline = heightMap.Skip(y * width).Take(width).ToArray();
                tiff.WriteScanline(Float32ToBytes(scanline), y);
            }
                
            tiff.Close();
        }


        #region DATA CONVERSION

        public static float[] BytesToFloat32(byte[] bytes)
        {
            int length = bytes.Length / sizeof(float);
            var converted = new float[length];
            
            // Convert to 32 bits (float)
            for (var x = 0; x < length; x ++)
            {
                var value = BitConverter.ToSingle(bytes, x * sizeof(float));
                        
                // Check if it's NaN
                if (float.IsNaN(value)) value = 0;
                
                converted[x] = value;
            }

            return converted;
        }

        public static byte[] Float32ToBytes(float[] data)
        {
            var converted = new byte[data.Length * sizeof(float)];
            
            // Convert to 32 bits (float)
            for (var i = 0; i < data.Length; i ++)
            {
                var value = data[i];
                var bytesValue = BitConverter.GetBytes(value);
                
                for (var j = 0; j < bytesValue.Length; j++)
                    converted[i * sizeof(float) + j] = bytesValue[j];
            }

            return converted;
        }

        #endregion
    }
}
