using AutoCar.Application.Modules.Estoque.DTOs;
using AutoCar.Domain.Common;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Estoque;

/// <summary>
/// Casos de uso do Estoque. Valida o DTO (FluentValidation), confere o produto e delega a regra de
/// movimentação ao agregado <see cref="Domain.Entities.EstoqueProduto"/>. Converte em <see cref="Result"/>:
/// as invariantes do domínio (saldo insuficiente) viram <see cref="Error.Validacao"/>; a colisão de
/// concorrência otimista (xmin — dois terminais no mesmo saldo) vira <see cref="Error.Conflito"/>.
/// A UI nunca recebe exceção de regra de negócio nem de concorrência.
/// </summary>
public sealed class EstoqueService : IEstoqueService
{
    private readonly IEstoqueRepository _estoque;
    private readonly IProdutoRepository _produtos;
    private readonly IValidator<MovimentarEstoqueDto> _validator;

    public EstoqueService(
        IEstoqueRepository estoque,
        IProdutoRepository produtos,
        IValidator<MovimentarEstoqueDto> validator)
    {
        _estoque = estoque;
        _produtos = produtos;
        _validator = validator;
    }

    public async Task<IReadOnlyList<SaldoEstoqueListaDto>> ListarSaldosAsync(
        string? filtro, CancellationToken ct = default)
    {
        // O repositório já resolve produto + saldo numa única consulta (sem N+1). Só mapeia para o DTO.
        var saldos = await _estoque.ListarSaldosAsync(filtro, ct);
        return saldos.Select(s => new SaldoEstoqueListaDto(
            s.IdProduto, s.CodProduto, s.Descricao, s.Unidade,
            s.QtdSaldo, s.QtdReservada, s.QtdSaldo - s.QtdReservada)).ToList();
    }

    public async Task<IReadOnlyList<MovimentoEstoqueDto>> ListarMovimentosAsync(
        Guid idProduto, CancellationToken ct = default)
    {
        var movimentos = await _estoque.ListarMovimentosAsync(idProduto, ct);
        return movimentos.Select(m => new MovimentoEstoqueDto(
            m.Id, m.CodMovimento, m.Tipo, m.Qtd, m.QtdSaldoApos,
            m.Origem, m.CodDocumentoOrigem, m.Observacao, m.CriadoEm)).ToList();
    }

    public async Task<Result> MovimentarAsync(
        Guid idUsuario, MovimentarEstoqueDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar(Error.Validacao(
                validacao.Errors.Count > 0 ? validacao.Errors[0].ErrorMessage : "Dados inválidos."));

        if (await _produtos.ObterPorIdAsync(dto.IdProduto, ct) is null)
            return Result.Falhar(Error.NaoEncontrado("Produto não encontrado."));

        try
        {
            await _estoque.MovimentarAsync(
                dto.IdProduto,
                estoque => estoque.Movimentar(dto.Tipo, dto.Qtd, idUsuario, dto.Observacao),
                ct);
            return Result.Ok();
        }
        catch (ConcorrenciaException)
        {
            // Outro terminal alterou o saldo entre a leitura e a gravação (xmin não bateu).
            return Result.Falhar(Error.Conflito(
                "O saldo deste produto foi alterado por outro terminal. Recarregue e tente novamente."));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Falhar(Error.Validacao(ex.Message));
        }
    }
}
