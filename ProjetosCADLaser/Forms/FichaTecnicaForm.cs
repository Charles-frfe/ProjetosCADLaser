// Analisado
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;

namespace ProjetosCADLaser.Forms
{
    public sealed partial class FichaTecnicaForm : Form
    {
        public FichaTecnicaForm(AppServices servicos, ConfiguracaoLocal local, PilotoCadastro piloto)
        {
            InitializeComponent();
            Text = "Ficha técnica — " + piloto.Codigo;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1000, 750);
            Font = new Font("Segoe UI", 10F);

            // Cabeçalho fixo (apenas com o título e origem descritos)
            var cabecalho = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, AutoSize = true, Padding = new Padding(15) };
            var lblTitulo = new Label { Text = $"{piloto.Codigo} — {piloto.NomeModelo}", Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true };
            var lblInfo = new Label { Text = $"Status: {piloto.Status} | Origem: {piloto.PastaOrigem}", ForeColor = Color.DimGray, AutoSize = true, Margin = new Padding(2, 5, 0, 10) };

            cabecalho.Controls.Add(lblTitulo);
            cabecalho.Controls.Add(lblInfo);

            // TabControl Principal
            var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(10, 10), Font = new Font("Segoe UI Semibold", 10F) };

            // ABA 1: Estrutura (A Árvore Refinada com 3x e 5x)
            var abaEstrutura = new TabPage("Estrutura (Matrizes e Texturas)");
            var arvore = new TreeView { Dock = DockStyle.Fill, Margin = new Padding(10), Font = new Font("Segoe UI", 11F) };

            foreach (var comp in piloto.Componentes)
            {
                var nodeComp = new TreeNode($"Componente: {comp.Nome}");
                foreach (var matriz in comp.Matrizes)
                {
                    var nodeMatriz = new TreeNode(matriz.Tipo);
                    if (matriz.Material.HasValue) nodeMatriz.Nodes.Add($"Material: {matriz.Material}");
                    if (matriz.Eixos.HasValue)
                    {
                        string valEixos = matriz.Eixos.ToString();
                        if (valEixos == "Tres") valEixos = "3x";
                        else if (valEixos == "Cinco") valEixos = "5x";
                        nodeMatriz.Nodes.Add($"Eixos: {valEixos}");
                    }
                    if (matriz.Maquina.HasValue) nodeMatriz.Nodes.Add($"Máquina: {matriz.Maquina}");
                    if (matriz.Acabamento.HasValue) nodeMatriz.Nodes.Add($"Acabamento: {matriz.Acabamento}");

                    // Coloca as texturas ligadas por Matriz. 
                    foreach (var tex in comp.Texturas)
                    {
                        if(matriz.TexturasIds!=null &&
                            matriz.TexturasIds.Contains(tex.Id))
                        {
                            nodeMatriz.Nodes.Add($"Textura: {tex.Nome}");
                        }
                    }

                    nodeComp.Nodes.Add(nodeMatriz);
                }
                arvore.Nodes.Add(nodeComp);
            }
            arvore.ExpandAll();
            abaEstrutura.Controls.Add(arvore);

            // ABA 2: Observações e Anexos Vinculados
            var abaAnexos = new TabPage("Observações");

            var pnlAnexos = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(15),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            var tituloObservacoes = new Label
            {
                Text = "Observações do projeto",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 14F),
                Margin = new Padding(0, 0, 0, 12)
            };

            pnlAnexos.Controls.Add(tituloObservacoes);

            foreach (var evento in piloto.Historico)
            {
                bool possuiObservacao =
                    !string.IsNullOrWhiteSpace(evento.Observacao);

                bool possuiAnexos =
                    evento.Anexos != null &&
                    evento.Anexos.Count > 0;

                bool possuiImagens =
                    evento.Imagens != null &&
                    evento.Imagens.Count > 0;

                if (!possuiObservacao &&
                    !possuiAnexos &&
                    !possuiImagens)
                {
                    continue;
                }

                var linhaObservacao = new TableLayoutPanel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 2,
                    RowCount = 1,
                    Width = 900,
                    Margin = new Padding(0, 0, 0, 10),
                    Padding = new Padding(12),
                    BackColor = Color.FromArgb(245, 245, 245)
                };

                linhaObservacao.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        70F));

                linhaObservacao.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        30F));

                var lblObs = new Label
                {
                    Text = possuiObservacao
                        ? evento.Observacao
                        : "Sem descrição",
                    AutoSize = true,
                    MaximumSize = new Size(600, 0),
                    Font = new Font(
                        "Segoe UI",
                        10.5F),
                    Margin = new Padding(
                        0,
                        4,
                        15,
                        4)
                };

                linhaObservacao.Controls.Add(
                    lblObs,
                    0,
                    0);

                var pnlLinks =
                    new FlowLayoutPanel
                    {
                        AutoSize = true,
                        AutoSizeMode =
                            AutoSizeMode.GrowAndShrink,
                        FlowDirection =
                            FlowDirection.LeftToRight,
                        WrapContents = true,
                        Margin = new Padding(
                            0,
                            0,
                            0,
                            0)
                    };

                if (evento.Anexos != null)
                {
                    foreach (var anexo in evento.Anexos)
                    {
                        var anexoCaptura = anexo;

                        var linkAnexo =
                            new LinkLabel
                            {
                                Text =
                                    "Abrir " +
                                    (
                                        !string.IsNullOrWhiteSpace(
                                            anexoCaptura.Titulo)
                                        ? anexoCaptura.Titulo
                                        : anexoCaptura.NomeOriginal
                                    ),
                                AutoSize = true,
                                LinkColor =
                                    Color.RoyalBlue,
                                ActiveLinkColor =
                                    Color.Navy,
                                VisitedLinkColor =
                                    Color.RoyalBlue,
                                Font = new Font(
                                    "Segoe UI Semibold",
                                    9.5F),
                                Margin = new Padding(
                                    5,
                                    4,
                                    10,
                                    4)
                            };

                        linkAnexo.LinkClicked +=
                            delegate
                            {
                                var caminhoReal =
                                    Path.Combine(
                                        local.PastaRaizDados,
                                        "Cadastros",
                                        piloto.Codigo,
                                        anexoCaptura
                                            .CaminhoRelativo ??
                                        string.Empty);

                                if (!File.Exists(
                                    caminhoReal))
                                {
                                    MessageBox.Show(
                                        "Arquivo não encontrado.\n\n" +
                                        "Caminho procurado:\n" +
                                        caminhoReal,
                                        "Anexo",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);

                                    return;
                                }

                                try
                                {
                                    System.Diagnostics
                                        .Process.Start(
                                            new System.Diagnostics
                                                .ProcessStartInfo
                                            {
                                                FileName =
                                                    caminhoReal,
                                                UseShellExecute =
                                                    true
                                            });
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(
                                        "Não foi possível abrir o anexo.\n\n" +
                                        "Caminho:\n" +
                                        caminhoReal +
                                        "\n\nErro:\n" +
                                        ex.Message,
                                        "Erro ao abrir anexo",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                }
                            };

                        pnlLinks.Controls.Add(
                            linkAnexo);
                    }
                }

                if (evento.Imagens != null)
                {
                    foreach (var imagem in evento.Imagens)
                    {
                        var imagemCaptura = imagem;

                        var linkImagem =
                            new LinkLabel
                            {
                                Text =
                                    "Abrir " +
                                    (
                                        !string.IsNullOrWhiteSpace(
                                            imagemCaptura.Titulo)
                                        ? imagemCaptura.Titulo
                                        : imagemCaptura.NomeOriginal
                                    ),
                                AutoSize = true,
                                LinkColor =
                                    Color.RoyalBlue,
                                ActiveLinkColor =
                                    Color.Navy,
                                VisitedLinkColor =
                                    Color.RoyalBlue,
                                Font = new Font(
                                    "Segoe UI Semibold",
                                    9.5F),
                                Margin = new Padding(
                                    5,
                                    4,
                                    10,
                                    4)
                            };

                        linkImagem.LinkClicked +=
                            delegate
                            {
                                var caminhoReal =
                                    Path.Combine(
                                        local.PastaRaizDados,
                                        "Cadastros",
                                        piloto.Codigo,
                                        imagemCaptura
                                            .CaminhoRelativo ??
                                        string.Empty);

                                if (!File.Exists(
                                    caminhoReal))
                                {
                                    MessageBox.Show(
                                        "Imagem não encontrada.\n\n" +
                                        "Caminho procurado:\n" +
                                        caminhoReal,
                                        "Imagem",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);

                                    return;
                                }

                                try
                                {
                                    System.Diagnostics
                                        .Process.Start(
                                            new System.Diagnostics
                                                .ProcessStartInfo
                                            {
                                                FileName =
                                                    caminhoReal,
                                                UseShellExecute =
                                                    true
                                            });
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(
                                        "Não foi possível abrir a imagem.\n\n" +
                                        "Caminho:\n" +
                                        caminhoReal +
                                        "\n\nErro:\n" +
                                        ex.Message,
                                        "Erro ao abrir imagem",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                }
                            };

                        pnlLinks.Controls.Add(
                            linkImagem);
                    }
                }

                linhaObservacao.Controls.Add(
                    pnlLinks,
                    1,
                    0);

                pnlAnexos.Controls.Add(
                    linhaObservacao);
            }

            abaAnexos.Controls.Add(
                pnlAnexos);
            // ABA 3: Histórico de Auditoria
            var abaAuditoria = new TabPage("Histórico de Alterações");
            var gridHistorico = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9.5F)
            };
            gridHistorico.Columns.Add("Data", "Data/Hora");
            gridHistorico.Columns.Add("Usuario", "Usuário");
            gridHistorico.Columns.Add("Tipo", "Tipo de Ação");
            gridHistorico.Columns.Add("Obs", "Observação");
            gridHistorico.Columns["Data"].FillWeight = 30;
            gridHistorico.Columns["Usuario"].FillWeight = 25;
            gridHistorico.Columns["Tipo"].FillWeight = 30;

            foreach (var evento in piloto.Historico)
            {
                gridHistorico.Rows.Add(
                    evento.DataHora.ToString("dd/MM/yyyy HH:mm"),
                    $"{evento.Usuario} ({evento.Computador})",
                    evento.Tipo.ToString(),
                    evento.Observacao ?? "-"
                );
            }
            abaAuditoria.Controls.Add(gridHistorico);

            tabs.TabPages.Add(abaEstrutura);
            tabs.TabPages.Add(abaAnexos);
            tabs.TabPages.Add(abaAuditoria);

            // Rodapé de Ações (Apenas Editar Modelo)
            var botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 60, Padding = new Padding(10) };
            var btnEditarModelo = new Button { Text = "Editar", AutoSize = true, Height = 35 };
            btnEditarModelo.Click += delegate {
                using (var form = new EditarPilotoForm(servicos, local, piloto))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        Close();
                    }
                }
            };
            botoes.Controls.Add(btnEditarModelo);

            // Montagem final
            Controls.Add(tabs);
            Controls.Add(cabecalho);
            Controls.Add(botoes);

            servicos.Tema.Aplicar(this);
        }

        private void AlterarStatus(AppServices servicos, ConfiguracaoLocal local, PilotoCadastro piloto)
        {
            var motivo = PromptMotivo(this, piloto.Status == StatusPiloto.Cancelada ? "Motivo da reativação" : "Motivo do cancelamento"); if (string.IsNullOrWhiteSpace(motivo)) return; RegistroBloqueio dono = null; string caminho = null;
            try { caminho = servicos.Bloqueios.ObterCaminho(local.PastaRaizDados, piloto.Codigo); dono = servicos.Bloqueios.CriarPorCodigo(local.PastaRaizDados, piloto.Codigo, piloto.Id, "Alteração de status", local.NomeExibido); if (piloto.Status == StatusPiloto.Cancelada) servicos.CancelamentoPiloto.Reativar(local.PastaRaizDados, piloto.Codigo, motivo); else servicos.CancelamentoPiloto.Cancelar(local.PastaRaizDados, piloto.Codigo, motivo); Close(); }
            catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is InvalidOperationException) { servicos.Log.Registrar(ex, "Falha na alteração de status", local.PastaRaizDados); MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { if (dono != null && caminho != null) try { servicos.Bloqueios.Remover(caminho, dono); } catch { } }
        }

        private static string PromptMotivo(IWin32Window owner, string titulo)
        {
            using (var dialogo = new Form { Text = titulo, Size = new Size(480, 170), StartPosition = FormStartPosition.CenterParent }) { var campo = new TextBox { Dock = DockStyle.Top, Width = 420 }; var ok = new Button { Text = "Confirmar", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom }; dialogo.Controls.Add(campo); dialogo.Controls.Add(ok); dialogo.AcceptButton = ok; return dialogo.ShowDialog(owner) == DialogResult.OK ? campo.Text.Trim() : string.Empty; }
        }
    }
}


