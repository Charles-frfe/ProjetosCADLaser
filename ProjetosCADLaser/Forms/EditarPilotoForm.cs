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

        // Novos controles para Adicionar Matriz
        private readonly ComboBox _comboComponentes = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        private MatrizEditorControl _editorMatriz;

        public EditarPilotoForm(AppServices servicos, ConfiguracaoLocal local, PilotoCadastro piloto)
        {
            _servicos = servicos; _local = local; _piloto = piloto;
            Text = "Editar piloto — " + piloto.Codigo;
            Size = new Size(680, 500);
            StartPosition = FormStartPosition.CenterParent;
            _nome.Text = piloto.NomeModelo;

            _heartbeat.Tick += delegate { if (_bloqueio != null) try { _servicos.Bloqueios.Atualizar(_caminho, _bloqueio); } catch { } };
            FormClosed += delegate { _heartbeat.Stop(); if (_bloqueio != null) try { _servicos.Bloqueios.Remover(_caminho, _bloqueio); } catch { } _bloqueio = null; };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 2, RowCount = 7 };

            layout.Controls.Add(new Label { Text = "Nome do modelo", AutoSize = true }, 0, 0);
            layout.Controls.Add(_nome, 1, 0);

            layout.Controls.Add(new Label { Text = "Nova Observação", AutoSize = true }, 0, 1);
            layout.Controls.Add(_observacao, 1, 1);

            // Adição de Matriz semelhante ao Cadastro
            layout.Controls.Add(new Label { Text = "--- Adicionar Nova Matriz ---", AutoSize = true, Margin = new Padding(0, 15, 0, 5), Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 0, 2);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 2), 2);

            layout.Controls.Add(new Label { Text = "Componente Alvo", AutoSize = true }, 0, 3);
            foreach (var comp in _piloto.Componentes)
            {
                _comboComponentes.Items.Add(comp.Nome);
            }
            if (_comboComponentes.Items.Count > 0) _comboComponentes.SelectedIndex = 0;
            layout.Controls.Add(_comboComponentes, 1, 3);

            // Carrega as texturas do piloto para o editor de matrizes
            _editorMatriz = new MatrizEditorControl(null, _piloto.Texturas.Select(t => t.Nome));
            layout.Controls.Add(_editorMatriz, 0, 4);
            layout.SetColumnSpan(_editorMatriz, 2);

            var btnAdicionarMatriz = new Button { Text = "Adicionar Matriz ao Componente", AutoSize = true, Width = 250 };
            btnAdicionarMatriz.Click += BtnAdicionarMatriz_Click;
            layout.Controls.Add(btnAdicionarMatriz, 1, 5);

            var salvar = new Button { Text = "Salvar Alterações e Fechar", AutoSize = true };
            salvar.Click += delegate { Salvar(); };
            var cancelar = new Button { Text = "Cancelar", AutoSize = true };
            cancelar.Click += delegate { Close(); };

            var botoes = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 20, 0, 0) };
            botoes.Controls.Add(cancelar); botoes.Controls.Add(salvar);
            layout.Controls.Add(botoes, 1, 6);

            Controls.Add(layout);
            servicos.Tema.Aplicar(this);
        }

        private void BtnAdicionarMatriz_Click(object sender, EventArgs e)
        {
            if (_comboComponentes.SelectedIndex < 0) return;
            string nomeComp = _comboComponentes.SelectedItem.ToString();
            var comp = _piloto.Componentes.Find(c => c.Nome == nomeComp);
            if (comp == null) return;

            MatrizCadastro novaMatriz;
            string erro;
            if (!_editorMatriz.TentarCriarMatriz(out novaMatriz, out erro))
            {
                MessageBox.Show(this, erro, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var texNome in _editorMatriz.TexturasSelecionadas)
            {
                var textura = _piloto.Texturas.Find(t => t.Nome == texNome);
                if (textura != null)
                {
                    novaMatriz.TexturasIds.Add(textura.Id);
                }
            }

            // A inserção direta no objeto será salva no disco quando clicarmos em Salvar
            comp.Matrizes.Add(novaMatriz);
            MessageBox.Show(this, $"Matriz '{novaMatriz.Tipo}' adicionada ao componente '{comp.Nome}' com sucesso!\n\nAs alterações só serão efetivadas no disco ao clicar em 'Salvar Alterações e Fechar'.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _editorMatriz.Limpar();
        }

        private void Salvar()
        {
            RegistroBloqueio dono = null; string caminho = null;
            try
            {
                caminho = _servicos.Bloqueios.ObterCaminho(_local.PastaRaizDados, _piloto.Codigo);
                dono = _servicos.Bloqueios.CriarPorCodigo(_local.PastaRaizDados, _piloto.Codigo, _piloto.Id, "Edição", _local.NomeExibido);
                _caminho = caminho; _bloqueio = dono; _heartbeat.Start();

                // Salva tudo e as matrizes que adicionamos no objeto já estarão persistidas em json
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