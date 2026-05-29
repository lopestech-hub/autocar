using AutoCar.Application.Modules.Registrations.Clientes.DTOs;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using AutoCar.Domain.ValueObjects;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Clientes;

/// <summary>
/// CRUD de Cliente. Valida o DTO (FluentValidation), garante documento único e
/// delega as invariantes ao domínio (Value Object Documento, entidade Cliente).
/// </summary>
public sealed class ClienteService : IClienteService
{
    private readonly IClienteRepository _clientes;
    private readonly IValidator<SalvarClienteDto> _validator;

    public ClienteService(IClienteRepository clientes, IValidator<SalvarClienteDto> validator)
    {
        _clientes = clientes;
        _validator = validator;
    }

    public async Task<Result<ClienteDto>> CriarAsync(SalvarClienteDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<ClienteDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        // Documento já foi validado pelo validator; aqui é garantido não-nulo.
        var documento = Documento.Criar(dto.Documento, dto.TipoPessoa)!;

        if (await _clientes.ExisteDocumentoAsync(documento.Numero, null, ct))
            return Result.Falhar<ClienteDto>(Error.Conflito("Já existe um cliente com este CPF/CNPJ."));

        var cliente = new Cliente(
            documento,
            dto.RazaoSocial,
            dto.NomeFantasia,
            dto.Telefone,
            dto.Email,
            MontarEndereco(dto),
            dto.Observacao,
            dto.LimiteCredito);

        await _clientes.AdicionarAsync(cliente, ct);
        await _clientes.SalvarAsync(ct);

        return Result.Ok(Mapear(cliente));
    }

    public async Task<Result<ClienteDto>> AtualizarAsync(Guid id, SalvarClienteDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<ClienteDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        var cliente = await _clientes.ObterPorIdAsync(id, ct);
        if (cliente is null)
            return Result.Falhar<ClienteDto>(Error.NaoEncontrado("Cliente não encontrado."));

        var documento = Documento.Criar(dto.Documento, dto.TipoPessoa)!;

        if (await _clientes.ExisteDocumentoAsync(documento.Numero, id, ct))
            return Result.Falhar<ClienteDto>(Error.Conflito("Já existe outro cliente com este CPF/CNPJ."));

        cliente.AlterarDados(
            documento,
            dto.RazaoSocial,
            dto.NomeFantasia,
            dto.Telefone,
            dto.Email,
            MontarEndereco(dto),
            dto.Observacao,
            dto.LimiteCredito);

        _clientes.Atualizar(cliente);
        await _clientes.SalvarAsync(ct);

        return Result.Ok(Mapear(cliente));
    }

    public async Task<Result> InativarAsync(Guid id, CancellationToken ct = default)
    {
        var cliente = await _clientes.ObterPorIdAsync(id, ct);
        if (cliente is null)
            return Result.Falhar(Error.NaoEncontrado("Cliente não encontrado."));

        cliente.Inativar();
        _clientes.Atualizar(cliente);
        await _clientes.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var cliente = await _clientes.ObterPorIdAsync(id, ct);
        if (cliente is null)
            return Result.Falhar(Error.NaoEncontrado("Cliente não encontrado."));

        cliente.Ativar();
        _clientes.Atualizar(cliente);
        await _clientes.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyList<ClienteListaDto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var clientes = await _clientes.ListarAsync(filtro, ct);
        return clientes
            .Select(c => new ClienteListaDto(c.Id, c.CodCliente, c.TipoPessoa, c.Documento, c.RazaoSocial, c.Telefone, c.FlgAtivo))
            .ToList();
    }

    public async Task<Result<ClienteDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var cliente = await _clientes.ObterPorIdAsync(id, ct);
        return cliente is null
            ? Result.Falhar<ClienteDto>(Error.NaoEncontrado("Cliente não encontrado."))
            : Result.Ok(Mapear(cliente));
    }

    private static Endereco MontarEndereco(SalvarClienteDto dto) =>
        new(dto.Cep, dto.Logradouro, dto.Numero, dto.Complemento, dto.Bairro, dto.Cidade, dto.Uf);

    private static string PrimeiraMensagem(FluentValidation.Results.ValidationResult r) =>
        r.Errors.Count > 0 ? r.Errors[0].ErrorMessage : "Dados inválidos.";

    private static ClienteDto Mapear(Cliente c) => new(
        c.Id, c.CodCliente, c.TipoPessoa, c.Documento, c.RazaoSocial, c.NomeFantasia,
        c.Telefone, c.Email,
        c.Endereco.Cep, c.Endereco.Logradouro, c.Endereco.Numero, c.Endereco.Complemento,
        c.Endereco.Bairro, c.Endereco.Cidade, c.Endereco.Uf,
        c.Observacao, c.LimiteCredito, c.FlgAtivo);
}
