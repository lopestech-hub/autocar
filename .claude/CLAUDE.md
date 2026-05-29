# AutoCar ERP — Regras Específicas

> Regras que valem APENAS para este projeto. As regras globais do NEXUS continuam valendo.

## Banco de Dados
- MCP a usar: `autocar-db`
- Localização: local/LAN — PostgreSQL em localhost:5432
- Banco: `autocar` · usuário dev: `postgres` / senha dev: `postgres`
- Tabelas: snake_case em português — prefixos id_, cod_, dat_, vlr_, qtd_, sts_, flg_, per_
- ORM: EF Core 9 — migrations via `dotnet ef` (projeto e startup = AutoCar.Infrastructure)
- Migrations: `InicialSecurity` (cria `usuario`) → `LoginPorUsuario` (coluna `email`→`usuario`)

## Stack
- UI: Avalonia UI **11.3.17** + MVVM (CommunityToolkit.Mvvm)
- Backend: C# 13 / .NET 9 + EF Core 9
- Arquitetura: Clean Architecture + DDD (módulos em AutoCar.Application/Modules)
- Libs: FluentValidation, Mapster, MediatR, BCrypt.Net-Next, Serilog, Npgsql,
  Projektanker.Icons.Avalonia + FontAwesome (9.6.2)

## Gotchas Conhecidos
- **Avalonia é a 11.3.17, NÃO a 12.** O projeto começou na 12.0.4 (template), mas baixou para 11
  porque o pacote de ícones FontAwesome (Projektanker) quebra em runtime no Avalonia 12 com
  `MissingMethodException: TemplateBinding.ProvideValue()`. Manter na linha 11.x.
- **`TextBox.Watermark`** é o correto no Avalonia 11 (`PlaceholderText` é do 12 e NÃO existe aqui).
- **Ícones FontAwesome:** registrar `IconProvider.Current.Register<FontAwesomeIconProvider>()` em
  `Program.Main` antes de `BuildAvaloniaApp`. Usar `<i:Icon Value="fa-solid fa-..."/>`. Conferir o
  nome no `icons.json` do pacote antes de usar.
- **Datas: persistir em UTC, exibir em Brasília.** PostgreSQL `timestamptz` só aceita
  `DateTime` com `Kind=Utc`. Usar `DataHora.AgoraUtc()` para gravar e `DataHora.ParaBrasilia()`
  para exibir. NUNCA gravar `DateTime` com `Kind=Unspecified`.
- **Não bloquear o thread de UI (STA) com `.GetAwaiter().GetResult()` em código async** —
  causa deadlock no Avalonia. Inicialização de banco roda em `Program.Main` (síncrona),
  ANTES de subir a UI.
- **Concorrência otimista via `xmin`**: mapeado como propriedade sombra `uint xmin` com
  `IsRowVersion()` (o método `UseXminAsConcurrencyToken` NÃO existe no Npgsql 9). Padrão a
  reaproveitar no estoque.
- **ViewModels que dependem de dados de runtime** (ex: `MainWindowViewModel(UsuarioLogado)`)
  não vão no DI nem em `Design.DataContext` — instanciar em code-behind.

## Bootstrap / Execução
- Composition root: `AutoCar.Desktop/Bootstrap.cs` (DI + Serilog + config).
- `Program.Main` monta serviços → inicializa banco (`DbInitializer.Inicializar`) → sobe Avalonia.
- App inicia na `LoginWindow`; ao autenticar, abre `MainWindow` com o usuário logado.
- Login é por **nome de usuário** (campo `Login`, normalizado lowercase), não e-mail.
- Seed: usuário admin padrão **`julio`** / senha `123` (idempotente, trocar no uso real).

## Deploy
- Publicação: `dotnet publish -c Release -r win-x64 --self-contained`
- Banco: PostgreSQL central no servidor da loja (LAN); `dotnet ef database update` no destino
- Backup: `pg_dump` diário só no servidor + no-break recomendado
- Atualização: manual (copiar executável) no MVP
