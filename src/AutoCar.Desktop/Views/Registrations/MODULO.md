# Módulo Cadastros (Registrations)

## Propósito

Cadastros base do AutoCar (Cliente, e futuramente Fornecedor, Produto, etc.). Primeiro módulo
da Fase 2. Estabelece o **padrão de cadastro** do projeto: listagem (Grid único) + formulário
denso de dois modos, reaproveitado pelos próximos cadastros.

## Cliente

### Tabela `cliente` (mestre)

- `id_cliente` (uuid PK), `cod_cliente` (int identity)
- `sts_tipo_pessoa` (int — enum TipoPessoa: Fisica=1, Juridica=2)
- `documento` (único, só dígitos — CPF 11 ou CNPJ 14), `razao_social`, `nome_fantasia`
- `telefone`, `email`
- Endereço (owned type, colunas na própria tabela): `cep`, `logradouro`, `numero`,
  `complemento`, `bairro`, `cidade`, `uf`
- `observacao`, `vlr_limite_credito` (decimal 10,2), `flg_ativo`
- `dat_criacao`, `dat_atualizacao` (UTC), `xmin` (concorrência)
- Índices: `ix_cliente_documento` (único), `ix_cliente_cod` (único), `ix_cliente_razao_social`
- Migration: `CadastroCliente`.

### Camadas

- **Domain:** `Cliente` (entidade rica), `TipoPessoa` (enum), `Documento` (VO com validação de
  dígito verificador CPF/CNPJ), `Endereco` (VO owned), `IClienteRepository`, `IConsultaCnpj`.
- **Application:** `IClienteService`/`ClienteService` (CRUD com `Result<T>`), DTOs
  (`SalvarClienteDto`, `ClienteDto`, `ClienteListaDto`), `SalvarClienteValidator` (FluentValidation).
- **Infrastructure:** `ClienteConfiguration` (Fluent + owned Endereco + xmin), `ClienteRepository`,
  `ConsultaCnpjBrasilApi` (consulta CNPJ na BrasilAPI via HttpClient).
- **Desktop:** `ClientesViewModel` (listagem), `ClienteFormViewModel` (form dois modos),
  `ClientesView` + `ClienteFormView`. Navegação via `INavegador` (rota "clientes").

### Telas

- **ClientesView** — listagem: busca automática (debounce 350ms), contador, Grid único via
  code-behind (zebra, badge PF/PJ, badge status, máscara de documento na exibição), botão Novo.
  Duplo-clique numa linha abre o formulário em visualização.
- **ClienteFormView** — formulário denso (label à esquerda, vários campos por linha), dois modos
  (visualização/edição), botão de consulta de CNPJ (lupa, só PJ em edição).

### Regras de Negócio

- Cliente só existe com `Documento` válido (dígito verificador conferido no VO).
- Documento único (validado no service antes de salvar).
- Mensagem de validação amigável; documento normalizado (só dígitos), email lowercase.
- Inativar em vez de excluir (`flg_ativo`).
- **Consulta de CNPJ** (BrasilAPI): preenche razão social, fantasia, telefone e endereço a partir
  do CNPJ. Só PJ (CPF não é consultável por LGPD). Exige internet; degrada com mensagem se offline.
  Não sobrescreve campos que a Receita não retornou.

### Decisões Técnicas

- **CRUD via serviço** (`IClienteService`), não MediatR — cadastro é linear, segue o padrão do Security.
- **`Documento` como UMA coluna** (não cpf+cnpj separados) — `TipoPessoa` diz o que é. Simplifica unicidade.
- **Endereço como owned type** — um endereço por cliente no MVP; vira tabela 1:N só se precisar.
- **Validação em 2 níveis** — VO no Domain (invariante) + validator na Application (UI + unicidade).
- **Consulta CNPJ via JsonDocument tolerante a tipo** — a BrasilAPI mistura string/número nos campos.
  Gotcha: a CDN exige header `User-Agent`, senão bloqueia o request.

## Dependências

- Depende de: `Security` (usuário logado para rastreabilidade futura). Shared (`Result<T>`).
- Será consumido por: Vendas/Balcão, OS, Contas a Receber (cliente é base dessas operações).
