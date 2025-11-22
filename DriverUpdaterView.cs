namespace AtronSuite
{
    // --- VIEW 4: DRIVER ENGINE (Mantido) ---
    public class DriverUpdaterView : UserControl
    {
        private DataGridView grid;
        private Button btnCheck, btnUpdateSel;
        private Label lblStatus;

        public DriverUpdaterView() { this.BackColor = Theme.BackDark; InitializeUI(); }

        private void InitializeUI()
        {
            int x = 90; int y = 30;

            btnCheck = new Button { Text = "🚀 Buscar Drivers", Size = new Size(180, 45), Location = new Point(x, y), FlatStyle = FlatStyle.Flat, BackColor = Theme.DriverColor, ForeColor = Color.White, Font = Theme.FontBold, Cursor = Cursors.Hand };
            btnCheck.Click += BtnCheck_Click;
            this.Controls.Add(btnCheck);

            lblStatus = new Label { Text = "Buscar drivers...", ForeColor = Theme.TextMuted, Location = new Point(x + 200, y + 15), AutoSize = true };
            this.Controls.Add(lblStatus);

            y += 60;
            grid = new DataGridView { Location = new Point(x, y), Size = new Size(620, 300), BackgroundColor = Theme.BackLight, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.Black, ForeColor = Color.White, Font = Theme.FontBold };
            grid.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Theme.BackLight, ForeColor = Color.White, SelectionBackColor = Theme.DriverColor }; // Roxo na seleção
            grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "✔", Width = 30, Name = "chk" });
            grid.Columns.Add("name", "Driver"); grid.Columns[1].Width = 250;
            grid.Columns.Add("id", "ID"); grid.Columns[2].Width = 200;
            grid.Columns.Add("ver", "Versão"); grid.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.Controls.Add(grid);

            y += 315;
            btnUpdateSel = new Button { Text = "Atualizar Drivers", Size = new Size(200, 40), Location = new Point(x, y), FlatStyle = FlatStyle.Flat, BackColor = Theme.DriverColor, ForeColor = Color.White, Font = Theme.FontBold };
            btnUpdateSel.Click += (s, e) => RunUpdates();
            this.Controls.Add(btnUpdateSel);
        }

        private async void BtnCheck_Click(object s, EventArgs e)
        {
            btnCheck.Enabled = false; lblStatus.Text = "Analisando..."; grid.Rows.Clear();
            try
            {
                var updates = await Task.Run(() => WingetHelper.GetUpdates(true));
                lblStatus.Text = $"{updates.Count} drivers.";
                foreach (var u in updates) grid.Rows.Add(false, u.Name, u.Id, u.Ver);
            }
            catch { }
            finally { btnCheck.Enabled = true; }
        }
        private async Task RunUpdates()
        {
            foreach (DataGridViewRow r in grid.Rows) if (Convert.ToBoolean(r.Cells["chk"].Value)) await WingetHelper.RunUpgrade(r.Cells[2].Value.ToString());
            MessageBox.Show("Drivers Atualizados.");
        }
    }
}