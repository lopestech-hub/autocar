# Módulo Segurança (Security)

## Propósito

Autenticação e controle de acesso do AutoCar ERP. Provê login por **nome de usuário** + senha
(hash BCrypt), sessão do usuário logado e filtragem da navegação por perfil. No MVP cada usuário
tem exatamente **um** perfil, fixo via enum.

## Tabelas

- `usuario` — tabela mestre.
  - `id_usuario` (uuid, PK), `cod_usuario` (int identity, código legível)
  - `nome`, `usuario` (login único, normalizado lowercase), `senha_hash` (BCrypt)
  - `sts_perfil` (int — enum PerfilUsuario), `flg_ativo`
  - `dat_criacao`, `dat_atualizacao` (timestamptz, gravados em UTC)
  - `xmin` (system column) — token de concorrência otimista
  - Índices únicos: `ix_usuario_login` (coluna `usuario`), `ix_usuario_cod`

## Telas (Views) e ViewModels

- `LoginWindow.axaml` + `LoginViewModel` — janela dedicada de login (campo Usuário + Senha).
  Valida credenciais, mostra loading/erro, dispara `LoginConcluido(UsuarioLogado)` → abre a MainWindow.
- `MainWindow.axaml` + `MainWindowViewModel` — shell ERP (menu de texto + toolbar de atalhos +
  área central + status bar). Monta menu/toolbar conforme o perfil (itens sem permissão ocultos).
- `ItemMenu` — item de módulo (Titulo, Icone FontAwesome, Categoria, FlgToolbar, `VisivelPara`).
- `CategoriaMenu` — agrupa itens por categoria no menu de texto.

## Camadas (Clean Architecture)

- Domain: `Usuario` (entidade, com `Login`), `PerfilUsuario` (enum), `IUsuarioRepository`, `IHashSenha`.
- Application: `IAutenticacaoService` + `AutenticacaoService`, DTO `UsuarioLogado`.
- Infrastructure: `UsuarioRepository`, `HashSenhaBCrypt`, `UsuarioConfiguration` (Fluent API),
  `DbInitializer` (migrations + seed admin).

## Regras de Negócio

- Senha sempre armazenada como hash BCrypt — nunca texto claro, nunca em log.
- Mensagem de login inválido é genérica ("Usuário ou senha inválidos") — não revela se existe.
- Usuário inativo (`flg_ativo=false`) não autentica.
- Login (`Login`) normalizado (trim + lowercase) na entidade — chave natural de autenticação.
- Perfis (ver navegação completa no CONTEXTO.MD): Admin vê tudo; Vendedor (cadastros + vendas);
  Mecanico (OS + cadastros básicos); Financeiro (caixa + contas + fiscal).
- Seed inicial: usuário admin **`julio`** / senha `123` (trocar no uso real).

## Decisões Técnicas

- **1 perfil por usuário via enum** (não tabela `perfil` nem N:N) — simplicidade no MVP;
  vira N:N depois se necessário.
- **Seed do admin no `DbInitializer`** (runtime, idempotente) em vez de `HasData` na migration —
  permite usar BCrypt e evita hash hardcoded.
- **`xmin` como concurrency token** — padrão estabelecido aqui, reaproveitado no estoque.

## Dependências

- Nenhuma (módulo base). Os demais módulos dependem do usuário logado para rastreabilidade.
