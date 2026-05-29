namespace AutoCar.Domain.Enums;

/// <summary>
/// Tipo de pessoa de um cliente/fornecedor. Define se o documento é CPF ou CNPJ
/// e quais campos de identificação se aplicam.
/// </summary>
public enum TipoPessoa
{
    /// <summary>Pessoa física — documento CPF.</summary>
    Fisica = 1,

    /// <summary>Pessoa jurídica — documento CNPJ.</summary>
    Juridica = 2,
}
