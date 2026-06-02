using AutoCar.Domain.Common;
using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>
/// Helpers de persistência de estoque compartilhados entre os repositórios que movimentam saldo
/// (<see cref="EstoqueRepository"/> avulso e <see cref="FaturamentoRepository"/> transacional). Mantém
/// numa fonte única duas invariantes de infraestrutura que, repetidas, divergiriam com o tempo:
/// (1) saldo nasce zerado quando o produto ainda não tem registro; (2) conflito de concorrência otimista
/// (xmin) vira <see cref="ConcorrenciaException"/>. Recebem o DbContext já criado — cada repositório
/// continua dono do seu contexto/transação (a decisão de ciclo de vida não é compartilhada).
/// </summary>
internal static class EstoquePersistencia
{
    /// <summary>Carrega o saldo RASTREADO do produto (o xmin lido aqui é o comparado no UPDATE). Se ainda
    /// não existe registro de saldo, cria zerado e marca como Added.</summary>
    public static async Task<EstoqueProduto> ObterOuCriarSaldoRastreadoAsync(
        AppDbContext db, Guid idProduto, CancellationToken ct)
    {
        var estoque = await db.EstoqueProdutos.FirstOrDefaultAsync(e => e.IdProduto == idProduto, ct);
        if (estoque is null)
        {
            estoque = new EstoqueProduto(idProduto);
            await db.EstoqueProdutos.AddAsync(estoque, ct);
        }
        return estoque;
    }

    /// <summary>Salva traduzindo a colisão de concorrência otimista (xmin) numa exceção neutra de domínio —
    /// a Application trata sem conhecer o EF Core (Clean Architecture).</summary>
    public static async Task SalvarTraduzindoConcorrenciaAsync(
        AppDbContext db, string mensagemConflito, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcorrenciaException(mensagemConflito, ex);
        }
    }
}
