using System.IO;

namespace ProjetosCADLaser.Configuration
{
    public static class EstruturaDados
    {
        public const int VersaoAtual = 1;
        public static void Criar(string raiz) { foreach (var pasta in new[] { "Cadastros", "BloqueiosCodigos", "BloqueiosTestes", "TestesTextura", "Lixeira", "Configuracoes", "Logs", "BackupsGerais" }) Directory.CreateDirectory(Path.Combine(raiz, pasta)); }
    }
}
