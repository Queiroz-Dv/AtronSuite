using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AtronSuite
{
    // --- ENTRY POINT ---
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    // --- JANELA PRINCIPAL ---
    public class MainForm : Form
    {
        private Panel pnlHeader;
        private Label lblTitle;
        private Button btnHome;
        private Panel pnlContainer;
        private Panel pnlLoading;
        private NotifyIcon trayIcon;

        public MainForm()
        {
            this.Text = "Atron Suite";
            this.Size = new Size(800, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.BackDark;
            this.ForeColor = Theme.TextMain;           
            this.MaximizeBox = true;
            this.ShowIcon = true;

            SetupTrayIcon();
            SentinelEngine.Initialize(trayIcon);

            InitializeShell();
            ShowLoadingAndStart();
        }

        private async void ShowLoadingAndStart()
        {
            pnlLoading = new Panel { Dock = DockStyle.Fill, BackColor = Theme.BackDark };
            Label lblLoad = new Label { Text = "Inicializando Módulos...", Font = Theme.FontTitle, ForeColor = Theme.Accent, AutoSize = true };

            // Centralização manual para o loading inicial
            lblLoad.Location = new Point((this.Width - 200) / 2, (this.Height / 2) - 40);

            ProgressBar pb = new ProgressBar { Style = ProgressBarStyle.Marquee, Width = 300, Height = 10 };
            pb.Location = new Point((this.Width - 300) / 2, (this.Height / 2) + 10);

            pnlLoading.Controls.Add(lblLoad);
            pnlLoading.Controls.Add(pb);
            this.Controls.Add(pnlLoading);
            pnlLoading.BringToFront();

            await Task.Delay(2500);

            this.Controls.Remove(pnlLoading);
            NavigateTo(new DashboardView(this));
        }

        private void SetupTrayIcon()
        {
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Shield,
                Text = "Atron Sentinel (Ativo)",
                Visible = true
            };

            ContextMenuStrip contextMenu = new();
            contextMenu.Items.Add("Abrir Painel", null, (s, e) => RestoreWindow());
            contextMenu.Items.Add("Sair", null, (s, e) => { SentinelEngine.Stop(); Application.Exit(); });
            trayIcon.ContextMenuStrip = contextMenu;

            trayIcon.DoubleClick += (s, e) => RestoreWindow();
            trayIcon.BalloonTipClicked += (s, e) => { RestoreWindow(); };
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && SentinelEngine.IsRunning)
            {
                e.Cancel = true;
                this.Hide();
                trayIcon.ShowBalloonTip(2000, "Atron Sentinel", "Proteção ativa em segundo plano.", ToolTipIcon.Info);
            }
            base.OnFormClosing(e);
        }

        private void RestoreWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void InitializeShell()
        {
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(25, 25, 25) };
            this.Controls.Add(pnlHeader);

            pnlContainer = new Panel { Dock = DockStyle.Fill };
            this.Controls.Add(pnlContainer);
            pnlContainer.BringToFront();

            btnHome = new Button { Text = "🏠 Início", Size = new Size(80, 35), Location = new Point(10, 12), FlatStyle = FlatStyle.Flat, ForeColor = Theme.Accent, Font = Theme.FontBold, Cursor = Cursors.Hand, Visible = false };
            btnHome.FlatAppearance.BorderSize = 0;
            btnHome.Click += (s, e) => NavigateTo(new DashboardView(this));
            pnlHeader.Controls.Add(btnHome);

            lblTitle = new Label { Text = "Atron Suite", AutoSize = true, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(100, 15) };
            pnlHeader.Controls.Add(lblTitle);
        }

        public void NavigateTo(UserControl view)
        {
            pnlContainer.Controls.Clear();
            view.Dock = DockStyle.Fill;
            pnlContainer.Controls.Add(view);

            if (view is DashboardView)
            {
                btnHome.Visible = false;
                lblTitle.Text = "Atron Suite";
            }
            else
            {
                btnHome.Visible = true;
                if (view is CleanerView) lblTitle.Text = "Cleaner Monitor";
                if (view is UpdaterView) lblTitle.Text = "Intelligent Updater";
                if (view is DriverUpdaterView) lblTitle.Text = "Driver Engine";
                if (view is SentinelView) lblTitle.Text = "Sentinel Patrol";
            }
        }
    }
}