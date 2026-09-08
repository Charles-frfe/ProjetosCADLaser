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

            // Cabeçalho fixo (fica fora das abas para ser visto o tempo todo)
            var cabecalho = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, AutoSize = true, Padding = new Padding(15) };
            var lblTitulo = new Label { Text = $"{piloto.Codigo} — {piloto.NomeModelo}", Font = new Font("Segoe UI", 16F, FontStyle.Bold), AutoSize = true };
            var lblInfo = new Label { Text = $"Status: {piloto.Status} | Origem: {piloto.PastaOrigem}", ForeColor = Color.DimGray, AutoSize = true, Margin = new Padding(2, 5, 0, 5) };

            var utilitariosHeader = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = new Padding(0, 5, 0, 10) };
            var btnCopiarOrigem = new Button { Text = "Copiar Caminho", AutoSize = true, Height = 30, FlatStyle = FlatStyle.Flat };
            btnCopiarOrigem.FlatAppearance.BorderSize = 1;
            btnCopiarOrigem.Click += delegate { if (!string.IsNullOrWhiteSpace(piloto.PastaOrigem)) Clipboard.SetText(piloto.PastaOrigem); };
            var btnAbrirOrigem = new Button { Text = "Abrir no Explorer", AutoSize = true, Height = 30, FlatStyle = FlatStyle.Flat };
            btnAbrirOrigem.FlatAppearance.BorderSize = 1;
            btnAbrirOrigem.Click += delegate { if (System.IO.Directory.Exists(piloto.PastaOrigem)) System.Diagnostics.Process.Start("explorer.exe", piloto.PastaOrigem); else MessageBox.Show(this, "A pasta não existe mais.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
            utilitariosHeader.Controls.Add(btnCopiarOrigem); utilitariosHeader.Controls.Add(btnAbrirOrigem);
            cabecalho.Controls.Add(lblTitulo); cabecalho.Controls.Add(lblInfo); cabecalho.Controls.Add(utilitariosHeader);

            // TabControl Principal
            var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(10, 10), Font = new Font("Segoe UI Semibold", 10F) };

            // ABA 1: Estrutura da Árvore
            var abaEstrutura = new TabPage("Estrutura do Modelo");
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
                    // (Exibição geral que você pediu para a árvore visual)
                    foreach (var tex in piloto.Texturas)
                    {
                        // Exibição provisória, até migrar o JSON interno para salvar TexturasIds dentro de MatrizCadastro
                        // nodeMatriz.Nodes.Add($"Textura: {tex.Nome}");
                    }

                    nodeComp.Nodes.Add(nodeMatriz);
                }
                arvore.Nodes.Add(nodeComp);
            }
            arvore.ExpandAll();
            abaEstrutura.Controls.Add(arvore);

            // ABA 2: Observações e Anexos Vinculados (Sem botões, apenas Links)
            var abaAnexos = new TabPage("Observações");
            var pnlAnexos = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10), FlowDirection = FlowDirection.TopDown, WrapContents = false };

            // Loop para processar os Históricos buscando as anotações textuais e arquivos atrelados
            foreach (var evento in piloto.Historico)
            {
                if (evento.Tipo == TipoEvento.ObservacaoGeral || !string.IsNullOrWhiteSpace(evento.Observacao) || (evento.Anexos != null && evento.Anexos.Count > 0))
                {
                    var lblObs = new Label { Text = $"- {evento.DataHora:dd/MM/yyyy}: {evento.Observacao ?? "Sem descrição"}", AutoSize = true, Font = new Font("Segoe UI", 11F), Margin = new Padding(0, 10, 0, 5) };
                    pnlAnexos.Controls.Add(lblObs);

                    var pnlLinks = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
                    if (evento.Anexos != null)
                    {
                        foreach (var anexo in evento.Anexos)
                        {
                            var linkAnexo = new LinkLabel { Text = $"Abrir {(anexo.Titulo ?? anexo.NomeOriginal)}", AutoSize = true, Margin = new Padding(20, 0, 10, 0) };
                            linkAnexo.LinkClicked += delegate {
                                // O processo chama diretamente o Windows para abrir imagens, PDFs ou pastas
                                try { System.Diagnostics.Process.Start(anexo.CaminhoRelativo); } catch { }
                            };
                            pnlLinks.Controls.Add(linkAnexo);
                        }
                    }
                    pnlAnexos.Controls.Add(pnlLinks);
                }
            }
            abaAnexos.Controls.Add(pnlAnexos);

            // Retaguarda de anexos base isolados
            foreach (var anexo in piloto.Anexos)
            {
                var linkAnexo = new LinkLabel { Text = $"Arquivo Isolado: {(anexo.Titulo ?? anexo.NomeOriginal)}", AutoSize = true, Margin = new Padding(5, 5, 0, 0) };
                linkAnexo.LinkClicked += delegate { try { System.Diagnostics.Process.Start(anexo.CaminhoRelativo); } catch { } };
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

            // Define um tamanho menorzinho só para a Data
            gridHistorico.Columns["Data"].FillWeight = 30;
            gridHistorico.Columns["Usuario"].FillWeight = 25;
            gridHistorico.Columns["Tipo"].FillWeight = 30;

            // Preenche o DataGridView
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

            // Rodapé de Ações
            var botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 60, Padding = new Padding(10) };
            var editar = new Button { Text = "Editar nome", AutoSize = true, Height = 35 };
            editar.Click += delegate { using (var form = new EditarPilotoForm(servicos, local, piloto)) if (form.ShowDialog(this) == DialogResult.OK) Close(); };
            var status = new Button { Text = piloto.Status == StatusPiloto.Cancelada ? "Reativar" : "Cancelar", AutoSize = true, Height = 35 };
            status.Click += delegate { AlterarStatus(servicos, local, piloto); };
            botoes.Controls.Add(editar); botoes.Controls.Add(status);

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
        

    