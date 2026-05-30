---
name: novo-cadastro
description: Gera o esqueleto completo de um cadastro mestre do AutoCar ERP (Clean Architecture + Avalonia), seguindo o padrão de Cliente/Fornecedor/Marca/Categoria. Use quando o usuário pedir para criar um novo cadastro/módulo de cadastro (ex: "/novo-cadastro Produto"). Cria entidade, repositório, service, DTOs, validator, configuration EF, ViewModels e Views nas 5 camadas.
disable-model-invocation: true
---

# Novo Cadastro — Scaffold de cadastro mestre (AutoCar ERP)

Gera as 5 camadas de um cadastro mestre seguindo EXATAMENTE o padrão já estabelecido no projeto.
Use os cadastros existentes como referência viva — leia-os antes de gerar.

## Antes de começar

1. **Pergunte/confirme** com o usuário: nome da entidade (singular), campos (nome, tipo,
   obrigatório, tamanho), e se entra na toolbar ou só no menu de texto + perfis que veem.
2. **Leia como referência** o cadastro mais próximo já pronto:
   - Simples (só descrição): `Marca` / `CategoriaProduto`.
   - Completo (com Documento/Endereço/PF-PJ): `Cliente` / `Fornecedor`.
3. **Leia** `src/AutoCar.Desktop/Views/Registrations/MODULO.md` para o padrão do módulo.

## Arquivos a gerar (nesta ordem)

### Domain (`src/AutoCar.Domain`)
- `Entities/<Entidade>.cs` — entidade rica herdando `EntidadeBase`; construtor protegido p/ EF +
  construtor público; `AlterarDados`, `Ativar`, `Inativar`. `cod_<entidade>` int identity.
  Campos de nome/descrição/endereço normalizados com `.Trim().ToUpperInvariant()` (CAIXA ALTA);
  e-mail `.ToLowerInvariant()`; observação só `.Trim()`.
- `Interfaces/I<Entidade>Repository.cs` — ObterPorId, Listar(filtro), Adicionar, Atualizar,
  ExisteDescricao/Documento (unicidade), Salvar.

### Application (`src/AutoCar.Application/Modules/Registrations/<Modulo>`)
- `DTOs/<Entidade>Dtos.cs` — Salvar<E>Dto, <E>Dto, (e <E>ListaDto se a lista for enxuta).
- `I<Entidade>Service.cs` + `<Entidade>Service.cs` — CRUD com `Result<T>`, valida via
  FluentValidation, checa unicidade (case-insensitive via repositório), mapeia entidade→DTO.
- `Validators/Salvar<Entidade>Validator.cs` — regras de borda (NotEmpty, MaxLength).
- Registrar service + validator em `Application/DependencyInjection.cs`.

### Infrastructure (`src/AutoCar.Infrastructure`)
- `Persistence/Configurations/<Entidade>Configuration.cs` — tabela snake_case PT, colunas com
  prefixos (id_, cod_, dat_, flg_, vlr_...), índices únicos (cod + chave natural), e o bloco
  `xmin` (concorrência otimista — copiar de Cliente).
- `Persistence/Repositories/<Entidade>Repository.cs` — EF Core; Listar com `AsNoTracking` +
  `EF.Functions.ILike` no filtro; unicidade com ILike (case-insensitive).
- Adicionar `DbSet<<Entidade>>` em `Persistence/AppDbContext.cs`.
- Registrar repositório em `Infrastructure/DependencyInjection.cs`.

### Desktop (`src/AutoCar.Desktop`)
- `ViewModels/Registrations/<Entidade>sViewModel.cs` — listagem com busca debounce 350ms,
  contador, form sobreposto (eventos Salvo/Cancelado).
- `ViewModels/Registrations/<Entidade>FormViewModel.cs` — dois modos (ModoVisualizacao),
  PrepararNovo/CarregarAsync/Salvar/Cancelar.
- `Views/Registrations/<Entidade>sView.axaml` + `.axaml.cs` — listagem Grid único via
  code-behind (copiar de MarcasView: header + zebra + hover + duplo-clique).
- `Views/Registrations/<Entidade>FormView.axaml` + `.axaml.cs` — form denso dois modos.
  Campos de nome/endereço com `b:MaiusculoBehavior.Ativo="True"`. `IsEnabled="{Binding !ModoVisualizacao}"`.
- Registrar os 2 ViewModels em `Bootstrap.cs`; rota em `Navegacao/Navegador.cs`; item de menu
  em `MainWindowViewModel.cs` (com perfis e rota).

## Depois de gerar

1. `dotnet build` (0 erros) — corrigir antes de seguir.
2. `dotnet ef migrations add Cadastro<Entidade>` (projeto+startup = AutoCar.Infrastructure).
3. **Revisar a migration** — confirmar que é aditiva (CreateTable, sem DROP/alteração nas
   tabelas existentes) antes de aplicar.
4. `dotnet ef database update` e validar a tabela no banco via MCP `autocar-db`.
5. Avisar o usuário para testar na tela (você não vê a UI) e propor atualizar MODULO.md/CONTEXTO.

## Regras inegociáveis

- Não inventar campos/regras — confirmar com o usuário.
- Migrations sempre aditivas (nunca DROP+CREATE em tabela existente).
- Datas em UTC (`DataHora.AgoraUtc` via EntidadeBase); exibição em Brasília.
- Nunca chamar subagente para isto (depende de contexto da sessão e MCP).
