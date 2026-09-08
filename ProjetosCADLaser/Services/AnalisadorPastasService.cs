using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class AnalisadorPastasService
    {
        private static readonly Regex TexturaRegex = new Regex(@"^gls?_", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex NumeroEscalaRegex = new Regex(@"^\d{2}(?:\s*(?:[-/]|ao)\s*\d{2})?$", RegexOptions.IgnoreCase);

        public AnalisePasta Analisar(string pastaOrigem, IEnumerable<DefinicaoComponente> definicoes)
        {
            var raiz = new DirectoryInfo(pastaOrigem); if (!raiz.Exists) throw new DirectoryNotFoundException(pastaOrigem);
            var raizIdentificada = IdentificarRaiz(raiz.Name);
            var resultado = new AnalisePasta { PastaOrigem = raiz.FullName, CodigoSugerido = raizIdentificada.Codigo, NomeModeloSugerido = raizIdentificada.NomeModelo };
            var componentes = definicoes.Where(d => d.Ativo).ToList();
            foreach (var pasta in raiz.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                var pastaNormalizada = NormalizadorPesquisa.Normalizar(pasta.Name);
                var componente = componentes.FirstOrDefault(d => PrependAliases(d.Nome, d.Apelidos).Any(alias => NormalizadorPesquisa.Normalizar(alias).Equals(pastaNormalizada, StringComparison.Ordinal)));
                var relativo = CaminhoRelativo(raiz.FullName, pasta.FullName);
                var profundidade = relativo.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries).Length;
                ClassificacaoPasta classificacao; string origem; string valor = null;
                if (componente != null) { resultado.Componentes.Add(new ComponenteDetectado(componente.Nome, pasta.FullName)); classificacao = ClassificacaoPasta.Componente; valor = componente.Nome; origem = NormalizadorPesquisa.Normalizar(componente.Nome) == pastaNormalizada ? "Nome oficial do componente" : "Apelido configurado de " + componente.Nome; }
                else if (TexturaRegex.IsMatch(pasta.Name)) { resultado.Texturas.Add(pasta.Name); classificacao = ClassificacaoPasta.Textura; valor = pasta.Name; origem = "Prefixo gl_ ou gls_"; }
                else if (pastaNormalizada == "piloto") { resultado.PastasPiloto.Add(pasta.FullName); classificacao = ClassificacaoPasta.Piloto; origem = "Nome da pasta"; }
                else if (pastaNormalizada == "escala") { resultado.PastasEscala.Add(pasta.FullName); classificacao = ClassificacaoPasta.Escala; origem = "Nome da pasta"; }
                else if (NumeroEscalaRegex.IsMatch(pasta.Name)) { resultado.NumerosEscala.Add(pasta.Name); classificacao = ClassificacaoPasta.NumeroEscala; valor = pasta.Name; origem = "Padrão numérico de escala"; }
                else { resultado.NaoClassificadas.Add(pasta.FullName); classificacao = ClassificacaoPasta.NaoClassificado; origem = "Nenhuma regra reconheceu o nome"; }
                resultado.Itens.Add(new ItemAnalisePasta { Nome = pasta.Name, CaminhoRelativo = relativo, Profundidade = profundidade, Classificacao = classificacao, OrigemClassificacao = origem, ValorSugerido = valor });
            }
            return resultado;
        }

        public static (string Codigo, string NomeModelo) IdentificarRaiz(string nomePasta)
        {
            var separador = nomePasta.IndexOf('_');
            if (separador <= 0 || separador == nomePasta.Length - 1) return (string.Empty, CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nomePasta.Replace('_', ' ')));
            var codigo = nomePasta.Substring(0, separador).Trim(); var nome = nomePasta.Substring(separador + 1).Replace('_', ' ').Trim();
            return (codigo, CultureInfo.GetCultureInfo("pt-BR").TextInfo.ToTitleCase(nome));
        }

        private static IEnumerable<string> PrependAliases(string nome, IEnumerable<string> aliases) { yield return nome; foreach (var alias in aliases) yield return alias; }
        private static string CaminhoRelativo(string raiz, string caminho)
        {
            var basePath = Path.GetFullPath(raiz).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var full = Path.GetFullPath(caminho); if (full.StartsWith(basePath, StringComparison.OrdinalIgnoreCase)) return full.Substring(basePath.Length);
            return full;
        }
    }
}
