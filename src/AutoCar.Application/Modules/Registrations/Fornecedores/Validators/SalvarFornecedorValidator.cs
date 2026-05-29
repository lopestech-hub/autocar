using AutoCar.Application.Modules.Registrations.Fornecedores.DTOs;
using AutoCar.Domain.Enums;
using AutoCar.Domain.ValueObjects;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Fornecedores.Validators;

/// <summary>
/// Validação de borda do cadastro de fornecedor (campos do DTO). A validação do dígito
/// verificador reusa o Value Object Documento. Unicidade é checada no serviço (precisa do banco).
/// </summary>
public sealed class SalvarFornecedorValidator : AbstractValidator<SalvarFornecedorDto>
{
    public SalvarFornecedorValidator()
    {
        RuleFor(f => f.RazaoSocial)
            .NotEmpty().WithMessage("Informe o nome ou a razão social.")
            .MaximumLength(150);

        RuleFor(f => f.Documento)
            .NotEmpty().WithMessage("Informe o CPF/CNPJ.")
            .Must((dto, doc) => Documento.Criar(doc, dto.TipoPessoa) is not null)
            .WithMessage(dto => dto.TipoPessoa == TipoPessoa.Fisica
                ? "CPF inválido."
                : "CNPJ inválido.");

        RuleFor(f => f.NomeFantasia)
            .MaximumLength(150);

        RuleFor(f => f.Email)
            .EmailAddress().When(f => !string.IsNullOrWhiteSpace(f.Email))
            .WithMessage("E-mail inválido.")
            .MaximumLength(160);

        RuleFor(f => f.Telefone)
            .MaximumLength(20);

        RuleFor(f => f.Uf)
            .Length(2).When(f => !string.IsNullOrWhiteSpace(f.Uf))
            .WithMessage("UF deve ter 2 letras.");

        RuleFor(f => f.InscricaoEstadual)
            .MaximumLength(20);

        RuleFor(f => f.Contato)
            .MaximumLength(100);
    }
}
