namespace ProjetosCADLaser.Models
{
    public enum StatusPiloto { Ativa, Cancelada }
    public enum StatusTesteTextura { EmTeste, Aprovada, Reprovada }
    public enum TipoEvento { CriacaoPiloto, ReformaPiloto, RetoquePiloto, ReformaEscala, RetoqueEscala, EdicaoCadastro, ObservacaoGeral, CadastroContinuado, ComponenteAdicionado, MatrizAdicionada, TexturaAdicionada, AnexoAdicionado, AlteracaoEscala, Reativacao, Restauracao, Cancelamento }
    public enum PerfilUsuario { Operacional, Consulta }
    public enum ModoAlteracao { SomenteRegistrar, RegistrarEAtualizar }
    public enum AlcanceAlteracao { ProjetoInteiro, ComponentesEMatrizes }
    public enum MaterialMatriz { Zamak, Aco, Aluminio }
    public enum QuantidadeEixos { Tres = 3, Cinco = 5 }
    public enum Maquina { M1000, M1200P, M1200S }
    public enum Acabamento { Fosco, Polido }
    public enum PreferenciaTema { SeguirWindows, Claro, Escuro }
    public enum ClassificacaoPasta { Componente, Textura, Piloto, Escala, NumeroEscala, NaoClassificado, Ignorado }
    public enum OrigemClassificacao { Automatica, Manual }
}
