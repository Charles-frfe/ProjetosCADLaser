using System;
using System.IO;

namespace ProjetosCADLaser.Services
{
    public sealed class BackupService
    {
        public string Criar(string arquivoOrigem, string pastaBackups)
        {
            if (!File.Exists(arquivoOrigem)) throw new FileNotFoundException("O arquivo original do backup não existe.", arquivoOrigem);
            Directory.CreateDirectory(pastaBackups);
            var nome = string.Format("{0}_{1:yyyyMMdd_HHmmss_fff}.json", Path.GetFileNameWithoutExtension(arquivoOrigem), DateTime.Now);
            var destino = Path.Combine(pastaBackups, nome);
            File.Copy(arquivoOrigem, destino, false);
            return destino;
        }
    }
}
