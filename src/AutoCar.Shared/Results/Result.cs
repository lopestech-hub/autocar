namespace AutoCar.Shared.Results;

/// <summary>
/// Resultado de uma operação sem valor de retorno. Carrega sucesso/falha
/// e o <see cref="Error"/> associado. Evita exceções no fluxo de negócio.
/// </summary>
public class Result
{
    protected Result(bool sucesso, Error erro)
    {
        if (sucesso && erro != Error.Nenhum)
            throw new InvalidOperationException("Resultado de sucesso não pode ter erro.");
        if (!sucesso && erro == Error.Nenhum)
            throw new InvalidOperationException("Resultado de falha precisa de um erro.");

        Sucesso = sucesso;
        Erro = erro;
    }

    public bool Sucesso { get; }
    public bool Falha => !Sucesso;
    public Error Erro { get; }

    public static Result Ok() => new(true, Error.Nenhum);
    public static Result Falhar(Error erro) => new(false, erro);

    public static Result<T> Ok<T>(T valor) => Result<T>.Ok(valor);
    public static Result<T> Falhar<T>(Error erro) => Result<T>.Falhar(erro);
}

/// <summary>
/// Resultado de uma operação que retorna um valor em caso de sucesso.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _valor;

    private Result(T? valor, bool sucesso, Error erro) : base(sucesso, erro)
        => _valor = valor;

    /// <summary>Valor produzido. Acessar apenas quando <see cref="Result.Sucesso"/> for verdadeiro.</summary>
    public T Valor => Sucesso
        ? _valor!
        : throw new InvalidOperationException("Não há valor em um resultado de falha.");

    public static Result<T> Ok(T valor) => new(valor, true, Error.Nenhum);
    public new static Result<T> Falhar(Error erro) => new(default, false, erro);
}
