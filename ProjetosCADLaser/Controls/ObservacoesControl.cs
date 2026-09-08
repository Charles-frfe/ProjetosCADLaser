using System.Collections.Generic;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;
using System.IO;

namespace ProjetosCADLaser.Controls
{
    public sealed class ObservacoesControl : UserControl
    {
        private readonly ListBox _anexos =
    new ListBox
    {
        Width = 650,
        Height = 70
    };

        private readonly Button _adicionarArquivo =
            new Button
            {
                Text = "Adicionar arquivo",
                AutoSize = true
            };

        private readonly Button _adicionarImagem =
            new Button
            {
                Text = "Adicionar imagem",
                AutoSize = true
            };

        private readonly Button _removerAnexo =
            new Button
            {
                Text = "Remover anexo",
                AutoSize = true
            };

        private readonly List<AnexoPendente> _anexosEmEdicao =
            new List<AnexoPendente>();
        private readonly TextBox _texto =
            new TextBox
            {
                Width = 650,
                Multiline = true,
                Height = 60
            };

        private AnexoService _anexoService;
        private string _sessao;
        private readonly Button _adicionar =
            new Button
            {
                Text = "+ Adicionar observação",
                AutoSize = true
            };

        private readonly FlowLayoutPanel _lista =
            new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

        private readonly List<ObservacaoPendente>
            _observacoes =
                new List<ObservacaoPendente>();

        public ObservacoesControl()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var layout =
                new TableLayoutPanel
                {
                    AutoSize = true,
                    AutoSizeMode =
                        AutoSizeMode.GrowAndShrink,
                    ColumnCount = 1,
                    RowCount = 6
                };

            layout.Controls.Add(
                new Label
                {
                    Text = "Observações",
                    AutoSize = true
                },
                0,
                0);

            layout.Controls.Add(
                _texto,
                0,
                1);
            var botoesAnexos =
    new FlowLayoutPanel
    {
        AutoSize = true,
        WrapContents = false
    };

            botoesAnexos.Controls.Add(_adicionarArquivo);
            botoesAnexos.Controls.Add(_adicionarImagem);
            botoesAnexos.Controls.Add(_removerAnexo);

            layout.Controls.Add(
                botoesAnexos,
                0,
                2);

            layout.Controls.Add(
                _anexos,
                0,
                3);
            layout.Controls.Add(
                _adicionar,
                0,
                4);

            layout.Controls.Add(
                _lista,
                0,
                5);

            Controls.Add(layout);

            _adicionar.Click += delegate
            {
                Adicionar();
            };
            _adicionarArquivo.Click += delegate
              {
                  AdicionarAnexo(false);
              };
            _adicionarImagem.Click += delegate
              {
                  AdicionarAnexo(true);
              };
            _removerAnexo.Click += delegate
              {
                  RemoverAnexo();
              };
            AtualizarLista();
        }

        public IReadOnlyList<ObservacaoPendente>
            Observacoes
        {
            get
            {
                return _observacoes.AsReadOnly();
            }
        }
        public void ConfigurarSessao(
            AnexoService servico,
            string sessao)
        {
            _anexoService = servico;
            _sessao = sessao;
        }
        private void AdicionarAnexo(bool imagem)
        {
            if (_anexoService == null ||
                string.IsNullOrWhiteSpace(_sessao))
                return;

            using (var dialogo = new OpenFileDialog())
            {
                dialogo.Multiselect = false;

                dialogo.Title =
                    imagem
                        ? "Selecionar imagem"
                        : "Selecionar arquivo";

                if (imagem)
                {
                    dialogo.Filter =
                        "Imagens|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp|Todos os arquivos|*.*";
                }

                if (dialogo.ShowDialog(this) !=
                    DialogResult.OK)
                    return;

                try
                {
                    var anexo =
                        _anexoService.AdicionarArquivo(
                            _sessao,
                            dialogo.FileName,
                            imagem);

                    _anexosEmEdicao.Add(anexo);

                    _anexos.Items.Add(
                        anexo.NomeOriginal);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(
                        this,
                        ex.Message,
                        "Anexo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }

        private void RemoverAnexo()
        {
            if (_anexos.SelectedIndex < 0 ||
                _anexos.SelectedIndex >=
                _anexosEmEdicao.Count)
                return;

            var indice =
                _anexos.SelectedIndex;

            var anexo =
                _anexosEmEdicao[indice];

            try
            {
                if (File.Exists(
                    anexo.CaminhoTemporario))
                {
                    File.Delete(
                        anexo.CaminhoTemporario);
                }
            }
            catch (IOException)
            {
            }

            _anexosEmEdicao.RemoveAt(indice);
            _anexos.Items.RemoveAt(indice);
        }
        private void Adicionar()
        {
            var texto =
                _texto.Text.Trim();

            if (string.IsNullOrWhiteSpace(texto))
                return;

            var observacao =
                new ObservacaoPendente
                {
                    Texto = texto
                };

            foreach(var anexo in
                _anexosEmEdicao)
            {
                observacao.Anexos.Add(
                    anexo);
            }
            _observacoes.Add(
                observacao);

            _anexosEmEdicao.Clear();
            _anexos.Items.Clear();

            _texto.Clear();

            AtualizarLista();
        }

        private void AtualizarLista()
        {
            _lista.Controls.Clear();

            for (var i = 0;
                i < _observacoes.Count;
                i++)
            {
                var indice = i;

                var linha =
                    new FlowLayoutPanel
                    {
                        AutoSize = true,
                        WrapContents = false
                    };

                var textoExibicao =
    _observacoes[indice].Texto;

                if (_observacoes[indice].Anexos.Count > 0)
                {
                    var nomes =
                        new List<string>();

                    foreach (var anexo in
                        _observacoes[indice].Anexos)
                    {
                        nomes.Add(
                            anexo.NomeOriginal);
                    }

                    textoExibicao +=
                        System.Environment.NewLine +
                        "Anexos: " +
                        string.Join(", ", nomes);
                }

                linha.Controls.Add(
                    new Label
                    {
                        Text = textoExibicao,
                        AutoSize = true,
                        MaximumSize =
                            new System.Drawing.Size(
                                550,
                                0)
                    });

                var remover =
                    new Button
                    {
                        Text = "Remover",
                        AutoSize = true
                    };

                remover.Click += delegate
                {
                    _observacoes.RemoveAt(
                        indice);

                    AtualizarLista();
                };

                linha.Controls.Add(remover);

                _lista.Controls.Add(linha);
            }
        }
    }
}