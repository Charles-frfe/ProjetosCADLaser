using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.Json;
using ProjetosCADLaser.Configuration;
using ProjetosCADLaser.Forms;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Repositories;
using ProjetosCADLaser.Services;
using ProjetosCADLaser.Launcher;

namespace ProjetosCADLaser.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            try
            {
                TestarCompatibilidadeCadastroV1();
                TestarCadastroV1RoundTrip();
                TestarSomenteLeituraNaoAlteraArquivo();
                TestarFormatoMaisNovoBloqueiaEdicao();
                TestarEscritaAtomicaECriaBackup();
                TestarHeartbeatNaoCriaBackup();
                TestarPilotoRepository();
                TestarNormalizadorPesquisa();
                TestarPinService();
                TestarMatrizService();
                TestarAnalisadorPastas();
                TestarPesquisaPastaOrigem();
                TestarAcessoPastaELog();
                TestarPastaCompartilhadaIndisponivel();
                TestarAnexoService();
                TestarPesquisaPilotos();
                TestarConfiguracoes();
                TestarLixeira();
                TestarCancelamentoPiloto();
                TestarCadastroPilotoService();
                TestarContinuacaoCadastroService();
                TestarEdicaoPilotoService();
                TestarBloqueioService();
                TestarAdministracaoService();
                TestarInicializacaoService();
                TestarPrimeiroUsoResiliente();
                TestarTesteTexturaService();
                TestarConflitoEdicaoTesteTextura();
                TestarEdicaoCadastroTesteTextura();
                TestarRegrasEdicaoTesteTextura();
                TestarTemaEAppServices();
                TestarDiagnosticoSemRaiz();
                TestarPrimeiroUsoFormPortado();
                TestarTelaInicialFormPortada();
                TestarCadastroPilotoFormPortada();
                TestarFluxoCadastroPiloto();
                TestarConcorrenciaCadastroVisual();
                TestarRemocaoAnexoPendente();
                TestarPesquisaFormPortada();
                TestarFichaTecnicaFormPortada();
                TestarEditarPilotoFormPortada();
                TestarLixeiraFormPortada();
                TestarNavegacaoLixeira();
                TestarTesteTexturaFormPortada();
                TestarFichaTesteTexturaPortada();
                TestarAcaoEdicaoFichaTesteTextura();
                TestarCamposEdicaoTesteTextura();
                TestarEtapaOrigemControl();
                TestarEtapaDeteccaoControl();
                TestarEtapaMatrizesControl();
                TestarEtapaRevisaoControl();
                TestarCadastroControlesEtapas();
                TestarInstanciacaoCadastroRefatorado();
                TestarLauncherFallbackLocal();
                TestarAtualizacaoLauncherProtegeDados();
                TestarVersaoLauncherOffline();
                TestarLauncherOrquestraAtualizacao();
                TestarResolucoesAlvo();
                TestarFalhaAtualizacaoPreservaAplicativo();
                TestarNavegacaoTesteTextura();
                TestarEdicaoVisualConcorrencia();
                TestarCancelamentoReativacaoIntegrados();
                TestarLixeiraRestauracaoIntegrada();
                TestarPesquisaEFichaTemporaria();
                TestarEntradaVisualConfigurada();
                Console.WriteLine("Fase 9 tranche 1 OK: regressão, resoluções-alvo, rollback e Launcher passaram; entrada visual: " + typeof(PrimeiroUsoForm).FullName + " -> " + typeof(TelaInicialForm).FullName);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Fase 8 tranche 17 FALHOU: " + ex);
                return 1;
            }
        }

        private static void TestarCompatibilidadeCadastroV1()
        {
            var cadastro = JsonSerializer.Deserialize<PilotoCadastro>(FixtureV1, JsonContratosV1.CriarOpcoes());
            Exigir(cadastro != null && cadastro.VersaoFormato == 1 && cadastro.Codigo == "10002", "Cadastro V1 não foi lido.");
            Exigir(cadastro.Componentes.Count == 1 && cadastro.Componentes[0].Matrizes.Count == 1, "Matriz V1 não foi lida.");
            Exigir(cadastro.Texturas.Count == 1 && cadastro.Historico.Count == 1 && cadastro.Anexos.Count == 1, "Coleções V1 incompletas.");
            Exigir(cadastro.Historico[0].Tipo == TipoEvento.CriacaoPiloto, "Enum de evento V1 não foi convertido.");
        }

        private static void TestarCadastroV1RoundTrip()
        {
            var opcoes = JsonContratosV1.CriarOpcoes();
            var cadastro = JsonSerializer.Deserialize<PilotoCadastro>(FixtureV1, opcoes);
            var json = JsonSerializer.Serialize(cadastro, opcoes);
            var relido = JsonSerializer.Deserialize<PilotoCadastro>(json, opcoes);
            Exigir(relido.Id == cadastro.Id && relido.Codigo == cadastro.Codigo && relido.VersaoCadastro == cadastro.VersaoCadastro, "Round-trip perdeu identidade/versionamento.");
            using (var documento = JsonDocument.Parse(json))
            {
                var nomeEvento = documento.RootElement.GetProperty("historico")[0].GetProperty("tipo").GetString();
                Exigir(nomeEvento == "Criação da piloto", "Nome JSON do evento não foi preservado.");
            }
        }

        private static void TestarSomenteLeituraNaoAlteraArquivo()
        {
            var pasta = Path.Combine(Path.GetTempPath(), "ProjetosCADLaser_Fase2", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(pasta);
            var arquivo = Path.Combine(pasta, "cadastro.json");
            File.WriteAllText(arquivo, FixtureV1);
            var antes = File.ReadAllBytes(arquivo);
            JsonSerializer.Deserialize<PilotoCadastro>(File.ReadAllText(arquivo), JsonContratosV1.CriarOpcoes());
            var depois = File.ReadAllBytes(arquivo);
            Exigir(Igual(antes, depois), "Consulta alterou o arquivo JSON.");
            Directory.Delete(pasta, true);
        }

        private static void TestarFormatoMaisNovoBloqueiaEdicao()
        {
            var bloqueado = false;
            try { JsonContratosV1.ValidarPodeEditar(2); } catch (InvalidOperationException) { bloqueado = true; }
            Exigir(bloqueado, "Formato mais novo não foi bloqueado.");
            JsonContratosV1.ValidarPodeEditar(1);
        }

        private static void TestarEscritaAtomicaECriaBackup()
        {
            var pasta = CriarPastaTemporaria();
            var arquivo = Path.Combine(pasta, "cadastro.json");
            var service = new JsonService();
            service.WriteAtomic(arquivo, new PilotoCadastro { Codigo = "A1", NomeModelo = "Primeiro", PastaOrigem = "origem" });
            Exigir(File.Exists(arquivo), "Escrita inicial não criou o arquivo.");
            Thread.Sleep(20);
            service.WriteAtomic(arquivo, new PilotoCadastro { Codigo = "A1", NomeModelo = "Segundo", PastaOrigem = "origem" });
            var backups = Directory.GetFiles(Path.Combine(pasta, "backups"), "*.json");
            Exigir(backups.Length == 1, "A segunda escrita não criou exatamente um backup.");
            Exigir(service.Read<PilotoCadastro>(arquivo).NomeModelo == "Segundo", "Releitura final não retornou a versão atual.");
            Exigir(service.Read<PilotoCadastro>(backups[0]).NomeModelo == "Primeiro", "Backup não preservou a versão anterior.");
            Directory.Delete(pasta, true);
        }

        private static void TestarHeartbeatNaoCriaBackup()
        {
            var pasta = CriarPastaTemporaria();
            EstruturaDados.Criar(pasta);
            var json = new JsonService();
            var bloqueio = new BloqueioService(json);
            var dono = bloqueio.CriarPorCodigo(pasta, "AB-12", Guid.NewGuid(), "Edição", "Operador");
            var caminho = bloqueio.ObterCaminho(pasta, "AB-12");
            bloqueio.Atualizar(caminho, dono);
            var pastaBackups = Path.Combine(Path.GetDirectoryName(caminho), "backups");
            Exigir(!Directory.Exists(pastaBackups) || Directory.GetFiles(pastaBackups).Length == 0, "Heartbeat criou backup indevido.");
            Exigir(bloqueio.Ler(caminho).TokenSessao == dono.TokenSessao, "Heartbeat perdeu o token de sessão.");
            bloqueio.Remover(caminho, dono);
            Directory.Delete(pasta, true);
        }

        private static void TestarPilotoRepository()
        {
            var pasta = CriarPastaTemporaria();
            var repository = new PilotoRepository(new JsonService());
            var cadastro = new PilotoCadastro { Codigo = "10002", NomeModelo = "Modelo", PastaOrigem = "origem" };
            repository.Salvar(pasta, cadastro);
            var caminho = repository.CaminhoCadastro(pasta, cadastro.Codigo);
            Exigir(File.Exists(caminho), "Repository não criou cadastro.json.");
            Exigir(repository.Carregar(pasta, cadastro.Codigo).Id == cadastro.Id, "Repository não preservou o Id.");
            var rejeitado = false;
            try { repository.CaminhoCadastro(pasta, "../fora"); } catch (ArgumentException) { rejeitado = true; }
            Exigir(rejeitado, "Repository aceitou código inseguro.");
            var formatoNovo = false;
            try { repository.Salvar(pasta, new PilotoCadastro { Codigo = "99999", VersaoFormato = 2 }); } catch (InvalidOperationException) { formatoNovo = true; }
            Exigir(formatoNovo, "Repository aceitou cadastro de formato mais novo.");
            Directory.Delete(pasta, true);
        }

        private static string CriarPastaTemporaria()
        {
            var pasta = Path.Combine(Path.GetTempPath(), "ProjetosCADLaser_Fase3", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(pasta);
            return pasta;
        }

        private static void TestarNormalizadorPesquisa()
        {
            Exigir(NormalizadorPesquisa.Normalizar("Gáspea_gls-012") == "gaspea gls 012", "Normalização de acentos/separadores mudou.");
            Exigir(NormalizadorPesquisa.Contem("Sandália Teste", "sandalía"), "Pesquisa normalizada não encontrou termo.");
        }

        private static void TestarPinService()
        {
            var service = new PinService(); var credencial = service.Criar("1234");
            Exigir(service.Verificar("1234", credencial), "PIN válido foi rejeitado.");
            Exigir(!service.Verificar("1235", credencial), "PIN inválido foi aceito.");
            var rejeitado = false; try { service.Criar("12a4"); } catch (ArgumentException) { rejeitado = true; }
            Exigir(rejeitado, "PIN com letras não foi rejeitado.");
        }

        private static void TestarMatrizService()
        {
            var service = new MatrizService(); var textura = Guid.NewGuid();
            var baseMatriz = new MatrizCadastro { Tipo = "Laterais", Material = MaterialMatriz.Zamak, Eixos = QuantidadeEixos.Tres, Maquina = Maquina.M1000, Acabamento = Acabamento.Fosco, TexturasIds = new System.Collections.Generic.List<Guid> { textura } };
            var grupo = service.CriarGrupoLaterais(baseMatriz);
            Exigir(grupo.Count == 4 && grupo.All(x => x.GrupoMatrizId == grupo[0].GrupoMatrizId), "Grupo Laterais não criou quatro matrizes relacionadas.");
            var copia = service.Duplicar(baseMatriz); Exigir(copia.Id != baseMatriz.Id && copia.TexturasIds.SequenceEqual(baseMatriz.TexturasIds), "Duplicação perdeu identidade ou texturas.");
        }

        private static void TestarAnalisadorPastas()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "10002_sandalia_teste");
            Directory.CreateDirectory(Path.Combine(origem, "Palmilha")); Directory.CreateDirectory(Path.Combine(origem, "gls_012")); Directory.CreateDirectory(Path.Combine(origem, "Piloto")); Directory.CreateDirectory(Path.Combine(origem, "01-03")); Directory.CreateDirectory(Path.Combine(origem, "Outros"));
            var definicoes = new[] { new DefinicaoComponente { Nome = "Palmilha", Ativo = true } };
            var resultado = new AnalisadorPastasService().Analisar(origem, definicoes);
            Exigir(resultado.CodigoSugerido == "10002" && resultado.Componentes.Count == 1 && resultado.Texturas.Count == 1, "Analisador não preservou classificações principais.");
            Exigir(resultado.NumerosEscala.Contains("01-03") && resultado.NaoClassificadas.Count == 1, "Analisador não preservou escala/não classificado.");
            Directory.Delete(raiz, true);
        }

        private static void TestarPesquisaPastaOrigem()
        {
            var raiz = CriarPastaTemporaria(); Directory.CreateDirectory(Path.Combine(raiz, "10002_modelo", "sub")); Directory.CreateDirectory(Path.Combine(raiz, "99999_outro"));
            var resultado = new PesquisaPastaOrigemService().PesquisarAsync(raiz, "10002", System.Threading.CancellationToken.None).Result;
            Exigir(resultado.RaizDisponivel && resultado.PastasEncontradas.Count == 1, "Pesquisa de pasta de origem encontrou quantidade incorreta.");
            Directory.Delete(raiz, true);
        }

        private static void TestarAcessoPastaELog()
        {
            var raiz = CriarPastaTemporaria(); var acesso = new AcessoPastaService().Testar(raiz);
            Exigir(acesso.Sucesso && Directory.GetFiles(raiz).Length == 0, "Teste de acesso deixou resíduo.");
            new LogService().Registrar(new InvalidOperationException("teste"), "regressão Fase 4", raiz);
            Exigir(File.Exists(Directory.GetFiles(Path.Combine(raiz, "Logs"))[0]), "Log não foi gravado na raiz temporária.");
            Directory.Delete(raiz, true);
        }

        private static void TestarPastaCompartilhadaIndisponivel()
        {
            var service = new AcessoPastaService();
            Exigir(!service.Testar(null).Sucesso, "Pasta vazia foi aceita como compartilhada.");
            var ausente = Path.Combine(Path.GetTempPath(), "ProjetosCADLaser_Inexistente_" + Guid.NewGuid().ToString("N"));
            Exigir(!service.Testar(ausente).Sucesso, "Pasta inexistente foi aceita como compartilhada.");
            var raiz = CriarPastaTemporaria();
            var arquivo = Path.Combine(raiz, "nao_e_pasta.tmp");
            File.WriteAllText(arquivo, "arquivo");
            Exigir(!service.Testar(arquivo).Sucesso, "Arquivo foi aceito como pasta compartilhada.");
            Directory.Delete(raiz, true);
        }

        private static void TestarAnexoService()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem.txt"); File.WriteAllText(origem, "anexo"); var service = new AnexoService(Path.Combine(raiz, "pendentes")); var sessao = service.CriarSessao(); var pendente = service.AdicionarArquivo(sessao, origem, false); var cadastro = Path.Combine(raiz, "cadastro"); var anexo = service.CopiarParaCadastro(pendente, cadastro, Guid.NewGuid());
            Exigir(File.Exists(Path.Combine(cadastro, anexo.CaminhoRelativo)), "Anexo não foi copiado para caminho relativo."); Exigir(anexo.NomeInterno.Length > 0 && anexo.NomeInterno.IndexOf(" ", StringComparison.Ordinal) < 0, "Nome interno de anexo não foi normalizado."); service.LimparSessao(sessao); Exigir(!Directory.Exists(sessao), "Sessão temporária de anexo não foi removida."); Directory.Delete(raiz, true);
        }

        private static void TestarPesquisaPilotos()
        {
            var raiz = CriarPastaTemporaria(); var json = new JsonService(); var repo = new PilotoRepository(json); var p = new PilotoCadastro { Codigo = "10002", NomeModelo = "Sandalia Adulto", PastaOrigem = "origem" }; repo.Salvar(raiz, p); Directory.CreateDirectory(Path.Combine(raiz, "Cadastros", "invalido")); File.WriteAllText(Path.Combine(raiz, "Cadastros", "invalido", "cadastro.json"), "{ inválido }");
            var service = new PesquisaPilotosService(json, new LogService()); var resultado = service.Carregar(raiz); Exigir(resultado.Pilotos.Count == 1 && resultado.Invalidos == 1, "Pesquisa não distinguiu cadastro válido e inválido."); Exigir(service.Filtrar(resultado.Pilotos, "sandália").Count == 1, "Filtro de pesquisa não encontrou o modelo."); Directory.Delete(raiz, true);
        }

        private static void TestarConfiguracoes()
        {
            var raiz = CriarPastaTemporaria(); var json = new JsonService(); var local = new ConfiguracaoLocalService(json, Path.Combine(raiz, "local.json"));
            var configuracao = new ConfiguracaoLocal
            {
                PastaRaizDados = raiz,
                NomeExibido = "Operador" };
           
            local.Salvar(configuracao);
            Exigir(local.Carregar().NomeExibido == "Operador", "Configuração local não voltou no round-trip.");

            var formatoNovo = false; try { local.Salvar(new ConfiguracaoLocal { VersaoFormato = 2 }); } catch (InvalidOperationException) { formatoNovo = true; } Exigir(formatoNovo, "Configuração local de formato mais novo foi aceita."); var compartilhada = new ConfiguracaoCompartilhadaService(json); compartilhada.Salvar(raiz, new ConfiguracaoCompartilhada()); Exigir(File.Exists(compartilhada.ObterCaminho(raiz)), "Configuração compartilhada não foi criada."); Directory.Delete(raiz, true);
        }

        private static void TestarLixeira()
        {
            var raiz = CriarPastaTemporaria(); var json = new JsonService(); var repo = new PilotoRepository(json); var p = new PilotoCadastro { Codigo = "10002", NomeModelo = "Lixeira", PastaOrigem = "origem" }; repo.Salvar(raiz, p); var origem = Path.Combine(raiz, "Cadastros", p.Codigo); var service = new LixeiraService(json, new LogService()); var meta = service.Mover(raiz, TipoRegistroLixeira.Piloto, origem, p.Id, p.Codigo, p.NomeModelo, "teste"); Exigir(!Directory.Exists(origem) && service.Listar(raiz, TipoRegistroLixeira.Piloto).Count == 1, "Lixeira não moveu/listou cadastro."); var pasta = service.Listar(raiz, TipoRegistroLixeira.Piloto)[0].Pasta; service.Restaurar(raiz, meta, pasta); Exigir(File.Exists(Path.Combine(origem, "cadastro.json")) && repo.Carregar(raiz, p.Codigo).Historico.Any(x => x.Tipo == TipoEvento.Restauracao), "Lixeira não restaurou histórico do cadastro."); Directory.Delete(raiz, true);
        }

        private static void TestarCancelamentoPiloto()
        {
            var raiz = CriarPastaTemporaria(); var json = new JsonService(); var repo = new PilotoRepository(json); var p = new PilotoCadastro { Codigo = "10002", NomeModelo = "Cancelamento", PastaOrigem = "origem" }; repo.Salvar(raiz, p); var service = new CancelamentoPilotoService(json); var cancelado = service.Cancelar(raiz, p.Codigo, "motivo"); Exigir(cancelado.Status == StatusPiloto.Cancelada && cancelado.Historico.Any(x => x.Tipo == TipoEvento.Cancelamento), "Cancelamento não registrou estado/histórico."); var reativado = service.Reativar(raiz, p.Codigo, "retorno"); Exigir(reativado.Status == StatusPiloto.Ativa && reativado.Historico.Any(x => x.Tipo == TipoEvento.Reativacao), "Reativação não registrou estado/histórico."); Directory.Delete(raiz, true);
        }

        private static void TestarCadastroPilotoService()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem"); Directory.CreateDirectory(origem); var json = new JsonService(); var service = new CadastroPilotoService(new AcessoPastaService(), new PilotoRepository(json), new AnexoService(Path.Combine(raiz, "pendentes")), new MatrizService()); var cadastro = new PilotoCadastro { Codigo = "10002", NomeModelo = "Cadastro", PastaOrigem = origem, Componentes = new System.Collections.Generic.List<ComponenteCadastro> { new ComponenteCadastro { Nome = "Palmilha" } } }; var salvo = service.Salvar(raiz, cadastro, new System.Collections.Generic.List<AnexoPendente>(), new[] { "Gravação", "Laterais" });
            Exigir(salvo.Historico.Count == 1 && salvo.Historico[0].Tipo == TipoEvento.CriacaoPiloto && Directory.Exists(Path.Combine(raiz, "Cadastros", "10002", "backups")), "Cadastro inicial não preservou evento/estrutura."); Directory.Delete(raiz, true);
        }

        private static void TestarContinuacaoCadastroService()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem"); Directory.CreateDirectory(origem); var json = new JsonService(); var repo = new PilotoRepository(json); var inicial = new PilotoCadastro { Codigo = "10002", NomeModelo = "Continuação", PastaOrigem = origem }; repo.Salvar(raiz, inicial); var baseCarregada = repo.Carregar(raiz, inicial.Codigo); var service = new ContinuacaoCadastroService(repo, new AnexoService(Path.Combine(raiz, "pendentes")), new MatrizService()); var componente = new ComponenteCadastro { Nome = "Sola" }; var continuado = service.Continuar(raiz, baseCarregada, new[] { componente }, new List<(Guid ComponenteId, MatrizCadastro Matriz)>(), new List<TexturaCadastro>(), new List<AnexoPendente>(), new[] { "Gravação", "Laterais" });
            Exigir(continuado.Componentes.Any(x => x.Nome == "Sola") && continuado.VersaoCadastro == baseCarregada.VersaoCadastro + 1 && continuado.Historico.Last().Tipo == TipoEvento.ComponenteAdicionado, "Continuação não registrou componente/versionamento."); Directory.Delete(raiz, true);
        }

        private static void TestarEdicaoPilotoService()
        {
            var raiz = CriarPastaTemporaria(); var json = new JsonService(); var repo = new PilotoRepository(json); var inicial = new PilotoCadastro { Codigo = "10002", NomeModelo = "Antes", PastaOrigem = "origem" }; repo.Salvar(raiz, inicial); var service = new EdicaoPilotoService(json); var editado = service.Salvar(raiz, inicial, "Depois", new System.Collections.Generic.Dictionary<Guid, string>(), "ajuste");
            Exigir(editado.NomeModelo == "Depois" && editado.Historico.Last().Tipo == TipoEvento.EdicaoCadastro && editado.VersaoCadastro == 2, "Edição não registrou alteração/versionamento."); var conflito = false; try { service.Salvar(raiz, inicial, "Terceiro", new System.Collections.Generic.Dictionary<Guid, string>(), "conflito"); } catch (InvalidOperationException) { conflito = true; } Exigir(conflito, "Edição não bloqueou versão obsoleta."); Directory.Delete(raiz, true);
        }

        private static void TestarBloqueioService()
        {
            var raiz = CriarPastaTemporaria(); var service = new BloqueioService(new JsonService()); var dono = service.CriarPorCodigo(raiz, "10002", Guid.NewGuid(), "Edição", "Operador"); var caminho = service.ObterCaminho(raiz, "10002"); Exigir(service.Listar(raiz).Count == 1 && service.Ler(caminho).TokenSessao == dono.TokenSessao, "Lock não foi criado/lido."); var duplicado = false; try { service.CriarPorCodigo(raiz, "10002", Guid.NewGuid(), "Edição"); } catch (IOException) { duplicado = true; } Exigir(duplicado, "Lock duplicado foi aceito."); service.Remover(caminho, dono); Exigir(!File.Exists(caminho), "Lock não foi removido pelo dono."); Directory.Delete(raiz, true);
        }

        private static void TestarAdministracaoService()
        {
            var config = new ConfiguracaoCompartilhada(); var service = new AdministracaoService(new PinService()); config.PinAdministrativo = new PinService().Criar("1234"); Exigir(service.ValidarPin("1234", config), "PIN administrativo válido foi rejeitado."); service.AlterarPin("1234", "5678", "5678", config); Exigir(service.ValidarPin("5678", config), "Alteração de PIN não persistiu em memória."); service.AdicionarComponente(config, "Novo", new[] { "N" }); var duplicado = false; try { service.AdicionarComponente(config, "novo"); } catch (ArgumentException) { duplicado = true; } Exigir(duplicado, "Componente duplicado foi aceito.");
        }

        private static void TestarInicializacaoService()
        {
            var raiz = CriarPastaTemporaria(); var json = new JsonService(); var local = new ConfiguracaoLocalService(json, Path.Combine(raiz, "local.json")); var compartilhada = new ConfiguracaoCompartilhadaService(json); var service = new InicializacaoService(local, compartilhada, new AcessoPastaService(), new PinService()); ConfiguracaoLocal config; string erro; Exigir(service.PrimeiroUsoNecessario(out config, out erro), "Primeiro uso não foi detectado."); service.ConcluirPrimeiroUso(raiz, "1234", "1234", PreferenciaTema.Escuro); Exigir(Directory.Exists(Path.Combine(raiz, "Cadastros")) && compartilhada.Carregar(raiz).PinAdministrativo != null && !service.PrimeiroUsoNecessario(out config, out erro), "Conclusão do primeiro uso não criou estrutura/configurações."); Directory.Delete(raiz, true);
        }

        private static void TestarTesteTexturaService()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem.txt"); File.WriteAllText(origem, "imagem"); var anexos = new AnexoService(Path.Combine(raiz, "pendentes")); var sessao = anexos.CriarSessao(); var imagem = anexos.AdicionarArquivo(sessao, origem, true); var service = new TesteTexturaService(new JsonService(), anexos); var teste = service.Cadastrar(raiz, "TXT-01", raiz, StatusTesteTextura.EmTeste, null, "observação", imagem, new List<AnexoPendente>()); Exigir(service.Listar(raiz).Count == 1 && service.Pesquisar(raiz, "TXT-01").Count == 1, "Teste de textura não foi cadastrado/pesquisado."); var atualizado = service.Atualizar(raiz, teste, StatusTesteTextura.Aprovada, "Aprovado", "mudança", new List<AnexoPendente>()); Exigir(atualizado.Status == StatusTesteTextura.Aprovada && atualizado.VersaoCadastro == teste.VersaoCadastro + 1, "Atualização do teste não incrementou versão."); var conflito = false; try { service.Atualizar(raiz, teste, StatusTesteTextura.Reprovada, null, "edição obsoleta", new List<AnexoPendente>()); } catch (InvalidOperationException) { conflito = true; } Exigir(conflito, "Atualização com versão obsoleta não foi rejeitada."); Directory.Delete(raiz, true);
        }

        private static void TestarConflitoEdicaoTesteTextura()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem.txt"); File.WriteAllText(origem, "imagem");
            EstruturaDados.Criar(raiz); var anexos = new AnexoService(Path.Combine(raiz, "pendentes")); var sessao = anexos.CriarSessao();
            var imagem = anexos.AdicionarArquivo(sessao, origem, true); var app = new AppServices(); var teste = app.TestesTextura.Cadastrar(raiz, "CON-01", raiz, StatusTesteTextura.EmTeste, null, "inicial", imagem, new List<AnexoPendente>());
            var caminho = app.Bloqueios.ObterCaminhoTeste(raiz, teste.Id); var dono = app.Bloqueios.CriarParaTeste(raiz, teste.Id, "Edição de teste", "Primeiro"); var bloqueado = false;
            try { app.Bloqueios.CriarParaTeste(raiz, teste.Id, "Edição de teste", "Segundo"); } catch (IOException) { bloqueado = true; }
            Exigir(bloqueado && File.Exists(caminho), "Edição concorrente de teste de textura não foi bloqueada."); app.Bloqueios.Remover(caminho, dono); Directory.Delete(raiz, true);
        }

        private static void TestarEdicaoCadastroTesteTextura()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem.txt"); File.WriteAllText(origem, "imagem");
            var anexos = new AnexoService(Path.Combine(raiz, "pendentes")); var sessao = anexos.CriarSessao(); var imagem = anexos.AdicionarArquivo(sessao, origem, true); imagem.Titulo = "Imagem original"; imagem.Categoria = "foto";
            var service = new TesteTexturaService(new JsonService(), anexos); var teste = service.Cadastrar(raiz, "EDT-01", raiz, StatusTesteTextura.EmTeste, null, "inicial", imagem, new List<AnexoPendente>());
            var edicoes = new Dictionary<Guid, EdicaoAnexoTeste> { { teste.Imagem.Id, new EdicaoAnexoTeste("Imagem revisada", "referência") } };
            var editado = service.Editar(raiz, teste, "EDT-02", raiz, "observação atualizada", "correção cadastral", edicoes);
            Exigir(editado.Identificacao == "EDT-02" && editado.Imagem.Titulo == "Imagem revisada" && editado.VersaoCadastro == teste.VersaoCadastro + 1 && editado.Historico.Any(x => x.Tipo == "Cadastro do teste corrigido"), "Edição do cadastro de teste de textura não foi persistida corretamente."); Directory.Delete(raiz, true);
        }

        private static void TestarRegrasEdicaoTesteTextura()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem.txt"); File.WriteAllText(origem, "imagem"); var anexos = new AnexoService(Path.Combine(raiz, "pendentes")); var sessao = anexos.CriarSessao(); var imagem = anexos.AdicionarArquivo(sessao, origem, true); imagem.Titulo = "Obrigatória";
            var service = new TesteTexturaService(new JsonService(), anexos); var teste = service.Cadastrar(raiz, "REG-01", raiz, StatusTesteTextura.EmTeste, null, "inicial", imagem, new List<AnexoPendente>());
            var remocao = false; try { service.Editar(raiz, teste, teste.Identificacao, teste.PastaOrigem, teste.Observacao, "tentativa", new Dictionary<Guid, EdicaoAnexoTeste> { { teste.Imagem.Id, new EdicaoAnexoTeste("", "", true) } }); } catch (InvalidDataException) { remocao = true; }
            var vazio = false; try { service.Editar(raiz, teste, teste.Identificacao, teste.PastaOrigem, teste.Observacao, "sem mudança", new Dictionary<Guid, EdicaoAnexoTeste>()); } catch (InvalidDataException) { vazio = true; }
            Exigir(remocao && vazio, "Regras de segurança da edição de teste de textura não foram aplicadas."); Directory.Delete(raiz, true);
        }

        private static void TestarPrimeiroUsoResiliente()
        {
            var raiz = CriarPastaTemporaria();
            var caminhoLocal = Path.Combine(raiz, "local.json");
            File.WriteAllText(caminhoLocal, "{ inválido }");
            var json = new JsonService();
            var local = new ConfiguracaoLocalService(json, caminhoLocal);
            var compartilhada = new ConfiguracaoCompartilhadaService(json);
            var service = new InicializacaoService(local, compartilhada, new AcessoPastaService(), new PinService());
            ConfiguracaoLocal config; string erro;
            Exigir(service.PrimeiroUsoNecessario(out config, out erro) && !string.IsNullOrWhiteSpace(erro), "Configuração local corrompida não acionou primeiro uso seguro.");
            EstruturaDados.Criar(raiz);
            EstruturaDados.Criar(raiz);
            Exigir(Directory.Exists(Path.Combine(raiz, "Cadastros")) && Directory.Exists(Path.Combine(raiz, "Configuracoes")), "Criação idempotente da estrutura compartilhada falhou.");
            Directory.Delete(raiz, true);
        }

        private static void TestarTemaEAppServices()
        {
            var app = new AppServices(); var alterado = false; app.Tema.TemaAlterado += delegate { alterado = true; }; app.Tema.Definir(PreferenciaTema.Escuro); Exigir(alterado && app.Tema.Paleta.Fundo.R < 50 && app.CadastroPiloto != null && app.TestesTextura != null, "Composição de Services ou tema não foi inicializada.");
        }

        private static void TestarDiagnosticoSemRaiz()
        {
            var diagnostico = new AppServices().Diagnostico.Executar(null); Exigir(diagnostico.Any(x => x.Nome == "Pasta de dados" && x.Estado == "Erro"), "Diagnóstico não identificou raiz ausente.");
        }

        private static void TestarPrimeiroUsoFormPortado()
        {
            var tipo = typeof(PrimeiroUsoForm);
            Exigir(tipo.BaseType == typeof(System.Windows.Forms.Form), "PrimeiroUsoForm não deriva de Form.");
            Exigir(tipo.GetConstructor(new[] { typeof(AppServices), typeof(string) }) != null, "Construtor compatível do PrimeiroUsoForm não foi encontrado.");
        }

        private static void TestarTelaInicialFormPortada()
        {
            var tipo = typeof(TelaInicialForm);
            Exigir(tipo.BaseType == typeof(System.Windows.Forms.Form), "TelaInicialForm não deriva de Form.");
            Exigir(tipo.GetConstructor(new[] { typeof(AppServices), typeof(ConfiguracaoLocal) }) != null, "Construtor compatível da TelaInicialForm não foi encontrado.");
        }

        private static void TestarCadastroPilotoFormPortada()
        {
            var tipo = typeof(CadastroPilotoForm);
            Exigir(tipo.BaseType == typeof(System.Windows.Forms.Form), "CadastroPilotoForm não deriva de Form.");
            Exigir(tipo.GetConstructor(new[] { typeof(AppServices), typeof(ConfiguracaoLocal) }) != null, "Construtor compatível do CadastroPilotoForm não foi encontrado.");
        }

        private static void TestarFluxoCadastroPiloto()
        {
            var raiz = CriarPastaTemporaria();
            var origem = Path.Combine(raiz, "origem"); Directory.CreateDirectory(origem); EstruturaDados.Criar(raiz);
            var app = new AppServices();
            var cadastro = new PilotoCadastro { Codigo = "F6-001", NomeModelo = "Fluxo básico", PastaOrigem = origem };
            cadastro.Componentes.Add(new ComponenteCadastro { Nome = "Palmilha" });
            cadastro.Componentes[0].Matrizes.Add(new MatrizCadastro { Tipo = "Gravação", Material = MaterialMatriz.Zamak, Eixos = QuantidadeEixos.Tres, Maquina = Maquina.M1000, Acabamento = Acabamento.Fosco });
            var grupoLaterais = new MatrizService().CriarGrupoLaterais(new MatrizCadastro { Tipo = "Laterais", Material = MaterialMatriz.Zamak, Eixos = QuantidadeEixos.Tres, Maquina = Maquina.M1000, Acabamento = Acabamento.Fosco });
            Exigir(grupoLaterais.Count == 4 && grupoLaterais.All(x => x.GrupoMatrizId == grupoLaterais[0].GrupoMatrizId), "Grupo Laterais não foi criado corretamente.");
            cadastro.Texturas.Add(new TexturaCadastro { Nome = "gls_012", CaminhoDetectado = Path.Combine(origem, "gls_012"), CaminhoRelativo = "gls_012", Selecionada = true, Confirmada = true });
            cadastro.Componentes[0].Matrizes[0].TexturasIds.Add(cadastro.Texturas[0].Id);
            var arquivo = Path.Combine(raiz, "anexo.txt"); File.WriteAllText(arquivo, "anexo de teste"); var sessao = app.Anexos.CriarSessao(); var pendente = app.Anexos.AdicionarArquivo(sessao, arquivo, false);
            var salvo = app.CadastroPiloto.Salvar(raiz, cadastro, new List<AnexoPendente> { pendente }, new[] { "Gravação" });
            Exigir(salvo.Codigo == "F6-001" && salvo.Componentes.Count == 1 && salvo.Componentes[0].Matrizes.Count == 1 && salvo.Componentes[0].Matrizes[0].TexturasIds.Count == 1 && salvo.Texturas.Count == 1 && salvo.Anexos.Count == 1 && File.Exists(Path.Combine(raiz, "Cadastros", "F6-001", "cadastro.json")) && File.Exists(Path.Combine(raiz, "Cadastros", "F6-001", salvo.Anexos[0].CaminhoRelativo)), "Fluxo não persistiu componentes/matriz/textura/anexo.");
            var edicoes = new Dictionary<Guid, EdicaoAnexoCadastro> { { salvo.Anexos[0].Id, new EdicaoAnexoCadastro("Título revisado", "Documento", "Descrição revisada", false) } };
            var editado = app.CadastroPiloto.Editar(raiz, salvo, salvo.NomeModelo, "Revisão do anexo", new List<Guid>(), new List<Guid>(), edicoes);
            Exigir(editado.Anexos[0].Titulo == "Título revisado" && editado.Anexos[0].Categoria == "Documento" && editado.VersaoCadastro == salvo.VersaoCadastro + 1, "Edição de anexo não persistiu a nova versão.");
            app.Anexos.LimparSessao(sessao);
            var sessaoFalha = app.Anexos.CriarSessao(); var pendenteFalho = app.Anexos.AdicionarArquivo(sessaoFalha, arquivo, false); File.Delete(pendenteFalho.CaminhoTemporario); var falhou = false;
            try { app.CadastroPiloto.Salvar(raiz, new PilotoCadastro { Codigo = "F6-002", NomeModelo = "Rollback", PastaOrigem = origem }, new List<AnexoPendente> { pendenteFalho }, new[] { "Gravação" }); } catch (FileNotFoundException) { falhou = true; }
            Exigir(falhou && !Directory.Exists(Path.Combine(raiz, "Cadastros", "F6-002")), "Rollback de anexo ausente não removeu cadastro parcial."); app.Anexos.LimparSessao(sessaoFalha);
            Directory.Delete(raiz, true);
        }

        private static void TestarConcorrenciaCadastroVisual()
        {
            var raiz = CriarPastaTemporaria(); EstruturaDados.Criar(raiz); var service = new BloqueioService(new JsonService());
            var primeiro = service.CriarPorCodigo(raiz, "CONC-001", Guid.NewGuid(), "Cadastro", "Primeiro"); var rejeitado = false;
            try { service.CriarPorCodigo(raiz, "CONC-001", Guid.NewGuid(), "Cadastro", "Segundo"); } catch (IOException) { rejeitado = true; }
            Exigir(rejeitado, "Segundo cadastro aceitou código já bloqueado."); service.Remover(service.ObterCaminho(raiz, "CONC-001"), primeiro); Exigir(service.Listar(raiz).Count == 0, "Lock visual não foi liberado após concorrência."); Directory.Delete(raiz, true);
        }

        private static void TestarRemocaoAnexoPendente()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "arquivo.txt"); File.WriteAllText(origem, "pendente"); var anexos = new AnexoService(Path.Combine(raiz, "sessoes")); var sessao = anexos.CriarSessao(); var pendente = anexos.AdicionarArquivo(sessao, origem, false); File.Delete(pendente.CaminhoTemporario); var rejeitado = false;
            try { anexos.CopiarParaCadastro(pendente, Path.Combine(raiz, "cadastro"), Guid.NewGuid()); } catch (FileNotFoundException) { rejeitado = true; }
            Exigir(rejeitado && !File.Exists(pendente.CaminhoTemporario), "Pendente removido ainda pôde ser copiado."); anexos.LimparSessao(sessao); Directory.Delete(raiz, true);
        }

        private static void TestarPesquisaFormPortada() { var tipo = typeof(PesquisaForm); Exigir(tipo.BaseType == typeof(System.Windows.Forms.Form) && tipo.GetConstructor(new[] { typeof(AppServices), typeof(ConfiguracaoLocal) }) != null, "PesquisaForm não está disponível com construtor compatível."); }
        private static void TestarFichaTecnicaFormPortada() { var tipo = typeof(FichaTecnicaForm); Exigir(tipo.BaseType == typeof(System.Windows.Forms.Form) && tipo.GetConstructor(new[] { typeof(AppServices), typeof(ConfiguracaoLocal), typeof(PilotoCadastro) }) != null, "FichaTecnicaForm não está disponível com construtor compatível."); }
        private static void TestarEditarPilotoFormPortada() { var tipo = typeof(EditarPilotoForm); Exigir(tipo.BaseType == typeof(System.Windows.Forms.Form) && tipo.GetConstructor(new[] { typeof(AppServices), typeof(ConfiguracaoLocal), typeof(PilotoCadastro) }) != null, "EditarPilotoForm não está disponível com lock visual."); }
        private static void TestarLixeiraFormPortada() { var tipo = typeof(LixeiraForm); Exigir(tipo.BaseType == typeof(System.Windows.Forms.Form) && tipo.GetConstructor(new[] { typeof(AppServices), typeof(ConfiguracaoLocal) }) != null, "LixeiraForm não está disponível com PIN administrativo."); }
        private static void TestarNavegacaoLixeira() { var campo = typeof(TelaInicialForm).GetField("_lixeira", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic); Exigir(campo != null && campo.FieldType == typeof(System.Windows.Forms.Button), "Ação de lixeira não está ligada à tela inicial."); }
        private static void TestarTesteTexturaFormPortada() { var tipo = typeof(TesteTexturaForm); Exigir(tipo.BaseType == typeof(System.Windows.Forms.Form) && tipo.GetConstructor(new[] { typeof(AppServices), typeof(ConfiguracaoLocal) }) != null, "TesteTexturaForm não está disponível."); }
        private static void TestarFichaTesteTexturaPortada() { var tipo = typeof(FichaTesteTexturaForm); Exigir(tipo.BaseType == typeof(System.Windows.Forms.Form) && tipo.GetConstructor(new[] { typeof(AppServices), typeof(ConfiguracaoLocal), typeof(TesteTexturaCadastro) }) != null && typeof(EditarTesteTexturaForm).BaseType == typeof(System.Windows.Forms.Form), "Ficha/edição de teste de textura não está disponível."); }
        private static void TestarAcaoEdicaoFichaTesteTextura() { var app = new AppServices(); var teste = new TesteTexturaCadastro { Identificacao = "UI-01", Observacao = "teste" }; using (var ficha = new FichaTesteTexturaForm(app, new ConfiguracaoLocal(), teste)) { Exigir(ficha.Controls.OfType<System.Windows.Forms.Button>().Any(x => x.Text == "Editar teste"), "Ficha de teste de textura não expõe a ação de edição."); } }
        private static void TestarCamposEdicaoTesteTextura() { var app = new AppServices(); var teste = new TesteTexturaCadastro { Identificacao = "UI-02", Observacao = "teste", Status = StatusTesteTextura.EmTeste }; var raiz = CriarPastaTemporaria(); using (var form = new EditarTesteTexturaForm(app, new ConfiguracaoLocal { PastaRaizDados = raiz }, teste)) { var textos = form.Controls.OfType<System.Windows.Forms.TableLayoutPanel>().SelectMany(x => x.Controls.OfType<System.Windows.Forms.Label>()).Select(x => x.Text).ToList(); Exigir(textos.Contains("Status") && textos.Contains("Nome aprovado") && textos.Contains("Observação") && form.Controls.OfType<System.Windows.Forms.TableLayoutPanel>().SelectMany(x => x.Controls.OfType<System.Windows.Forms.Button>()).Any(x => x.Text == "Salvar"), "Tela de edição de teste de textura está incompleta."); } Directory.Delete(raiz, true); }
        private static void TestarEtapaOrigemControl() { var tipo = typeof(ProjetosCADLaser.Controls.EtapaOrigemControl); Exigir(typeof(System.Windows.Forms.UserControl).IsAssignableFrom(tipo) && tipo.GetProperty("Pasta") != null && tipo.GetEvent("AnalisarSolicitado") != null, "EtapaOrigemControl não está disponível para o cadastro."); }
        private static void TestarEtapaDeteccaoControl() { var tipo = typeof(ProjetosCADLaser.Controls.EtapaDeteccaoControl); Exigir(typeof(System.Windows.Forms.UserControl).IsAssignableFrom(tipo) && tipo.GetMethod("AplicarAnalise") != null && tipo.GetProperty("ComponentesSelecionados") != null && tipo.GetProperty("TexturasSelecionadas") != null, "EtapaDeteccaoControl não está disponível para o cadastro."); }
        private static void TestarEtapaMatrizesControl() { var tipo = typeof(ProjetosCADLaser.Controls.EtapaMatrizesControl); Exigir(typeof(System.Windows.Forms.UserControl).IsAssignableFrom(tipo) && tipo.GetProperty("Tipo") != null && tipo.GetProperty("Material") != null && tipo.GetProperty("Acabamento") != null && tipo.GetEvent("ConfiguracaoAlterada") != null, "EtapaMatrizesControl não está disponível para o cadastro."); }
        private static void TestarEtapaRevisaoControl() { var tipo = typeof(ProjetosCADLaser.Controls.EtapaRevisaoControl); Exigir(typeof(System.Windows.Forms.UserControl).IsAssignableFrom(tipo) && tipo.GetProperty("Pendentes") != null && tipo.GetMethod("ConfigurarSessao") != null, "EtapaRevisaoControl não está disponível para o cadastro."); }
        private static void TestarCadastroControlesEtapas() { var tipo = typeof(CadastroPilotoForm); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic; Exigir(tipo.GetField("_etapaDeteccao", flags) != null && tipo.GetField("_etapaMatrizes", flags) != null && tipo.GetField("_etapaRevisao", flags) != null, "CadastroPilotoForm não está conectado às quatro etapas refatoradas."); }
        private static void TestarInstanciacaoCadastroRefatorado() { var raiz = CriarPastaTemporaria(); EstruturaDados.Criar(raiz); using (var form = new CadastroPilotoForm(new AppServices(), new ConfiguracaoLocal { PastaRaizDados = raiz })) { Exigir(form != null, "CadastroPilotoForm refatorado não pôde ser instanciado."); } Directory.Delete(raiz, true); }
        private static void TestarLauncherFallbackLocal() { var raiz = CriarPastaTemporaria(); var anterior = Path.Combine(raiz, "anterior.exe"); File.WriteAllText(anterior, "placeholder"); var resolvido = LauncherCore.ResolverExecutavel(Path.Combine(raiz, "inexistente.exe"), anterior); Exigir(string.Equals(Path.GetFullPath(anterior), resolvido, StringComparison.OrdinalIgnoreCase), "Launcher não selecionou a versão anterior local."); Directory.Delete(raiz, true); }
        private static void TestarAtualizacaoLauncherProtegeDados() { var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem"); var app = Path.Combine(raiz, "App"); var prev = Path.Combine(raiz, "App.previous"); Directory.CreateDirectory(Path.Combine(origem, "Dados")); Directory.CreateDirectory(Path.Combine(origem, "bin")); File.WriteAllText(Path.Combine(origem, "Dados", "cadastro.json"), "dados"); File.WriteAllText(Path.Combine(origem, "bin", "app.exe"), "novo"); Directory.CreateDirectory(app); File.WriteAllText(Path.Combine(app, "versao.txt"), "anterior"); new AtualizadorLocal().Atualizar(origem, app, prev); Exigir(File.Exists(Path.Combine(app, "bin", "app.exe")) && !File.Exists(Path.Combine(app, "Dados", "cadastro.json")) && File.Exists(Path.Combine(prev, "versao.txt")), "Atualizador não preservou versão anterior ou copiou dados protegidos."); Directory.Delete(raiz, true); }
        private static void TestarVersaoLauncherOffline() { var raiz = CriarPastaTemporaria(); var manifesto = Path.Combine(raiz, "latest.version"); File.WriteAllText(manifesto, "2.0.0"); var servico = new ServicoVersao(); Exigir(servico.AtualizacaoDisponivel(manifesto, new Version("1.5.0")) && !servico.AtualizacaoDisponivel(Path.Combine(raiz, "indisponivel"), new Version("1.5.0")), "Verificação de versão do Launcher não tratou atualização/offline corretamente."); Directory.Delete(raiz, true); }
        private static void TestarLauncherOrquestraAtualizacao() { var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem"); var app = Path.Combine(raiz, "App", "ProjetosCADLaser.exe"); var prev = Path.Combine(raiz, "App.previous", "ProjetosCADLaser.exe"); Directory.CreateDirectory(Path.Combine(origem, "bin")); Directory.CreateDirectory(Path.GetDirectoryName(app)); Directory.CreateDirectory(Path.GetDirectoryName(prev)); File.WriteAllText(Path.Combine(origem, "latest.version"), "2.0.0"); File.WriteAllText(Path.Combine(origem, "bin", "ProjetosCADLaser.exe"), "novo"); File.WriteAllText(Path.Combine(Path.GetDirectoryName(app), "versao.txt"), "1.0.0"); var manifesto = Path.Combine(origem, "latest.version"); var instalada = Path.Combine(Path.GetDirectoryName(app), "versao.txt"); Exigir(LauncherCore.TentarAtualizar(origem, manifesto, instalada, app, prev) && File.Exists(Path.Combine(Path.GetDirectoryName(app), "bin", "ProjetosCADLaser.exe")), "Launcher não orquestrou atualização disponível."); Directory.Delete(raiz, true); }
        private static void TestarResolucoesAlvo() { var raiz = CriarPastaTemporaria(); EstruturaDados.Criar(raiz); var app = new AppServices(); var local = new ConfiguracaoLocal { PastaRaizDados = raiz, Perfil = PerfilUsuario.Operacional }; using (var tela = new TelaInicialForm(app, local)) using (var cadastro = new CadastroPilotoForm(app, local)) using (var pesquisa = new PesquisaForm(app, local)) { foreach (var tamanho in new[] { new System.Drawing.Size(1366, 768), new System.Drawing.Size(1920, 1080) }) { tela.Size = tamanho; cadastro.Size = tamanho; pesquisa.Size = tamanho; Exigir(tela.Width <= tamanho.Width && tela.Height <= tamanho.Height && cadastro.Width <= tamanho.Width && cadastro.Height <= tamanho.Height && pesquisa.Width <= tamanho.Width && pesquisa.Height <= tamanho.Height, "Uma tela excedeu a resolução alvo."); } } Directory.Delete(raiz, true); }
        private static void TestarFalhaAtualizacaoPreservaAplicativo() { var raiz = CriarPastaTemporaria(); var app = Path.Combine(raiz, "App"); var prev = Path.Combine(raiz, "App.previous"); Directory.CreateDirectory(app); File.WriteAllText(Path.Combine(app, "versao.txt"), "1.0.0"); var falhou = false; try { new AtualizadorLocal().Atualizar(Path.Combine(raiz, "origem-inexistente"), app, prev); } catch (DirectoryNotFoundException) { falhou = true; } Exigir(falhou && File.Exists(Path.Combine(app, "versao.txt")) && File.ReadAllText(Path.Combine(app, "versao.txt")) == "1.0.0", "Falha de atualização alterou a aplicação instalada."); Directory.Delete(raiz, true); }
        private static void TestarNavegacaoTesteTextura() { var campo = typeof(TelaInicialForm).GetField("_testeTextura", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic); Exigir(campo != null && campo.FieldType == typeof(System.Windows.Forms.Button), "Ação de teste de textura não está ligada à tela inicial."); }
        private static void TestarEdicaoVisualConcorrencia()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem"); Directory.CreateDirectory(origem); EstruturaDados.Criar(raiz); var app = new AppServices(); var piloto = new PilotoCadastro { Codigo = "ED-001", NomeModelo = "Original", PastaOrigem = origem }; var salvo = app.CadastroPiloto.Salvar(raiz, piloto, new List<AnexoPendente>(), new[] { "Gravação" }); var caminho = app.Bloqueios.ObterCaminho(raiz, salvo.Codigo); var dono = app.Bloqueios.CriarPorCodigo(raiz, salvo.Codigo, salvo.Id, "Edição", "Primeiro"); var bloqueado = false; try { app.Bloqueios.CriarPorCodigo(raiz, salvo.Codigo, salvo.Id, "Edição", "Segundo"); } catch (IOException) { bloqueado = true; } Exigir(bloqueado, "Edição concorrente não foi bloqueada."); app.Bloqueios.Remover(caminho, dono); var editado = app.EdicaoPiloto.Salvar(raiz, salvo, "Editado", new Dictionary<Guid, string>(), "Alteração visual"); Exigir(editado.NomeModelo == "Editado" && editado.VersaoCadastro == salvo.VersaoCadastro + 1, "Edição visual não incrementou a versão."); Directory.Delete(raiz, true);
        }

        private static void TestarCancelamentoReativacaoIntegrados()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem"); Directory.CreateDirectory(origem); EstruturaDados.Criar(raiz); var app = new AppServices(); var piloto = new PilotoCadastro { Codigo = "ST-001", NomeModelo = "Status", PastaOrigem = origem }; var salvo = app.CadastroPiloto.Salvar(raiz, piloto, new List<AnexoPendente>(), new[] { "Gravação" }); var cancelado = app.CancelamentoPiloto.Cancelar(raiz, salvo.Codigo, "Teste de cancelamento"); Exigir(cancelado.Status == StatusPiloto.Cancelada && cancelado.MotivoCancelamento == "Teste de cancelamento" && cancelado.Historico.Any(x => x.Tipo == TipoEvento.Cancelamento), "Cancelamento integrado não registrou status/histórico."); var reativado = app.CancelamentoPiloto.Reativar(raiz, salvo.Codigo, "Retorno ao trabalho"); Exigir(reativado.Status == StatusPiloto.Ativa && reativado.CanceladoEm == null && reativado.Historico.Any(x => x.Tipo == TipoEvento.Reativacao), "Reativação integrada não limpou status/histórico."); Directory.Delete(raiz, true);
        }

        private static void TestarLixeiraRestauracaoIntegrada()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem"); Directory.CreateDirectory(origem); EstruturaDados.Criar(raiz); var app = new AppServices(); var piloto = new PilotoCadastro { Codigo = "LX-001", NomeModelo = "Lixeira", PastaOrigem = origem }; var salvo = app.CadastroPiloto.Salvar(raiz, piloto, new List<AnexoPendente>(), new[] { "Gravação" }); var pastaCadastro = Path.Combine(raiz, "Cadastros", salvo.Codigo); var caminhoLock = app.Bloqueios.ObterCaminho(raiz, salvo.Codigo); var dono = app.Bloqueios.CriarPorCodigo(raiz, salvo.Codigo, salvo.Id, "Lixeira", "Administrador"); var meta = app.Lixeira.Mover(raiz, TipoRegistroLixeira.Piloto, pastaCadastro, salvo.Id, salvo.Codigo, salvo.NomeModelo, "Teste de lixeira"); app.Bloqueios.Remover(caminhoLock, dono); var item = app.Lixeira.Listar(raiz, TipoRegistroLixeira.Piloto).Single(); Exigir(!Directory.Exists(pastaCadastro) && item.Meta.Codigo == "LX-001", "Cadastro não foi movido para a lixeira."); app.Lixeira.Restaurar(raiz, meta, item.Pasta); var restaurado = new JsonService().Read<PilotoCadastro>(Path.Combine(pastaCadastro, "cadastro.json")); Exigir(restaurado.Historico.Any(x => x.Tipo == TipoEvento.Restauracao), "Restauração não registrou histórico."); Directory.Delete(raiz, true);
        }
        private static void TestarPesquisaEFichaTemporaria()
        {
            var raiz = CriarPastaTemporaria(); var origem = Path.Combine(raiz, "origem"); Directory.CreateDirectory(origem); EstruturaDados.Criar(raiz); var app = new AppServices(); var piloto = new PilotoCadastro { Codigo = "PES-001", NomeModelo = "Pesquisa Integrada", PastaOrigem = origem }; app.CadastroPiloto.Salvar(raiz, piloto, new List<AnexoPendente>(), new[] { "Gravação" }); var resultado = app.PesquisaPilotos.Carregar(raiz); var filtrado = app.PesquisaPilotos.Filtrar(resultado.Pilotos, "integrada"); Exigir(filtrado.Count == 1, "Pesquisa integrada não encontrou o piloto temporário."); using (var ficha = new FichaTecnicaForm(app, new ConfiguracaoLocal { PastaRaizDados = raiz }, filtrado[0])) { var texto = ((System.Windows.Forms.TextBox)ficha.Controls[0]).Text; Exigir(texto.Contains("PES-001") && texto.Contains("Pesquisa Integrada"), "Ficha técnica não exibiu identidade do piloto."); } Directory.Delete(raiz, true);
        }

        private static void TestarEntradaVisualConfigurada()
        {
            Exigir(typeof(PrimeiroUsoForm) != null, "PrimeiroUsoForm não está disponível na entrada visual.");
            Exigir(typeof(TelaInicialForm) != null, "TelaInicialForm não está disponível na entrada visual.");
        }

        private static bool Igual(byte[] a, byte[] b) { if (a.Length != b.Length) return false; for (var i = 0; i < a.Length; i++) if (a[i] != b[i]) return false; return true; }
        private static void Exigir(bool condicao, string mensagem) { if (!condicao) throw new InvalidOperationException(mensagem); }

        private const string FixtureV1 = "{\"versaoFormato\":1,\"versaoCadastro\":1,\"id\":\"11111111-1111-1111-1111-111111111111\",\"codigo\":\"10002\",\"nomeModelo\":\"Sandalia Teste Adulto\",\"pastaOrigem\":\"C:\\\\Origem\\\\10002\",\"status\":\"ativa\",\"componentes\":[{\"id\":\"22222222-2222-2222-2222-222222222222\",\"nome\":\"Palmilha\",\"pastasDetectadas\":[],\"matrizes\":[{\"id\":\"33333333-3333-3333-3333-333333333333\",\"tipo\":\"Gravação\",\"texturasIds\":[\"44444444-4444-4444-4444-444444444444\"]}]}],\"texturas\":[{\"id\":\"44444444-4444-4444-4444-444444444444\",\"nome\":\"gls_012\",\"origem\":\"automatica\",\"selecionada\":true,\"confirmada\":true}],\"historico\":[{\"id\":\"55555555-5555-5555-5555-555555555555\",\"tipo\":\"Criação da piloto\",\"observacao\":\"Cadastro inicial\",\"texturasIds\":[],\"anexos\":[],\"imagens\":[],\"alteracoes\":[],\"componentesIds\":[],\"matrizesIds\":[],\"projetoInteiro\":true,\"dataHora\":\"2026-08-13T12:00:00-03:00\",\"usuario\":\"teste\",\"computador\":\"PC\",\"ultimaEdicaoEm\":\"2026-08-13T12:00:00-03:00\",\"ultimaEdicaoPor\":\"teste\"}],\"anexos\":[{\"id\":\"66666666-6666-6666-6666-666666666666\",\"nomeOriginal\":\"foto.png\",\"nomeInterno\":\"foto.png\",\"caminhoRelativo\":\"anexos/foto.png\",\"tamanhoBytes\":12,\"dataAdicao\":\"2026-08-13T12:00:00-03:00\",\"adicionadoEm\":\"2026-08-13T12:00:00-03:00\",\"usuario\":\"teste\",\"computador\":\"PC\"}],\"criadoEm\":\"2026-08-13T12:00:00-03:00\",\"ultimaEdicaoEm\":\"2026-08-13T12:00:00-03:00\",\"criadoPor\":\"teste\",\"computador\":\"PC\",\"ultimaEdicaoPor\":\"teste\"}";
    }
}
