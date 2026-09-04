using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Repositories;

namespace ProjetosCADLaser.Services
{
    public sealed class EdicaoAnexoCadastro { public string Titulo { get; private set; } public string Categoria { get; private set; } public string Descricao { get; private set; } public bool Remover { get; private set; } public EdicaoAnexoCadastro(string titulo, string categoria, string descricao, bool remover) { Titulo = titulo; Categoria = categoria; Descricao = descricao; Remover = remover; } }
    public sealed class CadastroPilotoService
    {
        private readonly AcessoPastaService _acesso; private readonly PilotoRepository _repositorio; private readonly AnexoService _anexos; private readonly MatrizService _matrizes;
        public CadastroPilotoService(AcessoPastaService acesso, PilotoRepository repositorio, AnexoService anexos, MatrizService matrizes) { _acesso = acesso; _repositorio = repositorio; _anexos = anexos; _matrizes = matrizes; }
        public bool CodigoExiste(string raizDados, string codigo) { return Directory.Exists(Path.Combine(raizDados, "Cadastros", ValidarCodigo(codigo))); }
        public IReadOnlyList<string> Validar(PilotoCadastro cadastro, IEnumerable<string> tiposAtivos)
        {
            var erros = new List<string>(); try { ValidarCodigo(cadastro.Codigo); } catch (ArgumentException ex) { erros.Add(ex.Message); } if (string.IsNullOrWhiteSpace(cadastro.NomeModelo)) erros.Add("Informe o nome do modelo."); if (!Directory.Exists(cadastro.PastaOrigem)) erros.Add("A pasta de origem não foi encontrada."); var componentes = new HashSet<string>(StringComparer.Ordinal);
            foreach (var componente in cadastro.Componentes) { if (!componentes.Add(NormalizadorPesquisa.Normalizar(componente.Nome))) erros.Add("O componente " + componente.Nome + " está duplicado."); foreach (var matriz in componente.Matrizes) erros.AddRange(_matrizes.Validar(matriz, tiposAtivos).Select(x => componente.Nome + ": " + x)); } return erros;
        }
        public PilotoCadastro Salvar(string raizDados, PilotoCadastro cadastro, IReadOnlyCollection<AnexoPendente> anexosPendentes, IEnumerable<string> tiposAtivos,
            IReadOnlyCollection<ObservacaoPendente>observacaoPendentes=null)
        {
            var acesso = _acesso.Testar(raizDados); if (!acesso.Sucesso) throw new IOException(acesso.Mensagem); var erros = Validar(cadastro, tiposAtivos); if (erros.Count > 0) throw new InvalidDataException(string.Join(Environment.NewLine, erros)); if (CodigoExiste(raizDados, cadastro.Codigo)) throw new InvalidOperationException("Já existe um cadastro com esse código.");
            var pastaCadastro = Path.Combine(raizDados, "Cadastros", ValidarCodigo(cadastro.Codigo)); var criada = false;
            try
            {
                Directory.CreateDirectory(pastaCadastro); criada = true; foreach (var pasta in new[] { "imagens", "arquivos", "backups", "bloqueio" }) Directory.CreateDirectory(Path.Combine(pastaCadastro, pasta)); cadastro.Id = cadastro.Id == Guid.Empty ? Guid.NewGuid() : cadastro.Id; cadastro.Status = StatusPiloto.Ativa; cadastro.CriadoEm = cadastro.UltimaEdicaoEm = DateTimeOffset.Now; cadastro.CriadoPor = cadastro.UltimaEdicaoPor = Environment.UserName; cadastro.Computador = Environment.MachineName;
                var evento = new EventoCadastro { Tipo = TipoEvento.CriacaoPiloto, Observacao = "Cadastro inicial da piloto.", EstadoInicialResumo = cadastro.Componentes.Count + " componente(s), " + cadastro.Componentes.Sum(x => x.Matrizes.Count) + " matriz(es) e " + cadastro.Texturas.Count + " textura(s)." };
                foreach (var pendente in anexosPendentes) { var anexo = _anexos.CopiarParaCadastro(pendente, pastaCadastro, evento.Id); cadastro.Anexos.Add(anexo); if (pendente.Imagem) evento.Imagens.Add(anexo); else evento.Anexos.Add(anexo); }
                cadastro.Historico.Add(evento);
                if (observacoesPendentes != null)
                {
                    foreach (var observacao in
                        observacoesPendentes)
                    {
                        if (string.IsNullOrWhiteSpace(
                            observacao.Texto))
                            continue;

                        var eventoObservacao =
                            new EventoCadastro
                            {
                                Tipo =
                                    TipoEvento.ObservacaoGeral,

                                Observacao =
                                    observacao.Texto,

                                ComponenteId =
                                    observacao.ComponenteId,

                                MatrizId =
                                    observacao.MatrizId
                            };

                        foreach (var pendente in
                            observacao.Anexos)
                        {
                            var anexo =
                                _anexos.CopiarParaCadastro(
                                    pendente,
                                    pastaCadastro,
                                    eventoObservacao.Id);

                            cadastro.Anexos.Add(anexo);

                            if (pendente.Imagem)
                            {
                                eventoObservacao.Imagens.Add(
                                    anexo);
                            }
                            else
                            {
                                eventoObservacao.Anexos.Add(
                                    anexo);
                            }
                        }

                        cadastro.Historico.Add(
                            eventoObservacao);
                    }
                }
                _repositorio.Salvar(raizDados, cadastro);


                var relido = _repositorio.Carregar(raizDados, cadastro.Codigo); if (relido.Id != cadastro.Id || relido.Codigo != cadastro.Codigo) throw new InvalidDataException("O cadastro salvo não passou pela validação final."); return relido;
            }
            catch { if (criada && Directory.Exists(pastaCadastro)) Directory.Delete(pastaCadastro, true); throw; }
        }
        public PilotoCadastro Editar(string raizDados, PilotoCadastro baseCarregada, string nomeModelo, string observacao, IReadOnlyCollection<Guid> componentesRemover, IReadOnlyCollection<Guid> matrizesRemover, IReadOnlyDictionary<Guid, EdicaoAnexoCadastro> edicoesAnexos)
        {
            if (string.IsNullOrWhiteSpace(nomeModelo)) throw new InvalidDataException("Informe o nome do modelo."); if (string.IsNullOrWhiteSpace(observacao)) throw new InvalidDataException("Informe a observação da edição."); var atual = _repositorio.Carregar(raizDados, baseCarregada.Codigo); if (atual.Id != baseCarregada.Id || atual.VersaoCadastro != baseCarregada.VersaoCadastro) throw new InvalidOperationException("O cadastro foi alterado por outra pessoa. Reabra a ficha."); var motivo = observacao.Trim(); var evento = new EventoCadastro { Tipo = TipoEvento.EdicaoCadastro, Observacao = motivo }; Comparar(evento.Alteracoes, "Nome do modelo", atual.NomeModelo, nomeModelo.Trim(), motivo); atual.NomeModelo = nomeModelo.Trim();
            foreach (var componente in atual.Componentes.ToArray()) { if (componentesRemover.Contains(componente.Id)) { evento.Alteracoes.Add(Alteracao("Componente", componente.Nome, "Removido", motivo)); atual.Componentes.Remove(componente); continue; } foreach (var matriz in componente.Matrizes.ToArray()) if (matrizesRemover.Contains(matriz.Id)) { evento.Alteracoes.Add(Alteracao("Matriz", matriz.NomeExibicao ?? matriz.Tipo, "Removida", motivo)); componente.Matrizes.Remove(matriz); } }
            var pastaCadastro = Path.GetDirectoryName(_repositorio.CaminhoCadastro(raizDados, atual.Codigo)); var movimentacoes = new List<Tuple<string, string>>();
            try
            {
                foreach (var anexo in atual.Anexos.ToArray()) { EdicaoAnexoCadastro edicao; if (!edicoesAnexos.TryGetValue(anexo.Id, out edicao)) continue; if (edicao.Remover) { evento.Alteracoes.Add(Alteracao("Anexo [" + anexo.NomeOriginal + "]", anexo.Titulo ?? anexo.NomeOriginal, "Removido do cadastro", motivo)); var arquivado = Copiar(anexo); ArquivarAnexoRemovido(pastaCadastro, arquivado, evento.Id, movimentacoes); if (EhImagem(anexo)) evento.Imagens.Add(arquivado); else evento.Anexos.Add(arquivado); atual.Anexos.Remove(anexo); continue; } if (string.IsNullOrWhiteSpace(edicao.Titulo)) throw new InvalidDataException("Informe o título do anexo " + anexo.NomeOriginal + "."); Comparar(evento.Alteracoes, "Título do anexo [" + anexo.NomeOriginal + "]", anexo.Titulo, edicao.Titulo.Trim(), motivo); Comparar(evento.Alteracoes, "Categoria do anexo [" + anexo.NomeOriginal + "]", anexo.Categoria, Limpar(edicao.Categoria), motivo); Comparar(evento.Alteracoes, "Descrição do anexo [" + anexo.NomeOriginal + "]", anexo.Descricao, Limpar(edicao.Descricao), motivo); anexo.Titulo = edicao.Titulo.Trim(); anexo.Categoria = Limpar(edicao.Categoria); anexo.Descricao = Limpar(edicao.Descricao); }
                if (evento.Alteracoes.Count == 0) throw new InvalidDataException("Nenhuma alteração foi informada."); atual.Historico.Add(evento); atual.VersaoCadastro++; atual.UltimaEdicaoEm = DateTimeOffset.Now; atual.UltimaEdicaoPor = Environment.UserName; _repositorio.Salvar(raizDados, atual); return _repositorio.Carregar(raizDados, atual.Codigo);
            }
            catch { foreach (var movimentacao in movimentacoes.AsEnumerable().Reverse()) try { if (File.Exists(movimentacao.Item2)) { Directory.CreateDirectory(Path.GetDirectoryName(movimentacao.Item1)); File.Move(movimentacao.Item2, movimentacao.Item1); } } catch { } throw; }
        }
        private static void ArquivarAnexoRemovido(string pastaCadastro, AnexoCadastro anexo, Guid eventoId, List<Tuple<string, string>> movimentacoes)
        {
            if (Path.IsPathRooted(anexo.CaminhoRelativo)) throw new InvalidDataException("O caminho do anexo removido é inválido."); var raiz = Path.GetFullPath(pastaCadastro).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar; var origem = Path.GetFullPath(Path.Combine(pastaCadastro, anexo.CaminhoRelativo)); if (!origem.StartsWith(raiz, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("O caminho do anexo removido está fora do cadastro."); if (!File.Exists(origem)) return; var destino = Path.Combine(pastaCadastro, "anexos-removidos", eventoId.ToString("D"), anexo.NomeInterno); Directory.CreateDirectory(Path.GetDirectoryName(destino)); File.Move(origem, destino); movimentacoes.Add(Tuple.Create(origem, destino)); anexo.CaminhoRelativo = CaminhoRelativo(pastaCadastro, destino);
        }
        private static string CaminhoRelativo(string raiz, string destino) { var basePath = Path.GetFullPath(raiz).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar; var full = Path.GetFullPath(destino); return full.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) ? full.Substring(basePath.Length) : full; }
        private static bool EhImagem(AnexoCadastro anexo) { return new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp" }.Contains(Path.GetExtension(anexo.NomeOriginal), StringComparer.OrdinalIgnoreCase); }
        private static AnexoCadastro Copiar(AnexoCadastro anexo) { return new AnexoCadastro { Id = anexo.Id, Titulo = anexo.Titulo, NomeOriginal = anexo.NomeOriginal, NomeInterno = anexo.NomeInterno, CaminhoRelativo = anexo.CaminhoRelativo, TamanhoBytes = anexo.TamanhoBytes, DataAdicao = anexo.DataAdicao, Descricao = anexo.Descricao, Categoria = anexo.Categoria, AdicionadoEm = anexo.AdicionadoEm, Usuario = anexo.Usuario, Computador = anexo.Computador }; }
        private static AlteracaoCampo Alteracao(string campo, string anterior, string novo, string observacao) { return new AlteracaoCampo { Campo = campo, ValorAnterior = anterior, ValorNovo = novo, Observacao = observacao }; }
        private static void Comparar(List<AlteracaoCampo> alteracoes, string campo, string anterior, string novo, string observacao) { if (!string.Equals(anterior, novo, StringComparison.Ordinal)) alteracoes.Add(Alteracao(campo, anterior, novo, observacao)); }
        private static string Limpar(string valor) { return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim(); }
        private static string ValidarCodigo(string codigo) { codigo = (codigo ?? string.Empty).Trim(); if (codigo.Length == 0) throw new ArgumentException("Informe o código do modelo."); if (codigo.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || codigo.IndexOf(Path.DirectorySeparatorChar) >= 0 || codigo.IndexOf(Path.AltDirectorySeparatorChar) >= 0 || codigo == "." || codigo == "..") throw new ArgumentException("O código contém caracteres inválidos."); return codigo; }
    }
}
