using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjetosCADLaser.Services
{
    public static class NormalizadorPesquisa
    {
        private static readonly Regex RegexEspacos = new Regex(@"\s+", RegexOptions.Compiled);
        public static string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            var decomposed = texto.Normalize(NormalizationForm.FormD); var builder = new StringBuilder(decomposed.Length);
            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                builder.Append(c == '_' || c == '-' ? ' ' : char.ToLowerInvariant(c));
            }
            return RegexEspacos.Replace(builder.ToString().Normalize(NormalizationForm.FormC).Trim(), " ");
        }
        public static bool Contem(string texto, string termo) { return Normalizar(texto).IndexOf(Normalizar(termo), System.StringComparison.Ordinal) >= 0; }
    }
}
