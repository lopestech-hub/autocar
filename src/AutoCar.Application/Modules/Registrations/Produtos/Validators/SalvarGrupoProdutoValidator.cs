using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Produtos.Validators;

/// <summary>Validação de borda do cadastro de grupo. Unicidade e existência da categoria são checadas no serviço.</summary>
public sealed class SalvarGrupoProdutoValidator : AbstractValidator<SalvarGrupoProdutoDto>
{
    public SalvarGrupoProdutoValidator()
    {
        RuleFor(g => g.Descricao)
            .NotEmpty().WithMessage("Informe a descrição do grupo.")
            .MaximumLength(80);

        RuleFor(g => g.IdCategoria)
            .NotEmpty().WithMessage("Selecione a categoria do grupo.");
    }
}
