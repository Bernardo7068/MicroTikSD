using System.Drawing;
using System.Windows.Forms;

namespace MikroTikSDN.UI
{
    // A palavra "partial" diz ao C# que esta é a METADE visual do nosso formulário
    public partial class LoginForm : Form
    {
        // ─── VARIÁVEIS VISUAIS ───
        private TextBox txtName, txtIp, txtMac, txtUser, txtPass;
        private Button btnLogin, btnScan;
        private CheckBox chkSave;
        private ListBox lstSavedRouters, lstDiscoveredRouters;

        // ─── CONSTRUÇÃO DO DESIGN ───
        private void SetupUI()
        {
            var bgDark = Color.FromArgb(30, 30, 46);
            var bgSidebar = Color.FromArgb(25, 25, 35);
            var bgPanel = Color.FromArgb(37, 37, 55);
            var accent = Color.FromArgb(124, 106, 247);
            var textPrim = Color.FromArgb(232, 232, 240);

            this.Text = "Login - MikroTik SDN";
            this.Size = new Size(880, 580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bgDark;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // BARRA LATERAL
            var pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 380, BackColor = bgSidebar };

            var lblSavedTitle = new Label { Text = "🕒 Guardados (Nome | IP | MAC)", ForeColor = accent, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Location = new Point(10, 15), AutoSize = true };
            lstSavedRouters = new ListBox { Location = new Point(10, 40), Size = new Size(360, 125), BackColor = bgSidebar, ForeColor = textPrim, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9f), ItemHeight = 20, Cursor = Cursors.Hand };
            lstSavedRouters.SelectedIndexChanged += LstSavedRouters_SelectedIndexChanged;
            lstSavedRouters.DoubleClick += LstSavedRouters_DoubleClick;

            var lblDiscoveredTitle = new Label { Text = "📡 Rede Local (IP | MAC)", ForeColor = accent, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Location = new Point(10, 180), AutoSize = true };
            lstDiscoveredRouters = new ListBox { Location = new Point(10, 205), Size = new Size(360, 200), BackColor = bgSidebar, ForeColor = Color.MediumSeaGreen, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9.5f), ItemHeight = 21, Cursor = Cursors.Hand };
            lstDiscoveredRouters.SelectedIndexChanged += LstDiscoveredRouters_SelectedIndexChanged;

            btnScan = new Button { Text = "🔍 Procurar Dispositivos", Location = new Point(10, 435), Size = new Size(360, 35), BackColor = bgPanel, ForeColor = textPrim, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
            btnScan.FlatAppearance.BorderSize = 0;
            btnScan.Click += BtnScan_Click;

            pnlSidebar.Controls.AddRange(new Control[] { lblSavedTitle, lstSavedRouters, lblDiscoveredTitle, lstDiscoveredRouters, btnScan });
            this.Controls.Add(pnlSidebar);

            // ÁREA DE LOGIN
            int rightX = 430;

            var lblTitle = new Label { Text = "🌐 MikroTik SDN", ForeColor = accent, Font = new Font("Segoe UI", 16f, FontStyle.Bold), Location = new Point(rightX + 60, 30), AutoSize = true };
            var lblSub = new Label { Text = "Acesso ao Controlador", ForeColor = Color.Gray, Font = new Font("Segoe UI", 10f), Location = new Point(rightX + 85, 60), AutoSize = true };

            txtName = CreateInput("Nome do Equipamento", 120, bgPanel, textPrim, rightX);
            txtIp = CreateInput("Endereço IP", 185, bgPanel, textPrim, rightX);
            txtMac = CreateInput("MAC Address (Automático)", 250, bgDark, Color.Gray, rightX);
            txtMac.ReadOnly = true;
            txtMac.Text = "Desconhecido";

            txtUser = CreateInput("Utilizador", 315, bgPanel, textPrim, rightX);
            txtPass = CreateInput("Palavra-passe", 380, bgPanel, textPrim, rightX);
            txtPass.PasswordChar = '•';

            chkSave = new CheckBox { Text = "Guardar dispositivo", ForeColor = textPrim, Font = new Font("Segoe UI", 9f), Location = new Point(rightX, 430), AutoSize = true, Cursor = Cursors.Hand };

            btnLogin = new Button { Text = "LIGAR AO ROUTER", Location = new Point(rightX, 470), Size = new Size(350, 45), BackColor = accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Cursor = Cursors.Hand };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            this.Controls.AddRange(new Control[] { lblTitle, lblSub, txtName, txtIp, txtMac, txtUser, txtPass, chkSave, btnLogin });
        }

        private TextBox CreateInput(string placeholder, int yPos, Color bg, Color fg, int xPos)
        {
            var lbl = new Label { Text = placeholder, ForeColor = Color.Gray, Location = new Point(xPos, yPos - 20), AutoSize = true };
            var txt = new TextBox { Location = new Point(xPos, yPos), Size = new Size(350, 30), BackColor = bg, ForeColor = fg, Font = new Font("Segoe UI", 12f), BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(lbl);
            return txt;
        }
    }
}