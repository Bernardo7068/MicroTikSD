using System.Drawing;
using System.Windows.Forms;

namespace MikroTikSDN.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlSidebar, pnlHeader, pnlContent, pnlNav, pnlStatus, pnlToolbar;
        private System.Windows.Forms.Label lblAppTitle, lblSectionTitle, lblRouterInfo, lblStatus;
        private System.Windows.Forms.DataGridView _dgvData;
        private System.Windows.Forms.ComboBox _cmbRouters;
        private System.Windows.Forms.Button _btnAddNewRouter, _btnLogout;

        // Botões da toolbar de ações
        private System.Windows.Forms.Button _btnAdd, _btnDelete, _btnRefresh, _btnAction, _btnToggle, _btnExport, _btnGoToPools;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            var bgDark = Color.FromArgb(30, 30, 46);
            var bgPanel = Color.FromArgb(37, 37, 55);
            var bgItem = Color.FromArgb(58, 58, 85);
            var accent = Color.FromArgb(124, 106, 247);
            var danger = Color.FromArgb(224, 108, 117);
            var success = Color.FromArgb(76, 175, 130);

            // ── Janela ────────────────────────────────────────────────────────
            this.Size = new Size(1250, 800);
            this.MinimumSize = new Size(900, 600);
            this.BackColor = bgDark;
            this.Text = "MikroTik SDN Controller";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5f);

            // ── Sidebar ───────────────────────────────────────────────────────
            pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = bgPanel };

            lblAppTitle = new Label
            {
                Text = "🌐 SDN Router",
                ForeColor = accent,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Location = new Point(22, 20),
                AutoSize = true
            };

            // Linha separadora
            var sep = new Label
            {
                Location = new Point(0, 70),
                Size = new Size(230, 1),
                BackColor = bgItem
            };

            pnlNav = new Panel
            {
                Location = new Point(0, 80),
                Size = new Size(230, 600),
                BackColor = bgPanel
            };

            // Itens de navegação: (texto, tag)
            string[,] navItems = {
                { "📡  Interfaces",      "iface"    },
                { "📶  Wireless",        "wifi"     },
                { "🌉  Bridges",         "bridge"   },
                { "🏷️  Endereços IP",   "ip"       },
                { "🛤️  Rotas Estáticas","route"    },
                { "🧬  Servidor DHCP",  "dhcp"     },
                { "💧  IP Pools",       "pool"     },
                { "🌐  Configurar DNS", "dns"      },
                { "🛡️  WireGuard VPN",  "wg"       }
            };

            for (int i = 0; i < navItems.GetLength(0); i++)
            {
                var btn = new Button
                {
                    Text = navItems[i, 0],
                    Tag = navItems[i, 1],
                    Dock = DockStyle.Top,
                    Height = 50,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.Silver,
                    BackColor = bgPanel,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(20, 0, 0, 0),
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 9.5f)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = bgItem;
                btn.Click += NavButton_Click;
                pnlNav.Controls.Add(btn);
                btn.BringToFront();
            }

            pnlSidebar.Controls.AddRange(new Control[] { lblAppTitle, sep, pnlNav });

            // ── Header ────────────────────────────────────────────────────────
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 65, Width = 1020, BackColor = bgPanel };

            lblSectionTitle = new Label
            {
                Text = "Dashboard",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16f),
                Location = new Point(15, 15),
                AutoSize = true
            };

            lblRouterInfo = new Label
            {
                Text = "—",
                ForeColor = Color.Gray,
                Location = new Point(180, 24),
                AutoSize = true
            };

            _btnAddNewRouter = new Button
            {
                Text = "＋ Router",
                Location = new Point(830, 16),
                Size = new Size(80, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f)
            };

            _btnLogout = new Button
            {
                Text = "🚪 Sair",
                Location = new Point(920, 16),
                Size = new Size(80, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = danger, // Usa a variável danger que já tinhas (vermelho bonito)
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f)
            };
            _btnLogout.FlatAppearance.BorderSize = 0;
            _btnLogout.Click += BtnLogout_Click;

            _btnAddNewRouter.FlatAppearance.BorderSize = 0;
            _btnAddNewRouter.Click += BtnAddNewRouter_Click;

            _cmbRouters = new ComboBox
            {
                Location = new Point(595, 20),
                Size = new Size(225, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = bgItem,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f)
            };
            _cmbRouters.SelectedIndexChanged += CmbRouters_SelectedIndexChanged;

            pnlHeader.Controls.AddRange(new Control[] { lblSectionTitle, lblRouterInfo, _btnLogout, _btnAddNewRouter, _cmbRouters });

            // ── Toolbar de ações (Add / Delete / Action / Refresh) ────────────
            pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = bgDark,
                Padding = new Padding(4, 8, 4, 4)
            };

            _btnAdd = MakeToolbarBtn("＋ Adicionar", accent, 0);
            _btnDelete = MakeToolbarBtn("🗑 Apagar", danger, 130);
            _btnToggle = MakeToolbarBtn("⚡ Toggle", success, 260);
            _btnAction = MakeToolbarBtn("🛠 Sub-Menu", bgItem, 390);
            _btnRefresh = MakeToolbarBtn("↻ Atualizar", bgItem, 520);
            _btnExport = MakeToolbarBtn("📥 Exportar .conf", success, 650);

            // NOVO: Inicialização do botão que atira para as Pools (mesma posição do Export)
            _btnGoToPools = MakeToolbarBtn("💧 Ver Pools", accent, 650);

            _btnAdd.Click += BtnAdd_Click;
            _btnDelete.Click += BtnDelete_Click;
            _btnAction.Click += BtnAction_Click;
            _btnRefresh.Click += BtnRefresh_Click;
            _btnToggle.Click += BtnToggle_Click;
            _btnExport.Click += BtnExport_Click;

            // NOVO: Evento para saltar para a página das Pools
            _btnGoToPools.Click += async (s, e) =>
            {
                _currentTag = "pool";
                lblSectionTitle.Text = "IP Pools";

                // Atualiza a cor de destaque no menu lateral
                foreach (Control c in pnlNav.Controls)
                {
                    if (c is Button b)
                    {
                        if (b.Tag?.ToString() == "pool")
                        {
                            b.ForeColor = accent;
                            b.BackColor = Color.Transparent;
                        }
                        else
                        {
                            b.ForeColor = Color.Silver;
                            b.BackColor = Color.Transparent;
                        }
                    }
                }

                UpdateToolbarForSection("pool");
                await LoadSectionAsync("pool");
            };

            pnlToolbar.Controls.AddRange(new Control[] { _btnAdd, _btnDelete, _btnToggle, _btnAction, _btnRefresh, _btnExport, _btnGoToPools });

            // ── DataGrid ──────────────────────────────────────────────────────
            _dgvData = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = bgDark,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor = bgItem,
                ColumnHeadersHeight = 36,
                Font = new Font("Segoe UI", 9.5f)
            };
            _dgvData.DefaultCellStyle.BackColor = bgDark;
            _dgvData.DefaultCellStyle.ForeColor = Color.White;
            _dgvData.DefaultCellStyle.SelectionBackColor = bgItem;
            _dgvData.DefaultCellStyle.SelectionForeColor = Color.White;
            _dgvData.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(37, 37, 55);
            _dgvData.ColumnHeadersDefaultCellStyle.BackColor = bgPanel;
            _dgvData.ColumnHeadersDefaultCellStyle.ForeColor = Color.Silver;
            _dgvData.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _dgvData.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // ── Status bar ────────────────────────────────────────────────────
            pnlStatus = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = bgPanel };
            lblStatus = new Label
            {
                Text = "Pronto.",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(10, 7),
                AutoSize = true
            };
            pnlStatus.Controls.Add(lblStatus);

            // ── Conteúdo ──────────────────────────────────────────────────────
            pnlContent = new Panel { Dock = DockStyle.Fill };
            pnlContent.Controls.Add(_dgvData);
            pnlContent.Controls.Add(pnlToolbar);

            // ── Montagem final ────────────────────────────────────────────────
            this.Controls.AddRange(new Control[] { pnlContent, pnlHeader, pnlStatus, pnlSidebar });
            this.ResumeLayout(false);
        }

        private Button MakeToolbarBtn(string text, Color color, int x)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 6),
                Size = new Size(118, 30),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}