using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MikroTikSDN.UI.Dialogs
{
    /// <summary>
    /// Diálogo genérico reutilizável para operações de criação (Add).
    /// Recebe uma lista de campos e devolve os valores preenchidos.
    /// Uso: new CrudDialog("Título", ("Campo1", "valorDefault"), ("Campo2", ""))
    /// </summary>
    public class CrudDialog : Form
    {
        private readonly Dictionary<string, TextBox> _fields = new();

        public string this[string fieldName] =>
            _fields.TryGetValue(fieldName, out var tb) ? tb.Text.Trim() : "";

        public CrudDialog(string title, params (string Label, string DefaultValue)[] fields)
        {
            var bgDark = Color.FromArgb(30, 30, 46);
            var bgInput = Color.FromArgb(46, 46, 69);
            var accent = Color.FromArgb(124, 106, 247);
            var textPrim = Color.FromArgb(232, 232, 240);
            var textMuted = Color.FromArgb(136, 136, 153);

            this.Text = title;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = bgDark;
            this.ForeColor = textPrim;
            this.Font = new Font("Segoe UI", 9.5f);

            int y = 16;

            var lblTitle = new Label
            {
                Text = title,
                ForeColor = textPrim,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(16, y),
                AutoSize = true
            };
            y += 36;

            var controls = new List<Control> { lblTitle };

            foreach (var (label, defaultValue) in fields)
            {
                var lbl = new Label
                {
                    Text = label,
                    ForeColor = textMuted,
                    Location = new Point(16, y),
                    AutoSize = true
                };
                y += 20;

                var txt = new TextBox
                {
                    Location = new Point(16, y),
                    Size = new Size(320, 28),
                    BackColor = bgInput,
                    ForeColor = textPrim,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 10f),
                    Text = defaultValue
                };
                y += 36;

                _fields[label] = txt;
                controls.Add(lbl);
                controls.Add(txt);
            }

            y += 8;

            var btnCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(16, y),
                Size = new Size(150, 34),
                BackColor = Color.FromArgb(58, 58, 85),
                ForeColor = textPrim,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            var btnOk = new Button
            {
                Text = "Confirmar",
                Location = new Point(184, y),
                Size = new Size(150, 34),
                BackColor = accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) => { this.DialogResult = DialogResult.OK; };

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            controls.Add(btnCancel);
            controls.Add(btnOk);
            this.Controls.AddRange(controls.ToArray());
            this.ClientSize = new Size(352, y + 50);
        }
    }
}