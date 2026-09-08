using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetosCADLaser.Services
{
    public sealed class ResultadoPesquisaOrigem { public bool RaizDisponivel { get; private set; } public IReadOnlyList<string> PastasEncontradas { get; private set; } public ResultadoPesquisaOrigem(bool raiz, IReadOnlyList<string> pastas) { RaizDisponivel = raiz; PastasEncontradas = pastas; } }
    public sealed class PesquisaPastaOrigemService
    {
        public Task<ResultadoPesquisaOrigem> PesquisarAsync(string pastaOrigemProjetos, string codigo, CancellationToken cancelamento) { return Task.Run(() => Pesquisar(pastaOrigemProjetos, codigo, cancelamento), cancelamento); }
        private static ResultadoPesquisaOrigem Pesquisar(string raiz, string codigo, CancellationToken cancelamento)
        {
            if (string.IsNullOrWhiteSpace(raiz) || !Directory.Exists(raiz)) return new ResultadoPesquisaOrigem(false, new List<string>());
            var prefixo = codigo + "_"; var encontradas = new List<string>(); var pendentes = new Stack<string>(new[] { Path.GetFullPath(raiz) });
            while (pendentes.Count > 0)
            {
                cancelamento.ThrowIfCancellationRequested(); var atual = pendentes.Pop(); IEnumerable<DirectoryInfo> subpastas;
                try { subpastas = new DirectoryInfo(atual).EnumerateDirectories(); } catch (UnauthorizedAccessException) { continue; } catch (DirectoryNotFoundException) { continue; }
                foreach (var pasta in subpastas) { cancelamento.ThrowIfCancellationRequested(); if (pasta.Name.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase)) encontradas.Add(pasta.FullName); pendentes.Push(pasta.FullName); }
            }
            return new ResultadoPesquisaOrigem(true, encontradas.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList());
        }
    }
}
