using System.Runtime.Serialization.Json;
using System.Text;

namespace AtronSuiteApp
{
    public static class DataManager
    {
        private static string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "atron_settings.ini");
        private static string HistoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sentinel_history.json");

        public static void SaveSettings(bool enabled, int thresholdGB, int intervalMinutes, bool autoClean)
        {
            try
            {
                File.WriteAllText(SettingsPath, $"MonitorEnabled={enabled}\nThresholdGB={thresholdGB}\nIntervalMinutes={intervalMinutes}\nAutoClean={autoClean}");
            }
            catch { }
        }

        public static (bool Enabled, int Threshold, int Interval, bool AutoClean) LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var lines = File.ReadAllLines(SettingsPath);
                    bool en = bool.Parse(lines[0].Split('=')[1]);
                    int th = int.Parse(lines[1].Split('=')[1]);
                    int inv = lines.Length > 2 ? int.Parse(lines[2].Split('=')[1]) : 30;
                    bool ac = lines.Length > 3 ? bool.Parse(lines[3].Split('=')[1]) : false;
                    return (en, th, inv, ac);
                }
            }
            catch { }
            return (false, 1, 30, false);
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
                        var logs = (List<AnalysisLog>)ser.ReadObject(ms);

                        // --- LÓGICA DE LIMPEZA AUTOMÁTICA (> 30 DIAS) ---
                        // Remove registros onde a data é anterior a 1 mês atrás
                        int removedCount = logs.RemoveAll(l =>
                        {
                            if (DateTime.TryParse(l.Date, out DateTime logDate))
                            {
                                return logDate < DateTime.Now.AddMonths(-1);
                            }
                            return true; // Se a data estiver corrompida, remove também
                        });

                        // Se removeu algo, salva o arquivo atualizado imediatamente
                        if (removedCount > 0)
                        {
                            SaveHistory(logs);
                        }

                        return logs;
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