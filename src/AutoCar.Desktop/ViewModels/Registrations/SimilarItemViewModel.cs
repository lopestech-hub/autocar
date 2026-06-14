using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Linha editável do mini-grid de equivalências (cross-reference) dentro do formulário de Produto.
/// O usuário informa marca + referência (+ observação opcional). O vínculo ao produto equivalente
/// (<see cref="IdProdutoEquivalente"/>) NÃO é digitado — vem do banco e é resolvido automaticamente
/// pela referência; aqui ele só é preservado no round-trip de edição. Salva junto com o produto.
/// </summary>
public partial class SimilarItemViewModel : ViewModelBase
{
    [ObservableProperty] private string _marca = string.Empty;
    [ObservableProperty] private string _codReferencia = string.Empty;
    [ObservableProperty] private string? _observacao;

    public SimilarItemViewModel() { }

    public SimilarItemViewModel(string marca, string codReferencia, Guid? idProdutoEquivalente, string? observacao)
    {
        Marca = marca;
        CodReferencia = codReferencia;
        IdProdutoEquivalente = idProdutoEquivalente;
        Observacao = observacao;
    }

    /// <summary>Vínculo ao produto equivalente quando ele já existe no cadastro (read-only na UI).</summary>
    public Guid? IdProdutoEquivalente { get; private set; }

    /// <summary>Linha "preenchível": exige marca E referência (sem os dois não cruza com nada).
    /// Linhas incompletas são descartadas ao salvar.</summary>
    public bool TemConteudo => !string.IsNullOrWhiteSpace(Marca) && !string.IsNullOrWhiteSpace(CodReferencia);

    /// <summary>True quando a equivalência já aponta para um produto da loja (cruza com estoque).
    /// Usado pela UI para sinalizar "em estoque" vs. "só referência".</summary>
    public bool Vinculado => IdProdutoEquivalente is not null;
}
