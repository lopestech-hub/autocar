# Módulo Cadastros (Registrations)

## Propósito

Cadastros base do AutoCar (Cliente, Fornecedor, e futuramente Produto, etc.). Primeiro módulo
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

## Fornecedor

### Tabela `fornecedor` (mestre)

- `id_fornecedor` (uuid PK), `cod_fornecedor` (int identity)
- `sts_tipo_pessoa` (int — enum TipoPessoa: Fisica=1, Juridica=2)
- `documento` (único, só dígitos — CPF 11 ou CNPJ 14), `razao_social`, `nome_fantasia`
- `telefone`, `email`
- Endereço (owned type): `cep`, `logradouro`, `numero`, `complemento`, `bairro`, `cidade`, `uf`
- `inscricao_estadual` (varchar 20), `contato` (varchar 100 — vendedor/representante)
- `observacao`, `flg_ativo`, `dat_criacao`, `dat_atualizacao` (UTC), `xmin` (concorrência)
- Índices: `ix_fornecedor_documento` (único), `ix_fornecedor_cod` (único), `ix_fornecedor_razao_social`
- Migration: `CadastroFornecedor`.

### Camadas / Telas

Espelham o Cliente (mesma estrutura de Domain/Application/Infrastructure/Desktop, Grid único na
listagem, form denso de dois modos, consulta CNPJ via BrasilAPI). Rota `"fornecedor"`.

### Diferenças em relação ao Cliente

- **Sem limite de crédito** (não faz sentido para quem a loja compra).
- **Inscrição Estadual** + **Contato** (vendedor) no lugar do limite de crédito.
- Form abre como **PJ** por padrão (fornecedor é tipicamente jurídico), enquanto Cliente abre como PF.
- Reusa integralmente os VOs `Documento` e `Endereco` e o serviço `IConsultaCnpj`.

## Marca e Categoria de Produto

Cadastros mestre **auxiliares do Produto** (FK futura). Enxutos: só descrição + ativo.

### Tabelas `marca` e `categoria_produto`

- `id_marca`/`id_categoria` (uuid PK), `cod_marca`/`cod_categoria` (int identity)
- `descricao` (varchar 80), `flg_ativo`, `dat_criacao`, `dat_atualizacao` (UTC), `xmin`
- Índices únicos: `ix_<t>_cod`, `ix_<t>_descricao`. Migration: `CadastroMarcaCategoria`.

### Camadas / Telas

- **Domain:** `Marca`, `CategoriaProduto` (descrição normalizada em CAIXA ALTA via `ToUpperInvariant`).
- **Application:** módulo `Produtos` — services/DTOs/validators dos dois (CRUD com `Result<T>`).
- **Infrastructure:** configurations + repositories. Unicidade **case-insensitive** via `EF.Functions.ILike`
  no repositório ("Bosch" colide com "BOSCH").
- **Desktop:** listagem Grid único (CÓDIGO · DESCRIÇÃO · STATUS) + form denso de um campo. Menu de texto
  (Cadastros), perfil Vendedor. Rotas `marcas` e `categorias`.

### Regras

- Descrição única (case-insensitive) e obrigatória; salva em CAIXA ALTA.
- Inativar em vez de excluir (`flg_ativo`).

## Produto

Cadastro mestre central do catálogo. Consome Marca, Categoria e Fornecedor via FK. **Saldo de
estoque NÃO mora aqui** — fica no módulo de Estoque (Fase 3).

### Tabela `produto` (mestre)

- `id_produto` (uuid PK), `cod_produto` (int identity)
- `cod_barras` (varchar 20, opcional — **único quando informado**, índice único parcial
  `WHERE cod_barras IS NOT NULL`)
- `descricao` (varchar 120), `descricao_complementar` (varchar 160), `cod_fabricante` (varchar 40)
- `sts_unidade` (int — enum `UnidadeMedida`: UN, PC, CX, JG, PAR, KIT, L, KG, M)
- `vlr_custo`, `vlr_venda` (decimal 10,2)
- `id_categoria` (FK **obrigatória** → `categoria_produto`, `Restrict`)
- `id_marca`, `id_fornecedor` (FKs **opcionais** → `marca`/`fornecedor`, `Restrict`)
- `flg_ativo`, `dat_criacao`, `dat_atualizacao` (UTC), `xmin` (concorrência)
- Índices: `ix_produto_cod` (único), `ix_produto_cod_barras` (único parcial), `ix_produto_descricao`,
  + os de FK. Migration: `CadastroProduto`.

### Camadas / Telas

- **Domain:** `Produto` (entidade rica, descrição/complementar em CAIXA ALTA; códigos só `Trim`),
  enum `UnidadeMedida`, `IProdutoRepository`.
- **Application:** módulo `Produtos` — `ProdutoService` (CRUD com `Result<T>` + métodos
  `ListarCategorias/Marcas/Fornecedores` para os combos), DTOs (`SalvarProdutoDto`, `ProdutoDto`,
  `ProdutoListaDto`, `OpcaoDto`), `SalvarProdutoValidator`.
- **Infrastructure:** `ProdutoConfiguration` (FKs + índice único parcial + xmin), `ProdutoRepository`
  (`Include` das navegações; filtro `ILike` por descrição/cod_barras/cod_fabricante).
- **Desktop:** `ProdutosViewModel` (listagem) + `ProdutoFormViewModel` (form em blocos de seção,
  combos de FK selecionados por Id, margem % calculada). `ProdutosView` (Grid único:
  CÓDIGO · DESCRIÇÃO · CATEGORIA · MARCA · UN · VENDA · STATUS) + `ProdutoFormView`. Rota `produtos`.

### Regras de Negócio

- **Categoria obrigatória** (validada no service via `ObterPorIdAsync`); Marca e Fornecedor opcionais.
- **cod_barras único quando informado** — checado no service (`ExisteCodBarrasAsync`) + índice parcial.
- Descrição obrigatória; valores não-negativos; unidade do enum.
- **Margem %** = `(venda - custo) / custo` exibida na tela (somente leitura; "—" sem custo).
- Inativar em vez de excluir (`flg_ativo`).

### Decisões Técnicas do Produto

- **Combos de FK selecionam por Id na coleção** (`FirstOrDefault(x => x.Id == ...)`) — Avalonia faz
  matching por referência; a navegação do EF é instância diferente da do combo. Marca/Fornecedor têm
  item nulo ("—") para o opcional. Ver [[combobox-avalonia-selecao-id]] (memória global).
- **Listagem sem paginação** (dívida) — alinhado ao padrão dos demais cadastros; revisar quando o
  volume de produtos crescer.

## Decisões Técnicas (UI do módulo)

Padrões de UI estabelecidos nesta fase, reaproveitáveis pelos próximos cadastros (ver também a skill
global `/design-engineer-desktop`):

- **CAIXA ALTA** em nome/razão social/fantasia/endereço/contato: normalização no domínio
  (`ToUpperInvariant`) + `MaiusculoBehavior` (AttachedProperty) na digitação. Exceto e-mail/observação/documento.
- **Valores monetários** usam `TextBox` com parse no ViewModel (`LimiteCreditoTexto`), não `NumericUpDown`
  (o ButtonSpinner do Fluent estiliza mal).
- **Larguras:** campos curtos (valor, IE, número) com `Width` fixo + `Left`; nomes esticam. Rótulo sempre
  na coluna de label do Grid.
- **FluentTheme:** cores de ComboBox/Menu corrigidas via resource-keys no `Tema.axaml` (não `/template/`
  em `:pointerover`, que causa flicker). Altura de campo 24px exige `MinHeight=24`.
- **Seções de formulário:** título em **azul primário** (`TextBlock.formsecao`) + **linha divisória**
  (`Border.formsecaoLinha`) que preenche a largura ao lado. Dois jeitos de montar conforme o layout:
  `DockPanel` (Grid único — Cliente/Fornecedor) ou `Grid Auto,*` (blocos — Produto). Detalhes e a
  variante "formulário em blocos de seção" no `system.md` da Luna.

## Dependências

- Depende de: `Security` (usuário logado para rastreabilidade futura). Shared (`Result<T>`).
- Será consumido por: Vendas/Balcão, OS, Contas a Receber (cliente é base dessas operações);
  Estoque/Compras (entrada de NF) e Contas a Pagar (fornecedor é base dessas operações).
- **Marca e Categoria** serão consumidos pelo **Produto** (FK `id_marca`, `id_categoria`).
