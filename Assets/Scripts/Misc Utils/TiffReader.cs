using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using BitMiracle.LibTiff.Classic;
using DavidUtils.ExtensionMethods;
using DotSpatial.Data;
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

            Debug.Log($"<color=orange>Model TiePoint Tag: {tiff.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG)?[0]}\n" +
                      $"Geo ASCII Params Tag: {tiff.GetField(TiffTag.GEOTIFF_GEOASCIIPARAMSTAG)?[0]}\n" +
                      $"Geo Key Directory Tag: {tiff.GetField(TiffTag.GEOTIFF_GEOKEYDIRECTORYTAG)?[0]}\n" +
                      $"Model Pixel Scale Tag: {tiff.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG)?[0]}\n" +
                      $"Geo Double Params Tag: {tiff.GetField(TiffTag.GEOTIFF_GEODOUBLEPARAMSTAG)?[0]}\n" +
                      $"Model Transformation Tag: {tiff.GetField(TiffTag.GEOTIFF_MODELTRANSFORMATIONTAG)?[0]}</color>");
             
            
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


        #region USING DOT SPATIAL LIB

        public static (float[], TiffMetaData) ReadGeoTiff(string path)
        {
            if (Raster.OpenFile(path) is not Raster raster)
                throw new FileLoadException("Error READING GeoTIFF file", path);

            Debug.Log($"Raster loaded: {raster.Filename}. Type: {raster.DataType}");

            var rasterFloat = raster.ToFloatRaster();

            float[] data = rasterFloat.Data.Flatten();

            var metaData = new TiffMetaData(
                (int)rasterFloat.Extent.Width,
                (int)rasterFloat.Extent.Height,
                raster.ByteSize * 8, 
                raster.DataType.ToString());

            return (data, metaData);
        }

        #endregion
    }
}
