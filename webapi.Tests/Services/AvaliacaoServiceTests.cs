using Microsoft.Extensions.Logging;
using Moq;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Services;
using ApiSgc.Tests.Helpers;
using Xunit;

namespace ApiSgc.Tests.Services;

public class AvaliacaoServiceTests
{
    private static AvaliacaoService CreateService(ApplicationDbContext context)
    {
        return new AvaliacaoService(context, Mock.Of<ILogger<AvaliacaoService>>());
    }

    private static async Task<Curso> CreateCursoAsync(ApplicationDbContext context, string nome = "Curso Teste")
    {
        var curso = new Curso { Nome = nome };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();
        return curso;
    }

    private static async Task<Turma> CreateTurmaAsync(
        ApplicationDbContext context,
        string nome = "Turma Teste",
        bool necessitaAtividades = false)
    {
        var curso = await CreateCursoAsync(context);
        var turma = new Turma
        {
            CursoId = curso.Id,
            Nome = nome,
            NecessitaAtividades = necessitaAtividades,
        };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();
        return turma;
    }

    // CreateAsync

    [Fact]
    public async Task CreateAsync_Success_ReturnsEntityWithId()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        var avaliacao = new Avaliacao
        {
            TurmaId = turma.Id,
            Nome = "Prova 1 - Doutrina",
        };

        var result = await service.CreateAsync(avaliacao);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(turma.Id, result.TurmaId);
        Assert.Equal("Prova 1 - Doutrina", result.Nome);
        Assert.Null(result.DeletedAt);

        var saved = await context.Avaliacoes.FindAsync(result.Id);
        Assert.NotNull(saved);
    }

    // GetAllAsync

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAll()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        context.Avaliacoes.AddRange(
            new Avaliacao { TurmaId = turma.Id, Nome = "Avaliacao A" },
            new Avaliacao { TurmaId = turma.Id, Nome = "Avaliacao B" },
            new Avaliacao { TurmaId = turma.Id, Nome = "Avaliacao C" }
        );
        await context.SaveChangesAsync();

        var (items, total) = await service.GetAllAsync(null, 1, 10);

        Assert.Equal(3, total);
        Assert.Equal(3, items.Count());
    }

    [Fact]
    public async Task GetAllAsync_FilterByTurmaId_ReturnsMatching()
    {
        using var context = TestHelpers.CreateDbContext();
        var turmaA = await CreateTurmaAsync(context, "Turma A");
        var turmaB = await CreateTurmaAsync(context, "Turma B");
        var service = CreateService(context);

        context.Avaliacoes.AddRange(
            new Avaliacao { TurmaId = turmaA.Id, Nome = "Avaliacao Turma A" },
            new Avaliacao { TurmaId = turmaB.Id, Nome = "Avaliacao Turma B" }
        );
        await context.SaveChangesAsync();

        var (items, total) = await service.GetAllAsync(turmaA.Id, 1, 10);

        Assert.Equal(1, total);
        Assert.Equal("Avaliacao Turma A", items.First().Nome);
    }

    [Fact]
    public async Task GetAllAsync_Pagination_ReturnsCorrectPage()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        for (int i = 1; i <= 15; i++)
        {
            context.Avaliacoes.Add(new Avaliacao
            {
                TurmaId = turma.Id,
                Nome = $"Avaliacao {i:D2}",
            });
        }
        await context.SaveChangesAsync();

        var (page1, total1) = await service.GetAllAsync(null, 1, 10);
        var (page2, total2) = await service.GetAllAsync(null, 2, 10);

        Assert.Equal(15, total1);
        Assert.Equal(10, page1.Count());
        Assert.Equal(15, total2);
        Assert.Equal(5, page2.Count());
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeleted()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        context.Avaliacoes.AddRange(
            new Avaliacao { TurmaId = turma.Id, Nome = "Ativa" },
            new Avaliacao { TurmaId = turma.Id, Nome = "Deletada", DeletedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var (items, total) = await service.GetAllAsync(null, 1, 10);

        Assert.Equal(1, total);
        Assert.Equal("Ativa", items.First().Nome);
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_Exists_ReturnsAvaliacao()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        var avaliacao = new Avaliacao
        {
            TurmaId = turma.Id,
            Nome = "Prova Teológica",
        };
        context.Avaliacoes.Add(avaliacao);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(avaliacao.Id);

        Assert.NotNull(result);
        Assert.Equal(avaliacao.Id, result.Id);
        Assert.Equal("Prova Teológica", result.Nome);
        Assert.Equal(turma.Id, result.TurmaId);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeleted_ReturnsNull()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        var avaliacao = new Avaliacao
        {
            TurmaId = turma.Id,
            Nome = "Deletada",
            DeletedAt = DateTime.UtcNow,
        };
        context.Avaliacoes.Add(avaliacao);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(avaliacao.Id);

        Assert.Null(result);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_Success_UpdatesEntity()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        var avaliacao = new Avaliacao
        {
            TurmaId = turma.Id,
            Nome = "Nome Original",
        };
        context.Avaliacoes.Add(avaliacao);
        await context.SaveChangesAsync();

        avaliacao.Nome = "Nome Atualizado";

        var result = await service.UpdateAsync(avaliacao);

        Assert.Equal("Nome Atualizado", result.Nome);

        var saved = await context.Avaliacoes.FindAsync(avaliacao.Id);
        Assert.Equal("Nome Atualizado", saved?.Nome);
    }

    [Fact]
    public async Task UpdateAsync_ChangeTurma_UpdatesTurmaLink()
    {
        using var context = TestHelpers.CreateDbContext();
        var turmaOriginal = await CreateTurmaAsync(context, "Turma Original");
        var turmaNova = await CreateTurmaAsync(context, "Turma Nova");
        var service = CreateService(context);

        var avaliacao = new Avaliacao
        {
            TurmaId = turmaOriginal.Id,
            Nome = "Avaliacao",
        };
        context.Avaliacoes.Add(avaliacao);
        await context.SaveChangesAsync();

        avaliacao.TurmaId = turmaNova.Id;

        var result = await service.UpdateAsync(avaliacao);

        Assert.Equal(turmaNova.Id, result.TurmaId);

        var saved = await context.Avaliacoes.FindAsync(avaliacao.Id);
        Assert.Equal(turmaNova.Id, saved?.TurmaId);
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_Success_SoftDeletes()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        var avaliacao = new Avaliacao
        {
            TurmaId = turma.Id,
            Nome = "Para deletar",
        };
        context.Avaliacoes.Add(avaliacao);
        await context.SaveChangesAsync();

        var result = await service.DeleteAsync(avaliacao.Id);

        Assert.True(result);

        var saved = await context.Avaliacoes.FindAsync(avaliacao.Id);
        Assert.NotNull(saved);
        Assert.NotNull(saved.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.DeleteAsync(999);

        Assert.False(result);
    }

    // ExistsAsync

    [Fact]
    public async Task ExistsAsync_IdExists_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        var avaliacao = new Avaliacao
        {
            TurmaId = turma.Id,
            Nome = "Existente",
        };
        context.Avaliacoes.Add(avaliacao);
        await context.SaveChangesAsync();

        var result = await service.ExistsAsync(avaliacao.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_IdNotExists_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.ExistsAsync(999);

        Assert.False(result);
    }

    // ExistsByTurmaAsync

    [Fact]
    public async Task ExistsByTurmaAsync_TurmaExists_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        var result = await service.ExistsByTurmaAsync(turma.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByTurmaAsync_TurmaNotFound_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.ExistsByTurmaAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByTurmaAsync_SoftDeletedTurma_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var turma = new Turma
        {
            CursoId = curso.Id,
            Nome = "Turma Deletada",
            DeletedAt = DateTime.UtcNow,
        };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.ExistsByTurmaAsync(turma.Id);

        Assert.False(result);
    }

    // TurmaNecessitaAtividadesAsync

    [Fact]
    public async Task TurmaNecessitaAtividadesAsync_True_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context, "Turma Atividades", necessitaAtividades: true);
        var service = CreateService(context);

        var result = await service.TurmaNecessitaAtividadesAsync(turma.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task TurmaNecessitaAtividadesAsync_False_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context, "Turma Sem Atividades", necessitaAtividades: false);
        var service = CreateService(context);

        var result = await service.TurmaNecessitaAtividadesAsync(turma.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task TurmaNecessitaAtividadesAsync_TurmaNotFound_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.TurmaNecessitaAtividadesAsync(999);

        Assert.False(result);
    }
}
