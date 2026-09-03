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
                Width = 110
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
                Width = 100
            };

        private readonly Button _remover =
            new Button
            {
                Text = "Remover",
                AutoSize = true
            };

        public MatrizEditorControl(
            IEnumerable<string> tiposMatriz = null)
        {
            AutoSize = true;
            Dock = DockStyle.Top;

            var tipos = tiposMatriz ??
                new[]
                {
                    "Gravação",
                    "Tampa",
                    "Laterais"
                };

            foreach (var tipo in tipos)
                _tipo.Items.Add(tipo);

            if (_tipo.Items.Count == 0)
                _tipo.Items.Add("Gravação");

            _tipo.SelectedIndex = 0;

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

            _material.SelectedIndex = 0;
            _eixos.SelectedIndex = 0;
            _maquina.SelectedIndex = 0;
            _acabamento.SelectedIndex = 0;

            var layout = new TableLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                ColumnCount = 6,
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
            layout.Controls.Add(_remover, 5, 1);

            Controls.Add(layout);

            _remover.Click += delegate
            {
                if (RemoverSolicitado != null)
                    RemoverSolicitado(this, EventArgs.Empty);
            };
        }

        public string Tipo
        {
            get { return _tipo.Text; }
        }

        public MaterialMatriz Material
        {
            get
            {
                if (_material.SelectedIndex == 1)
                    return MaterialMatriz.Aco;

                if (_material.SelectedIndex == 2)
                    return MaterialMatriz.Aluminio;

                return MaterialMatriz.Zamak;
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

        public event EventHandler RemoverSolicitado;
    }
}