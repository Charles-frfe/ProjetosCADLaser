using System;
using System.IO;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class CancelamentoPilotoService
    {
        private readonly JsonService _json; public CancelamentoPilotoService(JsonService json) { _json = json; }
        public PilotoCadastro Cancelar(string raiz, string codigo, string motivo) { return Alterar(raiz, codigo, motivo, false); }
        public PilotoCadastro Reativar(string raiz, string codigo, string motivo) { return Alterar(raiz, codigo, motivo, true); }
        private PilotoCadastro Alterar(string raiz, string codigo, string motivo, bool reativar)
        {
            var path = Path.Combine(raiz, "Cadastros", codigo, "cadastro.json"); var piloto = _json.Read<PilotoCadastro>(path); if (string.IsNullOrWhiteSpace(motivo)) throw new InvalidDataException(reativar ? "Informe o motivo da reativação." : "Informe o motivo do cancelamento.");
            piloto.Status = reativar ? StatusPiloto.Ativa : StatusPiloto.Cancelada; piloto.CanceladoEm = reativar ? (DateTimeOffset?)null : DateTimeOffset.Now; piloto.CanceladoPor = reativar ? null : Environment.UserName; piloto.MotivoCancelamento = reativar ? null : motivo.Trim(); piloto.VersaoCadastro++; piloto.UltimaEdicaoEm = DateTimeOffset.Now; piloto.UltimaEdicaoPor = Environment.UserName; piloto.Historico.Add(new EventoCadastro { Tipo = reativar ? TipoEvento.Reativacao : TipoEvento.Cancelamento, Observacao = motivo.Trim() }); _json.WriteAtomic(path, piloto, Path.Combine(Path.GetDirectoryName(path), "backups")); return piloto;
        }
    }
}
