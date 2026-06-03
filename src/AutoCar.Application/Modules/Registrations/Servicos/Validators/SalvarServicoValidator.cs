using AutoCar.Application.Modules.Registrations.Servicos.DTOs;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Servicos.Validators;

/// <summary>Validação de borda do cadastro de serviço. Unicidade é checada no serviço.</summary>
public sealed class SalvarServicoValidator : AbstractValidator<SalvarServicoDto>
{
    public SalvarServicoValidator()
    {
        RuleFor(s => s.Descricao)
            .NotEmpty().WithMessage("Informe a descrição do serviço.")
            .MaximumLength(120);

        RuleFor(s => s.VlrPadrao)
            .GreaterThanOrEqualTo(0).WithMessage("O valor padrão não pode ser negativo.");
    }
}
