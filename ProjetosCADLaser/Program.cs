using System;
using System.Windows.Forms;
using ProjetosCADLaser.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;

namespace ProjetosCADLaser
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var servicos = new AppServices();
            ConfiguracaoLocal configuracao;
            string erro;
            if (servicos.Inicializacao.PrimeiroUsoNecessario(out configuracao, out erro))
            {
                using (var primeiroUso = new PrimeiroUsoForm(servicos, erro))
                {
                    if (primeiroUso.ShowDialog() != DialogResult.OK) return;
                }
                configuracao = servicos.ConfiguracaoLocal.Carregar();
            }
            if (!configuracao.UsuarioAtivo)
            {
                MessageBox.Show("Este usuário está inativo. Procure o administrador.", "Acesso bloqueado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            servicos.Tema.Definir(configuracao.Tema);
            Application.Run(new TelaInicialForm(servicos, configuracao));
        }
    }
}
