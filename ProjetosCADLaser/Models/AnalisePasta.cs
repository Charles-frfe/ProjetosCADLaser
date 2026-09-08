using System;
using System.Collections.Generic;

namespace ProjetosCADLaser.Models
{
    public sealed class AnalisePasta { public string PastaOrigem { get; set; } public string CodigoSugerido { get; set; } public string NomeModeloSugerido { get; set; } public List<ComponenteDetectado> Componentes { get; set; } public List<string> Texturas { get; set; } public List<string> PastasPiloto { get; set; } public List<string> PastasEscala { get; set; } public List<string> NumerosEscala { get; set; } public List<string> NaoClassificadas { get; set; } public List<ItemAnalisePasta> Itens { get; set; } public AnalisePasta() { PastaOrigem = string.Empty; CodigoSugerido = string.Empty; NomeModeloSugerido = string.Empty; Componentes = new List<ComponenteDetectado>(); Texturas = new List<string>(); PastasPiloto = new List<string>(); PastasEscala = new List<string>(); NumerosEscala = new List<string>(); NaoClassificadas = new List<string>(); Itens = new List<ItemAnalisePasta>(); } }
    public sealed class ComponenteDetectado { public string Nome { get; set; } public string Caminho { get; set; } public ComponenteDetectado() { Nome = string.Empty; Caminho = string.Empty; } public ComponenteDetectado(string nome, string caminho) { Nome = nome; Caminho = caminho; } }
    public sealed class ItemAnalisePasta { public string Nome { get; set; } public string CaminhoRelativo { get; set; } public int Profundidade { get; set; } public ClassificacaoPasta Classificacao { get; set; } public string OrigemClassificacao { get; set; } public string ValorSugerido { get; set; } public ItemAnalisePasta() { Nome = string.Empty; CaminhoRelativo = string.Empty; OrigemClassificacao = string.Empty; } }
    public sealed class AnexoPendente
    {
        public Guid Id { get; set; }
        public string CaminhoTemporario { get; set; }
        public string NomeOriginal { get; set; }
        public string Titulo { get; set; }
        public bool Imagem { get; set; }
        public long TamanhoBytes { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }
        public DateTimeOffset DataAdicao { get; set; }
        public string Usuario { get; set; }
        public string Computador { get; set; }
        public AnexoPendente() { Id = Guid.NewGuid();
            CaminhoTemporario = string.Empty;
            NomeOriginal = string.Empty;
            DataAdicao = DateTimeOffset.Now;
            Usuario = Environment.UserName;
            Computador = Environment.MachineName; }
    } public sealed class ObservacaoPendente
        {
            public Guid Id { get; set; }
            public string Texto { get; set; }
            public Guid? ComponenteId { get; set; }
            public Guid? MatrizId { get; set; }
            public List<AnexoPendente> Anexos { get; set; }
            public ObservacaoPendente()
            {
                Id = Guid.NewGuid();

                Texto = String.Empty;

                Anexos = new List<AnexoPendente>();
            }
        }
    }

