using System;
using System.IO;
using System.Linq;

namespace ProjetosCADLaser.Launcher
{
    internal static class Program
    {
        // CAMINHO DA REDE: Altere para a pasta pública onde você vai "publicar" as novas versões do app.
        private const string CaminhoOrigemRede = @"\\Servidor\Laser\AppUpdate";

        private static int Main(string[] args)
        {
            // Estrutura Base Local (no computador do usuário)
            var diretorioBase = AppDomain.CurrentDomain.BaseDirectory;
            var aplicativoLocal = Path.Combine(diretorioBase, "App", "ProjetosCADLaser.exe");
            var aplicativoAnterior = Path.Combine(diretorioBase, "App.previous", "ProjetosCADLaser.exe");

            try
            {
                // Verifica na pasta da rede se existe alguma atualização
                if (Directory.Exists(CaminhoOrigemRede))
                {
                    // Lê a versão local que está na pasta App (se não existir, assume '0.0.0.0')
                    string versaoLocal = "0.0.0.0";
                    if (File.Exists(aplicativoLocal))
                        versaoLocal = System.Diagnostics.FileVersionInfo.GetVersionInfo(aplicativoLocal).FileVersion;

                    // Verifica se tem arquivo de manifesto/txt na rede. Ex: versao.txt contendo "1.0.1.0"
                    var arquivoVersaoRede = Path.Combine(CaminhoOrigemRede, "versao.txt");
                    if (File.Exists(arquivoVersaoRede))
                    {
                        string versaoRede = File.ReadAllText(arquivoVersaoRede).Trim();

                        // Uma checagem simples de versão
                        if (Version.TryParse(versaoRede, out Version vRede) && Version.TryParse(versaoLocal, out Version vLocal))
                        {
                            if (vRede > vLocal)
                            {
                                // Inicia a atualização (copia arquivos da rede para pasta temporária local e rotaciona pastas)
                                new AtualizadorLocal().Atualizar(CaminhoOrigemRede, Path.GetDirectoryName(aplicativoLocal), Path.GetDirectoryName(aplicativoAnterior));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Se falhar (rede caiu no meio, arquivo lockado), ignoramos a atualização silenciosamente 
                // e deixamos o usuário usar a versão local antiga. Em fábrica, o show não pode parar.
                System.Diagnostics.Debug.WriteLine($"Falha no update automático: {ex.Message}");
            }

            // Independentemente de ter atualizado ou não, tenta achar o executável válido localmente.
            var executavelFinal = LauncherCore.ResolverExecutavel(aplicativoLocal, aplicativoAnterior);
            return LauncherCore.Iniciar(executavelFinal, string.Empty) ? 0 : 2;
        }
    }
}