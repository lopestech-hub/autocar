# AutoCar ERP

> Mapa do projeto. Detalhe técnico de cada módulo mora no `MODULO.md` da pasta dele.

## Objetivo

Sistema desktop Windows para negócio **híbrido de loja de autopeças + oficina mecânica**.
MVP: balcão rápido, OS integrada, catálogo automotivo, estoque, financeiro e base fiscal.

## Stack

- UI: **Avalonia UI 11.3.17** + MVVM (CommunityToolkit.Mvvm)
- Backend: C# 13 / .NET 9 + EF Core 9
- Banco: PostgreSQL local/LAN (localhost:5432) — banco `autocar`
- Arquitetura: Clean Architecture + DDD (módulos por domínio em `Application/Modules`)
- Libs: FluentValidation, Mapster, MediatR, BCrypt.Net-Next, Serilog, Npgsql,
  Projektanker.Icons.Avalonia + FontAwesome (ícones — exige Avalonia 11)
- Deploy: `dotnet publish -c Release -r win-x64 --self-contained`

## Modelo de implantação

LAN multi-terminal: PostgreSQL central na rede; vários terminais (balcão/caixa/oficina).
Concorrência de estoque tratada com controle otimista (`xmin`) — risco nº 1 do projeto.

## Mapa de módulos

| Módulo | Pasta (MODULO.md) | Estado |
|--------|-------------------|--------|
| Segurança (login, perfis) | [Security](src/AutoCar.Desktop/Views/Security/MODULO.md) | ✅ Fase 1 |
| Cadastros (Cliente ✅; Fornecedor ✅; Marca ✅; Categoria ✅; Produto ✅ + aplicação ✅; Serviço ✅; Mecânico ✅) | [Registrations](src/AutoCar.Desktop/Views/Registrations/MODULO.md) | 🔄 |
| Catálogo automotivo (peça → veículo) | [Catalogo](src/AutoCar.Desktop/Views/Catalogo/) — busca de peça por veículo ✅ | ✅ Fase 2 |
| Vendas (Pré-venda) | [Sales](src/AutoCar.Desktop/Views/Sales/MODULO.md) — Pré-venda (cabeçalho + itens) ✅ | 🔄 Fase 3 |
| Estoque (saldo + movimentação) | [Inventory](src/AutoCar.Desktop/Views/Inventory/MODULO.md) — saldo, livro-razão, concorrência `xmin` ✅ | ✅ Fase 3 |
| Compras (entrada por compra) | [Purchases](src/AutoCar.Desktop/Views/Purchases/MODULO.md) — documento fornecedor + itens → entrada no estoque ✅ | ✅ Fase 3 |
| Ordem de Serviço (peças + mão de obra, mecânico, ciclo) | [Service](src/AutoCar.Desktop/Views/Service/MODULO.md) — backend + UI ✅; faltam faturar (4.4) e gancho financeiro (4.5) | 🔄 Fase 4 |
| Financeiro (receber, pagar, caixa, formas pgto) | _a criar (Fase 4, após a OS)_ | ⏳ |
| Fiscal (NCM/CFOP/CST, sem SEFAZ) | _a criar (Fase 5)_ | ⏳ |

## Estado atual

**Fase 1 (Setup) — concluída.** Solution Clean Architecture, DI + Serilog, PostgreSQL, login por
usuário + perfis (BCrypt), logout, shell ERP (menu de texto + toolbar + status bar).

**Fase 2 (Cadastros) — em andamento.** Módulos **Cliente** e **Fornecedor concluídos**: cadastro PF/PJ
completo (CRUD, validação CPF/CNPJ com dígito verificador, **consulta automática de CNPJ via BrasilAPI**,
endereço). Cliente tem limite de crédito; Fornecedor tem Inscrição Estadual + Contato (vendedor) e abre
como PJ por padrão. Listagem densa (busca automática, badges, contador) + formulário denso dois modos.
Navegação por rota (`INavegador`). Testes passando (VO Documento + consulta CNPJ).

Cadastros auxiliares do Produto concluídos: **Marca** e **Categoria** (CRUD enxuto — só descrição,
unicidade case-insensitive — no menu Cadastros, perfil Vendedor). Tema padronizado nesta rodada:
ComboBox/menu corrigidos para o FluentTheme (cores via resource-key, não via `/template/` — evita
flicker), campos a 24px (`MinHeight` obrigatório), CAIXA ALTA automática em nomes/endereço, valores
monetários como `TextBox` (não `NumericUpDown`).

**Produto (mestre) concluído.** Tabela `produto` com FK para `categoria_produto` (obrigatória),
`marca` e `fornecedor` (opcionais). Campos: cod_barras (único quando informado — índice único
parcial), descrição, descrição complementar, cod_fabricante, unidade (enum `UnidadeMedida`),
vlr_custo, vlr_venda. Saldo de estoque NÃO mora aqui (fica na Fase 3). Tela: listagem Grid único +
formulário em **blocos de seção** (IDENTIFICAÇÃO/CLASSIFICAÇÃO/VALORES) com largura controlada e
margem % calculada. Nesta rodada também: seções de formulário ganharam **título azul + linha
divisória** (classes `formsecao`/`formsecaoLinha` no Tema), aplicado em Cliente, Fornecedor e Produto.
Próximo: aplicação por veículo (montadora/modelo/ano — texto livre no MVP).

Produto ganhou (Fase 3) o campo **`sts_posicao`** (enum `PosicaoPeca`: NaoAplica/Dianteira/Traseira) e a
aplicação por veículo ganhou **`motorizacao`** (texto) + **`sts_combustivel`** (enum `Combustivel`).

**Fase 3 (Vendas + Estoque) — em andamento.** Módulo **Pré-venda concluído** (ver [Sales](src/AutoCar.Desktop/Views/Sales/MODULO.md)):
documento provisório de balcão (cabeçalho cliente opcional + veículo livre + itens), abre em **janela
separada maximizada**, **F2** abre o Catálogo como seletor (clique marca, duplo-clique/Enter adiciona).
Total calculado no domínio (item + desconto geral); ciclo Aberta→Faturada/Cancelada (UI só cria/edita
Aberta por ora). Não baixa estoque (só no faturamento). Testes do agregado passando.

**Estoque (fundação do saldo) concluído** (ver [Inventory](src/AutoCar.Desktop/Views/Inventory/MODULO.md)):
saldo por produto (`estoque_produto`, **com `xmin`**) + livro-razão imutável (`movimento_estoque`).
Movimentação Entrada/Saída/Ajuste com **saldo nunca negativo** e **concorrência otimista** (risco nº 1 —
dois terminais no último item geram `Error.Conflito`). Quantidades **inteiras** (autopeça não fraciona).
Tela: listagem de saldos + janela de movimentação por produto (saldo + tipo/qtd/observação + histórico).
Cada movimento tem **origem rastreável** (`Manual`/`Venda nº X`...), separada da observação livre.

**Faturar da Pré-venda baixa estoque concluído**: ao faturar, cada item gera uma **Saída** (origem
`Venda nº X`) numa **transação única atômica** (`FaturamentoRepository` — fatura + baixa todos os itens
num só `SaveChanges`; saldo insuficiente bloqueia e dá rollback). UI: botão **Faturar** + confirmação
(`ConfirmacaoWindow`) + badge **FATURADA**.

**Devolução de venda concluída**: a partir de uma venda **Faturada**, devolve itens ao estoque
(parcial por item), gerando **Entrada** (origem `Devolução nº X`). Documento `devolucao`+`devolucao_item`
à parte (não desfatura a venda). Valida **saldo devolvível** (vendido − já devolvido). Transação explícita
com 2 saves (`cod_devolucao` identity precede os movimentos). UI: botão **Devolver** na venda faturada →
`DevolucaoWindow`.

**Entrada por compra concluída** (ver [Purchases](src/AutoCar.Desktop/Views/Purchases/MODULO.md)):
documento de compra (`compra`+`compra_item`) que dá **Entrada** no estoque ao salvar (origem `Compra nº X`),
fechando o ciclo do estoque (saída pelo faturamento, entrada pela compra). Fornecedor **obrigatório**;
**entrada imediata** (sem ciclo de vida); qtd inteira; custo unitário por item. Transação explícita com 2
saves (`cod_compra` identity precede os movimentos), reusa `CompraRepository` + `EstoquePersistencia`,
espelha a Devolução. UI: listagem + janela maximizada (**F2** = catálogo, **F3** = seletor de fornecedor);
reabre em visualização read-only. **Não atualiza o custo do produto** no MVP (ver Evolução).

**Fase 4 (OS + Financeiro) — em andamento.** **Ordem de Serviço** quase completa (falta só faturar).
Pronto: **catálogo de serviços** (`servico` ✅) e **cadastro de mecânico** (`mecanico` — entidade
**própria**, NÃO usuário do sistema: não loga, é só quem executa o serviço ✅); **agregado da OS**
(tabelas `ordem_servico`+`ordem_servico_item`) com **dois tipos de linha** (peça → baixa estoque;
serviço → sem estoque, discriminador `sts_tipo_item`), **mecânico responsável** (opcional ao abrir,
exigido para Concluir), **ciclo** Aberta→EmAndamento→Concluída→Faturada/Cancelada, **quilometragem**
(`qtd_km`, opcional), 22 testes do agregado; e a **UI completa** (listagem com badge das 5 situações;
janela maximizada com F2=peça/F3=cliente/F4=serviço/F5=mecânico, todos seletores por janela;
form com grid peça+serviço, totais por tipo, botões de ciclo; "Cancelar OS" com confirmação).
**Faturar a OS concluído (4.4):** ao faturar, cada linha **Peça** gera uma **Saída** no estoque (origem
`OrdemServico`, "OS nº X") numa **transação atômica única** (`FaturamentoOrdemServicoRepository`, espelha o
`FaturamentoRepository`); linhas de serviço não tocam estoque; saldo insuficiente bloqueia e dá rollback.
UI: botão Faturar → `ConfirmacaoWindow` → badge **FATURADA**. **Falta:** 4.5 — gancho (Domain Event) para o
Financeiro. Depois o **Financeiro** (contas a receber/pagar, caixa, formas de pagamento). Sub-fases: 4.1 ✅
catálogo de serviços · 4.1b ✅ mecânico · 4.2 ✅ OS backend · 4.3 ✅ OS UI · 4.4 ✅ faturar · 4.5 gancho Financeiro.

**Código e referência da peça nos itens (OS + Pré-venda):** a grade de itens dos dois documentos exibe
**CÓDIGO** (`cod_produto`) e **REFERÊNCIA** (`cod_fabricante`) de cada peça, na adição (F2) e ao reabrir um
documento salvo. Enriquecimento por **query batch** (`IProdutoRepository.ObterCodigosPorIdsAsync`, sem N+1);
serviços não têm código/referência. Itens são snapshot (descrição), então os códigos vêm do produto na leitura.

Admin de teste: usuário **`julio`** / senha `123` (trocar).

## Navegação (shell)

Layout estilo ERP (sem sidebar): menu de texto no topo + toolbar de atalhos + área central + status bar.

- **Toolbar de atalhos:** Buscar, Clientes, Fornecedor, Produtos, Orçamento, Pré-venda, Ordens de Serviço.
- **Menu de texto:** Cadastros (Clientes, Fornecedor, Produtos, Marcas, Categorias, Usuários) · Movimentos
  (Orçamento, Pré-venda, OS, Estoque) · Financeiro (Caixa, Contas a Receber, Contas a Pagar, Fiscal).
- **Clientes, Fornecedor, Marcas, Categorias, Produtos, Catálogo e Pré-venda** já têm tela real. Demais
  módulos ainda são **placeholders**.
- "Balcão" do plano = **Pré-venda** (abre em janela separada; F2 = catálogo seletor).
- Menu de texto corrigido: as categorias não renderizavam (gotcha do FluentTheme) — resolvido via
  `Menu.Styles` ligando Header/ItemsSource no MenuItem + cor de texto fixa.

## Perfis de acesso (enum PerfilUsuario)

Um perfil por usuário. Admin vê tudo.

- **Vendedor:** Clientes, Fornecedor, Produtos, Orçamento, Pré-venda, Estoque, Buscar.
- **Mecanico:** Clientes, Produtos, Ordens de Serviço, Buscar.
- **Financeiro:** Caixa, Contas a Receber, Contas a Pagar, Fiscal, Buscar.

## Roadmap

- [x] Fase 1 — Setup, auth, menu, perfis
- [x] Fase 2 — Cadastros base + catálogo
- [x] Fase 3 — Pré-venda ✅ · Estoque saldo + movimentação + concorrência `xmin` ✅ · Faturar baixa estoque ✅ · Devolução ✅ · entrada por compra ✅
- [ ] Fase 4 — OS: cat. serviços ✅ · mecânico ✅ · backend ✅ · UI ✅ · faturar baixa estoque (4.4) ✅ · gancho financeiro (4.5) ⏳ · depois Financeiro (receber/pagar/caixa/formas pgto)
- [ ] Fase 5 — Fiscal estruturado + PDF
- [ ] Fase 6 — (pós-MVP) SEFAZ, custo médio, multi-depósito, DRE
- [ ] Fase 7 — Testes, ajustes, implantação

## Evolução (ideias e melhorias futuras)

> Funil de ideias que surgem na conversa mas ainda não são roadmap nem dívida técnica imediata.
> Daqui saem futuros itens de roadmap. Não duplica "Próximos Passos" (dívidas) nem o Roadmap (fases).

| Ideia | Contexto / origem | Quando reavaliar |
| --- | --- | --- |
| **Compra atualiza custo do produto** | A entrada por compra (MVP) não toca o `vlr_custo` do produto. Evoluir para atualizar o custo (sobrescrever vs. **custo médio ponderado**) e recalcular margem. | Quando a compra estiver em uso e o custo precisar refletir a última entrada |
| **Importar NF-e (XML) → Compra** | Importar o XML da NF-e do fornecedor para gerar a Compra automaticamente (em vez de digitar item a item). O XML **desemboca na Compra atual** (mesmo documento + entrada de estoque). 3 complexidades: (1) **de-para de produto** — código do produto na NF ≠ `cod_produto`, exige tabela `produto_fornecedor` (idProduto+idFornecedor+codNoFornecedor), aprendida na 1ª importação; (2) **parsing SEFAZ** (`nfeProc`/`infNFe`, ~40 campos/item, CST/CFOP/NCM); (3) **custo real** = vProd + rateio de frete/ST/IPI, não só o valor do item. | **Fase 5 (Fiscal)** — decidido em 2026-06-02 adiar para nascer junto das tabelas fiscais (NCM/CFOP/CST), sem duplicar. Construir o XML agora puxaria meio módulo fiscal para a Fase 3. |
| **Estorno de OS faturada** | OS faturada baixou a peça do estoque; cancelar depois exige estornar (devolver a peça). No MVP, OS Faturada **não cancela** (igual pré-venda). Evoluir para um documento de estorno/devolução de OS, análogo à Devolução de venda. | Quando a OS estiver em uso e surgir o caso real de estorno pós-faturamento |
| **Reserva de estoque em documento aberto (OS/Pré-venda)** | Julio, 2026-06-06: oficina vende no balcão também. Estoque só baixa ao **faturar** — entre adicionar a peça a uma OS/pré-venda aberta e faturar, a quantidade segue "disponível" para todos; com 1 unidade já numa OS, outro vendedor pode vender a mesma peça (venda dupla). **Gancho:** `estoque_produto.qtd_reservada` (Fase 3, hoje sempre 0); **DISPONÍVEL** = `saldo − reservado`. Decisões: quando reservar; quando liberar (cancelar/remover/faturar); atomicidade (`xmin`). Vale OS e Pré-venda. | Quando OS/balcão estiver em uso real com risco de venda concorrente da mesma peça (sugestão: junto/após a 4.4) |
| **Estoque pendente de recebimento físico (NF lançada, mercadoria não chegou)** | Julio, 2026-06-06: ao lançar uma NF de compra, a entrada soma no saldo — mas a mercadoria física pode não ter chegado. Esses produtos não podem ser vendidos até chegarem. Conceito de **estoque em trânsito/pendente**: conta no sistema mas indisponível para venda até confirmar o recebimento físico. Decisões: flag/status "recebido" na entrada por compra; saldo separado (pendente vs disponível) ou "disponível = saldo − pendente"; ponto de confirmação do recebimento (conferência física). Liga Compras + Fiscal (NF-e). | Quando a entrada por NF-e estiver no fluxo (Fase 5 — Fiscal) e a separação física/lançamento for problema real |

## Próximos Passos

1. **Cancelar** na UI da Pré-venda (o domínio já tem `Cancelar()`; falta botão — cancelar NÃO mexe em estoque).
2. Implementar a busca global de verdade (atalho "Buscar" hoje é placeholder)
3. Tela de troca de senha no primeiro acesso
4. (dívida) **Padronizar seleção≠adição nas listas de cadastro** — hoje só os seletores (Catálogo) usam;
   aplicar nas demais quando a marca tiver função (ex: habilitar Editar/Inativar). Ver padrão no system.md.
5. (dívida) Paginação na **query** de Produto/Catálogo/Pré-venda/Compra quando o volume crescer (sem
   `Take`/`Skip`) — a UI já não trava (todas as listagens virtualizadas em `ListBox.tabela`); resta
   limitar o que vem do banco pela LAN
6. (dívida) Migrar os demais repositórios para `IDbContextFactory` quando ganharem coleção filha
   (hoje `ProdutoRepository`, `PreVendaRepository`, `DevolucaoRepository` e `CompraRepository` usam; os outros usam `AppDbContext` injetado)
7. (dívida) **Remover/gear por flag o seed demo do DbInitializer antes do deploy** — hoje cria 8
   produtos de teste em banco com ≤1 produto; inofensivo em dev, mas não pode rodar no cliente real
8. (dívida) Estoque: tratar `DbUpdateException` por unique-violation no 1º movimento concorrente (mensagem
   genérica; integridade já garantida pelo índice) — vale p/ movimentação manual, faturamento, devolução e compra;
   e TOCTOU no saldo devolvível da Devolução (duas devoluções simultâneas da mesma venda sem lock).

## Histórico de Mudanças

| Data | Mudança |
| --- | --- |
| 2026-05-29 | Fase 1 concluída: solution, banco, auth por usuário + perfis, logout |
| 2026-05-29 | Shell redesenhado para layout ERP (menu de texto + toolbar) com ícones FontAwesome |
| 2026-05-29 | Login mudado de e-mail para usuário (julio/123); Avalonia 12 → 11 (compat. ícones) |
| 2026-05-29 | Toolbar reorganizada: Buscar, Clientes, Fornecedor, Produtos, Orçamento, Pré-venda, OS |
| 2026-05-29 | Fase 2: módulo Cliente (CRUD PF/PJ, validação CPF/CNPJ, consulta CNPJ BrasilAPI, form denso) + navegação por rota |
| 2026-05-29 | Fase 2: módulo Fornecedor (CRUD PF/PJ, Inscrição Estadual + Contato, consulta CNPJ, default PJ) |
| 2026-05-29 | Fase 2: cadastros Marca e Categoria (CRUD enxuto, unicidade case-insensitive) — base do Produto |
| 2026-05-29 | UI: correção do menu de texto + padronização de tema (FluentTheme via resource-keys, MinHeight 24, CAIXA ALTA, NumericUpDown→TextBox) |
| 2026-05-29 | Setup Claude Code: MCP context7, subagent avalonia-reviewer, skill /novo-cadastro, hook anti-edição de migration |
| 2026-05-31 | Fase 2: cadastro Produto (mestre) — FK categoria/marca/fornecedor, unidade enum, cod_barras único parcial, margem calculada |
| 2026-05-31 | UI: padronização das seções de formulário (título azul + linha) em Cliente/Fornecedor/Produto; hook PostToolUse lembra o system.md ao editar .axaml |
| 2026-05-31 | Fase 2: aplicação por veículo no Produto (tabela `produto_aplicacao` 1:N, mini-grid no form). ProdutoRepository → IDbContextFactory; Produto sem xmin (incompat. com coleção filha) |
| 2026-05-31 | Fase 2: tela **Catálogo** (busca de peça por montadora/modelo/ano, cruzando produto × aplicação) + seed demo de 8 produtos no DbInitializer |
| 2026-06-01 | Fase 3: módulo **Pré-venda** (tabelas `pre_venda`+`pre_venda_item`, agregado com total no domínio, cliente opcional, veículo livre, snapshot de preço, desconto item+geral, ciclo Aberta/Faturada/Cancelada). Abre em **janela separada**; **F2** = Catálogo seletor (clique marca, duplo-clique/Enter adiciona) |
| 2026-06-01 | Produto: campo **`sts_posicao`** (Dianteira/Traseira); aplicação por veículo ganhou **`motorizacao`** + **`sts_combustivel`** (migrations `PosicaoProduto`, `AplicacaoMotorCombustivel`) |
| 2026-06-01 | UI: realces de cor com significado (faixa do TOTAL, badge com borda, célula âmbar de desconto, flash de item novo, régua azul no seletor); padrão "Seletor" registrado no system.md |
| 2026-06-01 | UX Pré-venda: seleção-tudo ao focar campos numéricos (`SelecionarTudoBehavior`); borda de campo mais forte (#1E293B); cliente vira **seletor por janela** (F3, busca nome/código) — AutoCompleteBox descartado (quebrado no Avalonia 11); tooltip azul-ardósia padronizado |
| 2026-06-01 | Fase 3: módulo **Estoque** (fundação do saldo) — tabelas `estoque_produto` (**com `xmin`**) + `movimento_estoque` (livro-razão imutável); movimentação Entrada/Saída/Ajuste com saldo nunca negativo e **concorrência otimista** (risco nº 1); quantidades **inteiras**; `ConcorrenciaException` neutra (Clean Architecture); tela listagem + janela de movimentação. Migration `CadastroEstoque` (aditiva). 9 testes do agregado |
| 2026-06-01 | Fase 3: **Faturar da Pré-venda baixa estoque** — `FaturamentoRepository` (transação única atômica: fatura + Saída de todos os itens num `SaveChanges`; saldo insuficiente faz rollback). Botão Faturar + `ConfirmacaoWindow` (diálogo reutilizável) + badge FATURADA. Movimento ganhou **origem** rastreável (`sts_origem`/id/cod — "Venda nº X" separado da observação; migration `OrigemMovimentoEstoque`, aditiva). Helper `EstoquePersistencia` compartilhado entre os repos de estoque |
| 2026-06-02 | Fase 3: **Devolução de venda** — tabelas `devolucao`+`devolucao_item` (migration `CadastroDevolucao`, aditiva); devolve itens de venda Faturada ao estoque (parcial por item, **Entrada** origem `Devolução nº X`), valida saldo devolvível (vendido − já devolvido); `DevolucaoRepository` com transação explícita (2 saves — `cod` identity precede os movimentos). Botão Devolver + `DevolucaoWindow`. Histórico de estoque: colunas **ORIGEM** e **DOCUMENTO** separadas. Helper `QuantidadeEstoque.DeDocumento` (decimal→int, rejeita fração) compartilhado faturamento+devolução |
| 2026-06-02 | Fase 3 (fecha): **Entrada por compra** (backend `b072be6` + UI `4486025`) — tabelas `compra`+`compra_item` (migration `CadastroCompra`, aditiva); documento fornecedor + itens dá **Entrada** no estoque ao salvar (origem `Compra nº X`), fornecedor obrigatório, entrada imediata (sem ciclo), qtd inteira, custo por item. `CompraRepository` transação explícita (2 saves), reusa `EstoquePersistencia`, espelha a Devolução. UI: listagem + janela maximizada (F2=catálogo, F3=seletor de fornecedor), reabre read-only; menu Movimentos (perfil Vendedor). Não atualiza custo do produto (ver Evolução). 7 testes do agregado. Smoke test ok (saldo subiu) |
| 2026-06-02 | Fase 4 **planejada** (Theo Desktop): **Ordem de Serviço** primeiro. Decisões: linha única com discriminador `sts_tipo_item` (Peca/Servico); baixa estoque **ao faturar**; serviço do catálogo + editável; um mecânico por OS (opcional ao abrir, exigido p/ Concluir). Tabelas novas `servico`, `ordem_servico`, `ordem_servico_item`; enums `SituacaoOrdemServico`, `TipoItemOrdemServico` + `OrdemServico` no `OrigemMovimento`. MODULO.md do Service criado. Implementação em sub-fases 4.1→4.5. Financeiro a seguir |
| 2026-06-03 | Fase 4.1 (`da1db2f`): **catálogo de serviços** (`servico` — mão de obra: descrição + valor padrão, molde Marca/Categoria). 4.2 (`5d1d321`): **OS backend** — agregado `OrdemServico`+`OrdemServicoItem` (factories DePeca/DeServico garantem a invariante "uma FK por tipo"), ciclo Iniciar/Concluir/Cancelar/Faturar, subtotais por tipo, `OrdemServicoRepository` (separou `AtualizarAsync` de `AplicarTransicaoAsync` — transição não refaz o swap da coleção filha), 22 testes |
| 2026-06-03 | Fase 4.1b (`4da5dcd`): **cadastro de Mecânico** como entidade própria (não usuário — correção de modelo); FK da OS `id_usuario_mecanico`→`id_mecanico` (migration por rename, não-destrutiva) |
| 2026-06-14 | Fase 4.4 (`edda0ea`): **Faturar a OS baixa estoque das peças** — `FaturamentoOrdemServicoRepository` (transação atômica única, espelha o `FaturamentoRepository`; só linhas Peça dão Saída, origem `OrdemServico`; rollback se faltar saldo). `OrdemServicoService.FaturarAsync` + UI (botão Faturar → `ConfirmacaoWindow` → badge FATURADA, lista recarrega). Validado no banco (OS nº 3 faturada, peça baixou, serviço ignorado) |
| 2026-06-14 | UI (`edda0ea`): **código e referência da peça nos itens** de OS e Pré-venda — colunas CÓDIGO (`cod_produto`) e REFERÊNCIA (`cod_fabricante`) na grade, na adição (F2) e ao reabrir; enriquecimento por query batch (`IProdutoRepository.ObterCodigosPorIdsAsync`, sem N+1). Polimento: TOTAL e subtotais com borda, títulos do header alinhados ao conteúdo das caixas (header recua 6px = padding 5 + borda 1 do TextBox) |
| 2026-06-15 | Perf (`5935715`/`c6d6a5f`/`f1a34ea`): **virtualização concluída em TODAS as listagens** — Bloco A (6 cadastros: Marcas, Categorias, Serviços, Mecânicos, Clientes, Fornecedores), Bloco B (4 movimentos: Pré-vendas, Compras, OS, Estoque) e Bloco C (4 seletores F3/F4/F5: Cliente, Fornecedor, Serviço, Mecânico). Converters reutilizáveis novos: `StatusAtivo*`, `TipoPessoa*`/`DocumentoMascara*`, `Situacao*` (badges Pré-venda/OS), `Saldo*`, `DataBrasiliaCurta`, `Veiculo`. Padrão `ListBox.tabela` agora é universal no projeto |
| 2026-06-14 | Perf (`7e7b29d`): **virtualização da listagem** (Produtos + Catálogo) — troca o "Grid único code-behind" por `ListBox` virtualizado (estilo `ListBox.tabela` no Tema, reutilizável). A tela de Produtos **travava com 256 produtos** (montava ~11 controles/linha de uma vez na UI thread); agora abre instantânea e escala. Catálogo migrado nos 2 modos (consulta + seletor F2, API pública preservada); seletor F2 abre maximizado. Falta migrar as 13 listagens restantes |
| 2026-06-06 | Fase 4.3 (`beee975`): **UI completa da OS** — listagem (badge das 5 situações, coluna VEÍCULO=modelo+placa), janela maximizada (F2/F3/F4/F5 todos seletores por janela), form (grid peça+serviço, totais por tipo, botões de ciclo). Mecânico vira **botão-seletor por janela** (era combo). UX: "Cancelar OS (encerrar)" com confirmação ≠ "Fechar" (evita cancelar por engano). Feature **KM** (`qtd_km`, opcional; migration aditiva). Seletores `ServicoSeletorWindow`/`MecanicoSeletorWindow` (molde do Cliente) |
