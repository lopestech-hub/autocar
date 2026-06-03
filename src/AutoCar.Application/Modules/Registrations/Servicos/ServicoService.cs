using AutoCar.Application.Modules.Registrations.Servicos.DTOs;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Servicos;

/// <summary>CRUD de Serviço. Valida o DTO (FluentValidation) e garante descrição única.</summary>
public sealed class ServicoService : IServicoService
{
    private readonly IServicoRepository _servicos;
    private readonly IValidator<SalvarServicoDto> _validator;

    public ServicoService(IServicoRepository servicos, IValidator<SalvarServicoDto> validator)
    {
        _servicos = servicos;
        _validator = validator;
    }

    public async Task<Result<ServicoDto>> CriarAsync(SalvarServicoDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<ServicoDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        if (await _servicos.ExisteDescricaoAsync(dto.Descricao.Trim(), null, ct))
            return Result.Falhar<ServicoDto>(Error.Conflito("Já existe um serviço com esta descrição."));

        var servico = new Servico(dto.Descricao, dto.VlrPadrao);
        await _servicos.AdicionarAsync(servico, ct);
        await _servicos.SalvarAsync(ct);

        return Result.Ok(Mapear(servico));
    }

    public async Task<Result<ServicoDto>> AtualizarAsync(Guid id, SalvarServicoDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<ServicoDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        var servico = await _servicos.ObterPorIdAsync(id, ct);
        if (servico is null)
            return Result.Falhar<ServicoDto>(Error.NaoEncontrado("Serviço não encontrado."));

        if (await _servicos.ExisteDescricaoAsync(dto.Descricao.Trim(), id, ct))
            return Result.Falhar<ServicoDto>(Error.Conflito("Já existe outro serviço com esta descrição."));

        servico.AlterarDados(dto.Descricao, dto.VlrPadrao);
        _servicos.Atualizar(servico);
        await _servicos.SalvarAsync(ct);

        return Result.Ok(Mapear(servico));
    }

    public async Task<Result> InativarAsync(Guid id, CancellationToken ct = default)
    {
        var servico = await _servicos.ObterPorIdAsync(id, ct);
        if (servico is null)
            return Result.Falhar(Error.NaoEncontrado("Serviço não encontrado."));

        servico.Inativar();
        _servicos.Atualizar(servico);
        await _servicos.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var servico = await _servicos.ObterPorIdAsync(id, ct);
        if (servico is null)
            return Result.Falhar(Error.NaoEncontrado("Serviço não encontrado."));

        servico.Ativar();
        _servicos.Atualizar(servico);
        await _servicos.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyList<ServicoDto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var servicos = await _servicos.ListarAsync(filtro, ct);
        return servicos.Select(Mapear).ToList();
    }

    public async Task<Result<ServicoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var servico = await _servicos.ObterPorIdAsync(id, ct);
        return servico is null
            ? Result.Falhar<ServicoDto>(Error.NaoEncontrado("Serviço não encontrado."))
            : Result.Ok(Mapear(servico));
    }

    private static string PrimeiraMensagem(FluentValidation.Results.ValidationResult r) =>
        r.Errors.Count > 0 ? r.Errors[0].ErrorMessage : "Dados inválidos.";

    private static ServicoDto Mapear(Servico s) => new(s.Id, s.CodServico, s.Descricao, s.VlrPadrao, s.FlgAtivo);
}
