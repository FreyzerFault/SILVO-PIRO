using System;
using System.IO;
using System.Linq;
using BitMiracle.LibTiff.Classic;
using DavidUtils.ExtensionMethods;
using DotSpatial.Data;
using DotSpatial.Projections;
using SILVO.GeoReferencing;
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
            
            public Vector2Int Size => new Vector2Int(width, height);
            
            // Geo Data
            public Vector2 sampleScale;
            public Vector2 originRaster;
            public Vector2 originWorld;
            public string projectionStr;
            
            // TODO Extraer la ProjectionInfo del GEO-Tiff
            public ProjectionInfo Projection => GeoProjections.Utm30NProjInfo;

            public Vector2Int WorldSize => new Vector2Int((int)(width * sampleScale.x), (int)(height * sampleScale.y));

            public static TiffMetaData DefaultMetaData =>
                new TiffMetaData(0, 0, 0, "not defined");
            
            public TiffMetaData(int width, int height, int bitsPerSample, string format, 
                Vector2 sampleScale = default, Vector2 originRaster = default, Vector2 originWorld = default,
                string projectionStr = "")
            {
                this.width = width;
                this.height = height;
                this.bitsPerSample = bitsPerSample;
                this.format = format;
                this.sampleScale = sampleScale;
                this.originRaster = originRaster;
                this.originWorld = originWorld;
                this.projectionStr = projectionStr;
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

            // GEOTIFF Meta Data
            var keyDirectoryTag = tiff.GetField(TiffTag.GEOTIFF_GEOKEYDIRECTORYTAG);
            
            // This are NULL
            // var geoTiffTag5 = tiff.GetField(TiffTag.GEOTIFF_GEODOUBLEPARAMSTAG);
            // var geoTiffTag6 = tiff.GetField(TiffTag.GEOTIFF_MODELTRANSFORMATIONTAG);

            // Escala de los pixeles en el mundo real (metros por pixel)
            var pixelScaleTag = tiff.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
            double[] pixelScaleArray = pixelScaleTag[1].ToDoubleArray();
            Vector2 pixelScale = new Vector2((float)pixelScaleArray[0], (float)pixelScaleArray[1]);
            
            // Puntos de origen para transformar de raster a mundo
            var modelTiepoint = tiff.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG);
            double[] tiepoints = modelTiepoint[1].ToDoubleArray();
            Vector2 originRaster = new Vector2((float)tiepoints[0], (float)tiepoints[1]);
            Vector2 originWorld = new Vector2((float)tiepoints[3], (float)tiepoints[4]);
            
            // TIFF starts at the top left corner
            originWorld.y = originWorld.y - height * pixelScale.y;
            
            // PROJECTION Name
            var asciiParamsTag = tiff.GetField(TiffTag.GEOTIFF_GEOASCIIPARAMSTAG);
            var projectionStr = asciiParamsTag[1].ToString();
            
            var metaData = new TiffMetaData(
                width, height, bitsPerSample, format, pixelScale,
                originRaster, originWorld, projectionStr);
            
            // DEBUG GEOTIFF TAGS
            {
                // string tag1 = string.Join(" | ",
                //     modelTiepoint.Skip(1).Select(tag => string.Join(", ", tag.ToDoubleArray())));
                // string tag2 = asciiParamsTag[1].ToString();
                // string tag3 = string.Join("| ",
                //     keyDirectoryTag.Skip(1).Select(tag => string.Join(", ", tag.ToIntArray())));
                // string tag4 = string.Join("| ",
                //     pixelScaleTag.Skip(1).Select(tag => string.Join(", ", tag.ToDoubleArray())));
                //
                // Debug.Log($"<color=orange>Model Tie Point:</color> {tag1}");
                // Debug.Log($"<color=orange>GEO ASCII Params:</color> {tag2}");
                // Debug.Log($"<color=orange>GEO Key Directory:</color> {tag3}");
                // Debug.Log($"<color=orange>Model Pixel Scale:</color> {tag4}");
            }
            
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
