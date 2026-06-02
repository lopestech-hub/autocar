# Sistema de Design — AutoCar ERP

> Desktop **Avalonia UI 11.3.17** + C#/.NET 9. Direção: **Precision & Density / Compacto Enterprise**.
> Mantido pela Luna (`/design-engineer-desktop`). **Reusar os padrões e classes daqui antes de criar
> qualquer coisa nova.** Toda tela/componente novo deve seguir este documento.

## Direção

ERP de autopeças + oficina. Operador usa 8h/dia → densidade alta, monocromático,
bordas no lugar de sombras, tipografia compacta. Cor só comunica significado.
Única licença de personalidade: a marca "Auto**Car**" com "Car" em azul primário.

## Paleta (em Styles/Tema.axaml — consumir via DynamicResource)

| Token | Valor | Uso |
| --- | --- | --- |
| `BrushFundo` | #F0F2F5 | tela principal |
| `BrushFundoCard` | #FFFFFF | cards, formulários, menu/toolbar |
| `BrushFundoHover` | #F8FAFC | hover de linha/botão |
| `BrushFundoHeader` | #F8FAFC | barra de status, header de tabela |
| `BrushPrimario` | #3B82F6 | botão primário, foco, marca, ícone da toolbar |
| `BrushBorda` | #1E293B | borda padrão de campo (repouso) — forte/bem definida; foco vira azul |
| `BrushTexto` | #1E293B | texto principal |
| `BrushTextoSecundario` | #64748B | labels, subtítulos, legendas |
| `BrushTextoInverso` | #FFFFFF | texto/ícone sobre fundo colorido |
| `BrushErro` | #EF4444 | mensagens de erro, botão Sair |

> Os tokens `BrushFundoSidebar*` ainda existem no Tema.axaml mas **não são mais usados** (o layout
> deixou de ter sidebar escura). Podem ser removidos numa limpeza futura.

## Tipografia

- Fonte interface: **Segoe UI**. Dados/códigos/valores: **Consolas** (`FonteMono`).
- Título janela 14px SemiBold · seção 12px SemiBold · corpo 12px · label 10px Medium.

## Tokens de densidade

| Elemento | Altura | Fonte |
| --- | --- | --- |
| TextBox / ComboBox | 24px | 12px |
| TextBox login (`Classes="login"`) | 28px | 12px (exceção: tela baixa densidade) |
| Botão formulário (`primario`/`secundario`) | 26px | 12px |
| Botão grande (`primario grande`) | 32px largura total | login |
| Atalho da toolbar (`toolbaritem`) | 76×64px | ícone 16px, legenda 9px |

CornerRadius: controles 2px · cards 6px · ícone-marcador 3px.

## Classes de estilo disponíveis (Styles/Tema.axaml)

- `Button.primario` / `Button.primario.grande` — botão de ação principal (azul sólido). Use no "+ Novo".
- `Button.secundario` — botão de ação secundária (branco + borda). Use no "Buscar".
- `Button.faturar` — botão verde (`#22C55E`, família da situação Faturada), 26px, mesma forma do primário.
  Ação de **confirmar venda/faturar** (cor com significado, não decoração). Estados redefinem `Foreground`.
- `Button.toolbaritem` — atalho da toolbar do shell (ícone grande + legenda, 76×64, alinhado ao topo)
- `Menu.topo` / `Menu.topo > MenuItem` — menu superior de texto
- `TextBox.login` — input de 28px da tela de login
- `TextBlock.formlabel` — label à direita do campo (cinza secundário, 12px). Em todo cadastro.
- `TextBlock.formsecao` — título de seção em **azul primário** + SemiBold (ex: "IDENTIFICAÇÃO"). Destaca o
  grupo sem competir com as labels cinza. Acompanhar SEMPRE de `Border.formsecaoLinha` ao lado.
- `Border.formsecaoLinha` — linha divisória 1px (`BrushBordaSuave`) que preenche a largura ao lado do
  título de seção, dando o ar de "cabeçalho de grupo".

> ⚠️ **NÃO existe classe `toolbar`** — só `toolbaritem` (atalho do shell). Botão com `Classes="toolbar"`
> cai no estilo default do FluentTheme e fica apagado/cinza. Para botões de ação em listagens use
> `primario`/`secundario`.

> ⚠️ **FluentTheme apaga texto/borda em estados (:pointerover, :focus, :pressed).** Sempre que um
> estilo mexe no `Background` do `ContentPresenter`/`PART_BorderElement` num estado, **redefinir
> também `Foreground` e `BorderThickness`** — senão o tema aplica cor clara que some no fundo, ou
> engrossa a borda para 2px. Já mordeu: texto branco no login, texto sumindo no hover do botão
> secundário, borda grossa no foco do TextBox.

## Padrões estabelecidos

### Formulário denso (cadastros) — PADRÃO do projeto

> Estilo escolhido pelo Julio: **denso clássico de ERP Windows Forms** (referência: ERP Bezerra).
> NÃO usar label-em-cima espaçado. Vale para Cliente, Fornecedor, Produto, etc.

- **Label à ESQUERDA do campo**, alinhado à direita (`TextBlock.formlabel`), largura fixa ~90-110px,
  texto 12px cinza. Campo logo ao lado. Nunca label em cima.
- **Vários campos por linha** — usar um único `Grid` com colunas `label,campo,label,campo,...` e
  `RowSpacing="6"`, aproveitando a largura toda (`HorizontalAlignment="Stretch"`, sem MaxWidth
  centralizado). Campos longos usam `Grid.ColumnSpan`.
- **Compacto**: campos 24px, espaçamento vertical 6px entre linhas. Operador vê o cadastro quase
  todo sem rolar.
- **Seções** = título azul (`TextBlock.formsecao`) + linha divisória (`Border.formsecaoLinha`) ao lado,
  preenchendo a largura. Dois jeitos de montar o par, conforme o layout do form:
  - **Grid único** (Cliente, Fornecedor): a seção ocupa uma linha com `Grid.ColumnSpan` total. Envolver
    título+linha num `DockPanel LastChildFill="True"` (título `DockPanel.Dock="Left"` + Border).
  - **StackPanel de blocos** (Produto): cada seção é um `Grid ColumnDefinitions="Auto,*"` (título col 0 +
    Border col 1). Ver `ProdutoFormView.axaml`.
  - Margem do bloco de seção: `0,8,0,2` (primeira) / `0,12,0,4` (demais, mais respiro entre grupos).
- Header: título + badge VISUALIZAÇÃO/EDIÇÃO. Rodapé dois modos (Fechar/Editar ↔ Cancelar/Salvar).
- Classes no Tema: `TextBlock.formlabel`, `TextBlock.formsecao` (azul), `Border.formsecaoLinha`.
- Referência viva: `ClienteFormView.axaml` (Grid único) e `ProdutoFormView.axaml` (blocos em seções).

### Formulário em blocos de seção — variante para cadastros com poucos campos (PADRÃO do Produto)

> Quando o cadastro tem poucos campos, o Grid único de Cliente/Fornecedor deixa um vazião embaixo e
> espalha os pares. Para esses casos, agrupar em **blocos de seção** com largura controlada.
> Julio aprovou esse layout para o Produto ("agora eu amei").

- **StackPanel `HorizontalAlignment="Left"`** com um bloco por grupo (IDENTIFICAÇÃO, CLASSIFICAÇÃO, VALORES).
- **Largura controlada, não esticar até a borda**: cada campo com `Width` adequado ao conteúdo
  (`HorizontalAlignment="Left"`) — descrição larga, códigos/valores estreitos. Evita o "campo gigante".
- Cada bloco é um `Grid` próprio (`RowSpacing="6"`, colunas `label,campo,label,campo`). Campos longos com
  `ColumnSpan`. A linha de seção usa `Width` fixo (ex: 780) para o divisor não cruzar a tela inteira.
- **Valor calculado read-only** (ex: Margem % do Produto = `(venda-custo)/custo`) como `TextBlock` em
  `FonteMono`, cinza, ao lado dos campos de valor. Recalcula via `OnXChanged` no ViewModel; "—" sem base.

### Seletor de item (lista para ESCOLHER, não para abrir) — PADRÃO

> Estabelecido no Catálogo aberto via F2 na Pré-venda. Para qualquer lista cujo objetivo é **escolher
> um item para trazer para outro lugar** (seletor), separar SELEÇÃO de AÇÃO:

- **Hover** (mouse) = destaque passageiro `#EFF6FF`. **Não** muda a seleção.
- **Clique simples** = SELECIONA (marca a linha). **Setas ↑/↓** = movem a seleção. **Seleção vence o
  hover** (a linha marcada mantém o destaque forte mesmo sob o mouse).
- **Duplo-clique** ou **Enter** = AÇÃO (adiciona/confirma). Nunca adicionar com clique simples ou hover.
- Linha selecionada: fundo `#DBEAFE` + **barra lateral 3px** azul-primário `#3B82F6` na borda esquerda
  ("régua" que o olho segue). **1ª linha já vem selecionada** ao abrir (fluxo de teclado imediato).
- Implementado via Grid único code-behind (`CatalogoView` modo seletor): lista de Borders das linhas +
  lista de barras + `_indiceSelecionado`; `DestacarLinha(i)` repinta tudo e `BringIntoView`.

### Seleção por teclado nas listas de cadastro — PADRÃO

> Mesma mecânica de marca visual do "Seletor", aplicada a TODAS as listas de cadastro (Clientes,
> Fornecedor, Produtos, Marcas, Categorias, Pré-vendas). Aqui a marca é **só visual/consistência** —
> a ação principal (abrir o form) continua no duplo-clique/Enter.

- **Clique simples** marca a linha (fundo `#DBEAFE` + barra lateral 3px `#3B82F6`). **↑/↓** navegam a
  marca (`BringIntoView`). **Enter** ou **duplo-clique** abrem o form (via `AbrirCommand` com o DTO da linha).
- **Hover** (`#EFF6FF`) não desfaz a marca (a seleção vence o hover).
- Implementação por tela (Grid único code-behind): `Focusable=true` + `KeyDown += AoTeclarNaLista`;
  `_linhas`/`_barras`/`_indiceSelecionado` resetados a cada `RegerarTabela`; `Tapped` faz `grid.Focus()`
  + `DestacarLinha` (o foco habilita as setas). `DestacarLinha` e `AoTeclarNaLista` iguais em todas.
- **ESC fecha o formulário** (todos os forms): `KeyBinding Escape → CancelarCommand` (UserControl.KeyBindings
  nos cadastros; Window.KeyBindings na Pré-venda). Cancela direto, sem confirmação.
- Dívida (não feito): a marca ainda não habilita ações (Editar/Inativar na linha) — fazer se surgir o gatilho.

### Realces de cor com significado (Pré-venda) — PADRÃO

> Reforço de cor pedido para o vendedor de balcão (8h/dia), sem virar arco-íris. Cor = informação.

- **Faixa do TOTAL**: o número que o vendedor mais olha vira `Border` fundo `#EFF6FF`, radius 3, valor
  azul-primário 16px Bold. **Cinza `#94A3B8` quando zerado** (sem itens) via `TotalBrushConverter`.
- **Badge de situação**: fundo claro + **borda 1px** da cor forte da família (Aberta azul `#3B82F6`,
  Faturada verde `#22C55E`, Cancelada vermelho `#EF4444`) — etiqueta sólida que salta da linha.
- **Célula âmbar**: campo de desconto do item com desconto > 0 ganha fundo `#FEF3C7` (`DescontoFundoConverter`).
- **Flash de item novo**: linha pisca `#FEF9C3` por ~1.2s ao ser adicionada — via **`DispatcherTimer`**
  (NUNCA `Style.Animations`: anima trava o Avalonia). Propriedade `BrushFundoLinha` no item-VM.
- **Header de coluna**: texto `#475569` SemiBold (era `#64748B` Medium) — âncora visual mais forte.

### Documento de balcão em janela separada (PreVendaWindow) — PADRÃO

- Documento de venda (Pré-venda; futuro Orçamento/OS/Venda) abre em **`Window` maximizada não-modal**
  (`Show(dono)`), não embutido no shell — o principal segue acessível (Alt+Tab).
- A listagem dispara **evento** (`AbrirJanelaSolicitado`); a View (code-behind) abre a janela. A
  ViewModel da listagem recebe `Func<FormViewModel>` (form novo por janela, sem estado compartilhado).
- **F2** na janela abre o **seletor de peça** (Catálogo) via `KeyBinding` → comando do ViewModel.
- Vendedor (usuário logado) aparece read-only no cabeçalho.

### Confirmação de ação irreversível (ConfirmacaoWindow) — PADRÃO

> Janela modal genérica para confirmar ações sem volta (faturar, futuramente inativar/cancelar).
> Reutilizável — não criar diálogo one-off por tela.

- `Views/ConfirmacaoWindow` (400px, `SizeToContent=Height`, `CenterOwner`): título + mensagem +
  Cancelar (`IsCancel`, ESC) / Confirmar (`IsDefault`, Enter). Botão confirmar com rótulo customizável.
- Uso: `await ConfirmacaoWindow.MostrarAsync(dono, titulo, mensagem, textoConfirmar)` → `bool`.
- A janela-pai (ex: PreVendaWindow) escuta um **evento** do ViewModel (`ConfirmacaoXSolicitada`), abre a
  confirmação e, se `true`, chama o método assíncrono do VM que efetiva a ação. O VM não abre janela.
- Ex: faturar a Pré-venda — botão `Button.faturar` → evento → ConfirmacaoWindow → `ConfirmarFaturamentoAsync`.

### Janela de Login (LoginWindow)

- 380×440, `CanResize=False`, centralizada. Card branco central sobre fundo cinza.
- Marca → **Usuário** → Senha (PasswordChar="•") → botão Entrar (`IsDefault=True`, Enter dispara)
  → ProgressBar 2px (loading) → mensagem de erro 11px vermelha.
- Login é por **nome de usuário** (campo `Login`), não e-mail.
- Padrão de botão assíncrono: `Carregando` + `MensagemErro`, `IsEnabled="{Binding !Carregando}"`.

### Botão-seletor (campo que abre janela de busca) — PADRÃO

> Para escolher um registro relacionado (cliente, e futuros) sem combo: um `Button Classes="seletor"`
> que parece campo de formulário (24px, borda, fundo branco, texto à esquerda) e, ao clicar/Enter,
> abre uma **janela seletora** (Grid único: busca por nome/código, setas, Enter/duplo-clique).

- Estilo `Button.seletor` no Tema (reutilizável). O ViewModel expõe `XTexto` (read-only, "CÓD — NOME"
  ou rótulo padrão tipo "Consumidor"), um comando `AbrirSeletorX` e um método `DefinirX(item)`.
- A janela seletora segue o padrão "Seletor" (ver acima) e é aberta modal pela janela-pai via evento.
- Atalhos: na Pré-venda, **F2 = peça** (Catálogo), **F3 = cliente**.
- ⚠️ **NÃO usar `AutoCompleteBox` para isso no Avalonia 11** — ele renderiza quebrado (o PART_TextBox
  interno herda o fundo disabled e fica cinza mesmo habilitado; o popup posiciona mal). Foi descartado
  em favor do botão-seletor + janela. Ver [[licao-autocompletebox-avalonia11]] (memória global).

### Tooltip — PADRÃO

- Fundo **azul-ardósia `#1E293B`** (ColorTexto), texto branco, borda sutil `#64748B`, radius 3, 11px.
  Estilo global `Selector="ToolTip"` no Tema — destaca mais que o escuro padrão e usa cor da paleta.
- Microcopy de atalho: usar `=` (ex: "F3 = buscar cliente"), nunca traço longo.

### Contador de itens (cabeçalho de listagem) — PADRÃO

> A "etiqueta" ao lado do título da listagem que mostra "X produtos", "9 peças", etc.

- `Border` fundo **`#DBEAFE`** (azul-claro = informação) + borda **`#3B82F6`** 1px + texto **`#1E40AF`**
  11px Medium, **CornerRadius 2** (cantos retos, nunca pílula arredondada), `Padding="8,1"`.
- Mesma família visual do badge "Aberta" da Pré-venda — cor comunica "informação", coeso e leve.
- Aplicado em todas as listagens (Catálogo, Produtos, Clientes, Fornecedor, Marcas, Categorias, Pré-vendas).
- ⚠️ Não usar `CornerRadius` alto (pílula) — destoa do enterprise compacto; o padrão é canto reto.

### Shell principal (MainWindow) — layout ERP clássico

> Padrão de ERP desktop brasileiro (estilo Resulth). Sem sidebar — menu de texto + toolbar.

Estrutura `Grid RowDefinitions="Auto,Auto,*,Auto"`:

1. **Menu de texto** (`Menu.topo`, fundo branco): categorias **Cadastros · Movimentos · Financeiro**
   com submenus dos módulos. Bindado a `Categorias` (ObservableCollection<CategoriaMenu>).
2. **Toolbar de atalhos** (`Button.toolbaritem`, 76×64): ícone = Border 32×32 azul radius 3 com
   `<i:Icon>` FontAwesome branco (16px) + legenda 9px (altura fixa 22px = 2 linhas, alinhada ao
   topo). `ItemsControl` horizontal bindado a `Itens`. Botão **Sair** à direita (Border vermelho).
3. **Área central**: marca AutoCar 64px quando `MostrarBoasVindas`; senão placeholder do módulo ativo.
4. **Status bar** (24px, `BrushFundoHeader`): usuário · perfil (esq) | "AutoCar ERP" (centro) |
   data dd/MM/yyyy em FonteMono (dir).

- Toolbar mostra só itens com `ItemMenu.FlgToolbar = true`; menu de texto mostra todos por categoria.
- Item com `Categoria = ""` (ex: "Buscar") aparece só na toolbar, não no menu de texto.
- Menu/toolbar filtrados por perfil: `ItemMenu.VisivelPara(perfil)`. Categoria sem item visível
  não aparece.
- Comando via `$parent[Window].((vm:MainWindowViewModel)DataContext).SelecionarCommand` (menu) e
  `$parent[ItemsControl]...` (toolbar).
- Logout: `SairCommand` → evento `SairSolicitado` → code-behind reabre LoginWindow e fecha o shell.

### Atalhos atuais da toolbar (na ordem)

Buscar · Clientes · Fornecedor · Produtos · Orçamento · Pré-venda · Ordens de Serviço.
(Caixa, Contas a Receber/Pagar, Fiscal, Estoque, Usuários vivem só no menu de texto.)

## Ícones — FontAwesome via Projektanker.Icons.Avalonia

- **Pacotes:** `Projektanker.Icons.Avalonia` + `...FontAwesome` (9.6.2).
- ⚠️ **EXIGE Avalonia 11.** No Avalonia 12 quebra em runtime com
  `MissingMethodException: TemplateBinding.ProvideValue()` ao renderizar o `<i:Icon>`. Foi o motivo
  de o projeto usar Avalonia 11, não 12. Ver [[licao-fontawesome-avalonia]] (memória global).
- **Por que pacote, não path na mão:** desenhar `StreamGeometry` manualmente foi frágil (ícones
  borravam com `PathIcon`, que preenche). FontAwesome traz ícones sólidos testados e nítidos.
  (O pacote proibido pela regra da skill é o `Material.Icons.Avalonia`, que trava — este NÃO é ele.)
- **Registro (uma vez):** `IconProvider.Current.Register<FontAwesomeIconProvider>();` no início de
  `Program.Main`, antes de `BuildAvaloniaApp`. (Na v9 é via IconProvider.Current, não `WithIcons`.)
- **Uso no AXAML:** `xmlns:i="using:Projektanker.Icons.Avalonia"` +
  `<i:Icon Value="fa-solid fa-..." FontSize="16" Foreground=branco/>`.
- `ItemMenu.Icone` guarda o nome FA completo (ex: `"fa-solid fa-box"`).
- **Antes de usar um nome novo:** confirmar que existe e em qual estilo no `icons.json` embutido do
  pacote FontAwesome (todos os atuais são `solid`).
- Mapa atual: Buscar=magnifying-glass · Clientes=user-group · Fornecedor=truck-field · Produtos=box ·
  Orçamento=file-lines · Pré-venda=cart-shopping · OS=screwdriver-wrench · Estoque=boxes-stacked ·
  Caixa=money-bill-wave · C.Receber=arrow-down · C.Pagar=arrow-up · Fiscal=file-invoice-dollar ·
  Usuários=users · Sair=right-from-bracket.

## Notas técnicas (Avalonia 11)

- **`TextBox.Watermark`** é o correto no Avalonia 11 (o `PlaceholderText` é do Avalonia 12 e NÃO
  existe aqui). Atenção: a regra inverte conforme a versão.
- Bindings compilados ligados (`AvaloniaUseCompiledBindingsByDefault=true`) → todo `DataTemplate`
  precisa de `x:DataType`.
- ViewModels que dependem de dados de runtime (ex: `MainWindowViewModel(UsuarioLogado)`) **não** vão
  no DI nem em `Design.DataContext` — instanciar em code-behind.
- `MissingMethodException` ao renderizar um controle de pacote externo = incompatibilidade de versão
  Avalonia (pacote compilado p/ outra major). Validar render real, não só o build.
