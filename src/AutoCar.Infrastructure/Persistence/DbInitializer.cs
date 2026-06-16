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
        GarantirPosicoesELados();
        GarantirProdutosDemo();
        GarantirGruposDemo();
    }

    /// <summary>
    /// Garante os grupos base do catálogo demo (idempotente — roda sempre). Cria os grupos nas
    /// categorias demo e vincula os produtos demo que ainda não têm grupo. Necessário porque o seed
    /// de produtos só roda em banco vazio: sem este método, um banco que já tem produtos (mas foi
    /// criado antes do conceito de Grupo) nunca ganharia os grupos.
    /// </summary>
    private void GarantirGruposDemo()
    {
        var categorias = _db.Categorias.ToList();
        CategoriaProduto? Cat(string d) => categorias.FirstOrDefault(c => c.Descricao == d);

        var suspensao = Cat("SUSPENSAO");
        var freios = Cat("FREIOS");
        var motor = Cat("MOTOR");
        var filtros = Cat("FILTROS");
        if (suspensao is null || freios is null || motor is null || filtros is null)
            return; // sem as categorias demo não há onde pendurar os grupos

        var gAmortecedor = ObterOuCriarGrupo("AMORTECEDOR", suspensao);
        var gPastilha = ObterOuCriarGrupo("PASTILHA", freios);
        var gDisco = ObterOuCriarGrupo("DISCO", freios);
        var gVela = ObterOuCriarGrupo("VELA", motor);
        var gCorreia = ObterOuCriarGrupo("CORREIA", motor);
        var gFiltro = ObterOuCriarGrupo("FILTRO", filtros);
        _db.SaveChanges();

        // Vincula os produtos demo ao grupo certo (por descrição), só os que ainda não têm grupo.
        VincularGrupoDemo("AMORTECEDOR DIANTEIRO", gAmortecedor);
        VincularGrupoDemo("AMORTECEDOR TRASEIRO", gAmortecedor);
        VincularGrupoDemo("PASTILHA DE FREIO DIANTEIRA", gPastilha);
        VincularGrupoDemo("DISCO DE FREIO VENTILADO", gDisco);
        VincularGrupoDemo("VELA DE IGNICAO", gVela);
        VincularGrupoDemo("CORREIA DENTADA", gCorreia);
        VincularGrupoDemo("FILTRO DE OLEO", gFiltro);
        VincularGrupoDemo("FILTRO DE AR", gFiltro);
        _db.SaveChanges();
    }

    private void VincularGrupoDemo(string descricaoProduto, GrupoProduto grupo)
    {
        var produto = _db.Produtos.FirstOrDefault(p => p.Descricao == descricaoProduto && p.IdGrupo == null);
        produto?.DefinirGrupo(grupo.Id);
    }

    /// <summary>
    /// Seed idempotente das posições e lados base do domínio automotivo. Diferente do seed demo de
    /// produtos, este é dado REAL (o usuário usa Dianteira/Traseira/Esquerdo/Direito de fato) — roda
    /// sempre, criando só o que faltar. O usuário pode adicionar/editar/inativar pelo cadastro depois.
    /// </summary>
    private void GarantirPosicoesELados()
    {
        ObterOuCriarPosicao("DIANTEIRA");
        ObterOuCriarPosicao("TRASEIRA");
        ObterOuCriarLado("ESQUERDO");
        ObterOuCriarLado("DIREITO");
        _db.SaveChanges();
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

        // Posições base (já garantidas em GarantirPosicoesELados — aqui só recupera para vincular).
        var dianteira = ObterOuCriarPosicao("DIANTEIRA");
        var traseira = ObterOuCriarPosicao("TRASEIRA");

        // Grupos demo (nível Categoria → Grupo → Produto). O Id da categoria já existe (Guid gerado
        // no construtor), então dá para vincular o grupo à categoria antes do SaveChanges.
        var gAmortecedor = ObterOuCriarGrupo("AMORTECEDOR", suspensao);
        var gPastilha = ObterOuCriarGrupo("PASTILHA", freios);
        var gDisco = ObterOuCriarGrupo("DISCO", freios);
        var gVela = ObterOuCriarGrupo("VELA", motor);
        var gCorreia = ObterOuCriarGrupo("CORREIA", motor);
        var gFiltro = ObterOuCriarGrupo("FILTRO", filtros);
        _db.SaveChanges(); // garante Ids das categorias/marcas/posições/grupos antes de referenciar nos produtos

        // (descrição, categoria, grupo, marca, posição, custo, venda, [aplicações: montadora, modelo, anoIni, anoFim, obs])
        CriarProduto("AMORTECEDOR DIANTEIRO", suspensao, gAmortecedor, cofap, dianteira, 120m, 210m, new[]
        {
            ("VW", "GOL", (int?)2008, (int?)2014, (string?)"1.0/1.6"),
            ("VW", "VOYAGE", 2008, 2014, null),
            ("FIAT", "PALIO", 2004, 2012, null),
        });

        CriarProduto("AMORTECEDOR TRASEIRO", suspensao, gAmortecedor, nakata, traseira, 95m, 175m, new[]
        {
            ("VW", "GOL", (int?)2008, (int?)2014, (string?)null),
            ("CHEVROLET", "CORSA", 2002, 2012, null),
        });

        CriarProduto("PASTILHA DE FREIO DIANTEIRA", freios, gPastilha, bosch, dianteira, 45m, 89m, new[]
        {
            ("VW", "GOL", (int?)1996, (int?)2014, (string?)"todos"),
            ("FIAT", "UNO", 2010, null, "Mille/Way"),
        });

        CriarProduto("DISCO DE FREIO VENTILADO", freios, gDisco, cofap, dianteira, 130m, 240m, new[]
        {
            ("VW", "GOL", (int?)2008, (int?)2014, (string?)null),
            ("FORD", "KA", 2008, 2014, null),
        });

        CriarProduto("VELA DE IGNICAO", motor, gVela, ngk, posicao: null, 18m, 35m, new[]
        {
            ("VW", "GOL", (int?)1996, (int?)2020, (string?)null),
            ("FIAT", "PALIO", 1996, 2020, null),
            ("CHEVROLET", "CELTA", 2000, 2015, null),
        });

        CriarProduto("CORREIA DENTADA", motor, gCorreia, bosch, posicao: null, 60m, 115m, new[]
        {
            ("FIAT", "PALIO", (int?)2004, (int?)2012, (string?)"1.0 Fire"),
        });

        CriarProduto("FILTRO DE OLEO", filtros, gFiltro, fram, posicao: null, 15m, 32m, new[]
        {
            ("VW", "GOL", (int?)1996, (int?)2020, (string?)null),
            ("FIAT", "UNO", 1996, 2020, null),
            ("CHEVROLET", "CORSA", 1996, 2012, null),
            ("FORD", "KA", 2000, 2020, null),
        });

        CriarProduto("FILTRO DE AR", filtros, gFiltro, fram, posicao: null, 22m, 45m, new[]
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

    private GrupoProduto ObterOuCriarGrupo(string descricao, CategoriaProduto categoria)
    {
        var existente = _db.GruposProduto.FirstOrDefault(g => g.Descricao == descricao && g.IdCategoria == categoria.Id);
        if (existente is not null)
            return existente;

        var novo = new GrupoProduto(descricao, categoria.Id);
        _db.GruposProduto.Add(novo);
        return novo;
    }

    private PosicaoPeca ObterOuCriarPosicao(string descricao)
    {
        var existente = _db.PosicoesPeca.FirstOrDefault(p => p.Descricao == descricao);
        if (existente is not null)
            return existente;

        var nova = new PosicaoPeca(descricao);
        _db.PosicoesPeca.Add(nova);
        return nova;
    }

    private LadoPeca ObterOuCriarLado(string descricao)
    {
        var existente = _db.LadosPeca.FirstOrDefault(l => l.Descricao == descricao);
        if (existente is not null)
            return existente;

        var nova = new LadoPeca(descricao);
        _db.LadosPeca.Add(nova);
        return nova;
    }

    private void CriarProduto(
        string descricao,
        CategoriaProduto categoria,
        GrupoProduto grupo,
        Marca marca,
        PosicaoPeca? posicao,
        decimal custo,
        decimal venda,
        (string montadora, string modelo, int? anoIni, int? anoFim, string? obs)[] aplicacoes)
    {
        var produto = new Produto(
            categoria.Id, descricao, descricaoComplementar: null, codBarras: null,
            codFabricante: null, UnidadeMedida.UN, posicao?.Id, idLado: null, custo, venda, marca.Id, idFornecedor: null, idGrupo: grupo.Id);

        // Seed mantém motorização/combustível neutros (NaoAplica) — a observação do seed já cobre
        // os detalhes de motor onde existem ("1.0/1.6", "1.0 Fire").
        produto.DefinirAplicacoes(aplicacoes.Select(a =>
            new ProdutoAplicacao(a.montadora, a.modelo, a.anoIni, a.anoFim,
                motorizacao: null, Combustivel.NaoAplica, a.obs)));

        _db.Produtos.Add(produto);
    }
}
