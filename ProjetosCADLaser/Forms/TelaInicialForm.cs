using System;
using System.Drawing;
using System.Windows.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Services;
using ProjetosCADLaser.Utilities;

namespace ProjetosCADLaser.Forms
{
    public sealed partial class TelaInicialForm : Form
    {
        private readonly AppServices _servicos;
        private ConfiguracaoLocal _local;

        private readonly Button _cadastrar = CriarBotao("Cadastrar");
        private readonly Button _testeTextura = CriarBotao("Teste de textura");
        private readonly Button _pesquisar = CriarBotao("Pesquisar");
        private readonly Button _sair = CriarBotao("Sair");
        private readonly Label _aviso = new Label { AutoSize = true, TextAlign = ContentAlignment.MiddleCenter };
        private readonly Button _testarNovamente = new Button { Text = "Testar novamente", AutoSize = true, Visible = false };

        private TableLayoutPanel _raiz;
        private TableLayoutPanel _cabecalho;
        private TableLayoutPanel _conteudoCard;
        private TableLayoutPanel _gradeAcoes;
        private FlowLayoutPanel _rodape;
        private Panel _cardAcoes;
        private Label _titulo;
        private Label _subtitulo;
        private Label _usuario;
        private ComboBox _tema;

        public TelaInicialForm(AppServices servicos, ConfiguracaoLocal local)
        {
            InitializeComponent();
            _servicos = servicos;
            _local = local;

            Text = "PROJETOS CAD/LASER";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(720, 520);
            Size = new Size(840, 600);
            Font = new Font("Segoe UI", 10F);

            CriarInterface();
            ConectarEventos();

            _servicos.Tema.TemaAlterado += TemaAlterado;
            FormClosed += delegate { _servicos.Tema.TemaAlterado -= TemaAlterado; };

            AtualizarTema();
        }

        private void CriarInterface()
        {
            Controls.Clear();

            _raiz = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(36, 28, 36, 20)
            };
            _raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(_raiz);

            CriarCabecalho();
            CriarAreaCentral();
            CriarRodape();
        }

        private void CriarCabecalho()
        {
            _cabecalho = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 0, 0, 20)
            };

            _titulo = new Label
            {
                Text = "PROJETOS CAD/LASER",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Font = new Font("Segoe UI Semibold", 24F),
                Margin = new Padding(0)
            };

            _subtitulo = new Label
            {
                Text = "Cadastro, consulta e acompanhamento dos projetos do setor Laser",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(2, 6, 0, 0)
            };

            _cabecalho.Controls.Add(_titulo, 0, 0);
            _cabecalho.Controls.Add(_subtitulo, 0, 1);
            _raiz.Controls.Add(_cabecalho, 0, 0);
        }

        private void CriarAreaCentral()
        {
            var hospedeiro = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
                Margin = new Padding(0)
            };

            _cardAcoes = new Panel
            {
                Anchor = AnchorStyles.None,
                Size = new Size(540, _local.Perfil == PerfilUsuario.Operacional ? 390 : 250),
                Padding = new Padding(26)
            };

            _conteudoCard = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Margin = new Padding(0)
            };
            _conteudoCard.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _conteudoCard.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _conteudoCard.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _conteudoCard.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _conteudoCard.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _cardAcoes.Controls.Add(_conteudoCard);

            var tituloAcoes = new Label
            {
                Text = "Ações",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 15F),
                Margin = new Padding(0, 0, 0, 4)
            };
            _conteudoCard.Controls.Add(tituloAcoes, 0, 0);

            var instrucao = new Label
            {
                Text = "Escolha uma opção para continuar.",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 0, 0, 18)
            };
            _conteudoCard.Controls.Add(instrucao, 0, 1);

            _gradeAcoes = new TableLayoutPanel
            {
                AutoSize = true,
                Anchor = AnchorStyles.None,
                ColumnCount = 1,
                RowCount = _local.Perfil == PerfilUsuario.Operacional ? 3 : 1,
                Margin = new Padding(0)
            };
            _gradeAcoes.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 226));

            if (_local.Perfil == PerfilUsuario.Operacional)
            {
                _gradeAcoes.Controls.Add(_cadastrar, 0, 0);
                _gradeAcoes.Controls.Add(_pesquisar, 0, 1);
                _gradeAcoes.Controls.Add(_testeTextura, 0, 2);
            }
            else
            {
                _gradeAcoes.Controls.Add(_pesquisar, 0, 0);
                _pesquisar.Width = 214;
            }

            _conteudoCard.Controls.Add(_gradeAcoes, 0, 2);

            var linhaInferior = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 16, 0, 0)
            };
            _sair.Width = 120;
            _sair.Height = 40;
            _sair.Margin = new Padding(0);
            linhaInferior.Controls.Add(_sair);
            _conteudoCard.Controls.Add(linhaInferior, 0, 3);

            var areaAviso = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Anchor = AnchorStyles.None,
                Margin = new Padding(0, 14, 0, 0)
            };
            _aviso.MaximumSize = new Size(460, 0);
            _aviso.Anchor = AnchorStyles.None;
            _aviso.Margin = new Padding(0, 0, 0, 8);
            _testarNovamente.Anchor = AnchorStyles.None;
            areaAviso.Controls.Add(_aviso);
            areaAviso.Controls.Add(_testarNovamente);
            _conteudoCard.Controls.Add(areaAviso, 0, 4);

            hospedeiro.Controls.Add(_cardAcoes, 0, 0);
            _raiz.Controls.Add(hospedeiro, 0, 1);
        }

        private void CriarRodape()
        {
            _usuario = new Label
            {
                Text = (_local.NomeExibido ?? Environment.UserName) + " · " + _local.Perfil,
                AutoSize = true,
                Margin = new Padding(0, 7, 14, 0)
            };

            _tema = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 155,
                Margin = new Padding(0)
            };
            _tema.Items.AddRange(Apresentacao.Temas);
            _tema.SelectedItem = Apresentacao.Tema(_local.Tema);
            _tema.SelectedValueChanged += delegate { AlterarTema(Apresentacao.Tema(_tema.Text)); };

            _rodape = new FlowLayoutPanel
            {
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 16, 0, 0)
            };
            _rodape.Controls.Add(_usuario);
            _rodape.Controls.Add(_tema);
            _raiz.Controls.Add(_rodape, 0, 2);
        }

        private void ConectarEventos()
        {
            _cadastrar.Click += delegate { AbrirCadastro(); };
            _testeTextura.Click += delegate { AbrirTesteTextura(); };
            _pesquisar.Click += delegate { AbrirPesquisa(); };
            _sair.Click += delegate { Close(); };
            _testarNovamente.Click += delegate { AtualizarDisponibilidade(); };
        }

        private static Button CriarBotao(string texto)
        {
            return new Button
            {
                Text = texto,
                Width = 214,
                Height = 44,
                Anchor = AnchorStyles.None,
                Margin = new Padding(6),
                Font = new Font("Segoe UI Semibold", 10F)
            };
        }

        private void AlterarTema(PreferenciaTema tema)
        {
            if (_local.Tema == tema) return;
            _local.Tema = tema;
            _servicos.ConfiguracaoLocal.Salvar(_local);
            _servicos.Tema.Definir(tema);
        }

        private void AbrirCadastro()
        {
            if (!_cadastrar.Enabled)
            {
                MostrarIndisponibilidade("Cadastro de piloto");
                return;
            }

            using (var form = new CadastroPilotoForm(_servicos, _local)) form.ShowDialog(this);
            AtualizarDisponibilidade();
        }

        private void AbrirPesquisa()
        {
            if (!_pesquisar.Enabled)
            {
                MostrarIndisponibilidade("Pesquisa");
                return;
            }

            using (var form = new PesquisaForm(_servicos, _local)) form.ShowDialog(this);
        }

        private void AbrirTesteTextura()
        {
            if (!_testeTextura.Enabled)
            {
                MostrarIndisponibilidade("Teste de textura");
                return;
            }

            using (var form = new TesteTexturaForm(_servicos, _local)) form.ShowDialog(this);
        }

       
        private void AtualizarDisponibilidade()
        {
            var resultado = _servicos.Inicializacao.VerificarDisponibilidade(_local);
            _cadastrar.Enabled = _testeTextura.Enabled = _pesquisar.Enabled = resultado.Sucesso;
            _aviso.Text = resultado.Sucesso ? "" : "Pasta de dados indisponível.\n" + resultado.Mensagem;
            _aviso.ForeColor = resultado.Sucesso ? _servicos.Tema.Paleta.TextoSecundario : _servicos.Tema.Paleta.Perigo;
            _testarNovamente.Visible = !resultado.Sucesso;
        }

        private void MostrarIndisponibilidade(string recurso)
        {
            if (!_cadastrar.Enabled && recurso != "Pesquisa")
                MessageBox.Show(this, _aviso.Text, recurso, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void TemaAlterado(object sender, EventArgs e)
        {
            AtualizarTema();
        }

        private void AtualizarTema()
        {
            _servicos.Tema.Aplicar(this);

            var paleta = _servicos.Tema.Paleta;
            BackColor = paleta.Fundo;

            if (_raiz != null) _raiz.BackColor = paleta.Fundo;
            if (_cabecalho != null) _cabecalho.BackColor = paleta.Fundo;
            if (_rodape != null) _rodape.BackColor = paleta.Fundo;
            if (_cardAcoes != null) _cardAcoes.BackColor = paleta.Superficie;
            if (_conteudoCard != null) _conteudoCard.BackColor = paleta.Superficie;
            if (_gradeAcoes != null) _gradeAcoes.BackColor = paleta.Superficie;

            if (_subtitulo != null) _subtitulo.ForeColor = paleta.TextoSecundario;
            if (_usuario != null) _usuario.ForeColor = paleta.TextoSecundario;

            _sair.BackColor = paleta.Superficie;
            _sair.ForeColor = paleta.Texto;
            _sair.FlatStyle = FlatStyle.Flat;
            _sair.FlatAppearance.BorderSize = 1;
            _sair.FlatAppearance.BorderColor = paleta.Borda;
            _sair.FlatAppearance.MouseOverBackColor = paleta.Fundo;

            AtualizarDisponibilidade();
        }
    }
}
