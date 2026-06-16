using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>CRUD de Lado da peça. Valida o DTO (FluentValidation) e garante descrição única.</summary>
public sealed class LadoPecaService : ILadoPecaService
{
    private readonly ILadoPecaRepository _lados;
    private readonly IValidator<SalvarLadoPecaDto> _validator;

    public LadoPecaService(ILadoPecaRepository lados, IValidator<SalvarLadoPecaDto> validator)
    {
        _lados = lados;
        _validator = validator;
    }

    public async Task<Result<LadoPecaDto>> CriarAsync(SalvarLadoPecaDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<LadoPecaDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        if (await _lados.ExisteDescricaoAsync(dto.Descricao.Trim(), null, ct))
            return Result.Falhar<LadoPecaDto>(Error.Conflito("Já existe um lado com esta descrição."));

        var lado = new LadoPeca(dto.Descricao);
        await _lados.AdicionarAsync(lado, ct);
        await _lados.SalvarAsync(ct);

        return Result.Ok(Mapear(lado));
    }

    public async Task<Result<LadoPecaDto>> AtualizarAsync(Guid id, SalvarLadoPecaDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<LadoPecaDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        var lado = await _lados.ObterPorIdAsync(id, ct);
        if (lado is null)
            return Result.Falhar<LadoPecaDto>(Error.NaoEncontrado("Lado não encontrado."));

        if (await _lados.ExisteDescricaoAsync(dto.Descricao.Trim(), id, ct))
            return Result.Falhar<LadoPecaDto>(Error.Conflito("Já existe outro lado com esta descrição."));

        lado.AlterarDados(dto.Descricao);
        _lados.Atualizar(lado);
        await _lados.SalvarAsync(ct);

        return Result.Ok(Mapear(lado));
    }

    public async Task<Result> InativarAsync(Guid id, CancellationToken ct = default)
    {
        var lado = await _lados.ObterPorIdAsync(id, ct);
        if (lado is null)
            return Result.Falhar(Error.NaoEncontrado("Lado não encontrado."));

        lado.Inativar();
        _lados.Atualizar(lado);
        await _lados.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var lado = await _lados.ObterPorIdAsync(id, ct);
        if (lado is null)
            return Result.Falhar(Error.NaoEncontrado("Lado não encontrado."));

        lado.Ativar();
        _lados.Atualizar(lado);
        await _lados.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyList<LadoPecaDto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var lados = await _lados.ListarAsync(filtro, ct);
        return lados.Select(Mapear).ToList();
    }

    public async Task<Result<LadoPecaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var lado = await _lados.ObterPorIdAsync(id, ct);
        return lado is null
            ? Result.Falhar<LadoPecaDto>(Error.NaoEncontrado("Lado não encontrado."))
            : Result.Ok(Mapear(lado));
    }

    private static string PrimeiraMensagem(FluentValidation.Results.ValidationResult r) =>
        r.Errors.Count > 0 ? r.Errors[0].ErrorMessage : "Dados inválidos.";

    private static LadoPecaDto Mapear(LadoPeca l) => new(l.Id, l.CodLado, l.Descricao, l.FlgAtivo);
}
