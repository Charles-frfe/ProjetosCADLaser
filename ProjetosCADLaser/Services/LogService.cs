using System;
using System.IO;

namespace ProjetosCADLaser.Services
{
    public sealed class LogService
    {
        private readonly string _pastaLocal = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProjetosCADLaser", "Logs");
        public void Registrar(Exception excecao, string contexto, string pastaRaiz = null)
        {
            try
            {
                var pasta = !string.IsNullOrWhiteSpace(pastaRaiz) && Directory.Exists(pastaRaiz) ? Path.Combine(pastaRaiz, "Logs") : _pastaLocal;
                Directory.CreateDirectory(pasta); var linha = string.Format("[{0:O}] {1}{2}{3}{2}{4}{2}", DateTimeOffset.Now, contexto, Environment.NewLine, excecao, new string('-', 72));
                File.AppendAllText(Path.Combine(pasta, "aplicacao_" + DateTime.Now.ToString("yyyyMM") + ".log"), linha);
            }
            catch { }
        }
    }
}
