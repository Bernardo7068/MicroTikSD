using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using MikroTikSDN.Core.Managers;
using MikroTikSDN.Core.Models;
using MikroTikSDN.Core.Services;

// CORRIGIDO: alias para evitar ambiguidade com System.Net.NetworkInformation.NetworkInterface
using NetInterface = System.Net.NetworkInformation.NetworkInterface;

namespace MikroTikSDN.UI
{
    public partial class LoginForm : Form
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);

        private readonly bool _isDialogMode;
        public RouterClient? AuthenticatedClient { get; private set; }
        public RouterDevice? AuthenticatedDevice { get; private set; }

        private readonly RouterManager _routerManager = new();
        private string _detectedMac = "Desconhecido";

#pragma warning disable CS8618 // O campo não anulável precisa conter um valor não nulo ao sair do construtor
        public LoginForm(bool isDialogMode = false)
        {
            _isDialogMode = isDialogMode;
            SetupUI();
            LoadSavedRouters();
        }
#pragma warning restore CS8618

        // ─── Lista de routers guardados ───────────────────────────────────────

        private void LoadSavedRouters()
        {
            lstSavedRouters.Items.Clear();
            foreach (var r in _routerManager.GetAllRouters())
                lstSavedRouters.Items.Add($"{r.Name} | {r.IpAddress} | {r.MacAddress}");
        }

        private void LstSavedRouters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSavedRouters.SelectedItem?.ToString() is not string item) return;

            var parts = item.Split(new[] { " | " }, StringSplitOptions.None);
            if (parts.Length < 2) return;

            string ip = parts[1].Trim();
            var router = _routerManager.GetAllRouters().FirstOrDefault(r => r.IpAddress == ip);
            if (router == null) return;

            txtName.Text = router.Name;
            txtIp.Text = router.IpAddress;
            txtMac.Text = router.MacAddress;
            txtUser.Text = router.Username;
            txtPass.Text = router.Password;
            _detectedMac = router.MacAddress;
        }

        private void LstSavedRouters_DoubleClick(object sender, EventArgs e)
        {
            if (lstSavedRouters.SelectedItem != null)
                BtnLogin_Click(this, EventArgs.Empty);
        }

        // ─── Lista de dispositivos descobertos ────────────────────────────────

        private void LstDiscoveredRouters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstDiscoveredRouters.SelectedItem?.ToString() is not string item) return;

            var parts = item.Split(new[] { " | " }, StringSplitOptions.None);
            if (parts.Length < 2) return;

            txtName.Text = "Novo Router";
            txtIp.Text = parts[0].Trim();
            txtMac.Text = parts[1].Trim();
            _detectedMac = parts[1].Trim();
            txtUser.Text = "admin";
            txtPass.Text = "";
            txtPass.Focus();
        }

        // ─── Scanner ARP ──────────────────────────────────────────────────────

        private async void BtnScan_Click(object sender, EventArgs e)
        {
            lstDiscoveredRouters.Items.Clear();

            string? localIp = GetLocalIPAddress();
            if (localIp == null)
            {
                MessageBox.Show("Não foi possível determinar o IP local.", "Erro de rede",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string subnetBase = localIp.Substring(0, localIp.LastIndexOf('.') + 1);
            btnScan.Text = $"A procurar em {subnetBase}0/24...";
            btnScan.Enabled = false;

            var tasks = new List<Task>();
            for (int i = 1; i < 255; i++)
            {
                string ip = subnetBase + i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using var ping = new Ping();
                        var reply = await ping.SendPingAsync(ip, 500);
                        if (reply.Status == IPStatus.Success && ip != localIp)
                        {
                            string mac = GetMacAddress(ip);
                            this.Invoke((MethodInvoker)(() =>
                                lstDiscoveredRouters.Items.Add($"{ip,-15} | {mac}")));
                        }
                    }
                    catch { }
                }));
            }

            await Task.WhenAll(tasks);
            btnScan.Text = "🔍 Procurar Dispositivos";
            btnScan.Enabled = true;
        }

        private static string GetMacAddress(string ipAddress)
        {
            try
            {
#pragma warning disable CS0618
                byte[] macAddr = new byte[6];
                uint macAddrLen = (uint)macAddr.Length;
                int dest = (int)IPAddress.Parse(ipAddress).Address;
                if (SendARP(dest, 0, macAddr, ref macAddrLen) != 0) return "MAC Oculto";
                string[] parts = new string[(int)macAddrLen];
                for (int i = 0; i < macAddrLen; i++) parts[i] = macAddr[i].ToString("X2");
                return string.Join(":", parts);
#pragma warning restore CS0618
            }
            catch { return "MAC Oculto"; }
        }

        // CORRIGIDO: usa alias NetInterface em vez de NetworkInterface
        private static string? GetLocalIPAddress()
        {
            foreach (var nic in NetInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) continue;
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                if (nic.GetIPProperties().GatewayAddresses.Count == 0) continue;

                foreach (var uni in nic.GetIPProperties().UnicastAddresses)
                    if (uni.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return uni.Address.ToString();
            }
            return null;
        }

        // ─── Login ────────────────────────────────────────────────────────────

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIp.Text))
            {
                MessageBox.Show("Introduz o endereço IP do router.", "Campo obrigatório",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnLogin.Text = "A LIGAR...";
            btnLogin.Enabled = false;

            string name = string.IsNullOrWhiteSpace(txtName.Text) ? "MikroTik" : txtName.Text.Trim();
            var device = new RouterDevice(name, txtIp.Text.Trim(), txtUser.Text.Trim(), txtPass.Text, _detectedMac);
            var client = new RouterClient(device);

            bool ok = await client.TestConnectionAsync();

            if (ok)
            {
                if (chkSave.Checked)
                {
                    // CORRIGIDO: usar RemoveAll direto na lista em vez de RemoveRouter
                    var all = _routerManager.GetAllRouters();
                    var existing = all.FirstOrDefault(r => r.IpAddress == device.IpAddress);
                    if (existing != null) all.Remove(existing);
                    _routerManager.AddRouter(device);
                }

                if (_isDialogMode)
                {
                    AuthenticatedClient = client;
                    AuthenticatedDevice = device;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    var dashboard = new MainForm(client, device);
                    dashboard.FormClosed += (s, args) => this.Close();
                    this.Hide();
                    dashboard.Show();
                }
            }
            else
            {
                MessageBox.Show(
                    "Não foi possível ligar ao router.\n\nVerifica:\n• O endereço IP\n• As credenciais\n• Se a API REST está ativa no RouterOS",
                    "Falha de ligação", MessageBoxButtons.OK, MessageBoxIcon.Error);

                btnLogin.Text = "LIGAR AO ROUTER";
                btnLogin.Enabled = true;
            }
        }
    }
}