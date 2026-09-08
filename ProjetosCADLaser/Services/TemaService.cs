using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class PaletaTema { public Color Fundo, Superficie, Texto, TextoSecundario, Destaque, DestaqueHover, Borda, Perigo, Sucesso, Atencao, ComponentAccent; public PaletaTema(Color fundo, Color superficie, Color texto, Color textoSecundario, Color destaque, Color destaqueHover, Color borda, Color perigo, Color sucesso, Color atencao, Color componentAccent) { Fundo = fundo; Superficie = superficie; Texto = texto; TextoSecundario = textoSecundario; Destaque = destaque; DestaqueHover = destaqueHover; Borda = borda; Perigo = perigo; Sucesso = sucesso; Atencao = atencao; ComponentAccent = componentAccent; } }
    public sealed class TipografiaTema { public Font TituloPagina, TituloSecao, TituloCard, Corpo, CorpoSemibold, Legenda; public TipografiaTema(Font a, Font b, Font c, Font d, Font e, Font f) { TituloPagina = a; TituloSecao = b; TituloCard = c; Corpo = d; CorpoSemibold = e; Legenda = f; } }
    public static class EspacamentoTema { public const int Xs = 4, Sm = 8, Md = 12, Lg = 16, Xl = 24, Xxl = 32; }
    public sealed class TemaService
    {
        public event EventHandler TemaAlterado; public PreferenciaTema PreferenciaAtual { get; private set; }
        public TemaService() { PreferenciaAtual = PreferenciaTema.SeguirWindows; }
        public TipografiaTema Tipografia { get { return new TipografiaTema(new Font("Segoe UI Semibold", 24F), new Font("Segoe UI Semibold", 16F), new Font("Segoe UI Semibold", 12F), new Font("Segoe UI", 10F), new Font("Segoe UI Semibold", 10F), new Font("Segoe UI", 9F)); } }
        public PaletaTema Paleta { get { return TemaEscuroAtivo() ? new PaletaTema(Color.FromArgb(24, 28, 34), Color.FromArgb(35, 40, 48), Color.FromArgb(240, 242, 245), Color.FromArgb(174, 181, 190), Color.FromArgb(76, 125, 224), Color.FromArgb(93, 143, 239), Color.FromArgb(63, 70, 81), Color.FromArgb(205, 73, 73), Color.FromArgb(93, 190, 120), Color.FromArgb(230, 180, 70), Color.FromArgb(70, 170, 160)) : new PaletaTema(Color.FromArgb(243, 245, 248), Color.White, Color.FromArgb(31, 37, 46), Color.FromArgb(93, 102, 115), Color.FromArgb(43, 99, 217), Color.FromArgb(31, 82, 184), Color.FromArgb(210, 215, 223), Color.FromArgb(184, 50, 50), Color.FromArgb(35, 130, 75), Color.FromArgb(170, 110, 0), Color.FromArgb(20, 120, 110)); } }
        public void Definir(PreferenciaTema preferencia) { PreferenciaAtual = preferencia; var evento = TemaAlterado; if (evento != null) evento(this, EventArgs.Empty); }
        public void Aplicar(Control raiz) { var p = Paleta; raiz.BackColor = p.Fundo; raiz.ForeColor = p.Texto; AplicarRecursivo(raiz, p); }
        private static void AplicarRecursivo(Control controle, PaletaTema p)
        {
            foreach (Control filho in controle.Controls)
            {
                filho.ForeColor = p.Texto; if (filho is Button) filho.BackColor = p.Destaque; else if (filho is TextBox || filho is ComboBox || filho is NumericUpDown || filho is ListBox || filho is DataGridView || filho is TabPage || filho is Panel || filho is TableLayoutPanel || filho is FlowLayoutPanel || filho is GroupBox) filho.BackColor = p.Superficie; else filho.BackColor = controle.BackColor;
                var botao = filho as Button; if (botao != null) { botao.FlatStyle = FlatStyle.Flat; botao.FlatAppearance.BorderSize = 0; botao.FlatAppearance.MouseOverBackColor = p.DestaqueHover; botao.ForeColor = Color.White; botao.Cursor = Cursors.Hand; } var texto = filho as TextBox; if (texto != null) texto.BorderStyle = BorderStyle.FixedSingle; var grade = filho as DataGridView; if (grade != null) { grade.BackgroundColor = p.Superficie; grade.GridColor = p.Borda; grade.BorderStyle = BorderStyle.None; grade.EnableHeadersVisualStyles = false; grade.ColumnHeadersDefaultCellStyle.BackColor = p.Fundo; grade.ColumnHeadersDefaultCellStyle.ForeColor = p.Texto; grade.DefaultCellStyle.BackColor = p.Superficie; grade.DefaultCellStyle.ForeColor = p.Texto; grade.DefaultCellStyle.SelectionBackColor = p.Destaque; grade.DefaultCellStyle.SelectionForeColor = Color.White; } AplicarRecursivo(filho, p);
            }
        }
        private bool TemaEscuroAtivo() { if (PreferenciaAtual == PreferenciaTema.Escuro) return true; if (PreferenciaAtual == PreferenciaTema.Claro) return false; return WindowsUsaTemaEscuro(); }
        private static bool WindowsUsaTemaEscuro() { try { var valor = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1); return valor is int && (int)valor == 0; } catch { return false; } }
    }
}
