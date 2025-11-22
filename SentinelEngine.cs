using AtronSuite;

namespace AtronSuiteApp
{
    public class SentinelEngine
    {
        private static CancellationTokenSource _cts;
        private static NotifyIcon _notifyIcon;
        public static DateTime? LastRunTime { get; private set; } = null;
        private static bool _autoClean = false;

        private static readonly string[] PathsToCheck = { @"C:\Windows\Temp", @"C:\Windows\Prefetch", Path.GetTempPath() };

        public static void Initialize(NotifyIcon icon)
        {
            _notifyIcon = icon;
            var (Enabled, Threshold, Interval, AutoClean) = DataManager.LoadSettings();
            var hist = DataManager.LoadHistory();
            if (hist.Count > 0) LastRunTime = DateTime.Parse(hist.Last().Date);
            if (Enabled) Start(Threshold, Interval, AutoClean);
        }

        public static void Start(int thresholdGB, int intervalMinutes, bool autoClean)
        {
            Stop();
            _cts = new CancellationTokenSource();
            _autoClean = autoClean;
            DataManager.SaveSettings(true, thresholdGB, intervalMinutes, autoClean);
            Task.Run(async () => await PatrolLoop(thresholdGB, intervalMinutes, _cts.Token));
        }

        public static void Stop()
        {
            _cts?.Cancel();
            var current = DataManager.LoadSettings();
            DataManager.SaveSettings(false, current.Threshold, current.Interval, current.AutoClean);
        }

        public static bool IsRunning => _cts != null && !_cts.IsCancellationRequested;

        public static void RegisterLog(string sizeOrStatus, string triggerType)
        {
            LastRunTime = DateTime.Now;
            var log = new AnalysisLog
            {
                Date = DateTime.Now.ToString("g"),
                SizeFound = sizeOrStatus,
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
                    await Task.Delay(2000, token); // Aguarda inicialização

                    long totalSize = 0;
                    foreach (var p in PathsToCheck) totalSize += GetDirSize(p);

                    string sizeStr = FormatBytes(totalSize);

                    if (totalSize > limitBytes)
                    {
                        if (_autoClean)
                        {
                            // --- MODO LIMPEZA AUTOMÁTICA ---
                            ShowNotification("Limite Atingido", $"Iniciando limpeza automática de {sizeStr}...", ToolTipIcon.Info, 2000);

                            long cleaned = 0;
                            foreach (var p in PathsToCheck) cleaned += CleanDir(p);

                            // LOG: Limpeza Automática (como solicitado)
                            RegisterLog($"Limpo: {FormatBytes(cleaned)}", "Limpeza Automática");

                            ShowNotification("Concluído", "Limpeza automática realizada.", ToolTipIcon.Info, 3000);
                        }
                        else
                        {
                            // Limite atingido mas sem auto-clean
                            RegisterLog(sizeStr, "Alerta: Limite");
                            ShowNotification("Limite Excedido!", $"Encontrado: {sizeStr} (Limite: {thresholdGB}GB).", ToolTipIcon.Warning, 5000);
                        }
                    }
                    else
                    {
                        // --- MODO MONITORAMENTO (Sentinel Ativado) ---
                        // LOG: Análise Agendada (como solicitado para Sentinel Ativado)
                        RegisterLog(sizeStr, "Análise Agendada");
                    }

                    await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), token);
                }
                catch (TaskCanceledException) { break; }
                catch { await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), token); }
            }
        }

        private static long CleanDir(string path)
        {
            // Verifica se a pasta existe antes de tentar qualquer coisa
            if (!Directory.Exists(path)) return 0;

            long freed = 0;
            DirectoryInfo di = new DirectoryInfo(path);

            // 1. Tenta listar e deletar ARQUIVOS
            try
            {
                foreach (var f in di.GetFiles())
                {
                    try
                    {
                        long len = f.Length;
                        f.Delete(); // Tenta deletar o arquivo
                        freed += len; // Se deletou, soma o tamanho
                    }
                    catch
                    {
                        // Arquivo em uso ou sem permissão: Simplesmente ignora e vai para o próximo
                        // Isso impede que o loop quebre
                    }
                }
            }
            catch
            {
                // Erro de acesso à pasta principal (sem permissão de leitura): aborta esta pasta
                return freed;
            }

            // 2. Tenta listar e processar SUBPASTAS (Recursividade)
            try
            {
                foreach (var d in di.GetDirectories())
                {
                    try
                    {
                        // IMPORTANTE: Chama CleanDir recursivamente para entrar na subpasta
                        // e limpar os arquivos de lá ANTES de tentar deletar a pasta
                        freed += CleanDir(d.FullName);

                        // Tenta deletar a pasta vazia (false = não recursivo, pois já limpamos dentro)
                        d.Delete(false);
                    }
                    catch
                    {
                        // Se a pasta ainda tiver arquivos (pq estavam bloqueados), 
                        // o Delete(false) vai falhar. Ignoramos e deixamos a pasta lá.
                    }
                }
            }
            catch
            {
                // Erro ao listar subdiretórios
            }

            return freed;
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
            try { foreach (var f in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) size += new FileInfo(f).Length; } catch { }
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