using Microsoft.Extensions.Logging;
using Moq;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Services;
using ApiSgc.Tests.Helpers;
using Xunit;

namespace ApiSgc.Tests.Services;

public class ProfessorServiceTests
{
    private static ProfessorService CreateService(ApplicationDbContext context)
    {
        return new ProfessorService(context, Mock.Of<ILogger<ProfessorService>>());
    }

    // CreateAsync

    [Fact]
    public async Task CreateAsync_Success_ReturnsEntityWithId()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var professor = new Professor
        {
            Nome = "Kevin Lucas",
            Email = "kevin@email.com",
            Telefone = "31999999999"
        };

        var result = await service.CreateAsync(professor);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Kevin Lucas", result.Nome);
        Assert.Equal("kevin@email.com", result.Email);
        Assert.Equal("31999999999", result.Telefone);
        Assert.True(result.Ativo);

        var saved = await context.Professores.FindAsync(result.Id);
        Assert.NotNull(saved);
    }

    // GetAllAsync

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllProfessores()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        context.Professores.AddRange(
            new Professor { Nome = "Professor A", Email = "a@email.com" },
            new Professor { Nome = "Professor B", Email = "b@email.com" },
            new Professor { Nome = "Professor C", Email = "c@email.com" }
        );
        await context.SaveChangesAsync();

        var (items, total) = await service.GetAllAsync(null, 1, 10);

        Assert.Equal(3, total);
        Assert.Equal(3, items.Count());
    }

    [Fact]
    public async Task GetAllAsync_FilterByNome_ReturnsMatching()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        context.Professores.AddRange(
            new Professor { Nome = "Kevin Lucas", Email = "kevin@email.com" },
            new Professor { Nome = "João Pedro", Email = "joao@email.com" }
        );
        await context.SaveChangesAsync();

        var (items, total) = await service.GetAllAsync("Kevin", 1, 10);

        Assert.Equal(1, total);
        Assert.Contains("Kevin", items.First().Nome);
    }

    [Fact]
    public async Task GetAllAsync_Pagination_ReturnsCorrectPage()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        for (int i = 1; i <= 15; i++)
        {
            context.Professores.Add(new Professor
            {
                Nome = $"Professor {i:D2}",
                Email = $"professor{i}@email.com"
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

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_Exists_ReturnsProfessor()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var professor = new Professor
        {
            Nome = "Kevin Lucas",
            Email = "kevin@email.com",
            Telefone = "31999999999"
        };

        context.Professores.Add(professor);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(professor.Id);

        Assert.NotNull(result);
        Assert.Equal(professor.Id, result.Id);
        Assert.Equal("Kevin Lucas", result.Nome);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_Success_UpdatesEntity()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var professor = new Professor
        {
            Nome = "Nome Original",
            Email = "original@email.com",
            Telefone = "31999999999"
        };

        context.Professores.Add(professor);
        await context.SaveChangesAsync();

        professor.Nome = "Nome Atualizado";
        professor.Email = "atualizado@email.com";
        professor.Telefone = "31888888888";

        var result = await service.UpdateAsync(professor);

        Assert.Equal("Nome Atualizado", result.Nome);
        Assert.Equal("atualizado@email.com", result.Email);
        Assert.Equal("31888888888", result.Telefone);

        var saved = await context.Professores.FindAsync(professor.Id);
        Assert.Equal("Nome Atualizado", saved?.Nome);
    }

    // ToggleStatusAsync

    [Fact]
    public async Task ToggleStatusAsync_ActiveProfessor_Inactivates()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var professor = new Professor
        {
            Nome = "Professor Ativo",
            Email = "ativo@email.com",
            Ativo = true
        };

        context.Professores.Add(professor);
        await context.SaveChangesAsync();

        var result = await service.ToggleStatusAsync(professor.Id);

        Assert.True(result);

        var saved = await context.Professores.FindAsync(professor.Id);
        Assert.NotNull(saved);
        Assert.False(saved.Ativo);
    }

    [Fact]
    public async Task ToggleStatusAsync_InactiveProfessor_Activates()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var professor = new Professor
        {
            Nome = "Professor Inativo",
            Email = "inativo@email.com",
            Ativo = false
        };

        context.Professores.Add(professor);
        await context.SaveChangesAsync();

        var result = await service.ToggleStatusAsync(professor.Id);

        Assert.True(result);

        var saved = await context.Professores.FindAsync(professor.Id);
        Assert.NotNull(saved);
        Assert.True(saved.Ativo);
    }

    [Fact]
    public async Task ToggleStatusAsync_NotFound_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.ToggleStatusAsync(999);

        Assert.False(result);
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_Success_RemovesProfessor()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var professor = new Professor
        {
            Nome = "Para excluir",
            Email = "excluir@email.com"
        };

        context.Professores.Add(professor);
        await context.SaveChangesAsync();

        var result = await service.DeleteAsync(professor.Id);

        Assert.True(result);

        var saved = await context.Professores.FindAsync(professor.Id);
        Assert.Null(saved);
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
        var service = CreateService(context);

        var professor = new Professor
        {
            Nome = "Existente",
            Email = "existente@email.com"
        };

        context.Professores.Add(professor);
        await context.SaveChangesAsync();

        var result = await service.ExistsAsync(professor.Id);

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

    // ExistsByNomeAsync

    [Fact]
    public async Task ExistsByNomeAsync_Exists_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        context.Professores.Add(new Professor
        {
            Nome = "Kevin Lucas",
            Email = "kevin@email.com"
        });
        await context.SaveChangesAsync();

        var result = await service.ExistsByNomeAsync("Kevin Lucas");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByNomeAsync_NotExists_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.ExistsByNomeAsync("Professor Inexistente");

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByNomeAsync_ExcludesOwnId()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var professor = new Professor
        {
            Nome = "Kevin Lucas",
            Email = "kevin@email.com"
        };

        context.Professores.Add(professor);
        await context.SaveChangesAsync();

        var result = await service.ExistsByNomeAsync("Kevin Lucas", professor.Id);

        Assert.False(result);
    }

    // ExistsByEmailAsync

    [Fact]
    public async Task ExistsByEmailAsync_Exists_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        context.Professores.Add(new Professor
        {
            Nome = "Kevin Lucas",
            Email = "kevin@email.com"
        });
        await context.SaveChangesAsync();

        var result = await service.ExistsByEmailAsync("kevin@email.com");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByEmailAsync_NotExists_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.ExistsByEmailAsync("naoexiste@email.com");

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByEmailAsync_EmptyEmail_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.ExistsByEmailAsync("");

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ExcludesOwnId()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var professor = new Professor
        {
            Nome = "Kevin Lucas",
            Email = "kevin@email.com"
        };

        context.Professores.Add(professor);
        await context.SaveChangesAsync();

        var result = await service.ExistsByEmailAsync("kevin@email.com", professor.Id);

        Assert.False(result);
    }
}