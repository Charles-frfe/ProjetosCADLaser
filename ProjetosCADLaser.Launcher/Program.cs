using System;
using System.IO;
using System.Linq;

namespace ProjetosCADLaser.Launcher
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var aplicativo = Valor(args, "--app") ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App", "ProjetosCADLaser.exe");
            var anterior = Valor(args, "--previous") ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App.previous", "ProjetosCADLaser.exe");
            var origem = Valor(args, "--source"); var manifesto = Valor(args, "--manifest"); var instalada = Valor(args, "--installed-version");
            if (!string.IsNullOrWhiteSpace(origem) && !string.IsNullOrWhiteSpace(manifesto) && !string.IsNullOrWhiteSpace(instalada)) try { LauncherCore.TentarAtualizar(origem, manifesto, instalada, aplicativo, anterior); } catch (IOException) { }
            var executavel = LauncherCore.ResolverExecutavel(aplicativo, anterior);
            return LauncherCore.Iniciar(executavel, string.Empty) ? 0 : 2;
        }
        private static string Valor(string[] args, string chave) { var prefixo = chave + "="; var item = (args ?? new string[0]).FirstOrDefault(x => x.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase)); return item == null ? null : item.Substring(prefixo.Length).Trim('"'); }
    }
}
