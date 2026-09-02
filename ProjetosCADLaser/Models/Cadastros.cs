using System;
using System.Collections.Generic;

namespace ProjetosCADLaser.Models
{
    public sealed class PilotoCadastro
    {
        public int VersaoFormato { get; set; }
        public int VersaoCadastro { get; set; }
        public Guid Id { get; set; }
        public string Codigo { get; set; }
        public string NomeModelo { get; set; }
        public string PastaOrigem { get; set; }
        public StatusPiloto Status { get; set; }
        public DateTimeOffset? CanceladoEm { get; set; }
        public string CanceladoPor { get; set; }
        public string MotivoCancelamento { get; set; }
        public List<ComponenteCadastro> Componentes { get; set; }
        public List<TexturaCadastro> Texturas { get; set; }
        public List<EventoCadastro> Historico { get; set; }
        public List<AnexoCadastro> Anexos { get; set; }
        public DateTimeOffset CriadoEm { get; set; }
        public DateTimeOffset UltimaEdicaoEm { get; set; }
        public string CriadoPor { get; set; }
        public string Computador { get; set; }
        public string UltimaEdicaoPor { get; set; }

        public PilotoCadastro()
        {
            VersaoFormato = 1; VersaoCadastro = 1; Id = Guid.NewGuid(); Status = StatusPiloto.Ativa;
            Codigo = string.Empty; NomeModelo = string.Empty; PastaOrigem = string.Empty;
            Componentes = new List<ComponenteCadastro>(); Texturas = new List<TexturaCadastro>();
            Historico = new List<EventoCadastro>(); Anexos = new List<AnexoCadastro>();
            CriadoEm = DateTimeOffset.Now; UltimaEdicaoEm = DateTimeOffset.Now;
            CriadoPor = Environment.UserName; Computador = Environment.MachineName; UltimaEdicaoPor = Environment.UserName;
        }
    }

    public sealed class ComponenteCadastro
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public List<string> PastasDetectadas { get; set; }
        public List<MatrizCadastro> Matrizes { get; set; }
        public ComponenteCadastro() { Id = Guid.NewGuid(); Nome = string.Empty; PastasDetectadas = new List<string>(); Matrizes = new List<MatrizCadastro>(); }
    }

    public sealed class MatrizCadastro
    {
        public Guid Id { get; set; }
        public string Tipo { get; set; }
        public Guid? GrupoMatrizId { get; set; }
        public string NomeExibicao { get; set; }
        public MaterialMatriz? Material { get; set; }
        public QuantidadeEixos? Eixos { get; set; }
        public Maquina? Maquina { get; set; }
        public Acabamento? Acabamento { get; set; }
        public List<Guid> TexturasIds { get; set; }
        public MatrizCadastro() { Id = Guid.NewGuid(); Tipo = string.Empty; TexturasIds = new List<Guid>(); }
    }

    public sealed class TexturaCadastro
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string CaminhoDetectado { get; set; }
        public string CaminhoRelativo { get; set; }
        public string Observacao { get; set; }
        public OrigemClassificacao Origem { get; set; }
        public bool Selecionada { get; set; }
        public bool Confirmada { get; set; }
        public TexturaCadastro() { Id = Guid.NewGuid(); Nome = string.Empty; Origem = OrigemClassificacao.Automatica; Selecionada = true; Confirmada = true; }
    }

    public sealed class EventoCadastro
    {
        public Guid Id { get; set; }
        public TipoEvento Tipo { get; set; }
        public string Observacao { get; set; }
        public Guid? ComponenteId { get; set; }
        public Guid? MatrizId { get; set; }
        public List<Guid> TexturasIds { get; set; }
        public string NumerosEscala { get; set; }
        public List<AnexoCadastro> Anexos { get; set; }
        public List<AnexoCadastro> Imagens { get; set; }
        public List<AlteracaoCampo> Alteracoes { get; set; }
        public string EstadoInicialResumo { get; set; }
        public ModoAlteracao? ModoAlteracao { get; set; }
        public AlcanceAlteracao? Alcance { get; set; }
        public bool ProjetoInteiro { get; set; }
        public List<Guid> ComponentesIds { get; set; }
        public List<Guid> MatrizesIds { get; set; }
        public DateTimeOffset DataHora { get; set; }
        public string Usuario { get; set; }
        public string Computador { get; set; }
        public DateTimeOffset UltimaEdicaoEm { get; set; }
        public string UltimaEdicaoPor { get; set; }
        public EventoCadastro() { Id = Guid.NewGuid(); Observacao = string.Empty; TexturasIds = new List<Guid>(); Anexos = new List<AnexoCadastro>(); Imagens = new List<AnexoCadastro>(); Alteracoes = new List<AlteracaoCampo>(); ComponentesIds = new List<Guid>(); MatrizesIds = new List<Guid>(); DataHora = DateTimeOffset.Now; Usuario = Environment.UserName; Computador = Environment.MachineName; UltimaEdicaoEm = DateTimeOffset.Now; UltimaEdicaoPor = Environment.UserName; }
    }

    public sealed class AlteracaoCampo
    {
        public string Campo { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorNovo { get; set; }
        public DateTimeOffset DataHora { get; set; }
        public string Usuario { get; set; }
        public string Observacao { get; set; }
        public AlteracaoCampo() { Campo = string.Empty; DataHora = DateTimeOffset.Now; Usuario = Environment.UserName; Observacao = string.Empty; }
    }

    public sealed class AnexoCadastro
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string NomeOriginal { get; set; }
        public string NomeInterno { get; set; }
        public string CaminhoRelativo { get; set; }
        public long TamanhoBytes { get; set; }
        public DateTimeOffset DataAdicao { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }
        public DateTimeOffset AdicionadoEm { get; set; }
        public string Usuario { get; set; }
        public string Computador { get; set; }
        public AnexoCadastro() { Id = Guid.NewGuid(); NomeOriginal = string.Empty; NomeInterno = string.Empty; CaminhoRelativo = string.Empty; DataAdicao = DateTimeOffset.Now; AdicionadoEm = DateTimeOffset.Now; Usuario = Environment.UserName; Computador = Environment.MachineName; }
    }

    public sealed class TesteTexturaCadastro
    {
        public int VersaoFormato { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset Data { get; set; }
        public string PastaOrigem { get; set; }
        public string Identificacao { get; set; }
        public StatusTesteTextura Status { get; set; }
        public string Observacao { get; set; }
        public string Responsavel { get; set; }
        public string Computador { get; set; }
        public AnexoCadastro Imagem { get; set; }
        public List<AnexoCadastro> Imagens { get; set; }
        public List<AnexoCadastro> Arquivos { get; set; }
        public string NomeAprovado { get; set; }
        public int VersaoCadastro { get; set; }
        public DateTimeOffset UltimaEdicaoEm { get; set; }
        public string UltimaEdicaoPor { get; set; }
        public List<EventoTesteTextura> Historico { get; set; }
        public TesteTexturaCadastro() { VersaoFormato = 1; Id = Guid.NewGuid(); Data = DateTimeOffset.Now; PastaOrigem = string.Empty; Identificacao = string.Empty; Status = StatusTesteTextura.EmTeste; Responsavel = Environment.UserName; Computador = Environment.MachineName; Imagens = new List<AnexoCadastro>(); Arquivos = new List<AnexoCadastro>(); VersaoCadastro = 1; UltimaEdicaoEm = DateTimeOffset.Now; UltimaEdicaoPor = Environment.UserName; Historico = new List<EventoTesteTextura>(); }
    }

    public sealed class EventoTesteTextura
    {
        public Guid Id { get; set; }
        public string Tipo { get; set; }
        public string Observacao { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorNovo { get; set; }
        public List<AlteracaoCampo> Alteracoes { get; set; }
        public List<AnexoCadastro> Imagens { get; set; }
        public List<AnexoCadastro> Anexos { get; set; }
        public DateTimeOffset DataHora { get; set; }
        public string Usuario { get; set; }
        public string Computador { get; set; }
        public EventoTesteTextura() { Id = Guid.NewGuid(); Tipo = string.Empty; Alteracoes = new List<AlteracaoCampo>(); Imagens = new List<AnexoCadastro>(); Anexos = new List<AnexoCadastro>(); DataHora = DateTimeOffset.Now; Usuario = Environment.UserName; Computador = Environment.MachineName; }
    }
}
