using AutoCar.Application.Modules.Registrations.Mecanicos.DTOs;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Mecanicos;

/// <summary>CRUD de Mecânico. Valida o DTO (FluentValidation) e garante nome único.</summary>
public sealed class MecanicoService : IMecanicoService
{
    private readonly IMecanicoRepository _mecanicos;
    private readonly IValidator<SalvarMecanicoDto> _validator;

    public MecanicoService(IMecanicoRepository mecanicos, IValidator<SalvarMecanicoDto> validator)
    {
        _mecanicos = mecanicos;
        _validator = validator;
    }

    public async Task<Result<MecanicoDto>> CriarAsync(SalvarMecanicoDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<MecanicoDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        if (await _mecanicos.ExisteNomeAsync(dto.Nome.Trim(), null, ct))
            return Result.Falhar<MecanicoDto>(Error.Conflito("Já existe um mecânico com este nome."));

        var mecanico = new Mecanico(dto.Nome, dto.Telefone);
        await _mecanicos.AdicionarAsync(mecanico, ct);
        await _mecanicos.SalvarAsync(ct);

        return Result.Ok(Mapear(mecanico));
    }

    public async Task<Result<MecanicoDto>> AtualizarAsync(Guid id, SalvarMecanicoDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<MecanicoDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        var mecanico = await _mecanicos.ObterPorIdAsync(id, ct);
        if (mecanico is null)
            return Result.Falhar<MecanicoDto>(Error.NaoEncontrado("Mecânico não encontrado."));

        if (await _mecanicos.ExisteNomeAsync(dto.Nome.Trim(), id, ct))
            return Result.Falhar<MecanicoDto>(Error.Conflito("Já existe outro mecânico com este nome."));

        mecanico.AlterarDados(dto.Nome, dto.Telefone);
        _mecanicos.Atualizar(mecanico);
        await _mecanicos.SalvarAsync(ct);

        return Result.Ok(Mapear(mecanico));
    }

    public async Task<Result> InativarAsync(Guid id, CancellationToken ct = default)
    {
        var mecanico = await _mecanicos.ObterPorIdAsync(id, ct);
        if (mecanico is null)
            return Result.Falhar(Error.NaoEncontrado("Mecânico não encontrado."));

        mecanico.Inativar();
        _mecanicos.Atualizar(mecanico);
        await _mecanicos.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var mecanico = await _mecanicos.ObterPorIdAsync(id, ct);
        if (mecanico is null)
            return Result.Falhar(Error.NaoEncontrado("Mecânico não encontrado."));

        mecanico.Ativar();
        _mecanicos.Atualizar(mecanico);
        await _mecanicos.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyList<MecanicoDto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var mecanicos = await _mecanicos.ListarAsync(filtro, ct);
        return mecanicos.Select(Mapear).ToList();
    }

    public async Task<Result<MecanicoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var mecanico = await _mecanicos.ObterPorIdAsync(id, ct);
        return mecanico is null
            ? Result.Falhar<MecanicoDto>(Error.NaoEncontrado("Mecânico não encontrado."))
            : Result.Ok(Mapear(mecanico));
    }

    private static string PrimeiraMensagem(FluentValidation.Results.ValidationResult r) =>
        r.Errors.Count > 0 ? r.Errors[0].ErrorMessage : "Dados inválidos.";

    private static MecanicoDto Mapear(Mecanico m) => new(m.Id, m.CodMecanico, m.Nome, m.Telefone, m.FlgAtivo);
}
