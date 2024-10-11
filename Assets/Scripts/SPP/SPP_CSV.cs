using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Csv;
using UnityEngine;
using DavidUtils.ExtensionMethods;
using UnityEngine.Serialization;

namespace SILVO.SPP
{
    [Serializable]
    public class SPP_CsvLine : ICsvLine
    {
        private static string[] _headers = { "time", "device_id", "msg_type", "position_time", "lat", "lon" };
        
        public bool HasColumn(string name) => _headers.Contains(name);

        [SerializeField] public int index;
        [SerializeField] public string raw;
        [SerializeField] public string[] values;
        
        public string[] Headers => _headers;
        public int ColumnCount => Headers.Length;
        public string[] Values => values;

        public string Raw => raw;
        public int Index => index;

        public string this[string name] => HasColumn(name) ? this[Headers.IndexOf(name)] : "NULL";
        public string this[int index] => index < values.Length ? values[index] : "NULL";
        
        public bool IsValid => Index >= 0 && values.Length == ColumnCount;
        
        public SPP_CsvLine() : this("", -1) {}

        public SPP_CsvLine(ICsvLine line) : this(line.Raw, line.Index){ }
        
        public SPP_CsvLine(string raw, int index, string separator = ",")
        {
            this.raw = raw;
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
            
            // RECEIVED TIME
            badFlags[0] = !DateTime.TryParse(this["time"], out DateTime receivedTime);
            
            // ID
            badFlags[1] = !int.TryParse(this["device_id"], out int id);
            
            // TYPE
            SPP_Signal.SignalType type = SPP_Signal.GetSignalType(this["msg_type"]);
            badFlags[2] = type == SPP_Signal.SignalType.Unknown;
            
            // SENT TIME
            badFlags[3] = !DateTime.TryParse(this["position_time"], out DateTime sentTime);
            
            // POSITION
            badFlags[4] = !float.TryParse(this["lon"], NumberStyles.Float, new CultureInfo( "en-US"), out float lon);
            badFlags[5] = !float.TryParse(this["lat"], NumberStyles.Float, new CultureInfo( "en-US"), out float lat);
            Vector2 position = new Vector2(badFlags[4] ? 0 : lon, badFlags[5] ? 0 : lat);
            
            // No sent time => EMPTY Signal. No unexpected error
            if (this["position_time"] == "") return null;

            // Something wrong besides the sent time => Unexpected error!!
            if (badFlags.Any(b => b))
            {
                var errMsg = "";
                for (var i = 0; i < ColumnCount; i++)
                    if (badFlags[i])
                        errMsg += $"{headerLabels[i]} is invalid: {values[i]}\n";

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
            return new SPP_Signal(id, receivedTime, sentTime, position, type);
        }
        
        #endregion
        
        
        
        #region LABELS

        public static string[] headerLabels = { "Received", "ID", "Type", "Sent", "Lat", "Lon" };
        
        public string GetLabel(int index) => headerLabels[index];
        public string GetLabel(string name) => GetLabel(_headers.IndexOf(name));

        #endregion
        
        
        public override string ToString() => Raw;
    }
    
    [Serializable]
    public class SPP_CSV
    {
        public string filePath;

        [HideInInspector, SerializeField] public List<string> lines; 
        [HideInInspector, SerializeField] public List<SPP_CsvLine> csvLines;
        [HideInInspector, SerializeField] public List<SPP_CsvLine> validLines = new();
        [HideInInspector, SerializeField] public List<SPP_CsvLine> invalidLines = new();
        
        [HideInInspector, SerializeField] public List<SPP_Signal> signals = new();

        public bool IsEmpty => lines.IsNullOrEmpty();

        public SPP_CSV(string csvPath)
        {
            filePath = csvPath;

            // Read RAW Lines
            var rawCsvLines = CsvReader.ReadFromText(ReadTextFile(filePath)).ToArray();
            
            // Check Separator used
            char[] possibleSeparators = {',', ';'};
            string rawSampleLine = rawCsvLines.First().Raw.Replace(";;", "; ;").Replace(",,", ", ,");
            Debug.Log("Searching for Separator...\n" +
                      $"Column Count: {rawCsvLines.First().ColumnCount}" +
                      $"After Replace Duplicate Comas:\n" +
                      $"{rawCsvLines.First().Raw.Replace(",,", ", ,")}\n" +
                      $"{rawCsvLines.First().Raw.Replace(";;", "; ;")}\n" +
                      $"Repeticiones de coma: {rawSampleLine.Count(c => c == ',')}\n" +
                      $"{rawSampleLine.Count(c => c == ';')}");
            char separator = possibleSeparators.First(sep => rawSampleLine.Count(c => c == sep) == rawCsvLines.First().ColumnCount - 1);
            
            csvLines = rawCsvLines.Select(line => new SPP_CsvLine(line.Raw, line.Index, separator.ToString())).ToList();
            lines = csvLines.Select(l => l.ToString()).ToList();
        }
        
        public SPP_Signal this[int index] => signals[index];
        
        
        public SPP_Signal[] ParseAllSignals()
        {
            (SPP_Signal, SPP_CsvLine)[] parsedSignals = csvLines.Select((line) => (line.TryParse(out _), line)).ToArray();
            
            // Dividimos las lineas entre nulas y no nulas. Las no nulas son signals validas
            validLines = parsedSignals.Where(s => s.Item1 != null).Select(s => s.Item2).ToList();
            invalidLines = parsedSignals.Where(s => s.Item1 == null).Select(s => s.Item2).ToList();
            
            signals = parsedSignals.Where((s) => s.Item1 != null).Select(s => s.Item1).ToList();
            
            UpdateLog();
            // DebugInvalidLog();
            return signals.ToArray();
        }
        

        private void SortSignals()
        {
            signals.Sort((a,b) =>
            {
                int idComparison = a.id.CompareTo(b.id);
                return idComparison == 0 ? a.SentDateTime.CompareTo(b.SentDateTime) : idComparison;
            });
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
        static int[] MAX_COL_CHARS = {20, 6, 10, 20, 10, 10};
        static int MAX_ROWS = 10;
        
        [HideInInspector] public string headerLog;
        [HideInInspector, SerializeField] public List<string> allLog = new();
        [HideInInspector, SerializeField] public List<string> validLogs = new();
        [HideInInspector, SerializeField] public List<string> invalidLogs = new();

        public void UpdateLog()
        {
            headerLog = string.Join(" | ", SPP_CsvLine.headerLabels.Select((h,i) => h.TruncateFixedSize(MAX_COL_CHARS[i]))).Colored("cyan");
            allLog = csvLines.Select(GetInvalidLog).ToList();
            validLogs = validLines.Select(GetLog).ToList();
            invalidLogs = invalidLines.Select(GetInvalidLog).ToList();
        }

        private void DebugInvalidLog() =>
            Debug.LogWarning($"{invalidLines.Count} INVALID LINES FOUND:\n".Colored("yellow") +
                             $"{headerLog}\n" +
                             string.Join("\n", invalidLogs.Take(MAX_ROWS)));


        public string[] GetInvalidLogs() => invalidLines.Select(GetInvalidLog).ToArray();

        public string GetLog(SPP_CsvLine line) =>
            string.Join(" | ", line.values.Select((v, i) => $"<color=white>{v.TruncateFixedSize(MAX_COL_CHARS[i])}</color>"));

        private string GetInvalidLog(SPP_CsvLine line)
        {
            line.TryParse(out bool[] badFlags);
                
            string badColor = "#ff4f4c", goodColor = "gray";

            var values = line.values
                .Select((v,i) => (badFlags[i] ? "NULL" : v).TruncateFixedSize(MAX_COL_CHARS[i]).Colored(badFlags[i] ? badColor : goodColor));

            return string.Join(" | ", values);
        }

        #endregion


        #region ASYNC PARSE

        public IEnumerator ParseLinesCoroutine()
        {
            foreach (var line in csvLines)
            {
                ParseLine(line);
                yield return null;
            }
        }
        
        public (SPP_Signal, SPP_CsvLine) ParseLine(SPP_CsvLine line)
        {
            csvLines.Add(line);
            lines.Add(line.ToString());
            
            allLog.Add(GetLog(line));
            
            SPP_Signal signal = line.TryParse(out bool[] badFlags);
                    
            if (signal != null)
            {
                signals.Add(signal);
                validLogs.Add(allLog[^1]);
            }
            else
            {
                invalidLines.Add(line);
                invalidLogs.Add(GetInvalidLog(line));
            }
 
            return (signal, line);
        }

        #endregion
    }
}
