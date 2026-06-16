using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>CRUD de Posição da peça. Valida o DTO (FluentValidation) e garante descrição única.</summary>
public sealed class PosicaoPecaService : IPosicaoPecaService
{
    private readonly IPosicaoPecaRepository _posicoes;
    private readonly IValidator<SalvarPosicaoPecaDto> _validator;

    public PosicaoPecaService(IPosicaoPecaRepository posicoes, IValidator<SalvarPosicaoPecaDto> validator)
    {
        _posicoes = posicoes;
        _validator = validator;
    }

    public async Task<Result<PosicaoPecaDto>> CriarAsync(SalvarPosicaoPecaDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<PosicaoPecaDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        if (await _posicoes.ExisteDescricaoAsync(dto.Descricao.Trim(), null, ct))
            return Result.Falhar<PosicaoPecaDto>(Error.Conflito("Já existe uma posição com esta descrição."));

        var posicao = new PosicaoPeca(dto.Descricao);
        await _posicoes.AdicionarAsync(posicao, ct);
        await _posicoes.SalvarAsync(ct);

        return Result.Ok(Mapear(posicao));
    }

    public async Task<Result<PosicaoPecaDto>> AtualizarAsync(Guid id, SalvarPosicaoPecaDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<PosicaoPecaDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        var posicao = await _posicoes.ObterPorIdAsync(id, ct);
        if (posicao is null)
            return Result.Falhar<PosicaoPecaDto>(Error.NaoEncontrado("Posição não encontrada."));

        if (await _posicoes.ExisteDescricaoAsync(dto.Descricao.Trim(), id, ct))
            return Result.Falhar<PosicaoPecaDto>(Error.Conflito("Já existe outra posição com esta descrição."));

        posicao.AlterarDados(dto.Descricao);
        _posicoes.Atualizar(posicao);
        await _posicoes.SalvarAsync(ct);

        return Result.Ok(Mapear(posicao));
    }

    public async Task<Result> InativarAsync(Guid id, CancellationToken ct = default)
    {
        var posicao = await _posicoes.ObterPorIdAsync(id, ct);
        if (posicao is null)
            return Result.Falhar(Error.NaoEncontrado("Posição não encontrada."));

        posicao.Inativar();
        _posicoes.Atualizar(posicao);
        await _posicoes.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var posicao = await _posicoes.ObterPorIdAsync(id, ct);
        if (posicao is null)
            return Result.Falhar(Error.NaoEncontrado("Posição não encontrada."));

        posicao.Ativar();
        _posicoes.Atualizar(posicao);
        await _posicoes.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyList<PosicaoPecaDto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var posicoes = await _posicoes.ListarAsync(filtro, ct);
        return posicoes.Select(Mapear).ToList();
    }

    public async Task<Result<PosicaoPecaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var posicao = await _posicoes.ObterPorIdAsync(id, ct);
        return posicao is null
            ? Result.Falhar<PosicaoPecaDto>(Error.NaoEncontrado("Posição não encontrada."))
            : Result.Ok(Mapear(posicao));
    }

    private static string PrimeiraMensagem(FluentValidation.Results.ValidationResult r) =>
        r.Errors.Count > 0 ? r.Errors[0].ErrorMessage : "Dados inválidos.";

    private static PosicaoPecaDto Mapear(PosicaoPeca p) => new(p.Id, p.CodPosicao, p.Descricao, p.FlgAtivo);
}
