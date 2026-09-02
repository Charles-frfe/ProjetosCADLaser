using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;

namespace ProjetosCADLaser.Forms
{
    public sealed partial class LixeiraForm : Form
    {
        private readonly AppServices _servicos; private readonly ConfiguracaoLocal _local; private readonly ListBox _itens = new ListBox { Dock = DockStyle.Fill }; private readonly System.Collections.Generic.List<ItemLixeira> _dados = new System.Collections.Generic.List<ItemLixeira>();
        public LixeiraForm(AppServices servicos, ConfiguracaoLocal local) { _servicos = servicos; _local = local; Text = "Lixeira administrativa"; Size = new Size(760, 500); var restaurar = new Button { Text = "Restaurar selecionado", Dock = DockStyle.Bottom, Height = 38 }; restaurar.Click += delegate { Restaurar(); }; Controls.Add(_itens); Controls.Add(restaurar); Shown += delegate { Carregar(); }; servicos.Tema.Aplicar(this); }
        private void Carregar() { _itens.Items.Clear(); _dados.Clear(); foreach (var item in _servicos.Lixeira.Listar(_local.PastaRaizDados, TipoRegistroLixeira.Piloto)) { _dados.Add(item); _itens.Items.Add(item.Meta.Codigo + " — " + item.Meta.Nome + " · " + item.Meta.Motivo); } }
        private void Restaurar() { if (_itens.SelectedIndex < 0 || _itens.SelectedIndex >= _dados.Count) return; var pin = Prompt("PIN administrativo"); if (!_servicos.Pin.Verificar(pin, _servicos.ConfiguracaoCompartilhada.Carregar(_local.PastaRaizDados).PinAdministrativo)) { MessageBox.Show(this, "PIN inválido.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; } try { _servicos.Lixeira.Restaurar(_local.PastaRaizDados, _dados[_itens.SelectedIndex].Meta, _dados[_itens.SelectedIndex].Pasta); Carregar(); } catch (Exception ex) when (ex is IOException || ex is InvalidOperationException) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); } }
        private static string Prompt(string titulo) { using (var f = new Form { Text = titulo, Size = new Size(360, 150) }) { var t = new TextBox { UseSystemPasswordChar = true, Dock = DockStyle.Top }; var b = new Button { Text = "Confirmar", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom }; f.Controls.Add(t); f.Controls.Add(b); f.AcceptButton = b; return f.ShowDialog() == DialogResult.OK ? t.Text : string.Empty; } }
    }
}
