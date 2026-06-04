using AutoCar.Application.Modules.Registrations.Mecanicos.DTOs;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Mecanicos.Validators;

/// <summary>Validação de borda do cadastro de mecânico. Unicidade do nome é checada no serviço.</summary>
public sealed class SalvarMecanicoValidator : AbstractValidator<SalvarMecanicoDto>
{
    public SalvarMecanicoValidator()
    {
        RuleFor(m => m.Nome)
            .NotEmpty().WithMessage("Informe o nome do mecânico.")
            .MaximumLength(120);

        RuleFor(m => m.Telefone)
            .MaximumLength(20);
    }
}
