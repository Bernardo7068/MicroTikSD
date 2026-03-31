using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MikroTikSDN.UI.Dialogs
{
    public class CrudDialog : Form
    {
        private readonly Dictionary<string, Control> _fields = new();
        private readonly List<string> _orderedLabels = new();

        // Indexador para ler os valores: d[0], d[1]...
        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= _orderedLabels.Count) return "";
                var ctrl = _fields[_orderedLabels[index]];

                if (ctrl is ComboBox cb)
                    return cb.SelectedItem?.ToString() ?? "";

                return ctrl.Text.Trim();
            }
        }

        // Mudámos o parâmetro para 'object' para aceitar String ou String[]
        public CrudDialog(string title, params (string Label, object ValueOrOptions)[] fields)
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

            int y = 20;
            var lblHeader = new Label { Text = title, Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(16, y), AutoSize = true };
            this.Controls.Add(lblHeader);
            y += 40;

            foreach (var (label, val) in fields)
            {
                _orderedLabels.Add(label);
                var lbl = new Label { Text = label, Location = new Point(16, y), AutoSize = true, ForeColor = textMuted };
                y += 20;

                Control input;
                if (val is string[] options)
                {
                    // Cria Dropdown (ComboBox)
                    var cb = new ComboBox
                    {
                        Location = new Point(16, y),
                        Width = 320,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        BackColor = bgInput,
                        ForeColor = textPrim,
                        FlatStyle = FlatStyle.Flat
                    };
                    cb.Items.AddRange(options);
                    if (cb.Items.Count > 0) cb.SelectedIndex = 0;
                    input = cb;
                }
                else
                {
                    // Cria Caixa de Texto (TextBox)
                    input = new TextBox
                    {
                        Location = new Point(16, y),
                        Width = 320,
                        Text = val?.ToString() ?? "",
                        BackColor = bgInput,
                        ForeColor = textPrim,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                }

                _fields[label] = input;
                this.Controls.Add(lbl);
                this.Controls.Add(input);
                y += 40;
            }

            y += 10;
            var btnCancel = new Button { Text = "Cancelar", Location = new Point(16, y), Size = new Size(150, 34), BackColor = Color.FromArgb(58, 58, 85), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };
            var btnOk = new Button { Text = "Confirmar", Location = new Point(184, y), Size = new Size(150, 34), BackColor = accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            btnOk.Click += (s, e) => { this.DialogResult = DialogResult.OK; };

            this.Controls.Add(btnCancel);
            this.Controls.Add(btnOk);
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            this.ClientSize = new Size(352, y + 50);
        }
    }
}