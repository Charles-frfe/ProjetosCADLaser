using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjetosCADLaser.Controls
{
    public sealed class EtapaOrigemControl : UserControl
    {
        private readonly TextBox _pasta = new TextBox { ReadOnly = true, Width = 420 };
        private readonly Button _selecionar = new Button { Text = "Selecionar...", AutoSize = true };
        private readonly Button _analisar = new Button { Text = "Analisar pasta", AutoSize = true };
        public EtapaOrigemControl() { AutoSize = true; Dock = DockStyle.Fill; var linha = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = false }; linha.Controls.Add(_pasta); linha.Controls.Add(_selecionar); linha.Controls.Add(_analisar); Controls.Add(linha); _selecionar.Click += delegate { using (var dialogo = new FolderBrowserDialog { Description = "Selecione a pasta de origem" }) if (dialogo.ShowDialog(this) == DialogResult.OK) Pasta = dialogo.SelectedPath; }; _analisar.Click += delegate { if (AnalisarSolicitado != null) AnalisarSolicitado(this, EventArgs.Empty); }; }
        public string Pasta { get { return _pasta.Text; } set { _pasta.Text = value ?? string.Empty; } }
        public event EventHandler AnalisarSolicitado;
    }
}
