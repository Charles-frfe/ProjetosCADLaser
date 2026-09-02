using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class ResultadoDiagnostico { public string Nome, Estado, Detalhe; public ResultadoDiagnostico(string nome, string estado, string detalhe) { Nome = nome; Estado = estado; Detalhe = detalhe; } }
    public sealed class DiagnosticoService
    {
        private readonly AppServices _servicos; public DiagnosticoService(AppServices servicos) { _servicos = servicos; }
        public IReadOnlyList<ResultadoDiagnostico> Executar(string raiz)
        {
            var resultados = new List<ResultadoDiagnostico>(); var versao = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Version; resultados.Add(new ResultadoDiagnostico("Versão", "OK", versao == null ? "desconhecida" : versao.ToString(3))); resultados.Add(new ResultadoDiagnostico("Usuário", "OK", Environment.UserName)); resultados.Add(new ResultadoDiagnostico("Computador", "OK", Environment.MachineName)); if (string.IsNullOrWhiteSpace(raiz) || !Directory.Exists(raiz)) { resultados.Add(new ResultadoDiagnostico("Pasta de dados", "Erro", "Pasta não configurada ou indisponível.")); return resultados; } resultados.Add(new ResultadoDiagnostico("Pasta de dados", "OK", "Acessível")); var acesso = _servicos.AcessoPasta.Testar(raiz); resultados.Add(new ResultadoDiagnostico("Leitura e escrita", acesso.Sucesso ? "OK" : "Erro", acesso.Sucesso ? "Operação temporária concluída e removida." : "A pasta não permite escrita segura.")); var config = Path.Combine(raiz, "Configuracoes", "configuracao.json"); resultados.Add(new ResultadoDiagnostico("Configurações", File.Exists(config) ? "OK" : "Atenção", File.Exists(config) ? "Configuração compartilhada disponível." : "Configuração compartilhada ausente.")); resultados.Add(new ResultadoDiagnostico("Perfis", "OK", "Perfis Operacional e Consulta reconhecidos.")); var pilotos = _servicos.PesquisaPilotos.Carregar(raiz); resultados.Add(new ResultadoDiagnostico("Cadastros de pilotos", pilotos.Invalidos == 0 ? "OK" : "Atenção", pilotos.Pilotos.Count + " válidos; " + pilotos.Invalidos + " inválidos.")); var testesDir = Path.Combine(raiz, "TestesTextura"); var testesValidos = _servicos.TestesTextura.Listar(raiz).Count; var testesJson = Directory.Exists(testesDir) ? Directory.GetFiles(testesDir, "teste.json", SearchOption.AllDirectories).Length : 0; resultados.Add(new ResultadoDiagnostico("Testes de textura", testesJson == testesValidos ? "OK" : "Atenção", testesValidos + " válidos; " + (testesJson - testesValidos) + " inválidos.")); var lixeira = _servicos.Lixeira.Listar(raiz, TipoRegistroLixeira.Piloto).Count + _servicos.Lixeira.Listar(raiz, TipoRegistroLixeira.TesteTextura).Count; resultados.Add(new ResultadoDiagnostico("Lixeira", "OK", lixeira + " item(ns).")); resultados.Add(new ResultadoDiagnostico("Bloqueios", "OK", _servicos.Bloqueios.Listar(raiz).Count + " bloqueio(s) ativo(s).")); resultados.Add(new ResultadoDiagnostico("Logs", "OK", Directory.Exists(Path.Combine(raiz, "Logs")) ? "Pasta de logs disponível." : "Logs locais serão usados enquanto a pasta estiver indisponível.")); return resultados;
        }
        public string Exportar(string destino, string raiz) { var linhas = new List<string> { "Diagnóstico Projetos CAD/LASER", "Data: " + DateTimeOffset.Now.ToString("g") }; linhas.AddRange(Executar(raiz).Select(x => x.Estado + ": " + x.Nome + " — " + x.Detalhe)); File.WriteAllLines(destino, linhas); return destino; }
    }
}
