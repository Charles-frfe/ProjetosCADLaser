using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;

namespace ProjetosCADLaser.Forms
{
    public sealed partial class FichaTesteTexturaForm : Form
    {
        public FichaTesteTexturaForm(AppServices servicos, ConfiguracaoLocal local, TesteTexturaCadastro teste) { Text = "Ficha de teste de textura — " + teste.Identificacao; Size = new Size(760, 520); var texto = new StringBuilder(); texto.AppendLine("Identificação: " + teste.Identificacao); texto.AppendLine("Status: " + TesteTexturaService.Situacao(teste.Status)); texto.AppendLine("Origem: " + teste.PastaOrigem); texto.AppendLine("Observação: " + teste.Observacao); texto.AppendLine("Nome aprovado: " + (teste.NomeAprovado ?? "Não informado")); texto.AppendLine(); texto.AppendLine("Imagem principal: " + (teste.Imagem == null ? "Não informada" : teste.Imagem.NomeOriginal)); texto.AppendLine("Imagens adicionais: " + teste.Imagens.Count); texto.AppendLine("Arquivos: " + teste.Arquivos.Count); texto.AppendLine("Histórico: " + teste.Historico.Count + " evento(s)"); var conteudo = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Text = texto.ToString() }; var editar = new Button { Text = "Editar teste", Dock = DockStyle.Bottom, Height = 38 }; editar.Click += delegate { using (var form = new EditarTesteTexturaForm(servicos, local, teste)) if (form.ShowDialog(this) == DialogResult.OK) Close(); }; Controls.Add(conteudo); Controls.Add(editar); servicos.Tema.Aplicar(this); }
    }
}
