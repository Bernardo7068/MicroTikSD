using MikroTikSDN.Core.Managers;
using MikroTikSDN.Core.Models;
using MikroTikSDN.Core.Services;
using MikroTikSDN.UI.Dialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RouterBridge = MikroTikSDN.Core.Models.BridgeModel;
using RouterBridgePort = MikroTikSDN.Core.Models.BridgePortModel;
using RouterDevice = MikroTikSDN.Core.Models.RouterDevice;
using RouterDhcp = MikroTikSDN.Core.Models.DhcpModel;
using RouterIpAddress = MikroTikSDN.Core.Models.IpAddressModel;
// Aliases para evitar conflitos de nomes
using RouterNetInterface = MikroTikSDN.Core.Models.NetworkInterface;
using RouterRoute = MikroTikSDN.Core.Models.StaticRouteModel;
using RouterSecProfile = MikroTikSDN.Core.Models.SecurityProfile;
using RouterWireless = MikroTikSDN.Core.Models.WirelessInterface;

namespace MikroTikSDN.UI
{
    public partial class MainForm : Form
    {
        // ─── Estado da Aplicação ──────────────────────────────────────────────

        private readonly Dictionary<string, RouterSession> _sessions = new();
        private RouterSession _current = null!;
        private string _currentTag = "iface";
        private RouterClient _client;

        // Controlos de "sub-abas" (Estilo WinBox)
        private bool _showBridgePorts = false;
        private bool _showWirelessProfiles = false;
        private bool _showDhcpClient = false;

        // ─── Construtor ───────────────────────────────────────────────────────

        public MainForm(RouterClient firstClient, RouterDevice firstRouter)
        {
            InitializeComponent();
            _dgvData.CellDoubleClick += DgvData_CellDoubleClick;
            AddSession(firstClient, firstRouter);
            SwitchRouter(firstRouter.IpAddress);

            // Evento de carregamento inicial
            this.Load += async (s, e) => await LoadSectionAsync("iface");
        }

        // ─── Eventos de Navegação e Toolbar ───────────────────────────────────

        private async void NavButton_Click(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            // Reset visual dos botões da sidebar
            foreach (Control c in pnlNav.Controls)
                if (c is Button b) { b.ForeColor = Color.Silver; b.BackColor = Color.Transparent; }

            btn.ForeColor = Color.FromArgb(124, 106, 247);
            _currentTag = btn.Tag?.ToString() ?? "iface";

            // Reset de estados internos ao mudar de categoria
            _showBridgePorts = false;
            _showWirelessProfiles = false;

            lblSectionTitle.Text = GetSectionTitle(_currentTag);
            await LoadSectionAsync(_currentTag);
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
            => await LoadSectionAsync(_currentTag);

        private async void BtnAction_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_currentTag)
                {
                    case "wifi":
                        _showWirelessProfiles = !_showWirelessProfiles;
                        lblSectionTitle.Text = _showWirelessProfiles ? "Wireless - Perfis" : "Wireless - Interfaces";
                        break;

                    case "bridge":
                        _showBridgePorts = !_showBridgePorts;
                        lblSectionTitle.Text = _showBridgePorts ? "Bridge - Ports" : "Bridge - Lista";
                        break;

                    case "dhcp":
                        _showDhcpClient = !_showDhcpClient;
                        lblSectionTitle.Text = _showDhcpClient ? "DHCP - Clients" : "DHCP - Servers";
                        break;

                    case "dns":
                        await ConfigurarDnsWinboxStyle();
                        return; // O DNS não troca de lista, apenas abre o diálogo, por isso saímos aqui
                }

                // 1. Atualiza os textos dos botões na Toolbar (importante!)
                UpdateToolbarForSection(_currentTag);

                // 2. Recarrega os dados na Grid
                await LoadSectionAsync(_currentTag);
            }
            catch (Exception ex)
            {
                SetStatus($"❌ Erro na ação: {ex.Message}", true);
            }
        }

        // ─── Lógica de CRUD (Adicionar e Remover) ────────────────────────────

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            // 1. Declaramos as variáveis comuns aqui no topo
            // Assim não há erros de "já declarado" ou "não declarado" nos cases
            var svc = _current.Services;
            SetStatus("A carregar opções do router...");

            try
            {
                switch (_currentTag)
                {
                    case "bridge":
                        if (_showBridgePorts)
                        {
                            // Procuramos os dados necessários para as dropdowns
                            var bridges = await svc.Bridges.GetBridgesAsync();
                            var eths = await svc.Interfaces.GetAllAsync();

                            using (var d = new CrudDialog("Nova Porta de Bridge",
                                ("Interface", eths.Select(i => i.Name).ToArray()),
                                ("Bridge", bridges.Select(b => b.Name).ToArray()),
                                ("PVID (VLAN ID)", "1")))
                            {
                                if (d.ShowDialog(this) == DialogResult.OK)
                                    await svc.Bridges.AddPortToBridgeAsync(d[1], d[0], d[2]);
                            }
                        }
                        else
                        {
                            using (var d = new CrudDialog("Nova Bridge",
                                ("Nome", "bridge1"),
                                ("STP (Protocol)", new[] { "rstp", "stp", "mstp", "none" }),
                                ("VLAN Filtering", new[] { "no", "yes" })))
                            {
                                if (d.ShowDialog(this) == DialogResult.OK)
                                    await svc.Bridges.AddBridgeAsync(d[0], d[1], d[2] == "yes");
                            }
                        }
                        break;

                    case "wifi":
                        if (_showWirelessProfiles)
                        {
                            using (var d = new CrudDialog("Novo Perfil WiFi",
                                ("Nome", "perfil1"),
                                ("Auth. Types", new[] { "wpa2-psk", "wpa-psk,wpa2-psk" }),
                                ("Ciphers", new[] { "aes-ccm", "tkip,aes-ccm" }),
                                ("Password", "")))
                            {
                                if (d.ShowDialog(this) == DialogResult.OK)
                                    await svc.Wireless.AddProfileAsync(d[0], d[1], d[2], d[3]);
                            }
                        }
                        else
                        {
                            var wlanIfaces = await svc.Wireless.GetWirelessAsync();
                            var masters = wlanIfaces.Where(w => string.IsNullOrEmpty(w.MasterInterface)).Select(w => w.Name).ToArray();

                            using (var d = new CrudDialog("Nova Virtual (VAP)",
                                ("Master Interface", masters),
                                ("SSID", "WiFi-Guest"),
                                ("Security Profile", "default")))
                            {
                                if (d.ShowDialog(this) == DialogResult.OK)
                                    await svc.Wireless.AddVirtualInterfaceAsync(d[0], d[1], d[2]);
                            }
                        }
                        break;

                    case "ip":
                        var allIfaces = await svc.Interfaces.GetAllAsync(); // Declarada apenas aqui
                        using (var d = new CrudDialog("Novo Endereço IP",
                            ("Endereço/CIDR", "192.168.88.1/24"),
                            ("Interface", allIfaces.Select(i => i.Name).ToArray())))
                        {
                            if (d.ShowDialog(this) == DialogResult.OK)
                                await svc.IpAddresses.AddAddressAsync(d[0], d[1]);
                        }
                        break;

                    case "dhcp":
                        // Vamos buscar as interfaces para o utilizador escolher num dropdown
                        var ifaces = (await svc.Interfaces.GetAllAsync()).Select(i => i.Name).ToArray();

                        if (_showDhcpClient)
                        {
                            // --- ADICIONAR DHCP CLIENT ---
                            using (var d = new CrudDialog("Novo DHCP Client",
                                ("Interface", ifaces),
                                ("Usar DNS do ISP (Use Peer DNS)", new[] { "yes", "no" }),
                                ("Adicionar Rota Default", new[] { "yes", "no" })))
                            {
                                if (d.ShowDialog(this) == DialogResult.OK)
                                {
                                    // Chamamos o método específico para o Cliente
                                    await svc.Dhcp.AddClientAsync(
                                        d[0],               // Interface
                                        d[1] == "yes",      // Use Peer DNS
                                        d[2] == "yes"       // Add Default Route
                                    );
                                    SetStatus("✅ Cliente DHCP adicionado com sucesso!");
                                }
                            }
                        }
                        else
                        {
                            // --- ADICIONAR DHCP SERVER ---
                            var pools = (await svc.IpPools.GetAllAsync()).Select(p => p.Name).ToList();
                            pools.Insert(0, "static-only"); // Opção padrão do MikroTik

                            using (var d = new CrudDialog("Novo DHCP Server",
                                ("Nome do Servidor", "dhcp-novo"),
                                ("Interface", ifaces),
                                ("Address Pool", pools.ToArray())))
                            {
                                if (d.ShowDialog(this) == DialogResult.OK)
                                {
                                    // Chamamos o método específico para o Servidor
                                    await svc.Dhcp.AddServerAsync(d[0], d[1], d[2]);
                                    SetStatus("✅ Servidor DHCP adicionado com sucesso!");
                                }
                            }
                        }

                        // Recarrega a lista correta após adicionar
                        await LoadSectionAsync("dhcp");
                        break;
                }

                await LoadSectionAsync(_currentTag);
                SetStatus("✅ Adicionado com sucesso!");
            }
            catch (Exception ex)
            {
                SetStatus($"❌ Erro: {ex.Message}", true);
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            var svc = _current.Services;

            try
            {
                // 1. TRATAMENTO ESPECIAL PARA DNS (Não tem ID, não tem lista)
                if (_currentTag == "dns")
                {
                    var dnsSettings = await svc.Dns.GetSettingsAsync();
                    // Troca entre "yes" e "no"
                    string novoEstado = (dnsSettings.AllowRemote?.ToLower() == "yes") ? "no" : "yes";

                    // Enviamos APENAS o campo que queremos mudar, sem IDs
                    await svc.Dns.UpdateSettingsAsync(new
                    {
                        @allow_remote_requests = novoEstado
                    });

                    SetStatus(novoEstado == "yes" ? "🟢 DNS Remoto Ativado" : "🔴 DNS Remoto Desativado");
                    await LoadSectionAsync("dns");
                    return; // IMPORTANTE: Sai aqui para não dar erro de ID abaixo!
                }

                // 2. PARA O RESTO (IP, WIFI, BRIDGE, ROUTES)
                if (_dgvData.SelectedRows.Count == 0) return;

                var item = _dgvData.SelectedRows[0].DataBoundItem;

                // Só aqui é que tentamos ler o ID, porque nestas categorias ele existe
                string id = (item as dynamic).Id;

                if (_currentTag == "wifi")
                {
                    if (_showWirelessProfiles)
                    {
                        if (MessageBox.Show("Remover Perfil?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            await svc.Wireless.DeleteProfileAsync(id);
                    }
                    else
                    {
                        var wlan = (RouterWireless)item;
                        if (string.IsNullOrEmpty(wlan.MasterInterface))
                        {
                            bool newState = wlan.Disabled?.ToLower() != "true";
                            await svc.Wireless.SetStateAsync(id, newState);
                        }
                        else
                        {
                            if (MessageBox.Show("Remover Virtual?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                await svc.Wireless.DeleteVirtualInterfaceAsync(id);
                        }
                    }
                }
                else
                {
                    if (MessageBox.Show("Deseja apagar este registo?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        switch (_currentTag)
                        {
                            case "ip": await svc.IpAddresses.DeleteAddressAsync(id); break;
                            case "bridge":
                                if (_showBridgePorts) await svc.Bridges.DeleteBridgePortAsync(id);
                                else await svc.Bridges.DeleteBridgeAsync(id);
                                break;
                            case "dhcp": await svc.Dhcp.DeleteServerAsync(id); break;
                            case "route": await svc.Routes.DeleteRouteAsync(id); break;
                        }
                    }
                }

                await LoadSectionAsync(_currentTag);
            }
            catch (Exception ex)
            {
                SetStatus($"❌ Erro: {ex.Message}", true);
            }
        }

        // ─── Carregamento e Formatação (MÉTODOS QUE FALTAVAM) ────────────────

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
                    case "wifi": _dgvData.DataSource = _showWirelessProfiles ? await svc.Wireless.GetProfilesAsync() : await svc.Wireless.GetWirelessAsync(); break;
                    case "bridge": _dgvData.DataSource = _showBridgePorts ? await svc.Bridges.GetPortsAsync() : await svc.Bridges.GetBridgesAsync(); break;
                    case "ip": _dgvData.DataSource = await svc.IpAddresses.GetAddressesAsync(); break;
                    case "dhcp":
                        _dgvData.DataSource = _showDhcpClient
                            ? await svc.Dhcp.GetClientsAsync()
                            : await svc.Dhcp.GetServersAsync();
                        break;
                    case "route": _dgvData.DataSource = await svc.Routes.GetRoutesAsync(); break;
                    case "dns": await LoadDnsAsync(svc); break;
                }
                FormatGridColumns(tag);
                SetStatus($"✅ {_dgvData.Rows.Count} registos carregados.");
            }
            catch (Exception ex) { SetStatus(ex.Message, true); }
        }

        private async Task LoadDnsAsync(RouterServices svc)
        {
            var dns = await svc.Dns.GetSettingsAsync();
            var dt = new DataTable();
            dt.Columns.Add("Propriedade"); dt.Columns.Add("Valor");
            dt.Rows.Add("Servidores DNS", dns.Servers ?? "—");
            dt.Rows.Add("Pedidos Remotos", dns.AllowRemote);
            dt.Rows.Add("Cache Size", dns.CacheSize);
            _dgvData.DataSource = dt;
        }

        private void FormatGridColumns(string tag)
        {
            if (_dgvData.Columns.Count == 0) return;
            if (_dgvData.Columns.Contains("Id")) _dgvData.Columns["Id"].Visible = false;

            var names = new Dictionary<string, string>
            {
                ["MacAddress"] = "MAC",
                ["Ssid"] = "Rede (SSID)",
                ["Address"] = "Endereço IP",
                ["Running"] = "Ativo",
                ["Disabled"] = "Desativado"
            };
            foreach (DataGridViewColumn col in _dgvData.Columns)
                if (names.TryGetValue(col.Name, out var n)) col.HeaderText = n;
        }

        private void SetStatus(string msg, bool error = false)
        {
            lblStatus.Text = msg;
            lblStatus.ForeColor = error ? Color.Salmon : Color.Gray;
        }

        private void UpdateToolbarForSection(string tag)
        {
            // 1. Visibilidade base
            _btnAdd.Visible = tag != "iface";
            _btnDelete.Visible = _btnAdd.Visible && tag != "dns";

            // Adicionamos "dhcp" à lista de quem usa o botão de ação (Alternar)
            _btnAction.Visible = (tag == "bridge" || tag == "wifi" || tag == "dns" || tag == "dhcp");

            // 2. Texto dinâmico do botão ADICIONAR (+)
            _btnAdd.Text = tag switch
            {
                "wifi" => _showWirelessProfiles ? "➕ Novo Perfil" : "➕ Nova Virtual",
                "bridge" => _showBridgePorts ? "➕ Nova Porta" : "➕ Nova Bridge",
                "ip" => "➕ Novo Endereço IP",
                "dhcp" => _showDhcpClient ? "➕ Novo Client" : "➕ Novo Server", // Texto dinâmico aqui
                "route" => "➕ Nova Rota Estática",
                _ => "➕ Adicionar"
            };

            // 3. Reset do texto do botão DELETE (Para não ficar "Ativar/Desativar" noutras abas)
            _btnDelete.Text = "🗑️ Remover";

            // 4. Configuração específica por secção
            if (tag == "dns")
            {
                _btnAdd.Visible = false;
                _btnDelete.Visible = true;
                _btnDelete.Text = "⚡ Ativar/Desativar";
                _btnAction.Text = "⚙️ Configurar DNS";
            }
            else if (tag == "bridge")
            {
                _btnAction.Text = _showBridgePorts ? "🌉 Ver Bridges" : "🔌 Ver Portas";
            }
            else if (tag == "wifi")
            {
                _btnAction.Text = _showWirelessProfiles ? "🌐 Ver Wireless" : "🔐 Ver Perfis";
            }
            // --- ADICIONADO PARA DHCP ---
            else if (tag == "dhcp")
            {
                _btnAction.Text = _showDhcpClient ? "🖥️ Ver Servers" : "🌐 Ver Clients";
            }
        }

        private async void DgvData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Ignora cliques no cabeçalho

            var svc = _current.Services;
            var item = _dgvData.Rows[e.RowIndex].DataBoundItem;
                string id = (item as dynamic).Id;

            SetStatus("A carregar opções para edição...");

            try
            {
                switch (_currentTag)
                {
                    case "ip":
                        var ipItem = (RouterIpAddress)item;
                        var ifacesIP = await svc.Interfaces.GetAllAsync();
                        using (var d = new CrudDialog($"Editar IP: {ipItem.Address}",
                            ("Endereço/CIDR", ipItem.Address ?? ""),
                            ("Interface", ifacesIP.Select(i => i.Name).ToArray())))
                        {
                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                await svc.IpAddresses.UpdateAddressAsync(id, d[0], d[1]);
                                await LoadSectionAsync("ip");
                            }
                        }
                        break;

                    case "bridge":
                        if (_showBridgePorts)
                        {
                            var portItem = (RouterBridgePort)item;
                            var bList = await svc.Bridges.GetBridgesAsync();
                            var iList = await svc.Interfaces.GetAllAsync();
                            using (var d = new CrudDialog($"Editar Porta: {portItem.Interface}",
                                ("Interface", iList.Select(i => i.Name).ToArray()),
                                ("Bridge", bList.Select(b => b.Name).ToArray()),
                                ("PVID", portItem.Pvid ?? "1")))
                            {
                                if (d.ShowDialog(this) == DialogResult.OK)
                                {
                                    await svc.Bridges.UpdatePortAsync(id, d[1], d[0], d[2]);
                                    await LoadSectionAsync("bridge");
                                }
                            }
                        }
                        else
                        {
                            var brItem = (RouterBridge)item;
                            using (var d = new CrudDialog($"Editar Bridge: {brItem.Name}",
                                ("Nome", brItem.Name ?? ""),
                                ("STP", new[] { "rstp", "stp", "mstp", "none" })))
                            {
                                if (d.ShowDialog(this) == DialogResult.OK)
                                {
                                    await svc.Bridges.UpdateBridgeAsync(id, d[0], d[1]);
                                    await LoadSectionAsync("bridge");
                                }
                            }
                        }
                        break;

                    case "wifi":
                        if (_showWirelessProfiles)
                        {
                            // EDITAR PERFIL DE SEGURANÇA
                            var profile = (RouterSecProfile)item;
                            using (var d = new CrudDialog($"Editar Perfil: {profile.Name}",
                                ("Nome", profile.Name ?? ""),
                                ("Auth. Types", new[] { "wpa2-psk", "wpa-psk,wpa2-psk" }),
                                ("Unicast Ciphers", new[] { "aes-ccm", "tkip,aes-ccm" }),
                                ("WPA2 Pre-Shared Key", ""))) // Password
                            {
                                if (d.ShowDialog(this) == DialogResult.OK)
                                {
                                    await svc.Wireless.UpdateProfileAsync(id, d[0], d[1], d[2], d[3]);
                                    await LoadSectionAsync("wifi");
                                }
                            }
                        }
                        else
                        {
                            await EditarWirelessInterface((RouterWireless)item);
                        }
                        break;

                    case "route":
                        // EDITAR ROTA ESTÁTICA
                        var route = (RouterRoute)item;
                        using (var d = new CrudDialog($"Editar Rota: {route.DstAddress}",
                            ("Dst. Address", route.DstAddress ?? "0.0.0.0/0"),
                            ("Gateway", route.Gateway ?? ""),
                            ("Distance", route.Distance ?? "1"),
                            ("Comment", route.Comment ?? "")))
                        {
                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                await svc.Routes.UpdateRouteAsync(id, d[0], d[1], d[2], d[3]);
                                await LoadSectionAsync("route");
                            }
                        }
                        break;

                    case "dhcp":
                    // Criamos os serviços na hora usando o _client
                    var dhcpSvc = new DhcpService(_client);
                    var poolSvc = new IpPoolService(_client);
                    var ifaceSvc = new InterfaceService(_client);
                    var allIfaces = (await ifaceSvc.GetAllAsync()).Select(i => i.Name).ToArray();

                    if (_showDhcpClient) {
                        // EDITAR CLIENTE
                        var c = (DhcpClientModel)item;
                        using (var d = new CrudDialog("Editar DHCP Client", ("Interface", allIfaces), ("DNS", new[]{"yes","no"}), ("Route", new[]{"yes","no"}))) {
                            if (d.ShowDialog(this) == DialogResult.OK)
                                await dhcpSvc.UpdateClientAsync(id, d[0], d[1] == "yes", d[2] == "yes");
                        }
                    } else {
                        // EDITAR SERVER + POOL
                        var s = (DhcpModel)item;
                        var pools = await poolSvc.GetAllAsync();
                        var poolReal = pools.FirstOrDefault(p => p.Name == s.AddressPool);

                        using (var d = new CrudDialog("Editar DHCP Server", 
                            ("Nome", s.Name), 
                            ("Interface", allIfaces), 
                            ("Pool", pools.Select(p => p.Name).ToArray()), 
                            ("IP Ranges", poolReal?.Ranges ?? ""))) 
                        {
                            if (d.ShowDialog(this) == DialogResult.OK) {
                                await dhcpSvc.UpdateServerAsync(id, d[0], d[1], d[2]);
                                if (poolReal != null && d[3] != poolReal.Ranges)
                                    await poolSvc.UpdateAsync(poolReal.Id, poolReal.Name, d[3]);
                            }
                        }
                    }
                    await LoadSectionAsync("dhcp");
                    break;

                    case "dns":
                        // Chamamos a mesma lógica de cima (podes até criar um método separado para não repetir código)
                        await ConfigurarDnsWinboxStyle();
                        break;
                }
                SetStatus("✅ Alterações aplicadas!");
            }
            catch (Exception ex) { SetStatus($"❌ Erro ao editar: {ex.Message}", true); }
        }

        private async Task ConfigurarDnsWinboxStyle()
{
    var svc = _current.Services;
    SetStatus("A carregar definições DNS...");

    try 
    {
        var dns = await svc.Dns.GetSettingsAsync();
        var allIfaces = await svc.Interfaces.GetAllAsync();
        var ifaceNames = allIfaces.Select(i => i.Name).ToArray();

        using (var d = new CrudDialog("Configurações Avançadas DNS",
            ("Servers (ex: 8.8.8.8)", dns.Servers ?? ""),
            ("DoH Server URL", dns.UseDohServer ?? ""),
            ("mDNS Repeater Interface", ifaceNames),
            ("Cache Size (KiB)", dns.CacheSize ?? "2048"), // Agora o C# já reconhece dns.CacheSize
            ("Max UDP Packet Size", dns.MaxUdpPacketSize ?? "4096")))
        {
            if (d.ShowDialog(this) == DialogResult.OK)
            {
                var payload = new Dictionary<string, object>
                {
                    { "servers", d[0] },
                    { "use-doh-server", d[1] },
                    { "mdns-repeater-interfaces", d[2] },
                    { "cache-size", d[3] },
                    { "max-udp-packet-size", d[4] }
                };

                await svc.Dns.UpdateSettingsAsync(payload);
                await LoadSectionAsync("dns");
                SetStatus("✅ DNS e Cache atualizados!");
            }
        }
    }
    catch (Exception ex) { SetStatus($"❌ Erro: {ex.Message}", true); }
}

        private async Task EditarWirelessInterface(RouterWireless wlan)
        {
            var svc = _current.Services;

            // 1. Procurar Perfis de Segurança reais no router
            var profiles = await svc.Wireless.GetProfilesAsync();
            var profileNames = profiles.Select(p => p.Name).ToArray();
            if (profileNames.Length == 0) profileNames = new[] { "default" };

            // 2. Opções Oficiais (Igual ao Winbox)
            var modes = new[] { "ap-bridge", "station", "bridge", "station-bridge" };
            var bands = new[] { "2ghz-b/g/n", "5ghz-a/n/ac", "2ghz-onlyn", "5ghz-onlyac" };
            var widths = new[] { "20mhz", "20/40mhz-XX", "20/40mhz-Ce", "20/40mhz-eC" };
            var countries = new[] { "portugal", "brazil", "spain", "etsi" };

            using (var d = new CrudDialog($"Wireless <{wlan.Name}>",
                ("Mode", modes),
                ("Band", bands),
                ("Channel Width", widths),
                ("Frequency", "auto"),
                ("SSID", wlan.Ssid ?? "MikroTik"),
                ("Security Profile", profileNames),
                ("Country", countries)))
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    var updateData = new Dictionary<string, string> {
                { "mode", d[0] },
                { "band", d[1] },
                { "channel-width", d[2] },
                { "frequency", d[3] },
                { "ssid", d[4] },
                { "security-profile", d[5] },
                { "country", d[6] }
            };

                    await svc.Wireless.UpdateInterfaceAsync(wlan.Id, updateData);
                    await LoadSectionAsync("wifi");
                }
            }
        }
        // ─── Gestão de Sessões e Routers ─────────────────────────────────────

        private async void BtnEditWireless_Click()
        {
            if (_dgvData.SelectedRows.Count == 0) return;
            var wlan = (RouterWireless)_dgvData.SelectedRows[0].DataBoundItem;

            // 1. Procurar perfis de segurança disponíveis para a dropdown
            var profiles = await _current.Services.Wireless.GetProfilesAsync();
            string profileList = string.Join(", ", profiles.Select(p => p.Name));

            // 2. Abrir Diálogo estilo Winbox
            using (var d = new CrudDialog($"Editar Wireless: {wlan.Name}",
                ("SSID", wlan.Ssid ?? ""),
                ("Mode (ap-bridge/station/etc)", wlan.Mode ?? "ap-bridge"),
                ("Security Profile (Disponíveis: " + profileList + ")", wlan.SecurityProfile ?? "default")))
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    var updateData = new
                    {
                        ssid = d[0],
                        mode = d[1],
                        @security_profile = d[2]
                    };

                    await _current.Services.Wireless.UpdateInterfaceAsync(wlan.Id, updateData);
                    await LoadSectionAsync("wifi");
                    SetStatus("✅ Interface atualizada!");
                }
            }
        }



        private async void BtnAddNewRouter_Click(object sender, EventArgs e)
        {
            using var login = new LoginForm(isDialogMode: true);
            if (login.ShowDialog(this) != DialogResult.OK) return;

            AddSession(login.AuthenticatedClient!, login.AuthenticatedDevice!);
            SwitchRouter(login.AuthenticatedDevice!.IpAddress);
            await LoadSectionAsync(_currentTag);
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

        private void AddSession(RouterClient client, RouterDevice device)
        {
            if (_sessions.ContainsKey(device.IpAddress)) return;
            _sessions[device.IpAddress] = new RouterSession(client, device);
            RefreshRouterCombo();
        }

        private void SwitchRouter(string ip)
        {
            if (!_sessions.TryGetValue(ip, out var s)) return;
            _current = s;
            lblRouterInfo.Text = $"— {s.Device.Name} ({ip})";
        }

        private void RefreshRouterCombo()
        {
            _cmbRouters.Items.Clear();
            foreach (var s in _sessions.Values) _cmbRouters.Items.Add($"{s.Device.Name} ({s.Device.IpAddress})");
            if (_cmbRouters.Items.Count > 0) _cmbRouters.SelectedIndex = _cmbRouters.Items.Count - 1;
        }

        private string GetSectionTitle(string tag) => tag switch
        {
            "iface" => "Interfaces",
            "bridge" => "Bridge",
            "wifi" => "Wireless",
            "ip" => "IP Addresses",
            "dhcp" => "DHCP Server",
            "dns" => "DNS Settings",
            "route" => "Static Routes",
            _ => "Rede"
        };
    }

    // ─── Classes de Suporte ──────────────────────────────────────────────────

    public class RouterSession
    {
        public RouterClient Client { get; }
        public RouterDevice Device { get; }
        public RouterServices Services { get; }
        public RouterSession(RouterClient c, RouterDevice d) { Client = c; Device = d; Services = new RouterServices(c); }
    }

    public class RouterServices
    {
        public InterfaceService Interfaces { get; }
        public WirelessService Wireless { get; }
        public IpAddressService IpAddresses { get; }
        public BridgeService Bridges { get; }
        public RouteService Routes { get; }
        public DhcpService Dhcp { get; }
        public DnsService Dns { get; }
        public IpPoolService IpPools { get; }
        public RouterServices(RouterClient c)
        {
            Interfaces = new InterfaceService(c); Wireless = new WirelessService(c);
            IpAddresses = new IpAddressService(c); Bridges = new BridgeService(c);
            Routes = new RouteService(c); Dhcp = new DhcpService(c); Dns = new DnsService(c);
            IpPools = new IpPoolService(c);
        }
    }
}