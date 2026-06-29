using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Services;
using ApiSgc.Tests.Helpers;
using Xunit;

namespace ApiSgc.Tests.Services;

public class CursoServiceTests
{
    private static CursoService CreateService(ApplicationDbContext context)
    {
        return new CursoService(context, Mock.Of<ILogger<CursoService>>());
    }

    [Fact]
    public async Task CreateAsync_Success_ReturnsEntityWithId()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var curso = new Curso
        {
            Nome = "Curso Teste",
            Descricao = "Descricao teste",
        };

        var result = await service.CreateAsync(curso);

        Assert.NotNull(result);
        Assert.Equal("Curso Teste", result.Nome);
        Assert.Equal("Descricao teste", result.Descricao);
        Assert.True(result.Id > 0);
        Assert.Null(result.DeletedAt);

        var saved = await context.Cursos.FindAsync(result.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllCursos()
    {
        using var context = TestHelpers.CreateDbContext();
        context.Cursos.AddRange(
            new Curso { Nome = "Curso A" },
            new Curso { Nome = "Curso B" },
            new Curso { Nome = "Curso C" }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (items, total) = await service.GetAllAsync(null, 1, 10);

        Assert.Equal(3, total);
        Assert.Equal(3, items.Count());
    }

    [Fact]
    public async Task GetAllAsync_FilterByNome_ReturnsMatching()
    {
        using var context = TestHelpers.CreateDbContext();
        context.Cursos.AddRange(
            new Curso { Nome = "Formação de Líderes" },
            new Curso { Nome = "Discipulado Básico" },
            new Curso { Nome = "Escola Bíblica" }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (items, total) = await service.GetAllAsync("Líderes", 1, 10);

        Assert.Equal(1, total);
        Assert.Contains("Líderes", items.First().Nome);
    }

    [Fact]
    public async Task GetAllAsync_Pagination_ReturnsCorrectPage()
    {
        using var context = TestHelpers.CreateDbContext();
        for (int i = 1; i <= 15; i++)
        {
            context.Cursos.Add(new Curso { Nome = $"Curso {i:D2}" });
        }
        await context.SaveChangesAsync();

        var service = CreateService(context);

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
        context.Cursos.Add(new Curso { Nome = "Ativo" });
        context.Cursos.Add(new Curso
        {
            Nome = "Deletado",
            DeletedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (items, total) = await service.GetAllAsync(null, 1, 10);

        Assert.Equal(1, total);
        Assert.Equal("Ativo", items.First().Nome);
    }

    [Fact]
    public async Task GetByIdAsync_Exists_ReturnsCurso()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = new Curso
        {
            Nome = "Curso Teste",
        };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetByIdAsync(curso.Id);

        Assert.NotNull(result);
        Assert.Equal(curso.Id, result.Id);
        Assert.Equal("Curso Teste", result.Nome);
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
        var curso = new Curso
        {
            Nome = "Deletado",
            DeletedAt = DateTime.UtcNow,
        };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetByIdAsync(curso.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Success_UpdatesEntity()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = new Curso
        {
            Nome = "Original",
            Descricao = "Original desc",
        };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        curso.Nome = "Updated";
        curso.Descricao = "Updated desc";

        var result = await service.UpdateAsync(curso);

        Assert.Equal("Updated", result.Nome);
        Assert.Equal("Updated desc", result.Descricao);

        var saved = await context.Cursos.FindAsync(curso.Id);
        Assert.Equal("Updated", saved?.Nome);
    }

    [Fact]
    public async Task DeleteAsync_Success_SoftDeletes()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = new Curso { Nome = "Para deletar" };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.DeleteAsync(curso.Id);

        Assert.True(result);

        var saved = await context.Cursos.FindAsync(curso.Id);
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

    [Fact]
    public async Task ExistsAsync_IdExists_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = new Curso { Nome = "Existente" };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.ExistsAsync(curso.Id);

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

    [Fact]
    public async Task ExistsByNomeAsync_NomeExists_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        context.Cursos.Add(new Curso { Nome = "Unico" });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.ExistsByNomeAsync("Unico");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByNomeAsync_NomeNotExists_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.ExistsByNomeAsync("Inexistente");

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByNomeAsync_NomeExists_ExcludesOwnId()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = new Curso { Nome = "Curso" };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.ExistsByNomeAsync("Curso", curso.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByNomeAsync_SoftDeleted_Excluded()
    {
        using var context = TestHelpers.CreateDbContext();
        context.Cursos.Add(new Curso
        {
            Nome = "Deletado",
            DeletedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.ExistsByNomeAsync("Deletado");

        Assert.False(result);
    }
}
