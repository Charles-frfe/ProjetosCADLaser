using System;
using System.IO;

namespace ProjetosCADLaser.Services
{
    public sealed class ResultadoAcessoPasta
    {
        public bool Sucesso { get; private set; }
        public string Mensagem { get; private set; }
        private ResultadoAcessoPasta(bool sucesso, string mensagem) { Sucesso = sucesso; Mensagem = mensagem; }
        public static ResultadoAcessoPasta Ok() { return new ResultadoAcessoPasta(true, "A pasta está disponível para leitura e gravação."); }
        public static ResultadoAcessoPasta Falha(string mensagem) { return new ResultadoAcessoPasta(false, mensagem); }
    }
    public sealed class AcessoPastaService
    {
        public ResultadoAcessoPasta Testar(string pasta)
        {
            if (string.IsNullOrWhiteSpace(pasta)) return ResultadoAcessoPasta.Falha("Selecione uma pasta raiz.");
            if (!Directory.Exists(pasta)) return ResultadoAcessoPasta.Falha("A pasta selecionada não existe ou está indisponível.");
            var original = Path.Combine(pasta, ".cadlaser_teste_" + Guid.NewGuid().ToString("N") + ".tmp");
            var renomeado = original + ".ok";
            try
            {
                const string conteudo = "teste de acesso";
                File.WriteAllText(original, conteudo);
                if (File.ReadAllText(original) != conteudo) return ResultadoAcessoPasta.Falha("Não foi possível confirmar a leitura na pasta.");
                File.Move(original, renomeado); File.Delete(renomeado); return ResultadoAcessoPasta.Ok();
            }
            catch (UnauthorizedAccessException) { return ResultadoAcessoPasta.Falha("Sem permissão para gravar na pasta selecionada."); }
            catch (IOException) { return ResultadoAcessoPasta.Falha("Falha de leitura ou gravação. Verifique a rede e tente novamente."); }
            finally { TentarExcluir(original); TentarExcluir(renomeado); }
        }
        private static void TentarExcluir(string caminho) { try { if (File.Exists(caminho)) File.Delete(caminho); } catch { } }
    }
}
