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
                    foreach (var tex in piloto.Texturas)
                    {
                        // Mostra a textura apenas se ela estiver vinculada à Matriz atual
                        if (matriz.TexturasIds != null && matriz.TexturasIds.Contains(tex.Id))
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
            var pnlAnexos = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10), FlowDirection = FlowDirection.TopDown, WrapContents = false };

            // Loop processando o Histórico
            foreach (var evento in piloto.Historico)
            {
                if (evento.Tipo == TipoEvento.ObservacaoGeral || !string.IsNullOrWhiteSpace(evento.Observacao) || (evento.Anexos != null && evento.Anexos.Count > 0))
                {
                    // Removemos a data como você pediu, e deixamos só a observação
                    var lblObs = new Label { Text = $"- {evento.Observacao ?? "Sem descrição"}", AutoSize = true, Font = new Font("Segoe UI", 11F), Margin = new Padding(0, 10, 0, 5) };
                    pnlAnexos.Controls.Add(lblObs);

                    var pnlLinks = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
                    if (evento.Anexos != null)
                    {
                        foreach (var anexo in evento.Anexos)
                        {
                            var _anexoCaptura = anexo; // Captura para o delegate local
                            var linkAnexo = new LinkLabel { Text = $"Abrir {(_anexoCaptura.Titulo ?? _anexoCaptura.NomeOriginal)}", AutoSize = true, Margin = new Padding(20, 0, 10, 0) };
                            linkAnexo.LinkClicked += delegate {
                                var _caminhoReal = System.IO.Path.Combine(local.PastaRaizDados, "Cadastros", piloto.Codigo, "anexos", _anexoCaptura.CaminhoRelativo ?? string.Empty);
                                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = _caminhoReal, UseShellExecute = true }); } catch { }
                            };
                            pnlLinks.Controls.Add(linkAnexo);
                        }
                    }
                    pnlAnexos.Controls.Add(pnlLinks);
                }
            }
            // Retaguarda de anexos base caso estejam salvos em piloto.Anexos e não dentro dos Eventos
            foreach (var anexo in piloto.Anexos)
            {
                var _anexoCaptura = anexo; // Captura para o delegate local
                var linkAnexo = new LinkLabel { Text = $"Arquivo: {(_anexoCaptura.Titulo ?? _anexoCaptura.NomeOriginal)}", AutoSize = true, Margin = new Padding(5, 5, 0, 0) };
                linkAnexo.LinkClicked += delegate {
                    var _caminhoReal = System.IO.Path.Combine(local.PastaRaizDados, "Cadastros", piloto.Codigo, "anexos", _anexoCaptura.CaminhoRelativo ?? string.Empty);
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = _caminhoReal, UseShellExecute = true }); } catch { }
                };
                pnlAnexos.Controls.Add(linkAnexo);
            }

            abaAnexos.Controls.Add(pnlAnexos);

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
            var btnEditarModelo = new Button { Text = "Editar Modelo (Adicionar Matriz/Obs)", AutoSize = true, Height = 35, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };

            // O botão abre sua tela original de Edição/Retoque daquele modelo específico
            btnEditarModelo.Click += delegate {
                using (var form = new EditarPilotoForm(servicos, local, piloto))
                    if (form.ShowDialog(this) == DialogResult.OK)
                        Close(); // Ao finalizar a edição, fecha a ficha pro usuário reabri-la atualizada
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
