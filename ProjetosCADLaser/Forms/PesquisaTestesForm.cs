using System.Drawing;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;

namespace ProjetosCADLaser.Forms
{
    public sealed partial class PesquisaTestesForm : Form
    {
        private readonly AppServices _servicos; private readonly ConfiguracaoLocal _local; private readonly TextBox _busca = new TextBox { Width = 320 }; private readonly ListBox _lista = new ListBox { Dock = DockStyle.Fill }; private System.Collections.Generic.List<TesteTexturaCadastro> _testes = new System.Collections.Generic.List<TesteTexturaCadastro>();
        public PesquisaTestesForm(AppServices servicos, ConfiguracaoLocal local) { _servicos = servicos; _local = local; Text = "Pesquisar testes de textura"; Size = new Size(820, 560); var topo = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(14) }; topo.Controls.Add(new Label { Text = "Pesquisar", AutoSize = true, Margin = new Padding(0, 6, 10, 0) }); topo.Controls.Add(_busca); Controls.Add(_lista); Controls.Add(topo); _busca.TextChanged += delegate { Carregar(); }; _lista.DoubleClick += delegate { Abrir(); }; Shown += delegate { Carregar(); }; servicos.Tema.Aplicar(this); }
        private void Carregar() { _lista.Items.Clear(); _testes = string.IsNullOrWhiteSpace(_local.PastaRaizDados) ? new System.Collections.Generic.List<TesteTexturaCadastro>() : new System.Collections.Generic.List<TesteTexturaCadastro>(_servicos.TestesTextura.Pesquisar(_local.PastaRaizDados, _busca.Text)); foreach (var teste in _testes) _lista.Items.Add(teste.Identificacao + " · " + TesteTexturaService.Situacao(teste.Status)); }
        private void Abrir() { if (_lista.SelectedIndex < 0 || _lista.SelectedIndex >= _testes.Count) return; using (var ficha = new FichaTesteTexturaForm(_servicos, _local, _testes[_lista.SelectedIndex])) ficha.ShowDialog(this); }
    }
}
