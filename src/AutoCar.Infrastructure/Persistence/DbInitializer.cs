using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoCar.Infrastructure.Persistence;

/// <summary>
/// Aplica migrations pendentes e garante o seed inicial de forma idempotente.
/// Roda no startup da aplicação. Cria um usuário admin padrão na primeira execução
/// para permitir o primeiro login (senha provisória — trocar depois).
/// </summary>
public class DbInitializer
{
    private readonly AppDbContext _db;
    private readonly IHashSenha _hash;
    private readonly ILogger<DbInitializer> _logger;

    // Credenciais do usuário administrador padrão. Documentadas no CLAUDE.md.
    private const string AdminLogin = "julio";
    private const string AdminNome = "Julio";
    private const string AdminSenhaProvisoria = "123";

    public DbInitializer(AppDbContext db, IHashSenha hash, ILogger<DbInitializer> logger)
    {
        _db = db;
        _hash = hash;
        _logger = logger;
    }

    /// <summary>
    /// Aplica migrations e garante o seed. Síncrono de propósito: é chamado no
    /// Program.Main, ANTES de iniciar o lifetime do Avalonia, fora do contexto STA
    /// de UI — evita o deadlock de bloquear o thread de UI esperando trabalho async.
    /// </summary>
    public void Inicializar()
    {
        _db.Database.Migrate();
        GarantirAdmin();
        GarantirProdutosDemo();
    }

    private void GarantirAdmin()
    {
        var jaExiste = _db.Usuarios.Any(u => u.Perfil == PerfilUsuario.Admin);
        if (jaExiste)
            return;

        var admin = new Usuario(
            nome: AdminNome,
            login: AdminLogin,
            senhaHash: _hash.Gerar(AdminSenhaProvisoria),
            perfil: PerfilUsuario.Admin);

        _db.Usuarios.Add(admin);
        _db.SaveChanges();

        _logger.LogInformation(
            "Usuário admin padrão criado (login: {Login}).", AdminLogin);
    }

    /// <summary>
    /// Seed de produtos de demonstração (idempotente). Só roda se ainda não houver mais de um
    /// produto cadastrado — assim não duplica nem atrapalha dados reais. Cria categorias, marcas
    /// e produtos variados com aplicações por veículo, para exercitar a busca do Catálogo.
    /// </summary>
    private void GarantirProdutosDemo()
    {
        // Heurística simples de idempotência: se já há vários produtos, assume que o seed rodou
        // (ou que há dados reais) e não faz nada.
        if (_db.Produtos.Count() > 1)
            return;

        // Categorias e marcas auxiliares (reaproveita as que já existirem pela descrição).
        var freios = ObterOuCriarCategoria("FREIOS");
        var suspensao = ObterOuCriarCategoria("SUSPENSAO");
        var motor = ObterOuCriarCategoria("MOTOR");
        var filtros = ObterOuCriarCategoria("FILTROS");

        var cofap = ObterOuCriarMarca("COFAP");
        var bosch = ObterOuCriarMarca("BOSCH");
        var ngk = ObterOuCriarMarca("NGK");
        var fram = ObterOuCriarMarca("FRAM");
        var nakata = ObterOuCriarMarca("NAKATA");
        _db.SaveChanges(); // garante Ids das categorias/marcas antes de referenciar nos produtos

        // (descrição, categoria, marca, posição, custo, venda, [aplicações: montadora, modelo, anoIni, anoFim, obs])
        CriarProduto("AMORTECEDOR DIANTEIRO", suspensao, cofap, PosicaoPeca.Dianteira, 120m, 210m, new[]
        {
            ("VW", "GOL", (int?)2008, (int?)2014, (string?)"1.0/1.6"),
            ("VW", "VOYAGE", 2008, 2014, null),
            ("FIAT", "PALIO", 2004, 2012, null),
        });

        CriarProduto("AMORTECEDOR TRASEIRO", suspensao, nakata, PosicaoPeca.Traseira, 95m, 175m, new[]
        {
            ("VW", "GOL", (int?)2008, (int?)2014, (string?)null),
            ("CHEVROLET", "CORSA", 2002, 2012, null),
        });

        CriarProduto("PASTILHA DE FREIO DIANTEIRA", freios, bosch, PosicaoPeca.Dianteira, 45m, 89m, new[]
        {
            ("VW", "GOL", (int?)1996, (int?)2014, (string?)"todos"),
            ("FIAT", "UNO", 2010, null, "Mille/Way"),
        });

        CriarProduto("DISCO DE FREIO VENTILADO", freios, cofap, PosicaoPeca.Dianteira, 130m, 240m, new[]
        {
            ("VW", "GOL", (int?)2008, (int?)2014, (string?)null),
            ("FORD", "KA", 2008, 2014, null),
        });

        CriarProduto("VELA DE IGNICAO", motor, ngk, PosicaoPeca.NaoAplica, 18m, 35m, new[]
        {
            ("VW", "GOL", (int?)1996, (int?)2020, (string?)null),
            ("FIAT", "PALIO", 1996, 2020, null),
            ("CHEVROLET", "CELTA", 2000, 2015, null),
        });

        CriarProduto("CORREIA DENTADA", motor, bosch, PosicaoPeca.NaoAplica, 60m, 115m, new[]
        {
            ("FIAT", "PALIO", (int?)2004, (int?)2012, (string?)"1.0 Fire"),
        });

        CriarProduto("FILTRO DE OLEO", filtros, fram, PosicaoPeca.NaoAplica, 15m, 32m, new[]
        {
            ("VW", "GOL", (int?)1996, (int?)2020, (string?)null),
            ("FIAT", "UNO", 1996, 2020, null),
            ("CHEVROLET", "CORSA", 1996, 2012, null),
            ("FORD", "KA", 2000, 2020, null),
        });

        CriarProduto("FILTRO DE AR", filtros, fram, PosicaoPeca.NaoAplica, 22m, 45m, new[]
        {
            ("VW", "GOL", (int?)2008, (int?)2014, (string?)null),
            ("VW", "FOX", 2003, 2014, null),
        });

        _db.SaveChanges();
        _logger.LogInformation("Seed de produtos de demonstração criado.");
    }

    private CategoriaProduto ObterOuCriarCategoria(string descricao)
    {
        var existente = _db.Categorias.FirstOrDefault(c => c.Descricao == descricao);
        if (existente is not null)
            return existente;

        var nova = new CategoriaProduto(descricao);
        _db.Categorias.Add(nova);
        return nova;
    }

    private Marca ObterOuCriarMarca(string descricao)
    {
        var existente = _db.Marcas.FirstOrDefault(m => m.Descricao == descricao);
        if (existente is not null)
            return existente;

        var nova = new Marca(descricao);
        _db.Marcas.Add(nova);
        return nova;
    }

    private void CriarProduto(
        string descricao,
        CategoriaProduto categoria,
        Marca marca,
        PosicaoPeca posicao,
        decimal custo,
        decimal venda,
        (string montadora, string modelo, int? anoIni, int? anoFim, string? obs)[] aplicacoes)
    {
        var produto = new Produto(
            categoria.Id, descricao, descricaoComplementar: null, codBarras: null,
            codFabricante: null, UnidadeMedida.UN, posicao, LadoPeca.NaoAplica, custo, venda, marca.Id, idFornecedor: null);

        // Seed mantém motorização/combustível neutros (NaoAplica) — a observação do seed já cobre
        // os detalhes de motor onde existem ("1.0/1.6", "1.0 Fire").
        produto.DefinirAplicacoes(aplicacoes.Select(a =>
            new ProdutoAplicacao(a.montadora, a.modelo, a.anoIni, a.anoFim,
                motorizacao: null, Combustivel.NaoAplica, a.obs)));

        _db.Produtos.Add(produto);
    }
}
