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
            Text = "Ficha técnica — " + piloto.Codigo; StartPosition = FormStartPosition.CenterParent; Size = new Size(900, 650); Font = new Font("Segoe UI", 10F);
            var texto = new StringBuilder(); texto.AppendLine("Código: " + piloto.Codigo); texto.AppendLine("Nome: " + piloto.NomeModelo); texto.AppendLine("Origem: " + piloto.PastaOrigem); texto.AppendLine("Status: " + piloto.Status); texto.AppendLine(); texto.AppendLine("Componentes e matrizes:");
            foreach (var componente in piloto.Componentes) { texto.AppendLine("• " + componente.Nome); foreach (var matriz in componente.Matrizes) texto.AppendLine("  - " + matriz.Tipo + " / " + (matriz.Material.HasValue ? matriz.Material.ToString() : "sem material") + " / " + (matriz.Eixos.HasValue ? matriz.Eixos.ToString() : "sem eixos")); }
            texto.AppendLine(); texto.AppendLine("Texturas: " + piloto.Texturas.Count); foreach (var textura in piloto.Texturas) texto.AppendLine("- " + textura.Nome + " [" + textura.CaminhoRelativo + "]"); texto.AppendLine(); texto.AppendLine("Anexos: " + piloto.Anexos.Count); foreach (var anexo in piloto.Anexos) texto.AppendLine("- " + (anexo.Titulo ?? anexo.NomeOriginal) + " [" + anexo.CaminhoRelativo + "]");
            var conteudo = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Text = texto.ToString() }; var botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.RightToLeft }; var editar = new Button { Text = "Editar nome", AutoSize = true }; editar.Click += delegate { using (var form = new EditarPilotoForm(servicos, local, piloto)) if (form.ShowDialog(this) == DialogResult.OK) Close(); }; var status = new Button { Text = piloto.Status == StatusPiloto.Cancelada ? "Reativar" : "Cancelar", AutoSize = true }; status.Click += delegate { AlterarStatus(servicos, local, piloto); }; botoes.Controls.Add(editar); botoes.Controls.Add(status); Controls.Add(conteudo); Controls.Add(botoes); servicos.Tema.Aplicar(this);
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
        

    