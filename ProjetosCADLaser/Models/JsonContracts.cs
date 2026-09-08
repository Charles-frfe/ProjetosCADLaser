using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjetosCADLaser.Models
{
    public static class JsonContratosV1
    {
        public const int VersaoFormatoSuportada = 1;

        public static JsonSerializerOptions CriarOpcoes()
        {
            var opcoes = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            opcoes.Converters.Add(new EnumContratoConverterFactory());
            return opcoes;
        }

        public static void ValidarPodeEditar(int versaoFormato)
        {
            if (versaoFormato > VersaoFormatoSuportada)
                throw new InvalidOperationException("O cadastro usa uma versão de formato mais nova e não pode ser editado por esta versão.");
        }

        public static void ValidarPodeEditar(PilotoCadastro cadastro) { if (cadastro == null) throw new ArgumentNullException("cadastro"); ValidarPodeEditar(cadastro.VersaoFormato); }
        public static void ValidarPodeEditar(TesteTexturaCadastro teste) { if (teste == null) throw new ArgumentNullException("teste"); ValidarPodeEditar(teste.VersaoFormato); }
        public static void ValidarPodeEditar(ConfiguracaoLocal configuracao) { if (configuracao == null) throw new ArgumentNullException("configuracao"); ValidarPodeEditar(configuracao.VersaoFormato); }
        public static void ValidarPodeEditar(ConfiguracaoCompartilhada configuracao) { if (configuracao == null) throw new ArgumentNullException("configuracao"); ValidarPodeEditar(configuracao.VersaoEstruturaDados); }
    }

    internal sealed class EnumContratoConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) { return typeToConvert.IsEnum; }
        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
        {
            var converterType = typeof(EnumContratoConverter<>).MakeGenericType(type);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }
    }

    internal sealed class EnumContratoConverter<T> : JsonConverter<T> where T : struct
    {
        private static readonly Dictionary<T, string> Escrita = CriarEscrita();
        private static readonly Dictionary<string, T> Leitura = CriarLeitura();

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var texto = reader.GetString();
                T valor;
                if (texto != null && Leitura.TryGetValue(texto, out valor)) return valor;
                if (texto != null && Enum.TryParse<T>(texto, true, out valor)) return valor;
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                int numero;
                if (reader.TryGetInt32(out numero)) return (T)Enum.ToObject(typeof(T), numero);
            }
            throw new JsonException("Valor de enum inválido para " + typeof(T).Name + ".");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            string texto;
            if (!Escrita.TryGetValue(value, out texto)) texto = JsonNamingPolicy.CamelCase.ConvertName(value.ToString());
            writer.WriteStringValue(texto);
        }

        private static Dictionary<T, string> CriarEscrita()
        {
            var resultado = new Dictionary<T, string>();
            foreach (var valor in (T[])Enum.GetValues(typeof(T))) resultado[valor] = NomeJson(valor);
            return resultado;
        }

        private static Dictionary<string, T> CriarLeitura()
        {
            var resultado = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            foreach (var valor in (T[])Enum.GetValues(typeof(T))) resultado[NomeJson(valor)] = valor;
            return resultado;
        }

        private static string NomeJson(T valor)
        {
            var tipo = typeof(T);
            var nome = valor.ToString();
            if (tipo == typeof(StatusTesteTextura) && nome == "EmTeste") return "Em teste";
            if (tipo == typeof(TipoEvento))
            {
                var nomes = new Dictionary<string, string> { { "CriacaoPiloto", "Criação da piloto" }, { "ReformaPiloto", "Reforma da piloto" }, { "RetoquePiloto", "Retoque da piloto" }, { "ReformaEscala", "Reforma da escala" }, { "RetoqueEscala", "Retoque da escala" }, { "EdicaoCadastro", "Edição de cadastro" }, { "ObservacaoGeral", "Observação geral" }, { "CadastroContinuado", "Cadastro continuado" }, { "ComponenteAdicionado", "Componente adicionado" }, { "MatrizAdicionada", "Matriz adicionada" }, { "TexturaAdicionada", "Textura adicionada" }, { "AnexoAdicionado", "Anexo adicionado ao cadastro" }, { "AlteracaoEscala", "Alteração da escala" }, { "Reativacao", "Projeto reativado" }, { "Restauracao", "Projeto restaurado" } };
                string mapeado; if (nomes.TryGetValue(nome, out mapeado)) return mapeado;
            }
            if (tipo == typeof(MaterialMatriz) && nome == "Aco") return "Aço";
            if (tipo == typeof(MaterialMatriz) && nome == "Aluminio") return "Alumínio";
            if (tipo == typeof(QuantidadeEixos) && nome == "Tres") return "3";
            if (tipo == typeof(QuantidadeEixos) && nome == "Cinco") return "5";
            if (tipo == typeof(Maquina)) { if (nome == "M1000") return "1000"; if (nome == "M1200P") return "1200P"; if (nome == "M1200S") return "1200S"; }
            if (tipo == typeof(PreferenciaTema) && nome == "SeguirWindows") return "Seguir o Windows";
            if (tipo == typeof(ClassificacaoPasta) && nome == "NumeroEscala") return "Número de escala";
            if (tipo == typeof(ClassificacaoPasta) && nome == "NaoClassificado") return "Não classificado";
            return JsonNamingPolicy.CamelCase.ConvertName(nome);
        }
    }
}
