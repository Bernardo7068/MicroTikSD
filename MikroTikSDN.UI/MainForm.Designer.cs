namespace MikroTikSDN.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // Controlos
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlNav;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.ListBox lstRouters;
        private System.Windows.Forms.Button btnAddRouter;
        private System.Windows.Forms.Label lblRouters;
        private System.Windows.Forms.Label lblRouterInfo;
        private System.Windows.Forms.Label lblSectionTitle;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblAppTitle;

        // Botões de navegação
        private System.Windows.Forms.Button btnInterfaces;
        private System.Windows.Forms.Button btnBridges;
        private System.Windows.Forms.Button btnWireless;
        private System.Windows.Forms.Button btnIpAddresses;
        private System.Windows.Forms.Button btnRoutes;
        private System.Windows.Forms.Button btnDhcp;
        private System.Windows.Forms.Button btnDns;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // ── Cores ──────────────────────────────────────────────────────
            var bgDark    = System.Drawing.Color.FromArgb(30,  30,  46);
            var bgPanel   = System.Drawing.Color.FromArgb(37,  37,  55);
            var bgItem    = System.Drawing.Color.FromArgb(58,  58,  85);
            var accent    = System.Drawing.Color.FromArgb(124, 106, 247);
            var textPrim  = System.Drawing.Color.FromArgb(232, 232, 240);
            var textMuted = System.Drawing.Color.FromArgb(136, 136, 153);

            // ── Form ───────────────────────────────────────────────────────
            this.Text            = "MikroTik SDN Controller";
            this.Size            = new System.Drawing.Size(1100, 700);
            this.MinimumSize     = new System.Drawing.Size(850, 550);
            this.BackColor       = bgDark;
            this.ForeColor       = textPrim;
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Font            = new System.Drawing.Font("Segoe UI", 9.5f);

            // ── Sidebar ────────────────────────────────────────────────────
            pnlSidebar = new System.Windows.Forms.Panel
            {
                Dock      = System.Windows.Forms.DockStyle.Left,
                Width     = 220,
                BackColor = bgPanel
            };

            // Título app
            lblAppTitle = new System.Windows.Forms.Label
            {
                Text      = "🌐 SDN Controller",
                ForeColor = accent,
                Font      = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
                Location  = new System.Drawing.Point(14, 18),
                AutoSize  = true
            };

            var lblSubtitle = new System.Windows.Forms.Label
            {
                Text      = "MikroTik RouterOS",
                ForeColor = textMuted,
                Font      = new System.Drawing.Font("Segoe UI", 8.5f),
                Location  = new System.Drawing.Point(14, 42),
                AutoSize  = true
            };

            // Botão adicionar router
            btnAddRouter = new System.Windows.Forms.Button
            {
                Text      = "＋  Adicionar Router",
                Location  = new System.Drawing.Point(12, 70),
                Size      = new System.Drawing.Size(196, 34),
                BackColor = accent,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                Cursor    = System.Windows.Forms.Cursors.Hand,
                Font      = new System.Drawing.Font("Segoe UI", 9.5f)
            };
            btnAddRouter.FlatAppearance.BorderSize = 0;
            btnAddRouter.Click += btnAddRouter_Click;

            // Label routers
            lblRouters = new System.Windows.Forms.Label
            {
                Text      = "ROUTERS",
                ForeColor = System.Drawing.Color.FromArgb(85, 85, 102),
                Font      = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
                Location  = new System.Drawing.Point(14, 116),
                AutoSize  = true
            };

            // Lista de routers
            lstRouters = new System.Windows.Forms.ListBox
            {
                Location         = new System.Drawing.Point(8, 134),
                Size             = new System.Drawing.Size(204, 140),
                BackColor        = bgPanel,
                ForeColor        = textPrim,
                BorderStyle      = System.Windows.Forms.BorderStyle.None,
                Font             = new System.Drawing.Font("Segoe UI", 9.5f),
                ItemHeight       = 28
            };
            lstRouters.SelectedIndexChanged += lstRouters_SelectedIndexChanged;

            // Separador
            var sep = new System.Windows.Forms.Label
            {
                BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D,
                Location    = new System.Drawing.Point(8, 280),
                Size        = new System.Drawing.Size(204, 2),
                BackColor   = bgItem
            };

            // Label secções
            var lblSections = new System.Windows.Forms.Label
            {
                Text      = "SECÇÕES",
                ForeColor = System.Drawing.Color.FromArgb(85, 85, 102),
                Font      = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold),
                Location  = new System.Drawing.Point(14, 290),
                AutoSize  = true
            };

            // Painel de navegação
            pnlNav = new System.Windows.Forms.Panel
            {
                Location  = new System.Drawing.Point(0, 308),
                Size      = new System.Drawing.Size(220, 340),
                BackColor = bgPanel
            };

            // Botões de navegação
            btnInterfaces  = MakeNavButton("📡  Interfaces",      "interfaces",  0,  accent);
            btnBridges     = MakeNavButton("🌉  Bridges",          "bridges",     1,  bgPanel);
            btnWireless    = MakeNavButton("📶  Wireless",         "wireless",    2,  bgPanel);
            btnIpAddresses = MakeNavButton("🏷️  Endereços IP",    "ipaddresses", 3,  bgPanel);
            btnRoutes      = MakeNavButton("🗺️  Rotas Estáticas", "routes",      4,  bgPanel);
            btnDhcp        = MakeNavButton("📋  DHCP",             "dhcp",        5,  bgPanel);
            btnDns         = MakeNavButton("🔍  DNS",              "dns",         6,  bgPanel);

            pnlNav.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                btnInterfaces, btnBridges, btnWireless,
                btnIpAddresses, btnRoutes, btnDhcp, btnDns
            });

            pnlSidebar.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblAppTitle, lblSubtitle, btnAddRouter,
                lblRouters, lstRouters, sep, lblSections, pnlNav
            });

            // ── Header ─────────────────────────────────────────────────────
            pnlHeader = new System.Windows.Forms.Panel
            {
                Dock      = System.Windows.Forms.DockStyle.Top,
                Height    = 52,
                BackColor = bgPanel
            };

            lblSectionTitle = new System.Windows.Forms.Label
            {
                Text      = "Interfaces",
                ForeColor = textPrim,
                Font      = new System.Drawing.Font("Segoe UI", 14f, System.Drawing.FontStyle.Regular),
                Location  = new System.Drawing.Point(16, 12),
                AutoSize  = true
            };

            lblRouterInfo = new System.Windows.Forms.Label
            {
                Text      = "— Seleciona um router",
                ForeColor = textMuted,
                Font      = new System.Drawing.Font("Segoe UI", 10f),
                Location  = new System.Drawing.Point(160, 17),
                AutoSize  = true
            };

            pnlHeader.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblSectionTitle, lblRouterInfo
            });

            // ── Status bar ─────────────────────────────────────────────────
            pnlStatus = new System.Windows.Forms.Panel
            {
                Dock      = System.Windows.Forms.DockStyle.Bottom,
                Height    = 28,
                BackColor = bgPanel
            };

            lblStatus = new System.Windows.Forms.Label
            {
                Text      = "Pronto.",
                ForeColor = textMuted,
                Font      = new System.Drawing.Font("Segoe UI", 8.5f),
                Location  = new System.Drawing.Point(12, 6),
                AutoSize  = true
            };

            pnlStatus.Controls.Add(lblStatus);

            // ── Conteúdo ───────────────────────────────────────────────────
            pnlContent = new System.Windows.Forms.Panel
            {
                Dock      = System.Windows.Forms.DockStyle.Fill,
                BackColor = bgDark,
                Padding   = new System.Windows.Forms.Padding(16)
            };

            // ── Montagem ───────────────────────────────────────────────────
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlStatus);
            this.Controls.Add(pnlSidebar);

            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button MakeNavButton(string text, string tag, int index, System.Drawing.Color backColor)
        {
            var textMuted = System.Drawing.Color.FromArgb(136, 136, 153);
            var accent    = System.Drawing.Color.FromArgb(124, 106, 247);

            var btn = new System.Windows.Forms.Button
            {
                Text      = text,
                Tag       = tag,
                Location  = new System.Drawing.Point(8, index * 42 + 4),
                Size      = new System.Drawing.Size(204, 36),
                BackColor = backColor,
                ForeColor = index == 0 ? accent : textMuted,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding   = new System.Windows.Forms.Padding(8, 0, 0, 0),
                Cursor    = System.Windows.Forms.Cursors.Hand,
                Font      = new System.Drawing.Font("Segoe UI", 9.5f)
            };
            btn.FlatAppearance.BorderSize      = 0;
            btn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(58, 58, 85);
            btn.Click += NavButton_Click;
            return btn;
        }
    }
}
