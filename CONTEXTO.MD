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
| Cadastros (Cliente ✅; Fornecedor ✅; Marca ✅; Categoria ✅; Produto ✅ + aplicação por veículo ✅) | [Registrations](src/AutoCar.Desktop/Views/Registrations/MODULO.md) | 🔄 Fase 2 |
| Catálogo automotivo (peça → veículo) | [Catalogo](src/AutoCar.Desktop/Views/Catalogo/) — busca de peça por veículo ✅ | ✅ Fase 2 |
| Vendas (Pré-venda) | [Sales](src/AutoCar.Desktop/Views/Sales/MODULO.md) — Pré-venda (cabeçalho + itens) ✅ | 🔄 Fase 3 |
| Estoque (entrada por compra) | _a criar (Fase 3)_ | ⏳ |
| Oficina / OS | _a criar (Fase 4)_ | ⏳ |
| Financeiro (caixa, contas) | _a criar (Fase 4)_ | ⏳ |
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

**Fase 3 (Vendas) — em andamento.** Módulo **Pré-venda concluído** (ver [Sales](src/AutoCar.Desktop/Views/Sales/MODULO.md)):
documento provisório de balcão (cabeçalho cliente opcional + veículo livre + itens), abre em **janela
separada maximizada**, **F2** abre o Catálogo como seletor (clique marca, duplo-clique/Enter adiciona).
Total calculado no domínio (item + desconto geral); ciclo Aberta→Faturada/Cancelada (UI só cria/edita
Aberta por ora). Não baixa estoque (só no faturamento, Fase de Estoque). Testes do agregado passando.

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
- [ ] Fase 3 — Pré-venda ✅ · Estoque (entrada por compra) + Domain Events + concorrência (pendente)
- [ ] Fase 4 — OS + Financeiro
- [ ] Fase 5 — Fiscal estruturado + PDF
- [ ] Fase 6 — (pós-MVP) SEFAZ, custo médio, multi-depósito, DRE
- [ ] Fase 7 — Testes, ajustes, implantação

## Próximos Passos

1. **Estoque** (Fase 3): entrada por compra + saldo; o **Faturar** da Pré-venda baixa estoque
   (via Domain Event `PreVendaFaturada`). Concorrência otimista (`xmin`) — risco nº 1.
2. **Faturar/Cancelar** na UI da Pré-venda (o domínio já tem; falta botão + fluxo) — vinculado ao Estoque.
3. Implementar a busca global de verdade (atalho "Buscar" hoje é placeholder)
4. Tela de troca de senha no primeiro acesso
5. (dívida) **Padronizar seleção≠adição nas listas de cadastro** — hoje só os seletores (Catálogo) usam;
   aplicar nas demais quando a marca tiver função (ex: habilitar Editar/Inativar). Ver padrão no system.md.
6. (dívida) Paginação na listagem de Produto/Catálogo/Pré-venda quando o volume crescer (sem `Take`/`Skip`)
7. (dívida) Migrar os demais repositórios para `IDbContextFactory` quando ganharem coleção filha
   (hoje `ProdutoRepository` e `PreVendaRepository` usam; os outros usam `AppDbContext` injetado)
8. (dívida) **Remover/gear por flag o seed demo do DbInitializer antes do deploy** — hoje cria 8
   produtos de teste em banco com ≤1 produto; inofensivo em dev, mas não pode rodar no cliente real

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
