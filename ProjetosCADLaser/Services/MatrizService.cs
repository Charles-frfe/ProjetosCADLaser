using System;
using System.Collections.Generic;
using System.Linq;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class MatrizService
    {
        public static readonly string[] NomesLaterais = { "Lateral Fixa Direita", "Lateral Fixa Esquerda", "Lateral Móvel Direita", "Lateral Móvel Esquerda" };
        public IReadOnlyList<MatrizCadastro> CriarGrupoLaterais(MatrizCadastro configuracaoBase)
        {
            var grupo = Guid.NewGuid(); return NomesLaterais.Select(nome => new MatrizCadastro { Tipo = "Laterais", GrupoMatrizId = grupo, NomeExibicao = nome, Material = configuracaoBase.Material, Eixos = configuracaoBase.Eixos, Maquina = configuracaoBase.Maquina, Acabamento = configuracaoBase.Acabamento, TexturasIds = new List<Guid>(configuracaoBase.TexturasIds) }).ToList();
        }
        public bool PossuiGrupoLaterais(IEnumerable<MatrizCadastro> matrizes) { return matrizes.Any(x => x.GrupoMatrizId.HasValue && x.Tipo.Equals("Laterais", StringComparison.OrdinalIgnoreCase)); }
        public void AplicarConfiguracaoAoGrupo(MatrizCadastro origem, IEnumerable<MatrizCadastro> grupo, bool material, bool eixos, bool maquina, bool acabamento) { AplicarConfiguracao(origem, grupo, material, eixos, maquina, acabamento); }
        public IReadOnlyList<string> Validar(MatrizCadastro matriz, IEnumerable<string> tiposAtivos)
        {
            var erros = new List<string>(); if (string.IsNullOrWhiteSpace(matriz.Tipo)) erros.Add("Selecione o tipo da matriz."); else if (!tiposAtivos.Contains(matriz.Tipo, StringComparer.OrdinalIgnoreCase)) erros.Add("O tipo selecionado não está ativo.");
            if (!matriz.Material.HasValue || !Enum.IsDefined(typeof(MaterialMatriz), matriz.Material.Value)) erros.Add("Selecione o material.");
            if (!matriz.Eixos.HasValue || !Enum.IsDefined(typeof(QuantidadeEixos), matriz.Eixos.Value)) erros.Add("Selecione a quantidade de eixos.");
            if (!matriz.Maquina.HasValue || !Enum.IsDefined(typeof(Maquina), matriz.Maquina.Value)) erros.Add("Selecione a máquina.");
            if (!matriz.Acabamento.HasValue || !Enum.IsDefined(typeof(Acabamento), matriz.Acabamento.Value)) erros.Add("Selecione o acabamento."); return erros;
        }
        public MatrizCadastro Duplicar(MatrizCadastro origem) { return new MatrizCadastro { Id = Guid.NewGuid(), Tipo = origem.Tipo, Material = origem.Material, Eixos = origem.Eixos, Maquina = origem.Maquina, Acabamento = origem.Acabamento, TexturasIds = new List<Guid>(origem.TexturasIds) }; }
        public void AplicarConfiguracao(MatrizCadastro origem, IEnumerable<MatrizCadastro> destinos, bool material, bool eixos, bool maquina, bool acabamento) { foreach (var destino in destinos.Where(x => x.Id != origem.Id)) { if (material) destino.Material = origem.Material; if (eixos) destino.Eixos = origem.Eixos; if (maquina) destino.Maquina = origem.Maquina; if (acabamento) destino.Acabamento = origem.Acabamento; } }
    }
}
