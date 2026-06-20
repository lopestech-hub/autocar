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

## Grupo, Posição e Lado de Peça — atributos do Produto (cadastro)

Cadastros mestre **auxiliares do Produto**, criados em 2026-06-16 (antes eram enums fixos). Todos no
**molde de Marca** (id + cod_X, descrição CAIXA ALTA única, `flg_ativo`, `xmin`; CRUD com `Result<T>`,
unicidade case-insensitive via `ILike`; listagem `ListBox.tabela` + form denso). Consumidos pelo
Produto como **FK opcional** (sem FK = "—").

### Tabelas

- **`posicao_peca`** — `id_posicao`/`cod_posicao`, `descricao` (única). Eixo da peça (Dianteira,
  Traseira, e o que mais cadastrarem). Seed base idempotente: DIANTEIRA, TRASEIRA.
- **`lado_peca`** — `id_lado`/`cod_lado`, `descricao` (única). Lado da peça (Esquerdo, Direito).
  Dimensão **independente** da posição. Seed base: ESQUERDO, DIREITO.
- **`grupo_produto`** — `id_grupo`/`cod_grupo`, `descricao`, **`id_categoria`** (FK obrigatória →
  `categoria_produto`, `Restrict`). Família dentro da categoria (AMORTECEDOR em SUSPENSÃO).
  Descrição **única DENTRO da categoria** (índice composto `id_categoria`+`descricao`).

### Camadas / Telas

- **Domain:** `PosicaoPeca`, `LadoPeca`, `GrupoProduto` (+ repositórios). `Produto.DefinirGrupo` para
  vínculo isolado. Os enums `PosicaoPeca`/`LadoPeca` foram **removidos** de `Domain/Enums`.
- **Application:** services/DTOs/validators dos três no módulo `Produtos`. `ProdutoService` ganhou
  `ListarPosicoesAsync`, `ListarLadosAsync` e `ListarGruposAsync(idCategoria)` (grupo filtrado).
- **Infrastructure:** configurations + repositories; `GrupoProdutoRepository` tem `ListarPorCategoriaAsync`
  (combo dependente). Migrations `PosicaoLadoComoCadastro` e `GrupoProduto` (aditivas).
- **Desktop:** `PosicoesView`/`LadosView`/`GruposView` (+ forms). Grupo tem **combo de categoria** no
  form; a listagem mostra a categoria. Rotas `posicoes`/`lados`/`grupos` (menu Cadastros, perfil Vendedor).

### Regras / Decisões

- **Por que cadastro e não enum:** régua do projeto — *atributo é lógica do sistema → enum; é rótulo
  descritivo que o usuário escolhe → cadastro*. Posição/Lado/Grupo são puro rótulo do domínio
  automotivo. Unidade e Combustível **continuam enum** (o código depende do valor / conjunto fechado).
- **Grupo é combo dependente** no form do Produto: ao trocar a categoria, recarrega os grupos e zera a
  seleção. O `ProdutoFormViewModel` usa flag `_carregandoProduto` para o trigger não atrapalhar a
  restauração da seleção na edição.
- Seed dos grupos demo num método **idempotente próprio** (`GarantirGruposDemo`) que roda sempre — o
  seed de produtos só roda em banco vazio, então sem isso um banco com produtos nunca ganharia grupos.

## Serviço (catálogo de mão de obra) — Fase 4

Cadastro mestre **auxiliar da Ordem de Serviço** (FK em `ordem_servico_item` quando a linha é do tipo
Serviço). Mesmo molde enxuto de Marca/Categoria, com um campo a mais: o valor padrão sugerido.

### Tabela `servico`

- `id_servico` (uuid PK), `cod_servico` (int identity)
- `descricao` (varchar 120, CAIXA ALTA, **única case-insensitive**)
- `vlr_padrao` (decimal 10,2) — valor sugerido da mão de obra (snapshot editável na OS)
- `flg_ativo`, `dat_criacao`, `dat_atualizacao` (UTC), `xmin` (concorrência — cadastro sem coleção filha)
- Índices únicos: `ix_servico_cod`, `ix_servico_descricao` (lower). Migration: `CatalogoServico`.

### Camadas / Telas

- **Domain:** `Servico` (descrição normalizada em CAIXA ALTA; `vlr_padrao` não-negativo).
- **Application:** módulo `Registrations/Servicos` — `ServicoService` (CRUD com `Result<T>` + `ListarAtivosAsync`
  para o seletor da OS), DTOs, `SalvarServicoValidator`.
- **Infrastructure:** `ServicoConfiguration` + `ServicoRepository`. Unicidade case-insensitive via `ILike`.
- **Desktop:** listagem Grid único (CÓDIGO · DESCRIÇÃO · VALOR PADRÃO · STATUS) + form denso (descrição +
  valor padrão). Menu de texto (Cadastros), perfil Vendedor. Rota `servicos`.

### Regras

- Descrição única (case-insensitive) e obrigatória; salva em CAIXA ALTA.
- `vlr_padrao` ≥ 0 (pode ser 0 — serviço de cortesia/diagnóstico).
- Inativar em vez de excluir (`flg_ativo`).
- Consumido pela **Ordem de Serviço** (linha tipo Serviço traz descrição + `vlr_padrao` como snapshot,
  ambos editáveis na linha). Ver [Service](../Service/MODULO.md).

## Mecânico (quem executa o serviço) — Fase 4

Cadastro mestre **auxiliar da Ordem de Serviço** (FK `id_mecanico` em `ordem_servico`). **Não é
usuário do sistema** — o mecânico não loga, não tem perfil nem senha; é só a identificação de quem
executou o trabalho na OS (base para produtividade/comissão futura). Molde enxuto de Marca/Categoria.

### Tabela `mecanico`

- `id_mecanico` (uuid PK), `cod_mecanico` (int identity)
- `nome` (varchar 120, CAIXA ALTA, **único case-insensitive**)
- `telefone` (varchar 20, opcional — texto livre, sem CAIXA ALTA)
- `flg_ativo`, `dat_criacao`, `dat_atualizacao` (UTC), `xmin` (concorrência — cadastro sem coleção filha)
- Índices únicos: `ix_mecanico_cod`, `ix_mecanico_nome`. Migration: `CadastroMecanico`.

### Camadas / Telas

- **Domain:** `Mecanico` (nome em CAIXA ALTA, telefone opcional).
- **Application:** módulo `Registrations/Mecanicos` — `MecanicoService` (CRUD com `Result<T>` +
  `ListarAsync` para o seletor da OS), DTOs, `SalvarMecanicoValidator`.
- **Infrastructure:** `MecanicoConfiguration` + `MecanicoRepository` (unicidade `ILike`).
- **Desktop:** listagem Grid único (CÓDIGO · NOME · TELEFONE · STATUS) + form denso (nome + telefone).
  Menu de texto (Cadastros), perfil Vendedor. Rota `mecanicos`.

### Regras

- Nome único (case-insensitive) e obrigatório; salva em CAIXA ALTA. Telefone opcional.
- Inativar em vez de excluir (`flg_ativo`).
- **Decisão de modelo:** mecânico é entidade própria, não usuário — a 4.2 inicialmente modelou como FK
  para `usuario` (`id_usuario_mecanico`); a 4.1b corrigiu para FK ao cadastro de mecânico (`id_mecanico`),
  via migration por **rename** (não-destrutiva). Consumido pela OS, ver [Service](../Service/MODULO.md).

## Produto

Cadastro mestre central do catálogo. Consome Marca, Categoria e Fornecedor via FK. **Saldo de
estoque NÃO mora aqui** — fica no módulo de Estoque (Fase 3).

### Tabela `produto` (mestre)

- `id_produto` (uuid PK), `cod_produto` (int identity)
- `cod_barras` (varchar 20, opcional — **único quando informado**, índice único parcial
  `WHERE cod_barras IS NOT NULL`)
- `descricao` (varchar 120), `descricao_complementar` (varchar 160), `cod_fabricante` (varchar 40)
- `sts_unidade` (int — enum `UnidadeMedida`: UN, PC, CX, JG, PAR, KIT, L, KG, M)
- `id_grupo` (FK **opcional** → `grupo_produto`, `Restrict`). Grupo/família dentro da categoria.
- `id_posicao` (FK **opcional** → `posicao_peca`, `Restrict`). Eixo da peça (dianteira/traseira).
- `id_lado` (FK **opcional** → `lado_peca`, `Restrict`). Lado da peça (esquerdo/direito).
  > **Histórico:** Posição e Lado eram enums (`sts_posicao`/`sts_lado`); viraram cadastro editável
  > em 2026-06-16 (migration `PosicaoLadoComoCadastro` dropa as colunas `sts_` e cria as FKs).
- `vlr_custo`, `vlr_venda` (decimal 10,2)
- `arquivo_imagem` (varchar 300, opcional) — **só o nome do arquivo** (ex: `27022.jpg`), não o caminho.
  A pasta-base é configurável por terminal (`appsettings: Imagens:PastaBase`), porque o sistema é 2-tier
  e o caminho físico varia entre as máquinas da LAN. Migration `RenomearArquivoImagem` (renomeou a
  coluna `url_imagem`, criada fora do EF, e limpou os caminhos legados para só o nome).
- `id_categoria` (FK **obrigatória** → `categoria_produto`, `Restrict`)
- `id_marca`, `id_fornecedor` (FKs **opcionais** → `marca`/`fornecedor`, `Restrict`)
  > Hierarquia do catálogo: **Categoria → Grupo → Produto**. Grupo é opcional e depende da categoria.
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
  combos de FK selecionados por Id, margem % calculada). `ProdutosView` (`ListBox.tabela`:
  CÓDIGO · DESCRIÇÃO · CATEGORIA · **GRUPO** · MARCA · UN · POSIÇÃO · LADO · VENDA · STATUS) + `ProdutoFormView`. Rota `produtos`.
  **O form abre em janela separada** (`ProdutoWindow`, não-modal, **850×550**, título fixo "Produto" —
  não duplica o nome do card) — padrão do Estoque/Pré-venda: `ProdutosViewModel` recebe
  `Func<ProdutoFormViewModel>` (form **novo por janela**, sem estado compartilhado) e dispara
  `AbrirFormularioSolicitado`; a `ProdutosView` (code-behind) abre a janela e recarrega a listagem ao
  **Salvo**. Não é mais embutido na listagem (`FormularioAtivo` removido).
  A aba **Dados** usa **group box** (molduras `HeaderedContentControl.grupo`, título laranja embutido)
  nas seções IDENTIFICAÇÃO/CLASSIFICAÇÃO/VALORES. Padrão a replicar nos demais cadastros.
  Na seção CLASSIFICAÇÃO: **grade de 6 colunas** (rótulo/campo em pares) com os rótulos Posição e Lado
  alinhados nas colunas (não flutuando colados ao campo). **Categoria | Grupo** lado a lado (grupo é
  **combo dependente** — trocar a categoria recarrega os grupos e zera a seleção); Posição e Lado são
  combos com item nulo em branco. Todos via `OpcaoDto` (Id+descrição); os converters de enum
  `PosicaoPecaConverter`/`LadoPecaConverter` foram **removidos** (a descrição vem da navegação).
  Os mini-grids de Aplicações/Equivalências usam a classe **`colheader`** (rótulo de coluna preto).

### Regras de Negócio

- **Categoria obrigatória** (validada no service via `ObterPorIdAsync`); Marca e Fornecedor opcionais.
- **cod_barras único quando informado** — checado no service (`ExisteCodBarrasAsync`) + índice parcial.
- Descrição obrigatória; valores não-negativos; unidade do enum.
- **Margem %** = `(venda - custo) / custo` exibida na tela (somente leitura; **em branco** sem custo).
- Inativar em vez de excluir (`flg_ativo`).

### Aplicação por veículo (tabela `produto_aplicacao`)

Tabela filho 1:N do Produto — quais veículos a peça atende. Texto livre no MVP (sem cadastro
normalizado de montadora/modelo). Sem `cod_` (registro filho). Migration: `AplicacaoProduto`.

- `id_aplicacao` (uuid PK), `id_produto` (FK → produto, **Cascade**)
- `montadora`, `modelo` (varchar, CAIXA ALTA, obrigatórios), `ano_inicio`, `ano_fim` (int, opcionais —
  ano_fim vazio = "em diante")
- `motorizacao` (varchar 60, opcional, CAIXA ALTA — texto livre: "1.0", "1.6 FIRE", "2.0 16V TURBO FLEX")
- `sts_combustivel` (int — enum `Combustivel`: NaoAplica=0, Flex, Gasolina, Diesel, Etanol, GNV; default 0)
- `observacao` (varchar 120, opcional)
- Índices: `ix_produto_aplicacao_veiculo` (montadora+modelo, busca), `ix_produto_aplicacao_produto` (FK)
- Migrations: `AplicacaoProduto` (inicial) + `AplicacaoMotorCombustivel` (motorização/combustível).
- **UI:** seção APLICAÇÕES no form do Produto — mini-grid editável (`AplicacaoItemViewModel`): montadora,
  modelo, ano ini/fim, **motor** (texto), **combustível** (combo via `CombustivelConverter`), observação;
  botão "+ Adicionar" e "✕" por linha; some no modo visualização. **Salva junto com o produto** (substitui
  todas a cada gravação — `Produto.DefinirAplicacoes`).

### Decisões Técnicas do Produto

- **Combos de FK selecionam por Id na coleção** (`FirstOrDefault(x => x.Id == ...)`) — Avalonia faz
  matching por referência; a navegação do EF é instância diferente da do combo. Marca/Fornecedor têm
  item nulo ("—") para o opcional. Ver [[combobox-avalonia-selecao-id]] (memória global).
- **Produto sem `xmin`** (concorrência otimista) — diferente dos outros cadastros. O `xmin` quebrava o
  save quando a coleção de aplicações mudava no mesmo `SaveChanges` (UPDATE afetava 0 linhas). Ver
  lição global de EF Core + Npgsql.
- **`ProdutoRepository` usa `IDbContextFactory`** (contexto novo por operação), não o `AppDbContext`
  injetado dos demais repos — evita estado defasado de contexto de longa duração no desktop.
- **Aplicações novas forçadas a `State = Added`** no `AtualizarAsync` — a PK gerada no cliente
  (`Guid.NewGuid`) fazia o EF inferir `Modified` → UPDATE em linha inexistente. Ver lição global.
- **Listagem virtualizada** (`ListBox.tabela`, 2026-06-14/15) — todas as listagens migraram do "Grid
  único code-behind" para `ListBox` virtualizado (só renderiza linhas visíveis). Motivo: o code-behind
  montava ~11 controles por linha de uma vez na UI thread e **travava com 256 produtos**. Padrão
  reutilizável no `Tema.axaml` (`ListBox.tabela` + classes `th`/`td`/`td.mono`/`td.suave`) — **universal
  no projeto** (todos os cadastros, movimentos e os 5 seletores F2–F5). Ver memória local
  `feedback-padrao-listagem`. A **query** ainda não pagina (`ListarAsync` sem `LIMIT`) — dívida
  separada, só relevante quando o volume crescer muito.

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
- **Seções de formulário:** título em **laranja acento** (`TextBlock.formsecao`, unificado com o
  cabeçalho do group box desde `6cd7d4f`) + **linha divisória** (`Border.formsecaoLinha`) ao lado. Dois jeitos de montar conforme o layout:
  `DockPanel` (Grid único — Cliente/Fornecedor) ou `Grid Auto,*` (blocos — Produto). Detalhes e a
  variante "formulário em blocos de seção" no `system.md` da Luna.
- **Abas (tabs) em formulários densos:** quando o form tem muitas seções ou mini-grids/listas filhas,
  organizar em **abas** em vez de empilhar tudo num scroll (empilhar corta a última seção no rodapé).
  Forms simples (poucos campos) ficam diretos, sem abas. Mecânica: VM com `AbaAtual` (string) +
  booleanos derivados (`AbaXAtiva`, com `[NotifyPropertyChangedFor]`) + comando `SelecionarAba`;
  View com barra de `Button Classes="aba" Classes.ativo="{Binding AbaXAtiva}"` (sublinhado azul na
  ativa) e painéis irmãos com `IsVisible`. Estilo `Button.aba` no `Tema.axaml` (Foreground fixado no
  `/template/ ContentPresenter` em todos os estados — gotcha FluentTheme). **Referência:** `ProdutoFormView`
  (Dados / Aplicações / Equivalências), com badge de contagem nas abas de mini-grid.
- **Botão remover de linha (mini-grids):** `Button Classes="remover"` (no `Tema.axaml`) — ícone de
  lixeira (`fa-solid fa-trash`) vermelho, sem borda/fundo, hover com fundo vermelho-claro. Usado nas
  linhas de Aplicações e Equivalências.

## Catálogo (consulta peça → veículo)

Tela de **consulta** (não cadastro) em `Views/Catalogo/` — o vendedor responde "quais peças servem
nesse carro?". Consome o módulo Produtos; não tem tabela própria.

- **Busca:** cruza `produto` × `produto_aplicacao`. Filtros opcionais (vão estreitando): montadora,
  modelo, ano (faixa `ano_inicio..ano_fim`, null = aberta) e termo da peça (descrição/cod_barras/
  cod_fabricante via `ILike`). O produto entra se tiver **ao menos uma** aplicação que case com todos
  os critérios de veículo informados.
- **Camadas:** reusa `IProdutoService`/`ProdutoRepository` — `BuscarCatalogoAsync`, `ListarMontadorasAsync`,
  `ListarModelosAsync`; DTOs `BuscaCatalogoDto`/`CatalogoItemDto` (este traz Posicao + Lado + CodFabricante).
  `CatalogoViewModel` + `CatalogoView` (Grid único:
  **CÓDIGO·DESCRIÇÃO·APLICAÇÃO·POSIÇÃO·LADO·COD.FABRIC·UN·VENDA**). Rota `catalogo`.
- **Dois modos do CatalogoView:** consulta (toolbar) e **seletor** (Pré-venda F2). Em ambos: clique marca
  a linha + régua lateral, setas navegam. Só no seletor o duplo-clique/Enter **adiciona** a peça; na
  consulta não há ação (teclado da consulta no próprio grid; no seletor, na `CatalogoSeletorWindow`).
- **Dado de teste:** seed demo no `DbInitializer` (8 produtos variados, idempotente — só roda em banco
  com ≤1 produto). Remover/gear por flag antes do deploy real.

## Dependências

- Depende de: `Security` (usuário logado para rastreabilidade futura). Shared (`Result<T>`).
- Será consumido por: Vendas/Balcão, OS, Contas a Receber (cliente é base dessas operações);
  Estoque/Compras (entrada de NF) e Contas a Pagar (fornecedor é base dessas operações).
- **Marca e Categoria** serão consumidos pelo **Produto** (FK `id_marca`, `id_categoria`).
