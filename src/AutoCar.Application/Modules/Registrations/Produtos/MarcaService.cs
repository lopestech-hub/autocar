using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>CRUD de Marca. Valida o DTO (FluentValidation) e garante descrição única.</summary>
public sealed class MarcaService : IMarcaService
{
    private readonly IMarcaRepository _marcas;
    private readonly IValidator<SalvarMarcaDto> _validator;

    public MarcaService(IMarcaRepository marcas, IValidator<SalvarMarcaDto> validator)
    {
        _marcas = marcas;
        _validator = validator;
    }

    public async Task<Result<MarcaDto>> CriarAsync(SalvarMarcaDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<MarcaDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        if (await _marcas.ExisteDescricaoAsync(dto.Descricao.Trim(), null, ct))
            return Result.Falhar<MarcaDto>(Error.Conflito("Já existe uma marca com esta descrição."));

        var marca = new Marca(dto.Descricao);
        await _marcas.AdicionarAsync(marca, ct);
        await _marcas.SalvarAsync(ct);

        return Result.Ok(Mapear(marca));
    }

    public async Task<Result<MarcaDto>> AtualizarAsync(Guid id, SalvarMarcaDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<MarcaDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        var marca = await _marcas.ObterPorIdAsync(id, ct);
        if (marca is null)
            return Result.Falhar<MarcaDto>(Error.NaoEncontrado("Marca não encontrada."));

        if (await _marcas.ExisteDescricaoAsync(dto.Descricao.Trim(), id, ct))
            return Result.Falhar<MarcaDto>(Error.Conflito("Já existe outra marca com esta descrição."));

        marca.AlterarDados(dto.Descricao);
        _marcas.Atualizar(marca);
        await _marcas.SalvarAsync(ct);

        return Result.Ok(Mapear(marca));
    }

    public async Task<Result> InativarAsync(Guid id, CancellationToken ct = default)
    {
        var marca = await _marcas.ObterPorIdAsync(id, ct);
        if (marca is null)
            return Result.Falhar(Error.NaoEncontrado("Marca não encontrada."));

        marca.Inativar();
        _marcas.Atualizar(marca);
        await _marcas.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var marca = await _marcas.ObterPorIdAsync(id, ct);
        if (marca is null)
            return Result.Falhar(Error.NaoEncontrado("Marca não encontrada."));

        marca.Ativar();
        _marcas.Atualizar(marca);
        await _marcas.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyList<MarcaDto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var marcas = await _marcas.ListarAsync(filtro, ct);
        return marcas.Select(Mapear).ToList();
    }

    public async Task<Result<MarcaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var marca = await _marcas.ObterPorIdAsync(id, ct);
        return marca is null
            ? Result.Falhar<MarcaDto>(Error.NaoEncontrado("Marca não encontrada."))
            : Result.Ok(Mapear(marca));
    }

    private static string PrimeiraMensagem(FluentValidation.Results.ValidationResult r) =>
        r.Errors.Count > 0 ? r.Errors[0].ErrorMessage : "Dados inválidos.";

    private static MarcaDto Mapear(Marca m) => new(m.Id, m.CodMarca, m.Descricao, m.FlgAtivo);
}
