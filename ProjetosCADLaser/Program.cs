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

            // TEMPORÁRIO:
            // Durante o desenvolvimento, não abrir a tela de primeiro uso.
            if (servicos.Inicializacao.PrimeiroUsoNecessario(
    out configuracao,
    out erro))
{
    var pastaDesenvolvimento = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ProjetosCADLaser_DEV");

    Directory.CreateDirectory(pastaDesenvolvimento);
    EstruturaDados.Criar(pastaDesenvolvimento);

    configuracao = new ConfiguracaoLocal
    {
        PastaRaizDados = pastaDesenvolvimento,
        NomeExibido = Environment.UserName,
        UsuarioAtivo = true,
        Perfil = PerfilUsuario.Operacional
    };
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