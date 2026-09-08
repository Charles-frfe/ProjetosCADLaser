using System;
using System.IO;

namespace ProjetosCADLaser.Launcher
{
    public sealed class AtualizadorLocal
    {
        private static readonly string[] PastasProtegidas = { "Dados", "Cadastros", "TestesTextura", "Backups", "Lixeira" };

        public void Atualizar(string origem, string aplicativo, string anterior)
        {
            if (string.IsNullOrWhiteSpace(origem) || !Directory.Exists(origem)) throw new DirectoryNotFoundException("Origem da atualização não encontrada.");
            var app = Path.GetFullPath(aplicativo); var prev = Path.GetFullPath(anterior); var temporaria = app + ".next";
            if (Directory.Exists(temporaria)) Directory.Delete(temporaria, true);
            CopiarAplicacao(origem, temporaria);
            var movido = false;
            try
            {
                if (Directory.Exists(prev)) Directory.Delete(prev, true);
                if (Directory.Exists(app)) { Directory.Move(app, prev); movido = true; }
                Directory.Move(temporaria, app);
            }
            catch
            {
                if (Directory.Exists(temporaria)) Directory.Delete(temporaria, true);
                if (movido && !Directory.Exists(app) && Directory.Exists(prev)) Directory.Move(prev, app);
                throw;
            }
        }

        private static void CopiarAplicacao(string origem, string destino)
        {
            Directory.CreateDirectory(destino);
            foreach (var arquivo in Directory.GetFiles(origem, "*", SearchOption.TopDirectoryOnly)) File.Copy(arquivo, Path.Combine(destino, Path.GetFileName(arquivo)), true);
            foreach (var pasta in Directory.GetDirectories(origem, "*", SearchOption.TopDirectoryOnly))
            {
                if (Array.Exists(PastasProtegidas, x => string.Equals(x, Path.GetFileName(pasta), StringComparison.OrdinalIgnoreCase))) continue;
                CopiarAplicacao(pasta, Path.Combine(destino, Path.GetFileName(pasta)));
            }
        }
    }
}
