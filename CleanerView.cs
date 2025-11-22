using System.Runtime.InteropServices;

namespace AtronSuite
{
    // --- VIEW 2: CLEANER MONITOR (Mantido) ---
    public class CleanerView : UserControl
    {
        private Label lblWinTempSize, lblPrefetchSize, lblUserTempSize, lblRecycleStatus;
        private TextBox txtLog;
        private readonly string pathWinTemp = @"C:\Windows\Temp";
        private readonly string pathPrefetch = @"C:\Windows\Prefetch";
        private readonly string pathUserTemp = Path.GetTempPath();

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);

        public CleanerView() { this.BackColor = Theme.BackDark; InitializeUI(); }

        private void InitializeUI()
        {
            int y = 30; int x = 90;

            CreateCard(x, y, "Windows Temp", pathWinTemp, out lblWinTempSize); y += 90;
            CreateCard(x, y, "Prefetch", pathPrefetch, out lblPrefetchSize); y += 90;
            CreateCard(x, y, "User Temp", pathUserTemp, out lblUserTempSize); y += 90;

            Button btnAnalisar = CreateButton("🔍 Analisar", x, y, Theme.Accent);
            btnAnalisar.Click += (s, e) => RunAnalysis();
            this.Controls.Add(btnAnalisar);

            Button btnLimpar = CreateButton("🧹 Limpar Sistema", x + 160, y, Theme.CleanerColor);
            btnLimpar.Click += (s, e) => RunCleaning();
            this.Controls.Add(btnLimpar);

            y += 60;

            Panel pnlTrash = new Panel { BackColor = Theme.BackLight, Size = new Size(620, 50), Location = new Point(x, y) };
            lblRecycleStatus = new Label { Text = "Lixeira: ...", ForeColor = Color.Gray, Location = new Point(15, 15), AutoSize = true };
            pnlTrash.Controls.Add(lblRecycleStatus);

            Button btnTrash = new Button { Text = "Esvaziar", BackColor = Theme.CleanerColor, FlatStyle = FlatStyle.Flat, Size = new Size(100, 30), Location = new Point(500, 10), ForeColor = Color.White, Font = Theme.FontBold };
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

        private void RunAnalysis()
        {
            Log("Analisando...");
            long s1 = GetSize(pathWinTemp);
            long s2 = GetSize(pathPrefetch);
            long s3 = GetSize(pathUserTemp);
            long total = s1 + s2 + s3;

            lblWinTempSize.Text = SentinelEngine.FormatBytes(s1);
            lblPrefetchSize.Text = SentinelEngine.FormatBytes(s2);
            lblUserTempSize.Text = SentinelEngine.FormatBytes(s3);

            SentinelEngine.RegisterLog(total, "Manual (Cleaner)");
            Log("Análise completa. Dados enviados ao Sentinel.");
        }

        private void RunCleaning()
        {
            if (MessageBox.Show("Limpar arquivos?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Log("Limpando...");
                CleanDir(pathWinTemp); CleanDir(pathPrefetch); CleanDir(pathUserTemp);
                RunAnalysis();
            }
        }

        private void EmptyTrash() { try { SHEmptyRecycleBin(IntPtr.Zero, null, 0); Log("Lixeira esvaziada."); } catch { Log("Erro Lixeira."); } }

        private void CleanDir(string path)
        {
            if (!Directory.Exists(path)) return;
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (var f in di.GetFiles()) { try { f.Delete(); } catch { } }
            foreach (var d in di.GetDirectories()) { try { d.Delete(true); } catch { } }
        }

        private long GetSize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            long s = 0;
            try { foreach (var f in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)) s += new FileInfo(f).Length; } catch { }
            return s;
        }

        private void Log(string m) => txtLog.AppendText($"[{DateTime.Now:T}] {m}\r\n");
    }
}