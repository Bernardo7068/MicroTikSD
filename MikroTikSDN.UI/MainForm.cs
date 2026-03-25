using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MikroTikSDN.Core;

namespace MikroTikSDN.UI
{
    public partial class MainForm : Form
    {
        private readonly List<RouterManager> _routers = new();
        private RouterManager? _selectedRouter;

        // Painéis de conteúdo (carregados a pedido)
        private Panel _currentPanel = new();

        public MainForm()
        {
            InitializeComponent();
            lstRouters.DisplayMember = "Name";
            lstRouters.DataSource = _routers;
        }

        // ─── Eventos de navegação ─────────────────────────────────────────

        private void lstRouters_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedRouter = lstRouters.SelectedItem as RouterManager;
            if (_selectedRouter != null)
            {
                lblRouterInfo.Text = $"{_selectedRouter.Name} ({_selectedRouter.Host})";
                LoadSection(btnInterfaces);
            }
        }

        private void NavButton_Click(object sender, EventArgs e)
        {
            if (_selectedRouter == null)
            {
                MessageBox.Show("Seleciona um router primeiro.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            LoadSection((Button)sender);
        }

        private void LoadSection(Button btn)
        {
            // Highlight botão ativo
            foreach (Control c in pnlNav.Controls)
                if (c is Button b) b.BackColor = Color.FromArgb(37, 37, 55);

            btn.BackColor = Color.FromArgb(58, 58, 85);

            // Carregar painel correspondente
            UserControl panel = btn.Tag?.ToString() switch
            {
                "interfaces"  => new Panels.InterfacesPanel(_selectedRouter!),
                "bridges"     => new Panels.BridgesPanel(_selectedRouter!),
                "wireless"    => new Panels.WirelessPanel(_selectedRouter!),
                "ipaddresses" => new Panels.IpAddressesPanel(_selectedRouter!),
                "routes"      => new Panels.RoutesPanel(_selectedRouter!),
                "dhcp"        => new Panels.DhcpPanel(_selectedRouter!),
                "dns"         => new Panels.DnsPanel(_selectedRouter!),
                _             => new Panels.InterfacesPanel(_selectedRouter!)
            };

            pnlContent.Controls.Clear();
            panel.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(panel);
            lblSectionTitle.Text = btn.Text.Trim();
        }

        // ─── Adicionar router ─────────────────────────────────────────────

        private async void btnAddRouter_Click(object sender, EventArgs e)
        {
            using var dlg = new Dialogs.AddRouterForm();
            if (dlg.ShowDialog() != DialogResult.OK) return;

            SetStatus("A ligar ao router...");

            var router = new RouterManager(
                dlg.RouterHost,
                dlg.RouterUser,
                dlg.RouterPassword,
                dlg.UseHttps,
                dlg.RouterName);

            var ok = await router.TestConnectionAsync();
            if (!ok)
            {
                SetStatus("Erro: não foi possível ligar.");
                MessageBox.Show(
                    "Não foi possível ligar ao router.\n\nVerifica o IP, credenciais e se a API REST está ativa.",
                    "Erro de ligação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var identity = await router.System.GetIdentityAsync();
                if (!string.IsNullOrWhiteSpace(identity?.Name))
                    router.Name = identity.Name;
            }
            catch { }

            _routers.Add(router);

            // Forçar atualização da ListBox
            lstRouters.DataSource = null;
            lstRouters.DataSource = _routers;
            lstRouters.DisplayMember = "Name";
            lstRouters.SelectedItem = router;

            SetStatus($"Ligado a {router.Name} ({router.Host})");
        }

        // ─── Helper ───────────────────────────────────────────────────────

        public void SetStatus(string msg) => lblStatus.Text = msg;
    }
}
