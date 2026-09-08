using System;
using System.IO;

namespace ProjetosCADLaser.Launcher
{
    public sealed class ServicoVersao
    {
        public Version LerVersao(string caminho)
        {
            if (string.IsNullOrWhiteSpace(caminho) || !File.Exists(caminho)) return null;
            Version versao; return Version.TryParse(File.ReadAllText(caminho).Trim(), out versao) ? versao : null;
        }

        public bool AtualizacaoDisponivel(string manifesto, Version instalada)
        {
            var remota = LerVersao(manifesto); return remota != null && instalada != null && remota > instalada;
        }
    }
}
