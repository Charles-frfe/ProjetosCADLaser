using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class BloqueioService
    {
        private readonly JsonService _json; public BloqueioService(JsonService json) { _json = json; }
        public string ObterCaminho(string raizDados, string codigo) { var normalizado = NormalizadorPesquisa.Normalizar(codigo).Replace(' ', '_'); if (string.IsNullOrWhiteSpace(normalizado) || normalizado.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new ArgumentException("Informe um código válido."); return Path.Combine(raizDados, "BloqueiosCodigos", normalizado + ".lock.json"); }
        public string ObterCaminhoTeste(string raizDados, Guid testeId) { return Path.Combine(raizDados, "BloqueiosTestes", testeId.ToString("N") + ".lock.json"); }
        public RegistroBloqueio CriarParaTeste(string raizDados, Guid testeId, string tipoOperacao, string nomeExibido = null) { return Criar(ObterCaminhoTeste(raizDados, testeId), testeId, testeId.ToString("D"), tipoOperacao, nomeExibido); }
        public static string MensagemTesteOcupado(RegistroBloqueio registro) { return "Este teste está sendo editado por " + registro.NomeExibido + " no computador " + registro.Computador + "."; }
        public RegistroBloqueio CriarPorCodigo(string raizDados, string codigo, Guid cadastroId, string tipoOperacao, string nomeExibido = null) { return Criar(ObterCaminho(raizDados, codigo), cadastroId, codigo, tipoOperacao, nomeExibido); }
        public RegistroBloqueio Criar(string caminhoBloqueio, Guid cadastroId) { return Criar(caminhoBloqueio, cadastroId, string.Empty, "Edição", null); }
        public RegistroBloqueio Criar(string caminhoBloqueio, Guid cadastroId, string codigo, string tipoOperacao, string nomeExibido = null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(caminhoBloqueio))); if (File.Exists(caminhoBloqueio)) throw new IOException(MensagemOcupado(Ler(caminhoBloqueio))); var registro = new RegistroBloqueio { CadastroId = cadastroId, Codigo = (codigo ?? string.Empty).Trim(), Usuario = Environment.UserName, NomeExibido = string.IsNullOrWhiteSpace(nomeExibido) ? Environment.UserName : nomeExibido.Trim(), Computador = Environment.MachineName, ProcessoId = Process.GetCurrentProcess().Id, TipoOperacao = tipoOperacao };
            try { using (var stream = new FileStream(caminhoBloqueio, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { System.Text.Json.JsonSerializer.Serialize(stream, registro, _json.Options); stream.Flush(true); } return registro; } catch (IOException) when (File.Exists(caminhoBloqueio)) { throw new IOException(MensagemOcupado(Ler(caminhoBloqueio))); }
        }
        public RegistroBloqueio Ler(string caminhoBloqueio) { return _json.Read<RegistroBloqueio>(caminhoBloqueio); }
        public void Atualizar(string caminhoBloqueio, RegistroBloqueio registro) { registro.UltimaAtualizacao = DateTimeOffset.Now; _json.WriteAtomicWithoutBackup(caminhoBloqueio, registro); }
        public bool EstaAbandonado(RegistroBloqueio registro, TimeSpan tolerancia) { return DateTimeOffset.Now - registro.UltimaAtualizacao > tolerancia; }
        public IReadOnlyList<(string Caminho, RegistroBloqueio Registro)> Listar(string raizDados) { var resultado = new List<(string, RegistroBloqueio)>(); foreach (var nomePasta in new[] { "BloqueiosCodigos", "BloqueiosTestes" }) { var pasta = Path.Combine(raizDados, nomePasta); if (!Directory.Exists(pasta)) continue; foreach (var caminho in Directory.GetFiles(pasta, "*.lock.json", SearchOption.TopDirectoryOnly)) try { resultado.Add((caminho, Ler(caminho))); } catch { } } return resultado; }
        public void RemoverAbandonado(string caminhoBloqueio, TimeSpan tolerancia) { var registro = Ler(caminhoBloqueio); if (!EstaAbandonado(registro, tolerancia)) throw new InvalidOperationException("O bloqueio ainda está ativo e não pode ser liberado."); File.Delete(caminhoBloqueio); }
        public void Remover(string caminhoBloqueio, RegistroBloqueio dono) { if (!File.Exists(caminhoBloqueio)) return; var atual = Ler(caminhoBloqueio); if (atual.TokenSessao != dono.TokenSessao || atual.ProcessoId != dono.ProcessoId || !string.Equals(atual.Computador, dono.Computador, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedAccessException("O bloqueio pertence a outro processo."); File.Delete(caminhoBloqueio); }
        public static string MensagemOcupado(RegistroBloqueio registro) { return "O código " + registro.Codigo + " está sendo trabalhado por " + registro.NomeExibido + " no computador " + registro.Computador + ". Operação atual: " + registro.TipoOperacao + ". Iniciado em: " + registro.CriadoEm.ToString("g") + "."; }
    }
}
