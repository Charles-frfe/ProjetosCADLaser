using System;
using System.Collections.Generic;
using System.Linq;
using ProjetosCADLaser.Models;

namespace ProjetosCADLaser.Services
{
    public sealed class AdministracaoService
    {
        private readonly PinService _pin; public AdministracaoService(PinService pin) { _pin = pin; }
        public bool ValidarPin(string valor, ConfiguracaoCompartilhada configuracao) { return configuracao.PinAdministrativo != null && _pin.Verificar(valor, configuracao.PinAdministrativo); }
        public void AlterarPin(string atual, string novo, string confirmacao, ConfiguracaoCompartilhada configuracao) { if (!ValidarPin(atual, configuracao)) throw new UnauthorizedAccessException("PIN atual incorreto."); if (novo != confirmacao) throw new ArgumentException("A confirmação do novo PIN é diferente."); configuracao.PinAdministrativo = _pin.Criar(novo); }
        public DefinicaoComponente AdicionarComponente(ConfiguracaoCompartilhada configuracao, string nome, IEnumerable<string> apelidos = null) { var reservados = configuracao.Componentes.SelectMany(x => x.Apelidos.Concat(new[] { x.Nome })); ValidarNomeUnico(nome, reservados, "componente"); var lista = ValidarApelidos(apelidos ?? new string[0], reservados.Concat(new[] { nome })); var item = new DefinicaoComponente { Nome = nome.Trim(), Apelidos = lista }; configuracao.Componentes.Add(item); return item; }
        public void EditarComponente(ConfiguracaoCompartilhada configuracao, Guid id, string nome, IEnumerable<string> apelidos) { var item = configuracao.Componentes.Single(x => x.Id == id); var reservados = configuracao.Componentes.Where(x => x.Id != id).SelectMany(x => x.Apelidos.Concat(new[] { x.Nome })).ToList(); ValidarNomeUnico(nome, reservados, "componente"); item.Nome = nome.Trim(); item.Apelidos = ValidarApelidos(apelidos, reservados.Concat(new[] { nome })); }
        public ItemConfiguravel AdicionarTipoMatriz(ConfiguracaoCompartilhada configuracao, string nome) { ValidarNomeUnico(nome, configuracao.TiposMatriz.Select(x => x.Nome), "tipo de matriz"); var item = new ItemConfiguravel { Nome = nome.Trim() }; configuracao.TiposMatriz.Add(item); return item; }
        public void EditarTipoMatriz(ConfiguracaoCompartilhada configuracao, Guid id, string nome) { var item = configuracao.TiposMatriz.Single(x => x.Id == id); ValidarNomeUnico(nome, configuracao.TiposMatriz.Where(x => x.Id != id).Select(x => x.Nome), "tipo de matriz"); item.Nome = nome.Trim(); }
        public static void AlternarAtivo(DefinicaoComponente item) { item.Ativo = !item.Ativo; } public static void AlternarAtivo(ItemConfiguravel item) { item.Ativo = !item.Ativo; }
        private static void ValidarNomeUnico(string nome, IEnumerable<string> existentes, string tipo) { var normalizado = NormalizadorPesquisa.Normalizar(nome); if (string.IsNullOrWhiteSpace(normalizado)) throw new ArgumentException("Informe o nome do " + tipo + "."); if (existentes.Any(x => NormalizadorPesquisa.Normalizar(x) == normalizado)) throw new ArgumentException("Já existe um " + tipo + " com esse nome."); }
        private static List<string> ValidarApelidos(IEnumerable<string> apelidos, IEnumerable<string> reservados) { var resultado = new List<string>(); var normalizados = new HashSet<string>(reservados.Select(NormalizadorPesquisa.Normalizar), StringComparer.Ordinal); foreach (var apelido in apelidos.Select(x => x.Trim()).Where(x => x.Length > 0)) { if (!normalizados.Add(NormalizadorPesquisa.Normalizar(apelido))) throw new ArgumentException("O apelido está vazio, duplicado ou já é usado por outro componente."); resultado.Add(apelido); } return resultado; }
    }
}
