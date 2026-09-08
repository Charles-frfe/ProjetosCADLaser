using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;

namespace ProjetosCADLaser.Controls
{
    public sealed class EtapaRevisaoControl : UserControl
    {
        private readonly ListBox _anexos = new ListBox { Width = 180, Height = 70 };
        private readonly TextBox _titulo = new TextBox { Width = 180 };
        private readonly TextBox _categoria = new TextBox { Width = 140 };
        private readonly List<AnexoPendente> _pendentes = new List<AnexoPendente>();
        private AnexoService _servico; private string _sessao;
        public EtapaRevisaoControl() { AutoSize = true; Dock = DockStyle.Fill; var painel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true }; painel.Controls.Add(_anexos); painel.Controls.Add(new Label { Text = "Título", AutoSize = true }); painel.Controls.Add(_titulo); painel.Controls.Add(new Label { Text = "Categoria", AutoSize = true }); painel.Controls.Add(_categoria); var aplicar = new Button { Text = "Aplicar", AutoSize = true }; aplicar.Click += delegate { AplicarEdicao(); }; painel.Controls.Add(aplicar); var adicionar = new Button { Text = "Adicionar anexo", AutoSize = true }; adicionar.Click += delegate { Adicionar(); }; painel.Controls.Add(adicionar); var remover = new Button { Text = "Remover anexo", AutoSize = true }; remover.Click += delegate { Remover(); }; painel.Controls.Add(remover); Controls.Add(painel); _anexos.SelectedIndexChanged += delegate { Carregar(); }; }
        public IReadOnlyList<AnexoPendente> Pendentes { get { return _pendentes.AsReadOnly(); } }
        public void ConfigurarSessao(AnexoService servico, string sessao) { _servico = servico; _sessao = sessao; }
        private void Adicionar() { if (_servico == null) return; using (var dialogo = new OpenFileDialog { Multiselect = false, Title = "Selecionar anexo" }) if (dialogo.ShowDialog(this) == DialogResult.OK) try { var pendente = _servico.AdicionarArquivo(_sessao, dialogo.FileName, false); _pendentes.Add(pendente); _anexos.Items.Add(pendente.NomeOriginal); } catch (IOException) { } }
        private void Carregar() { if (_anexos.SelectedIndex < 0 || _anexos.SelectedIndex >= _pendentes.Count) return; var p = _pendentes[_anexos.SelectedIndex]; _titulo.Text = p.Titulo; _categoria.Text = p.Categoria; }
        private void AplicarEdicao() { if (_anexos.SelectedIndex < 0 || _anexos.SelectedIndex >= _pendentes.Count) return; var p = _pendentes[_anexos.SelectedIndex]; p.Titulo = _titulo.Text.Trim(); p.Categoria = _categoria.Text.Trim(); }
        private void Remover() { if (_anexos.SelectedIndex < 0 || _anexos.SelectedIndex >= _pendentes.Count) return; var i = _anexos.SelectedIndex; var p = _pendentes[i]; try { if (File.Exists(p.CaminhoTemporario)) File.Delete(p.CaminhoTemporario); } catch (IOException) { } _pendentes.RemoveAt(i); _anexos.Items.RemoveAt(i); _titulo.Clear(); _categoria.Clear(); }
    }
}
