namespace AutoCar.Domain.Common;

/// <summary>
/// Lançada quando uma operação com concorrência otimista falha porque o registro foi alterado por
/// outro processo entre a leitura e a gravação (ex: dois terminais movimentando o mesmo saldo de
/// estoque ao mesmo tempo). Exceção neutra de domínio — a Infrastructure traduz o erro específico do
/// ORM (DbUpdateConcurrencyException) para esta, mantendo a Application livre de dependência do EF Core.
/// </summary>
public sealed class ConcorrenciaException : Exception
{
    public ConcorrenciaException(string mensagem) : base(mensagem) { }

    public ConcorrenciaException(string mensagem, Exception innerException)
        : base(mensagem, innerException) { }
}
