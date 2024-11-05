using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Csv;
using DavidUtils.DevTools.Table;
using UnityEngine;
using DavidUtils.ExtensionMethods;
using UnityEngine.Serialization;

namespace SILVO.SPP
{
    [Serializable]
    public class SPP_CsvLine : ICsvLine
    {
        private static string[] HEADERS = { "device_id", "sent_time", "received_time", "msg_type", "lat", "lon" };
        private static int[] MAX_COL_LENGHT = { 6, 20, 20, 10, 10, 10};
        
        public bool HasColumn(string name) => HEADERS.Contains(name);

        [SerializeField] public int index;
        [SerializeField] public string[] values;
        
        public string[] Headers => HEADERS;
        public int ColumnCount => Headers.Length;
        public string[] Values => values;

        public string Raw => string.Join(",", values.Select(v => v ?? ""));
        public int Index => index;

        public string this[string name] => HasColumn(name) ? this[Headers.IndexOf(name)] : "NULL";
        public string this[int i] => i < values.Length ? values[i] : "NULL";
        
        public bool IsValid => Index >= 0 && values.Length == ColumnCount;
        
        public SPP_CsvLine() : this("", -1) {}

        public SPP_CsvLine(ICsvLine line) : this(line.Raw, line.Index)
        {
            index = line.Index;
            values = line.Values;
        }
        
        public SPP_CsvLine(string raw, int index, char separator = ',')
        {
            this.index = index;
            values = raw.Split(separator);
        }

        #region PARSE
        
        /// <summary>
        /// Parse CSV Line to SPP Signal
        /// Null si hay una columna invalida
        /// </summary>
        public SPP_Signal TryParse(out bool[] badFlags)
        {
            badFlags = new bool[ColumnCount];
            Array.Fill(badFlags, false);
            
            // Debe tener todas las columnas
            if (!IsValid) 
                Debug.LogError($"CSV line is not valid: {this}\n" +
                               $"Index: {Index} | Values: {values.Length} | Headers: {ColumnCount}");
            
            
            // ID
            badFlags[0] = !int.TryParse(this["device_id"], out int id);
            
            // TIME
            badFlags[1] = !DateTime.TryParse(this["sent_time"], out DateTime sentTime);
            badFlags[2] = !DateTime.TryParse(this["received_time"], out DateTime receivedTime);
            
            // TYPE
            SPP_Signal.SignalType type = SPP_Signal.GetSignalType(this["msg_type"]);
            badFlags[3] = type == SPP_Signal.SignalType.Unknown;
            
            
            // POSITION
            badFlags[4] = !float.TryParse(this["lon"], NumberStyles.Float, new CultureInfo( "en-US"), out float lon);
            badFlags[5] = !float.TryParse(this["lat"], NumberStyles.Float, new CultureInfo( "en-US"), out float lat);
            Vector2 position = new Vector2(badFlags[4] ? 0 : lon, badFlags[5] ? 0 : lat);
            
            Debug.Log(position);
            
            // No sent time => EMPTY Signal. No unexpected error
            if (this["sent_time"] == "") return null;

            // Something wrong besides the sent time => Unexpected error!!
            if (badFlags.Any(b => b))
            {
                var errMsg = "";
                for (var i = 0; i < ColumnCount; i++)
                    if (badFlags[i])
                        errMsg += $"{HEADER_LABELS[i]} is invalid: {values[i]}\n";

                Debug.LogWarning($"Failed to parse CSV line: {this}\n" +
                                 $"{errMsg}");
                return null;
            }
            
            // Antes de crear la señal debemos asegurarnos de que la posicion sea valida
            // Hay datos que traen la posicion escalada * 1.000.000, sin punto decimal
            // Reescalarlo dividiendo / 1.000.000
            // (383831473 => 38.3831473)
            if (position.x > 180 || position.x < -180 || position.y > 90 || position.y < -90) 
                position /= 1000000;
            
            // All GOOD => Create Signal
            return new SPP_Signal(id, sentTime, receivedTime, type, position);
        }
        
        #endregion
        
        
        
        #region LABELS

        public static string[] HEADER_LABELS = { "ID", "Sent", "Received", "Type", "Lat", "Lon" };
        public static string Header_Table_Line => 
            string.Join(" | ", HEADER_LABELS.Select((label, i) => label.TruncateFixedSize(MAX_COL_LENGHT[i])));
        
        public string GetLabel(int i) => HEADER_LABELS[i];
        public string GetLabel(string name) => GetLabel(HEADERS.IndexOf(name));

        #endregion
        
        
        public override string ToString() => Raw;

        public string ToTableLine(bool colorInvalid = false)
        {
            if (!colorInvalid) return values.ToTableLine(MAX_COL_LENGHT);
            
            TryParse(out bool[] badFlags);
            return values.ToTableLine_ColoredByValidation(MAX_COL_LENGHT, badFlags);
        }
    }
    
    [Serializable]
    public class SPP_CSV
    {
        public string filePath;

        [HideInInspector, SerializeField] public SPP_CsvLine[] csvLines;
        [HideInInspector, SerializeField] public int[] validLineIndices;
        [HideInInspector, SerializeField] public int[] invalidLineIndices;
        
        public SPP_CsvLine[] ValidLines => csvLines?.FromIndices(validLineIndices)?.ToArray();
        public SPP_CsvLine[] InvalidLines => csvLines?.FromIndices(invalidLineIndices)?.ToArray();
        
        public int LineCount => csvLines.Length;
        
        [HideInInspector, SerializeField] public SPP_Signal[] signals = Array.Empty<SPP_Signal>();

        public bool IsEmpty => csvLines.IsNullOrEmpty() && signals.IsNullOrEmpty();

        public SPP_CSV(string csvPath, bool checkForSeparator = true, int maxLines = -1)
        {
            filePath = csvPath;

            // Read RAW Lines
            var rawCsvLines = CsvReader.ReadFromText(ReadTextFile(filePath)).ToArray();
            
            if (maxLines != -1) rawCsvLines = rawCsvLines.Take(maxLines).ToArray();
            csvLines = rawCsvLines.Select(line => new SPP_CsvLine(line)).ToArray();
        }
        
        public SPP_Signal this[int index] => signals[index];
        
        
        public SPP_Signal[] ParseAllSignals(int maxLines = -1, bool freeMemoryWhenParsed = false)
        {
            maxLines = Mathf.Min(maxLines, csvLines.Length);
            if (maxLines == -1) maxLines = csvLines.Length;

            List<SPP_Signal> signalsParsed = new(maxLines);
            List<int> validLineIndicesList = new List<int>(maxLines);
            List<int> invalidLineIndicesList = new List<int>(maxLines);
            
            for (int i = 0; i < maxLines; i++)
            {
                SPP_CsvLine line = csvLines[i];
                SPP_Signal signal = line.TryParse(out _);
                
                if (signal == null)
                    invalidLineIndicesList.Add(i);
                else
                {
                    validLineIndicesList.Add(i);
                    signalsParsed.Add(signal);
                }
            }

            
            validLineIndices = validLineIndicesList.ToArray();
            invalidLineIndices = invalidLineIndicesList.ToArray();
            
            // DebugInvalidLog();
            
            // CLEAN MEMORY for Performance
            if (freeMemoryWhenParsed)
                FreeCsvMemory();

            return signals = signalsParsed.ToArray();;
        }
        

        private void SortSignals()
        {
            var list = signals.ToList();
            list.Sort();
            signals = list.ToArray();
        }

        private void FreeCsvMemory()
        {
            csvLines = Array.Empty<SPP_CsvLine>();
            invalidLineIndices = Array.Empty<int>();
            validLineIndices = Array.Empty<int>();
        }
        
        
        #region CSV

        private SPP_CsvLine GetCsvLine(int index) => csvLines[index];
        private string GetCsvValue(int index, string header) => GetCsvLine(index)[header];

        private static string ReadTextFile(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{e.Message}]: Failed to open CSV file with SPP Data: {path}");
                return "";
            }
        }

        #endregion
        
        
        #region LOG INFO

        // static int MAX_COL_CHARS = 15;
        static int MAX_DEBUG_ROWS = 10;
        
        public string HeaderLineColored => SPP_CsvLine.Header_Table_Line.Colored("cyan");


        // TABLE FORMAT
        public string[] GetInvalidTable(int count = -1, bool header = false, bool colorInvalid = true) =>
            GetTable(InvalidLines, count, header, colorInvalid);
        
        public string[] GetValidTable(int count = -1, bool header = false, bool colorInvalid = true) => 
            GetTable(ValidLines, count, header, colorInvalid);
        
        public string[] GetTable(int count = -1, bool header = false, bool colorInvalid = true) => 
            GetTable(csvLines, count, header, colorInvalid);

        public string[] GetTable(IEnumerable<SPP_CsvLine> lines, int count = -1, bool header = false, bool colorInvalid = false) => 
            (header ? new[] {HeaderLineColored} : Array.Empty<string>()).Concat(
                (count == -1 ? lines : lines.Take(count))
                .Select(line => line.ToTableLine(colorInvalid)))
            .ToArray();
        
        private void DebugInvalidLog() =>
            Debug.LogWarning($"{invalidLineIndices.Length} INVALID LINES FOUND:\n".Colored("yellow") +
                             $"{HeaderLineColored}\n" +
                             string.Join("\n", GetInvalidTable(MAX_DEBUG_ROWS)));
        #endregion


        #region ASYNC PARSE

        public IEnumerator ParseLinesCoroutine()
        {
            foreach (SPP_CsvLine line in csvLines)
            {
                ParseLine(line);
                yield return null;
            }
        }

        public (SPP_Signal, SPP_CsvLine) ParseLine(int i)
        {
            if (i < 0 || i >= csvLines.Length) return (null, null);
            return ParseLine(csvLines[i]);
        } 
        
        public (SPP_Signal, SPP_CsvLine) ParseLine(SPP_CsvLine line)
        {
            SPP_Signal signal = line.TryParse(out bool[] _);
                    
            if (signal != null)
            {
                signals = signals.Append(signal).ToArray();
                validLineIndices = validLineIndices.Append(csvLines.Length - 1).ToArray();
            }
            else
                invalidLineIndices = invalidLineIndices.Append(csvLines.Length - 1).ToArray();
 
            return (signal, line);
        }

        #endregion
    }
}
