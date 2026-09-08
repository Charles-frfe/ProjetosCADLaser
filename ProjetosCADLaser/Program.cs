using System;
using System.Windows.Forms;
using ProjetosCADLaser.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;
using System.IO;
using ProjetosCADLaser.Configuration;

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

            // Removidas as diretivas #if DEBUG.
            // O sistema sempre chamará a verificação base, e futuramente abrirá o PrimeiroUsoForm em produção
            if (servicos.Inicializacao.PrimeiroUsoNecessario(out configuracao, out erro))
            {
                // Como workaround para testarmos produção sem UI de setup inicial pronta, instanciamos a base vazia:
                var pastaProducao = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ProjetosCADLaser_PROD");

                Directory.CreateDirectory(pastaProducao);
                EstruturaDados.Criar(pastaProducao);

                configuracao = new ConfiguracaoLocal
                {
                    PastaRaizDados = pastaProducao,
                    NomeExibido = Environment.UserName,
                    UsuarioAtivo = true,
                    Perfil = PerfilUsuario.Operacional
                };

                servicos.ConfiguracaoLocal.Salvar(configuracao);
            }

            if (!configuracao.UsuarioAtivo)
            {
                MessageBox.Show(
                    "Este usuário está inativo. Procure o administrador.",
                    "Acesso bloqueado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            servicos.Tema.Definir(configuracao.Tema);

            Application.Run(new TelaInicialForm(servicos, configuracao));
        }
    }
}