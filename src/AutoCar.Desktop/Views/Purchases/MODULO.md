# Módulo Compras (Purchases) — Entrada por Compra

## Propósito

Documentos de compra do AutoCar — a **contrapartida da venda** que fecha o ciclo do estoque (a saída
sai pelo faturamento da Pré-venda; a entrada entra pela Compra). A **Compra** registra mercadoria
recebida de um fornecedor (cabeçalho + itens) e dá **ENTRADA** no estoque ao salvar, com origem
`Compra nº X`. Entrada por documento — antes só havia entrada manual pela tela de Movimentação de Estoque.

## Tabelas

### `compra` (mestre — cabeçalho)

- `id_compra` (uuid PK), `cod_compra` (int identity — número do documento)
- `id_fornecedor` (uuid FK **obrigatório** → `fornecedor`, `Restrict`) — sem fornecedor a compra não existe
- `id_usuario` (FK → `usuario`, `Restrict` — quem registrou)
- `num_documento` (varchar 40) — nº da nota do fornecedor, **texto livre opcional** no MVP
- `observacao` (varchar 255), `vlr_total` (decimal 10,2 — total persistido, calculado no domínio)
- `dat_criacao`, `dat_atualizacao` (UTC). **Sem `xmin`** (coleção filha no mesmo save — padrão Pré-venda).
- Índices: `ix_compra_cod` (único), `ix_compra_fornecedor`. Migration: `CadastroCompra` (aditiva).

### `compra_item` (filho — linha)

- `id_compra_item` (uuid PK) — sem `cod_` (registro filho)
- `id_compra` (FK → `compra`, **Cascade**), `id_produto` (FK → `produto`, `Restrict`)
- `descricao_produto` (varchar 160) — **snapshot** da descrição na inclusão
- `qtd` (int — **inteira**, autopeça não fraciona), `vlr_custo_unitario` (decimal 10,2 — custo pago),
  `vlr_total_item` (decimal 10,2 — qtd × custo)
- Índice: `ix_compra_item_compra` (FK).

## Telas (Views) e ViewModels

- `ComprasView` + `ComprasViewModel` — **listagem** (Grid único code-behind): Nº · DATA · FORNECEDOR ·
  ITENS · TOTAL. "Nova" e duplo-clique **abrem janela separada** (evento `AbrirJanelaSolicitado`).
  Sem busca no MVP (lista curta e cronológica).
- `CompraWindow` — **janela maximizada não-modal** que hospeda o `CompraFormView`. **F2** abre o catálogo
  seletor (`AbrirCatalogoCommand`), **F3** abre o seletor de fornecedor, **ESC** fecha.
- `CompraFormView` + `CompraFormViewModel` — form dois modos: seções DADOS (fornecedor obrigatório via
  **botão-seletor** F3 + nº doc + observação) · ITENS (grid editável: descrição/qtd/custo unit./total) ·
  TOTAIS (faixa do TOTAL). Nova abre em **edição**; compra registrada reabre em **visualização read-only**.
- `CompraItemViewModel` — linha do grid (qtd inteira + custo unitário editáveis; total recalcula; flash ao adicionar).
- `FornecedorSeletorWindow` — janela seletora no **F3** (busca por nome/código, setas, Enter/duplo-clique).
  Espelha o `ClienteSeletorWindow`, sem opção "Consumidor" (fornecedor é obrigatório).

## Regras de Negócio

- **Fornecedor obrigatório** — a compra sempre tem origem; validado no service e no form (botão Salvar).
- **Entrada imediata ao salvar** — a compra representa mercadoria que já chegou; **sem ciclo de vida**
  (Aberta/Recebida). Ao registrar, já dá entrada no estoque numa transação única e atômica.
- **Total no domínio** (a UI nunca calcula): item = `qtd × custo unitário`; documento = `Σ itens`. Sem desconto no MVP.
- **Quantidade inteira** — igual ao estoque (autopeça não fraciona).
- **Snapshot da descrição** — o item copia `produto.Descricao` na inclusão.
- **Custo unitário inicia em 0** ao adicionar a peça do catálogo — o operador digita o custo da nota do
  fornecedor (o custo do cadastro pode estar desatualizado). Decisão de produto (2026-06-02).
- **Não atualiza o `vlr_custo` do produto** no MVP — ver Evolução no CONTEXTO (custo médio ponderado).
- **Compra registrada não se edita** — reabre só em visualização (já consumou a entrada de estoque).

## Camadas

- **Domain:** `Compra` (raiz, backing field `_itens`, total no domínio, ≥1 item; navegação `Fornecedor`
  só-leitura p/ a listagem) + `CompraItem` (imutável). `ICompraRepository`.
- **Application:** `Modules/Purchases/Compras` — `ICompraService`/`CompraService` (`CriarAsync` valida
  fornecedor + produtos via `Result`; `ListarAsync`; `ObterPorIdAsync` → `CompraDetalheDto`), DTOs,
  `CriarCompraValidator`. Converte invariante do domínio em `Error.Validacao` e `ConcorrenciaException`
  em `Error.Conflito`.
- **Infrastructure:** `CompraConfiguration` + `CompraItemConfiguration` (Cascade, sem xmin, itens novos
  `State=Added`). **`CompraRepository`** (`IDbContextFactory`): `RegistrarComEntradaEstoqueAsync` é a
  operação transacional cross-agregado Compra + Estoque.
- **Desktop:** `Views/Purchases` + `ViewModels/Purchases`. Rota `compras` no menu **Movimentos** (perfil
  Vendedor); `ComprasViewModel` montado à mão no `Navegador` (depende do `UsuarioLogado`); `CompraFormViewModel` no DI.

## Decisões Técnicas

- **Espelha a Devolução** — a Compra é o caso mais próximo: documento à parte + entrada no estoque + `cod`
  identity que precede a origem do movimento. Reusa o mesmo padrão de repositório.
- **Transação explícita com 2 SaveChanges** — o `cod_compra` é identity do banco e só existe após o 1º
  INSERT; os movimentos de estoque precisam dele para a origem "Compra nº X". Salva o documento → cria os
  movimentos com o cod real → salva. Tudo na mesma transação (rollback se algo falhar). Ver lição global
  "EF Core: coluna identity só tem valor APÓS o SaveChanges".
- **Reusa `EstoquePersistencia`** (obter-ou-criar saldo com **cache** + tradução de concorrência) e a
  `OrigemMovimento.Compra` (enum já existente). Mesma fonte de invariantes de saldo/concorrência que
  faturamento e devolução. Ver [Inventory](../Inventory/MODULO.md).
- **Sem `xmin` no agregado** — coleção filha (itens) no mesmo save faz o UPDATE do pai afetar 0 linhas.
  Mesmo padrão de Pré-venda/Devolução. (O `estoque_produto` tocado pela entrada **tem** `xmin` — é outra tabela.)
- **F2 = catálogo seletor / F3 = fornecedor seletor** — reusa `CatalogoSeletorWindow` (de Sales) e o padrão
  "Seletor" e "Botão-seletor" do system.md. AutoCompleteBox descartado (quebrado no Avalonia 11).

## Dívidas / Pendências

- **Listagem sem busca nem paginação** — mesma dívida dos demais módulos; lista de compras cresce devagar.
- **Não atualiza custo do produto** — evolução planejada (sobrescrever vs. custo médio ponderado). Ver CONTEXTO → Evolução.
- **Importar NF-e (XML) → adiado para a Fase 5** (Fiscal). O XML do fornecedor desemboca nesta Compra
  (mesmo documento + entrada). Exige de-para `produto_fornecedor` (código do produto ≠ na NF), parsing
  SEFAZ e custo real (frete/ST/IPI). Nasce junto das tabelas fiscais (NCM/CFOP/CST). Ver CONTEXTO → Evolução.
- **`DbUpdateException` por unique-violation** no 1º movimento concorrente cai na mensagem genérica
  (integridade preservada pelo índice) — dívida compartilhada com faturamento/devolução/movimentação manual.

## Dependências

- Depende de: Cadastros (Fornecedor obrigatório, Produto/Catálogo), Security (usuário logado), Shared (`Result`),
  e **Estoque** (a entrada ao registrar — `CompraRepository` orquestra Compra + Estoque).
- Consome o **Catálogo** (F2) e o seletor de **Fornecedor** (F3).
