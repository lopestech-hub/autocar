# Checklist de Auditoria Visual — AutoCar ERP

> Auditoria de consistência tela por tela contra o `system.md` + `Tema.axaml`.
> Iniciada em 2026-06-19. Cada tela recebe dois passes: **auditoria de código** (Claude lê o
> `.axaml`) + **validação visual** (Julio olha a tela rodando). Desvios → Luna aplica.
>
> Legenda de status: ⬜ pendente · 🔍 auditada (aguarda validação visual) · 🔧 ajustar · ✅ ok

## Critérios universais

### A. Cor & Paleta Cofap
- Zero cor hardcoded que já é token; azul = `#1E5CA5` (nunca `#3B82F6` do template)
- Header de tabela = faixa azul + texto branco (`th`)
- Linha selecionada = laranja-claro `#FCD9C2` + barra lateral 3px laranja
- Status bar = faixa azul + texto branco
- Cor só comunica significado (badge, total, âmbar de desconto)

### B. Densidade
- Campos (TextBox/ComboBox/Button.seletor) = 24px
- Botões de ação (`primario`/`secundario`/`faturar`) = 26px · Abas = 30px
- Sem padding inflado / sem cara de IA

### C. Tipografia
- Labels = preto `#000000` + Medium (`formlabel`)
- Legenda toolbar / menu de texto = preto Medium
- Dados/códigos/valores = Consolas (`FonteMono`)
- Título de seção = azul SemiBold (`formsecao`) + linha divisória

### D. Componentes (reusar classe, não reinventar)
- Listagem = `ListBox.tabela` virtualizado
- Contador = `Border.contador` (cantos retos)
- Buscar registro relacionado = `Button.seletor` + janela (nunca `AutoCompleteBox`)
- Botões usam classes do Tema (nunca `Classes="toolbar"`)

### E. Interação
- `Cursor="Hand"` em tudo clicável
- Lista: clique marca / hover passageiro / seleção vence hover / Enter ou duplo-clique = ação
- Janela-documento: ESC fecha · F2/F3/F4/F5 corretos
- Modo visualização = `IsEnabled=false`, nunca `IsReadOnly`

### F. Conteúdo
- Zero traço longo (—) em texto visível
- Zero emoji/pictograma (só `✓`)
- Nomes/endereços em CAIXA ALTA automática
- Microcopy de atalho usa `=`, nunca traço

---

## Progresso por tela

### 1. Shell
- ✅ MainWindow — conforme (tokens Cofap, status bar azul+branco, marca Auto azul/Car laranja)
- ✅ LoginWindow — marca Cofap + ERP/USUÁRIO/SENHA em preto Medium. **Validado visualmente 2026-06-19**

### 2. Listagens
> Varredura de código (2026-06-19): todas as 15 usam `ListBox.tabela` virtualizado e zero azul
> antigo hardcoded. 14/15 usam `Border.contador` padrão. Falta validação visual de cada uma.
- 🔍 ClientesView — conforme no código
- 🔍 FornecedoresView — conforme no código
- 🔧 ProdutosView — removido "—" de Grupo/Posição/Lado (célula vazia). Aguarda validação visual
- 🔍 MarcasView — conforme no código
- 🔍 CategoriasView — conforme no código
- 🔍 GruposView — conforme no código
- 🔍 PosicoesView — conforme no código
- 🔍 LadosView — conforme no código
- 🔍 ServicosView — conforme no código
- 🔍 MecanicosView — conforme no código
- 🔍 PreVendasView — conforme no código
- 🔍 ComprasView — conforme no código
- 🔍 EstoqueView — conforme no código
- 🔍 OrdensServicoView — conforme no código
- ✅ CatalogoView — contador alinhado ao padrão laranja (Border.contador), igual às demais

### 3. Formulários
- ⬜ ClienteFormView
- ⬜ FornecedorFormView
- ⬜ ProdutoFormView
- ⬜ MarcaFormView
- ⬜ CategoriaFormView
- ⬜ GrupoFormView
- ⬜ PosicaoFormView
- ⬜ LadoFormView
- ⬜ ServicoFormView
- ⬜ MecanicoFormView
- ⬜ MovimentoEstoqueFormView

### 4. Janelas-documento
- ⬜ PreVendaWindow / PreVendaFormView
- ⬜ CompraWindow / CompraFormView
- ⬜ OrdemServicoWindow / OrdemServicoFormView
- ⬜ DevolucaoWindow / DevolucaoFormView

### 5. Seletores
- ⬜ ClienteSeletorWindow
- ⬜ FornecedorSeletorWindow
- ⬜ ServicoSeletorWindow
- ⬜ MecanicoSeletorWindow
- ⬜ CatalogoSeletorWindow

### 6. Diálogos
- ⬜ ConfirmacaoWindow
- ⬜ ImagemAmpliadaWindow
- ⬜ MovimentoEstoqueWindow

---

## Achados (preenchido durante a auditoria)

> Cada desvio: tela · item do critério · o que está · o que deveria · status.

| Tela | Critério | Estava | Deveria | Status |
|------|----------|--------|---------|--------|
| LoginWindow | A. Paleta Cofap | Marca Auto=preto, Car=azul (template antigo) | Auto=azul `#1E5CA5`, Car=laranja `#F86518` | ✅ corrigido |
| LoginWindow | C. Tipografia | ERP/USUÁRIO/SENHA em cinza | Preto Medium (padrão legenda) | ✅ corrigido |
| ProdutosView | F. Sem traço | Grupo/Posição/Lado nulos exibiam "—" | Célula em branco | ✅ corrigido |
| Todas as listagens | Identidade | Contador azul-claro + "N produtos" | **Laranja-claro (acento) + só o número** | ✅ aplicado (15 VMs + tokens) |
| CatalogoView | D. Componente | Contador em faixa azul sólida (CofapAzul) | Alinhado ao laranja (Border.contador) | ✅ alinhado |
| system.md (doc) | — | Seção "Direção" diz "Car em azul primário" (defasado) | "Car em laranja acento" | ⬜ corrigir doc depois |
| MovimentoEstoqueFormView | A. Token | Labels com `#475569` hardcoded (8x) | Token de cor | ⬜ grupo 3/4 |
| DevolucaoFormView | A. Token | Labels com `#475569` hardcoded (6x) | Token de cor | ⬜ grupo 3/4 |
| CatalogoSeletorWindow | F. Sem traço | Título "Catálogo — selecionar peça" | Sem traço longo | ⬜ grupo 5 |
| ProdutoFormView / GrupoFormView | F. Sem traço | Combos com `FallbackValue=—` | Vazio | ⬜ grupo 3 |
