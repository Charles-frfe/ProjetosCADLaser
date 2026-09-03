using System;
using System.IO;
using System.Text.Json;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class ConfiguracaoLocalService
    {
        private readonly JsonService _json; public string CaminhoPadrao { get; private set; }
        public ConfiguracaoLocalService(JsonService json, string caminhoPersonalizado = null) { _json = json; CaminhoPadrao = caminhoPersonalizado ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProjetosCADLaser", "configuracao.json"); }
        public ConfiguracaoLocal Carregar()
        {
            if (!File.Exists(CaminhoPadrao))
                return new ConfiguracaoLocal();

            var configuracao =
                _json.Read<ConfiguracaoLocal>(CaminhoPadrao);

            if(configuracao.IdInstalacao==Guid.Empty)
            {
                configuracao.IdInstalacao = Guid.NewGuid();
                _json.WriteAtomic(CaminhoPadrao, configuracao);
            }
            return configuracao;
        }
        public bool TentarCarregar(out ConfiguracaoLocal configuracao, out string erro)
        {
            try { configuracao = Carregar(); erro = null; return !string.IsNullOrWhiteSpace(configuracao.PastaRaizDados); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) { configuracao = new ConfiguracaoLocal(); erro = "A configuração local está inválida ou não pôde ser lida."; return false; }
        }
        public void Salvar(ConfiguracaoLocal configuracao)
        {
            JsonContratosV1.ValidarPodeEditar(configuracao);

            if (configuracao.IdInstalacao == Guid.Empty)
                configuracao.IdInstalacao = Guid.NewGuid();

            _json.WriteAtomic(CaminhoPadrao, configuracao); }
    }
    public sealed class ConfiguracaoCompartilhadaService
    {
        private readonly JsonService _json; public ConfiguracaoCompartilhadaService(JsonService json) { _json = json; }
        public string ObterCaminho(string raizDados) { return Path.Combine(raizDados, "Configuracoes", "configuracao.json"); }
        public ConfiguracaoCompartilhada Carregar(string raizDados) { var path = ObterCaminho(raizDados); return File.Exists(path) ? _json.Read<ConfiguracaoCompartilhada>(path) : new ConfiguracaoCompartilhada(); }
        public void Salvar(string raizDados, ConfiguracaoCompartilhada configuracao) { JsonContratosV1.ValidarPodeEditar(configuracao); _json.WriteAtomic(ObterCaminho(raizDados), configuracao); }
    }
}