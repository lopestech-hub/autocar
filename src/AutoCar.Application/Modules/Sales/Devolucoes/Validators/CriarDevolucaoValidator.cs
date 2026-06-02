using AutoCar.Application.Modules.Sales.Devolucoes.DTOs;
using FluentValidation;

namespace AutoCar.Application.Modules.Sales.Devolucoes.Validators;

/// <summary>Validação de borda da devolução. Exige a venda de origem e ao menos um item com quantidade
/// positiva. A regra de saldo devolvível (≤ vendido − já devolvido) é checada no service (precisa
/// consultar a venda e as devoluções anteriores).</summary>
public sealed class CriarDevolucaoValidator : AbstractValidator<CriarDevolucaoDto>
{
    public CriarDevolucaoValidator()
    {
        RuleFor(d => d.IdPreVenda)
            .NotEmpty().WithMessage("Selecione a venda de origem da devolução.");

        RuleFor(d => d.Motivo)
            .MaximumLength(255);

        RuleFor(d => d.Itens)
            .NotEmpty().WithMessage("Selecione ao menos um item para devolver.");

        RuleForEach(d => d.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.IdProduto)
                .NotEmpty().WithMessage("Item sem produto.");

            item.RuleFor(i => i.Qtd)
                .GreaterThan(0).WithMessage("A quantidade devolvida deve ser maior que zero.");
        });
    }
}
