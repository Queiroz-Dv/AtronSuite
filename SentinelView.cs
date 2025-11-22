namespace AtronSuite
{
    // --- VIEW 5: SENTINEL PATROL (Mantido) ---
    public class SentinelView : UserControl
    {
        private ComboBox cmbLimit, cmbInterval;
        private Button btnToggle, btnClearHistory;
        private DataGridView gridHistory;
        private Label lblStatus;
        private bool isInitializing = true;

        public SentinelView() { this.BackColor = Theme.BackDark; InitializeUI(); }

        private void InitializeUI()
        {
            int x = 90; int y = 30;

            Panel pnlConfig = new Panel { BackColor = Theme.BackLight, Size = new Size(620, 80), Location = new Point(x, y) };

            Label lblLim = new Label { Text = "Limite:", ForeColor = Theme.TextMuted, Location = new Point(20, 28), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            pnlConfig.Controls.Add(lblLim);

            cmbLimit = new ComboBox { Location = new Point(85, 26), Width = 80, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Theme.BackDark, ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
            cmbLimit.Items.AddRange(new object[] { "1 GB", "2 GB", "5 GB", "10 GB" });
            pnlConfig.Controls.Add(cmbLimit);

            Label lblInt = new Label { Text = "Intervalo:", ForeColor = Theme.TextMuted, Location = new Point(190, 28), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            pnlConfig.Controls.Add(lblInt);

            cmbInterval = new ComboBox { Location = new Point(275, 26), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Theme.BackDark, ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
            cmbInterval.Items.AddRange(new object[] { "1 min", "15 min", "30 min", "1 Hora", "2 Horas" });
            pnlConfig.Controls.Add(cmbInterval);

            btnToggle = new Button { Size = new Size(120, 40), Location = new Point(400, 20), FlatStyle = FlatStyle.Flat, Font = Theme.FontBold };
            btnToggle.Click += BtnToggle_Click;
            pnlConfig.Controls.Add(btnToggle);

            lblStatus = new Label { Location = new Point(540, 30), AutoSize = true, Font = new Font("Segoe UI", 9) };
            pnlConfig.Controls.Add(lblStatus);

            this.Controls.Add(pnlConfig);
            y += 100;

            Label lblHist = new Label { Text = "Histórico de Patrulha", Font = Theme.FontBold, ForeColor = Theme.Accent, Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblHist);

            btnClearHistory = new Button { Text = "Limpar Histórico", Size = new Size(120, 25), Location = new Point(x + 500, y - 5), FlatStyle = FlatStyle.Flat, BackColor = Theme.BackLight, ForeColor = Color.White, Font = new Font("Segoe UI", 8) };
            btnClearHistory.Click += (s, e) => { DataManager.DeleteHistory(); LoadGridData(); };
            this.Controls.Add(btnClearHistory);

            y += 30;

            gridHistory = new DataGridView { Location = new Point(x, y), Size = new Size(620, 250), BackgroundColor = Theme.BackLight, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, RowHeadersVisible = false, ReadOnly = true };
            gridHistory.EnableHeadersVisualStyles = false;
            gridHistory.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.Black, ForeColor = Color.White, Font = Theme.FontBold };
            gridHistory.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Theme.BackLight, ForeColor = Color.White, SelectionBackColor = Theme.SentinelColor };

            gridHistory.Columns.Add("date", "Data/Hora"); gridHistory.Columns[0].Width = 180;
            gridHistory.Columns.Add("size", "Dados Encontrados"); gridHistory.Columns[1].Width = 150;
            gridHistory.Columns.Add("type", "Disparo"); gridHistory.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.Controls.Add(gridHistory);

            var s = DataManager.LoadSettings();
            cmbLimit.SelectedItem = $"{s.Threshold} GB";

            string invTxt = "30 min";
            if (s.Interval == 1) invTxt = "1 min";
            else if (s.Interval == 15) invTxt = "15 min";
            else if (s.Interval == 60) invTxt = "1 Hora";
            else if (s.Interval == 120) invTxt = "2 Horas";
            cmbInterval.SelectedItem = invTxt;

            UpdateStatusUI(s.Enabled);
            LoadGridData();

            isInitializing = false;
            cmbLimit.SelectedIndexChanged += OnConfigChanged;
            cmbInterval.SelectedIndexChanged += OnConfigChanged;
        }

        private void OnConfigChanged(object sender, EventArgs e)
        {
            if (isInitializing) return;
            int limit = ParseLimit();
            int interval = ParseInterval();
            if (SentinelEngine.IsRunning) SentinelEngine.Start(limit, interval);
            else DataManager.SaveSettings(false, limit, interval);
        }

        private int ParseLimit() => int.Parse(cmbLimit.SelectedItem.ToString().Split(' ')[0]);

        private int ParseInterval()
        {
            string sel = cmbInterval.SelectedItem.ToString();
            if (sel == "1 min") return 1;
            if (sel == "15 min") return 15;
            if (sel == "1 Hora") return 60;
            if (sel == "2 Horas") return 120;
            return 30;
        }

        private void LoadGridData()
        {
            gridHistory.Rows.Clear();
            var logs = DataManager.LoadHistory();
            logs.Reverse();
            foreach (var l in logs) gridHistory.Rows.Add(l.Date, l.SizeFound, l.TriggerType);
        }

        private void BtnToggle_Click(object sender, EventArgs e)
        {
            if (SentinelEngine.IsRunning)
            {
                SentinelEngine.Stop();
                UpdateStatusUI(false);
            }
            else
            {
                SentinelEngine.Start(ParseLimit(), ParseInterval());
                UpdateStatusUI(true);
                MessageBox.Show("Sentinela Ativado!\nEle rodará em segundo plano.", "AtronSuite", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateStatusUI(bool active)
        {
            if (active)
            {
                btnToggle.Text = "DESATIVAR";
                btnToggle.BackColor = Theme.CleanerColor;
                btnToggle.ForeColor = Color.White;
                lblStatus.Text = "Monitorando...";
                lblStatus.ForeColor = Theme.Success;
            }
            else
            {
                btnToggle.Text = "ATIVAR";
                btnToggle.BackColor = Theme.Success;
                btnToggle.ForeColor = Color.White;
                lblStatus.Text = "Parado";
                lblStatus.ForeColor = Theme.TextMuted;
            }
        }
    }
}