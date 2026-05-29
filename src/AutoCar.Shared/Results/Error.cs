namespace AutoCar.Shared.Results;

/// <summary>
/// Representa um erro de domínio/aplicação com código e mensagem.
/// Usado dentro de <see cref="Result"/> e <see cref="Result{T}"/> para
/// propagar falhas sem lançar exceções no fluxo normal.
/// </summary>
public sealed record Error(string Codigo, string Mensagem)
{
    /// <summary>Ausência de erro — usado internamente por resultados de sucesso.</summary>
    public static readonly Error Nenhum = new(string.Empty, string.Empty);

    /// <summary>Erro genérico de validação.</summary>
    public static Error Validacao(string mensagem) => new("validacao", mensagem);

    /// <summary>Recurso não encontrado.</summary>
    public static Error NaoEncontrado(string mensagem) => new("nao_encontrado", mensagem);

    /// <summary>Conflito (ex: concorrência, duplicidade).</summary>
    public static Error Conflito(string mensagem) => new("conflito", mensagem);

    /// <summary>Falha de autenticação/autorização.</summary>
    public static Error NaoAutorizado(string mensagem) => new("nao_autorizado", mensagem);
}
