using AutoCar.Application.Modules.Service.OrdensServico.DTOs;
using AutoCar.Domain.Common;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Service.OrdensServico;

/// <summary>
/// Casos de uso da Ordem de Serviço. Valida o DTO (FluentValidation), confere as FKs (cliente,
/// mecânico, e o produto/serviço de cada item conforme o tipo) e delega cálculo de totais, invariantes
/// e transições de ciclo ao agregado <see cref="OrdemServico"/>. Exceções de invariante do domínio
/// viram <see cref="Result"/> de erro — a UI nunca recebe exceção de regra de negócio.
/// </summary>
public sealed class OrdemServicoService : IOrdemServicoService
{
    private readonly IOrdemServicoRepository _ordens;
    private readonly IClienteRepository _clientes;
    private readonly IProdutoRepository _produtos;
    private readonly IServicoRepository _servicos;
    private readonly IMecanicoRepository _mecanicos;
    private readonly IValidator<SalvarOrdemServicoDto> _validator;

    public OrdemServicoService(
        IOrdemServicoRepository ordens,
        IClienteRepository clientes,
        IProdutoRepository produtos,
        IServicoRepository servicos,
        IMecanicoRepository mecanicos,
        IValidator<SalvarOrdemServicoDto> validator)
    {
        _ordens = ordens;
        _clientes = clientes;
        _produtos = produtos;
        _servicos = servicos;
        _mecanicos = mecanicos;
        _validator = validator;
    }

    public async Task<Result<OrdemServicoDto>> CriarAsync(Guid idUsuario, SalvarOrdemServicoDto dto, CancellationToken ct = default)
    {
        var erro = await ValidarAsync(dto, ct);
        if (erro is not null)
            return Result.Falhar<OrdemServicoDto>(erro);

        try
        {
            var os = new OrdemServico(
                idUsuario, dto.IdCliente, dto.NomeClienteAvulso, dto.IdMecanico,
                dto.VeiculoMontadora, dto.VeiculoModelo, dto.VeiculoAno, dto.VeiculoPlaca, dto.Observacao);
            os.DefinirItens(MapearItens(dto.Itens));
            os.AplicarDescontoGeral(dto.VlrDesconto);

            await _ordens.AdicionarAsync(os, ct);

            var criada = await _ordens.ObterPorIdAsync(os.Id, ct);
            return Result.Ok(Mapear(criada!));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Falhar<OrdemServicoDto>(Error.Validacao(ex.Message));
        }
    }

    public async Task<Result<OrdemServicoDto>> AtualizarAsync(Guid id, SalvarOrdemServicoDto dto, CancellationToken ct = default)
    {
        var erro = await ValidarAsync(dto, ct);
        if (erro is not null)
            return Result.Falhar<OrdemServicoDto>(erro);

        var existe = await _ordens.ObterPorIdAsync(id, ct);
        if (existe is null)
            return Result.Falhar<OrdemServicoDto>(Error.NaoEncontrado("Ordem de serviço não encontrada."));

        try
        {
            await _ordens.AtualizarAsync(id, os =>
            {
                os.AlterarCabecalho(dto.IdCliente, dto.NomeClienteAvulso, dto.IdMecanico,
                    dto.VeiculoMontadora, dto.VeiculoModelo, dto.VeiculoAno, dto.VeiculoPlaca, dto.Observacao);
                os.DefinirItens(MapearItens(dto.Itens));
                os.AplicarDescontoGeral(dto.VlrDesconto);
            }, ct);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Falhar<OrdemServicoDto>(Error.Validacao(ex.Message));
        }

        var atualizada = await _ordens.ObterPorIdAsync(id, ct);
        return Result.Ok(Mapear(atualizada!));
    }

    public Task<Result> IniciarAsync(Guid id, CancellationToken ct = default) =>
        TransicionarAsync(id, os => os.Iniciar(), "Ordem de serviço não encontrada.", ct);

    public Task<Result> ConcluirAsync(Guid id, CancellationToken ct = default) =>
        TransicionarAsync(id, os => os.Concluir(), "Ordem de serviço não encontrada.", ct);

    public Task<Result> CancelarAsync(Guid id, CancellationToken ct = default) =>
        TransicionarAsync(id, os => os.Cancelar(), "Ordem de serviço não encontrada.", ct);

    public async Task<IReadOnlyList<OrdemServicoListaDto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var ordens = await _ordens.ListarAsync(filtro, ct);
        return ordens.Select(o => new OrdemServicoListaDto(
            o.Id, o.CodOrdemServico, o.Situacao, NomeCliente(o), o.VeiculoPlaca,
            o.IdMecanico, o.Itens.Count, o.VlrTotal, o.CriadoEm)).ToList();
    }

    public async Task<Result<OrdemServicoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var os = await _ordens.ObterPorIdAsync(id, ct);
        return os is null
            ? Result.Falhar<OrdemServicoDto>(Error.NaoEncontrado("Ordem de serviço não encontrada."))
            : Result.Ok(Mapear(os));
    }

    /// <summary>Aplica uma transição de ciclo (Iniciar/Concluir/Cancelar) à OS rastreada, traduzindo
    /// a invariante do domínio em erro. Nenhuma dessas transições mexe em estoque (isso é o Faturar, 4.4).</summary>
    private async Task<Result> TransicionarAsync(Guid id, Action<OrdemServico> transicao, string msgNaoEncontrada, CancellationToken ct)
    {
        var existe = await _ordens.ObterPorIdAsync(id, ct);
        if (existe is null)
            return Result.Falhar(Error.NaoEncontrado(msgNaoEncontrada));

        try
        {
            await _ordens.AplicarTransicaoAsync(id, transicao, ct);
            return Result.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Falhar(Error.Validacao(ex.Message));
        }
    }

    /// <summary>Valida o DTO e confere as FKs: cliente e mecânico (quando informados) e o produto OU
    /// serviço de cada item, conforme o tipo da linha. Retorna o erro ou null.</summary>
    private async Task<Error?> ValidarAsync(SalvarOrdemServicoDto dto, CancellationToken ct)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Error.Validacao(validacao.Errors.Count > 0 ? validacao.Errors[0].ErrorMessage : "Dados inválidos.");

        if (dto.IdCliente is Guid idCliente && await _clientes.ObterPorIdAsync(idCliente, ct) is null)
            return Error.Validacao("Selecione um cliente válido.");

        if (dto.IdMecanico is Guid idMecanico && await _mecanicos.ObterPorIdAsync(idMecanico, ct) is null)
            return Error.Validacao("Selecione um mecânico válido.");

        foreach (var item in dto.Itens)
        {
            if (item.Tipo == TipoItemOrdemServico.Peca)
            {
                if (item.IdProduto is not Guid idProduto || await _produtos.ObterPorIdAsync(idProduto, ct) is null)
                    return Error.Validacao($"Produto do item \"{item.DescricaoItem}\" não encontrado.");
            }
            else
            {
                if (item.IdServico is not Guid idServico || await _servicos.ObterPorIdAsync(idServico, ct) is null)
                    return Error.Validacao($"Serviço do item \"{item.DescricaoItem}\" não encontrado.");
            }
        }

        return null;
    }

    // Converte os DTOs de item em entidades, escolhendo o factory conforme o tipo. O total da linha é
    // calculado no construtor da entidade; a invariante "FK do tipo certo" é garantida pelos factories.
    private static IEnumerable<OrdemServicoItem> MapearItens(IReadOnlyList<OrdemServicoItemDto> itens) =>
        itens.Select(i => i.Tipo == TipoItemOrdemServico.Peca
            ? OrdemServicoItem.DePeca(i.IdProduto!.Value, i.DescricaoItem, i.Qtd, i.VlrUnitario, i.VlrDesconto)
            : OrdemServicoItem.DeServico(i.IdServico!.Value, i.DescricaoItem, i.Qtd, i.VlrUnitario, i.VlrDesconto));

    // Nome a exibir na listagem: cliente cadastrado, avulso, ou "Consumidor" quando nenhum.
    private static string NomeCliente(OrdemServico o) =>
        o.Cliente?.RazaoSocial ?? o.NomeClienteAvulso ?? "CONSUMIDOR";

    private static OrdemServicoDto Mapear(OrdemServico o) => new(
        o.Id, o.CodOrdemServico, o.Situacao, o.IdCliente, o.NomeClienteAvulso, o.IdMecanico,
        o.VeiculoMontadora, o.VeiculoModelo, o.VeiculoAno, o.VeiculoPlaca,
        o.SubtotalItens, o.SubtotalPecas, o.SubtotalServicos, o.VlrDesconto, o.VlrTotal, o.Observacao, o.FlgAtivo,
        o.Itens.Select(i => new OrdemServicoItemDetalheDto(
            i.Id, i.Tipo, i.IdProduto, i.IdServico, i.DescricaoItem, i.Qtd, i.VlrUnitario, i.VlrDesconto, i.VlrTotalItem)).ToList());
}
