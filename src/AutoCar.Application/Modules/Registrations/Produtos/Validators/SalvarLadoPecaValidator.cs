using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Produtos.Validators;

/// <summary>Validação de borda do cadastro de lado. Unicidade é checada no serviço.</summary>
public sealed class SalvarLadoPecaValidator : AbstractValidator<SalvarLadoPecaDto>
{
    public SalvarLadoPecaValidator()
    {
        RuleFor(l => l.Descricao)
            .NotEmpty().WithMessage("Informe a descrição do lado.")
            .MaximumLength(80);
    }
}
