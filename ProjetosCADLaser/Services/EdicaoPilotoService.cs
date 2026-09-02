using System;
using System.Collections.Generic;
using System.IO;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class EdicaoPilotoService
    {
        private readonly JsonService _json; public EdicaoPilotoService(JsonService json) { _json = json; }
        public PilotoCadastro Salvar(string raiz, PilotoCadastro original, string nomeModelo, IReadOnlyDictionary<Guid, string> titulosAnexos, string observacao)
        {
            if (string.IsNullOrWhiteSpace(observacao)) throw new InvalidDataException("Informe uma observação para a edição."); var caminho = Path.Combine(raiz, "Cadastros", original.Codigo, "cadastro.json"); var atual = _json.Read<PilotoCadastro>(caminho); if (atual.VersaoCadastro != original.VersaoCadastro) throw new InvalidOperationException("O cadastro foi alterado por outra pessoa. Recarregue a pesquisa."); var alteracoes = new List<AlteracaoCampo>(); var nome = (nomeModelo ?? string.Empty).Trim();
            if (!string.Equals(atual.NomeModelo, nome, StringComparison.Ordinal)) { alteracoes.Add(new AlteracaoCampo { Campo = "NomeModelo", ValorAnterior = atual.NomeModelo, ValorNovo = nome, Observacao = observacao }); atual.NomeModelo = nome; }
            foreach (var anexo in atual.Anexos) { string titulo; if (titulosAnexos.TryGetValue(anexo.Id, out titulo) && !string.Equals(anexo.Titulo, titulo.Trim(), StringComparison.Ordinal)) { alteracoes.Add(new AlteracaoCampo { Campo = "Anexo:" + anexo.Id + ":Titulo", ValorAnterior = anexo.Titulo, ValorNovo = titulo.Trim(), Observacao = observacao }); anexo.Titulo = titulo.Trim(); } }
            if (alteracoes.Count == 0) throw new InvalidOperationException("Não existem alterações para salvar."); atual.Historico.Add(new EventoCadastro { Tipo = TipoEvento.EdicaoCadastro, Observacao = observacao.Trim(), Alteracoes = alteracoes }); atual.VersaoCadastro++; atual.UltimaEdicaoEm = DateTimeOffset.Now; atual.UltimaEdicaoPor = Environment.UserName; _json.WriteAtomic(caminho, atual, Path.Combine(Path.GetDirectoryName(caminho), "backups")); return atual;
        }
    }
}
