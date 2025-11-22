namespace AtronSuite
{
    // --- SENTINEL ENGINE ---
    public class SentinelEngine
    {
        private static CancellationTokenSource _cts;
        private static NotifyIcon _notifyIcon;
        public static DateTime? LastRunTime { get; private set; } = null;

        private static readonly string[] PathsToCheck = { @"C:\Windows\Temp", @"C:\Windows\Prefetch", Path.GetTempPath() };

        public static void Initialize(NotifyIcon icon)
        {
            _notifyIcon = icon;
            var (Enabled, Threshold, Interval) = DataManager.LoadSettings();
            var hist = DataManager.LoadHistory();
            if (hist.Count > 0) LastRunTime = DateTime.Parse(hist.Last().Date);
            if (Enabled) Start(Threshold, Interval);
        }

        public static void Start(int thresholdGB, int intervalMinutes)
        {
            Stop();
            _cts = new CancellationTokenSource();
            DataManager.SaveSettings(true, thresholdGB, intervalMinutes);
            Task.Run(async () => await PatrolLoop(thresholdGB, intervalMinutes, _cts.Token));
        }

        public static void Stop()
        {
            _cts?.Cancel();
            var current = DataManager.LoadSettings();
            DataManager.SaveSettings(false, current.Threshold, current.Interval);
        }

        public static bool IsRunning => _cts != null && !_cts.IsCancellationRequested;

        public static void RegisterLog(long totalBytes, string triggerType)
        {
            LastRunTime = DateTime.Now;
            var log = new AnalysisLog
            {
                Date = DateTime.Now.ToString("g"),
                SizeFound = FormatBytes(totalBytes),
                TriggerType = triggerType
            };
            var hist = DataManager.LoadHistory();
            hist.Add(log);
            DataManager.SaveHistory(hist);
        }

        private static async Task PatrolLoop(int thresholdGB, int intervalMinutes, CancellationToken token)
        {
            long limitBytes = (long)thresholdGB * 1024 * 1024 * 1024;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    ShowNotification("Patrulha Iniciada", "Verificando integridade das pastas...", ToolTipIcon.Info, 1000);
                    await Task.Delay(2000, token);

                    long totalSize = 0;
                    foreach (var p in PathsToCheck) totalSize += GetDirSize(p);

                    RegisterLog(totalSize, "Automático");

                    if (totalSize > limitBytes)
                    {
                        ShowNotification("Limite Excedido!", $"Encontrado: {FormatBytes(totalSize)} (Limite: {thresholdGB}GB).", ToolTipIcon.Warning, 5000);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), token);
                }
                catch (TaskCanceledException) { break; }
                catch { await Task.Delay(TimeSpan.FromMinutes(5), token); }
            }
        }

        public static async Task RunManualScan()
        {
            long totalSize = 0;
            foreach (var p in PathsToCheck) totalSize += GetDirSize(p);
            RegisterLog(totalSize, "Manual (Sentinel)");
        }

        private static void ShowNotification(string title, string msg, ToolTipIcon icon, int duration)
        {
            if (_notifyIcon == null) return;
            _notifyIcon.ShowBalloonTip(duration, "🛡️ Atron Sentinel", $"{title}\n{msg}", icon);
        }

        private static long GetDirSize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            long size = 0;
            try { foreach (var f in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)) size += new FileInfo(f).Length; } catch { }
            return size;
        }

        public static string FormatBytes(long b)
        {
            string[] s = { "B", "KB", "MB", "GB" };
            double l = b; int o = 0;
            while (l >= 1024 && o < s.Length - 1) { o++; l /= 1024; }
            return $"{l:0.##} {s[o]}";
        }
    }
}