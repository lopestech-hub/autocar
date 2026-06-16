using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>
/// CRUD de Grupo de produto. Valida o DTO (FluentValidation), confere a categoria (obrigatória)
/// e garante descrição única DENTRO da categoria (pode haver "TAMPA" em categorias diferentes).
/// </summary>
public sealed class GrupoProdutoService : IGrupoProdutoService
{
    private readonly IGrupoProdutoRepository _grupos;
    private readonly ICategoriaProdutoRepository _categorias;
    private readonly IValidator<SalvarGrupoProdutoDto> _validator;

    public GrupoProdutoService(
        IGrupoProdutoRepository grupos,
        ICategoriaProdutoRepository categorias,
        IValidator<SalvarGrupoProdutoDto> validator)
    {
        _grupos = grupos;
        _categorias = categorias;
        _validator = validator;
    }

    public async Task<Result<GrupoProdutoDto>> CriarAsync(SalvarGrupoProdutoDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<GrupoProdutoDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        if (await _categorias.ObterPorIdAsync(dto.IdCategoria, ct) is null)
            return Result.Falhar<GrupoProdutoDto>(Error.Validacao("Selecione uma categoria válida."));

        if (await _grupos.ExisteDescricaoAsync(dto.Descricao.Trim(), dto.IdCategoria, null, ct))
            return Result.Falhar<GrupoProdutoDto>(Error.Conflito("Já existe um grupo com esta descrição nesta categoria."));

        var grupo = new GrupoProduto(dto.Descricao, dto.IdCategoria);
        await _grupos.AdicionarAsync(grupo, ct);
        await _grupos.SalvarAsync(ct);

        return Result.Ok(await MapearComCategoriaAsync(grupo, ct));
    }

    public async Task<Result<GrupoProdutoDto>> AtualizarAsync(Guid id, SalvarGrupoProdutoDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<GrupoProdutoDto>(Error.Validacao(PrimeiraMensagem(validacao)));

        var grupo = await _grupos.ObterPorIdAsync(id, ct);
        if (grupo is null)
            return Result.Falhar<GrupoProdutoDto>(Error.NaoEncontrado("Grupo não encontrado."));

        if (await _categorias.ObterPorIdAsync(dto.IdCategoria, ct) is null)
            return Result.Falhar<GrupoProdutoDto>(Error.Validacao("Selecione uma categoria válida."));

        if (await _grupos.ExisteDescricaoAsync(dto.Descricao.Trim(), dto.IdCategoria, id, ct))
            return Result.Falhar<GrupoProdutoDto>(Error.Conflito("Já existe outro grupo com esta descrição nesta categoria."));

        grupo.AlterarDados(dto.Descricao, dto.IdCategoria);
        _grupos.Atualizar(grupo);
        await _grupos.SalvarAsync(ct);

        return Result.Ok(await MapearComCategoriaAsync(grupo, ct));
    }

    public async Task<Result> InativarAsync(Guid id, CancellationToken ct = default)
    {
        var grupo = await _grupos.ObterPorIdAsync(id, ct);
        if (grupo is null)
            return Result.Falhar(Error.NaoEncontrado("Grupo não encontrado."));

        grupo.Inativar();
        _grupos.Atualizar(grupo);
        await _grupos.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var grupo = await _grupos.ObterPorIdAsync(id, ct);
        if (grupo is null)
            return Result.Falhar(Error.NaoEncontrado("Grupo não encontrado."));

        grupo.Ativar();
        _grupos.Atualizar(grupo);
        await _grupos.SalvarAsync(ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyList<GrupoProdutoDto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var grupos = await _grupos.ListarAsync(filtro, ct);
        return grupos.Select(Mapear).ToList();
    }

    public async Task<Result<GrupoProdutoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var grupo = await _grupos.ObterPorIdAsync(id, ct);
        return grupo is null
            ? Result.Falhar<GrupoProdutoDto>(Error.NaoEncontrado("Grupo não encontrado."))
            : Result.Ok(Mapear(grupo));
    }

    public async Task<IReadOnlyList<OpcaoDto>> ListarCategoriasAsync(CancellationToken ct = default)
    {
        var lista = await _categorias.ListarAsync(null, ct);
        return lista.Select(c => new OpcaoDto(c.Id, c.Descricao)).ToList();
    }

    private static string PrimeiraMensagem(FluentValidation.Results.ValidationResult r) =>
        r.Errors.Count > 0 ? r.Errors[0].ErrorMessage : "Dados inválidos.";

    private static GrupoProdutoDto Mapear(GrupoProduto g) =>
        new(g.Id, g.CodGrupo, g.Descricao, g.IdCategoria, g.Categoria?.Descricao, g.FlgAtivo);

    // Após criar/atualizar, a navegação Categoria pode não estar carregada — resolve o nome p/ retornar.
    private async Task<GrupoProdutoDto> MapearComCategoriaAsync(GrupoProduto g, CancellationToken ct)
    {
        var categoria = g.Categoria ?? await _categorias.ObterPorIdAsync(g.IdCategoria, ct);
        return new(g.Id, g.CodGrupo, g.Descricao, g.IdCategoria, categoria?.Descricao, g.FlgAtivo);
    }
}
