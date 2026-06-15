using System;
using System.IO;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Configuration;

namespace AutoCar.Desktop.Services;

/// <summary>
/// Resolve a imagem de um produto a partir do NOME do arquivo guardado no banco
/// (ex: "27022.jpg"). O caminho físico é montado com a pasta-base configurável por terminal
/// (appsettings: Imagens:PastaBase) — o sistema é 2-tier, cada máquina aponta para onde tem
/// as imagens (pasta local ou share de rede). Carrega sob demanda (ex: tooltip no Catálogo).
/// </summary>
public static class ImagemProduto
{
    // Lido uma vez do appsettings via o ServiceProvider global (App.Services).
    private static readonly string _pastaBase =
        App.Services.GetService(typeof(IConfiguration)) is IConfiguration cfg
            ? cfg["Imagens:PastaBase"] ?? string.Empty
            : string.Empty;

    /// <summary>Caminho completo do arquivo, ou null se o nome for vazio. NÃO garante existência.</summary>
    public static string? CaminhoDe(string? arquivo) =>
        string.IsNullOrWhiteSpace(arquivo) || string.IsNullOrWhiteSpace(_pastaBase)
            ? null
            : Path.Combine(_pastaBase, arquivo.Trim());

    /// <summary>True se o produto tem imagem e o arquivo existe fisicamente na pasta-base.</summary>
    public static bool Existe(string? arquivo)
    {
        var caminho = CaminhoDe(arquivo);
        return caminho is not null && File.Exists(caminho);
    }

    /// <summary>
    /// Carrega o Bitmap da imagem do produto, ou null se não houver imagem / o arquivo não existir
    /// / falhar ao decodificar. O chamador é dono do Bitmap e deve descartá-lo (Dispose) ao trocar.
    /// </summary>
    public static Bitmap? Carregar(string? arquivo)
    {
        var caminho = CaminhoDe(arquivo);
        if (caminho is null || !File.Exists(caminho))
            return null;

        try
        {
            return new Bitmap(caminho);
        }
        catch
        {
            // Arquivo corrompido ou não é uma imagem válida: degrada para "sem imagem".
            return null;
        }
    }
}
