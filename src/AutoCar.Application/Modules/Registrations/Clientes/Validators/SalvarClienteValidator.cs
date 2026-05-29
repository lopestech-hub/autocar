using AutoCar.Application.Modules.Registrations.Clientes.DTOs;
using AutoCar.Domain.Enums;
using AutoCar.Domain.ValueObjects;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Clientes.Validators;

/// <summary>
/// Validação de borda do cadastro de cliente (campos do DTO). A validação do dígito
/// verificador reusa o Value Object Documento. Unicidade é checada no serviço (precisa do banco).
/// </summary>
public sealed class SalvarClienteValidator : AbstractValidator<SalvarClienteDto>
{
    public SalvarClienteValidator()
    {
        RuleFor(c => c.RazaoSocial)
            .NotEmpty().WithMessage("Informe o nome ou a razão social.")
            .MaximumLength(150);

        RuleFor(c => c.Documento)
            .NotEmpty().WithMessage("Informe o CPF/CNPJ.")
            .Must((dto, doc) => Documento.Criar(doc, dto.TipoPessoa) is not null)
            .WithMessage(dto => dto.TipoPessoa == TipoPessoa.Fisica
                ? "CPF inválido."
                : "CNPJ inválido.");

        RuleFor(c => c.NomeFantasia)
            .MaximumLength(150);

        RuleFor(c => c.Email)
            .EmailAddress().When(c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("E-mail inválido.")
            .MaximumLength(160);

        RuleFor(c => c.Telefone)
            .MaximumLength(20);

        RuleFor(c => c.Uf)
            .Length(2).When(c => !string.IsNullOrWhiteSpace(c.Uf))
            .WithMessage("UF deve ter 2 letras.");

        RuleFor(c => c.LimiteCredito)
            .GreaterThanOrEqualTo(0).WithMessage("Limite de crédito não pode ser negativo.");
    }
}
