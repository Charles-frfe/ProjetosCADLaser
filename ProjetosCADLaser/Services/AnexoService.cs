using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class AnexoService
    {
        private readonly string _baseTemporaria;
        private static readonly Regex RegexNomeSeguro = new Regex(@"[^\p{L}\p{Nd}.-]+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex RegexEspacos = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        public AnexoService(string baseTemporaria = null) { _baseTemporaria = baseTemporaria ?? Path.Combine(Path.GetTempPath(), "ProjetosCADLaser", "CadastrosPendentes"); }
        public string CriarSessao() { var pasta = Path.Combine(_baseTemporaria, Guid.NewGuid().ToString("N")); Directory.CreateDirectory(pasta); return pasta; }
        public AnexoPendente AdicionarArquivo(string sessao, string arquivoOrigem, bool imagem)
        {
            if (!File.Exists(arquivoOrigem)) throw new FileNotFoundException("O arquivo selecionado não foi encontrado.", arquivoOrigem); Directory.CreateDirectory(sessao); var temporario = Path.Combine(sessao, Guid.NewGuid().ToString("N") + Path.GetExtension(arquivoOrigem));
            try { File.Copy(arquivoOrigem, temporario, false); return new AnexoPendente { CaminhoTemporario = temporario, NomeOriginal = Path.GetFileName(arquivoOrigem), Titulo = Path.GetFileName(arquivoOrigem), Imagem = imagem, TamanhoBytes = new FileInfo(temporario).Length }; }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { throw new IOException("Não foi possível preparar o anexo selecionado.", ex); }
        }
        public AnexoPendente AdicionarImagem(string sessao, Image imagem)
        {
            Directory.CreateDirectory(sessao); var nome = "imagem_colada_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png"; var temporario = Path.Combine(sessao, Guid.NewGuid().ToString("N") + ".png");
            try { imagem.Save(temporario, ImageFormat.Png); return new AnexoPendente { CaminhoTemporario = temporario, NomeOriginal = nome, Imagem = true, Titulo = nome, TamanhoBytes = new FileInfo(temporario).Length }; }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ExternalException) { throw new IOException("Não foi possível preparar a imagem da área de transferência.", ex); }
        }
        public AnexoCadastro CopiarParaCadastro(AnexoPendente pendente, string pastaCadastro, Guid eventoId)
        {
            if (!File.Exists(pendente.CaminhoTemporario)) throw new FileNotFoundException("O anexo temporário não está mais disponível.", pendente.CaminhoTemporario);
            var pastaEvento = Path.Combine(pastaCadastro, pendente.Imagem ? "imagens" : "arquivos", eventoId.ToString("D")); Directory.CreateDirectory(pastaEvento); var nomeInterno = CriarNomeInterno(pendente.Titulo, pendente.NomeOriginal); var destino = Path.Combine(pastaEvento, nomeInterno); File.Copy(pendente.CaminhoTemporario, destino, false);
            return new AnexoCadastro { Id = pendente.Id, Titulo = string.IsNullOrWhiteSpace(pendente.Titulo) ? pendente.NomeOriginal : pendente.Titulo, NomeOriginal = pendente.NomeOriginal, NomeInterno = nomeInterno, CaminhoRelativo = CaminhoRelativo(pastaCadastro, destino), TamanhoBytes = new FileInfo(destino).Length, DataAdicao = pendente.DataAdicao, Descricao = pendente.Descricao, Categoria = pendente.Categoria, Usuario = pendente.Usuario, Computador = pendente.Computador, AdicionadoEm = pendente.DataAdicao };
        }
        public static string CriarNomeInterno(string titulo, string nomeOriginal)
        {
            var baseNome = string.IsNullOrWhiteSpace(titulo) ? Path.GetFileNameWithoutExtension(nomeOriginal) : titulo.Trim(); baseNome = RegexNomeSeguro.Replace(baseNome, " "); baseNome = RegexEspacos.Replace(baseNome, " ").Trim(); if (baseNome.Length == 0) baseNome = "anexo"; if (baseNome.Length > 32) baseNome = baseNome.Substring(0, 32).Trim(); return baseNome + "_" + Guid.NewGuid().ToString("N") + Path.GetExtension(nomeOriginal);
        }
        public void LimparSessao(string sessao)
        {
            if (string.IsNullOrWhiteSpace(sessao) || !Directory.Exists(sessao)) return; var completa = Path.GetFullPath(sessao); var baseCompleta = BaseComSeparador(); if (!completa.StartsWith(baseCompleta, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("A pasta não pertence à área temporária de anexos."); Directory.Delete(completa, true);
        }
        public int LimparTemporariosAntigos(TimeSpan idadeMinima)
        {
            if (!Directory.Exists(_baseTemporaria)) return 0; var removidos = 0; var limite = DateTime.UtcNow - idadeMinima;
            foreach (var pasta in Directory.GetDirectories(_baseTemporaria)) try { var completa = Path.GetFullPath(pasta); if (!completa.StartsWith(BaseComSeparador(), StringComparison.OrdinalIgnoreCase) || Directory.GetLastWriteTimeUtc(completa) >= limite) continue; Directory.Delete(completa, true); removidos++; } catch { }
            return removidos;
        }
        private string BaseComSeparador() { return Path.GetFullPath(_baseTemporaria).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar; }
        private static string CaminhoRelativo(string raiz, string destino) { var basePath = Path.GetFullPath(raiz).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar; var full = Path.GetFullPath(destino); return full.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) ? full.Substring(basePath.Length) : full; }
    }
}
