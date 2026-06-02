namespace AutoCar.Domain.Common;

/// <summary>
/// Lançada por um repositório quando um registro exigido por uma operação não existe (ex: faturar uma
/// pré-venda inexistente). Exceção neutra de domínio — a Application a traduz para um resultado de
/// "não encontrado" sem precisar de uma consulta prévia só para checar a existência.
/// </summary>
public sealed class NaoEncontradoException : Exception
{
    public NaoEncontradoException(string mensagem) : base(mensagem) { }
}
