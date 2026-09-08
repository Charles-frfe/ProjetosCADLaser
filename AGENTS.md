# Orientações para o repositório

## Descrição funcional

O sistema registra como uma piloto de um modelo de calçado foi realizada para servir de referência durante a escala. O cadastro inclui componentes, matrizes, texturas, parâmetros técnicos, observações, possíveis problemas, anexos, imagens e histórico.

## Contexto técnico

- Projeto C# WinForms, compatível com Visual Studio 2017 e .NET Framework 4.7.2, com C# 7.3 configurado nos projetos.
- A solução contém a aplicação ProjetosCADLaser, o launcher ProjetosCADLaser.Launcher e o projeto ProjetosCADLaser.Tests.
- A aplicação está organizada em Forms, Controls, Models, Services, Repositories, Configuration e Utilities.
- A persistência principal é em JSON, utilizando System.Text.Json, JsonService, PilotoRepository e contratos versionados em JsonContratosV1.

## Regras de trabalho

- Preservar a arquitetura atual do projeto.
- Manter compatibilidade com Visual Studio 2017, .NET Framework 4.7.2 e a versão de C# já usada (7.3).
- Não migrar para SQL, banco de dados ou outra persistência sem pedido explícito.
- Preservar compatibilidade com os cadastros JSON existentes.
- Não fazer grandes refatorações sem pedido explícito.
- Alterar o menor número possível de arquivos por tarefa.
- Não remover código legado apenas por parecer não utilizado sem verificar dependências e testes.
- Nunca usar caminhos do servidor ou da produção para testes locais.
- Dados de desenvolvimento devem ficar completamente separados dos dados de produção.
- Não colocar caminhos absolutos específicos deste computador no código de produção.
- Preservar funcionalidades existentes não relacionadas à tarefa solicitada.
- Sempre informar os arquivos alterados e o motivo de cada alteração.
- Sempre que possível, compilar e testar antes de concluir.
- Não fazer commit, push, pull, merge, rebase ou alterações no Git automaticamente; deixar isso para o usuário.
- Não modificar configurações de produção sem pedido explícito.

## Fluxo de aprovação

- Antes de modificar qualquer arquivo em uma nova tarefa, apresentar:
  1. objetivo da alteração;
  2. arquivos que pretende criar ou modificar;
  3. resumo técnico da implementação;
  4. possíveis riscos ou impactos.

- Aguardar autorização explícita do usuário antes de editar qualquer arquivo.

- Após autorizado, executar somente a alteração aprovada.

- Depois da alteração:
  - informar os arquivos modificados;
  - resumir o que mudou;
  - compilar e testar quando possível;
  - informar erros ou avisos novos;
  - mostrar git status;
  - parar e aguardar nova instrução.

- Não iniciar automaticamente a próxima melhoria.
- Não aproveitar uma autorização para fazer alterações adicionais não solicitadas.
