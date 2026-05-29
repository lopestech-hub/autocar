using AutoCar.Application.Modules.Registrations.Fornecedores.DTOs;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using AutoCar.Domain.ValueObjects;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Fornecedores;

/// <summary>
/// CRUD de Fornecedor. Valida o DTO (FluentValidation), garante documento único e
/// delega as invariantes ao domínio (Value Object Documento, entidade Fornecedor).
/// </summary>
public sealed class FornecedorService : IFornecedorService
{
    private readonly IFornecedorRepository _fornecedores;
    private readonly IValidator<SalvarFornecedorDto> _validator;

    public FornecedorService(IFornecedorRepository fornecedores, IValidator<SalvarFornecedorDto> validator)
    {
        _fornecedores = fornecedores;
        _validator = validator;
    }

    public async Task<Result<FornecedorDto>> CriarAsync(SalvarFornecedorDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<FornecedorDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        // Documento já foi validado pelo validator; aqui é garantido não-nulo.
        var documento = Documento.Criar(dto.Documento, dto.TipoPessoa)!;

        if (await _fornecedores.ExisteDocumentoAsync(documento.Numero, null, ct))
            return Result.Falhar<FornecedorDto>(Error.Conflito("Já existe um fornecedor com este CPF/CNPJ."));

        var fornecedor = new Fornecedor(
            documento,
            dto.RazaoSocial,
            dto.NomeFantasia,
            dto.Telefone,
            dto.Email,
            MontarEndereco(dto),
            dto.InscricaoEstadual,
            dto.Contato,
            dto.Observacao);

        await _fornecedores.AdicionarAsync(fornecedor, ct);
        await _fornecedores.SalvarAsync(ct);

        return Result.Ok(Mapear(fornecedor));
    }

    public async Task<Result<FornecedorDto>> AtualizarAsync(Guid id, SalvarFornecedorDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<FornecedorDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        var fornecedor = await _fornecedores.ObterPorIdAsync(id, ct);
        if (fornecedor is null)
            return Result.Falhar<FornecedorDto>(Error.NaoEncontrado("Fornecedor não encontrado."));

        var documento = Documento.Criar(dto.Documento, dto.TipoPessoa)!;

        if (await _fornecedores.ExisteDocumentoAsync(documento.Numero, id, ct))
            return Result.Falhar<FornecedorDto>(Error.Conflito("Já existe outro fornecedor com este CPF/CNPJ."));

        fornecedor.AlterarDados(
            documento,
            dto.RazaoSocial,
            dto.NomeFantasia,
            dto.Telefone,
            dto.Email,
            MontarEndereco(dto),
            dto.InscricaoEstadual,
            dto.Contato,
            dto.Observacao);

        _fornecedores.Atualizar(fornecedor);
        await _fornecedores.SalvarAsync(ct);

        return Result.Ok(Mapear(fornecedor));
    }

    public async Task<Result> InativarAsync(Guid id, CancellationToken ct = default)
    {
        var fornecedor = await _fornecedores.ObterPorIdAsync(id, ct);
        if (fornecedor is null)
            return Result.Falhar(Error.NaoEncontrado("Fornecedor não encontrado."));

        fornecedor.Inativar();
        _fornecedores.Atualizar(fornecedor);
        await _fornecedores.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var fornecedor = await _fornecedores.ObterPorIdAsync(id, ct);
        if (fornecedor is null)
            return Result.Falhar(Error.NaoEncontrado("Fornecedor não encontrado."));

        fornecedor.Ativar();
        _fornecedores.Atualizar(fornecedor);
        await _fornecedores.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyList<FornecedorListaDto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var fornecedores = await _fornecedores.ListarAsync(filtro, ct);
        return fornecedores
            .Select(f => new FornecedorListaDto(f.Id, f.CodFornecedor, f.TipoPessoa, f.Documento, f.RazaoSocial, f.Telefone, f.FlgAtivo))
            .ToList();
    }

    public async Task<Result<FornecedorDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var fornecedor = await _fornecedores.ObterPorIdAsync(id, ct);
        return fornecedor is null
            ? Result.Falhar<FornecedorDto>(Error.NaoEncontrado("Fornecedor não encontrado."))
            : Result.Ok(Mapear(fornecedor));
    }

    private static Endereco MontarEndereco(SalvarFornecedorDto dto) =>
        new(dto.Cep, dto.Logradouro, dto.Numero, dto.Complemento, dto.Bairro, dto.Cidade, dto.Uf);

    private static string PrimeiraMensagem(FluentValidation.Results.ValidationResult r) =>
        r.Errors.Count > 0 ? r.Errors[0].ErrorMessage : "Dados inválidos.";

    private static FornecedorDto Mapear(Fornecedor f) => new(
        f.Id, f.CodFornecedor, f.TipoPessoa, f.Documento, f.RazaoSocial, f.NomeFantasia,
        f.Telefone, f.Email,
        f.Endereco.Cep, f.Endereco.Logradouro, f.Endereco.Numero, f.Endereco.Complemento,
        f.Endereco.Bairro, f.Endereco.Cidade, f.Endereco.Uf,
        f.InscricaoEstadual, f.Contato, f.Observacao, f.FlgAtivo);
}
