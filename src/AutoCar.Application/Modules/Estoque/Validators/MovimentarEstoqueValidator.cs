using AutoCar.Application.Modules.Estoque.DTOs;
using FluentValidation;

namespace AutoCar.Application.Modules.Estoque.Validators;

/// <summary>Validação de borda da movimentação de estoque. Exige produto e quantidade positiva;
/// a regra de saldo não-negativo é garantida no domínio (invariante de <c>EstoqueProduto</c>).</summary>
public sealed class MovimentarEstoqueValidator : AbstractValidator<MovimentarEstoqueDto>
{
    public MovimentarEstoqueValidator()
    {
        RuleFor(m => m.IdProduto)
            .NotEmpty().WithMessage("Selecione um produto.");

        RuleFor(m => m.Tipo)
            .IsInEnum().WithMessage("Tipo de movimento inválido.");

        RuleFor(m => m.Qtd)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");

        RuleFor(m => m.Observacao)
            .MaximumLength(255);
    }
}
