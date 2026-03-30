using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MikroTikSDN.Core.Managers;
using MikroTikSDN.Core.Services;
using MikroTikSDN.UI.Dialogs;

// CORRIGIDO: alias para evitar ambiguidade com System.Net.NetworkInformation.NetworkInterface
using RouterNetInterface = MikroTikSDN.Core.Models.NetworkInterface;
using RouterIpAddress = MikroTikSDN.Core.Models.IpAddressModel;
using RouterDevice = MikroTikSDN.Core.Models.RouterDevice;
using RouterWireless = MikroTikSDN.Core.Models.WirelessInterface;
using RouterSecProfile = MikroTikSDN.Core.Models.SecurityProfile;
using RouterBridge = MikroTikSDN.Core.Models.BridgeModel;
using RouterBridgePort = MikroTikSDN.Core.Models.BridgePortModel;
using RouterRoute = MikroTikSDN.Core.Models.StaticRouteModel;
using RouterDhcp = MikroTikSDN.Core.Models.DhcpModel;

namespace MikroTikSDN.UI
{
    public partial class MainForm : Form
    {
        // ─── Estado ───────────────────────────────────────────────────────────

        private readonly Dictionary<string, RouterSession> _sessions = new();
        private RouterSession _current = null!;
        private string _currentTag = "iface";
        private readonly RouterManager _routerManager = new();

        // ─── Construtor ───────────────────────────────────────────────────────

        public MainForm(RouterClient firstClient, RouterDevice firstRouter)
        {
            InitializeComponent();
            AddSession(firstClient, firstRouter);
            SwitchRouter(firstRouter.IpAddress);
            this.Load += async (s, e) => await LoadSectionAsync("iface");
        }

        // ─── Gestão de sessões ────────────────────────────────────────────────

        private void AddSession(RouterClient client, RouterDevice device)
        {
            if (_sessions.ContainsKey(device.IpAddress)) return;
            _sessions[device.IpAddress] = new RouterSession(client, device);
            RefreshRouterCombo();
        }

        private void SwitchRouter(string ip)
        {
            if (!_sessions.TryGetValue(ip, out var session)) return;
            _current = session;
            lblRouterInfo.Text = $"— {_current.Device.Name}  ({ip})";
            _dgvData.DataSource = null;
        }

        private void RefreshRouterCombo()
        {
            _cmbRouters.Items.Clear();
            foreach (var s in _sessions.Values)
                _cmbRouters.Items.Add($"{s.Device.Name} ({s.Device.IpAddress})");
            _cmbRouters.SelectedIndex = _cmbRouters.Items.Count - 1;
        }

        private void CmbRouters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cmbRouters.SelectedItem is not string item) return;
            int start = item.LastIndexOf('(') + 1;
            int end = item.LastIndexOf(')');
            if (start > 0 && end > start)
            {
                SwitchRouter(item.Substring(start, end - start));
                _ = LoadSectionAsync(_currentTag);
            }
        }

        // ─── Adicionar router extra ───────────────────────────────────────────

        private async void BtnAddNewRouter_Click(object sender, EventArgs e)
        {
            using var login = new LoginForm(isDialogMode: true);
            if (login.ShowDialog(this) != DialogResult.OK) return;

            var client = login.AuthenticatedClient!;
            var device = login.AuthenticatedDevice!;

            if (_sessions.ContainsKey(device.IpAddress))
            {
                _cmbRouters.SelectedItem = _cmbRouters.Items
                    .Cast<string>()
                    .FirstOrDefault(i => i.Contains(device.IpAddress));
                return;
            }

            AddSession(client, device);
            SwitchRouter(device.IpAddress);
            SetStatus($"✅ Router adicionado: {device.Name}");
            await LoadSectionAsync(_currentTag);
        }

        // ─── Navegação ────────────────────────────────────────────────────────

        private async void NavButton_Click(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            foreach (Control c in pnlNav.Controls)
                if (c is Button b) { b.ForeColor = Color.Silver; b.BackColor = Color.FromArgb(37, 37, 55); }
            btn.ForeColor = Color.FromArgb(124, 106, 247);
            btn.BackColor = Color.FromArgb(46, 46, 69);

            _currentTag = btn.Tag?.ToString() ?? "iface";
            lblSectionTitle.Text = GetSectionTitle(_currentTag);

            await LoadSectionAsync(_currentTag);
        }

        private static string GetSectionTitle(string tag) => tag switch
        {
            "iface" => "Interfaces",
            "wifi" => "Wireless",
            "wifi_sec" => "Perfis de Segurança WiFi",
            "bridge" => "Bridges",
            "bridge_p" => "Portas de Bridge",
            "ip" => "Endereços IP",
            "route" => "Rotas Estáticas",
            "dhcp" => "Servidor DHCP",
            "dns" => "Configuração DNS",
            _ => ""
        };

        // ─── Carregar dados ───────────────────────────────────────────────────

        private async Task LoadSectionAsync(string tag)
        {
            _dgvData.DataSource = null;
            SetStatus("A carregar dados...");
            UpdateToolbarForSection(tag);

            try
            {
                var svc = _current.Services;

                switch (tag)
                {
                    case "iface": _dgvData.DataSource = await svc.Interfaces.GetAllAsync(); break;
                    case "wifi": _dgvData.DataSource = await svc.Wireless.GetWirelessAsync(); break;
                    case "wifi_sec": _dgvData.DataSource = await svc.Wireless.GetProfilesAsync(); break;
                    case "bridge": _dgvData.DataSource = await svc.Bridges.GetBridgesAsync(); break;
                    case "bridge_p": _dgvData.DataSource = await svc.Bridges.GetPortsAsync(); break;
                    case "ip": _dgvData.DataSource = await svc.IpAddresses.GetAddressesAsync(); break;
                    case "route": _dgvData.DataSource = await svc.Routes.GetRoutesAsync(); break;
                    case "dhcp": _dgvData.DataSource = await svc.Dhcp.GetServersAsync(); break;
                    case "dns": await LoadDnsAsync(svc); break;
                }

                FormatGridColumns(tag);
                SetStatus($"✅ {_dgvData.Rows.Count} registos carregados.");
            }
            catch (Exception ex)
            {
                SetStatus($"❌ Erro: {ex.Message}", error: true);
            }
        }

        private async Task LoadDnsAsync(RouterServices svc)
        {
            var dns = await svc.Dns.GetSettingsAsync();
            var table = new DataTable();
            table.Columns.Add("Propriedade");
            table.Columns.Add("Valor");
            table.Rows.Add("Servidores DNS", dns.Servers ?? "—");
            table.Rows.Add("Pedidos remotos", dns.AllowRemote ? "✅ Ativo" : "❌ Desativo");
            table.Rows.Add("Tamanho de cache", dns.CacheSize ?? "—");
            _dgvData.DataSource = table;
        }

        // ─── Formatação das colunas ───────────────────────────────────────────

        private void FormatGridColumns(string tag)
        {
            if (_dgvData.Columns.Count == 0) return;

            if (_dgvData.Columns.Contains("Id"))
                _dgvData.Columns["Id"].Visible = false;

            var friendlyNames = new Dictionary<string, string>
            {
                ["MacAddress"] = "Endereço MAC",
                ["Ssid"] = "SSID (Nome da Rede)",
                ["SecurityProfile"] = "Perfil de Segurança",
                ["AuthTypes"] = "Tipos de Autenticação",
                ["DstAddress"] = "Destino",
                ["Gateway"] = "Gateway",
                ["Distance"] = "Distância",
                ["Interface"] = "Interface",
                ["AddressPool"] = "Pool de Endereços",
                ["AllowRemote"] = "Pedidos Remotos",
                ["Running"] = "Ativo",
                ["Disabled"] = "Desativado",
                ["Dynamic"] = "Dinâmico",
                ["Address"] = "Endereço IP",
                ["Network"] = "Rede",
                ["Name"] = "Nome",
                ["Type"] = "Tipo",
                ["Comment"] = "Comentário",
                ["Mode"] = "Modo",
                ["Band"] = "Banda",
                ["Frequency"] = "Frequência",
                ["Mtu"] = "MTU",
                ["Bridge"] = "Bridge",
                ["LeaseTime"] = "Lease Time",
                ["Active"] = "Ativa",
                ["Servers"] = "Servidores",
                ["CacheSize"] = "Cache"
            };

            foreach (DataGridViewColumn col in _dgvData.Columns)
                if (friendlyNames.TryGetValue(col.Name, out var friendly))
                    col.HeaderText = friendly;
        }

        // ─── Toolbar de ações ─────────────────────────────────────────────────

        private void UpdateToolbarForSection(string tag)
        {
            bool hasCrud = tag is "wifi_sec" or "bridge" or "bridge_p" or "ip" or "route" or "dhcp";
            bool hasAction = tag is "wifi" or "dns";

            _btnAdd.Visible = hasCrud;
            _btnDelete.Visible = hasCrud;
            _btnAction.Visible = hasAction;
            _btnAction.Text = tag == "dns" ? "✏️ Editar DNS" : "⚡ Ativar/Desativar";
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var svc = _current.Services;
                switch (_currentTag)
                {
                    case "wifi_sec":
                        using (var d = new CrudDialog("Novo Perfil de Segurança",
                            ("Nome", ""), ("Tipos Auth (wpa2-psk)", "wpa2-psk")))
                        {
                            if (d.ShowDialog(this) != DialogResult.OK) return;
                            await svc.Wireless.AddProfileAsync(d["Nome"], d["Tipos Auth (wpa2-psk)"]);
                        }
                        break;

                    case "bridge":
                        using (var d = new CrudDialog("Nova Bridge", ("Nome", "")))
                        {
                            if (d.ShowDialog(this) != DialogResult.OK) return;
                            await svc.Bridges.AddBridgeAsync(d["Nome"]);
                        }
                        break;

                    case "bridge_p":
                        using (var d = new CrudDialog("Adicionar Porta à Bridge",
                            ("Bridge", ""), ("Interface", "")))
                        {
                            if (d.ShowDialog(this) != DialogResult.OK) return;
                            await svc.Bridges.AddPortToBridgeAsync(d["Bridge"], d["Interface"]);
                        }
                        break;

                    case "ip":
                        using (var d = new CrudDialog("Novo Endereço IP",
                            ("Endereço (ex: 192.168.1.1/24)", ""), ("Interface", "")))
                        {
                            if (d.ShowDialog(this) != DialogResult.OK) return;
                            await svc.IpAddresses.AddAddressAsync(
                                d["Endereço (ex: 192.168.1.1/24)"], d["Interface"]);
                        }
                        break;

                    case "route":
                        // CORRIGIDO: AddRouteAsync aceita 2 argumentos (dstAddress, gateway)
                        // a distância é opcional e tem de ser adicionada via PATCH depois se necessário
                        using (var d = new CrudDialog("Nova Rota Estática",
                            ("Destino (ex: 0.0.0.0/0)", ""), ("Gateway", "")))
                        {
                            if (d.ShowDialog(this) != DialogResult.OK) return;
                            await svc.Routes.AddRouteAsync(
                                d["Destino (ex: 0.0.0.0/0)"], d["Gateway"]);
                        }
                        break;

                    case "dhcp":
                        using (var d = new CrudDialog("Novo Servidor DHCP",
                            ("Nome", ""), ("Interface", ""), ("Pool", "")))
                        {
                            if (d.ShowDialog(this) != DialogResult.OK) return;
                            await svc.Dhcp.AddServerAsync(d["Nome"], d["Interface"], d["Pool"]);
                        }
                        break;
                }

                await LoadSectionAsync(_currentTag);
            }
            catch (Exception ex) { SetStatus($"❌ {ex.Message}", error: true); }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_dgvData.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleciona uma linha primeiro.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var row = _dgvData.SelectedRows[0];
            string? id = null;

            // CORRIGIDO: usa aliases para evitar ambiguidade
            if (row.DataBoundItem is RouterNetInterface iface) id = iface.Id;
            else if (row.DataBoundItem is RouterWireless w) id = w.Id;
            else if (row.DataBoundItem is RouterSecProfile sp) id = sp.Id;
            else if (row.DataBoundItem is RouterBridge br) id = br.Id;
            else if (row.DataBoundItem is RouterBridgePort bp) id = bp.Id;
            else if (row.DataBoundItem is RouterIpAddress ip) id = ip.Id;
            else if (row.DataBoundItem is RouterRoute rt) id = rt.Id;
            else if (row.DataBoundItem is RouterDhcp dh) id = dh.Id;

            if (string.IsNullOrWhiteSpace(id))
            {
                SetStatus("❌ Não foi possível obter o ID do recurso.", error: true);
                return;
            }

            if (MessageBox.Show("Tens a certeza que queres apagar este registo?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                var svc = _current.Services;
                switch (_currentTag)
                {
                    case "wifi_sec": await svc.Wireless.DeleteProfileAsync(id); break;
                    case "bridge": await svc.Bridges.DeleteBridgeAsync(id); break;
                    // CORRIGIDO: DeletePortAsync → DeleteAsync direto na BridgeService via endpoint
                    case "bridge_p": await svc.Bridges.DeleteBridgePortAsync(id); break;
                    case "ip": await svc.IpAddresses.DeleteAddressAsync(id); break;
                    case "route": await svc.Routes.DeleteRouteAsync(id); break;
                    case "dhcp": await svc.Dhcp.DeleteServerAsync(id); break;
                }

                await LoadSectionAsync(_currentTag);
            }
            catch (Exception ex) { SetStatus($"❌ {ex.Message}", error: true); }
        }

        private async void BtnAction_Click(object sender, EventArgs e)
        {
            try
            {
                var svc = _current.Services;

                if (_currentTag == "dns")
                {
                    var dns = await svc.Dns.GetSettingsAsync();
                    using var d = new CrudDialog("Configurar DNS",
                        ("Servidores (vírgula)", dns.Servers ?? ""),
                        ("Pedidos Remotos (yes/no)", dns.AllowRemote ? "yes" : "no"));
                    if (d.ShowDialog(this) != DialogResult.OK) return;
                    bool allow = d["Pedidos Remotos (yes/no)"].ToLower() == "yes";
                    await svc.Dns.UpdateSettingsAsync(d["Servidores (vírgula)"], allow);
                    await LoadSectionAsync("dns");
                }
                else if (_currentTag == "wifi")
                {
                    if (_dgvData.SelectedRows.Count == 0) return;
                    if (_dgvData.SelectedRows[0].DataBoundItem is not RouterWireless w) return;
                    if (string.IsNullOrEmpty(w.Id)) return;
                    await svc.Wireless.SetStateAsync(w.Id, w.Disabled); // Disabled=true → ativar
                    await LoadSectionAsync("wifi");
                }
            }
            catch (Exception ex) { SetStatus($"❌ {ex.Message}", error: true); }
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
            => await LoadSectionAsync(_currentTag);

        // ─── Helper de status ─────────────────────────────────────────────────

        private void SetStatus(string msg, bool error = false)
        {
            lblStatus.Text = msg;
            lblStatus.ForeColor = error
                ? Color.FromArgb(224, 108, 117)
                : Color.Gray;
        }
    }

    // ─── Sessão de router ─────────────────────────────────────────────────────

    public class RouterSession
    {
        public RouterClient Client { get; }
        public RouterDevice Device { get; }
        public RouterServices Services { get; }

        public RouterSession(RouterClient client, RouterDevice device)
        {
            Client = client;
            Device = device;
            Services = new RouterServices(client);
        }
    }

    // ─── Services agrupados por sessão ────────────────────────────────────────

    public class RouterServices
    {
        public InterfaceService Interfaces { get; }
        public WirelessService Wireless { get; }
        public IpAddressService IpAddresses { get; }
        public BridgeService Bridges { get; }
        public RouteService Routes { get; }
        public DhcpService Dhcp { get; }
        public DnsService Dns { get; }

        public RouterServices(RouterClient client)
        {
            Interfaces = new InterfaceService(client);
            Wireless = new WirelessService(client);
            IpAddresses = new IpAddressService(client);
            Bridges = new BridgeService(client);
            Routes = new RouteService(client);
            Dhcp = new DhcpService(client);
            Dns = new DnsService(client);
        }
    }
}