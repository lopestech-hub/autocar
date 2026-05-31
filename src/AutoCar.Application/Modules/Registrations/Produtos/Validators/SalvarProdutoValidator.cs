using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Produtos.Validators;

/// <summary>Validação de borda do cadastro de produto. Categoria e unicidade do
/// código de barras são checadas no serviço.</summary>
public sealed class SalvarProdutoValidator : AbstractValidator<SalvarProdutoDto>
{
    public SalvarProdutoValidator()
    {
        RuleFor(p => p.IdCategoria)
            .NotEmpty().WithMessage("Selecione a categoria do produto.");

        RuleFor(p => p.Descricao)
            .NotEmpty().WithMessage("Informe a descrição do produto.")
            .MaximumLength(120);

        RuleFor(p => p.DescricaoComplementar)
            .MaximumLength(160);

        RuleFor(p => p.CodBarras)
            .MaximumLength(20);

        RuleFor(p => p.CodFabricante)
            .MaximumLength(40);

        RuleFor(p => p.Unidade)
            .IsInEnum().WithMessage("Selecione a unidade de medida.");

        RuleFor(p => p.VlrCusto)
            .GreaterThanOrEqualTo(0).WithMessage("O custo não pode ser negativo.");

        RuleFor(p => p.VlrVenda)
            .GreaterThanOrEqualTo(0).WithMessage("O valor de venda não pode ser negativo.");
    }
}
