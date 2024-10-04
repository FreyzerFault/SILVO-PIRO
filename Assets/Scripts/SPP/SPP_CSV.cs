using System;
using System.IO;
using System.Linq;
using Csv;
using UnityEngine;
using DavidUtils.ExtensionMethods;

namespace SILVO.SPP
{
    [Serializable]
    public class SPP_CSV
    {
        private static string separator = ",";
        private static string[] headers = { "time", "device_id", "msg_type", "position_time", "lat", "lon" };
        private static string[] headerLabels = { "Received", "ID", "Type", "Sent", "Lat", "Lon" };

        public string filePath;

        [SerializeField] public readonly ICsvLine[] csvLines;
        [SerializeField] public SPP_Signal[] signals;
        [SerializeField] public ICsvLine[] invalidLines;
        
        public bool IsEmpty => signals == null || signals.Length == 0;

        public SPP_CSV(string csvPath)
        {
            filePath = csvPath;

            csvLines = CsvReader.ReadFromText(ReadTextFile(filePath)).ToArray();

            (SPP_Signal, int)[] parsedSignals = csvLines.Select((line,i) => (ToSignal(line), i)).ToArray();
            
            signals = parsedSignals.Where((s) => s.Item1 != null).Select(s => s.Item1).ToArray();
            invalidLines = parsedSignals.Where(s => s.Item1 == null).Select(s => csvLines[s.Item2]).ToArray();

            UpdateInvalidLog();
            
            // DebugInvalidLog();
        }

        private static SPP_Signal ToSignal(ICsvLine line)
        {
            if (!headers.All(line.HasColumn))
                Debug.LogError($"CSV line does not contain all required headers: [{string.Join(", ", headers)}]");;
            
            var values = headers.Select(h => line[h]).ToArray();
            
            bool badId = !int.TryParse(line["device_id"], out int id);
            
            bool badReceivedDate = !DateTime.TryParse(line["time"], out DateTime receivedTime);
            bool badSentDate = !DateTime.TryParse(line["position_time"], out DateTime sentTime);
            
            bool badPosition = !float.TryParse(line["lon"], out float lon);
            badPosition = !float.TryParse(line["lat"], out float lat) || badPosition;
            
            Vector2 position = badPosition ? Vector2.zero : new Vector2(lon, lat);
            
            SPP_Signal.SignalType type = SPP_Signal.GetSignalType(line["msg_type"]);

            if (line["position_time"] == "")
            {
                // Debug.LogWarning($"Empty Seq signal: {line}");
                return null;
            }

            if (!badId && !badReceivedDate && !badSentDate && !badPosition)
                return new SPP_Signal(id, receivedTime, sentTime, position, type);

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
        
        private ICsvLine GetCsvLine(int index) => csvLines[index];
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

        
        #region LOG INFO

        public string headerLog;
        public string[] invalidLogs;

        public string[] GetInvalidLogs()
        {
            var invalidMsgs = invalidLines.Select(GetInvalidLog);
            return invalidMsgs.ToArray();
        }

        // static int MAX_COL_CHARS = 15;
        static int[] MAX_COL_CHARS = {20, 6, 10, 20, 10, 10};
        static int MAX_ROWS = 10;
        private string GetInvalidLog(ICsvLine line)
        {
            bool badId = !int.TryParse(line["device_id"], out int id);

            bool badReceivedDate = !DateTime.TryParse(line["time"], out DateTime receivedTime);
            bool badSentDate = !DateTime.TryParse(line["position_time"], out DateTime sentTime);

            bool badPosition = !float.TryParse(line["lon"], out float lon);
            badPosition = !float.TryParse(line["lat"], out float lat) || badPosition;
                
            SPP_Signal.SignalType type = SPP_Signal.GetSignalType(line["msg_type"]);
                
            string badColor = "#ff4f4c", goodColor = "gray";
                
            var badFlags = new[] {badReceivedDate, badId, false, badSentDate, badPosition, badPosition};

            var values = line.Values
                .Select((v,i) => (badFlags[i] ? "NULL" : v).TruncateFixedSize(MAX_COL_CHARS[i]).Colored(badFlags[i] ? badColor : goodColor));

            return string.Join(" | ", values);
        }

        private void UpdateInvalidLog()
        {
            headerLog = string.Join(" | ", headerLabels.Select((h,i) => h.TruncateFixedSize(MAX_COL_CHARS[i])));
            invalidLogs = GetInvalidLogs();
        }

        private void DebugInvalidLog() =>
            Debug.LogWarning($"{invalidLines.Length} INVALID LINES FOUND:\n".Colored("yellow") +
                             $"{headerLog}\n" +
                             string.Join("\n", invalidLogs.Take(MAX_ROWS)));

        #endregion
        
        
    }
}
