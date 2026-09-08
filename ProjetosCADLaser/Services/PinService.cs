using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class PinService
    {
        public const int IteracoesPadrao = 210000;
        private const int SaltBytes = 16;
        private const int HashBytes = 32;

        public CredencialPin Criar(string pin)
        {
            ValidarFormato(pin); var salt = new byte[SaltBytes];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(salt);
            var hash = Derivar(pin, salt, IteracoesPadrao, HashBytes);
            return new CredencialPin { SaltBase64 = Convert.ToBase64String(salt), HashBase64 = Convert.ToBase64String(hash), Iteracoes = IteracoesPadrao };
        }

        public bool Verificar(string pin, CredencialPin credencial)
        {
            try
            {
                var salt = Convert.FromBase64String(credencial.SaltBase64); var esperado = Convert.FromBase64String(credencial.HashBase64); var calculado = Derivar(pin, salt, credencial.Iteracoes, esperado.Length);
                return ComparacaoConstante(calculado, esperado);
            }
            catch (FormatException) { return false; }
            catch (ArgumentException) { return false; }
        }

        private static byte[] Derivar(string pin, byte[] salt, int iteracoes, int tamanho)
        {
            using (var derivador = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(pin), salt, iteracoes, HashAlgorithmName.SHA256)) return derivador.GetBytes(tamanho);
        }
        private static bool ComparacaoConstante(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false; var diferenca = 0;
            for (var i = 0; i < a.Length; i++) diferenca |= a[i] ^ b[i]; return diferenca == 0;
        }
        private static void ValidarFormato(string pin) { if (pin == null || pin.Length < 4 || pin.Length > 12 || pin.Any(c => !char.IsDigit(c))) throw new ArgumentException("O PIN deve conter de 4 a 12 dígitos.", "pin"); }
    }
}
