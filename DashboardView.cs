using AtronSuite;

namespace AtronSuiteApp
{
    // --- VIEW 1: DASHBOARD (RESPONSIVA) ---
    public class DashboardView : UserControl
    {
        private MainForm _parent;

        // Controles promovidos a campos da classe para reajuste
        private Panel pnlStatus;
        private Label lblWelcome;
        private Button btnCleaner, btnUpdater, btnDrivers, btnSentinel;

        public DashboardView(MainForm parent)
        {
            _parent = parent;
            BackColor = Theme.BackDark;
            Dock = DockStyle.Fill; // Garante que o UC preencha o container
            InitializeUI();

            // Evento vital para responsividade
            Resize += (s, e) => RecalculateLayout();
        }

        private void InitializeUI()
        {
            // 1. Status do Sentinel
            pnlStatus = new Panel { Size = new Size(700, 40), BackColor = Color.Transparent };
            string statusText = SentinelEngine.IsRunning ? "ATIVO" : "PARADO";
            Color statusColor = SentinelEngine.IsRunning ? Theme.Success : Theme.TextMuted;
            string lastRun = SentinelEngine.LastRunTime.HasValue ? SentinelEngine.LastRunTime.Value.ToString("g") : "Nunca";

            Label lblStatus = new Label { Text = $"STATUS SENTINEL: {statusText}", ForeColor = statusColor, Font = Theme.FontBold, AutoSize = true, Location = new Point(0, 10) };
            Label lblLastRun = new Label { Text = $"ÚLTIMA ANÁLISE: {lastRun}", ForeColor = Theme.TextMuted, Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(400, 12) };
            pnlStatus.Controls.Add(lblStatus);
            pnlStatus.Controls.Add(lblLastRun);
            Controls.Add(pnlStatus);

            // 2. Título
            lblWelcome = new Label { Text = "Painel de Controle", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Theme.TextMain, AutoSize = true };
            Controls.Add(lblWelcome);

            // 3. Criação dos Botões
            btnCleaner = CreateAnimatedButton("🧹", "Cleaner Monitor", "Limpeza de Disco", Theme.CleanerColor, () => _parent.NavigateTo(new CleanerView()));
            btnUpdater = CreateAnimatedButton("🔄", "Intelligent Updater", "Atualizar Apps", Theme.UpdaterColor, () => _parent.NavigateTo(new UpdaterView()));
            btnDrivers = CreateAnimatedButton("🚀", "Driver Engine", "Otimizar Hardware", Theme.DriverColor, () => _parent.NavigateTo(new DriverUpdaterView()));
            btnSentinel = CreateAnimatedButton("🛡️", "Sentinel Patrol", "Automação & Logs", Theme.SentinelColor, () => _parent.NavigateTo(new SentinelView()));

            Controls.Add(btnCleaner);
            Controls.Add(btnUpdater);
            Controls.Add(btnDrivers);
            Controls.Add(btnSentinel);

            // Primeira passada de layout
            RecalculateLayout();
        }

        // Método mágico da responsividade
        private void RecalculateLayout()
        {
            if (Width == 0) return; // Evita erro na inicialização

            int centerX = ClientSize.Width / 2; // Usa a largura real da área cliente

            // Reposiciona Status
            pnlStatus.Location = new Point(centerX - pnlStatus.Width / 2, 20);

            // Reposiciona Título
            lblWelcome.Location = new Point(centerX - lblWelcome.Width / 2, 80);

            // Grid 2x2
            int btnW = 260;
            int btnH = 180;
            int gap = 30;

            int gridWidth = btnW * 2 + gap; // Largura total do bloco de botões

            int startX = centerX - gridWidth / 2;
            int startY = 170;

            // Aplica novas coordenadas
            if (btnCleaner != null) btnCleaner.Location = new Point(startX, startY);
            if (btnUpdater != null) btnUpdater.Location = new Point(startX + btnW + gap, startY);
            if (btnDrivers != null) btnDrivers.Location = new Point(startX, startY + btnH + gap);
            if (btnSentinel != null) btnSentinel.Location = new Point(startX + btnW + gap, startY + btnH + gap);
        }

        private Button CreateAnimatedButton(string icon, string title, string desc, Color accentColor, Action onClick)
        {
            Button btn = new Button();
            btn.Size = new Size(260, 180);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Theme.BackLight;
            btn.Cursor = Cursors.Hand;

            btn.Click += (s, e) => onClick();

            // Hook para animação que preserva a posição dinâmica
            btn.MouseEnter += (s, e) => {
                btn.Top -= 5;
                btn.BackColor = Color.FromArgb(50, 53, 58);
            };
            btn.MouseLeave += (s, e) => {
                btn.Top += 5;
                btn.BackColor = Theme.BackLight;
            };

            btn.Paint += (s, e) =>
            {
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                using (Font fIcon = new Font("Segoe UI Emoji", 40))
                    e.Graphics.DrawString(icon, fIcon, Brushes.White, new Rectangle(0, 20, 260, 60), new StringFormat { Alignment = StringAlignment.Center });
                using (Font fTitle = new Font("Segoe UI", 12, FontStyle.Bold))
                    e.Graphics.DrawString(title, fTitle, new SolidBrush(accentColor), new Rectangle(0, 90, 260, 30), new StringFormat { Alignment = StringAlignment.Center });
                using (Font fDesc = new Font("Segoe UI", 9))
                    e.Graphics.DrawString(desc, fDesc, Brushes.Gray, new Rectangle(10, 125, 240, 40), new StringFormat { Alignment = StringAlignment.Center });
            };
            return btn;
        }
    }
}