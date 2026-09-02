using System;
using System.Collections.Generic;
using System.IO;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class ItemLixeira { public RegistroLixeira Meta { get; private set; } public string Pasta { get; private set; } public ItemLixeira(RegistroLixeira meta, string pasta) { Meta = meta; Pasta = pasta; } }
    public sealed class LixeiraService
    {
        private readonly JsonService _json; private readonly LogService _log;
        public LixeiraService(JsonService json, LogService log) { _json = json; _log = log; }
        public string Pasta(string raiz, TipoRegistroLixeira tipo) { return Path.Combine(raiz, "Lixeira", tipo == TipoRegistroLixeira.Piloto ? "Pilotos" : "TestesTextura"); }
        public RegistroLixeira Mover(string raiz, TipoRegistroLixeira tipo, string origem, Guid id, string codigo, string nome, string motivo)
        {
            if (!Directory.Exists(origem)) throw new DirectoryNotFoundException(origem); var meta = new RegistroLixeira { Tipo = tipo, Id = id, Codigo = codigo, Nome = nome, CaminhoOriginal = origem, Motivo = motivo }; var destino = Path.Combine(Pasta(raiz, tipo), (codigo ?? id.ToString()) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + id.ToString("N")); Directory.CreateDirectory(Path.GetDirectoryName(destino));
            try { Directory.Move(origem, destino); _json.WriteAtomic(Path.Combine(destino, "lixeira.json"), meta); return meta; } catch (Exception ex) { _log.Registrar(ex, "Falha ao mover registro para a lixeira", raiz); if (Directory.Exists(destino) && !Directory.Exists(origem)) Directory.Move(destino, origem); throw; }
        }
        public IReadOnlyList<ItemLixeira> Listar(string raiz, TipoRegistroLixeira tipo)
        {
            var pasta = Pasta(raiz, tipo); var resultado = new List<ItemLixeira>(); if (!Directory.Exists(pasta)) return resultado;
            foreach (var arquivo in Directory.GetFiles(pasta, "lixeira.json", SearchOption.AllDirectories)) try { resultado.Add(new ItemLixeira(_json.Read<RegistroLixeira>(arquivo), Path.GetDirectoryName(arquivo))); } catch { }
            return resultado;
        }
        public void Restaurar(string raiz, RegistroLixeira meta, string pastaLixeira)
        {
            if (Directory.Exists(meta.CaminhoOriginal)) throw new IOException("O cadastro original já existe."); Directory.CreateDirectory(Path.GetDirectoryName(meta.CaminhoOriginal)); Directory.Move(pastaLixeira, meta.CaminhoOriginal); var jsonPath = Path.Combine(meta.CaminhoOriginal, "lixeira.json"); if (File.Exists(jsonPath)) File.Delete(jsonPath);
            try
            {
                if (meta.Tipo == TipoRegistroLixeira.Piloto) { var caminho = Path.Combine(meta.CaminhoOriginal, "cadastro.json"); var piloto = _json.Read<PilotoCadastro>(caminho); piloto.Historico.Add(new EventoCadastro { Tipo = TipoEvento.Restauracao, Observacao = "Projeto restaurado da lixeira." }); piloto.VersaoCadastro++; piloto.UltimaEdicaoEm = DateTimeOffset.Now; piloto.UltimaEdicaoPor = Environment.UserName; _json.WriteAtomic(caminho, piloto, Path.Combine(meta.CaminhoOriginal, "backups")); }
                else { var caminho = Path.Combine(meta.CaminhoOriginal, "teste.json"); var teste = _json.Read<TesteTexturaCadastro>(caminho); teste.Historico.Add(new EventoTesteTextura { Tipo = "Teste restaurado", Observacao = "Teste restaurado da lixeira." }); teste.VersaoCadastro++; teste.UltimaEdicaoEm = DateTimeOffset.Now; teste.UltimaEdicaoPor = Environment.UserName; _json.WriteAtomic(caminho, teste, Path.Combine(meta.CaminhoOriginal, "backups")); }
            }
            catch (Exception ex) { _log.Registrar(ex, "Registro restaurado, mas o histórico não pôde ser atualizado", raiz); }
        }
        public int LimparVencidos(string raiz, TipoRegistroLixeira tipo, DateTimeOffset agora) { var total = 0; foreach (var item in Listar(raiz, tipo)) if (item.Meta.DisponivelAte <= agora) { Directory.Delete(item.Pasta, true); total++; } return total; }
    }
}
