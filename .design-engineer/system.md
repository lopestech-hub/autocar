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
| `BrushBorda` | #CBD5E1 | borda padrão |
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
- `Button.toolbaritem` — atalho da toolbar do shell (ícone grande + legenda, 76×64, alinhado ao topo)
- `Menu.topo` / `Menu.topo > MenuItem` — menu superior de texto
- `TextBox.login` — input de 28px da tela de login

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
- **Seções** com `TextBlock.formsecao` (ex: "ENDEREÇO"), `ColumnSpan` total, margem `0,8,0,2`.
- Header: título + badge VISUALIZAÇÃO/EDIÇÃO. Rodapé dois modos (Fechar/Editar ↔ Cancelar/Salvar).
- Classes novas no Tema: `TextBlock.formlabel` (label à direita) e `TextBlock.formsecao` (título de seção).
- Referência viva: `Views/Registrations/ClienteFormView.axaml`.

### Janela de Login (LoginWindow)

- 380×440, `CanResize=False`, centralizada. Card branco central sobre fundo cinza.
- Marca → **Usuário** → Senha (PasswordChar="•") → botão Entrar (`IsDefault=True`, Enter dispara)
  → ProgressBar 2px (loading) → mensagem de erro 11px vermelha.
- Login é por **nome de usuário** (campo `Login`), não e-mail.
- Padrão de botão assíncrono: `Carregando` + `MensagemErro`, `IsEnabled="{Binding !Carregando}"`.

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
