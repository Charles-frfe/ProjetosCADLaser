using System;
using System.IO;
using ProjetosCADLaser.Configuration;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class InicializacaoService
    {
        private readonly ConfiguracaoLocalService _local; private readonly ConfiguracaoCompartilhadaService _compartilhada; private readonly AcessoPastaService _acesso; private readonly PinService _pin;
        public InicializacaoService(ConfiguracaoLocalService local, ConfiguracaoCompartilhadaService compartilhada, AcessoPastaService acesso, PinService pin) { _local = local; _compartilhada = compartilhada; _acesso = acesso; _pin = pin; }
        public bool PrimeiroUsoNecessario(out ConfiguracaoLocal configuracao, out string erro) { return !_local.TentarCarregar(out configuracao, out erro); }
        public ResultadoAcessoPasta VerificarDisponibilidade(ConfiguracaoLocal configuracao) { return _acesso.Testar(configuracao.PastaRaizDados); }
        public void ConcluirPrimeiroUso(string pastaRaiz, string novoPin, string confirmacao, PreferenciaTema tema) { if (novoPin != confirmacao) throw new ArgumentException("A confirmação do PIN é diferente do PIN informado."); var resultado = _acesso.Testar(pastaRaiz); if (!resultado.Sucesso) throw new IOException(resultado.Mensagem); var credencial = _pin.Criar(novoPin); EstruturaDados.Criar(pastaRaiz); _compartilhada.Salvar(pastaRaiz, new ConfiguracaoCompartilhada { PinAdministrativo = credencial }); _local.Salvar(new ConfiguracaoLocal { PastaRaizDados = Path.GetFullPath(pastaRaiz), Tema = tema }); }
    }
}
