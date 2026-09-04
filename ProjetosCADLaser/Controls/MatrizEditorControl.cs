using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Controls
{
    public sealed class MatrizEditorControl : UserControl
    {
        private readonly ComboBox _tipo =
            new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 160
            };

        private readonly ComboBox _material =
            new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120
            };

        private readonly ComboBox _eixos =
            new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 80
            };

        private readonly ComboBox _maquina =
            new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 100
            };

        private readonly ComboBox _acabamento =
            new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 110
            };

        public MatrizEditorControl(
            IEnumerable<string> tiposMatriz = null)
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var tipos =
                tiposMatriz ??
                new[]
                {
                    "Gravação",
                    "Tampa",
                    "Laterais"
                };

            foreach (var tipo in tipos)
                _tipo.Items.Add(tipo);

            _material.Items.AddRange(
                new object[]
                {
                    "Zamak",
                    "Aço",
                    "Alumínio"
                });

            _eixos.Items.AddRange(
                new object[]
                {
                    "3x",
                    "5x"
                });

            _maquina.Items.AddRange(
                new object[]
                {
                    "1000",
                    "1200P",
                    "1200S"
                });

            _acabamento.Items.AddRange(
                new object[]
                {
                    "Fosco",
                    "Polido"
                });

            Limpar();

            var layout =
                new TableLayoutPanel
                {
                    AutoSize = true,
                    AutoSizeMode =
                        AutoSizeMode.GrowAndShrink,
                    ColumnCount = 5,
                    RowCount = 2
                };

            layout.Controls.Add(
                new Label
                {
                    Text = "Tipo de matriz",
                    AutoSize = true
                },
                0,
                0);

            layout.Controls.Add(
                new Label
                {
                    Text = "Material",
                    AutoSize = true
                },
                1,
                0);

            layout.Controls.Add(
                new Label
                {
                    Text = "Eixos",
                    AutoSize = true
                },
                2,
                0);

            layout.Controls.Add(
                new Label
                {
                    Text = "Máquina",
                    AutoSize = true
                },
                3,
                0);

            layout.Controls.Add(
                new Label
                {
                    Text = "Acabamento",
                    AutoSize = true
                },
                4,
                0);

            layout.Controls.Add(_tipo, 0, 1);
            layout.Controls.Add(_material, 1, 1);
            layout.Controls.Add(_eixos, 2, 1);
            layout.Controls.Add(_maquina, 3, 1);
            layout.Controls.Add(_acabamento, 4, 1);

            Controls.Add(layout);
        }

        public bool TentarCriarMatriz(
            out MatrizCadastro matriz,
            out string erro)
        {
            matriz = null;
            erro = null;

            if (_tipo.SelectedIndex < 0)
            {
                erro = "Selecione o tipo de matriz.";
                return false;
            }

            if (_material.SelectedIndex < 0)
            {
                erro = "Selecione o material.";
                return false;
            }

            if (_eixos.SelectedIndex < 0)
            {
                erro = "Selecione os eixos.";
                return false;
            }

            if (_maquina.SelectedIndex < 0)
            {
                erro = "Selecione a máquina.";
                return false;
            }

            if (_acabamento.SelectedIndex < 0)
            {
                erro = "Selecione o acabamento.";
                return false;
            }

            MaterialMatriz material;

            if (_material.SelectedIndex == 1)
                material = MaterialMatriz.Aco;
            else if (_material.SelectedIndex == 2)
                material = MaterialMatriz.Aluminio;
            else
                material = MaterialMatriz.Zamak;

            QuantidadeEixos eixos =
                _eixos.SelectedIndex == 1
                    ? QuantidadeEixos.Cinco
                    : QuantidadeEixos.Tres;

            Maquina maquina;

            if (_maquina.SelectedIndex == 1)
                maquina = Maquina.M1200P;
            else if (_maquina.SelectedIndex == 2)
                maquina = Maquina.M1200S;
            else
                maquina = Maquina.M1000;

            Acabamento acabamento =
                _acabamento.SelectedIndex == 1
                    ? Acabamento.Polido
                    : Acabamento.Fosco;

            matriz =
                new MatrizCadastro
                {
                    Tipo = _tipo.Text,
                    Material = material,
                    Eixos = eixos,
                    Maquina = maquina,
                    Acabamento = acabamento
                };

            return true;
        }

        public void Limpar()
        {
            _tipo.SelectedIndex = -1;
            _material.SelectedIndex = -1;
            _eixos.SelectedIndex = -1;
            _maquina.SelectedIndex = -1;
            _acabamento.SelectedIndex = -1;
        }

        // Mantidos temporariamente para o
        // ComponenteEditorControl antigo compilar.
        public string Tipo
        {
            get { return _tipo.Text; }
        }

        public MaterialMatriz Material
        {
            get
            {
                return _material.SelectedIndex == 1
                    ? MaterialMatriz.Aco
                    : _material.SelectedIndex == 2
                        ? MaterialMatriz.Aluminio
                        : MaterialMatriz.Zamak;
            }
        }

        public QuantidadeEixos Eixos
        {
            get
            {
                return _eixos.SelectedIndex == 1
                    ? QuantidadeEixos.Cinco
                    : QuantidadeEixos.Tres;
            }
        }

        public Maquina Maquina
        {
            get
            {
                if (_maquina.SelectedIndex == 1)
                    return Maquina.M1200P;

                if (_maquina.SelectedIndex == 2)
                    return Maquina.M1200S;

                return Maquina.M1000;
            }
        }

        public Acabamento Acabamento
        {
            get
            {
                return _acabamento.SelectedIndex == 1
                    ? Acabamento.Polido
                    : Acabamento.Fosco;
            }
        }

        // Temporário para manter compatibilidade
        // com o controle antigo.
        public event EventHandler RemoverSolicitado;
    }
}