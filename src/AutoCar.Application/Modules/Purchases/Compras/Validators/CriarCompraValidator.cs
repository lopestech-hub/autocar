using AutoCar.Application.Modules.Purchases.Compras.DTOs;
using FluentValidation;

namespace AutoCar.Application.Modules.Purchases.Compras.Validators;

/// <summary>Validação de borda da compra. Exige fornecedor e ao menos um item com quantidade positiva
/// e custo não-negativo. A existência do fornecedor/produto (FK) é checada no service (consulta o banco).</summary>
public sealed class CriarCompraValidator : AbstractValidator<CriarCompraDto>
{
    public CriarCompraValidator()
    {
        RuleFor(d => d.IdFornecedor)
            .NotEmpty().WithMessage("Selecione o fornecedor da compra.");

        RuleFor(d => d.NumDocumento)
            .MaximumLength(40);

        RuleFor(d => d.Observacao)
            .MaximumLength(255);

        RuleFor(d => d.Itens)
            .NotEmpty().WithMessage("Adicione ao menos um item à compra.");

        RuleForEach(d => d.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.IdProduto)
                .NotEmpty().WithMessage("Item sem produto.");

            item.RuleFor(i => i.Qtd)
                .GreaterThan(0).WithMessage("A quantidade comprada deve ser maior que zero.");

            item.RuleFor(i => i.VlrCustoUnitario)
                .GreaterThanOrEqualTo(0).WithMessage("O custo unitário não pode ser negativo.");
        });
    }
}
