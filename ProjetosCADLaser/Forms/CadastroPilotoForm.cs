using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;
using ProjetosCADLaser.Controls;

namespace ProjetosCADLaser.Forms
{
    public sealed partial class CadastroPilotoForm : Form
    {
        private readonly AppServices _servicos;
        private readonly ConfiguracaoLocal _local;
        private readonly TextBox _codigo = new TextBox { Width = 220 };
        private readonly TextBox _nome = new TextBox { Width = 420 };
        private readonly TextBox _origem = new TextBox { ReadOnly = true, Width = 420 };
        private readonly Label _status = new Label { AutoSize = true };
        private readonly ListBox _detectados = new ListBox { Dock = DockStyle.Fill, Height = 100 };
        private readonly CheckedListBox _componentes = new CheckedListBox { Dock = DockStyle.Fill, Height = 80, CheckOnClick = true };
        private readonly CheckedListBox _texturas = new CheckedListBox { Dock = DockStyle.Fill, Height = 80, CheckOnClick = true };
        private readonly ComboBox _tipoMatriz = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        private readonly ComboBox _material = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
        private readonly ComboBox _eixos = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
        private readonly ComboBox _maquina = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
        private readonly ComboBox _acabamento = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
        private readonly Label _matrizResumo = new Label { AutoSize = true, ForeColor = Color.DimGray };
        private readonly ListBox _anexos = new ListBox { Dock = DockStyle.Fill, Height = 70 };
        private readonly TextBox _tituloAnexo = new TextBox { Width = 180 };
        private readonly TextBox _categoriaAnexo = new TextBox { Width = 140 };
        private readonly System.Collections.Generic.List<AnexoPendente> _pendentes = new System.Collections.Generic.List<AnexoPendente>();
        private string _sessaoAnexos;
        private RegistroBloqueio _bloqueio;
        private string _caminhoBloqueio;
        private readonly Timer _heartbeat = new Timer { Interval = 30000 };
        private readonly AnalisadorPastasService _analisador = new AnalisadorPastasService();
        private readonly Button _analisar = new Button { Text = "Analisar pasta", AutoSize = true };
        private readonly EtapaDeteccaoControl _etapaDeteccao = new EtapaDeteccaoControl();
        private readonly EtapaMatrizesControl _etapaMatrizes = new EtapaMatrizesControl();
        private EtapaRevisaoControl _etapaRevisao;

        public CadastroPilotoForm(AppServices servicos, ConfiguracaoLocal local)
        {
            InitializeComponent();
            _servicos = servicos; _local = local; _sessaoAnexos = _servicos.Anexos.CriarSessao(); _etapaRevisao = new EtapaRevisaoControl(); _etapaRevisao.ConfigurarSessao(_servicos.Anexos, _sessaoAnexos);
            _heartbeat.Tick += delegate { if (_bloqueio != null && !string.IsNullOrWhiteSpace(_caminhoBloqueio)) try { _servicos.Bloqueios.Atualizar(_caminhoBloqueio, _bloqueio); } catch { } };
            FormClosed += delegate { _heartbeat.Stop(); if (_bloqueio != null && !string.IsNullOrWhiteSpace(_caminhoBloqueio)) try { _servicos.Bloqueios.Remover(_caminhoBloqueio, _bloqueio); } catch { } _bloqueio = null; try { _servicos.Anexos.LimparSessao(_sessaoAnexos); } catch { } };
            if (local == null || string.IsNullOrWhiteSpace(local.PastaRaizDados)) throw new InvalidOperationException("A pasta raiz não está configurada.");
            Text = "Cadastrar piloto — Projetos CAD/LASER"; StartPosition = FormStartPosition.CenterParent; Size = new Size(760, 460); MinimumSize = new Size(700, 400); Font = new Font("Segoe UI", 10F);
            _tipoMatriz.Items.AddRange(new object[] { "Gravação", "Tampa", "Laterais" }); _tipoMatriz.SelectedIndex = 0;
            _material.Items.AddRange(Enum.GetNames(typeof(MaterialMatriz))); _material.SelectedIndex = 0;
            _eixos.Items.AddRange(Enum.GetNames(typeof(QuantidadeEixos))); _eixos.SelectedIndex = 0;
            _maquina.Items.AddRange(Enum.GetNames(typeof(Maquina))); _maquina.SelectedIndex = 0;
            _acabamento.Items.AddRange(Enum.GetNames(typeof(Acabamento))); _acabamento.SelectedIndex = 0;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(30), ColumnCount = 2, RowCount = 11 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(new Label { Text = "Código do modelo", AutoSize = true }, 0, 0); layout.Controls.Add(_codigo, 1, 0);
            layout.Controls.Add(new Label { Text = "Nome do modelo", AutoSize = true }, 0, 1); layout.Controls.Add(_nome, 1, 1);
            layout.Controls.Add(new Label { Text = "Pasta de origem", AutoSize = true }, 0, 2);
            var etapaOrigem = new EtapaOrigemControl(); etapaOrigem.AnalisarSolicitado += delegate { _origem.Text = etapaOrigem.Pasta; AnalisarPasta(); }; layout.Controls.Add(etapaOrigem, 1, 2);
            layout.Controls.Add(new Label { Text = "A análise lê somente nomes de pastas; o cadastro básico ainda será salvo sem anexos, componentes ou matrizes.", AutoSize = true, ForeColor = Color.DimGray }, 0, 3); layout.SetColumnSpan(layout.GetControlFromPosition(0, 3), 2);
            layout.Controls.Add(_status, 0, 4); layout.SetColumnSpan(_status, 2);
            layout.Controls.Add(_etapaDeteccao, 0, 5); layout.SetColumnSpan(_etapaDeteccao, 2);
            layout.Controls.Add(new Label { Text = "Matriz básica", AutoSize = true }, 0, 8); layout.Controls.Add(_etapaMatrizes, 1, 8);
            layout.Controls.Add(_matrizResumo, 1, 9);
            layout.Controls.Add(new Label { Text = "Anexos pendentes", AutoSize = true }, 0, 10); layout.Controls.Add(_etapaRevisao, 1, 10); _etapaMatrizes.ConfiguracaoAlterada += delegate { AtualizarResumoMatriz(); };
            var salvar = new Button { Text = "Salvar cadastro", AutoSize = true, Height = 38 }; salvar.Click += delegate { Salvar(); }; var cancelar = new Button { Text = "Cancelar", AutoSize = true, Height = 38 }; cancelar.Click += delegate { Close(); };
            var selecionarAnexo = new Button { Text = "Adicionar anexo", AutoSize = true, Visible = false }; var removerAnexo = new Button { Text = "Remover anexo", AutoSize = true, Visible = false };
            var botoes = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill }; botoes.Controls.Add(cancelar); botoes.Controls.Add(salvar); layout.Controls.Add(botoes, 0, 12); layout.SetColumnSpan(botoes, 2); Controls.Add(layout); _tipoMatriz.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); }; _material.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); }; _eixos.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); }; _maquina.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); }; _acabamento.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); }; AtualizarResumoMatriz(); _servicos.Tema.Aplicar(this);
        }

        private void Salvar()
        {
            RegistroBloqueio dono = null;
            try
            {
                _caminhoBloqueio = _servicos.Bloqueios.ObterCaminho(_local.PastaRaizDados, _codigo.Text.Trim());
                dono = _servicos.Bloqueios.CriarPorCodigo(_local.PastaRaizDados, _codigo.Text.Trim(), Guid.NewGuid(), "Cadastro", _local.NomeExibido);
                _bloqueio = dono;
                _heartbeat.Start();
                var cadastro = new PilotoCadastro { Codigo = _codigo.Text.Trim(), NomeModelo = _nome.Text.Trim(), PastaOrigem = _origem.Text.Trim() };
                foreach (var item in _etapaDeteccao.ComponentesSelecionados) cadastro.Componentes.Add(new ComponenteCadastro { Nome = item });
                foreach (var item in _etapaDeteccao.TexturasSelecionadas) cadastro.Texturas.Add(new TexturaCadastro { Nome = item, CaminhoDetectado = Path.Combine(_origem.Text, item), CaminhoRelativo = item, Selecionada = true, Confirmada = true });
                if (cadastro.Componentes.Count > 0)
                {
                    var matrizBase = new MatrizCadastro { Tipo = _etapaMatrizes.Tipo, Material = (MaterialMatriz)Enum.Parse(typeof(MaterialMatriz), _etapaMatrizes.Material), Eixos = (QuantidadeEixos)Enum.Parse(typeof(QuantidadeEixos), _etapaMatrizes.Eixos), Maquina = (Maquina)Enum.Parse(typeof(Maquina), _etapaMatrizes.Maquina), Acabamento = (Acabamento)Enum.Parse(typeof(Acabamento), _etapaMatrizes.Acabamento) };
                    foreach (var textura in cadastro.Texturas) matrizBase.TexturasIds.Add(textura.Id);
                    if (string.Equals(matrizBase.Tipo, "Laterais", StringComparison.OrdinalIgnoreCase)) cadastro.Componentes[0].Matrizes.AddRange(_servicos.Matrizes.CriarGrupoLaterais(matrizBase)); else cadastro.Componentes[0].Matrizes.Add(matrizBase);
                }
                var salvo = _servicos.CadastroPiloto.Salvar(_local.PastaRaizDados, cadastro, _etapaRevisao.Pendentes, new[] { "Gravação", "Tampa", "Laterais" });
                _status.ForeColor = Color.SeaGreen; _status.Text = "Cadastro salvo: " + salvo.Codigo; DialogResult = DialogResult.OK;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidDataException || ex is InvalidOperationException || ex is IOException) { _servicos.Log.Registrar(ex, "Falha no cadastro básico", _local.PastaRaizDados); _status.ForeColor = Color.Firebrick; _status.Text = ex.Message; }
            finally { _heartbeat.Stop(); if (dono != null && !string.IsNullOrWhiteSpace(_caminhoBloqueio)) try { _servicos.Bloqueios.Remover(_caminhoBloqueio, dono); } catch { } _bloqueio = null; }
        }

        private void AtualizarResumoMatriz() { _matrizResumo.Text = "Revisão: " + _etapaMatrizes.Tipo + " · " + _etapaMatrizes.Material + " · " + _etapaMatrizes.Eixos + " eixos · " + _etapaMatrizes.Maquina + " · " + _etapaMatrizes.Acabamento; }

        private void AnalisarPasta()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_origem.Text)) throw new DirectoryNotFoundException("Selecione a pasta de origem antes de analisar.");
                var analise = _analisador.Analisar(_origem.Text, new ConfiguracaoCompartilhada().Componentes);
                _etapaDeteccao.AplicarAnalise(analise);
                if (string.IsNullOrWhiteSpace(_codigo.Text)) _codigo.Text = analise.CodigoSugerido;
                if (string.IsNullOrWhiteSpace(_nome.Text)) _nome.Text = analise.NomeModeloSugerido;
                _status.ForeColor = Color.SeaGreen; _status.Text = "Análise concluída: " + analise.Componentes.Count + " componente(s), " + analise.Texturas.Count + " textura(s), " + analise.NaoClassificadas.Count + " não classificada(s).";
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is IOException) { _status.ForeColor = Color.Firebrick; _status.Text = ex.Message; }
        }

        private void AdicionarAnexo() { using (var dialogo = new OpenFileDialog { Multiselect = false, Title = "Selecionar anexo" }) if (dialogo.ShowDialog(this) == DialogResult.OK) try { var pendente = _servicos.Anexos.AdicionarArquivo(_sessaoAnexos, dialogo.FileName, false); _pendentes.Add(pendente); _anexos.Items.Add(pendente.NomeOriginal); } catch (IOException ex) { _status.ForeColor = Color.Firebrick; _status.Text = ex.Message; } }
        private void CarregarEdicaoAnexo() { if (_anexos.SelectedIndex < 0 || _anexos.SelectedIndex >= _pendentes.Count) return; var pendente = _pendentes[_anexos.SelectedIndex]; _tituloAnexo.Text = pendente.Titulo; _categoriaAnexo.Text = pendente.Categoria; }
        private void AplicarEdicaoAnexo() { if (_anexos.SelectedIndex < 0 || _anexos.SelectedIndex >= _pendentes.Count) return; var pendente = _pendentes[_anexos.SelectedIndex]; pendente.Titulo = _tituloAnexo.Text.Trim(); pendente.Categoria = _categoriaAnexo.Text.Trim(); _status.ForeColor = Color.SeaGreen; _status.Text = "Edição do anexo aplicada à sessão temporária."; }
        private void RemoverAnexo() { if (_anexos.SelectedIndex < 0 || _anexos.SelectedIndex >= _pendentes.Count) return; var indice = _anexos.SelectedIndex; var pendente = _pendentes[indice]; try { if (File.Exists(pendente.CaminhoTemporario)) File.Delete(pendente.CaminhoTemporario); } catch (IOException) { } _pendentes.RemoveAt(indice); _anexos.Items.RemoveAt(indice); _tituloAnexo.Clear(); _categoriaAnexo.Clear(); _status.Text = "Anexo removido da sessão temporária."; }
    }
}
