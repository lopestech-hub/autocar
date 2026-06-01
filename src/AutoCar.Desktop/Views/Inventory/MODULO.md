# Módulo Estoque (Inventory)

## Propósito

Controle de saldo de estoque por produto. Primeiro módulo a tratar **concorrência otimista**
(`xmin`) — o **risco nº 1** do projeto (dois terminais disputando o último item). Nesta fase entrega
a **fundação do saldo**: saldo atual + livro-razão de movimentos + tela de consulta/movimentação.
NÃO inclui ainda documento de compra nem a baixa pelo Faturar da Pré-venda (próximos passos).

## Tabelas

### `estoque_produto` (saldo atual — 1:1 com produto)

- `id_estoque_produto` (uuid PK), `id_produto` (FK única → `produto`, `Restrict`)
- `qtd_saldo` (int), `qtd_reservada` (int, default 0 — preparado para reserva de venda, **não usado no MVP**)
- `dat_criacao`, `dat_atualizacao` (UTC), **`xmin`** (concorrência otimista)
- Índice único: `ix_estoque_produto_produto`. Migration: `CadastroEstoque`.

### `movimento_estoque` (livro-razão — imutável)

- `id_movimento_estoque` (uuid PK), `cod_movimento` (int identity — rastreável)
- `id_produto` (FK → `produto`, `Restrict`), `id_usuario` (FK → `usuario`, `Restrict` — quem movimentou)
- `sts_tipo` (int — enum `TipoMovimentoEstoque`: Entrada=1, Saida=2, AjustePositivo=3, AjusteNegativo=4)
- `qtd` (int, **sempre positiva** — o tipo diz a direção), `qtd_saldo_apos` (int — foto do saldo p/ auditoria)
- `observacao` (varchar 255, opcional), `dat_criacao` (UTC). **Sem `xmin`** (registro imutável).
- Índices: `ix_movimento_estoque_cod` (único), `ix_movimento_estoque_produto`, `ix_movimento_estoque_data`.

> **Quantidades inteiras.** No setor automotivo a peça é unidade fechada (UN/PC/PAR/JG/KIT/CX); óleo
> e mangueira vendem em embalagem fechada. Não há saldo fracionado — `int`, não `decimal`.

## Camadas

- **Domain:** `EstoqueProduto` (raiz do agregado — `Movimentar(tipo, qtd, idUsuario, obs)` valida saldo
  não-negativo e devolve o `MovimentoEstoque`), `MovimentoEstoque` (imutável, construtor `internal`),
  enum `TipoMovimentoEstoque`, `IEstoqueRepository` (+ projeção de leitura `SaldoProdutoLeitura`),
  `ConcorrenciaException` (exceção neutra de domínio).
- **Application:** `Modules/Estoque` — `IEstoqueService`/`EstoqueService` (`Result`; converte invariante
  do domínio em `Error.Validacao` e `ConcorrenciaException` em `Error.Conflito`), DTOs
  (`MovimentarEstoqueDto`, `SaldoEstoqueListaDto`, `MovimentoEstoqueDto`), `MovimentarEstoqueValidator`.
- **Infrastructure:** `EstoqueProdutoConfiguration` (xmin via `IsRowVersion`), `MovimentoEstoqueConfiguration`,
  `EstoqueRepository` (`IDbContextFactory`). `ListarSaldosAsync` **projeta `SaldoProdutoLeitura` direto na
  query** (left join produto×saldo numa única consulta — sem N+1). Migration `CadastroEstoque` (aditiva).
- **Desktop:** `Views/Inventory` + `ViewModels/Inventory` — `EstoqueViewModel` (listagem) +
  `MovimentoEstoqueFormViewModel` (janela). `EstoqueView` (Grid único: CÓDIGO·PRODUTO·UN·SALDO·DISPONÍVEL,
  saldo zero em cinza), `MovimentoEstoqueWindow` (não-modal, ESC fecha), `MovimentoEstoqueFormView` (saldo
  em faixa azul + tipo/qtd/observação + histórico com OBS. e cor por direção). Rota `estoque` (perfil
  Vendedor). Conversores em `Converters/MovimentoEstoqueConverters.cs`.

## Regras de Negócio

- **Saldo nunca negativo** — saída/ajuste negativo maior que o saldo é rejeitado no domínio
  (não vende o que não tem). É a regra que a concorrência protege.
- **Quantidade sempre > 0** — o tipo do movimento define a direção (entrada/ajuste+ elevam; saída/ajuste− abaixam).
- **Movimento imutável** — cada operação gera uma linha no livro-razão; nunca se altera um movimento.
- **Saldo zerado implícito** — produto sem nenhum movimento aparece na listagem com saldo 0 (left join).

## Decisões Técnicas

- **`xmin` (concorrência otimista) — funciona limpo aqui.** Diferente de Produto/PreVenda (que tiveram de
  remover o `xmin` por causa da coleção filha no mesmo `SaveChanges`), o `estoque_produto` **não tem coleção
  filha**: o `MovimentoEstoque` gerado é um INSERT independente, não um UPDATE do mesmo registro. O
  `UPDATE estoque_produto ... WHERE xmin=@p` afeta 1 linha normalmente; se outro terminal moveu antes, afeta
  0 → `DbUpdateConcurrencyException`. Ver lição global de EF Core + Npgsql.
- **Tradução de exceção na Infra.** O repositório traduz `DbUpdateConcurrencyException` → `ConcorrenciaException`
  (neutra); a Application captura a neutra e devolve `Error.Conflito`. Mantém a Application livre do EF Core
  (Clean Architecture). Não generalizado para base repository: hoje o Estoque é o único agregado com conflito
  de concorrência real (N=1).
- **`MovimentarAsync(idProduto, Func<EstoqueProduto, MovimentoEstoque>)`** — saldo + movimento na MESMA
  transação. Carrega o saldo rastreado (cria zerado se não existir), aplica o `Func` do domínio, salva os dois.
- **Sem retry automático** na concorrência — o usuário reage à mensagem e refaz (MVP).

## Dívidas / Pendências

- **Race no primeiro movimento.** Dois terminais movimentando ao mesmo tempo um produto que ainda não tem
  registro de saldo: ambos tentam INSERT. O `xmin` não cobre INSERT concorrente — quem garante é o índice
  único `ix_estoque_produto_produto` (o 2º INSERT lança `DbUpdateException`, não `DbUpdateConcurrencyException`,
  e cai na mensagem genérica do ViewModel). **Integridade preservada** (nunca cria saldo duplicado); só a
  mensagem fica genérica nesse caso estreito. Tratar `DbUpdateException` por unique-violation se virar incômodo.
- **Coluna DISPONÍVEL = SALDO no MVP** (reservado sempre 0) — só ganha sentido quando a reserva existir.
- **Sem paginação** na listagem (mesma dívida dos demais cadastros).

## Dependências

- Depende de: Produto (FK), Security (usuário logado = quem movimentou), Shared (`Result`).
- Será consumido por: **Faturar da Pré-venda** (baixa estoque — próximo passo, via futuro evento
  `PreVendaFaturada`), entrada por compra/documento (próximo passo), e relatórios de estoque.
