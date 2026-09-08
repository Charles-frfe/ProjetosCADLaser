using ProjetosCADLaser.Repositories;

namespace ProjetosCADLaser.Services
{
    public sealed class AppServices
    {
        public JsonService Json { get; private set; } public PinService Pin { get; private set; } public AcessoPastaService AcessoPasta { get; private set; } public TemaService Tema { get; private set; } public LogService Log { get; private set; } public PesquisaPastaOrigemService PesquisaPastaOrigem { get; private set; } public PesquisaPilotosService PesquisaPilotos { get; private set; } public TesteTexturaService TestesTextura { get; private set; } public LixeiraService Lixeira { get; private set; } public CancelamentoPilotoService CancelamentoPiloto { get; private set; } public DiagnosticoService Diagnostico { get; private set; } public EdicaoPilotoService EdicaoPiloto { get; private set; } public AnexoService Anexos { get; private set; } public MatrizService Matrizes { get; private set; } public ConfiguracaoLocalService ConfiguracaoLocal { get; private set; } public ConfiguracaoCompartilhadaService ConfiguracaoCompartilhada { get; private set; } public InicializacaoService Inicializacao { get; private set; } public AdministracaoService Administracao { get; private set; } public CadastroPilotoService CadastroPiloto { get; private set; } public ContinuacaoCadastroService ContinuacaoCadastro { get; private set; } public BloqueioService Bloqueios { get; private set; }
        public AppServices()
        {
            Json = new JsonService(); Pin = new PinService(); AcessoPasta = new AcessoPastaService(); Tema = new TemaService(); Log = new LogService(); PesquisaPastaOrigem = new PesquisaPastaOrigemService(); Anexos = new AnexoService(); Matrizes = new MatrizService(); ConfiguracaoLocal = new ConfiguracaoLocalService(Json); ConfiguracaoCompartilhada = new ConfiguracaoCompartilhadaService(Json); Inicializacao = new InicializacaoService(ConfiguracaoLocal, ConfiguracaoCompartilhada, AcessoPasta, Pin); Administracao = new AdministracaoService(Pin); var repositorio = new PilotoRepository(Json); CadastroPiloto = new CadastroPilotoService(AcessoPasta, repositorio, Anexos, Matrizes); ContinuacaoCadastro = new ContinuacaoCadastroService(repositorio, Anexos, Matrizes); Bloqueios = new BloqueioService(Json); PesquisaPilotos = new PesquisaPilotosService(Json, Log); TestesTextura = new TesteTexturaService(Json, Anexos); Lixeira = new LixeiraService(Json, Log); CancelamentoPiloto = new CancelamentoPilotoService(Json); Diagnostico = new DiagnosticoService(this); EdicaoPiloto = new EdicaoPilotoService(Json);
        }
    }
}
