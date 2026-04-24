using MikroTikSDN.Core.Managers;
using MikroTikSDN.Core.Models;
using MikroTikSDN.Core.Services;
using MikroTikSDN.UI.Dialogs;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

// Aliases para evitar conflitos de nomes com System.Net.NetworkInformation
using RouterBridge = MikroTikSDN.Core.Models.BridgeModel;
using RouterBridgePort = MikroTikSDN.Core.Models.BridgePortModel;
using RouterDevice = MikroTikSDN.Core.Models.RouterDevice;
using RouterDhcp = MikroTikSDN.Core.Models.DhcpModel;
using RouterIpAddress = MikroTikSDN.Core.Models.IpAddressModel;
using RouterNetInterface = MikroTikSDN.Core.Models.NetworkInterface;
using RouterRoute = MikroTikSDN.Core.Models.StaticRouteModel;
using RouterSecProfile = MikroTikSDN.Core.Models.SecurityProfile;
using RouterWireless = MikroTikSDN.Core.Models.WirelessInterface;

namespace MikroTikSDN.UI
{
    public partial class MainForm : Form
    {
        // ─── Estado ───────────────────────────────────────────────────────────

        private readonly Dictionary<string, RouterSession> _sessions = new();
        private Dictionary<string, string> _vaultPrivado = new Dictionary<string, string>();
        private RouterSession _current = null!;
        private string _currentTag = "iface";

        // Sub-vistas (estilo WinBox — alternar entre listas dentro da mesma secção)
        private bool _showBridgePorts = false;
        private bool _showWirelessProfiles = false;
        private bool _showDhcpClient = false;
        private bool _showWgPeers = true; // WireGuard: True = Clientes, False = Servidores

        // ─── Construtor ───────────────────────────────────────────────────────

        public MainForm(RouterClient firstClient, RouterDevice firstRouter)
        {
            InitializeComponent();
            _dgvData.CellDoubleClick += DgvData_CellDoubleClick;
            _dgvData.CellFormatting += DgvData_CellFormatting;
            AddSession(firstClient, firstRouter);
            SwitchRouter(firstRouter.IpAddress);
            this.Load += async (s, e) => await LoadSectionAsync("iface");
        }

        // ─── Navegação ────────────────────────────────────────────────────────

        private async void NavButton_Click(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            foreach (Control c in pnlNav.Controls)
                if (c is Button b) { b.ForeColor = Color.Silver; b.BackColor = Color.Transparent; }
            btn.ForeColor = Color.FromArgb(124, 106, 247);

            _currentTag = btn.Tag?.ToString() ?? "iface";
            _showBridgePorts = false;
            _showWirelessProfiles = false;
            _showDhcpClient = false;
            _showWgPeers = true; // Reset da vista do WireGuard
            lblSectionTitle.Text = GetSectionTitle(_currentTag);

            await LoadSectionAsync(_currentTag);
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
            => await LoadSectionAsync(_currentTag);

        // ─── Botão Ação (alterna sub-vista ou abre diálogo DNS) ───────────────
        private async void BtnLogout_Click(object sender, EventArgs e)
        {
            if (_current == null) return;

            string ipLogout = _current.Device.IpAddress;
            string nomeLogout = _current.Device.Name;

            // Pergunta de confirmação
            if (MessageBox.Show($"Tem a certeza que deseja terminar a sessão no router '{nomeLogout}' ({ipLogout})?",
                "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // 1. Remove o router da memória
                _sessions.Remove(ipLogout);

                // 2. Verifica se ainda sobraram routers
                if (_sessions.Count > 0)
                {
                    // Pega no IP do "próximo" router que sobrou na lista
                    string proximoIp = _sessions.Keys.First();

                    // Muda o controlo para esse router
                    SwitchRouter(proximoIp);
                    RefreshRouterCombo();

                    // Recarrega a tabela com os dados do novo router
                    await LoadSectionAsync(_currentTag);
                    SetStatus($"Sessão terminada em {ipLogout}. Agora a gerir {proximoIp}.", false);
                }
                else
                {
                    // 3. Era o último router! 
                    // Application.Restart() é a forma mais segura de fazer logout no WinForms.
                    // Limpa a RAM (fechando o _vaultPrivado e as ligações) e volta a abrir a app do zero (LoginForm).
                    Application.Restart();
                }
            }
        }

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
                        lblSectionTitle.Text = _showBridgePorts ? "Bridge - Portas" : "Bridge - Lista";
                        break;
                    case "dhcp":
                        _showDhcpClient = !_showDhcpClient;
                        lblSectionTitle.Text = _showDhcpClient ? "DHCP - Clients" : "DHCP - Servers";
                        break;
                    case "wg":
                        _showWgPeers = !_showWgPeers;
                        lblSectionTitle.Text = _showWgPeers ? "WireGuard - Clientes" : "WireGuard - Servidor";
                        break;
                    case "dns":
                        await ConfigurarDnsAsync();
                        return;
                }

                UpdateToolbarForSection(_currentTag);
                await LoadSectionAsync(_currentTag);
            }
            catch (Exception ex) { SetStatus($"❌ Erro: {ex.Message}", true); }
        }

        // ─── Adicionar ────────────────────────────────────────────────────────

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            var svc = _current.Services;
            SetStatus("A preparar formulário...");

            try
            {
                switch (_currentTag)
                {
                    case "bridge":
                        {
                            if (_showBridgePorts)
                            {
                                var bridges = await svc.Bridges.GetBridgesAsync();
                                var eths = await svc.Interfaces.GetAllAsync();
                                using var d = new CrudDialog("Nova Porta de Bridge",
                                    ("Interface", eths.Select(i => i.Name).ToArray()),
                                    ("Bridge", bridges.Select(b => b.Name).ToArray()),
                                    ("PVID", "1"));
                                if (d.ShowDialog(this) == DialogResult.OK)
                                    await svc.Bridges.AddPortToBridgeAsync(d[1], d[0], d[2]);
                            }
                            else
                            {
                                using var d = new CrudDialog("Nova Bridge",
                                    ("Nome", "bridge1"),
                                    ("STP Protocol", new[] { "rstp", "stp", "mstp", "none" }),
                                    ("VLAN Filtering", new[] { "no", "yes" }));
                                if (d.ShowDialog(this) == DialogResult.OK)
                                    await svc.Bridges.AddBridgeAsync(d[0], d[1], d[2] == "yes");
                            }
                        }
                        break;

                    case "wifi":
                        {
                            if (_showWirelessProfiles)
                            {
                                using var d = new CrudDialog("Novo Perfil WiFi",
                                    ("Nome", "perfil1"),
                                    ("Auth Types", new[] {
                                     "wpa2-psk",
                                       "wpa-psk,wpa2-psk",
                                       "wpa2-eap",
                                       "wpa-eap,wpa2-eap"
                                     }),
                                    ("Ciphers", new[] { "aes-ccm", "tkip", "tkip,aes-ccm" }),
                                    ("Password", ""));
                                if (d.ShowDialog(this) == DialogResult.OK)
                                    await svc.Wireless.AddProfileAsync(d[0], d[1], d[2], d[3]);
                            }
                            else
                            {
                                var wlans = await svc.Wireless.GetWirelessAsync();
                                var masters = wlans.Where(w => string.IsNullOrEmpty(w.MasterInterface))
                                                   .Select(w => w.Name).ToArray();
                                using var d = new CrudDialog("Nova Interface Virtual (VAP)",
                                    ("Master Interface", masters),
                                    ("SSID", "WiFi-Guest"),
                                    ("Security Profile", "default"));
                                if (d.ShowDialog(this) == DialogResult.OK)
                                    await svc.Wireless.AddVirtualInterfaceAsync(d[0], d[1], d[2]);
                            }
                        }
                        break;

                    case "ip":
                        {
                            var ifacesIP = await svc.Interfaces.GetAllAsync();
                            using var d = new CrudDialog("Novo Endereço IP",
                                ("Endereço/CIDR", "192.168.88.1/24"),
                                ("Interface", ifacesIP.Select(i => i.Name).ToArray()));
                            if (d.ShowDialog(this) == DialogResult.OK)
                                await svc.IpAddresses.AddAddressAsync(d[0], d[1]);
                        }
                        break;

                    case "route":
                        {
                            using var d = new CrudDialog("Nova Rota Estática",
                                ("Destino (ex: 0.0.0.0/0)", "0.0.0.0/0"),
                                ("Gateway", ""),
                                ("Distância", "1"));
                            if (d.ShowDialog(this) == DialogResult.OK)
                                await svc.Routes.AddRouteAsync(d[0], d[1], d[2]);
                        }
                        break;

                    case "dhcp":
                        {
                            var ifacesDhcp = (await svc.Interfaces.GetAllAsync()).Select(i => i.Name).ToArray();

                            if (_showDhcpClient)
                            {
                                using var d = new CrudDialog("Novo DHCP Client",
                                    ("Interface", ifacesDhcp),
                                    ("Usar DNS do ISP", new[] { "yes", "no" }),
                                    ("Adicionar Rota Def.", new[] { "yes", "no" }));

                                if (d.ShowDialog(this) == DialogResult.OK)
                                {
                                    SetStatus("A criar DHCP Client...");
                                    try
                                    {
                                        await svc.Dhcp.AddClientAsync(d[0], d[1] == "yes", d[2] == "yes");
                                        SetStatus("✅ DHCP Client criado com sucesso!");
                                    }
                                    catch (Exception ex) { SetStatus($"❌ Erro: {ex.Message}", true); }
                                }
                            }
                            else
                            {
                                using var d1 = new CrudDialog("DHCP Setup (1/2)",
                                    ("Nome do Servidor", "dhcp1"),
                                    ("Interface", ifacesDhcp),
                                    ("DHCP Address Space (ex: 192.168.88.0/24)", ""));

                                if (d1.ShowDialog(this) == DialogResult.OK)
                                {
                                    string serverName = d1[0].Trim();
                                    string iface = d1[1].Trim();
                                    string network = d1[2].Trim();

                                    string baseIp = "";
                                    if (network.Contains("/") && network.Contains("."))
                                    {
                                        string ipPart = network.Split('/')[0];
                                        int lastDot = ipPart.LastIndexOf('.');
                                        if (lastDot > 0) baseIp = ipPart.Substring(0, lastDot);
                                    }

                                    string sugeridoGateway = baseIp != "" ? $"{baseIp}.1" : "";
                                    string sugeridoRange = baseIp != "" ? $"{baseIp}.2-{baseIp}.254" : "";

                                    using var d2 = new CrudDialog("DHCP Setup (2/2)",
                                        ("Gateway for DHCP Network", sugeridoGateway),
                                        ("Addresses to Give Out", sugeridoRange),
                                        ("DNS Servers (opcional)", sugeridoGateway));

                                    if (d2.ShowDialog(this) == DialogResult.OK)
                                    {
                                        string gateway = d2[0].Trim();
                                        string ipRange = d2[1].Trim();
                                        string dnsServer = d2[2].Trim();
                                        string poolName = $"pool-{serverName}";

                                        SetStatus("A configurar DHCP completo...");

                                        try
                                        {
                                            await svc.IpPools.AddAsync(poolName, ipRange);
                                            await svc.Dhcp.AddNetworkAsync(network, gateway, dnsServer);
                                            await svc.Dhcp.AddServerAsync(serverName, iface, poolName);
                                            SetStatus("✅ DHCP Setup concluído com sucesso!");
                                        }
                                        catch (Exception ex)
                                        {
                                            SetStatus($"❌ Erro no Setup: {ex.Message}", true);
                                            MessageBox.Show($"Detalhe:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case "pool":
                        using (var d = new CrudDialog("Nova IP Pool",
                            ("Nome", "dhcp-pool"),
                            ("Ranges (IP-IP)", "192.168.88.10-192.168.88.254")))
                        {
                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                await svc.IpPools.AddAsync(d[0].Trim(), d[1].Trim());
                            }
                        }
                        break;

                    case "wg":
                        if (_showWgPeers) // Adicionar CLIENTE
                        {
                            var chaves = WireGuardService.GenerateKeyPair();
                            var wgIfaces = (await svc.WireGuard.GetInterfacesAsync()).Select(i => i.Name).ToArray();

                            if (wgIfaces.Length == 0)
                            {
                                MessageBox.Show("Crie primeiro um Servidor/Interface!", "Aviso");
                                return;
                            }

                            using var d = new CrudDialog("Novo Cliente VPN",
                                ("Nome do Dispositivo", "Telemovel"),
                                ("Interface Servidor", wgIfaces),
                                ("IP na VPN", "192.168.2.2/32"),
                                ("Public Key (Gerada pela App)", chaves.pub));

                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                SetStatus("A criar cliente...");
                                await svc.WireGuard.AddPeerAsync(d[1], d[3], d[2], d[0]);

                                // 1. Guardar no Vault Privado
                                _vaultPrivado[d[0]] = chaves.priv;

                                // 2. Perguntar APENAS se quer ver o QR Code
                                if (MessageBox.Show("Cliente criado! Queres ver o QR Code agora?", "Sucesso", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    var server = (await svc.WireGuard.GetInterfacesAsync()).First(i => i.Name == d[1]);

                                    // 3. Descobrir o IP de forma 100% invisível (sem diálogos)
                                    string ipDetectado = await svc.WireGuard.GetPublicEndpointAsync();

                                    // Fallback silencioso caso a API do MikroTik falhe
                                    if (ipDetectado == "TEU_IP_PUBLICO" || string.IsNullOrWhiteSpace(ipDetectado))
                                        ipDetectado = _current.Device.IpAddress;

                                    // 4. Montar a configuração e mostrar a imagem
                                    string config = WireGuardService.GenerateClientConfig(chaves.priv, d[2], server.PublicKey ?? "", $"{ipDetectado}:{server.ListenPort}");
                                    MostrarQRCode(config);
                                }
                            }
                        }
                        else // Adicionar SERVIDOR
                        {
                            using var d = new CrudDialog("Novo Servidor WireGuard", ("Nome", "wireguard1"), ("Porta", "13231"));
                            if (d.ShowDialog(this) == DialogResult.OK)
                                await svc.WireGuard.AddInterfaceAsync(d[0], d[1]);
                        }
                        break;
                }

                await LoadSectionAsync(_currentTag);
            }
            catch (Exception ex) { SetStatus($"❌ Erro: {ex.Message}", true); }
        }

        private async void BtnToggle_Click(object sender, EventArgs e)
        {
            if (_current == null || _dgvData.SelectedRows.Count == 0) return;

            var svc = _current.Services;
            var item = _dgvData.SelectedRows[0].DataBoundItem;
            if (item == null) return;
            string id = ((dynamic)item).Id ?? "";

            string val = ((dynamic)item).Disabled?.ToString().ToLower() ?? "false";
            bool isCurrentlyDisabled = val == "true" || val == "yes";

            try
            {
                SetStatus("A processar...");
                switch (_currentTag)
                {
                    case "iface": await svc.Interfaces.SetStateAsync(id, !isCurrentlyDisabled); break;
                    case "wifi": await svc.Wireless.SetStateAsync(id, !isCurrentlyDisabled); break;
                    case "ip": await svc.IpAddresses.SetStateAsync(id, !isCurrentlyDisabled); break;
                    case "route": await svc.Routes.SetStateAsync(id, !isCurrentlyDisabled); break;
                    case "bridge":
                        if (_showBridgePorts) await svc.Bridges.SetPortStateAsync(id, !isCurrentlyDisabled);
                        else await svc.Bridges.SetBridgeStateAsync(id, !isCurrentlyDisabled);
                        break;
                    case "dhcp":
                        if (_showDhcpClient) await svc.Dhcp.SetClientStateAsync(id, !isCurrentlyDisabled);
                        else await svc.Dhcp.SetServerStateAsync(id, !isCurrentlyDisabled);
                        break;
                }
                await LoadSectionAsync(_currentTag);
                SetStatus(isCurrentlyDisabled ? "🟢 Ativado" : "🔴 Desativado");
            }
            catch (Exception ex) { SetStatus($"❌ Erro: {ex.Message}", true); }
        }

        // ─── Apagar / Ação rápida ─────────────────────────────────────────────

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            var svc = _current.Services;

            try
            {
                if (_currentTag == "dns")
                {
                    var dns = await svc.Dns.GetSettingsAsync();

                    // Vê o estado atual (seja "yes" ou "true")
                    string currentAllow = dns.AllowRemote?.ToLower() ?? "no";

                    // Se estiver ligado ("yes" ou "true"), muda para "no". Senão, muda para "yes".
                    string novo = (currentAllow == "yes" || currentAllow == "true") ? "no" : "yes";

                    await svc.Dns.UpdateSettingsAsync(new Dictionary<string, string>
                    {
                        ["allow-remote-requests"] = novo
                    });

                    // Atualiza a barra de estado
                    SetStatus(novo == "yes" ? "🟢 DNS Remoto Ativado" : "🔴 DNS Remoto Desativado");
                    await LoadSectionAsync("dns");
                    return;
                }

                if (_dgvData.SelectedRows.Count == 0) return;

                var item = _dgvData.SelectedRows[0].DataBoundItem;
                if (item == null) return;
                string id = ((dynamic)item).Id ?? "";

                if (_currentTag == "wifi")
                {
                    if (_showWirelessProfiles)
                    {
                        if (MessageBox.Show("Remover perfil?", "Confirmar",
                            MessageBoxButtons.YesNo) == DialogResult.Yes)
                            await svc.Wireless.DeleteProfileAsync(id);
                    }
                    else
                    {
                        var wlan = (RouterWireless)item;
                        if (string.IsNullOrEmpty(wlan.MasterInterface))
                        {
                            bool estaDesativada = wlan.Disabled?.ToLower() == "true" ||
                                                  wlan.Disabled?.ToLower() == "yes";
                            await svc.Wireless.SetStateAsync(id, !estaDesativada);
                        }
                        else
                        {
                            if (MessageBox.Show("Remover interface virtual?", "Confirmar",
                                MessageBoxButtons.YesNo) == DialogResult.Yes)
                                await svc.Wireless.DeleteVirtualInterfaceAsync(id);
                        }
                    }
                }
                else
                {
                    if (MessageBox.Show("Apagar este registo?", "Confirmar",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

                    switch (_currentTag)
                    {
                        case "ip":
                            await svc.IpAddresses.DeleteAddressAsync(id); break;
                        case "bridge":
                            if (_showBridgePorts) await svc.Bridges.DeleteBridgePortAsync(id);
                            else await svc.Bridges.DeleteBridgeAsync(id);
                            break;
                        case "dhcp":
                            if (_showDhcpClient) await svc.Dhcp.DeleteClientAsync(id);
                            else await svc.Dhcp.DeleteServerAsync(id);
                            break;
                        case "route":
                            await svc.Routes.DeleteRouteAsync(id); break;
                        case "wg":
                            if (_showWgPeers) // 📱 SE A TUA GRELHA ESTIVER A MOSTRAR OS CLIENTES
                            {
                                // Ele vai usar APENAS o método DeletePeerAsync. 
                                // O servidor (Interface) fica 100% intacto a funcionar para os outros!
                                await svc.WireGuard.DeletePeerAsync(id);
                            }
                            else // 🖥️ SE A TUA GRELHA ESTIVER A MOSTRAR OS SERVIDORES
                            {
                                // Aqui sim, ele tenta apagar o servidor, mas primeiro...
                                if (MessageBox.Show("Atenção: Apagar a interface vai desligar todos os clientes associados a ela. Queres mesmo continuar?",
                                    "Apagar Servidor", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                                {
                                    await svc.WireGuard.DeleteInterfaceAsync(id);
                                }
                            }
                            break;
                        case "pool":
                            await svc.IpPools.DeleteAsync(id);
                            break;
                    }
                }

                await LoadSectionAsync(_currentTag);
                SetStatus("✅ Operação concluída.");
            }
            catch (Exception ex) { SetStatus($"❌ Erro: {ex.Message}", true); }
        }

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
                    case "wifi":
                        _dgvData.DataSource = _showWirelessProfiles
                                       ? await svc.Wireless.GetProfilesAsync()
                                       : await svc.Wireless.GetWirelessAsync(); break;
                    case "bridge":
                        _dgvData.DataSource = _showBridgePorts
                                       ? await svc.Bridges.GetPortsAsync()
                                       : await svc.Bridges.GetBridgesAsync(); break;
                    case "ip": _dgvData.DataSource = await svc.IpAddresses.GetAddressesAsync(); break;
                    case "route": _dgvData.DataSource = await svc.Routes.GetRoutesAsync(); break;
                    case "dhcp":
                        _dgvData.DataSource = _showDhcpClient
                                       ? await svc.Dhcp.GetClientsAsync()
                                       : await svc.Dhcp.GetServersAsync(); break;
                    case "pool":
                        _dgvData.DataSource = await svc.IpPools.GetAllAsync();
                        break;
                    case "dns": await LoadDnsAsync(svc); break;
                    case "wg": _dgvData.DataSource = _showWgPeers ? await svc.WireGuard.GetPeersAsync() : await svc.WireGuard.GetInterfacesAsync(); break;
                }

                FormatGridColumns(tag);
                SetStatus($"✅ {_dgvData.Rows.Count} registos carregados.");
            }
            catch (Exception ex) { SetStatus($"❌ {ex.Message}", true); }
        }

        private async Task LoadDnsAsync(RouterServices svc)
        {
            var dns = await svc.Dns.GetSettingsAsync();
            var dt = new DataTable();
            dt.Columns.Add("Propriedade");
            dt.Columns.Add("Valor");
            dt.Rows.Add("Servidores DNS", dns.Servers ?? "—");
            dt.Rows.Add("Pedidos Remotos", dns.AllowRemote ?? "—");
            dt.Rows.Add("Cache Size (KiB)", dns.CacheSize ?? "—");
            dt.Rows.Add("DoH Server", dns.UseDohServer ?? "—");
            dt.Rows.Add("Dynamic Servers", dns.DynamicServers ?? "—");
            _dgvData.DataSource = dt;
        }

        // ─── Diálogo DNS completo ─────────────────────────────────────────────

        private async Task ConfigurarDnsAsync()
        {
            var svc = _current.Services;
            SetStatus("A carregar definições DNS...");

            try
            {
                var dns = await svc.Dns.GetSettingsAsync();

                using var d = new CrudDialog("Configurações DNS",
                    ("Servers (ex: 8.8.8.8)", dns.Servers ?? ""),
                    ("DoH Server URL", dns.UseDohServer ?? ""),
                    ("Cache Size (KiB)", dns.CacheSize ?? "2048"),
                    ("Max UDP Packet Size", dns.MaxUdpPacketSize ?? "4096"),
                    ("Pedidos Remotos", dns.AllowRemote ?? "no"));

                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    // Lemos o que o utilizador escreveu e passamos tudo para minúsculas
                    string allowVal = d[4].Trim().ToLower();

                    // Aceita "yes" ou "true" para ligar. Qualquer outra coisa (como "no" ou "false") desliga.
                    string finalAllow = (allowVal == "true" || allowVal == "yes") ? "yes" : "no";

                    var updates = new Dictionary<string, string>
                    {
                        ["servers"] = d[0].Trim(),
                        ["use-doh-server"] = d[1].Trim(),
                        ["cache-size"] = string.IsNullOrWhiteSpace(d[2]) ? "2048" : d[2].Trim(),
                        ["max-udp-packet-size"] = string.IsNullOrWhiteSpace(d[3]) ? "4096" : d[3].Trim(),
                        ["allow-remote-requests"] = finalAllow // <--- Vai sempre enviar "yes" ou "no"
                    };

                    SetStatus("A guardar DNS...");
                    await svc.Dns.UpdateSettingsAsync(updates);

                    await LoadSectionAsync("dns");
                    SetStatus("✅ DNS atualizado com sucesso!");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"❌ Erro DNS: {ex.Message}", true);
                MessageBox.Show($"Erro ao guardar DNS:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Double-click para editar ─────────────────────────────────────────

        private async void DgvData_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var svc = _current.Services;
            var item = _dgvData.Rows[e.RowIndex].DataBoundItem;

            if (item == null) return;
            SetStatus("A carregar dados para edição...");

            try
            {
                switch (_currentTag)
                {
                    case "ip":
                        string idIp = ((dynamic)item).Id ?? "";
                        var ipItem = (RouterIpAddress)item;
                        var ifacesIP = await svc.Interfaces.GetAllAsync();
                        using (var d = new CrudDialog($"Editar IP: {ipItem.Address}",
                            ("Endereço/CIDR", ipItem.Address ?? ""),
                            ("Interface", ifacesIP.Select(i => i.Name).ToArray())))
                        {
                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                await svc.IpAddresses.UpdateAddressAsync(idIp, d[0], d[1]);
                                await LoadSectionAsync("ip");
                            }
                        }
                        break;

                    case "bridge":
                        string idBridge = ((dynamic)item).Id ?? "";
                        if (_showBridgePorts)
                        {
                            var portItem = (RouterBridgePort)item;
                            var bList = await svc.Bridges.GetBridgesAsync();
                            var iList = await svc.Interfaces.GetAllAsync();
                            using var d = new CrudDialog($"Editar Porta: {portItem.Interface}",
                                ("Interface", iList.Select(i => i.Name).ToArray()),
                                ("Bridge", bList.Select(b => b.Name).ToArray()),
                                ("PVID", portItem.Pvid ?? "1"));
                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                await svc.Bridges.UpdatePortAsync(idBridge, d[1], d[0], d[2]);
                                await LoadSectionAsync("bridge");
                            }
                        }
                        else
                        {
                            var brItem = (RouterBridge)item;
                            using var d = new CrudDialog($"Editar Bridge: {brItem.Name}",
                                ("Nome", brItem.Name ?? ""),
                                ("STP", new[] { "rstp", "stp", "mstp", "none" }));
                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                await svc.Bridges.UpdateBridgeAsync(idBridge, d[0], d[1]);
                                await LoadSectionAsync("bridge");
                            }
                        }
                        break;

                    case "wifi":
                        string idWifi = ((dynamic)item).Id ?? "";
                        if (_showWirelessProfiles)
                        {
                            var profile = (RouterSecProfile)item;
                            using var d = new CrudDialog($"Editar Perfil: {profile.Name}",
                                ("Nome", profile.Name ?? ""),
                                ("Auth Types", new[] { "wpa2-psk", "wpa-psk,wpa2-psk" }),
                                ("Ciphers", new[] { "aes-ccm", "tkip,aes-ccm" }),
                                ("Password", ""));
                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                await svc.Wireless.UpdateProfileAsync(idWifi, d[0], d[1], d[2], d[3]);
                                await LoadSectionAsync("wifi");
                            }
                        }
                        else
                        {
                            await EditarWirelessAsync((RouterWireless)item);
                        }
                        break;

                    case "route":
                        string idRoute = ((dynamic)item).Id ?? "";
                        var route = (RouterRoute)item;
                        using (var d = new CrudDialog($"Editar Rota: {route.DstAddress}",
                            ("Destino", route.DstAddress ?? "0.0.0.0/0"),
                            ("Gateway", route.Gateway ?? ""),
                            ("Distância", route.Distance ?? "1"),
                            ("Comment", route.Comment ?? "")))
                        {
                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                await svc.Routes.UpdateRouteAsync(idRoute, d[0], d[1], d[2], d[3]);
                                await LoadSectionAsync("route");
                            }
                        }
                        break;

                    case "dhcp":
                        string idDhcp = ((dynamic)item).Id ?? "";
                        var allIfaces = (await svc.Interfaces.GetAllAsync()).Select(i => i.Name).ToArray();
                        if (_showDhcpClient)
                        {
                            var c = (DhcpClientModel)item;
                            using var d = new CrudDialog("Editar DHCP Client",
                                ("Interface", allIfaces),
                                ("Usar DNS", new[] { "yes", "no" }),
                                ("Rota Def.", new[] { "yes", "no" }));
                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                await svc.Dhcp.UpdateClientAsync(idDhcp, d[0], d[1] == "yes", d[2] == "yes");
                                await LoadSectionAsync("dhcp");
                            }
                        }
                        else
                        {
                            var s = (RouterDhcp)item;
                            var pools = await svc.IpPools.GetAllAsync();
                            var poolReal = pools.FirstOrDefault(p => p.Name == s.AddressPool);
                            using var d = new CrudDialog("Editar DHCP Server",
                                ("Nome", s.Name ?? ""),
                                ("Interface", allIfaces),
                                ("Pool", pools.Select(p => p.Name).ToArray()),
                                ("IP Ranges", poolReal?.Ranges ?? ""));
                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                await svc.Dhcp.UpdateServerAsync(idDhcp, d[0], d[1], d[2]);
                                if (poolReal?.Id != null && d[3] != poolReal.Ranges)
                                    await svc.IpPools.UpdateAsync(poolReal.Id, poolReal.Name!, d[3]);
                                await LoadSectionAsync("dhcp");
                            }
                        }
                        break;

                    case "pool":
                        var poolItem = item as MikroTikSDN.Core.Models.IpPoolModel; // Garante que o nome do teu modelo é este
                        if (poolItem == null) return;
                        using (var d = new CrudDialog($"Editar Pool: {poolItem.Name}",
                            ("Nome", poolItem.Name ?? ""),
                            ("Ranges", poolItem.Ranges ?? "")))
                        {
                            if (d.ShowDialog(this) == DialogResult.OK)
                            {
                                await svc.IpPools.UpdateAsync(poolItem.Id!, d[0].Trim(), d[1].Trim());
                                await LoadSectionAsync("pool");
                            }
                        }
                        break;

                    case "dns":
                        await ConfigurarDnsAsync();
                        break;
                    case "wg":
                        if (_showWgPeers)
                        {
                            var peer = item as WireGuardPeerModel;
                            if (peer == null) return;
                            using var d = new CrudDialog($"Editar Peer: {peer.Comment}", ("Interface", (await svc.WireGuard.GetInterfacesAsync()).Select(i => i.Name).ToArray()), ("Public Key", peer.PublicKey ?? ""), ("Allowed Address", peer.AllowedAddress ?? ""), ("Comment", peer.Comment ?? ""));
                            if (d.ShowDialog(this) == DialogResult.OK) { await svc.WireGuard.UpdatePeerAsync(peer.Id!, d[0], d[1], d[2], d[3]); await LoadSectionAsync("wg"); }
                        }
                        else
                        {
                            var iface = item as WireGuardInterfaceModel;
                            if (iface == null) return;
                            using var d = new CrudDialog($"Editar Servidor: {iface.Name}", ("Nome", iface.Name ?? ""), ("Porta", iface.ListenPort ?? "13231"));
                            if (d.ShowDialog(this) == DialogResult.OK) { await svc.WireGuard.UpdateInterfaceAsync(iface.Id!, d[0], d[1]); await LoadSectionAsync("wg"); }
                        }
                        break;
                }

                SetStatus("✅ Alterações aplicadas!");
            }
            catch (Exception ex) { SetStatus($"❌ Erro ao editar: {ex.Message}", true); }
        }

        private async void BtnExport_Click(object sender, EventArgs e)
        {
            if (_dgvData.SelectedRows.Count == 0 || !_showWgPeers) return;
            var peer = _dgvData.SelectedRows[0].DataBoundItem as WireGuardPeerModel;
            if (peer == null) return;

            try
            {
                // Verifica a memória
                if (!_vaultPrivado.TryGetValue(peer.Comment ?? "", out string privKey ))
                {
                    MessageBox.Show("Chave privada não encontrada nesta sessão. Tem de criar um cliente novo para gerar o QR Code.", "Aviso");
                    return;
                }

                SetStatus("A montar configuração e gerar QR Code...");

                // 1. Obter o IP/DNS automaticamente
                string ipDetectado = await _current.Services.WireGuard.GetPublicEndpointAsync();
                if (ipDetectado == "TEU_IP_PUBLICO" || string.IsNullOrWhiteSpace(ipDetectado))
                    ipDetectado = _current.Device.IpAddress;

                var server = (await _current.Services.WireGuard.GetInterfacesAsync()).First(i => i.Name == peer.Interface);

                // 2. Gerar a string do ficheiro sem perguntar mais nada
                string config = WireGuardService.GenerateClientConfig(privKey, peer.AllowedAddress ?? "", server.PublicKey ?? "", $"{ipDetectado}:{server.ListenPort}");

                // 3. Guardar Ficheiro .conf (Opcional, mas útil para o PC)
                using (var sfd = new SaveFileDialog { Filter = "Config|*.conf", FileName = $"{peer.Comment}.conf" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        System.IO.File.WriteAllText(sfd.FileName, config);
                        SetStatus("✅ Ficheiro exportado com sucesso!");
                    }
                }

                // 4. Mostrar QR Code no ecrã imediatamente
                MostrarQRCode(config);
            }
            catch (Exception ex) { SetStatus($"Erro: {ex.Message}", true); }
        }

        private void MostrarQRCode(string texto)
        {
            using (var qrGen = new QRCodeGenerator())
            using (var qrData = qrGen.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrData))
            {
                var bitmap = qrCode.GetGraphic(20);
                Form f = new Form { Text = "Scan WireGuard VPN", Size = new Size(420, 480), StartPosition = FormStartPosition.CenterParent, BackColor = Color.White, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
                PictureBox pic = new PictureBox { Dock = DockStyle.Fill, Image = bitmap, SizeMode = PictureBoxSizeMode.Zoom, Padding = new Padding(25) };
                f.Controls.Add(pic);
                f.ShowDialog(this);
            }
        }
        // ─── Editar interface wireless (diálogo completo) ─────────────────────

        private async Task EditarWirelessAsync(RouterWireless wlan)
        {
            var svc = _current.Services;
            var profiles = await svc.Wireless.GetProfilesAsync();
            var profNames = profiles.Select(p => p.Name).DefaultIfEmpty("default").ToArray();

            using var d = new CrudDialog($"Wireless — {wlan.Name}",
                ("Mode", new[] { "ap-bridge", "station", "bridge", "station-bridge" }),
                ("Band", new[] { "2ghz-b/g/n", "5ghz-a/n/ac", "2ghz-onlyn", "5ghz-onlyac" }),
                ("Channel Width", new[] { "20mhz", "20/40mhz-XX", "20/40mhz-Ce", "20/40mhz-eC" }),
                ("Frequency", wlan.Frequency ?? "auto"),
                ("SSID", wlan.Ssid ?? "MikroTik"),
                ("Security Profile", profNames),
                ("Country", new[] { "portugal", "brazil", "spain", "etsi" }));

            if (d.ShowDialog(this) == DialogResult.OK)
            {
                await svc.Wireless.UpdateInterfaceAsync(wlan.Id!, new Dictionary<string, string>
                {
                    ["mode"] = d[0],
                    ["band"] = d[1],
                    ["channel-width"] = d[2],
                    ["frequency"] = d[3],
                    ["ssid"] = d[4],
                    ["security-profile"] = d[5],
                    ["country"] = d[6]
                });

                await LoadSectionAsync("wifi");
                SetStatus("✅ Interface Wireless atualizada!");
            }
        }

        // ─── Formatação das colunas ───────────────────────────────────────────

        private void DgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Verifica se estamos na coluna "Disabled" (o nome programático definido no teu Model)
            if (_dgvData.Columns[e.ColumnIndex].Name == "Disabled" && e.Value != null)
            {
                string val = e.Value.ToString().ToLower();

                if (val == "true" || val == "yes")
                {
                    e.Value = "🔴 Desativado";
                    e.CellStyle.ForeColor = Color.Red; // Opcional: muda a cor do texto para vermelho
                    e.FormattingApplied = true;
                }
                else if (val == "false" || val == "no")
                {
                    e.Value = "🟢 Ativo";
                    e.CellStyle.ForeColor = Color.Green; // Opcional: muda a cor do texto para verde
                    e.FormattingApplied = true;
                }
            }
        }

        private void FormatGridColumns(string tag)
        {
            if (_dgvData.Columns.Count == 0) return;
            if (_dgvData.Columns.Contains("Id")) _dgvData.Columns["Id"]!.Visible = false;
            if (_dgvData.Columns.Contains("Running")) _dgvData.Columns["Running"]!.Visible = false;

            var names = new Dictionary<string, string>
            {
                ["MacAddress"] = "MAC",
                ["Ssid"] = "Rede (SSID)",
                ["Address"] = "Endereço IP",
                ["Network"] = "Rede",
                ["Interface"] = "Interface",
                ["Running"] = "Ativo",
                ["Disabled"] = "Estado",
                ["DstAddress"] = "Destino",
                ["Gateway"] = "Gateway",
                ["Distance"] = "Distância",
                ["SecurityProfile"] = "Perfil Seg.",
                ["AddressPool"] = "Pool",
                ["MasterInterface"] = "Master",
                ["AuthTypes"] = "Auth Types",
                ["Ranges"] = "Gamas de IPs (Ranges)",
                // Novos para o WireGuard:
                ["AllowedAddress"] = "IP Permitido",
                ["PublicKey"] = "Chave Pública",
                ["ListenPort"] = "Porta de Escuta",
                ["Comment"] = "Descrição"
            };

            foreach (DataGridViewColumn col in _dgvData.Columns)
                if (names.TryGetValue(col.Name, out var n)) col.HeaderText = n;
        }

        // ─── Toolbar ──────────────────────────────────────────────────────────

        private void UpdateToolbarForSection(string tag)
        {
            _btnAdd.Visible = tag != "iface" && tag != "dns";
            _btnDelete.Visible = tag != "iface" && tag != "dns";
            _btnAction.Visible = tag is "bridge" or "wifi" or "dhcp" or "dns" or "wg";

            // As pools não se podem "desativar", por isso escondemos o botão Toggle
            _btnToggle.Visible = tag is "iface" or "wifi" or "bridge" or "ip" or "dhcp" or "route";

            if (_btnExport != null) _btnExport.Visible = (tag == "wg" && _showWgPeers);

            // MOSTRA O ATALHO SÓ NA PÁGINA DO DHCP
            if (_btnGoToPools != null) _btnGoToPools.Visible = (tag == "dhcp");

            if (tag == "wifi" && _showWirelessProfiles) _btnToggle.Visible = false;

            // Muda o texto do botão de Adicionar consoante a página
            _btnAdd.Text = tag switch
            {
                "wifi" => _showWirelessProfiles ? "➕ Perfil" : "➕ VAP",
                "bridge" => _showBridgePorts ? "➕ Porta" : "➕ Bridge",
                "dhcp" => _showDhcpClient ? "➕ Client" : "➕ Server",
                "wg" => _showWgPeers ? "➕ Cliente" : "➕ Servidor",
                "ip" => "➕ IP",
                "route" => "➕ Rota",
                "pool" => "➕ Pool", // <-- NOVO
                _ => "➕ Adicionar"
            };

            _btnDelete.Text = "🗑️ Remover";
            _btnToggle.Text = "⚡ Ativar/Desativar";

            if (tag == "dns") _btnAction.Text = "⚙️ Configurar DNS";
            else if (tag == "bridge") _btnAction.Text = _showBridgePorts ? "🌉 Bridges" : "🔌 Portas";
            else if (tag == "wifi") _btnAction.Text = _showWirelessProfiles ? "🌐 Wireless" : "🔐 Perfis";
            else if (tag == "dhcp") _btnAction.Text = _showDhcpClient ? "🖥️ Servers" : "🌐 Clients";
            else if (tag == "wg") _btnAction.Text = _showWgPeers ? "🖥️ Ver Servidores" : "📱 Ver Clientes";
        }

        // ─── Gestão de sessões e routers ──────────────────────────────────────

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
            foreach (var s in _sessions.Values)
                _cmbRouters.Items.Add($"{s.Device.Name} ({s.Device.IpAddress})");
            if (_cmbRouters.Items.Count > 0)
                _cmbRouters.SelectedIndex = _cmbRouters.Items.Count - 1;
        }

        private void SetStatus(string msg, bool error = false)
        {
            lblStatus.Text = msg;
            lblStatus.ForeColor = error ? Color.Salmon : Color.Gray;
        }

        private static string GetSectionTitle(string tag) => tag switch
        {
            "iface" => "Interfaces",
            "bridge" => "Bridge",
            "wifi" => "Wireless",
            "ip" => "Endereços IP",
            "dhcp" => "DHCP",
            "dns" => "DNS Settings",
            "route" => "Rotas Estáticas",
            "wg" => "WireGuard VPN",
            _ => "Rede"
        };
    }

    // ─── Classes de suporte ───────────────────────────────────────────────────

    public class RouterSession
    {
        public RouterClient Client { get; }
        public RouterDevice Device { get; }
        public RouterServices Services { get; }

        public RouterSession(RouterClient c, RouterDevice d)
        {
            Client = c;
            Device = d;
            Services = new RouterServices(c);
        }
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
        public WireGuardService WireGuard { get; } // <--- ADICIONADO AQUI

        public RouterServices(RouterClient c)
        {
            Interfaces = new InterfaceService(c);
            Wireless = new WirelessService(c);
            IpAddresses = new IpAddressService(c);
            Bridges = new BridgeService(c);
            Routes = new RouteService(c);
            Dhcp = new DhcpService(c);
            Dns = new DnsService(c);
            IpPools = new IpPoolService(c);
            WireGuard = new WireGuardService(c); // <--- INICIALIZADO AQUI
        }
    }
}