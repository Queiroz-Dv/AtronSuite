using AtronSuite;
using System.Runtime.InteropServices;

namespace AtronSuiteApp
{
    public class CleanerView : UserControl
    {
        private Label lblWinTempSize, lblPrefetchSize, lblUserTempSize, lblRecycleStatus;
        private TextBox txtLog;
        private ProgressBar pbLoading;
        private Button btnAnalisar, btnLimpar, btnTrash, btnAnalyzeTrash;

        private readonly string pathWinTemp = @"C:\Windows\Temp";
        private readonly string pathPrefetch = @"C:\Windows\Prefetch";
        private readonly string pathUserTemp = Path.GetTempPath();

        [DllImport("shell32.dll")]
        static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct SHQUERYRBINFO
        {
            public int cbSize;
            public long i64Size;
            public long i64NumItems;
        }

        public CleanerView()
        {
            this.BackColor = Theme.BackDark;
            InitializeUI();
        }

        private void InitializeUI()
        {
            int y = 30; int x = 90;

            CreateCard(x, y, "Windows Temp", pathWinTemp, out lblWinTempSize); y += 90;
            CreateCard(x, y, "Prefetch", pathPrefetch, out lblPrefetchSize); y += 90;
            CreateCard(x, y, "User Temp", pathUserTemp, out lblUserTempSize); y += 90;

            btnAnalisar = CreateButton("🔍 Analisar Pastas", x, y, Theme.Accent);
            btnAnalisar.Click += async (s, e) => await RunAnalysis();
            this.Controls.Add(btnAnalisar);

            btnLimpar = CreateButton("🧹 Limpar Sistema", x + 160, y, Theme.CleanerColor);
            btnLimpar.Click += (s, e) => RunCleaning();
            this.Controls.Add(btnLimpar);

            pbLoading = new ProgressBar
            {
                Location = new Point(x + 320, y + 12),
                Size = new Size(300, 20),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false
            };
            this.Controls.Add(pbLoading);

            y += 60;

            Panel pnlTrash = new Panel { BackColor = Theme.BackLight, Size = new Size(620, 50), Location = new Point(x, y) };
            lblRecycleStatus = new Label { Text = "Lixeira: (Aguardando)", ForeColor = Color.Gray, Location = new Point(15, 15), AutoSize = true };
            pnlTrash.Controls.Add(lblRecycleStatus);

            btnAnalyzeTrash = new Button { Text = "Analisar", Size = new Size(100, 30), Location = new Point(390, 10), FlatStyle = FlatStyle.Flat, BackColor = Theme.BackDark, ForeColor = Theme.Accent, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnAnalyzeTrash.FlatAppearance.BorderSize = 0;
            btnAnalyzeTrash.Click += async (s, e) => await AnalyzeRecycleBin();
            pnlTrash.Controls.Add(btnAnalyzeTrash);

            btnTrash = new Button { Text = "Esvaziar", BackColor = Theme.CleanerColor, FlatStyle = FlatStyle.Flat, Size = new Size(100, 30), Location = new Point(500, 10), ForeColor = Color.White, Font = Theme.FontBold };
            btnTrash.Click += (s, e) => EmptyTrash();
            pnlTrash.Controls.Add(btnTrash);
            this.Controls.Add(pnlTrash);

            y += 70;

            txtLog = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = Color.Black, ForeColor = Theme.Success, Font = new Font("Consolas", 9), Location = new Point(x, y), Size = new Size(620, 150), BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(txtLog);
        }

        private void CreateCard(int x, int y, string title, string path, out Label lblSize)
        {
            Panel p = new Panel { BackColor = Theme.BackLight, Size = new Size(620, 80), Location = new Point(x, y) };
            p.Controls.Add(new Label { Text = title, Font = Theme.FontBold, ForeColor = Theme.TextMain, Location = new Point(15, 15), AutoSize = true });
            p.Controls.Add(new Label { Text = path, Font = new Font("Segoe UI", 8), ForeColor = Theme.TextMuted, Location = new Point(15, 40), AutoSize = true });
            lblSize = new Label { Text = "---", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Theme.Accent, Location = new Point(450, 20), AutoSize = false, Size = new Size(150, 40), TextAlign = ContentAlignment.MiddleRight };
            p.Controls.Add(lblSize);
            this.Controls.Add(p);
        }

        private Button CreateButton(string text, int x, int y, Color bg)
        {
            return new Button { Text = text, Location = new Point(x, y), Size = new Size(150, 45), FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = Color.White, Font = Theme.FontBold, Cursor = Cursors.Hand };
        }

        private async Task RunAnalysis()
        {
            ToggleLoading(true);
            Log("Iniciando análise APENAS das pastas...");

            var result = await Task.Run(() =>
            {
                long s1 = GetSize(pathWinTemp);
                long s2 = GetSize(pathPrefetch);
                long s3 = GetSize(pathUserTemp);
                return (s1, s2, s3);
            });

            lblWinTempSize.Text = SentinelEngine.FormatBytes(result.s1);
            lblPrefetchSize.Text = SentinelEngine.FormatBytes(result.s2);
            lblUserTempSize.Text = SentinelEngine.FormatBytes(result.s3);

            long totalFolders = result.s1 + result.s2 + result.s3;

            // --- LOG PADRONIZADO: Análise Manual ---
            SentinelEngine.RegisterLog(SentinelEngine.FormatBytes(totalFolders), "Análise Manual");

            Log($"Pastas analisadas: {SentinelEngine.FormatBytes(totalFolders)} encontrados.");
            ToggleLoading(false);
        }

        private async Task AnalyzeRecycleBin()
        {
            Log("Verificando lixeira...");
            var stats = await Task.Run(() => GetRecycleBinStats());

            if (stats.Items >= 0)
            {
                string sizeStr = SentinelEngine.FormatBytes(stats.Size);
                lblRecycleStatus.Text = $"Lixeira: {sizeStr} ({stats.Items} itens)";
                lblRecycleStatus.ForeColor = Theme.TextMain;
                Log($"Lixeira analisada: {sizeStr} em {stats.Items} itens.");
            }
            else
            {
                lblRecycleStatus.Text = "Lixeira: Erro";
                Log("Erro ao ler Lixeira.");
            }
        }

        private (long Size, long Items) GetRecycleBinStats()
        {
            try
            {
                SHQUERYRBINFO info = new SHQUERYRBINFO();
                info.cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO));
                int result = SHQueryRecycleBin(string.Empty, ref info);
                if (result == 0) return (info.i64Size, info.i64NumItems);
            }
            catch { }
            return (0, -1);
        }

        private async void RunCleaning()
        {
            if (MessageBox.Show("Excluir arquivos temporários das pastas?", "Limpeza", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ToggleLoading(true);
                Log("Limpando pastas...");

                long cleaned = await Task.Run(() =>
                {
                    long c = 0;
                    c += CleanDir(pathWinTemp);
                    c += CleanDir(pathPrefetch);
                    c += CleanDir(pathUserTemp);
                    return c;
                });

                Log("Concluído.");

                // --- LOG PADRONIZADO: Limpeza Manual ---
                SentinelEngine.RegisterLog(SentinelEngine.FormatBytes(cleaned), "Limpeza Manual");

                ToggleLoading(false);
                await RunAnalysis();
            }
        }

        private void EmptyTrash()
        {
            try { SHEmptyRecycleBin(IntPtr.Zero, null, 0); Log("Comando esvaziar enviado."); _ = AnalyzeRecycleBin(); }
            catch (Exception ex) { Log($"Erro Lixeira: {ex.Message}"); }
        }

        private long CleanDir(string path)
        {
            if (!Directory.Exists(path)) return 0;
            long freed = 0;
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (var f in di.GetFiles()) { try { long l = f.Length; f.Delete(); freed += l; } catch { } }
            foreach (var d in di.GetDirectories()) { try { d.Delete(true); } catch { } }
            return freed;
        }

        private long GetSize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            long s = 0;
            try { foreach (var f in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) s += new FileInfo(f).Length; } catch { }
            return s;
        }

        private void ToggleLoading(bool isLoading)
        {
            pbLoading.Visible = isLoading;
            btnAnalisar.Enabled = !isLoading;
            btnLimpar.Enabled = !isLoading;
            btnAnalyzeTrash.Enabled = !isLoading;
            Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
        }

        private void Log(string m) => txtLog.AppendText($"[{DateTime.Now:T}] {m}\r\n");
    }
}