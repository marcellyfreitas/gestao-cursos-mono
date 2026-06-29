using Microsoft.Extensions.Logging;
using Moq;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Services;
using ApiSgc.Tests.Helpers;
using Xunit;

namespace ApiSgc.Tests.Services;

public class TurmaServiceTests
{
    private static TurmaService CreateService(ApplicationDbContext context)
    {
        return new TurmaService(context, Mock.Of<ILogger<TurmaService>>());
    }

    private static async Task<Curso> CreateCursoAsync(ApplicationDbContext context, string nome = "Curso Teste")
    {
        var curso = new Curso { Nome = nome };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();
        return curso;
    }

    // CreateAsync

    [Fact]
    public async Task CreateAsync_Success_ReturnsEntityWithId()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var turma = new Turma
        {
            CursoId = curso.Id,
            Nome = "Turma Teste",
            NecessitaAtividades = true,
            FaltasParaReprovacao = 3,
        };

        var result = await service.CreateAsync(turma);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Turma Teste", result.Nome);
        Assert.Equal(curso.Id, result.CursoId);
        Assert.True(result.NecessitaAtividades);
        Assert.Equal(3, result.FaltasParaReprovacao);
        Assert.Null(result.DeletedAt);

        var saved = await context.Turmas.FindAsync(result.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task CreateAsync_WithDefaults_SavesSuccessfully()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var turma = new Turma
        {
            CursoId = curso.Id,
            Nome = "Turma Sem Configurações",
        };

        var result = await service.CreateAsync(turma);

        Assert.NotNull(result);
        Assert.False(result.NecessitaAtividades);
        Assert.Equal(0, result.FaltasParaReprovacao);
    }

    // GetAllAsync

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllTurmas()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        context.Turmas.AddRange(
            new Turma { CursoId = curso.Id, Nome = "Turma A" },
            new Turma { CursoId = curso.Id, Nome = "Turma B" },
            new Turma { CursoId = curso.Id, Nome = "Turma C" }
        );
        await context.SaveChangesAsync();

        var (items, total) = await service.GetAllAsync(null, null, 1, 10);

        Assert.Equal(3, total);
        Assert.Equal(3, items.Count());
    }

    [Fact]
    public async Task GetAllAsync_FilterByNome_ReturnsMatching()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        context.Turmas.AddRange(
            new Turma { CursoId = curso.Id, Nome = "Turma Alpha" },
            new Turma { CursoId = curso.Id, Nome = "Turma Beta" },
            new Turma { CursoId = curso.Id, Nome = "Turma Gamma" }
        );
        await context.SaveChangesAsync();

        var (items, total) = await service.GetAllAsync(null, "Alpha", 1, 10);

        Assert.Equal(1, total);
        Assert.Contains("Alpha", items.First().Nome);
    }

    [Fact]
    public async Task GetAllAsync_FilterByCursoId_ReturnsMatching()
    {
        using var context = TestHelpers.CreateDbContext();
        var cursoA = await CreateCursoAsync(context, "Curso A");
        var cursoB = await CreateCursoAsync(context, "Curso B");
        var service = CreateService(context);

        context.Turmas.AddRange(
            new Turma { CursoId = cursoA.Id, Nome = "Turma A1" },
            new Turma { CursoId = cursoA.Id, Nome = "Turma A2" },
            new Turma { CursoId = cursoB.Id, Nome = "Turma B1" }
        );
        await context.SaveChangesAsync();

        var (items, total) = await service.GetAllAsync(cursoA.Id, null, 1, 10);

        Assert.Equal(2, total);
        Assert.All(items, t => Assert.Equal(cursoA.Id, t.CursoId));
    }

    [Fact]
    public async Task GetAllAsync_Pagination_ReturnsCorrectPage()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        for (int i = 1; i <= 15; i++)
        {
            context.Turmas.Add(new Turma { CursoId = curso.Id, Nome = $"Turma {i:D2}" });
        }
        await context.SaveChangesAsync();

        var (page1, total1) = await service.GetAllAsync(null, null, 1, 10);
        var (page2, total2) = await service.GetAllAsync(null, null, 2, 10);

        Assert.Equal(15, total1);
        Assert.Equal(10, page1.Count());
        Assert.Equal(15, total2);
        Assert.Equal(5, page2.Count());
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeleted()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        context.Turmas.AddRange(
            new Turma { CursoId = curso.Id, Nome = "Ativa" },
            new Turma { CursoId = curso.Id, Nome = "Deletada", DeletedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var (items, total) = await service.GetAllAsync(null, null, 1, 10);

        Assert.Equal(1, total);
        Assert.Equal("Ativa", items.First().Nome);
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_Exists_ReturnsTurma()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var turma = new Turma
        {
            CursoId = curso.Id,
            Nome = "Turma Teste",
            NecessitaAtividades = true,
            FaltasParaReprovacao = 5,
        };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(turma.Id);

        Assert.NotNull(result);
        Assert.Equal(turma.Id, result.Id);
        Assert.Equal("Turma Teste", result.Nome);
        Assert.True(result.NecessitaAtividades);
        Assert.Equal(5, result.FaltasParaReprovacao);
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
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var turma = new Turma
        {
            CursoId = curso.Id,
            Nome = "Deletada",
            DeletedAt = DateTime.UtcNow,
        };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(turma.Id);

        Assert.Null(result);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_Success_UpdatesEntity()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var turma = new Turma
        {
            CursoId = curso.Id,
            Nome = "Original",
            NecessitaAtividades = false,
            FaltasParaReprovacao = 0,
        };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();

        turma.Nome = "Atualizada";
        turma.NecessitaAtividades = true;
        turma.FaltasParaReprovacao = 5;

        var result = await service.UpdateAsync(turma);

        Assert.Equal("Atualizada", result.Nome);
        Assert.True(result.NecessitaAtividades);
        Assert.Equal(5, result.FaltasParaReprovacao);

        var saved = await context.Turmas.FindAsync(turma.Id);
        Assert.Equal("Atualizada", saved?.Nome);
        Assert.True(saved?.NecessitaAtividades);
        Assert.Equal(5, saved?.FaltasParaReprovacao);
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_Success_SoftDeletes()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var turma = new Turma { CursoId = curso.Id, Nome = "Para deletar" };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();

        var result = await service.DeleteAsync(turma.Id);

        Assert.True(result);

        var saved = await context.Turmas.FindAsync(turma.Id);
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
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var turma = new Turma { CursoId = curso.Id, Nome = "Existente" };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();

        var result = await service.ExistsAsync(turma.Id);

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

    // ExistsByNomeAndCursoAsync

    [Fact]
    public async Task ExistsByNomeAndCursoAsync_Exists_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        context.Turmas.Add(new Turma { CursoId = curso.Id, Nome = "Turma Única" });
        await context.SaveChangesAsync();

        var result = await service.ExistsByNomeAndCursoAsync("Turma Única", curso.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByNomeAndCursoAsync_NotExists_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var result = await service.ExistsByNomeAndCursoAsync("Inexistente", curso.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByNomeAndCursoAsync_ExcludesOwnId()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var turma = new Turma { CursoId = curso.Id, Nome = "Turma" };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();

        var result = await service.ExistsByNomeAndCursoAsync("Turma", curso.Id, turma.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByNomeAndCursoAsync_SoftDeleted_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        context.Turmas.Add(new Turma
        {
            CursoId = curso.Id,
            Nome = "Deletada",
            DeletedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var result = await service.ExistsByNomeAndCursoAsync("Deletada", curso.Id);

        Assert.False(result);
    }

    // PossuiMatriculasAtivasAsync

    [Fact]
    public async Task PossuiMatriculasAtivasAsync_ComMatriculas_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var turma = new Turma { CursoId = curso.Id, Nome = "Turma Com Alunos" };
        context.Turmas.Add(turma);

        var usuario = new Usuario { Nome = "Aluno Teste", Email = "aluno@teste.com" };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        context.Matriculas.Add(new Matricula
        {
            TurmaId = turma.Id,
            UsuarioId = usuario.Id,
        });
        await context.SaveChangesAsync();

        var result = await service.PossuiMatriculasAtivasAsync(turma.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task PossuiMatriculasAtivasAsync_ComMultiplasMatriculas_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var turma = new Turma { CursoId = curso.Id, Nome = "Turma Com Vários Alunos" };
        context.Turmas.Add(turma);

        var usuario1 = new Usuario { Nome = "Aluno 1", Email = "aluno1@teste.com" };
        var usuario2 = new Usuario { Nome = "Aluno 2", Email = "aluno2@teste.com" };
        context.Usuarios.AddRange(usuario1, usuario2);
        await context.SaveChangesAsync();

        context.Matriculas.AddRange(
            new Matricula { TurmaId = turma.Id, UsuarioId = usuario1.Id },
            new Matricula { TurmaId = turma.Id, UsuarioId = usuario2.Id }
        );
        await context.SaveChangesAsync();

        var result = await service.PossuiMatriculasAtivasAsync(turma.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task PossuiMatriculasAtivasAsync_SemMatriculas_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var turma = new Turma { CursoId = curso.Id, Nome = "Turma Vazia" };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();

        var result = await service.PossuiMatriculasAtivasAsync(turma.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task PossuiMatriculasAtivasAsync_MatriculasSoftDeleted_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var curso = await CreateCursoAsync(context);
        var service = CreateService(context);

        var turma = new Turma { CursoId = curso.Id, Nome = "Turma Com Alunos Deletados" };
        context.Turmas.Add(turma);

        var usuario = new Usuario { Nome = "Aluno Teste", Email = "aluno@teste.com" };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        context.Matriculas.Add(new Matricula
        {
            TurmaId = turma.Id,
            UsuarioId = usuario.Id,
            DeletedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var result = await service.PossuiMatriculasAtivasAsync(turma.Id);

        Assert.False(result);
    }
}