using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjetosCADLaser.Models
{
    public sealed class ConfiguracaoLocal
    {
        public int VersaoFormato { get; set; }
        public Guid IdInstalacao { get; set; }
        public string PastaRaizDados { get; set; }
        public string PastaOrigemProjetos { get; set; }
        public PreferenciaTema Tema { get; set; }
        public PerfilUsuario Perfil { get; set; }
        public string NomeExibido { get; set; }
        public bool UsuarioAtivo { get; set; }
        public Dictionary<string, string> PreferenciasInterface { get; set; }
        public ConfiguracaoLocal()
        {
            VersaoFormato = 1;

            PastaOrigemProjetos =

                @"\\Clfssrvfar\gr2\CAD_Textura\Projetos_Em_Andamento\PILOTO";
            Tema = PreferenciaTema.SeguirWindows;
            Perfil = PerfilUsuario.Operacional;
            UsuarioAtivo = true;
            PreferenciasInterface = new Dictionary<string, string>(); }
    }
    public sealed class ConfiguracaoCompartilhada
    {
        public int VersaoEstruturaDados { get; set; }
        public Guid IdInstalacaoAdministradora { get; set; }
        public List<ComputadorAutorizado> ComputadoresAutorizados { get; set; }
        public List<DefinicaoComponente> Componentes { get; set; }
        public List<ItemConfiguravel> TiposMatriz { get; set; }
        public int RetencaoLixeiraDias { get; set; }
        public CredencialPin PinAdministrativo { get; set; }
        public ConfiguracaoCompartilhada() { VersaoEstruturaDados = 1; ComputadoresAutorizados = new List<ComputadorAutorizado>(); Componentes = Padroes.Componentes(); TiposMatriz = Padroes.TiposMatriz(); RetencaoLixeiraDias = 7; }
    }
    public sealed class ComputadorAutorizado
    {
        public Guid IdInstalacao { get; set; }

        public string NomeComputador { get; set; }

        public string UsuarioWindows { get; set; }

        public string NomeExibido { get; set; }

        public PerfilUsuario Perfil { get; set; }

        public bool Ativo { get; set; }

        public DateTimeOffset CadastradoEm { get; set; }

        public ComputadorAutorizado()
        {
            NomeComputador = string.Empty;
            UsuarioWindows = string.Empty;
            NomeExibido = string.Empty;
            Perfil = PerfilUsuario.Operacional;
            Ativo = true;
            CadastradoEm = DateTimeOffset.Now;
        }
    }
    public sealed class DefinicaoComponente { public Guid Id { get; set; } public string Nome { get; set; } public List<string> Apelidos { get; set; } public bool Ativo { get; set; } public DefinicaoComponente() { Id = Guid.NewGuid(); Nome = string.Empty; Apelidos = new List<string>(); Ativo = true; } }
    public sealed class ItemConfiguravel { public Guid Id { get; set; } public string Nome { get; set; } public bool Ativo { get; set; } public ItemConfiguravel() { Id = Guid.NewGuid(); Nome = string.Empty; Ativo = true; } }
    public sealed class CredencialPin { public string HashBase64 { get; set; } public string SaltBase64 { get; set; } public int Iteracoes { get; set; } public CredencialPin() { HashBase64 = string.Empty; SaltBase64 = string.Empty; } }
    public static class Padroes
    {
        public static List<DefinicaoComponente>
            Componentes()
        {
            return new[]
            {
                "Palmilha",
                "Forquilha",
                "Gáspea",
                "Sola",
                "Cabedal",
                "Tira",
                "Lingueta",
                "Fivela",
                "Acessório",
                "Lançador",
                "Enfeite",
                "Monobloco",
                "Bolsa",
                "Passador",
                "Tope",
                "Soleta",
                "Tampa",
                "Salto",
                "Elos",
                "Salomé",
                "Cabedais"
            }.Select(nome => new DefinicaoComponente
            {
                Nome = nome
            })
            .ToList();
        }
        public static List<ItemConfiguravel> TiposMatriz()
        {
            return new[]
            {
        "Gravação",
        "Tampa",
        "Lateral fixa",
        "Lateral móvel",
        "Lateral fixa direita",
        "Lateral fixa esquerda",
        "Lateral móvel direita",
        "Lateral móvel esquerda",
        "Postiço",
        "Tacelo",
        "Encaixe",
        "Forma",
        "Forma direita",
        "Forma esquerda",
        "Fundo",
        "Fundo direito",
        "Fundo esquerdo",
        "Gaveta"
    }
            .Select(nome => new ItemConfiguravel { Nome = nome })
            .ToList();
        }
    }
    public sealed class RegistroExclusao { public Guid CadastroId { get; set; } public string Codigo { get; set; } public string Usuario { get; set; } public string Computador { get; set; } public DateTimeOffset DataHora { get; set; } public string Motivo { get; set; } public DateTimeOffset RestauravelAte { get; set; } public RegistroExclusao() { Codigo = string.Empty; Usuario = string.Empty; Computador = string.Empty; Motivo = string.Empty; DataHora = DateTimeOffset.Now; RestauravelAte = DateTimeOffset.Now.AddDays(7); } }
    public sealed class RegistroBloqueio { public Guid CadastroId { get; set; } public string Codigo { get; set; } public string Usuario { get; set; } public string NomeExibido { get; set; } public string Computador { get; set; } public int ProcessoId { get; set; } public Guid TokenSessao { get; set; } public string TipoOperacao { get; set; } public DateTimeOffset CriadoEm { get; set; } public DateTimeOffset UltimaAtualizacao { get; set; } public RegistroBloqueio() { Codigo = string.Empty; Usuario = string.Empty; NomeExibido = Environment.UserName; Computador = Environment.MachineName; TokenSessao = Guid.NewGuid(); TipoOperacao = "Edição"; CriadoEm = DateTimeOffset.Now; UltimaAtualizacao = DateTimeOffset.Now; } }
}
