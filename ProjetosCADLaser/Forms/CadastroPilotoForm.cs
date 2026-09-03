using System.Collections.Generic;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;
using ProjetosCADLaser.Controls;
using System.Threading.Tasks;


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
        private System.Threading.CancellationTokenSource _pesquisaOrigemCts;
        private readonly EtapaDeteccaoControl _etapaDeteccao = new EtapaDeteccaoControl();
        private readonly EtapaMatrizesControl _etapaMatrizes = new EtapaMatrizesControl();
        private EtapaRevisaoControl _etapaRevisao;
        private readonly FlowLayoutPanel
            _editoresComponentes =
            new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
        private readonly Dictionary<string,
            ComponenteEditorControl>
            _editorPorComponente =
            new Dictionary<string,
                ComponenteEditorControl>(
                StringComparer.OrdinalIgnoreCase);
        public CadastroPilotoForm(AppServices servicos, ConfiguracaoLocal local)
        {
            InitializeComponent();
            _servicos = servicos; _local = local; _sessaoAnexos = _servicos.Anexos.CriarSessao(); _etapaRevisao = new EtapaRevisaoControl(); _etapaRevisao.ConfigurarSessao(_servicos.Anexos, _sessaoAnexos);
            _heartbeat.Tick += delegate { if (_bloqueio != null && !string.IsNullOrWhiteSpace(_caminhoBloqueio)) try { _servicos.Bloqueios.Atualizar(_caminhoBloqueio, _bloqueio); } catch { } };
            FormClosed += delegate { _heartbeat.Stop(); if (_bloqueio != null && !string.IsNullOrWhiteSpace(_caminhoBloqueio)) try { _servicos.Bloqueios.Remover(_caminhoBloqueio, _bloqueio); } catch { } _bloqueio = null; try { _servicos.Anexos.LimparSessao(_sessaoAnexos); } catch { } };
            if (local == null || string.IsNullOrWhiteSpace(local.PastaRaizDados)) throw new InvalidOperationException("A pasta raiz não está configurada.");
            Text = "Cadastrar piloto — Projetos CAD/LASER";
            StartPosition = FormStartPosition.CenterParent;
            Size =new Size(1050, 700);
            MinimumSize = new Size(900, 600);
            Font = new Font("Segoe UI", 10F);

            _tipoMatriz.Items.AddRange(new object[] { "Gravação", "Tampa", "Laterais" }); _tipoMatriz.SelectedIndex = 0;
            _material.Items.AddRange(Enum.GetNames(typeof(MaterialMatriz))); _material.SelectedIndex = 0;
            _eixos.Items.AddRange(Enum.GetNames(typeof(QuantidadeEixos))); _eixos.SelectedIndex = 0;
            _maquina.Items.AddRange(Enum.GetNames(typeof(Maquina))); _maquina.SelectedIndex = 0;
            _acabamento.Items.AddRange(Enum.GetNames(typeof(Acabamento))); _acabamento.SelectedIndex = 0;
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll=true,
                Padding = new Padding(30),
                ColumnCount = 2,
                RowCount = 8
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.Controls.Add(
                new Label { Text = "Código do modelo", AutoSize = true },
                0,
                0);

            layout.Controls.Add(_codigo, 1, 0);

            _codigo.Leave += async delegate
            {
                await LocalizarPilotoAutomaticamente();
            };

            layout.Controls.Add(
                new Label { Text = "Nome do modelo", AutoSize = true },
                0,
                1);

            layout.Controls.Add(_nome, 1, 1);

            // Mantemos somente para erros e avisos.
            // Quando tudo der certo, ficará vazio.
            layout.Controls.Add(_status, 0, 2);
            layout.SetColumnSpan(_status, 2);

            layout.Controls.Add(_etapaDeteccao, 0, 3);
            layout.SetColumnSpan(_etapaDeteccao, 2);
            _etapaDeteccao.ComponentesAlterados += delegate
              { AtualizarEditoresComponentes(); };

            layout.Controls.Add(
                new Label
                {
                    Text = "Matrizes",
                    AutoSize = true
                },
                0,
                4);
            layout.Controls.Add(_editoresComponentes, 0, 5);
            layout.SetColumnSpan(_editoresComponentes, 2);
            layout.Controls.Add(
                new Label { Text = "Anexos pendentes", AutoSize = true },
                0,
                6);

            layout.Controls.Add(_etapaRevisao, 1, 6);

            _etapaMatrizes.ConfiguracaoAlterada += delegate
            {
                AtualizarResumoMatriz();
            };

            var salvar = new Button
            {
                Text = "Salvar cadastro",
                AutoSize = true,
                Height = 38
            };

            salvar.Click += delegate
            {
                Salvar();
            };

            var cancelar = new Button
            {
                Text = "Cancelar",
                AutoSize = true,
                Height = 38
            };

            cancelar.Click += delegate
            {
                Close();
            };

            var botoes = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill
            };

            botoes.Controls.Add(cancelar);
            botoes.Controls.Add(salvar);

            layout.Controls.Add(botoes, 0, 7);
            layout.SetColumnSpan(botoes, 2);

            Controls.Add(layout);
            _tipoMatriz.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); };
            _material.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); };
            _eixos.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); };
            _maquina.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); };
            _acabamento.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); }; AtualizarResumoMatriz();
            _servicos.Tema.Aplicar(this);
        }

        private async Task LocalizarPilotoAutomaticamente()
        {
            var codigo = _codigo.Text.Trim();

            if (string.IsNullOrWhiteSpace(codigo))
                return;

            if
                (string.IsNullOrWhiteSpace(_local.PastaOrigemProjetos
                ))
            {
                _status.ForeColor = Color.Firebrick;
                _status.Text = "A pasta-base das pilotos não está configurada.";
                return;
            }

            if (_pesquisaOrigemCts != null)
            {
                _pesquisaOrigemCts.Cancel();
                _pesquisaOrigemCts.Dispose();
            }

            _pesquisaOrigemCts = new
            System.Threading.CancellationTokenSource();

            try
            {
                _status.ForeColor = Color.DimGray;
                _status.Text = "Procurando piloto" + codigo + "...";

                var resultado =
                    await
                    _servicos.PesquisaPastaOrigem.PesquisarAsync(
                        _local.PastaOrigemProjetos,
                        codigo,
                        _pesquisaOrigemCts.Token);

                if (!resultado.RaizDisponivel)
                {
                    _status.ForeColor = Color.Firebrick;
                    _status.Text = "A pasta de pilotos está indisponível.";
                    return;
                }
                if (resultado.PastasEncontradas.Count == 0)
                {
                    _status.ForeColor = Color.DarkOrange;
                    _status.Text = "Foram encontradas" +
                    resultado.PastasEncontradas.Count +
                    "pastas para este código. Selecione a pasta manualmente.";
                    return;
                }

                var pastaEncontrada =
                    resultado.PastasEncontradas[0];

                _origem.Text = pastaEncontrada;

                var identificacao =
                AnalisadorPastasService.IdentificarRaiz(
                    new DirectoryInfo(pastaEncontrada).Name);

                if (string.IsNullOrWhiteSpace(_nome.Text))
                    _nome.Text = identificacao.NomeModelo;

                AnalisarPasta();
            }
            catch (OperationCanceledException)
            {
            }
            catch (IOException ex)
            {
                _status.ForeColor = Color.Firebrick;
                _status.Text = ex.Message;
            }
            catch (UnauthorizedAccessException)
            {
                _status.ForeColor = Color.Firebrick;
                _status.Text = "Sem permissão para acessar a pasta pilotos";
            }
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
                var cadastro = new PilotoCadastro
                {
                    Codigo = _codigo.Text.Trim(),
                    NomeModelo = _nome.Text.Trim(),
                    PastaOrigem = _origem.Text.Trim()
                };

                foreach (var nomeComponente in
                    _etapaDeteccao.ComponentesSelecionados)
                {
                    var componente =
                        new ComponenteCadastro
                        {
                            Nome = nomeComponente
                        };

                    ComponenteEditorControl editor;

                    if (_editorPorComponente.TryGetValue(
                        nomeComponente,
                        out editor))
                    {
                        foreach (var matriz in editor.Matrizes)
                        {
                            if (string.Equals(
                                matriz.Tipo,
                                "Laterais",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                componente.Matrizes.AddRange(
                                    _servicos.Matrizes.CriarGrupoLaterais(
                                        matriz));
                            }
                            else
                            {
                                componente.Matrizes.Add(matriz);
                            }
                        }
                    }

                    cadastro.Componentes.Add(componente);
                }

                // Temporário:
                // ainda mantemos as texturas no nível geral
                // para não quebrar a estrutura antiga.
                // Na próxima etapa elas serão distribuídas
                // para cada componente.
                foreach (var item in
                    _etapaDeteccao.TexturasSelecionadas)
                {
                    cadastro.Texturas.Add(
                        new TexturaCadastro
                        {
                            Nome = item,
                            CaminhoDetectado =
                                Path.Combine(_origem.Text, item),
                            CaminhoRelativo = item,
                            Selecionada = true,
                            Confirmada = true
                        });
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
                _status.Text = string.Empty;
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is IOException) { _status.ForeColor = Color.Firebrick; _status.Text = ex.Message; }
        }
        private void AtualizarEditoresComponentes()
        {
            var selecionados =
                new HashSet<string>(
                    _etapaDeteccao.ComponentesSelecionados,
                    StringComparer.OrdinalIgnoreCase);

            foreach (var nome in selecionados)
            {
                ComponenteEditorControl editor;

                if (!_editorPorComponente.TryGetValue(
                    nome,
                    out editor))
                {
                    editor =
                        new ComponenteEditorControl(nome);

                    _editorPorComponente.Add(
                        nome,
                        editor);
                }
            }

            _editoresComponentes.SuspendLayout();

            _editoresComponentes.Controls.Clear();

            foreach (var nome in
                _etapaDeteccao.ComponentesSelecionados)
            {
                ComponenteEditorControl editor;

                if (_editorPorComponente.TryGetValue(
                    nome,
                    out editor))
                {
                    _editoresComponentes.Controls.Add(editor);
                }
            }

            _editoresComponentes.ResumeLayout();
        }
        private void AdicionarAnexo() { using (var dialogo = new OpenFileDialog { Multiselect = false, Title = "Selecionar anexo" }) if (dialogo.ShowDialog(this) == DialogResult.OK) try { var pendente = _servicos.Anexos.AdicionarArquivo(_sessaoAnexos, dialogo.FileName, false); _pendentes.Add(pendente); _anexos.Items.Add(pendente.NomeOriginal); } catch (IOException ex) { _status.ForeColor = Color.Firebrick; _status.Text = ex.Message; } }
        private void CarregarEdicaoAnexo() { if (_anexos.SelectedIndex < 0 || _anexos.SelectedIndex >= _pendentes.Count) return; var pendente = _pendentes[_anexos.SelectedIndex]; _tituloAnexo.Text = pendente.Titulo; _categoriaAnexo.Text = pendente.Categoria; }
        private void AplicarEdicaoAnexo() { if (_anexos.SelectedIndex < 0 || _anexos.SelectedIndex >= _pendentes.Count) return; var pendente = _pendentes[_anexos.SelectedIndex]; pendente.Titulo = _tituloAnexo.Text.Trim(); pendente.Categoria = _categoriaAnexo.Text.Trim(); _status.ForeColor = Color.SeaGreen; _status.Text = "Edição do anexo aplicada à sessão temporária."; }
        private void RemoverAnexo() { if (_anexos.SelectedIndex < 0 || _anexos.SelectedIndex >= _pendentes.Count) return; var indice = _anexos.SelectedIndex; var pendente = _pendentes[indice]; try { if (File.Exists(pendente.CaminhoTemporario)) File.Delete(pendente.CaminhoTemporario); } catch (IOException) { } _pendentes.RemoveAt(indice); _anexos.Items.RemoveAt(indice); _tituloAnexo.Clear(); _categoriaAnexo.Clear(); _status.Text = "Anexo removido da sessão temporária."; }
    }
}
