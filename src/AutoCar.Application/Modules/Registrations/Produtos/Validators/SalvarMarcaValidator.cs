using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Produtos.Validators;

/// <summary>Validação de borda do cadastro de marca. Unicidade é checada no serviço.</summary>
public sealed class SalvarMarcaValidator : AbstractValidator<SalvarMarcaDto>
{
    public SalvarMarcaValidator()
    {
        RuleFor(m => m.Descricao)
            .NotEmpty().WithMessage("Informe a descrição da marca.")
            .MaximumLength(80);
    }
}
