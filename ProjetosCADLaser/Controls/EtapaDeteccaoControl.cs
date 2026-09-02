using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Controls
{
    public sealed class EtapaDeteccaoControl : UserControl
    {
        private readonly ListBox _detectados = new ListBox { Dock = DockStyle.Fill, Height = 100 };
        private readonly CheckedListBox _componentes = new CheckedListBox { Dock = DockStyle.Fill, Height = 80, CheckOnClick = true };
        private readonly CheckedListBox _texturas = new CheckedListBox { Dock = DockStyle.Fill, Height = 80, CheckOnClick = true };

        public EtapaDeteccaoControl()
        {
            Dock = DockStyle.Fill; AutoSize = true;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, AutoSize = true };
            layout.Controls.Add(new Label { Text = "Itens detectados (revisão)", AutoSize = true }, 0, 0); layout.Controls.Add(_detectados, 1, 0);
            layout.Controls.Add(new Label { Text = "Componentes confirmados", AutoSize = true }, 0, 1); layout.Controls.Add(_componentes, 1, 1);
            layout.Controls.Add(new Label { Text = "Texturas confirmadas", AutoSize = true }, 0, 2); layout.Controls.Add(_texturas, 1, 2); Controls.Add(layout);
        }

        public IEnumerable<string> ComponentesSelecionados { get { return _componentes.CheckedItems.Cast<object>().Select(x => x.ToString()); } }
        public IEnumerable<string> TexturasSelecionadas { get { return _texturas.CheckedItems.Cast<object>().Select(x => x.ToString()); } }
        public void AplicarAnalise(AnalisePasta analise) { _detectados.Items.Clear(); _componentes.Items.Clear(); _texturas.Items.Clear(); foreach (var item in analise.Itens) _detectados.Items.Add(item.Classificacao + ": " + item.CaminhoRelativo); foreach (var componente in analise.Componentes) { var indice = _componentes.Items.Add(componente.Nome); _componentes.SetItemChecked(indice, true); } foreach (var textura in analise.Texturas) { var indice = _texturas.Items.Add(textura); _texturas.SetItemChecked(indice, true); } }
    }
}
