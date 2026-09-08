using System;
using System.IO;
using System.Text.Json;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class JsonService
    {
        private readonly BackupService _backups;

        public JsonSerializerOptions Options { get; private set; }

        public JsonService(BackupService backups = null)
        {
            _backups = backups ?? new BackupService();
            Options = JsonContratosV1.CriarOpcoes();
            Options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        }

        public T Read<T>(string path)
        {
            int maxRetries = 5;
            int delayMs = 100;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // FileShare.ReadWrite permite leitura mesmo se outro processo estiver escrevendo (sem dar Lock)
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var value = JsonSerializer.Deserialize<T>(stream, Options);
                        if (value == null) throw new InvalidDataException("JSON vazio ou inválido: " + path);
                        return value;
                    }
                }
                catch (IOException) when (i < maxRetries - 1)
                {
                    // Tratamento de bloqueio de rede temporário (file lock do SMB do Windows)
                    System.Threading.Thread.Sleep(delayMs);
                }
            }

            throw new IOException($"Não foi possível ler o arquivo '{path}' após várias tentativas. Ele pode estar bloqueado pela rede.");
        }
        public void WriteAtomic<T>(string path, T value, string backupDirectory = null)
        {
            WriteAtomicCore(path, value, backupDirectory, true);
        }

        public void WriteAtomicWithoutBackup<T>(string path, T value)
        {
            WriteAtomicCore(path, value, null, false);
        }

        private void WriteAtomicCore<T>(string path, T value, string backupDirectory, bool criarBackup)
        {
            var fullPath = Path.GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath);
            if (directory == null) throw new ArgumentException("O arquivo precisa ter uma pasta.", "path");
            Directory.CreateDirectory(directory);
            var temporary = fullPath + ".tmp." + Guid.NewGuid().ToString("N");
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                {
                    JsonSerializer.Serialize(stream, value, Options);
                    stream.Flush(true);
                }
                Read<T>(temporary);
                if (File.Exists(fullPath))
                {
                    if (criarBackup)
                        _backups.Criar(fullPath, backupDirectory ?? Path.Combine(directory, "backups"));
                    File.Replace(temporary, fullPath, null, true);
                }
                else File.Move(temporary, fullPath);
                Read<T>(fullPath);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }
    }
}
