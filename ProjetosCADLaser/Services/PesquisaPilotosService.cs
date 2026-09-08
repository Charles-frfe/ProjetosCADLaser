using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class ResultadoPesquisaPilotos { public IReadOnlyList<PilotoCadastro> Pilotos { get; private set; } public int Invalidos { get; private set; } public ResultadoPesquisaPilotos(IReadOnlyList<PilotoCadastro> pilotos, int invalidos) { Pilotos = pilotos; Invalidos = invalidos; } }
    public sealed class PesquisaPilotosService
    {
        private readonly JsonService _json; private readonly LogService _log;
        public PesquisaPilotosService(JsonService json, LogService log) { _json = json; _log = log; }
        public ResultadoPesquisaPilotos Carregar(string raizDados)
        {
            var encontrados = new List<PilotoCadastro>(); var invalidos = 0; var pasta = Path.Combine(raizDados, "Cadastros"); if (!Directory.Exists(pasta)) return new ResultadoPesquisaPilotos(encontrados, 0);
            foreach (var arquivo in Directory.GetFiles(pasta, "cadastro.json", SearchOption.AllDirectories)) { if (arquivo.IndexOf(Path.DirectorySeparatorChar + "Lixeira" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) >= 0) continue; try { encontrados.Add(_json.Read<PilotoCadastro>(arquivo)); } catch (Exception ex) { invalidos++; _log.Registrar(ex, "Falha ao carregar cadastro para pesquisa", raizDados); } }
            return new ResultadoPesquisaPilotos(encontrados.OrderByDescending(x => x.UltimaEdicaoEm).ToList(), invalidos);
        }
        public IReadOnlyList<PilotoCadastro> Filtrar(IEnumerable<PilotoCadastro> pilotos, string termo)
        {
            var normalizado = NormalizadorPesquisa.Normalizar(termo ?? string.Empty); if (normalizado.Length == 0) return pilotos.OrderByDescending(x => x.UltimaEdicaoEm).ToList();
            return pilotos.Where(p => NormalizadorPesquisa.Contem(p.Codigo, normalizado) || NormalizadorPesquisa.Contem(p.NomeModelo, normalizado) || p.Texturas.Any(t => NormalizadorPesquisa.Contem(t.Nome, normalizado)) || p.Anexos.Any(a => NormalizadorPesquisa.Contem(a.Titulo ?? string.Empty, normalizado) || NormalizadorPesquisa.Contem(a.NomeOriginal, normalizado))).ToList();
        }
    }
}
