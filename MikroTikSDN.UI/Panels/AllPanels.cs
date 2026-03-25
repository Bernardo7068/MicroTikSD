using System;
using System.Drawing;
using System.Windows.Forms;
using MikroTikSDN.Core;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.UI.Panels
{
    // ─── Helper base ──────────────────────────────────────────────────────────
    internal static class PanelHelper
    {
        internal static readonly Color BgDark    = Color.FromArgb(30,  30,  46);
        internal static readonly Color BgPanel   = Color.FromArgb(37,  37,  55);
        internal static readonly Color BgItem    = Color.FromArgb(58,  58,  85);
        internal static readonly Color Accent    = Color.FromArgb(124, 106, 247);
        internal static readonly Color Danger    = Color.FromArgb(224, 108, 117);
        internal static readonly Color Success   = Color.FromArgb(76,  175, 130);
        internal static readonly Color TextPrim  = Color.FromArgb(232, 232, 240);
        internal static readonly Color TextMuted = Color.FromArgb(136, 136, 153);

        internal static Button MakeBtn(string text, Color color, int x, int y, int w = 120)
        {
            var btn = new Button
            {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(w, 30),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Font      = new Font("Segoe UI", 9f)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        internal static DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                BackgroundColor       = BgPanel,
                ForeColor             = TextPrim,
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
            g.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(30, 30, 46);
            g.ColumnHeadersDefaultCellStyle.ForeColor  = TextMuted;
            g.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            g.ColumnHeadersBorderStyle                  = DataGridViewHeaderBorderStyle.None;
            g.DefaultCellStyle.BackColor                = BgPanel;
            g.DefaultCellStyle.ForeColor                = TextPrim;
            g.DefaultCellStyle.SelectionBackColor       = BgItem;
            g.DefaultCellStyle.SelectionForeColor       = TextPrim;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(42, 42, 62);
            return g;
        }
    }


    // ─── BridgesPanel ─────────────────────────────────────────────────────────
    public class BridgesPanel : UserControl
    {
        private readonly RouterManager _router;
        private DataGridView gridBridges, gridPorts;

        public BridgesPanel(RouterManager router)
        {
            _router    = router;
            BackColor  = PanelHelper.BgDark;
            Dock       = DockStyle.Fill;

            // Toolbar bridges
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = PanelHelper.BgDark };
            var lblB = new Label { Text = "Bridges", ForeColor = PanelHelper.TextMuted, Location = new Point(0, 10), AutoSize = true };
            var btnAdd = PanelHelper.MakeBtn("＋ Adicionar", PanelHelper.Accent, 80, 5);
            btnAdd.Click += BtnAddBridge_Click;
            var btnDel = PanelHelper.MakeBtn("🗑 Apagar", PanelHelper.Danger, 208, 5);
            btnDel.Click += BtnDeleteBridge_Click;
            var btnRef = PanelHelper.MakeBtn("↻", PanelHelper.BgItem, 336, 5, 40);
            btnRef.Click += (s, e) => _ = LoadAsync();
            toolbar.Controls.AddRange(new Control[] { lblB, btnAdd, btnDel, btnRef });

            // Grid bridges
            gridBridges = PanelHelper.MakeGrid();
            gridBridges.Dock = DockStyle.None;
            gridBridges.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nome",      FillWeight = 25 });
            gridBridges.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "MTU",       FillWeight = 10 });
            gridBridges.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Protocolo", FillWeight = 20 });
            gridBridges.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Comentário",FillWeight = 45 });

            // Toolbar portas
            var toolbarP = new Panel { Dock = DockStyle.None, Height = 40, BackColor = PanelHelper.BgDark };
            var lblP = new Label { Text = "Portas da Bridge", ForeColor = PanelHelper.TextMuted, Location = new Point(0, 10), AutoSize = true };
            var btnAddP = PanelHelper.MakeBtn("＋ Porta", PanelHelper.Accent, 140, 5);
            btnAddP.Click += BtnAddPort_Click;
            var btnDelP = PanelHelper.MakeBtn("🗑 Porta", PanelHelper.Danger, 268, 5);
            btnDelP.Click += BtnRemovePort_Click;
            toolbarP.Controls.AddRange(new Control[] { lblP, btnAddP, btnDelP });

            // Grid portas
            gridPorts = PanelHelper.MakeGrid();
            gridPorts.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Bridge",    FillWeight = 33 });
            gridPorts.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Interface", FillWeight = 33 });
            gridPorts.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Desativado",FillWeight = 34 });

            // Layout com SplitContainer
            var split = new SplitContainer
            {
                Dock        = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                BackColor   = PanelHelper.BgDark,
                Panel1MinSize = 100,
                Panel2MinSize = 80
            };
            split.Panel1.Controls.Add(gridBridges);
            split.Panel1.Controls.Add(toolbar);
            gridBridges.Dock = DockStyle.Fill;
            split.Panel2.Controls.Add(gridPorts);
            split.Panel2.Controls.Add(toolbarP);
            gridPorts.Dock = DockStyle.Fill;
            toolbarP.Dock  = DockStyle.Top;
            toolbar.Dock   = DockStyle.Top;

            Controls.Add(split);
            _ = LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                gridBridges.Rows.Clear();
                var bridges = await _router.Bridges.GetBridgesAsync();
                foreach (var b in bridges)
                    gridBridges.Rows.Add(b.Name, b.Mtu, b.ProtocolMode, b.Comment);

                gridPorts.Rows.Clear();
                var ports = await _router.Bridges.GetPortsAsync();
                foreach (var p in ports)
                    gridPorts.Rows.Add(p.Bridge, p.Interface, p.Disabled);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnAddBridge_Click(object sender, EventArgs e)
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("Nome da bridge:", "Adicionar Bridge");
            if (string.IsNullOrWhiteSpace(name)) return;
            try { await _router.Bridges.CreateBridgeAsync(new System.Collections.Generic.Dictionary<string, string> { ["name"] = name }); await LoadAsync(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnDeleteBridge_Click(object sender, EventArgs e)
        {
            if (gridBridges.SelectedRows.Count == 0) return;
            var name = gridBridges.SelectedRows[0].Cells[0].Value?.ToString();
            if (MessageBox.Show($"Apagar bridge '{name}'?", "Confirmar", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            // Necessitas do ID — recarregar para obter
            try
            {
                var bridges = await _router.Bridges.GetBridgesAsync();
                var bridge = bridges.Find(b => b.Name == name);
                if (bridge?.Id != null) { await _router.Bridges.DeleteBridgeAsync(bridge.Id); await LoadAsync(); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnAddPort_Click(object sender, EventArgs e)
        {
            if (gridBridges.SelectedRows.Count == 0)
            { MessageBox.Show("Seleciona uma bridge primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var bridgeName = gridBridges.SelectedRows[0].Cells[0].Value?.ToString() ?? "";
            var ifaceName = Microsoft.VisualBasic.Interaction.InputBox("Nome da interface a adicionar:", "Adicionar Porta");
            if (string.IsNullOrWhiteSpace(ifaceName)) return;
            try { await _router.Bridges.AddPortAsync(bridgeName, ifaceName); await LoadAsync(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnRemovePort_Click(object sender, EventArgs e)
        {
            if (gridPorts.SelectedRows.Count == 0) return;
            var iface = gridPorts.SelectedRows[0].Cells[1].Value?.ToString();
            try
            {
                var ports = await _router.Bridges.GetPortsAsync();
                var port = ports.Find(p => p.Interface == iface);
                if (port?.Id != null) { await _router.Bridges.RemovePortAsync(port.Id); await LoadAsync(); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }


    // ─── WirelessPanel ────────────────────────────────────────────────────────
    public class WirelessPanel : UserControl
    {
        private readonly RouterManager _router;
        private DataGridView gridWlan, gridProfiles;

        public WirelessPanel(RouterManager router)
        {
            _router   = router;
            BackColor = PanelHelper.BgDark;
            Dock      = DockStyle.Fill;

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = PanelHelper.BgDark };
            var btnEnable  = PanelHelper.MakeBtn("✔ Ativar",    PanelHelper.Success, 0,   5);
            var btnDisable = PanelHelper.MakeBtn("✖ Desativar", PanelHelper.Danger,  128, 5);
            var btnRef     = PanelHelper.MakeBtn("↻",           PanelHelper.BgItem,  256, 5, 40);
            btnEnable.Click  += BtnEnable_Click;
            btnDisable.Click += BtnDisable_Click;
            btnRef.Click     += (s, e) => _ = LoadAsync();
            toolbar.Controls.AddRange(new Control[] { btnEnable, btnDisable, btnRef });

            gridWlan = PanelHelper.MakeGrid();
            gridWlan.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nome",      FillWeight = 15 });
            gridWlan.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SSID",      FillWeight = 20 });
            gridWlan.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Modo",      FillWeight = 15 });
            gridWlan.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Banda",     FillWeight = 15 });
            gridWlan.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Frequência",FillWeight = 15 });
            gridWlan.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Segurança", FillWeight = 20 });

            var lblP = new Label { Text = "Perfis de Segurança", ForeColor = PanelHelper.TextMuted, Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(0, 0, 0, 4) };

            gridProfiles = PanelHelper.MakeGrid();
            gridProfiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nome", FillWeight = 30 });
            gridProfiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Modo", FillWeight = 30 });
            gridProfiles.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tipos Auth", FillWeight = 40 });

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, BackColor = PanelHelper.BgDark };
            split.Panel1.Controls.Add(gridWlan);
            split.Panel1.Controls.Add(toolbar);
            gridWlan.Dock  = DockStyle.Fill;
            toolbar.Dock   = DockStyle.Top;
            split.Panel2.Controls.Add(gridProfiles);
            split.Panel2.Controls.Add(lblP);
            gridProfiles.Dock = DockStyle.Fill;

            Controls.Add(split);
            _ = LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                gridWlan.Rows.Clear();
                var wlan = await _router.Interfaces.GetWirelessAsync();
                foreach (var w in wlan)
                    gridWlan.Rows.Add(w.Name, w.Ssid, w.Mode, w.Band, w.Frequency, w.SecurityProfile);

                gridProfiles.Rows.Clear();
                var profiles = await _router.SecurityProfiles.GetAllAsync();
                foreach (var p in profiles)
                    gridProfiles.Rows.Add(p.Name, p.Mode, p.AuthenticationTypes);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnEnable_Click(object sender, EventArgs e)
        {
            if (gridWlan.SelectedRows.Count == 0) return;
            var name = gridWlan.SelectedRows[0].Cells[0].Value?.ToString();
            try
            {
                var wlan = await _router.Interfaces.GetWirelessAsync();
                var w = wlan.Find(x => x.Name == name);
                if (w?.Id != null) { await _router.Interfaces.EnableWirelessAsync(w.Id); await LoadAsync(); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnDisable_Click(object sender, EventArgs e)
        {
            if (gridWlan.SelectedRows.Count == 0) return;
            var name = gridWlan.SelectedRows[0].Cells[0].Value?.ToString();
            try
            {
                var wlan = await _router.Interfaces.GetWirelessAsync();
                var w = wlan.Find(x => x.Name == name);
                if (w?.Id != null) { await _router.Interfaces.DisableWirelessAsync(w.Id); await LoadAsync(); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }


    // ─── IpAddressesPanel ─────────────────────────────────────────────────────
    public class IpAddressesPanel : UserControl
    {
        private readonly RouterManager _router;
        private DataGridView grid;

        public IpAddressesPanel(RouterManager router)
        {
            _router   = router;
            BackColor = PanelHelper.BgDark;
            Dock      = DockStyle.Fill;

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = PanelHelper.BgDark };
            var btnAdd = PanelHelper.MakeBtn("＋ Adicionar", PanelHelper.Accent, 0,   5);
            var btnDel = PanelHelper.MakeBtn("🗑 Apagar",   PanelHelper.Danger, 128, 5);
            var btnRef = PanelHelper.MakeBtn("↻",           PanelHelper.BgItem, 256, 5, 40);
            btnAdd.Click += BtnAdd_Click;
            btnDel.Click += BtnDelete_Click;
            btnRef.Click += (s, e) => _ = LoadAsync();
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnDel, btnRef });

            grid = PanelHelper.MakeGrid();
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Endereço",   FillWeight = 25 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rede",       FillWeight = 25 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Interface",  FillWeight = 25 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Comentário", FillWeight = 25 });

            Controls.Add(grid);
            Controls.Add(toolbar);
            _ = LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                grid.Rows.Clear();
                var ips = await _router.IpAddresses.GetAllAsync();
                foreach (var ip in ips)
                    grid.Rows.Add(ip.Address, ip.Network, ip.Interface, ip.Comment);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            var address = Microsoft.VisualBasic.Interaction.InputBox("Endereço IP (ex: 192.168.1.1/24):", "Adicionar Endereço");
            if (string.IsNullOrWhiteSpace(address)) return;
            var iface = Microsoft.VisualBasic.Interaction.InputBox("Interface (ex: ether1):", "Adicionar Endereço");
            if (string.IsNullOrWhiteSpace(iface)) return;
            try { await _router.IpAddresses.CreateAsync(address, iface); await LoadAsync(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) return;
            var addr = grid.SelectedRows[0].Cells[0].Value?.ToString();
            if (MessageBox.Show($"Apagar endereço '{addr}'?", "Confirmar", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                var ips = await _router.IpAddresses.GetAllAsync();
                var ip = ips.Find(x => x.Address == addr);
                if (ip?.Id != null) { await _router.IpAddresses.DeleteAsync(ip.Id); await LoadAsync(); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }


    // ─── RoutesPanel ──────────────────────────────────────────────────────────
    public class RoutesPanel : UserControl
    {
        private readonly RouterManager _router;
        private DataGridView grid;

        public RoutesPanel(RouterManager router)
        {
            _router   = router;
            BackColor = PanelHelper.BgDark;
            Dock      = DockStyle.Fill;

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = PanelHelper.BgDark };
            var btnAdd = PanelHelper.MakeBtn("＋ Adicionar", PanelHelper.Accent, 0,   5);
            var btnDel = PanelHelper.MakeBtn("🗑 Apagar",   PanelHelper.Danger, 128, 5);
            var btnRef = PanelHelper.MakeBtn("↻",           PanelHelper.BgItem, 256, 5, 40);
            btnAdd.Click += BtnAdd_Click;
            btnDel.Click += BtnDelete_Click;
            btnRef.Click += (s, e) => _ = LoadAsync();
            toolbar.Controls.AddRange(new Control[] { btnAdd, btnDel, btnRef });

            grid = PanelHelper.MakeGrid();
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Destino",    FillWeight = 25 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Gateway",    FillWeight = 25 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Distância",  FillWeight = 10 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Interface",  FillWeight = 20 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Comentário", FillWeight = 20 });

            Controls.Add(grid);
            Controls.Add(toolbar);
            _ = LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                grid.Rows.Clear();
                var routes = await _router.Routes.GetAllAsync();
                foreach (var r in routes)
                    grid.Rows.Add(r.DstAddress, r.Gateway, r.Distance, r.Interface, r.Comment);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            var dst = Microsoft.VisualBasic.Interaction.InputBox("Destino (ex: 0.0.0.0/0):", "Adicionar Rota");
            if (string.IsNullOrWhiteSpace(dst)) return;
            var gw = Microsoft.VisualBasic.Interaction.InputBox("Gateway (ex: 192.168.1.1):", "Adicionar Rota");
            if (string.IsNullOrWhiteSpace(gw)) return;
            try { await _router.Routes.CreateAsync(dst, gw); await LoadAsync(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) return;
            var dst = grid.SelectedRows[0].Cells[0].Value?.ToString();
            if (MessageBox.Show($"Apagar rota '{dst}'?", "Confirmar", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                var routes = await _router.Routes.GetAllAsync();
                var route = routes.Find(r => r.DstAddress == dst);
                if (route?.Id != null) { await _router.Routes.DeleteAsync(route.Id); await LoadAsync(); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }


    // ─── DhcpPanel ────────────────────────────────────────────────────────────
    public class DhcpPanel : UserControl
    {
        private readonly RouterManager _router;
        private DataGridView gridServers, gridPools;

        public DhcpPanel(RouterManager router)
        {
            _router   = router;
            BackColor = PanelHelper.BgDark;
            Dock      = DockStyle.Fill;

            var lblS = new Label { Text = "Servidores DHCP", ForeColor = PanelHelper.TextMuted, Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.BottomLeft };
            gridServers = PanelHelper.MakeGrid();
            gridServers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nome",      FillWeight = 25 });
            gridServers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Interface",  FillWeight = 25 });
            gridServers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Pool",       FillWeight = 25 });
            gridServers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Lease Time", FillWeight = 25 });

            var lblP = new Label { Text = "Pools de Endereços", ForeColor = PanelHelper.TextMuted, Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.BottomLeft };
            gridPools = PanelHelper.MakeGrid();
            gridPools.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nome",   FillWeight = 40 });
            gridPools.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ranges", FillWeight = 60 });

            var btnRef = PanelHelper.MakeBtn("↻ Atualizar", PanelHelper.BgItem, 0, 5);
            btnRef.Click += (s, e) => _ = LoadAsync();
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = PanelHelper.BgDark };
            toolbar.Controls.Add(btnRef);

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, BackColor = PanelHelper.BgDark };
            split.Panel1.Controls.Add(gridServers);
            split.Panel1.Controls.Add(lblS);
            split.Panel1.Controls.Add(toolbar);
            gridServers.Dock = DockStyle.Fill;
            split.Panel2.Controls.Add(gridPools);
            split.Panel2.Controls.Add(lblP);
            gridPools.Dock = DockStyle.Fill;

            Controls.Add(split);
            _ = LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                gridServers.Rows.Clear();
                var servers = await _router.Dhcp.GetServersAsync();
                foreach (var s in servers)
                    gridServers.Rows.Add(s.Name, s.Interface, s.AddressPool, s.LeaseTime);

                gridPools.Rows.Clear();
                var pools = await _router.Dhcp.GetPoolsAsync();
                foreach (var p in pools)
                    gridPools.Rows.Add(p.Name, p.Ranges);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }


    // ─── DnsPanel ─────────────────────────────────────────────────────────────
    public class DnsPanel : UserControl
    {
        private readonly RouterManager _router;
        private TextBox txtServers, txtCacheSize;
        private CheckBox chkRemote;

        public DnsPanel(RouterManager router)
        {
            _router   = router;
            BackColor = PanelHelper.BgDark;
            Dock      = DockStyle.Fill;

            int y = 16;

            var lblTitle = new Label { Text = "Configuração DNS", ForeColor = PanelHelper.TextPrim, Font = new Font("Segoe UI", 11f, FontStyle.Bold), Location = new Point(0, y), AutoSize = true };
            y += 36;

            var lblServers = new Label { Text = "Servidores DNS (separados por vírgula):", ForeColor = PanelHelper.TextMuted, Location = new Point(0, y), AutoSize = true };
            y += 22;
            txtServers = new TextBox { Location = new Point(0, y), Size = new Size(400, 28), BackColor = Color.FromArgb(46, 46, 69), ForeColor = PanelHelper.TextPrim, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10f) };
            y += 40;

            chkRemote = new CheckBox { Text = "Permitir pedidos remotos (allow-remote-requests)", ForeColor = PanelHelper.TextPrim, Location = new Point(0, y), AutoSize = true };
            y += 36;

            var lblCache = new Label { Text = "Tamanho do cache:", ForeColor = PanelHelper.TextMuted, Location = new Point(0, y), AutoSize = true };
            y += 22;
            txtCacheSize = new TextBox { Location = new Point(0, y), Size = new Size(200, 28), BackColor = Color.FromArgb(46, 46, 69), ForeColor = PanelHelper.TextPrim, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10f), ReadOnly = true };
            y += 48;

            var btnSave = PanelHelper.MakeBtn("💾 Guardar", PanelHelper.Accent, 0, y, 140);
            btnSave.Click += BtnSave_Click;
            var btnRef  = PanelHelper.MakeBtn("↻ Atualizar", PanelHelper.BgItem, 148, y, 130);
            btnRef.Click  += (s, e) => _ = LoadAsync();

            Controls.AddRange(new Control[] { lblTitle, lblServers, txtServers, chkRemote, lblCache, txtCacheSize, btnSave, btnRef });
            _ = LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                var dns = await _router.Dns.GetSettingsAsync();
                txtServers.Text    = dns?.Servers ?? "";
                chkRemote.Checked  = dns?.AllowRemoteRequests ?? false;
                txtCacheSize.Text  = dns?.CacheSize ?? "";
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                await _router.Dns.SetSettingsAsync(txtServers.Text.Trim(), chkRemote.Checked);
                MessageBox.Show("Configuração DNS guardada.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
