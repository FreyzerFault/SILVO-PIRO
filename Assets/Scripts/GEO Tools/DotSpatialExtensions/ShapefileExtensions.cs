using System;
using System.Data;
using System.Linq;
using DavidUtils.ExtensionMethods;
using DotSpatial.Data;
using UnityEngine;

namespace SILVO.GEO_Tools.DotSpatialExtensions
{
    public static class ShapefileExtensions
    {
        
        
        #region INFO

        public static void DebugAllSHPInfo(Shapefile shp)
        {
            // SHAPEFILE INFO
            Debug.Log(ParseShapeFile(shp));

            // DATA TABLE
            Debug.Log(ParseTable(shp.DataTable));
            
            // SHAPE
            Shape shape = shp.GetShape(0, true);
        }
        
        private static string ParseShapeFile(Shapefile shp)
        {
            if (shp.Features is not FeatureList features) 
                throw new Exception("No features in SHP file. Can't work with it");
            
            FeatureType[] types = features.Select(f => f.FeatureType).ToArray();
            
            return $"<color=lime>Imported <i><b>{shp.Name}</b></i>:</color> <b>{shp.Features.Count} features</b>\n" +
                   $"Types: <color=cyan>{(types.Length == 1 ? shp.FeatureType : string.Join(", ", types))}</color>\n" +
                   $"Projection: <color=yellow>{shp.ProjectionString}</color>\n" +
                   $"Bounds: <color=orange>{shp.Extent}</color>";
        }
        
        private static string ParseShape(Shape shape)
        {
            // EXTENT
            Extent extent = shape.Range.CalculateExtents();
            var min = new Vector2((float)extent.MinX, (float)extent.MinY);
            var max = new Vector2((float)extent.MaxX, (float)extent.MaxY);
            var width = (float)extent.Width;
            var height = (float)extent.Height;

            return $"<b>Shape: ({shape.Vertices.Length / 2} vertices)</b>\n" +
                   $"Extent: <color=orange>[MIN {min}, MAX {max}]. Width: {width}, Height: {height}</color>\n" +
                   $"Vertices: <color=teal>{string.Join(", ", shape.Vertices.Take(10))} {(shape.Vertices.Length > 10 ? "..." : "")}</color>\n";
        }

        #endregion
        
        
        #region DATA TABLE
        
        private struct DataTableLogConfig
        {
            public int maxColLog, maxRowLog, maxItemChars;
            public string colSeparator;
        }
        private static DataTableLogConfig dtConfig = new()
        {
            maxColLog = 8, maxRowLog = 20, maxItemChars = 10, colSeparator = " | "
        };
        
        public static string ParseTable(DataTable table)
        {
            DataRowCollection rows = table.Rows;
            DataColumnCollection cols = table.Columns;
            
            return "<color=cyan>DATA TABLE:</color>\n" +
                   $"<b>{ParseColumnCollection(cols)} {(cols.Count > dtConfig.maxColLog ? "..." : "")}</b>\n" +
                   $"{string.Join("\n", rows.Cast<DataRow>().Select(ParseRow))} {(cols.Count > dtConfig.maxColLog ? "..." : "")}";
        }
        
        public static string ParseColumnCollection(DataColumnCollection col) =>
            string.Join(dtConfig.colSeparator, col.Cast<DataColumn>().Take(dtConfig.maxColLog).Select(c => c.ColumnName.TruncateFixedSize(dtConfig.maxItemChars)));

        public static string ParseRow(DataRow row) =>
            string.Join(dtConfig.colSeparator, row.ItemArray.Take(dtConfig.maxColLog).Select(item => item.ToString().TruncateFixedSize(dtConfig. maxItemChars)));
        

        #endregion

    }
}
