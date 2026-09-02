using System;
using System.Diagnostics;
using System.IO;

namespace ProjetosCADLaser.Launcher
{
    public static class LauncherCore
    {
        public static string ResolverExecutavel(string aplicativo, string anterior)
        {
            if (!string.IsNullOrWhiteSpace(aplicativo) && File.Exists(aplicativo)) return Path.GetFullPath(aplicativo);
            if (!string.IsNullOrWhiteSpace(anterior) && File.Exists(anterior)) return Path.GetFullPath(anterior);
            return null;
        }

        public static bool Iniciar(string executavel, string argumentos)
        {
            if (string.IsNullOrWhiteSpace(executavel) || !File.Exists(executavel)) return false;
            Process.Start(new ProcessStartInfo { FileName = executavel, Arguments = argumentos ?? string.Empty, WorkingDirectory = Path.GetDirectoryName(executavel), UseShellExecute = true });
            return true;
        }

        public static bool TentarAtualizar(string origem, string manifesto, string versaoInstalada, string aplicativo, string anterior)
        {
            var servico = new ServicoVersao(); var instalada = servico.LerVersao(versaoInstalada); if (!servico.AtualizacaoDisponivel(manifesto, instalada)) return false;
            new AtualizadorLocal().Atualizar(origem, Path.GetDirectoryName(aplicativo), Path.GetDirectoryName(anterior)); return true;
        }
    }
}
