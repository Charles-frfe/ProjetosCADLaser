using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Controls
{
    public sealed class EtapaDeteccaoControl : UserControl
    {
        private readonly CheckedListBox _componentes = new CheckedListBox { Dock = DockStyle.Fill, Height = 80, CheckOnClick = true };
        private readonly CheckedListBox _texturas = new CheckedListBox { Dock = DockStyle.Fill, Height = 80, CheckOnClick = true };
        private readonly TextBox _novoComponente = new TextBox { Width = 220 };
        private readonly Button _adicionarComponente = new Button { Text = "+Adicionar componente", AutoSize = true };
        public event EventHandler ComponentesAlterados;
        public EtapaDeteccaoControl()
        {
            Dock = DockStyle.Fill; AutoSize = true;
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                AutoSize = true
            };

            layout.Controls.Add(new Label { Text = "Componentes confirmados", AutoSize = true }, 0, 0);
            layout.Controls.Add(_componentes, 1, 0);

            var linhaAdicionar = new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                WrapContents = false
            };

            linhaAdicionar.Controls.Add(_novoComponente);
            linhaAdicionar.Controls.Add(_adicionarComponente);

            layout.Controls.Add(
                new Label
                {
                    Text = "Adicionar componente",
                    AutoSize = true
                },
                0,
                1);

            layout.Controls.Add(linhaAdicionar, 1, 1);
            layout.Controls.Add(new Label { Text = "Texturas confirmadas", AutoSize = true }, 0, 2);
            _componentes.ItemCheck += delegate
              {
                  BeginInvoke(new Action(delegate
                  {
                      if (ComponentesAlterados != null)
                          ComponentesAlterados(this, EventArgs.Empty);
                  }));
              };

            layout.Controls.Add(_texturas, 1, 2); Controls.Add(layout);
            _adicionarComponente.Click += delegate
            {
                var nome = _novoComponente.Text.Trim();

                if (string.IsNullOrWhiteSpace(nome))
                    return;

                foreach (var item in _componentes.Items)
                {
                    if (string.Equals(
                        item.ToString(),
                        nome,
                        System.StringComparison.OrdinalIgnoreCase))
                    {
                        _novoComponente.Clear();
                        return;
                    }
                }

                var indice = _componentes.Items.Add(nome);
                _componentes.SetItemChecked(indice, true);
                if (ComponentesAlterados != null)
                    ComponentesAlterados(this, EventArgs.Empty);
                _novoComponente.Clear();
                _novoComponente.Focus();
            };
        }

        public IEnumerable<string> ComponentesSelecionados { get { return _componentes.CheckedItems.Cast<object>().Select(x => x.ToString()); } }
        public IEnumerable<string> TexturasSelecionadas { get { return _texturas.CheckedItems.Cast<object>().Select(x => x.ToString()); } }
        public void AplicarAnalise(AnalisePasta analise)
        {
            _componentes.Items.Clear();
            _texturas.Items.Clear();
            if (ComponentesAlterados != null)
                ComponentesAlterados(this, EventArgs.Empty);

            foreach (var componente in analise.Componentes)
            {
                var indice =
                    _componentes.Items.Add(componente.Nome); _componentes.SetItemChecked(indice, true);
            }
            foreach (var textura in analise.Texturas)
            {
                var indice = _texturas.Items.Add(textura);
                _texturas.SetItemChecked(indice, true); } }
    }
}
