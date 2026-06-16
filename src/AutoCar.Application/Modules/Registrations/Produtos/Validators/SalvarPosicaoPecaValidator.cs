using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Produtos.Validators;

/// <summary>Validação de borda do cadastro de posição. Unicidade é checada no serviço.</summary>
public sealed class SalvarPosicaoPecaValidator : AbstractValidator<SalvarPosicaoPecaDto>
{
    public SalvarPosicaoPecaValidator()
    {
        RuleFor(p => p.Descricao)
            .NotEmpty().WithMessage("Informe a descrição da posição.")
            .MaximumLength(80);
    }
}
