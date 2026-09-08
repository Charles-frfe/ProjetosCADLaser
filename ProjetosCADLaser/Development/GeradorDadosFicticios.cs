#if DEBUG
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Repositories;
using ProjetosCADLaser.Services;

namespace ProjetosCADLaser.Development
{
    internal static class GeradorDadosFicticios
    {
        private const string TextoLongo =
            "DADO FICTÍCIO, SEM VALIDADE PARA PRODUÇÃO. Conferir o alinhamento visual, " +
            "a continuidade da textura entre as regiões e a legibilidade das informações. " +
            "Esta observação extensa serve para testar rolagem e quebra de linha na interface. ";

        internal static IReadOnlyList<string> Executar(ConfiguracaoLocal local, AppServices servicos)
        {
            var resultados = new List<string>();
            try
            {
                ValidarAmbiente(local);
                // Serializa as instâncias do gerador nesta sessão do Windows.
                using (var mutex = new Mutex(false, @"Local\ProjetosCADLaser_DEV_Gerador"))
                {
                    var adquirido = false;
                    try
                    {
                        try { adquirido = mutex.WaitOne(0); }
                        catch (AbandonedMutexException) { adquirido = true; }
                        if (!adquirido)
                        {
                            resultados.Add("Ignorado: outra instância do gerador está em execução.");
                            return resultados;
                        }

                        var temporarios = CaminhoSeguro(local.PastaRaizDados, "Desenvolvimento", "Temporarios");
                        var anexos = new AnexoService(temporarios);
                        var cadastroService = new CadastroPilotoService(
                            servicos.AcessoPasta, new PilotoRepository(servicos.Json), anexos, servicos.Matrizes);

                        ExecutarCenario(resultados, "90001", () => CriarCadastro(
                            local, servicos, cadastroService, anexos, temporarios,
                            "90001", "MODELO TESTE ALPHA", 2, 2, 3, 1, 1));
                        ExecutarCenario(resultados, "90002", () => CriarCadastro(
                            local, servicos, cadastroService, anexos, temporarios,
                            "90002", "MODELO TESTE BETA", 4, 3, 8, 3, 3));
                        ExecutarCenario(resultados, "90003", () => CriarOrigemProblematica(local, servicos));
                        ExecutarCenario(resultados, "99999", () => CriarCadastro(
                            local, servicos, cadastroService, anexos, temporarios,
                            "99999", "TESTE INTERFACE COMPLETA", 6, 8, 24, 8, 6));
                    }
                    finally { if (adquirido) mutex.ReleaseMutex(); }
                }
            }
            catch (Exception ex)
            {
                // Uma falha do gerador não deve impedir a abertura da aplicação.
                resultados.Add("Falha no gerador DEBUG: " + ex.Message);
            }
            return resultados;
        }

        private static void ExecutarCenario(List<string> resultados, string codigo, Func<string> criar)
        {
            try { resultados.Add(codigo + ": " + criar()); }
            catch (Exception ex) { resultados.Add("Falha no cenário " + codigo + ": " + ex.Message); }
        }

        internal static void ValidarAmbiente(ConfiguracaoLocal local)
        {
            if (local == null) throw new ArgumentNullException("local");
            var dev = NormalizarLocal(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProjetosCADLaser_DEV"));
            // Valida ambos os textos antes de qualquer consulta ao sistema de arquivos.
            var dados = NormalizarLocal(local.PastaRaizDados);
            var origem = NormalizarLocal(local.PastaOrigemProjetos);
            if (!string.Equals(dados, Path.Combine(dev, "Dados"), StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(origem, Path.Combine(dev, "OrigemPilotos"), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("O gerador exige exclusivamente as pastas Dados e OrigemPilotos do DEV.");
            ValidarSemRedirecionamento(dados);
            ValidarSemRedirecionamento(origem);
        }

        private static string NormalizarLocal(string caminho)
        {
            if (string.IsNullOrWhiteSpace(caminho) || caminho.Length < 3 ||
                !char.IsLetter(caminho[0]) || caminho[1] != ':' ||
                (caminho[2] != '\\' && caminho[2] != '/') || caminho.IndexOf(':', 2) >= 0)
                throw new InvalidOperationException("Caminho inválido: UNC e caminhos de dispositivo não são permitidos.");
            return Path.GetFullPath(caminho).TrimEnd('\\', '/');
        }

        private static void ValidarSemRedirecionamento(string caminho)
        {
            // Examina os ancestrais primeiro, sem atravessar junções ou links.
            var ancestrais = new Stack<string>();
            for (var atual = caminho; !string.IsNullOrEmpty(atual); atual = Path.GetDirectoryName(atual))
                ancestrais.Push(atual);
            while (ancestrais.Count > 0)
            {
                var atual = ancestrais.Pop();
                try
                {
                    if ((File.GetAttributes(atual) & FileAttributes.ReparsePoint) != 0)
                        throw new InvalidOperationException("Redirecionamento de pasta não permitido: " + atual);
                }
                catch (FileNotFoundException) { }
                catch (DirectoryNotFoundException) { }
            }
        }

        private static string CaminhoSeguro(string raiz, params string[] partes)
        {
            var baseCompleta = NormalizarLocal(raiz);
            var destino = NormalizarLocal(Path.Combine(new[] { baseCompleta }.Concat(partes).ToArray()));
            if (!destino.StartsWith(baseCompleta + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("O destino está fora da raiz DEV.");
            ValidarSemRedirecionamento(destino);
            return destino;
        }

        private static bool Existe(string caminho)
        {
            return Directory.Exists(caminho) || File.Exists(caminho);
        }

        private static ConfiguracaoCompartilhada LerDefinicoes(ConfiguracaoLocal local, AppServices servicos)
        {
            CaminhoSeguro(local.PastaRaizDados, "Configuracoes", "configuracao.json");
            return servicos.ConfiguracaoCompartilhada.Carregar(local.PastaRaizDados);
        }

        private static string CriarCadastro(
            ConfiguracaoLocal local, AppServices servicos, CadastroPilotoService cadastroService,
            AnexoService anexos, string temporarios, string codigo, string nome,
            int quantidadeComponentes, int texturasPorComponente, int quantidadeObservacoes,
            int quantidadeArquivos, int quantidadeImagens)
        {
            ValidarAmbiente(local);
            var destino = CaminhoSeguro(local.PastaRaizDados, "Cadastros", codigo);
            // Antes de criar origem, temporários ou anexos, preserva o cenário inteiro.
            if (cadastroService.CodigoExiste(local.PastaRaizDados, codigo) || File.Exists(destino))
                return "ignorado; destino de cadastro já existente.";
            var origem = CaminhoSeguro(local.PastaOrigemProjetos, codigo + "_" + nome.Replace(' ', '_'));
            if (Existe(origem)) return "ignorado; origem já existente, preservada sem alterações.";

            var definicoes = LerDefinicoes(local, servicos);
            var tipos = definicoes.TiposMatriz.Where(t => t.Ativo).Select(t => t.Nome).ToArray();
            Directory.CreateDirectory(origem);
            var cadastro = new PilotoCadastro { Codigo = codigo, NomeModelo = nome, PastaOrigem = origem };
            var nomes = new[] { "Palmilha", "Forquilha", "Gáspea", "Sola", "Cabedal", "Tira" };
            for (var i = 0; i < quantidadeComponentes; i++)
            {
                var pastaComponente = CaminhoSeguro(origem, nomes[i]);
                Directory.CreateDirectory(pastaComponente);
                var componente = new ComponenteCadastro
                {
                    Nome = nomes[i] + (codigo == "99999" ? " — região de teste com descrição extensa para avaliação da interface" : "")
                };
                componente.PastasDetectadas.Add(pastaComponente);
                for (var t = 0; t < texturasPorComponente; t++)
                {
                    var nomeTextura = (t % 2 == 0 ? "gl_" : "gls_") + (i * texturasPorComponente + t + 1).ToString("000");
                    var pastaTextura = CaminhoSeguro(pastaComponente, nomeTextura);
                    Directory.CreateDirectory(pastaTextura);
                    var textura = new TexturaCadastro
                    {
                        Nome = nomeTextura + (codigo == "99999" ? " — padrão fictício detalhado para testar quebra de linha" : ""),
                        CaminhoDetectado = pastaTextura,
                        CaminhoRelativo = nomes[i] + Path.DirectorySeparatorChar + nomeTextura,
                        Observacao = "Textura fictícia de desenvolvimento.",
                        Origem = OrigemClassificacao.Automatica
                    };
                    componente.Texturas.Add(textura);
                    cadastro.Texturas.Add(textura);
                }

                if (codigo == "90002" && i == 0)
                    componente.Matrizes.AddRange(servicos.Matrizes.CriarGrupoLaterais(CriarMatriz(i, 0, componente)));
                else
                {
                    componente.Matrizes.Add(CriarMatriz(i, 0, componente));
                    if (codigo != "90001" || i == 0) componente.Matrizes.Add(CriarMatriz(i, 1, componente));
                    if (codigo == "99999")
                        componente.Matrizes.AddRange(servicos.Matrizes.CriarGrupoLaterais(CriarMatriz(i, 2, componente)));
                }
                cadastro.Componentes.Add(componente);
            }

            var erros = cadastroService.Validar(cadastro, tipos);
            if (erros.Count > 0) throw new InvalidDataException(string.Join(Environment.NewLine, erros));
            var observacoes = CriarObservacoes(cadastro, quantidadeObservacoes);
            CaminhoSeguro(local.PastaRaizDados, "Desenvolvimento", "Temporarios");
            var sessao = anexos.CriarSessao();
            var pendentes = CriarAnexos(anexos, sessao, codigo, quantidadeArquivos, quantidadeImagens);
            // Um arquivo e uma imagem pertencem a uma observação vinculada ao componente.
            var arquivoObservacao = pendentes.First(a => !a.Imagem);
            var imagemObservacao = pendentes.First(a => a.Imagem);
            observacoes[1].Anexos.Add(arquivoObservacao);
            observacoes[1].Anexos.Add(imagemObservacao);
            pendentes.Remove(arquivoObservacao);
            pendentes.Remove(imagemObservacao);

            ValidarAmbiente(local);
            CaminhoSeguro(local.PastaRaizDados, "Cadastros", codigo);
            CaminhoSeguro(temporarios, Path.GetFileName(sessao));
            if (cadastroService.CodigoExiste(local.PastaRaizDados, codigo) || File.Exists(destino))
                return "ignorado; cadastro criado por outra execução, preservado.";
            cadastroService.Salvar(local.PastaRaizDados, cadastro, pendentes, tipos,
                observacoesPendentes: observacoes);
            return "criado com " + cadastro.Componentes.Count + " componentes, " +
                cadastro.Componentes.Sum(c => c.Matrizes.Count) + " matrizes e " + cadastro.Texturas.Count + " texturas.";
        }

        private static MatrizCadastro CriarMatriz(int componente, int indice, ComponenteCadastro cadastro)
        {
            return new MatrizCadastro
            {
                Tipo = indice == 1 ? "Tampa" : "Gravação",
                NomeExibicao = "Matriz " + (indice + 1) + " — referência fictícia de " + cadastro.Nome,
                Material = (MaterialMatriz)((componente + indice) % 3),
                Eixos = indice % 2 == 0 ? QuantidadeEixos.Tres : QuantidadeEixos.Cinco,
                Maquina = (Maquina)((componente + indice) % 3),
                Acabamento = indice % 2 == 0 ? Acabamento.Fosco : Acabamento.Polido,
                TexturasIds = cadastro.Texturas.Select(t => t.Id).ToList()
            };
        }

        private static List<ObservacaoPendente> CriarObservacoes(PilotoCadastro cadastro, int quantidade)
        {
            var resultado = new List<ObservacaoPendente>();
            for (var i = 0; i < quantidade; i++)
            {
                var componente = cadastro.Componentes[(i / 3) % cadastro.Componentes.Count];
                var observacao = new ObservacaoPendente
                {
                    Texto = "Observação fictícia " + (i + 1) + " de " + cadastro.NomeModelo + ". " +
                        (i % 2 == 0 ? "Conferir acabamento e continuidade da textura." :
                        string.Concat(Enumerable.Repeat(TextoLongo + Environment.NewLine, cadastro.Codigo == "99999" ? 5 : 1)))
                };
                if (i % 3 != 0) observacao.ComponenteId = componente.Id;
                if (i % 3 == 2) observacao.MatrizId = componente.Matrizes[(i / 3) % componente.Matrizes.Count].Id;
                resultado.Add(observacao);
            }
            return resultado;
        }

        private static List<AnexoPendente> CriarAnexos(
            AnexoService anexos, string sessao, string codigo, int arquivos, int imagens)
        {
            var resultado = new List<AnexoPendente>();
            for (var i = 0; i < arquivos; i++)
            {
                var fonte = CaminhoSeguro(sessao, "referencia_" + (i + 1) + ".txt");
                using (var stream = new FileStream(fonte, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream))
                    writer.WriteLine("Cenário fictício " + codigo + Environment.NewLine + TextoLongo);
                var pendente = anexos.AdicionarArquivo(sessao, fonte, false);
                pendente.Titulo = "Referência técnica fictícia " + (i + 1) +
                    (codigo == "99999" ? " — descrição extensa para avaliar a apresentação dos anexos na interface" : "");
                pendente.Descricao = TextoLongo;
                pendente.Categoria = "Desenvolvimento";
                resultado.Add(pendente);
            }
            for (var i = 0; i < imagens; i++)
            {
                var largura = i % 3 == 1 ? 360 : 640;
                var altura = i % 3 == 0 ? 360 : 640;
                using (var bitmap = new Bitmap(largura, altura))
                using (var desenho = Graphics.FromImage(bitmap))
                using (var fonte = new Font(FontFamily.GenericSansSerif, 16))
                {
                    desenho.Clear(i % 2 == 0 ? Color.LightSteelBlue : Color.Beige);
                    desenho.FillEllipse(Brushes.CadetBlue, 35, 100, largura - 70, altura - 140);
                    desenho.DrawString("DADO FICTÍCIO DEV\n" + codigo + " / imagem " + (i + 1),
                        fonte, Brushes.Black, new RectangleF(15, 15, largura - 30, 90));
                    var pendente = anexos.AdicionarImagem(sessao, bitmap);
                    pendente.Titulo = "Imagem fictícia " + codigo + " número " + (i + 1);
                    pendente.Descricao = TextoLongo;
                    pendente.Categoria = "Desenvolvimento";
                    resultado.Add(pendente);
                }
            }
            return resultado;
        }

        private static string CriarOrigemProblematica(ConfiguracaoLocal local, AppServices servicos)
        {
            ValidarAmbiente(local);
            var origem = CaminhoSeguro(local.PastaOrigemProjetos, "90003_MODELO_PROBLEMA");
            if (Existe(origem)) return "ignorado; origem já existente, preservada sem alterações.";
            var definicoes = LerDefinicoes(local, servicos);
            var partes = new[]
            {
                @"Palmilha\gl_001", @"Palmilha\gls_002", @"Componente_Experimental_DEV\gl_003",
                "Piloto", @"Escala\35-36", "Documentos_Sem_Classificacao"
            };
            foreach (var parte in partes) Directory.CreateDirectory(CaminhoSeguro(origem, parte));
            var analise = new AnalisadorPastasService().Analisar(origem, definicoes.Componentes);
            if (!analise.Componentes.Any(c => c.Nome == "Palmilha") ||
                analise.Texturas.Count < 3 ||
                !analise.NaoClassificadas.Any(p => Path.GetFileName(p) == "Componente_Experimental_DEV") ||
                !analise.NaoClassificadas.Any(p => Path.GetFileName(p) == "Documentos_Sem_Classificacao"))
                throw new InvalidDataException("A origem foi preservada, mas as definições DEV não produziram as classificações esperadas.");
            return "origem problemática criada e analisada; nenhum cadastro foi salvo.";
        }
    }
}
#endif
