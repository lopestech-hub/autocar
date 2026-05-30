using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>CRUD de Categoria de produto. Valida o DTO (FluentValidation) e garante descrição única.</summary>
public sealed class CategoriaProdutoService : ICategoriaProdutoService
{
    private readonly ICategoriaProdutoRepository _categorias;
    private readonly IValidator<SalvarCategoriaProdutoDto> _validator;

    public CategoriaProdutoService(ICategoriaProdutoRepository categorias, IValidator<SalvarCategoriaProdutoDto> validator)
    {
        _categorias = categorias;
        _validator = validator;
    }

    public async Task<Result<CategoriaProdutoDto>> CriarAsync(SalvarCategoriaProdutoDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<CategoriaProdutoDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        if (await _categorias.ExisteDescricaoAsync(dto.Descricao.Trim(), null, ct))
            return Result.Falhar<CategoriaProdutoDto>(Error.Conflito("Já existe uma categoria com esta descrição."));

        var categoria = new CategoriaProduto(dto.Descricao);
        await _categorias.AdicionarAsync(categoria, ct);
        await _categorias.SalvarAsync(ct);

        return Result.Ok(Mapear(categoria));
    }

    public async Task<Result<CategoriaProdutoDto>> AtualizarAsync(Guid id, SalvarCategoriaProdutoDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<CategoriaProdutoDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        var categoria = await _categorias.ObterPorIdAsync(id, ct);
        if (categoria is null)
            return Result.Falhar<CategoriaProdutoDto>(Error.NaoEncontrado("Categoria não encontrada."));

        if (await _categorias.ExisteDescricaoAsync(dto.Descricao.Trim(), id, ct))
            return Result.Falhar<CategoriaProdutoDto>(Error.Conflito("Já existe outra categoria com esta descrição."));

        categoria.AlterarDados(dto.Descricao);
        _categorias.Atualizar(categoria);
        await _categorias.SalvarAsync(ct);

        return Result.Ok(Mapear(categoria));
    }

    public async Task<Result> InativarAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _categorias.ObterPorIdAsync(id, ct);
        if (categoria is null)
            return Result.Falhar(Error.NaoEncontrado("Categoria não encontrada."));

        categoria.Inativar();
        _categorias.Atualizar(categoria);
        await _categorias.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _categorias.ObterPorIdAsync(id, ct);
        if (categoria is null)
            return Result.Falhar(Error.NaoEncontrado("Categoria não encontrada."));

        categoria.Ativar();
        _categorias.Atualizar(categoria);
        await _categorias.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyList<CategoriaProdutoDto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var categorias = await _categorias.ListarAsync(filtro, ct);
        return categorias.Select(Mapear).ToList();
    }

    public async Task<Result<CategoriaProdutoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _categorias.ObterPorIdAsync(id, ct);
        return categoria is null
            ? Result.Falhar<CategoriaProdutoDto>(Error.NaoEncontrado("Categoria não encontrada."))
            : Result.Ok(Mapear(categoria));
    }

    private static string PrimeiraMensagem(FluentValidation.Results.ValidationResult r) =>
        r.Errors.Count > 0 ? r.Errors[0].ErrorMessage : "Dados inválidos.";

    private static CategoriaProdutoDto Mapear(CategoriaProduto c) => new(c.Id, c.CodCategoria, c.Descricao, c.FlgAtivo);
}
