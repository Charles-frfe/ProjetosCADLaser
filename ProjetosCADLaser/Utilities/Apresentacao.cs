using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Utilities
{
    public static class Apresentacao
    {
        public static string[] Temas { get { return new[] { "Seguir o Windows", "Claro", "Escuro" }; } }
        public static string Tema(PreferenciaTema tema) { return tema == PreferenciaTema.Claro ? "Claro" : tema == PreferenciaTema.Escuro ? "Escuro" : "Seguir o Windows"; }
        public static PreferenciaTema Tema(string texto) { return texto == "Claro" ? PreferenciaTema.Claro : texto == "Escuro" ? PreferenciaTema.Escuro : PreferenciaTema.SeguirWindows; }
    }
}
