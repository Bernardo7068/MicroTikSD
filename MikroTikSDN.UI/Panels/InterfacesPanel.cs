using System;
using System.Drawing;
using System.Windows.Forms;
using MikroTikSDN.Core;

namespace MikroTikSDN.UI.Panels
{
    public class InterfacesPanel : UserControl
    {
        private readonly RouterManager _router;
        private DataGridView grid;
        private Label lblCount;

        public InterfacesPanel(RouterManager router)
        {
            _router = router;
            BuildUI();
            _ = LoadAsync();
        }

        private void BuildUI()
        {
            var bgDark   = Color.FromArgb(30,  30,  46);
            var bgPanel  = Color.FromArgb(37,  37,  55);
            var textPrim = Color.FromArgb(232, 232, 240);
            var textMuted= Color.FromArgb(136, 136, 153);

            this.BackColor = bgDark;
            this.Dock      = DockStyle.Fill;

            // Toolbar
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = bgDark };

            lblCount = new Label { Text = "", ForeColor = textMuted, Font = new Font("Segoe UI", 9f), Location = new Point(0, 10), AutoSize = true };

            var btnRefresh = MakeButton("↻  Atualizar", Color.FromArgb(58, 58, 85));
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Location = new Point(this.Width - 120, 4);
            btnRefresh.Click += (s, e) => _ = LoadAsync();

            toolbar.Controls.AddRange(new Control[] { lblCount, btnRefresh });

            // Grid
            grid = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                BackgroundColor       = bgPanel,
                ForeColor             = textPrim,
                GridColor             = Color.FromArgb(46, 46, 69),
                BorderStyle           = BorderStyle.None,
                ColumnHeadersHeight   = 36,
                RowHeadersVisible     = false,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                Font                  = new Font("Segoe UI", 9.5f)
            };

            // Estilo header
            grid.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(30, 30, 46);
            grid.ColumnHeadersDefaultCellStyle.ForeColor  = textMuted;
            grid.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            grid.ColumnHeadersBorderStyle                  = DataGridViewHeaderBorderStyle.None;

            // Estilo células
            grid.DefaultCellStyle.BackColor          = bgPanel;
            grid.DefaultCellStyle.ForeColor          = textPrim;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(58, 58, 85);
            grid.DefaultCellStyle.SelectionForeColor = textPrim;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(42, 42, 62);

            // Colunas
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Estado",     HeaderText = "Estado",      Width = 70,  FillWeight = 10 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nome",       HeaderText = "Nome",        FillWeight = 20 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tipo",       HeaderText = "Tipo",        FillWeight = 15 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "MAC",        HeaderText = "MAC Address", FillWeight = 20 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "MTU",        HeaderText = "MTU",         FillWeight = 8  });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Comentario", HeaderText = "Comentário",  FillWeight = 27 });

            this.Controls.Add(grid);
            this.Controls.Add(toolbar);
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                grid.Rows.Clear();
                var interfaces = await _router.Interfaces.GetAllAsync();
                foreach (var iface in interfaces)
                {
                    string estado = iface.Disabled ? "❌ Desativ." : iface.Running ? "✅ Ativo" : "⬜ Parado";
                    grid.Rows.Add(estado, iface.Name, iface.Type, iface.MacAddress, iface.Mtu, iface.Comment);
                }
                lblCount.Text = $"{interfaces.Count} interfaces";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar interfaces:\n{ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Button MakeButton(string text, Color color)
        {
            var btn = new Button
            {
                Text      = text,
                Size      = new Size(110, 30),
                BackColor = color,
                ForeColor = Color.FromArgb(232, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Font      = new Font("Segoe UI", 9f)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
