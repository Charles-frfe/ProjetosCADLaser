using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;

namespace ProjetosCADLaser.Forms
{
    public sealed partial class PesquisaForm : Form
    {
        private readonly AppServices _servicos; private readonly ConfiguracaoLocal _local;
        private readonly TextBox _busca = new TextBox { Width = 360 }; private readonly ListBox _resultados = new ListBox { Dock = DockStyle.Fill };
        private readonly Label _status = new Label { AutoSize = true }; private System.Collections.Generic.List<PilotoCadastro> _pilotos = new System.Collections.Generic.List<PilotoCadastro>();
        public PesquisaForm(AppServices servicos, ConfiguracaoLocal local) { _servicos = servicos; _local = local; Text = "Pesquisar — Projetos CAD/LASER"; StartPosition = FormStartPosition.CenterParent; Size = new Size(900, 620); var topo = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(16), WrapContents = false }; topo.Controls.Add(new Label { Text = "Pesquisar projetos", AutoSize = true, Margin = new Padding(0, 7, 12, 0) }); topo.Controls.Add(_busca); var atualizar = new Button { Text = "Atualizar", AutoSize = true }; atualizar.Click += delegate { Carregar(); }; topo.Controls.Add(atualizar); var testes = new Button { Text = "Testes de textura", AutoSize = true }; testes.Click += delegate { using (var form = new PesquisaTestesForm(_servicos, _local)) form.ShowDialog(this); }; topo.Controls.Add(testes); topo.Controls.Add(_status); Controls.Add(_resultados); Controls.Add(topo); _busca.TextChanged += delegate { Renderizar(); }; _resultados.DoubleClick += delegate { AbrirSelecionado(); }; Shown += delegate { Carregar(); }; _servicos.Tema.Aplicar(this); }
        private void Carregar() { if (string.IsNullOrWhiteSpace(_local.PastaRaizDados)) return; var resultado = _servicos.PesquisaPilotos.Carregar(_local.PastaRaizDados); _pilotos = resultado.Pilotos.ToList(); _status.Text = _pilotos.Count + " piloto(s)" + (resultado.Invalidos == 0 ? string.Empty : "; " + resultado.Invalidos + " inválido(s)"); Renderizar(); }
        private void Renderizar() { _resultados.Items.Clear(); foreach (var piloto in _servicos.PesquisaPilotos.Filtrar(_pilotos, _busca.Text)) _resultados.Items.Add(piloto.Codigo + " — " + piloto.NomeModelo + " · " + (piloto.Status == StatusPiloto.Cancelada ? "Cancelada" : "Ativa")); }
        private void AbrirSelecionado() { var filtrados = _servicos.PesquisaPilotos.Filtrar(_pilotos, _busca.Text).ToList(); if (_resultados.SelectedIndex < 0 || _resultados.SelectedIndex >= filtrados.Count) return; using (var ficha = new FichaTecnicaForm(_servicos, _local, filtrados[_resultados.SelectedIndex])) ficha.ShowDialog(this); }
    }
}
