using AutoCar.Application.Modules.Service.OrdensServico.DTOs;
using AutoCar.Domain.Enums;
using FluentValidation;

namespace AutoCar.Application.Modules.Service.OrdensServico.Validators;

/// <summary>Validação de borda da OS. Exige ao menos um item válido e descontos não-negativos; a
/// coerência dos descontos (≤ subtotal) é garantida no domínio. Cada item precisa ter a FK do seu
/// tipo: Peça ⇒ id_produto; Serviço ⇒ id_servico (a invariante também é garantida pelo agregado).</summary>
public sealed class SalvarOrdemServicoValidator : AbstractValidator<SalvarOrdemServicoDto>
{
    public SalvarOrdemServicoValidator()
    {
        RuleFor(o => o.NomeClienteAvulso).MaximumLength(120);

        RuleFor(o => o.VeiculoMontadora).MaximumLength(60);
        RuleFor(o => o.VeiculoModelo).MaximumLength(60);
        RuleFor(o => o.VeiculoAno).MaximumLength(9);
        RuleFor(o => o.VeiculoPlaca).MaximumLength(8);
        RuleFor(o => o.Observacao).MaximumLength(255);

        RuleFor(o => o.VlrDesconto)
            .GreaterThanOrEqualTo(0).WithMessage("O desconto não pode ser negativo.");

        RuleFor(o => o.Itens)
            .NotEmpty().WithMessage("Adicione ao menos um item à ordem de serviço.");

        RuleForEach(o => o.Itens).ChildRules(item =>
        {
            // Peça precisa de produto; Serviço precisa de serviço (a FK do tipo errado fica nula).
            item.RuleFor(i => i.IdProduto)
                .NotNull().When(i => i.Tipo == TipoItemOrdemServico.Peca)
                .WithMessage("Item de peça sem produto selecionado.");

            item.RuleFor(i => i.IdServico)
                .NotNull().When(i => i.Tipo == TipoItemOrdemServico.Servico)
                .WithMessage("Item de serviço sem serviço selecionado.");

            item.RuleFor(i => i.Qtd)
                .GreaterThan(0).WithMessage("A quantidade do item deve ser maior que zero.");

            item.RuleFor(i => i.VlrUnitario)
                .GreaterThanOrEqualTo(0).WithMessage("O valor unitário do item não pode ser negativo.");

            item.RuleFor(i => i.VlrDesconto)
                .GreaterThanOrEqualTo(0).WithMessage("O desconto do item não pode ser negativo.");
        });
    }
}
