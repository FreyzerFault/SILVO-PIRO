using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Csv;
using UnityEngine;

namespace SILVO.Asset_Importers
{
    public class SPP_CSV
    {
        private static string separator = ",";
        private static string[] headers = { "time", "device_id", "msg_type", "position_time", "lat", "lon" };

        public string filePath;

        private readonly ICsvLine[] _csvLines;
        public SPP_Signal[] signals;
        
        public bool IsEmpty => signals.Length == 0;

        public SPP_CSV(string csvPath)
        {
            filePath = csvPath;

            _csvLines = CsvReader.ReadFromText(ReadTextFile(filePath)).ToArray();
            signals = _csvLines.Select(ToSignal).Where(s => s != null).ToArray();
        }

        private static SPP_Signal ToSignal(ICsvLine line)
        {
            if (!headers.All(line.HasColumn))
                Debug.LogError($"CSV line does not contain all required headers: [{string.Join(", ", headers)}]");;
            
            bool badId = !int.TryParse(line["device_id"], out int id);
            
            var culture = CultureInfo.CreateSpecificCulture("es-ES");
            bool badReceivedDate = !DateTime.TryParse(line["time"], out DateTime receivedTime);
            bool badSentDate = !DateTime.TryParse(line["position_time"], out DateTime sentTime);
            
            bool badPosition = !float.TryParse(line["lon"], out float lon);
            badPosition = !float.TryParse(line["lat"], out float lat) || badPosition;
            
            Vector2 position = badPosition ? Vector2.zero : new Vector2(lon, lat);
            
            SPP_Signal.SignalType type = SPP_Signal.GetSignalType(line["msg_type"]);

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
        
        private ICsvLine GetCsvLine(int index) => _csvLines[index];
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
    }
}
