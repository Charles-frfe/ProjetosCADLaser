// Analisado
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;
using ProjetosCADLaser.Controls;
using System.Collections.Generic;

namespace ProjetosCADLaser.Forms
{
    public sealed partial class EditarPilotoForm : Form
    {
        private readonly AppServices _servicos;
        private readonly ConfiguracaoLocal _local;
        private readonly PilotoCadastro _piloto;
        private readonly TextBox _nome = new TextBox { Width = 420 };
        private readonly TextBox _observacao = new TextBox { Width = 420 };
        private readonly Timer _heartbeat = new Timer { Interval = 30000 };
        private RegistroBloqueio _bloqueio;
        private string _caminho;

        public EditarPilotoForm(AppServices servicos, ConfiguracaoLocal local, PilotoCadastro piloto)
        {
            _servicos = servicos; _local = local; _piloto = piloto;
            Text = "Editar piloto — " + piloto.Codigo;
            Size = new Size(680, 500);
            StartPosition = FormStartPosition.CenterParent;
            _nome.Text = piloto.NomeModelo;

            _heartbeat.Tick += delegate { if (_bloqueio != null) try { _servicos.Bloqueios.Atualizar(_caminho, _bloqueio); } catch { } };
            FormClosed += delegate { _heartbeat.Stop(); if (_bloqueio != null) try { _servicos.Bloqueios.Remover(_caminho, _bloqueio); } catch { } _bloqueio = null; };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 2, RowCount = 5 };
            layout.Controls.Add(new Label { Text = "Nome do modelo", AutoSize = true }, 0, 0);
            layout.Controls.Add(_nome, 1, 0);
            layout.Controls.Add(new Label { Text = "Nova Observação", AutoSize = true }, 0, 1);
            layout.Controls.Add(_observacao, 1, 1);

            var addMatrizBtn = new Button { Text = "Adicionar Matrizes / Editar Estrutura no Assistente", AutoSize = true, Width = 350 };
            addMatrizBtn.Click += delegate {
                using (var form = new CadastroPilotoForm(_servicos, _local, piloto))
                {
                    form.ShowDialog(this);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            };
            layout.Controls.Add(addMatrizBtn, 1, 2);

            var salvar = new Button { Text = "Salvar Alterações e Fechar", AutoSize = true };
            salvar.Click += delegate { Salvar(); };
            var cancelar = new Button { Text = "Cancelar", AutoSize = true };
            cancelar.Click += delegate { Close(); };

            var botoes = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 20, 0, 0) };
            botoes.Controls.Add(cancelar); botoes.Controls.Add(salvar);
            layout.Controls.Add(botoes, 1, 3);

            Controls.Add(layout);
            servicos.Tema.Aplicar(this);
        }

        private void Salvar()
        {
            RegistroBloqueio dono = null; string caminho = null;
            try
            {
                caminho = _servicos.Bloqueios.ObterCaminho(_local.PastaRaizDados, _piloto.Codigo);
                dono = _servicos.Bloqueios.CriarPorCodigo(_local.PastaRaizDados, _piloto.Codigo, _piloto.Id, "Edição", _local.NomeExibido);
                _caminho = caminho; _bloqueio = dono; _heartbeat.Start();

                var editado = _servicos.EdicaoPiloto.Salvar(_local.PastaRaizDados, _piloto, _nome.Text, new Dictionary<Guid, string>(), _observacao.Text);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex) { _servicos.Log.Registrar(ex, "Conflito de versão na edição visual", _local.PastaRaizDados); MessageBox.Show(this, "Este cadastro foi alterado por outra pessoa. Feche esta ficha, atualize a pesquisa e abra novamente.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidDataException || ex is System.IO.IOException) { _servicos.Log.Registrar(ex, "Falha na edição visual", _local.PastaRaizDados); MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { _heartbeat.Stop(); if (dono != null && caminho != null) try { _servicos.Bloqueios.Remover(caminho, dono); } catch { } _bloqueio = null; }
        }
    }
}