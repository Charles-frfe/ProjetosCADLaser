using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;
using ProjetosCADLaser.Utilities;

namespace ProjetosCADLaser.Forms
{
    public sealed partial class PrimeiroUsoForm : Form
    {
        private readonly AppServices _servicos;
        private readonly TextBox _pasta = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
        private readonly TextBox _pin = new TextBox { UseSystemPasswordChar = true, MaxLength = 12, Dock = DockStyle.Fill };
        private readonly TextBox _confirmacao = new TextBox { UseSystemPasswordChar = true, MaxLength = 12, Dock = DockStyle.Fill };
        private readonly ComboBox _tema = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        private readonly Label _status = new Label { AutoSize = true };

        public PrimeiroUsoForm(AppServices servicos, string avisoConfiguracao = null)
        {
            InitializeComponent();
            _servicos = servicos;
            Text = "Configuração inicial — Projetos CAD/LASER";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(680, 520);
            Size = new Size(760, 580);
            Font = new Font("Segoe UI", 10F);
            _tema.Items.AddRange(Apresentacao.Temas);
            _tema.SelectedIndex = 0;
            _tema.SelectedIndexChanged += delegate { _servicos.Tema.Definir(Apresentacao.Tema(_tema.Text)); _servicos.Tema.Aplicar(this); };
            var conteudo = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(34), ColumnCount = 1, RowCount = 10 };
            conteudo.Controls.Add(new Label { Text = "Bem-vindo ao Projetos CAD/LASER", AutoSize = true, Font = new Font("Segoe UI Semibold", 20F) });
            conteudo.Controls.Add(new Label { Text = "Escolha uma pasta existente para guardar os dados compartilhados. O programa testará leitura e gravação antes de criar a estrutura.", AutoSize = true, MaximumSize = new Size(650, 0) });
            if (!string.IsNullOrWhiteSpace(avisoConfiguracao)) conteudo.Controls.Add(new Label { Text = avisoConfiguracao, AutoSize = true, ForeColor = Color.DarkRed });
            conteudo.Controls.Add(CriarCampoPasta());
            conteudo.Controls.Add(CriarCampo("PIN administrativo (4 a 12 dígitos)", _pin));
            conteudo.Controls.Add(CriarCampo("Confirme o PIN", _confirmacao));
            conteudo.Controls.Add(CriarCampo("Tema inicial", _tema));
            conteudo.Controls.Add(_status);
            conteudo.Controls.Add(CriarBotoes());
            Controls.Add(conteudo);
            _servicos.Tema.Aplicar(this);
        }

        private Control CriarCampoPasta()
        {
            var selecionar = new Button { Text = "Selecionar pasta", AutoSize = true, Height = 36 };
            selecionar.Click += delegate { using (var dialogo = new FolderBrowserDialog { Description = "Selecione a pasta raiz dos dados", ShowNewFolderButton = true }) if (dialogo.ShowDialog(this) == DialogResult.OK) { _pasta.Text = dialogo.SelectedPath; TestarPasta(); } };
            var linha = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
            linha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); linha.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            linha.Controls.Add(_pasta, 0, 0); linha.Controls.Add(selecionar, 1, 0);
            return CriarCampo("Pasta raiz", linha);
        }

        private static Control CriarCampo(string rotulo, Control campo)
        {
            var painel = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 6, 0, 6) };
            painel.Controls.Add(new Label { Text = rotulo, AutoSize = true }); painel.Controls.Add(campo); return painel;
        }

        private Control CriarBotoes()
        {
            var botoes = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
            var concluir = new Button { Text = "Concluir configuração", AutoSize = true, Height = 40 };
            var cancelar = new Button { Text = "Cancelar", AutoSize = true, Height = 40 };
            concluir.Click += delegate { Concluir(); }; cancelar.Click += delegate { Close(); }; botoes.Controls.Add(concluir); botoes.Controls.Add(cancelar); return botoes;
        }

        private void TestarPasta()
        {
            var resultado = _servicos.AcessoPasta.Testar(_pasta.Text); _status.Text = resultado.Mensagem; _status.ForeColor = resultado.Sucesso ? Color.SeaGreen : Color.Firebrick;
        }

        private void Concluir()
        {
            var acesso = _servicos.AcessoPasta.Testar(_pasta.Text);
            if (!acesso.Sucesso) { MostrarErro(acesso.Mensagem); return; }
            if (_pin.Text != _confirmacao.Text) { MostrarErro("A confirmação do PIN é diferente."); return; }
            if (MessageBox.Show(this, "A estrutura de dados será criada dentro da pasta selecionada. Deseja continuar?", "Confirmar configuração", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { _servicos.Inicializacao.ConcluirPrimeiroUso(_pasta.Text, _pin.Text, _confirmacao.Text, Apresentacao.Tema(_tema.Text)); DialogResult = DialogResult.OK; Close(); }
            catch (Exception ex) when (
            ex is ArgumentException || 
            ex is IOException || 
            ex is UnauthorizedAccessException ||
            ex is InvalidOperationException) { _servicos.Log.Registrar(ex, "Falha no primeiro uso", _pasta.Text); MostrarErro(ex.Message); }
        }

        private void MostrarErro(string mensagem) { MessageBox.Show(this, mensagem, "Não foi possível concluir", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
}
