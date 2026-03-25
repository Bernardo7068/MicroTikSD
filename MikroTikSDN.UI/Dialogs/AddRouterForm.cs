using System;
using System.Drawing;
using System.Windows.Forms;

namespace MikroTikSDN.UI.Dialogs
{
    public class AddRouterForm : Form
    {
        public string RouterHost     => txtHost.Text.Trim();
        public string RouterUser     => txtUser.Text.Trim();
        public string RouterPassword => txtPassword.Text;
        public string RouterName     => string.IsNullOrWhiteSpace(txtName.Text) ? RouterHost : txtName.Text.Trim();
        public bool   UseHttps       => chkHttps.Checked;

        private TextBox txtName, txtHost, txtUser, txtPassword;
        private CheckBox chkHttps;

        public AddRouterForm()
        {
            var bgDark   = Color.FromArgb(30,  30,  46);
            var bgInput  = Color.FromArgb(46,  46,  69);
            var accent   = Color.FromArgb(124, 106, 247);
            var textPrim = Color.FromArgb(232, 232, 240);
            var textMuted= Color.FromArgb(136, 136, 153);

            this.Text            = "Adicionar Router";
            this.Size            = new Size(360, 340);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = bgDark;
            this.ForeColor       = textPrim;
            this.Font            = new Font("Segoe UI", 9.5f);

            int y = 16;

            // Título
            var lblTitle = new Label { Text = "Novo Router", ForeColor = textPrim, Font = new Font("Segoe UI", 12f, FontStyle.Bold), Location = new Point(16, y), AutoSize = true };
            y += 40;

            // Nome
            var lblName = new Label { Text = "Nome (opcional)", ForeColor = textMuted, Location = new Point(16, y), AutoSize = true };
            y += 20;
            txtName = MakeTextBox(bgInput, textPrim, y); y += 36;

            // Host
            var lblHost = new Label { Text = "IP / Hostname *", ForeColor = textMuted, Location = new Point(16, y), AutoSize = true };
            y += 20;
            txtHost = MakeTextBox(bgInput, textPrim, y); y += 36;

            // User
            var lblUser = new Label { Text = "Utilizador *", ForeColor = textMuted, Location = new Point(16, y), AutoSize = true };
            y += 20;
            txtUser = MakeTextBox(bgInput, textPrim, y);
            txtUser.Text = "admin";
            y += 36;

            // Password
            var lblPass = new Label { Text = "Password", ForeColor = textMuted, Location = new Point(16, y), AutoSize = true };
            y += 20;
            txtPassword = MakeTextBox(bgInput, textPrim, y);
            txtPassword.UseSystemPasswordChar = true;
            y += 36;

            // HTTPS
            chkHttps = new CheckBox { Text = "Usar HTTPS (porta 443)", ForeColor = textPrim, Location = new Point(16, y), AutoSize = true };
            y += 32;

            // Botões
            var btnCancel = new Button
            {
                Text      = "Cancelar",
                Location  = new Point(16, y),
                Size      = new Size(148, 34),
                BackColor = Color.FromArgb(58, 58, 85),
                ForeColor = textPrim,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Cursor    = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            var btnOk = new Button
            {
                Text      = "Ligar",
                Location  = new Point(176, y),
                Size      = new Size(148, 34),
                BackColor = accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(RouterHost))
                { MessageBox.Show("Introduz o IP ou hostname.", "Campo obrigatório", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(RouterUser))
                { MessageBox.Show("Introduz o utilizador.", "Campo obrigatório", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                this.DialogResult = DialogResult.OK;
            };

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblName, txtName,
                lblHost, txtHost,
                lblUser, txtUser,
                lblPass, txtPassword,
                chkHttps,
                btnCancel, btnOk
            });

            this.ClientSize = new Size(340, y + 50);
        }

        private TextBox MakeTextBox(Color bg, Color fg, int y)
        {
            var txt = new TextBox
            {
                Location  = new Point(16, y),
                Size      = new Size(308, 28),
                BackColor = bg,
                ForeColor = fg,
                BorderStyle = BorderStyle.FixedSingle,
                Font      = new Font("Segoe UI", 10f)
            };
            return txt;
        }
    }
}
