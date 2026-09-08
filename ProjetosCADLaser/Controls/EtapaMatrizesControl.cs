using System;
using System.Windows.Forms;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Controls
{
    public sealed class EtapaMatrizesControl : UserControl
    {
        private readonly ComboBox _tipo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        private readonly ComboBox _material = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private readonly ComboBox _eixos = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
        private readonly ComboBox _maquina = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
        private readonly ComboBox _acabamento = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
        public EtapaMatrizesControl()
        {
            AutoSize = true; Dock = DockStyle.Fill; _tipo.Items.AddRange(new object[] { "Gravação", "Tampa", "Laterais" }); _tipo.SelectedIndex = 0; _material.Items.AddRange(Enum.GetNames(typeof(MaterialMatriz))); _material.SelectedIndex = 0; _eixos.Items.AddRange(Enum.GetNames(typeof(QuantidadeEixos))); _eixos.SelectedIndex = 0; _maquina.Items.AddRange(Enum.GetNames(typeof(Maquina))); _maquina.SelectedIndex = 0; _acabamento.Items.AddRange(Enum.GetNames(typeof(Acabamento))); _acabamento.SelectedIndex = 0;
            var linha = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill }; linha.Controls.Add(_tipo); linha.Controls.Add(_material); linha.Controls.Add(_eixos); linha.Controls.Add(_maquina); linha.Controls.Add(_acabamento); Controls.Add(linha);
            _tipo.SelectedIndexChanged += Alterado; _material.SelectedIndexChanged += Alterado; _eixos.SelectedIndexChanged += Alterado; _maquina.SelectedIndexChanged += Alterado; _acabamento.SelectedIndexChanged += Alterado;
        }
        public string Tipo { get { return _tipo.Text; } } public string Material { get { return _material.Text; } } public string Eixos { get { return _eixos.Text; } } public string Maquina { get { return _maquina.Text; } } public string Acabamento { get { return _acabamento.Text; } }
        public event EventHandler ConfiguracaoAlterada;
        private void Alterado(object sender, EventArgs e) { if (ConfiguracaoAlterada != null) ConfiguracaoAlterada(this, EventArgs.Empty); }
    }
}
