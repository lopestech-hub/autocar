# Domínios (colunas `sts_`) — AutoCar ERP

> Referência do significado das colunas de status do banco. **A fonte da verdade são os enums C#**
> em `src/AutoCar.Domain/Enums/` — o EF Core converte enum ↔ int automaticamente. Este documento é só
> uma tradução legível para quem consulta o banco direto. **Ao alterar um enum, atualizar aqui no mesmo commit.**

## Como funciona

- Cada coluna `sts_*` guarda um **int** que corresponde a um valor de um `enum` do domínio.
- Consultar o banco mostra só o número; o significado está no enum (e nesta tabela).
- **Não existe tabela `dominios` no banco** — seria uma segunda cópia dos enums, que divergiria. A
  documentação vive junto do código (versionada).

> ⚠️ Ativo/inativo **não** é `sts_` — é a flag booleana `flg_ativo` (true/false), presente nas tabelas
> mestre (cliente, fornecedor, produto, etc.). Não confundir com as colunas de status abaixo.

## Mapa das colunas `sts_`

| Coluna | Tabela(s) | Enum (`Domain/Enums`) | Valores |
| --- | --- | --- | --- |
| `sts_perfil` | `usuario` | `PerfilUsuario` | 1=Admin · 2=Vendedor · 3=Mecanico · 4=Financeiro |
| `sts_tipo_pessoa` | `cliente`, `fornecedor` | `TipoPessoa` | 1=Fisica · 2=Juridica |
| `sts_unidade` | `produto` | `UnidadeMedida` | 1=UN · 2=PC · 3=CX · 4=JG · 5=PAR · 6=KIT · 7=L · 8=KG · 9=M |
| `sts_posicao` | `produto` | `PosicaoPeca` | 0=NaoAplica · 1=Dianteira · 2=Traseira |
| `sts_combustivel` | `produto_aplicacao` | `Combustivel` | 0=NaoAplica · 1=Flex · 2=Gasolina · 3=Diesel · 4=Etanol · 5=GNV |
| `sts_situacao` | `pre_venda` | `SituacaoPreVenda` | 1=Aberta · 2=Faturada · 3=Cancelada |
| `sts_tipo` | `movimento_estoque` | `TipoMovimentoEstoque` | 1=Entrada · 2=Saida · 3=AjustePositivo · 4=AjusteNegativo |
| `sts_origem` | `movimento_estoque` | `OrigemMovimento` | 1=Manual · 2=Venda · 3=Compra · 4=Devolucao |

## Consulta rápida no banco

Para ver o número bruto de uma coluna e traduzir por esta tabela:

```sql
SELECT cod_produto, descricao, sts_posicao, sts_unidade FROM produto;
-- sts_posicao: 0=NaoAplica, 1=Dianteira, 2=Traseira
-- sts_unidade: 1=UN, 2=PC, ... (ver tabela acima)
```

## Convenção de prefixos (lembrete)

- `sts_` → status/situação (enum persistido como int) — **este documento**
- `flg_` → flag booleana (`flg_ativo`)
- `id_` → UUID (PK/FK) · `cod_` → código legível (int identity)
- `dat_` → data/hora (UTC) · `vlr_` → monetário · `qtd_` → quantidade · `per_` → percentual
