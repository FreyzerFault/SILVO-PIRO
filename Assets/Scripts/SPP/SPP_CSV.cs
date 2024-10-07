using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Csv;
using UnityEngine;
using DavidUtils.ExtensionMethods;

namespace SILVO.SPP
{
    [Serializable]
    public class SPP_CsvLine : ICsvLine
    {
        private static string[] _headers = { "time", "device_id", "msg_type", "position_time", "lat", "lon" };
        public static string[] headerLabels = { "Received", "ID", "Type", "Sent", "Lat", "Lon" };
        
        public bool HasColumn(string name) => _headers.Contains(name);

        public string[] Headers => _headers;
        public int ColumnCount => Headers.Length;
        
        [SerializeField]
        public string[] Values { get; }
        [SerializeField]public string Raw { get; }
        [SerializeField]public int Index { get; }

        public string this[string name] => HasColumn(name) ? this[Headers.IndexOf(name)] : "NULL";
        public string this[int index] => index < Values.Length ? Values[index] : "NULL";
        
        public bool IsValid => Index >= 0 && Values.Length == ColumnCount;
        
        public SPP_CsvLine() : this("", -1) {}

        public SPP_CsvLine(ICsvLine line) : this(line.Raw, line.Index)
        {
            
        }
        public SPP_CsvLine(string raw, int index, string separator = ",")
        {
            Raw = raw;
            Index = index;
            Values = raw.Split(separator);
        }
        
        
        public string GetLabel(int index) => headerLabels[index];
        public string GetLabel(string name) => GetLabel(_headers.IndexOf(name));

        public override string ToString() => Raw;
    }
    
    [Serializable]
    public class SPP_CSV
    {
        public string filePath;

        [HideInInspector, SerializeField] public List<string> lines; 
        [HideInInspector, SerializeField] public List<SPP_CsvLine> csvLines;
        [HideInInspector, SerializeField] public List<SPP_Signal> signals = new();
        [HideInInspector, SerializeField] public List<SPP_CsvLine> invalidLines = new();

        public bool IsEmpty => lines.IsNullOrEmpty();

        public SPP_CSV(string csvPath)
        {
            filePath = csvPath;

            csvLines = CsvReader.ReadFromText(ReadTextFile(filePath)).Select(line =>
            {
                Debug.Log($"Creating CSV Line: {line}\n" +
                          $"Raw: {line.Raw}\n" +
                          $"Values: {string.Join(",", line.Values)}");
                return new SPP_CsvLine(line.Raw, line.Index);
            }).ToList();
            lines = csvLines.Select(l => l.ToString()).ToList();
        }
        
        public SPP_Signal[] ParseSignals()
        {
            (SPP_Signal, int)[] parsedSignals = csvLines.Select((line,i) => (TryParseToSignal(line), i)).ToArray();
            
            // Dividimos las lineas entre validas (no nulas) e invalidas (Lineas originales de las nulas)
            signals = parsedSignals.Where((s) => s.Item1 != null).Select(s => s.Item1).ToList();
            invalidLines = parsedSignals.Where(s => s.Item1 == null).Select(s => csvLines[s.Item2]).ToList();
            
            UpdateInvalidLog();
            // DebugInvalidLog();
            return signals.ToArray();
        }

        /// <summary>
        /// Parse CSV Line to SPP Signal
        /// Null si hay una columna invalida
        /// </summary>
        public static SPP_Signal TryParseToSignal(SPP_CsvLine line)
        {
            // Debe tener todas las columnas
            if (!line.IsValid)
                Debug.LogError($"CSV line is not valid");
            
            // ID
            bool badId = !int.TryParse(line["device_id"], out int id);
            
            // DATEs
            bool badReceivedDate = !DateTime.TryParse(line["time"], out DateTime receivedTime);
            bool badSentDate = !DateTime.TryParse(line["position_time"], out DateTime sentTime);
            
            // POSITION
            bool badPosition = !float.TryParse(line["lon"], out float lon);
            badPosition = !float.TryParse(line["lat"], out float lat) || badPosition;
            Vector2 position = badPosition ? Vector2.zero : new Vector2(lon, lat);

            // TYPE
            SPP_Signal.SignalType type = SPP_Signal.GetSignalType(line["msg_type"]);

            // No sent time => Invalid Signal. No unexpected error
            if (line["position_time"] == "") return null;

            // All GOOD => Create Signal
            if (!badId && !badReceivedDate && !badSentDate && !badPosition)
                return new SPP_Signal(id, receivedTime, sentTime, position, type);

            // Something wrong besides the sent time => Unexpected error!!
            var errMsg = "";
            if (badId) errMsg += $"ID is invalid: {line["device_id"]}\n";
            if (badReceivedDate) errMsg += $"Received Date is invalid: {line["time"]}\n";
            if (badSentDate) errMsg += $"Sent Date is invalid: {line["position_time"]}\n";
            if (badPosition) errMsg += $"Position is invalid: {line["lon"]}, {line["lat"]}\n";
            
            Debug.LogWarning($"Failed to parse CSV line: {line}\n" +
                           $"{errMsg}");
            return null;
        }
        
        public SPP_Signal this[int index] => signals[index];

        private void SortSignals()
        {
            signals.Sort((a,b) =>
            {
                int idComparison = a.id.CompareTo(b.id);
                return idComparison == 0 ? a.sentTime.CompareTo(b.sentTime) : idComparison;
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

        [HideInInspector] public string headerLog;
        [HideInInspector, SerializeField] public List<string> invalidLogs = new();

        public string[] GetInvalidLogs()
        {
            var invalidMsgs = invalidLines.Select(GetInvalidLog);
            return invalidMsgs.ToArray();
        }

        // static int MAX_COL_CHARS = 15;
        static int[] MAX_COL_CHARS = {20, 6, 10, 20, 10, 10};
        static int MAX_ROWS = 10;
        private string GetInvalidLog(SPP_CsvLine line)
        {
            bool badId = !int.TryParse(line["device_id"], out int _);

            bool badReceivedDate = !DateTime.TryParse(line["time"], out DateTime _);
            bool badSentDate = !DateTime.TryParse(line["position_time"], out DateTime _);

            bool badPosition = !float.TryParse(line["lon"], out float _);
            badPosition = !float.TryParse(line["lat"], out float _) || badPosition;
                
            SPP_Signal.SignalType type = SPP_Signal.GetSignalType(line["msg_type"]);
                
            string badColor = "#ff4f4c", goodColor = "gray";
                
            var badFlags = new[] {badReceivedDate, badId, false, badSentDate, badPosition, badPosition};

            var values = line.Values
                .Select((v,i) => (badFlags[i] ? "NULL" : v).TruncateFixedSize(MAX_COL_CHARS[i]).Colored(badFlags[i] ? badColor : goodColor));

            return string.Join(" | ", values);
        }

        public void UpdateInvalidLog()
        {
            headerLog = string.Join(" | ", SPP_CsvLine.headerLabels.Select((h,i) => h.TruncateFixedSize(MAX_COL_CHARS[i])));
            invalidLogs = GetInvalidLogs().ToList();
        }

        private void DebugInvalidLog() =>
            Debug.LogWarning($"{invalidLines.Count} INVALID LINES FOUND:\n".Colored("yellow") +
                             $"{headerLog}\n" +
                             string.Join("\n", invalidLogs.Take(MAX_ROWS)));

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
                    
            SPP_Signal signal = TryParseToSignal(line);
                    
            if (signal != null)
            {
                signals.Add(signal);
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
