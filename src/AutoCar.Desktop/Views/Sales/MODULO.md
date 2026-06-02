# Módulo Vendas (Sales) — Pré-venda

## Propósito

Documentos de venda do AutoCar. Primeiro módulo da Fase 3. A **Pré-venda** é um documento
provisório de balcão (cabeçalho + itens) que estabelece o **padrão de documento** a ser reusado em
Orçamento, OS e Venda. Não baixa estoque ao salvar — o saldo só sai quando o documento é **faturado**
(vira Venda, na Fase de Estoque). Por isso a pré-venda tem ciclo de vida: Aberta → Faturada/Cancelada.

## Tabelas

### `pre_venda` (mestre — cabeçalho)

- `id_pre_venda` (uuid PK), `cod_pre_venda` (int identity — número do documento)
- `sts_situacao` (int — enum `SituacaoPreVenda`: Aberta=1, Faturada=2, Cancelada=3)
- `id_cliente` (uuid FK **opcional** → `cliente`, `Restrict`) — balcão avulso não tem
- `nome_cliente_avulso` (varchar 120) — nome digitado quando sem cliente cadastrado
- `veiculo_montadora`/`veiculo_modelo` (varchar 60), `veiculo_ano` (varchar 9), `veiculo_placa` (varchar 8)
  — **texto livre** no MVP (CAIXA ALTA; ano só `Trim`)
- `vlr_desconto`, `vlr_total` (decimal 10,2 — total persistido, calculado no domínio)
- `observacao` (varchar 255), `id_usuario` (FK → `usuario`, `Restrict` — vendedor que abriu)
- `flg_ativo`, `dat_criacao`, `dat_atualizacao` (UTC). **Sem `xmin`** (coleção filha no mesmo save).
- Índices: `ix_pre_venda_cod` (único), `ix_pre_venda_cliente`, `ix_pre_venda_situacao`, `ix_pre_venda_data`.

### `pre_venda_item` (filho — linha)

- `id_pre_venda_item` (uuid PK) — sem `cod_` (registro filho)
- `id_pre_venda` (FK → `pre_venda`, **Cascade**), `id_produto` (FK → `produto`, `Restrict`)
- `descricao_produto` (varchar 160) — **snapshot** da descrição na inclusão
- `qtd`, `vlr_unitario` (snapshot editável), `vlr_desconto`, `vlr_total_item` (decimal 10,2)
- Índice: `ix_pre_venda_item_pre_venda` (FK). Migration: `CadastroPreVenda`.

## Telas (Views) e ViewModels

- `PreVendasView` + `PreVendasViewModel` — **listagem** (Grid único code-behind): Nº · DATA · CLIENTE ·
  ITENS · TOTAL · SITUAÇÃO (badge com borda colorida). Busca por cliente/número (debounce), contador.
  "Nova" e duplo-clique **abrem janela separada** (evento `AbrirJanelaSolicitado`, não embute o form).
- `PreVendaWindow` — **janela maximizada não-modal**: o shell principal segue acessível. Hospeda o
  `PreVendaFormView`. **F2** abre o catálogo seletor (`AbrirCatalogoCommand`). Fecha ao Salvar/Cancelar.
- `PreVendaFormView` + `PreVendaFormViewModel` — form dois modos: seções DADOS (cliente opcional +
  veículo livre + vendedor read-only) · ITENS (grid editável) · TOTAIS (subtotal/desconto/TOTAL em faixa).
- `PreVendaItemViewModel` — linha do grid de itens (qtd/unitário/desconto editáveis; total recalcula).
- `CatalogoSeletorWindow` — janela do catálogo no **F2**, reusa `CatalogoView` em **modo seletor**.
- `ClienteSeletorWindow` — janela seletora de cliente no **F3** (ou clique no campo Cliente): busca por
  nome ou código, setas, Enter/duplo-clique; botão "Consumidor" = avulso. Cliente é um `Button.seletor`
  (não combo) — o AutoCompleteBox foi descartado por renderizar quebrado no Avalonia 11.

## Regras de Negócio

- **Cliente opcional** (balcão avulso): ou `id_cliente`, ou `nome_cliente_avulso`; listagem mostra
  "CONSUMIDOR" quando nenhum. Ao escolher cliente cadastrado, o nome avulso é limpo.
- **Veículo texto livre** (montadora/modelo/ano/placa), igual à aplicação por veículo do Produto.
- **Snapshot de preço**: o item copia `descricao` e `vlr_venda` do produto na inclusão; unitário é
  editável na linha (desconto manual). O documento registra o preço praticado na hora.
- **Desconto por item + desconto geral** do documento.
- **Total no domínio** (a UI nunca calcula): item = `qtd × unitário − desconto` (≥0); documento =
  `Σ itens − desconto geral` (≥0). Desconto geral ajusta para baixo se exceder o subtotal.
- **Ciclo de vida**: nasce Aberta (editável). `Faturar()` exige ≥1 item e torna imutável. `Cancelar()`
  encerra. Documento Faturado/Cancelado não aceita mais alterações (invariante no agregado).
- **Faturar baixa estoque** (✅): ao faturar, cada item gera uma **Saída** no estoque, com origem
  rastreável (`Venda nº X`). Tudo numa **transação única** (ver Decisões). Falha se algum item não
  tiver saldo (a venda continua Aberta). UI: botão verde **Faturar** (perfil só em pré-venda Aberta
  salva, em visualização) → **ConfirmacaoWindow** (ação irreversível) → fatura. Badge **FATURADA** no
  header quando imutável. `Cancelar()` existe no domínio (sem botão ainda; cancelar NÃO mexe em estoque).

## Camadas

- **Domain:** `PreVenda` (agregado raiz, backing field `_itens`), `PreVendaItem`, enums
  `SituacaoPreVenda`, `IPreVendaRepository`.
- **Application:** `Modules/Sales/PreVendas` — `IPreVendaService`/`PreVendaService` (CRUD + Faturar/
  Cancelar com `Result<T>`; converte invariante do domínio em `Error.Validacao`, `ConcorrenciaException`
  em `Error.Conflito`, `NaoEncontradoException` em `Error.NaoEncontrado`), DTOs, `SalvarPreVendaValidator`.
  `FaturarAsync(id, idUsuario)` — o usuário fatura (registrado nos movimentos de estoque).
- **Infrastructure:** `PreVendaConfiguration` + `PreVendaItemConfiguration` (Cascade, sem xmin),
  `PreVendaRepository` (`IDbContextFactory`; itens novos forçados a `State=Added`). **`FaturamentoRepository`**
  (`IFaturamentoRepository`): operação transacional cross-agregado Pré-venda + Estoque.

## Decisões Técnicas

- **Sem `xmin`** no agregado — coleção filha (itens) editada no mesmo `SaveChanges` faz o UPDATE do pai
  afetar 0 linhas. Mesmo padrão do Produto. Ver lição global de EF Core + Npgsql.
- **`IDbContextFactory` + `State=Added`** nos itens novos — PK gerada no cliente faria o EF inferir
  `Modified` → UPDATE em linha inexistente. Mesma lição da aplicação por veículo do Produto.
- **Janela separada (não embutida)** — a ViewModel dispara evento; a View abre a `PreVendaWindow`
  não-modal (`Show(dono)`). `PreVendasViewModel` recebe `Func<PreVendaFormViewModel>` (form novo por
  janela). Montada à mão no `Navegador` (depende do `UsuarioLogado`, runtime).
- **F2 = catálogo seletor** — reusa `CatalogoView` em modo seletor (ver padrão "Seletor" no system.md):
  hover ≠ seleção, clique simples marca, duplo-clique/Enter adiciona. Preço entra como snapshot.
- **Realces de cor** (ver system.md): faixa do TOTAL, badge com borda, célula âmbar de desconto, flash
  do item recém-adicionado (via `DispatcherTimer`, nunca animação).
- **Faturar = transação única atômica** (não Domain Event ainda). `FaturamentoRepository` abre UM
  `DbContext`, fatura a pré-venda e baixa o estoque de todos os itens, com **um único `SaveChanges`**
  (transação implícita do EF — tudo ou nada; saldo insuficiente lança e faz rollback). Domain Event
  `PreVendaFaturada` adiado: N=1 consumidor, e o evento in-process precisaria compartilhar a mesma
  transação de qualquer forma. Reavaliar quando surgir 2º consumidor (título a receber, comissão, NFC-e).
- **Confirmação via evento**: o VM dispara `ConfirmacaoFaturamentoSolicitada`; a janela mostra a
  `ConfirmacaoWindow` e, se confirmado, chama `ConfirmarFaturamentoAsync` — o VM não conhece a janela.
- **Saída do estoque carrega a origem** (`OrigemMovimento.Venda` + id/nº da pré-venda) — rastreável no
  histórico de estoque como "Venda nº X". Ver [Inventory](../Inventory/MODULO.md).

## Devolução de venda

Documento que devolve itens de uma **venda faturada** ao estoque (parcial por item). Não "desfatura" a
venda — é um documento à parte que aponta para ela (histórico preservado).

- **Tabelas:** `devolucao` (id, `cod_devolucao`, `id_pre_venda` Restrict, `id_usuario`, motivo, vlr_total)
  e `devolucao_item` (produto, qtd **int**, vlr_unitario snapshot, vlr_total_item; Cascade). Sem `xmin`
  (coleção filha). Migration `CadastroDevolucao` (aditiva).
- **Domain:** `Devolucao` (raiz, total no domínio, ≥1 item) + `DevolucaoItem` (imutável). `IDevolucaoRepository`.
- **Application:** `Modules/Sales/Devolucoes` — `DevolucaoService`: `ListarItensDevolviveisAsync` (mostra
  vendido/já devolvido/devolvível por produto) e `CriarAsync` (valida **saldo devolvível** = vendido − já
  devolvido; a regra é inter-agregado, por isso vive no caso de uso, não no agregado). DTOs + validator.
- **Infra:** `DevolucaoRepository` — **transação explícita com 2 SaveChanges** (o `cod_devolucao` identity só
  existe após o 1º INSERT, e os movimentos de estoque precisam dele para a origem "Devolução nº X"); cada
  item gera **ENTRADA** no estoque (`OrigemMovimento.Devolucao`). Reusa `EstoquePersistencia` + `QuantidadeEstoque`.
- **Desktop:** botão **Devolver** na Pré-venda faturada (`PodeDevolver`) → `DevolucaoWindow` (grid de itens
  com qtd a devolver editável, limitada ao devolvível) → `ConfirmacaoWindow` → registra. A venda segue Faturada.
- **Dívida (MVP):** TOCTOU — duas devoluções simultâneas da mesma venda podem ambas passar na validação de
  devolvível (sem lock). Integridade do estoque preservada (devolução só soma). Aceitável; revisar se virar problema.

## Dependências

- Depende de: Cadastros (Cliente, Produto/Catálogo), Security (usuário logado = vendedor), Shared (`Result<T>`),
  e **Estoque** (a baixa ao faturar — `IFaturamentoRepository` orquestra os dois agregados).
- Será base de: Orçamento, OS e Venda (mesmo padrão de documento).
