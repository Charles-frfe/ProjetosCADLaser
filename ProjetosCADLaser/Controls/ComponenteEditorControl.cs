using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly FlowLayoutPanel _matrizes =
            new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode=AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

        private readonly Button _adicionarMatriz =
            new Button
            {
                Text = "+ Adicionar matriz",
                AutoSize = true
            };

        private readonly IEnumerable<string> _tiposMatriz;

        public ComponenteEditorControl(
            string nome,
            IEnumerable<string> tiposMatriz = null)
        {
            _tiposMatriz = tiposMatriz;

            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(12);
            Margin = new Padding(0, 0, 0, 12);

            _nome.Text = nome;

            var layout = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode=AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 4
            };

            layout.Controls.Add(_nome, 0, 0);

            layout.Controls.Add(
                new Label
                {
                    Text = "Matrizes",
                    AutoSize = true,
                    Margin = new Padding(0, 10, 0, 4)
                },
                0,
                1);

            layout.Controls.Add(_matrizes, 0, 2);
            layout.Controls.Add(_adicionarMatriz, 0, 3);

            Controls.Add(layout);

            _adicionarMatriz.Click += delegate
            {
                AdicionarMatriz();
            };

            // Começa com uma matriz,
            // mas o usuário poderá remover ou adicionar outras.
            AdicionarMatriz();
        }

        public string NomeComponente
        {
            get { return _nome.Text; }
        }

        public IEnumerable<MatrizCadastro> Matrizes
        {
            get
            {
                foreach (var editor in
                    _matrizes.Controls
                        .OfType<MatrizEditorControl>())
                {
                    yield return new MatrizCadastro
                    {
                        Tipo = editor.Tipo,
                        Material = editor.Material,
                        Eixos = editor.Eixos,
                        Maquina = editor.Maquina,
                        Acabamento = editor.Acabamento
                    };
                }
            }
        }

        private void AdicionarMatriz()
        {
            var editor =
                new MatrizEditorControl(_tiposMatriz);

            editor.RemoverSolicitado += delegate
            {
                _matrizes.Controls.Remove(editor);
                editor.Dispose();
            };

            _matrizes.Controls.Add(editor);
        }
    }
}