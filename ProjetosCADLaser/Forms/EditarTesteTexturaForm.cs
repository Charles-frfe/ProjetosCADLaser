using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;

namespace ProjetosCADLaser.Forms
{
    public sealed partial class EditarTesteTexturaForm : Form
    {
        private readonly AppServices _servicos; private readonly ConfiguracaoLocal _local; private readonly TesteTexturaCadastro _teste; private readonly ComboBox _status = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 }; private readonly TextBox _nome = new TextBox { Width = 300 }; private readonly TextBox _obs = new TextBox { Width = 300 }; private readonly Timer _heartbeat = new Timer { Interval = 30000 }; private RegistroBloqueio _bloqueio; private string _caminho;
        public EditarTesteTexturaForm(AppServices servicos, ConfiguracaoLocal local, TesteTexturaCadastro teste) { _servicos = servicos; _local = local; _teste = teste; Text = "Editar teste — " + teste.Identificacao; Size = new Size(600, 300); _status.Items.AddRange(new object[] { "EmTeste", "Aprovada", "Reprovada" }); _status.SelectedItem = teste.Status.ToString(); _nome.Text = teste.NomeAprovado; _obs.Text = teste.Observacao; var l = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(22), ColumnCount = 2 }; l.Controls.Add(new Label { Text = "Status", AutoSize = true }, 0, 0); l.Controls.Add(_status, 1, 0); l.Controls.Add(new Label { Text = "Nome aprovado", AutoSize = true }, 0, 1); l.Controls.Add(_nome, 1, 1); l.Controls.Add(new Label { Text = "Observação", AutoSize = true }, 0, 2); l.Controls.Add(_obs, 1, 2); var salvar = new Button { Text = "Salvar", AutoSize = true }; salvar.Click += delegate { Salvar(); }; l.Controls.Add(salvar, 1, 3); Controls.Add(l); _heartbeat.Tick += delegate { if (_bloqueio != null) try { _servicos.Bloqueios.Atualizar(_caminho, _bloqueio); } catch { } }; FormClosed += delegate { _heartbeat.Stop(); if (_bloqueio != null) try { _servicos.Bloqueios.Remover(_caminho, _bloqueio); } catch { } }; servicos.Tema.Aplicar(this); }
        private void Salvar() { RegistroBloqueio dono = null; try { _caminho = _servicos.Bloqueios.ObterCaminhoTeste(_local.PastaRaizDados, _teste.Id); dono = _servicos.Bloqueios.CriarParaTeste(_local.PastaRaizDados, _teste.Id, "Edição de teste", _local.NomeExibido); _bloqueio = dono; _heartbeat.Start(); var status = (StatusTesteTextura)Enum.Parse(typeof(StatusTesteTextura), _status.Text); _servicos.TestesTextura.Atualizar(_local.PastaRaizDados, _teste, status, string.IsNullOrWhiteSpace(_nome.Text) ? null : _nome.Text.Trim(), _obs.Text, new System.Collections.Generic.List<AnexoPendente>()); DialogResult = DialogResult.OK; Close(); } catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is InvalidOperationException) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); } finally { _heartbeat.Stop(); if (dono != null && _caminho != null) try { _servicos.Bloqueios.Remover(_caminho, dono); } catch { } _bloqueio = null; } }
    }
}
