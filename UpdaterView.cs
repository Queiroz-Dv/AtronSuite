namespace AtronSuite
{
    // --- VIEW 3: INTELLIGENT UPDATER (Mantido) ---
    public class UpdaterView : UserControl
    {
        private DataGridView grid;
        private Button btnCheck, btnUpdateSel, btnUpdateAll;
        private Label lblStatus;

        public UpdaterView() { this.BackColor = Theme.BackDark; InitializeUI(); }

        private void InitializeUI()
        {
            int x = 90; int y = 30;

            btnCheck = new Button { Text = "🔄 Buscar Apps", Size = new Size(180, 45), Location = new Point(x, y), FlatStyle = FlatStyle.Flat, BackColor = Theme.UpdaterColor, ForeColor = Color.White, Font = Theme.FontBold, Cursor = Cursors.Hand };
            btnCheck.Click += BtnCheck_Click;
            this.Controls.Add(btnCheck);

            lblStatus = new Label { Text = "Clique para buscar...", ForeColor = Theme.TextMuted, Location = new Point(x + 200, y + 15), AutoSize = true };
            this.Controls.Add(lblStatus);

            y += 60;
            grid = CreateGrid(x, y);
            this.Controls.Add(grid);
            y += 315;

            btnUpdateSel = new Button { Text = "Atualizar Marcados", Size = new Size(200, 40), Location = new Point(x, y), FlatStyle = FlatStyle.Flat, BackColor = Theme.UpdaterColor, ForeColor = Color.White, Font = Theme.FontBold };
            btnUpdateSel.Click += (s, e) => RunUpdates(true);
            this.Controls.Add(btnUpdateSel);

            btnUpdateAll = new Button { Text = "Atualizar TODOS", Size = new Size(200, 40), Location = new Point(x + 210, y), FlatStyle = FlatStyle.Flat, BackColor = Theme.UpdaterColor, ForeColor = Color.White, Font = Theme.FontBold };
            btnUpdateAll.Click += (s, e) => RunUpdates(false);
            this.Controls.Add(btnUpdateAll);
        }

        private DataGridView CreateGrid(int x, int y)
        {
            var g = new DataGridView { Location = new Point(x, y), Size = new Size(620, 300), BackgroundColor = Theme.BackLight, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.Black, ForeColor = Color.White, Font = Theme.FontBold };
            g.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Theme.BackLight, ForeColor = Color.White, SelectionBackColor = Theme.Accent };
            g.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "✔", Width = 30, Name = "chk" });
            g.Columns.Add("name", "Nome"); g.Columns[1].Width = 250;
            g.Columns.Add("id", "ID"); g.Columns[2].Width = 200;
            g.Columns.Add("ver", "Versão"); g.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            return g;
        }

        private async void BtnCheck_Click(object s, EventArgs e)
        {
            btnCheck.Enabled = false; lblStatus.Text = "Buscando..."; grid.Rows.Clear();
            try
            {
                var updates = await Task.Run(() => WingetHelper.GetUpdates(false));
                lblStatus.Text = $"{updates.Count} apps encontrados.";
                foreach (var u in updates) grid.Rows.Add(false, u.Name, u.Id, u.Ver);
            }
            catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
            finally { btnCheck.Enabled = true; }
        }

        private async Task RunUpdates(bool selectedOnly)
        {
            foreach (DataGridViewRow r in grid.Rows)
            {
                if (!selectedOnly || Convert.ToBoolean(r.Cells["chk"].Value))
                {
                    await WingetHelper.RunUpgrade(r.Cells[2].Value.ToString());
                }
            }
            MessageBox.Show("Finalizado."); BtnCheck_Click(null, null);
        }
    }
}