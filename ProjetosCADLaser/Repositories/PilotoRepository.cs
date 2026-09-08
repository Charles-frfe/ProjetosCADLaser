using System;
using System.IO;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;

namespace ProjetosCADLaser.Repositories
{
    public sealed class PilotoRepository
    {
        private readonly JsonService _json;

        public PilotoRepository(JsonService json) { _json = json; }

        public string CaminhoCadastro(string raizDados, string codigo)
        {
            return Path.Combine(raizDados, "Cadastros", ValidarCodigo(codigo), "cadastro.json");
        }

        public PilotoCadastro Carregar(string raizDados, string codigo) { return _json.Read<PilotoCadastro>(CaminhoCadastro(raizDados, codigo)); }
        public PilotoCadastro ObterPorCodigo(string raizDados, string codigo) { return Carregar(raizDados, codigo); }

        public void Salvar(string raizDados, PilotoCadastro cadastro)
        {
            JsonContratosV1.ValidarPodeEditar(cadastro);
            var path = CaminhoCadastro(raizDados, cadastro.Codigo);
            _json.WriteAtomic(path, cadastro, Path.Combine(Path.GetDirectoryName(path), "backups"));
        }

        private static string ValidarCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo) || codigo.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || codigo.Contains(Path.DirectorySeparatorChar.ToString()) || codigo.Contains(Path.AltDirectorySeparatorChar.ToString()))
                throw new ArgumentException("Código inválido para armazenamento.", "codigo");
            return codigo;
        }
    }
}
