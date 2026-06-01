using AutoCar.Application.Modules.Sales.PreVendas.DTOs;
using FluentValidation;

namespace AutoCar.Application.Modules.Sales.PreVendas.Validators;

/// <summary>Validação de borda da pré-venda. Exige ao menos um item válido e descontos não-negativos;
/// a coerência dos descontos (≤ subtotal) é garantida no domínio (invariante do agregado).</summary>
public sealed class SalvarPreVendaValidator : AbstractValidator<SalvarPreVendaDto>
{
    public SalvarPreVendaValidator()
    {
        RuleFor(p => p.NomeClienteAvulso)
            .MaximumLength(120);

        RuleFor(p => p.VeiculoMontadora).MaximumLength(60);
        RuleFor(p => p.VeiculoModelo).MaximumLength(60);
        RuleFor(p => p.VeiculoAno).MaximumLength(9);
        RuleFor(p => p.VeiculoPlaca).MaximumLength(8);
        RuleFor(p => p.Observacao).MaximumLength(255);

        RuleFor(p => p.VlrDesconto)
            .GreaterThanOrEqualTo(0).WithMessage("O desconto não pode ser negativo.");

        RuleFor(p => p.Itens)
            .NotEmpty().WithMessage("Adicione ao menos um item à pré-venda.");

        RuleForEach(p => p.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.IdProduto)
                .NotEmpty().WithMessage("Item sem produto selecionado.");

            item.RuleFor(i => i.Qtd)
                .GreaterThan(0).WithMessage("A quantidade do item deve ser maior que zero.");

            item.RuleFor(i => i.VlrUnitario)
                .GreaterThanOrEqualTo(0).WithMessage("O valor unitário do item não pode ser negativo.");

            item.RuleFor(i => i.VlrDesconto)
                .GreaterThanOrEqualTo(0).WithMessage("O desconto do item não pode ser negativo.");
        });
    }
}
