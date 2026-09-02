using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjetosCADLaser.Models;
using ProjetosCADLaser.Repositories;

namespace ProjetosCADLaser.Services
{
    public sealed class ContinuacaoCadastroService
    {
        private readonly PilotoRepository _repositorio; private readonly AnexoService _anexos; private readonly MatrizService _matrizes;
        public ContinuacaoCadastroService(PilotoRepository repositorio, AnexoService anexos, MatrizService matrizes) { _repositorio = repositorio; _anexos = anexos; _matrizes = matrizes; }
        public PilotoCadastro Continuar(string raizDados, PilotoCadastro baseCarregada, IReadOnlyCollection<ComponenteCadastro> componentes, IReadOnlyCollection<(Guid ComponenteId, MatrizCadastro Matriz)> matrizesNovas, IReadOnlyCollection<TexturaCadastro> texturas, IReadOnlyCollection<AnexoPendente> anexosPendentes, IEnumerable<string> tiposAtivos)
        {
            var atual = _repositorio.Carregar(raizDados, baseCarregada.Codigo); if (atual.VersaoCadastro != baseCarregada.VersaoCadastro) throw new InvalidOperationException("O cadastro foi alterado por outra pessoa. Reabra o projeto."); var nomes = new HashSet<string>(atual.Componentes.Select(x => NormalizadorPesquisa.Normalizar(x.Nome)));
            foreach (var componente in componentes) { var nome = NormalizadorPesquisa.Normalizar(componente.Nome); if (string.IsNullOrWhiteSpace(nome)) throw new InvalidDataException("Informe o nome do componente."); if (!nomes.Add(nome)) throw new InvalidDataException("O componente " + componente.Nome + " já está cadastrado."); }
            var destinos = atual.Componentes.Concat(componentes).ToDictionary(x => x.Id);
            foreach (var item in matrizesNovas) { ComponenteCadastro componente; if (!destinos.TryGetValue(item.ComponenteId, out componente)) throw new InvalidDataException("O componente selecionado não existe."); var erros = _matrizes.Validar(item.Matriz, tiposAtivos); if (erros.Count > 0) throw new InvalidDataException(string.Join(Environment.NewLine, erros)); if (item.Matriz.Tipo.Equals("Laterais", StringComparison.OrdinalIgnoreCase) && (_matrizes.PossuiGrupoLaterais(componente.Matrizes) || matrizesNovas.Where(x => x.ComponenteId == item.ComponenteId && x.Matriz.Id != item.Matriz.Id).Any(x => x.Matriz.Tipo.Equals("Laterais", StringComparison.OrdinalIgnoreCase) && x.Matriz.GrupoMatrizId != item.Matriz.GrupoMatrizId))) throw new InvalidDataException("O grupo Laterais já está cadastrado neste componente."); if (componente.Matrizes.Any(x => MesmaMatriz(x, item.Matriz))) throw new InvalidDataException("A matriz já está cadastrada em " + componente.Nome + "."); }
            var nomesTexturas = new HashSet<string>(atual.Texturas.Select(x => NormalizadorPesquisa.Normalizar(x.Nome))); foreach (var textura in texturas) if (!nomesTexturas.Add(NormalizadorPesquisa.Normalizar(textura.Nome))) throw new InvalidDataException("A textura " + textura.Nome + " já está cadastrada."); atual.Componentes.AddRange(componentes); foreach (var item in matrizesNovas) destinos[item.ComponenteId].Matrizes.Add(item.Matriz); atual.Texturas.AddRange(texturas);
            var evento = new EventoCadastro { Tipo = TipoDoEvento(componentes.Count, matrizesNovas.Count, texturas.Count, anexosPendentes.Count), Observacao = Resumo(componentes, matrizesNovas.Select(x => x.Matriz), texturas, anexosPendentes) }; var pastaCadastro = Path.Combine(raizDados, "Cadastros", atual.Codigo);
            foreach (var pendente in anexosPendentes) { var anexo = _anexos.CopiarParaCadastro(pendente, pastaCadastro, evento.Id); atual.Anexos.Add(anexo); if (pendente.Imagem) evento.Imagens.Add(anexo); else evento.Anexos.Add(anexo); } atual.Historico.Add(evento); atual.VersaoCadastro++; atual.UltimaEdicaoEm = DateTimeOffset.Now; atual.UltimaEdicaoPor = Environment.UserName; _repositorio.Salvar(raizDados, atual); return _repositorio.Carregar(raizDados, atual.Codigo);
        }
        public static bool MesmaMatriz(MatrizCadastro a, MatrizCadastro b) { return NormalizadorPesquisa.Normalizar(a.Tipo) == NormalizadorPesquisa.Normalizar(b.Tipo) && NormalizadorPesquisa.Normalizar(a.NomeExibicao ?? string.Empty) == NormalizadorPesquisa.Normalizar(b.NomeExibicao ?? string.Empty) && a.Material == b.Material && a.Eixos == b.Eixos && a.Maquina == b.Maquina && a.Acabamento == b.Acabamento && a.TexturasIds.OrderBy(x => x).SequenceEqual(b.TexturasIds.OrderBy(x => x)); }
        private static TipoEvento TipoDoEvento(int componentes, int matrizes, int texturas, int anexos) { var total = componentes + matrizes + texturas + anexos; if (total != 1) return TipoEvento.CadastroContinuado; if (componentes == 1) return TipoEvento.ComponenteAdicionado; if (matrizes == 1) return TipoEvento.MatrizAdicionada; if (texturas == 1) return TipoEvento.TexturaAdicionada; return TipoEvento.AnexoAdicionado; }
        private static string Resumo(IEnumerable<ComponenteCadastro> componentes, IEnumerable<MatrizCadastro> matrizes, IEnumerable<TexturaCadastro> texturas, IEnumerable<AnexoPendente> anexos) { var itens = componentes.Select(x => "Componente " + x.Nome).Concat(matrizes.Select(x => "Matriz " + (x.NomeExibicao ?? x.Tipo))).Concat(texturas.Select(x => "Textura " + x.Nome)).Concat(anexos.Select(x => "Anexo " + (x.Titulo ?? x.NomeOriginal))).ToArray(); return itens.Length == 0 ? "Cadastro continuado sem novos itens." : "Adicionado:\n- " + string.Join(";\n- ", itens) + "."; }
    }
}
