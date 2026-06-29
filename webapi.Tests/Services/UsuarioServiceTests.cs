using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.DTOs;
using ApiSgc.Models.Enums;
using ApiSgc.Services;
using ApiSgc.Tests.Helpers;
using Xunit;

namespace ApiSgc.Tests.Services;

public class UsuarioServiceTests
{
    private readonly Mock<ILogger<UsuarioService>> _loggerMock;

    public UsuarioServiceTests()
    {
        _loggerMock = new Mock<ILogger<UsuarioService>>();
    }

    private static UsuarioService CreateService(ApplicationDbContext context)
    {
        return new UsuarioService(context, Mock.Of<ILogger<UsuarioService>>());
    }

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllUsers()
    {
        using var context = TestHelpers.CreateDbContext();
        context.Usuarios.AddRange(
            TestHelpers.CreateUser("user1@email.com"),
            TestHelpers.CreateUser("user2@email.com"),
            TestHelpers.CreateUser("user3@email.com")
        );
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetAllAsync();

        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_FilterByNome_ReturnsMatching()
    {
        using var context = TestHelpers.CreateDbContext();
        var u1 = TestHelpers.CreateUser("john@email.com");
        u1.Nome = "João Silva";
        var u2 = TestHelpers.CreateUser("maria@email.com");
        u2.Nome = "Maria Santos";
        var u3 = TestHelpers.CreateUser("pedro@email.com");
        u3.Nome = "Pedro João";
        context.Usuarios.AddRange(u1, u2, u3);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (items, total) = await service.GetAllAsync(new UsuarioFiltroDto
        {
            Nome = "João"
        });

        Assert.Equal(2, total);
        Assert.All(items, u => Assert.Contains("João", u.Nome));
    }

    [Fact]
    public async Task GetAllAsync_FilterByEmail_ReturnsMatching()
    {
        using var context = TestHelpers.CreateDbContext();
        context.Usuarios.AddRange(
            TestHelpers.CreateUser("admin@sistema.com"),
            TestHelpers.CreateUser("aluno@sistema.com"),
            TestHelpers.CreateUser("professor@sistema.com")
        );
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (items, total) = await service.GetAllAsync(new UsuarioFiltroDto
        {
            Email = "aluno"
        });

        Assert.Equal(1, total);
        Assert.Contains("aluno", items.First().Email);
    }

    [Fact]
    public async Task GetAllAsync_FilterByRole_ReturnsMatching()
    {
        using var context = TestHelpers.CreateDbContext();
        context.Usuarios.AddRange(
            TestHelpers.CreateUser("admin@email.com", UserRole.ADMIN),
            TestHelpers.CreateUser("aluno1@email.com", UserRole.ALUNO),
            TestHelpers.CreateUser("aluno2@email.com", UserRole.ALUNO)
        );
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (items, total) = await service.GetAllAsync(new UsuarioFiltroDto
        {
            Role = UserRole.ALUNO
        });

        Assert.Equal(2, total);
        Assert.All(items, u => Assert.Equal(UserRole.ALUNO, u.Role));
    }

    [Fact]
    public async Task GetAllAsync_FilterByEstaEmCelula_ReturnsMatching()
    {
        using var context = TestHelpers.CreateDbContext();
        var u1 = TestHelpers.CreateUser("cel1@email.com");
        u1.EstaEmCelula = true;
        var u2 = TestHelpers.CreateUser("cel2@email.com");
        u2.EstaEmCelula = true;
        var u3 = TestHelpers.CreateUser("nocel@email.com");
        u3.EstaEmCelula = false;
        context.Usuarios.AddRange(u1, u2, u3);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (items, total) = await service.GetAllAsync(new UsuarioFiltroDto
        {
            EstaEmCelula = true
        });

        Assert.Equal(2, total);
        Assert.All(items, u => Assert.True(u.EstaEmCelula));
    }

    [Fact]
    public async Task GetAllAsync_FilterCombined_ReturnsIntersection()
    {
        using var context = TestHelpers.CreateDbContext();
        var u1 = TestHelpers.CreateUser("admin1@email.com", UserRole.ADMIN);
        u1.Nome = "Admin Silva";
        u1.EstaEmCelula = true;
        var u2 = TestHelpers.CreateUser("admin2@email.com", UserRole.ADMIN);
        u2.Nome = "Admin João";
        u2.EstaEmCelula = false;
        var u3 = TestHelpers.CreateUser("aluno@email.com", UserRole.ALUNO);
        u3.Nome = "Aluno Silva";
        u3.EstaEmCelula = true;
        context.Usuarios.AddRange(u1, u2, u3);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (items, total) = await service.GetAllAsync(new UsuarioFiltroDto
        {
            Nome = "Silva",
            Role = UserRole.ADMIN,
            EstaEmCelula = true
        });

        Assert.Equal(1, total);
        Assert.Equal("admin1@email.com", items.First().Email);
    }

    [Fact]
    public async Task GetAllAsync_Pagination_ReturnsCorrectPage()
    {
        using var context = TestHelpers.CreateDbContext();
        for (int i = 1; i <= 15; i++)
        {
            context.Usuarios.Add(TestHelpers.CreateUser($"user{i:D2}@email.com"));
        }
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (page1, total1) = await service.GetAllAsync(new UsuarioFiltroDto
        {
            Page = 1,
            PerPage = 10
        });

        var (page2, total2) = await service.GetAllAsync(new UsuarioFiltroDto
        {
            Page = 2,
            PerPage = 10
        });

        Assert.Equal(15, total1);
        Assert.Equal(10, page1.Count());
        Assert.Equal(15, total2);
        Assert.Equal(5, page2.Count());
    }

    [Fact]
    public async Task GetAsync_UserExists_ReturnsUser()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com");
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetAsync_UserNotFound_ReturnsNull()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.GetAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_Success_ReturnsEntity()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var user = TestHelpers.CreateUser("new@email.com");

        var result = await service.AddAsync(user);

        Assert.NotNull(result);
        Assert.Equal("new@email.com", result.Email);
        Assert.True(result.Id > 0);

        var saved = await context.Usuarios.FindAsync(result.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task UpdateAsync_Success_UpdatesEntity()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("original@email.com");
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        user.Nome = "Updated Name";
        user.Email = "updated@email.com";

        var result = await service.UpdateAsync(user);

        Assert.Equal("Updated Name", result.Nome);
        Assert.Equal("updated@email.com", result.Email);

        var saved = await context.Usuarios.FindAsync(user.Id);
        Assert.Equal("Updated Name", saved?.Nome);
        Assert.Equal("updated@email.com", saved?.Email);
    }

    [Fact]
    public async Task DeleteAsync_Success_RemovesEntity()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("delete@email.com");
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.DeleteAsync(user.Id);

        Assert.True(result);

        var saved = await context.Usuarios.FindAsync(user.Id);
        Assert.Null(saved);
    }

    [Fact]
    public async Task GetByEmailAsync_EmailExists_ReturnsUser()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("find@email.com");
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetByEmailAsync("find@email.com");

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_EmailNotFound_ReturnsNull()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.GetByEmailAsync("nonexistent@email.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_WithIdParameter_ExcludesOwnId()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("same@email.com");
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetByEmailAsync("same@email.com", user.Id);

        Assert.Null(result);
    }
}
