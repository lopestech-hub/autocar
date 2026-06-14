# Módulo Ordem de Serviço (Service)

## Propósito

Documento da **oficina mecânica**: registra o que foi feito num veículo — **peças** aplicadas (que
baixam do estoque, como uma venda) e **mão de obra / serviços** (valor cobrado, sem estoque). Primeiro
módulo da Fase 4. É uma **variação enriquecida do documento Pré-venda** (ver [Sales](../Sales/MODULO.md)):
herda o esqueleto de documento (cabeçalho + itens, total no domínio, janela maximizada separada,
F2=catálogo seletor, baixa de estoque transacional ao faturar) e acrescenta o que é próprio da oficina:
dois tipos de linha, mecânico responsável e um ciclo de status mais rico.

A OS faturada será o gatilho do **título a receber** no Financeiro (Fase 4, etapa seguinte) — o
faturamento emite um gancho (Domain Event) que o Financeiro consumirá depois; aqui só se prepara o ponto
de extensão, sem implementar o Financeiro.

## Tabelas

### `ordem_servico` (mestre — cabeçalho)

- `id_ordem_servico` (uuid PK), `cod_ordem_servico` (int identity — nº da OS)
- `sts_situacao` (int — enum `SituacaoOrdemServico`: Aberta=1, EmAndamento=2, Concluida=3, Faturada=4, Cancelada=5)
- `id_cliente` (uuid FK **opcional** → `cliente`, `Restrict`) — balcão avulso não tem
- `nome_cliente_avulso` (varchar 120) — nome digitado quando sem cliente cadastrado
- `veiculo_montadora`/`veiculo_modelo` (varchar 60), `veiculo_ano` (varchar 9), `veiculo_placa` (varchar 8)
  — **texto livre** (CAIXA ALTA; ano só `Trim`), igual à Pré-venda
- `id_usuario` (FK → `usuario`, `Restrict`) — atendente/vendedor que abriu a OS
- `id_mecanico` (uuid FK **opcional** → `mecanico`, `Restrict`) — mecânico responsável. **Mecânico é
  cadastro próprio, NÃO usuário do sistema** (ver [Registrations](../Registrations/MODULO.md)). A 4.2
  modelou isto como `id_usuario_mecanico`→`usuario`; a 4.1b corrigiu (migration `CadastroMecanico`, rename).
- `qtd_km` (int, **opcional**) — quilometragem do veículo no momento da OS (migration `KmOrdemServico`, aditiva)
- `vlr_desconto`, `vlr_total` (decimal 10,2 — total persistido, calculado no domínio)
- `observacao` (varchar 255)
- `flg_ativo`, `dat_criacao`, `dat_atualizacao` (UTC). **Sem `xmin`** (coleção filha no mesmo save).
- Índices: `ix_os_cod` (único), `ix_os_cliente`, `ix_os_situacao`, `ix_os_mecanico`, `ix_os_data`.

### `ordem_servico_item` (filho — linha única para peça E serviço)

- `id_ordem_servico_item` (uuid PK) — sem `cod_` (registro filho)
- `id_ordem_servico` (FK → `ordem_servico`, **Cascade**)
- `sts_tipo_item` (int — enum `TipoItemOrdemServico`: Peca=1, Servico=2) — **discriminador da linha**
- `id_produto` (uuid FK **opcional** → `produto`, `Restrict`) — preenchido só quando Peca
- `id_servico` (uuid FK **opcional** → `servico`, `Restrict`) — preenchido só quando Servico
- `descricao_item` (varchar 160) — **snapshot** (descrição do produto OU do serviço na inclusão)
- `qtd` (int), `vlr_unitario` (snapshot editável), `vlr_desconto`, `vlr_total_item` (decimal 10,2)
- Índice: `ix_os_item_os` (FK). Migration: `CadastroOrdemServico`.

> **Invariante (no domínio):** Peca ⇒ `id_produto` preenchido e `id_servico` nulo; Servico ⇒ o inverso.
> As duas FKs são opcionais no banco; o agregado garante que exatamente uma esteja setada por tipo.

### Catálogo de serviços (`servico`)

Tabela mestre auxiliar — ver [Registrations](../Registrations/MODULO.md), seção **Serviço**. Migration `CatalogoServico`.

### Enums (Domain/Enums — ver [docs/dominios.md](../../../../../docs/dominios.md))

- `SituacaoOrdemServico` (novo): Aberta=1 · EmAndamento=2 · Concluida=3 · Faturada=4 · Cancelada=5
- `TipoItemOrdemServico` (novo): Peca=1 · Servico=2
- `OrigemMovimento` (estendido): **OrdemServico=5** — baixa de estoque gerada pelo faturamento da OS

## Telas (Views) e ViewModels

- `OrdensServicoView` + `OrdensServicoViewModel` — **listagem** (Grid único): Nº · DATA · CLIENTE ·
  **VEÍCULO** (modelo + placa; sem placa só o modelo) · ITENS · TOTAL · SITUAÇÃO (badge com borda colorida
  pelas **5 situações**). Busca por cliente/número/placa (debounce), contador. "Nova"/duplo-clique **abrem
  janela separada** (evento `AbrirJanelaSolicitado`).
- `OrdemServicoWindow` — **janela maximizada não-modal** (shell segue acessível). Hospeda o form.
  **F2** = catálogo de peça (`CatalogoSeletorWindow`), **F3** = seletor de cliente (`ClienteSeletorWindow`),
  **F4** = seletor de serviço (`ServicoSeletorWindow`), **F5** = seletor de mecânico (`MecanicoSeletorWindow`).
  Orquestra também a `ConfirmacaoWindow` ao cancelar a OS. Fecha ao Salvar/Cancelar.
- `OrdemServicoFormView` + `OrdemServicoFormViewModel` — form dois modos: DADOS (cliente opcional [seletor F3] +
  **mecânico** [seletor F5] + veículo livre + **KM** + atendente read-only) · ITENS (**um grid** com coluna TIPO
  distinguindo peça/serviço, mini-badge azul/índigo; F2 add peça, F4 add serviço) · TOTAIS (subtotal peças +
  subtotal serviços + subtotal itens + desconto + TOTAL em faixa). Rodapé: ação destrutiva **"Cancelar OS
  (encerrar)"** (vermelha, isolada, com confirmação) ≠ **"Fechar"** (só fecha a janela); botões de ciclo
  Iniciar/Concluir/Faturar/Editar conforme o status.
- `OrdemServicoItemViewModel` — linha do grid (tipo, **código** + **referência** read-only da peça, descrição,
  qtd **inteira**, unitário, desconto, total recalcula). CÓDIGO (`cod_produto`) e REFERÊNCIA (`cod_fabricante`)
  vêm do produto: na adição via `CatalogoItemDto`; ao reabrir, enriquecidos por `IProdutoRepository.ObterCodigosPorIdsAsync`
  (1 query batch, sem N+1) no `MapearAsync` do service. Serviço não tem código/referência (células vazias).
- `ServicoSeletorWindow` (F4) e `MecanicoSeletorWindow` (F5) — janelas seletoras (molde do `ClienteSeletorWindow`:
  busca por descrição/nome ou código, setas, Enter/duplo-clique seleciona, Esc fecha). O serviço traz `vlr_padrao`
  como snapshot; o seletor de mecânico tem botão **"Sem mecânico"** (limpa). Padrão "Seletor" do system.md.

> **Mecânico vira botão-seletor por janela** (não combo). O planejamento previa combo "por volume baixo", mas o
> Julio pediu o mesmo padrão do cliente (F5) — mais consistente e evita o gotcha do ComboBox no FluentTheme.
> Reusa `CatalogoSeletorWindow`, `ClienteSeletorWindow` e `ConfirmacaoWindow` (cancelar; faturar virá na 4.4).

## Regras de Negócio

- **Cliente opcional** (balcão avulso): ou `id_cliente`, ou `nome_cliente_avulso` ("CONSUMIDOR" na listagem).
- **Veículo texto livre** (montadora/modelo/ano/placa), igual à Pré-venda e à aplicação do Produto.
- **Dois tipos de linha** num só grid: Peça (FK produto, baixa estoque) e Serviço (FK serviço, sem estoque).
  O snapshot copia descrição + valor (do `vlr_venda` do produto ou `vlr_padrao` do serviço); unitário editável.
- **Total no domínio** (a UI nunca calcula): item = `qtd × unitário − desconto` (≥0); documento =
  `Σ itens − desconto geral` (≥0). Desconto geral ajusta para baixo se exceder o subtotal. Mesma regra da Pré-venda.
- **Ciclo de status:** nasce **Aberta** (editável). **Iniciar** → EmAndamento. **Concluir** → Concluida
  (exige **mecânico responsável** definido e ≥1 item). **Faturar** → Faturada (baixa estoque das peças,
  torna imutável). **Cancelar** encerra (só antes de faturar; não mexe em estoque). Documento Faturado/
  Cancelado não aceita mais alterações (invariante no agregado).
- **Mecânico:** opcional ao abrir/editar; **obrigatório para Concluir** (não se conclui trabalho sem responsável).
- **Quilometragem (`qtd_km`):** opcional; registra a km do veículo na OS (não-negativa, validada no domínio).
  Dado de oficina para histórico/revisões por km. Não aparece na listagem (só no form).
- **Faturar baixa estoque** (✅ **4.4**): ao faturar, **cada linha do tipo Peça** gera uma **Saída** no
  estoque, origem `OrdemServico` ("OS nº X"), numa **transação única atômica** (`FaturamentoOrdemServicoRepository`,
  espelha o `FaturamentoRepository`). Linhas de Serviço não tocam estoque. Falha se alguma peça não tiver saldo
  (a OS continua Concluída, rollback total). `xmin` do saldo protege concorrência → `Error.Conflito`. UI: botão
  Faturar (só em OS Concluída) → `ConfirmacaoWindow` → efetiva; badge **FATURADA** e a listagem recarrega.

## Cálculos

- `SubtotalItens` = Σ `vlr_total_item` (peças + serviços juntos).
- `VlrTotal` = `SubtotalItens − VlrDesconto` (≥ 0; desconto nunca excede o subtotal).
- Subtotais por tipo (peças / serviços) são exibidos na faixa de TOTAIS — derivados, não persistidos.

## Decisões Técnicas

- **Linha única com discriminador** (`sts_tipo_item`) em vez de duas tabelas — espelha `pre_venda_item`,
  reusa o grid/snapshot/cálculo num só lugar; o total soma peça+serviço naturalmente. FKs opcionais; a
  invariante peça×serviço mora no construtor do item (Domain) + validator (Application).
- **Sem `xmin`** no agregado — coleção filha (itens) editada no mesmo `SaveChanges` faz o UPDATE do pai
  afetar 0 linhas. Mesmo padrão de Produto/PreVenda. Ver lição global de EF Core + Npgsql.
- **`IDbContextFactory` + `State=Added`** nos itens novos — PK gerada no cliente faria o EF inferir
  `Modified` → UPDATE em linha inexistente. Mesma lição da Pré-venda e da aplicação por veículo do Produto.
- **Baixa ao faturar (não ao concluir)** — alinha com a Pré-venda: baixa = cobrança. Concluída é só "trabalho
  pronto"; o estoque sai quando vira dinheiro. Evita o intervalo "concluída-sem-faturar com estoque baixado"
  e a questão de estorno ao cancelar pós-conclusão (ver Evolução no CONTEXTO.md).
- **`FaturamentoOrdemServicoRepository`** — transação atômica cross-agregado OS + Estoque (espelha
  `FaturamentoRepository`): abre UM `DbContext`, fatura a OS e baixa só as linhas Peça via `EstoquePersistencia`
  (origem `OrdemServico`), num único `SaveChanges` (tudo ou nada). Reusa `QuantidadeEstoque.DeDocumento`.
- **Janela separada (não embutida)** — a ViewModel dispara evento; a View abre a `OrdemServicoWindow`
  não-modal. Montada à mão no `Navegador` (depende do `UsuarioLogado`, runtime), como a Pré-venda.
- **Gancho Financeiro = Domain Event in-process** (`OrdemServicoFaturada`) — só o ponto de extensão na 4.5;
  N=0 consumidores hoje. Sem fila/MediatR distribuído. O Financeiro pluga o handler quando nascer.

## Dependências

- Depende de: Cadastros (Cliente, Produto/Catálogo, **Serviço**), Security (atendente + mecânico = usuários),
  Shared (`Result<T>`), e **Estoque** (baixa ao faturar — `FaturamentoOrdemServicoRepository` orquestra os dois).
- Será base de: **Financeiro** (a OS faturada gera o título a receber — gancho previsto na 4.5).
