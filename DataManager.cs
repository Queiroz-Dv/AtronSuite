using System.Runtime.Serialization.Json; // Para JSON nativo
using System.Text;

namespace AtronSuite
{
    public static class DataManager
    {
        private static string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "atron_settings.ini");
        private static string HistoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sentinel_history.json");

        public static void SaveSettings(bool enabled, int thresholdGB, int intervalMinutes)
        {
            try { File.WriteAllText(SettingsPath, $"MonitorEnabled={enabled}\nThresholdGB={thresholdGB}\nIntervalMinutes={intervalMinutes}"); } catch { }
        }

        public static (bool Enabled, int Threshold, int Interval) LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var lines = File.ReadAllLines(SettingsPath);
                    bool en = bool.Parse(lines[0].Split('=')[1]);
                    int th = int.Parse(lines[1].Split('=')[1]);
                    int inv = lines.Length > 2 ? int.Parse(lines[2].Split('=')[1]) : 30;
                    return (en, th, inv);
                }
            }
            catch { }
            return (false, 1, 30);
        }

        public static void SaveHistory(List<AnalysisLog> logs)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    var ser = new DataContractJsonSerializer(typeof(List<AnalysisLog>));
                    ser.WriteObject(ms, logs);
                    File.WriteAllText(HistoryPath, Encoding.UTF8.GetString(ms.ToArray()));
                }
            }
            catch { }
        }

        public static List<AnalysisLog> LoadHistory()
        {
            try
            {
                if (File.Exists(HistoryPath))
                {
                    string json = File.ReadAllText(HistoryPath);
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    {
                        var ser = new DataContractJsonSerializer(typeof(List<AnalysisLog>));
                        return (List<AnalysisLog>)ser.ReadObject(ms);
                    }
                }
            }
            catch { }
            return new List<AnalysisLog>();
        }

        public static void DeleteHistory()
        {
            if (File.Exists(HistoryPath)) File.Delete(HistoryPath);
        }
    }
}