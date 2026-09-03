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
        public void ConcluirPrimeiroUso(
    string pastaRaiz,
    string novoPin,
    string confirmacao,
    PreferenciaTema tema)
        {
            if (novoPin != confirmacao)
                throw new ArgumentException(
                    "A confirmação do PIN é diferente do PIN informado.");

            var pastaCompleta = Path.GetFullPath(pastaRaiz);

            var resultado = _acesso.Testar(pastaCompleta);

            if (!resultado.Sucesso)
                throw new IOException(resultado.Mensagem);

            var caminhoConfiguracao =
                _compartilhada.ObterCaminho(pastaCompleta);

            if (File.Exists(caminhoConfiguracao))
            {
                throw new InvalidOperationException(
                    "Esta pasta já contém uma base do Projetos CAD/LASER.");
            }

            EstruturaDados.Criar(pastaCompleta);

            var configuracaoLocal = new ConfiguracaoLocal
            {
                PastaRaizDados = pastaCompleta,
                Tema = tema,
                NomeExibido = Environment.UserName,
                UsuarioAtivo = true,
                Perfil = PerfilUsuario.Operacional
            };

            // Salvar gera o IdInstalacao permanente deste PC.
            _local.Salvar(configuracaoLocal);

            var configuracaoCompartilhada =
                new ConfiguracaoCompartilhada
                {
                    PinAdministrativo = _pin.Criar(novoPin)
                };

            var administracao =
                new AdministracaoService(_pin);

            administracao.DefinirAdministradorInicial(
                configuracaoCompartilhada,
                configuracaoLocal);

            _compartilhada.Salvar(
                pastaCompleta,
                configuracaoCompartilhada);
        }
    }
}