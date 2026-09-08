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
        private readonly ObservacoesControl _observacoes = new ObservacoesControl();
        private readonly Dictionary<string,
            ComponenteEditorControl>
            _editorPorComponente =
            new Dictionary<string,
                ComponenteEditorControl>(
                StringComparer.OrdinalIgnoreCase);
        public CadastroPilotoForm(AppServices servicos, ConfiguracaoLocal local)
        {
            InitializeComponent();
            _servicos = servicos; _local = local; _sessaoAnexos = _servicos.Anexos.CriarSessao(); _etapaRevisao = new EtapaRevisaoControl();
            _etapaRevisao.ConfigurarSessao(
                _servicos.Anexos, 
                _sessaoAnexos);
            _observacoes.ConfigurarSessao(
                _servicos.Anexos,
                _sessaoAnexos);
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
            // 1. Criação do TabControl (Wizard)
            _wizardTab = new TabControl { Dock = DockStyle.Fill, ItemSize = new Size(0, 1), SizeMode = TabSizeMode.Fixed, Appearance = TabAppearance.FlatButtons };

            // ABA 1: Identificação e Origem
            var abaOrigem = new TabPage("1. Identificação");
            var pnlOrigem = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(30) };
            pnlOrigem.Controls.Add(new Label { Text = "Identificação do Projeto", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Margin = new Padding(0, 0, 0, 15), AutoSize = true });
            pnlOrigem.Controls.Add(new Label { Text = "Código do modelo:", AutoSize = true });
            pnlOrigem.Controls.Add(_codigo);
            _codigo.Leave += async delegate { await LocalizarPilotoAutomaticamente(); };

            pnlOrigem.Controls.Add(new Label { Text = "Nome do modelo:", AutoSize = true, Margin = new Padding(0, 15, 0, 0) });
            pnlOrigem.Controls.Add(_nome);

            pnlOrigem.Controls.Add(new Label { Text = "Pasta de Origem:", AutoSize = true, Margin = new Padding(0, 15, 0, 0) });
            pnlOrigem.Controls.Add(_origem);

            abaOrigem.Controls.Add(pnlOrigem);

            // ABA 2: Escaneamento e Texturas
            var abaComponentes = new TabPage("2. Escaneamento");
            var pnlComponentes = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 2, Padding = new Padding(30) };
            pnlComponentes.Controls.Add(new Label { Text = "Componentes e Texturas Detectadas", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Margin = new Padding(0, 0, 0, 15), AutoSize = true }, 0, 0);
            pnlComponentes.SetColumnSpan(pnlComponentes.GetControlFromPosition(0, 0), 2);

            // Reutiliza o controle _etapaDeteccao e o botão analisar já existentes no topo do seu código
            _analisar.Height = 35; _analisar.Width = 200; _analisar.Margin = new Padding(0, 0, 0, 10);
            _analisar.Click += delegate { AnalisarPasta(); };
            pnlComponentes.Controls.Add(_analisar, 0, 1);
            pnlComponentes.SetColumnSpan(_analisar, 2);

            _etapaDeteccao.ComponentesAlterados += delegate { AtualizarEditoresComponentes(); };
            _etapaDeteccao.Dock = DockStyle.Fill;
            pnlComponentes.Controls.Add(_etapaDeteccao, 0, 2);
            pnlComponentes.SetColumnSpan(_etapaDeteccao, 2);
            abaComponentes.Controls.Add(pnlComponentes);

            // ABA 3: Configuração de Matrizes (Parâmetros Técnicos e Componentes Dinâmicos)
            var abaMatriz = new TabPage("3. Matrizes");
            var pnlMatriz = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, WrapContents = false, Padding = new Padding(30) };
            pnlMatriz.Controls.Add(new Label { Text = "Parâmetros Técnicos e Matrizes", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Margin = new Padding(0, 0, 0, 15), AutoSize = true });

            pnlMatriz.Controls.Add(new Label { Text = "Tipo de Matriz Geral:", AutoSize = true }); pnlMatriz.Controls.Add(_tipoMatriz);
            pnlMatriz.Controls.Add(new Label { Text = "Material:", AutoSize = true }); pnlMatriz.Controls.Add(_material);
            pnlMatriz.Controls.Add(new Label { Text = "Eixos:", AutoSize = true }); pnlMatriz.Controls.Add(_eixos);
            pnlMatriz.Controls.Add(new Label { Text = "Máquina e Acabamento:", AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            var rowMatriz = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
            rowMatriz.Controls.Add(_maquina); rowMatriz.Controls.Add(_acabamento);
            pnlMatriz.Controls.Add(rowMatriz);
            pnlMatriz.Controls.Add(_matrizResumo);

            pnlMatriz.Controls.Add(new Label { Text = "Matrizes por Componente:", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Margin = new Padding(0, 20, 0, 10), AutoSize = true });
            pnlMatriz.Controls.Add(_editoresComponentes); // O controle dinâmico dos componentes
            abaMatriz.Controls.Add(pnlMatriz);

            // ABA 4: Observações e Anexos (A tela final)
            var abaObservacoes = new TabPage("4. Detalhes Finais");
            var pnlFinal = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(30) };
            pnlFinal.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Título
            pnlFinal.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // Observações
            pnlFinal.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // Anexos

            pnlFinal.Controls.Add(new Label { Text = "Observações e Anexos", Font = new Font("Segoe UI", 14F, FontStyle.Bold), Margin = new Padding(0, 0, 0, 15), AutoSize = true }, 0, 0);

            _observacoes.Dock = DockStyle.Fill;
            pnlFinal.Controls.Add(_observacoes, 0, 1);

            var pnlAnex = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 2 };
            var btnAddAnexo = new Button { Text = "Adicionar Anexo", AutoSize = true, Height = 35 };
            btnAddAnexo.Click += delegate { AdicionarAnexo(); };
            pnlAnex.Controls.Add(btnAddAnexo, 0, 0);

            var btnRemAnexo = new Button { Text = "Remover", AutoSize = true, Height = 35 };
            btnRemAnexo.Click += delegate { RemoverAnexo(); };
            pnlAnex.Controls.Add(btnRemAnexo, 1, 0);

            pnlAnex.Controls.Add(_anexos, 0, 1);
            pnlAnex.SetColumnSpan(_anexos, 2);
            pnlFinal.Controls.Add(pnlAnex, 0, 2);

            abaObservacoes.Controls.Add(pnlFinal);

            // Adiciona as 4 abas no controle
            _wizardTab.TabPages.Add(abaOrigem);
            _wizardTab.TabPages.Add(abaComponentes);
            _wizardTab.TabPages.Add(abaMatriz);
            _wizardTab.TabPages.Add(abaObservacoes);

            // 2. Painel de Rodapé (Avançar e Voltar)
            var rodape = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 65, Padding = new Padding(15) };

            _btnAvancar = new Button { Text = "Avançar >", Width = 110, Height = 40, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            _btnAvancar.Click += BtnAvancar_Click;

            _btnVoltar = new Button { Text = "< Voltar", Width = 110, Height = 40, Enabled = false };
            _btnVoltar.Click += BtnVoltar_Click;

            var cancelar = new Button { Text = "Cancelar", Width = 110, Height = 40, Margin = new Padding(0, 0, 20, 0) };
            cancelar.Click += delegate { Close(); };

            rodape.Controls.Add(_btnAvancar);
            rodape.Controls.Add(_btnVoltar);
            rodape.Controls.Add(cancelar);

            _status.Margin = new Padding(0, 10, 20, 0);
            rodape.Controls.Add(_status); // Label de erros (no footer)

            // Montagem Final do Form
            Controls.Add(_wizardTab);
            Controls.Add(rodape);

            // Eventos dos combos que já existiam
            _tipoMatriz.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); };
            _material.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); };
            _eixos.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); };
            _maquina.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); };
            _acabamento.SelectedIndexChanged += delegate { AtualizarResumoMatriz(); };
            AtualizarResumoMatriz();
            _servicos.Tema.Aplicar(this);
        }

        // Variáveis privadas do fluxo Wizard
        private TabControl _wizardTab;
        private Button _btnVoltar;
        private Button _btnAvancar;

        // Lógica de Navegação
        private void BtnAvancar_Click(object sender, EventArgs e)
        {
            if (_wizardTab.SelectedIndex < _wizardTab.TabCount - 1)
            {
                _wizardTab.SelectedIndex++;
                AtualizarBotoesWizard();
            }
            else
            {
                Salvar(); // No último passo, ele Salva o cadastro!
            }
        }

        private void BtnVoltar_Click(object sender, EventArgs e)
        {
            if (_wizardTab.SelectedIndex > 0)
            {
                _wizardTab.SelectedIndex--;
                AtualizarBotoesWizard();
            }
        }

        private void AtualizarBotoesWizard()
        {
            _btnVoltar.Enabled = _wizardTab.SelectedIndex > 0;
            if (_wizardTab.SelectedIndex == _wizardTab.TabCount - 1)
                _btnAvancar.Text = "Concluir Cadastro";
            else
                _btnAvancar.Text = "Avançar >";
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
                        foreach (var textura in editor.Texturas)
                        {
                            componente.Texturas.Add(textura);
                        }
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
                // Mantém também no nível geral
                // para compatibilidade com cadastros antigos.
                // A relação real fica:
                // componente -> matriz-> TexturasIds.
                foreach (var componente in
                    cadastro.Componentes)
                {
                    foreach (var textura in
                        componente.Texturas)
                    {
                        if(!cadastro.Texturas.Exists(
                            x=>x.Id==textura.Id))
                        {
                            cadastro.Texturas.Add(
                                textura);
                        }
                    }
                }
                var salvo = _servicos.CadastroPiloto.Salvar(
                    _local.PastaRaizDados,
                    cadastro,
                    _etapaRevisao.Pendentes,
                    new[] { "Gravação", "Tampa", "Laterais" },
                    _observacoes.Observacoes);

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
                        new ComponenteEditorControl(nome,null,_etapaDeteccao.TexturasSelecionadas);

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

                if (_editorPorComponente.TryGetValue(nome,out editor))
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
