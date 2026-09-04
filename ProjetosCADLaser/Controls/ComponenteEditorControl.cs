using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Controls
{
    public sealed class ComponenteEditorControl : UserControl
    {
        private readonly Label _nome =
            new Label
            {
                AutoSize = true,
                Font = new System.Drawing.Font(
                    "Segoe UI Semibold",
                    11F)
            };

        private readonly FlowLayoutPanel _listaMatrizes =
            new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

        private readonly MatrizEditorControl _novaMatriz;

        private readonly Button _adicionarMatriz =
            new Button
            {
                Text = "+ Adicionar matriz",
                AutoSize = true
            };

        private readonly Label _mensagem =
            new Label
            {
                AutoSize = true
            };

        private readonly List<MatrizCadastro>
            _matrizesAdicionadas =
                new List<MatrizCadastro>();

        public ComponenteEditorControl(
            string nome,
            IEnumerable<string> tiposMatriz = null)
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            Padding = new Padding(12);
            Margin = new Padding(0, 0, 0, 16);

            _nome.Text = nome;

            _novaMatriz =
                new MatrizEditorControl(tiposMatriz);

            var layout =
                new TableLayoutPanel
                {
                    AutoSize = true,
                    AutoSizeMode =
                        AutoSizeMode.GrowAndShrink,
                    ColumnCount = 1,
                    RowCount = 7
                };

            layout.Controls.Add(
                _nome,
                0,
                0);

            layout.Controls.Add(
                new Label
                {
                    Text = "Matrizes adicionadas",
                    AutoSize = true,
                    Margin = new Padding(0, 10, 0, 4)
                },
                0,
                1);

            layout.Controls.Add(
                _listaMatrizes,
                0,
                2);

            layout.Controls.Add(
                new Label
                {
                    Text = "Nova matriz",
                    AutoSize = true,
                    Margin = new Padding(0, 12, 0, 4)
                },
                0,
                3);

            layout.Controls.Add(
                _novaMatriz,
                0,
                4);

            layout.Controls.Add(
                _adicionarMatriz,
                0,
                5);

            layout.Controls.Add(
                _mensagem,
                0,
                6);

            Controls.Add(layout);

            _adicionarMatriz.Click += delegate
            {
                AdicionarMatriz();
            };

            AtualizarListaMatrizes();
        }

        public string NomeComponente
        {
            get { return _nome.Text; }
        }

        public IEnumerable<MatrizCadastro> Matrizes
        {
            get { return _matrizesAdicionadas; }
        }

        private void AdicionarMatriz()
        {
            MatrizCadastro matriz;
            string erro;

            if (!_novaMatriz.TentarCriarMatriz(
                out matriz,
                out erro))
            {
                _mensagem.Text = erro;
                return;
            }

            _matrizesAdicionadas.Add(matriz);

            _novaMatriz.Limpar();

            _mensagem.Text =
                "Matriz adicionada.";

            AtualizarListaMatrizes();
        }

        private void AtualizarListaMatrizes()
        {
            _listaMatrizes.SuspendLayout();

            _listaMatrizes.Controls.Clear();

            if (_matrizesAdicionadas.Count == 0)
            {
                _listaMatrizes.Controls.Add(
                    new Label
                    {
                        Text = "Nenhuma matriz adicionada.",
                        AutoSize = true
                    });

                _listaMatrizes.ResumeLayout();
                return;
            }

            for (var i = 0;
                i < _matrizesAdicionadas.Count;
                i++)
            {
                var indice = i;
                var matriz =
                    _matrizesAdicionadas[indice];

                matriz.NomeExibicao =
                    "Matriz " + (indice + 1);

                var linha =
                    new FlowLayoutPanel
                    {
                        AutoSize = true,
                        AutoSizeMode =
                            AutoSizeMode.GrowAndShrink,
                        WrapContents = false
                    };

                var texto =
                    "Matriz " + (indice + 1) +
                    " — " +
                    matriz.Tipo +
                    " | " +
                    TextoMaterial(matriz.Material) +
                    " | " +
                    TextoEixos(matriz.Eixos) +
                    " | " +
                    TextoMaquina(matriz.Maquina) +
                    " | " +
                    TextoAcabamento(matriz.Acabamento);

                linha.Controls.Add(
                    new Label
                    {
                        Text = texto,
                        AutoSize = true,
                        Margin = new Padding(
                            0,
                            7,
                            10,
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
                    _matrizesAdicionadas.RemoveAt(
                        indice);

                    AtualizarListaMatrizes();
                };

                linha.Controls.Add(remover);

                _listaMatrizes.Controls.Add(linha);
            }

            _listaMatrizes.ResumeLayout();
        }

        private static string TextoMaterial(
            MaterialMatriz? valor)
        {
            if (!valor.HasValue)
                return "-";

            if (valor.Value ==
                MaterialMatriz.Aco)
                return "Aço";

            if (valor.Value ==
                MaterialMatriz.Aluminio)
                return "Alumínio";

            return "Zamak";
        }

        private static string TextoEixos(
            QuantidadeEixos? valor)
        {
            if (!valor.HasValue)
                return "-";

            return valor.Value ==
                QuantidadeEixos.Cinco
                    ? "5x"
                    : "3x";
        }

        private static string TextoMaquina(
            Maquina? valor)
        {
            if (!valor.HasValue)
                return "-";

            if (valor.Value ==
                Maquina.M1200P)
                return "1200P";

            if (valor.Value ==
                Maquina.M1200S)
                return "1200S";

            return "1000";
        }

        private static string TextoAcabamento(
            Acabamento? valor)
        {
            if (!valor.HasValue)
                return "-";

            return valor.Value ==
                Acabamento.Polido
                    ? "Polido"
                    : "Fosco";
        }
    }
}