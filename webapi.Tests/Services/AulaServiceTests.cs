using Microsoft.Extensions.Logging;
using Moq;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Services;
using ApiSgc.Tests.Helpers;
using Xunit;

namespace ApiSgc.Tests.Services;

public class AulaServiceTests
{
    private static AulaService CreateService(ApplicationDbContext context)
    {
        return new AulaService(context, Mock.Of<ILogger<AulaService>>());
    }

    private static async Task<Curso> CreateCursoAsync(ApplicationDbContext context, string nome = "Curso Teste")
    {
        var curso = new Curso { Nome = nome };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();
        return curso;
    }

    private static async Task<Turma> CreateTurmaAsync(ApplicationDbContext context, string nome = "Turma Teste")
    {
        var curso = await CreateCursoAsync(context);
        var turma = new Turma
        {
            CursoId = curso.Id,
            Nome = nome
        };

        context.Turmas.Add(turma);
        await context.SaveChangesAsync();
        return turma;
    }

    private static async Task<Professor> CreateProfessorAsync(
        ApplicationDbContext context,
        string nome = "Professor Teste",
        bool ativo = true)
    {
        var professor = new Professor
        {
            Nome = nome,
            Email = $"{nome.Replace(" ", "").ToLower()}@email.com",
            Telefone = "31999999999",
            Ativo = ativo
        };

        context.Professores.Add(professor);
        await context.SaveChangesAsync();
        return professor;
    }

    // CreateAsync

    [Fact]
    public async Task CreateAsync_Success_ReturnsEntityWithId()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var professor = await CreateProfessorAsync(context);
        var service = CreateService(context);

        var aula = new Aula
        {
            TurmaId = turma.Id,
            ProfessorId = professor.Id,
            Titulo = "Aula Teste",
            DataAula = new DateOnly(2026, 4, 22),
            Descricao = "Descrição da aula"
        };

        var result = await service.CreateAsync(aula);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(turma.Id, result.TurmaId);
        Assert.Equal(professor.Id, result.ProfessorId);
        Assert.Equal("Aula Teste", result.Titulo);
        Assert.Null(result.DeletedAt);

        var saved = await context.Aulas.FindAsync(result.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task CreateAsync_WithoutProfessor_SavesSuccessfully()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        var aula = new Aula
        {
            TurmaId = turma.Id,
            ProfessorId = null,
            Titulo = "Aula Sem Professor",
        };

        var result = await service.CreateAsync(aula);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Null(result.ProfessorId);
        Assert.Equal("Aula Sem Professor", result.Titulo);
    }

    // GetAllAsync

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllAulas()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var professor = await CreateProfessorAsync(context);
        var service = CreateService(context);

        context.Aulas.AddRange(
            new Aula { TurmaId = turma.Id, ProfessorId = professor.Id, Titulo = "Aula A" },
            new Aula { TurmaId = turma.Id, ProfessorId = professor.Id, Titulo = "Aula B" },
            new Aula { TurmaId = turma.Id, ProfessorId = professor.Id, Titulo = "Aula C" }
        );
        await context.SaveChangesAsync();

        var (items, total) = await service.GetAllAsync(null, null, 1, 10);

        Assert.Equal(3, total);
        Assert.Equal(3, items.Count());
    }

    [Fact]
    public async Task GetAllAsync_FilterByTitulo_ReturnsMatching()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        context.Aulas.AddRange(
            new Aula { TurmaId = turma.Id, Titulo = "Introdução Bíblica" },
            new Aula { TurmaId = turma.Id, Titulo = "Discipulado Básico" },
            new Aula { TurmaId = turma.Id, Titulo = "Liderança Cristã" }
        );
        await context.SaveChangesAsync();

        var (items, total) = await service.GetAllAsync("Discipulado", null, 1, 10);

        Assert.Equal(1, total);
        Assert.Contains("Discipulado", items.First().Titulo);
    }

    [Fact]
    public async Task GetAllAsync_Pagination_ReturnsCorrectPage()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        for (int i = 1; i <= 15; i++)
        {
            context.Aulas.Add(new Aula
            {
                TurmaId = turma.Id,
                Titulo = $"Aula {i:D2}",
            });
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
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        context.Aulas.AddRange(
            new Aula { TurmaId = turma.Id, Titulo = "Aula Ativa" },
            new Aula { TurmaId = turma.Id, Titulo = "Aula Deletada", DeletedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var (items, total) = await service.GetAllAsync(null, null, 1, 10);

        Assert.Equal(1, total);
        Assert.Equal("Aula Ativa", items.First().Titulo);
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_Exists_ReturnsAula()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var professor = await CreateProfessorAsync(context);
        var service = CreateService(context);

        var aula = new Aula
        {
            TurmaId = turma.Id,
            ProfessorId = professor.Id,
            Titulo = "Aula Teste",
        };

        context.Aulas.Add(aula);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(aula.Id);

        Assert.NotNull(result);
        Assert.Equal(aula.Id, result.Id);
        Assert.Equal("Aula Teste", result.Titulo);
        Assert.Equal(turma.Id, result.TurmaId);
        Assert.Equal(professor.Id, result.ProfessorId);
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

        var aula = new Aula
        {
            TurmaId = turma.Id,
            Titulo = "Aula Deletada",
            DeletedAt = DateTime.UtcNow
        };

        context.Aulas.Add(aula);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(aula.Id);

        Assert.Null(result);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_Success_UpdatesEntity()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var professor = await CreateProfessorAsync(context);
        var service = CreateService(context);

        var aula = new Aula
        {
            TurmaId = turma.Id,
            ProfessorId = professor.Id,
            Titulo = "Título Original",
            Descricao = "Descrição original"
        };

        context.Aulas.Add(aula);
        await context.SaveChangesAsync();

        aula.Titulo = "Título Atualizado";
        aula.Descricao = "Descrição atualizada";

        var result = await service.UpdateAsync(aula);

        Assert.Equal("Título Atualizado", result.Titulo);
        Assert.Equal("Descrição atualizada", result.Descricao);

        var saved = await context.Aulas.FindAsync(aula.Id);
        Assert.Equal("Título Atualizado", saved?.Titulo);
    }

    [Fact]
    public async Task UpdateAsync_ChangeProfessor_UpdatesProfessorLink()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var professorOriginal = await CreateProfessorAsync(context, "Professor Original");
        var professorNovo = await CreateProfessorAsync(context, "Professor Novo");
        var service = CreateService(context);

        var aula = new Aula
        {
            TurmaId = turma.Id,
            ProfessorId = professorOriginal.Id,
            Titulo = "Aula com Professor",
        };

        context.Aulas.Add(aula);
        await context.SaveChangesAsync();

        aula.ProfessorId = professorNovo.Id;

        var result = await service.UpdateAsync(aula);

        Assert.Equal(professorNovo.Id, result.ProfessorId);

        var saved = await context.Aulas.FindAsync(aula.Id);
        Assert.Equal(professorNovo.Id, saved?.ProfessorId);
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_Success_SoftDeletes()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        var aula = new Aula
        {
            TurmaId = turma.Id,
            Titulo = "Aula Para Deletar",
        };

        context.Aulas.Add(aula);
        await context.SaveChangesAsync();

        var result = await service.DeleteAsync(aula.Id);

        Assert.True(result);

        var saved = await context.Aulas.FindAsync(aula.Id);
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

        var aula = new Aula
        {
            TurmaId = turma.Id,
            Titulo = "Aula Existente",
        };

        context.Aulas.Add(aula);
        await context.SaveChangesAsync();

        var result = await service.ExistsAsync(aula.Id);

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
    public async Task ExistsAsync_SoftDeleted_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        var aula = new Aula
        {
            TurmaId = turma.Id,
            Titulo = "Aula Deletada",
            DeletedAt = DateTime.UtcNow
        };

        context.Aulas.Add(aula);
        await context.SaveChangesAsync();

        var result = await service.ExistsAsync(aula.Id);

        Assert.False(result);
    }

    // ExistsByTurmaAsync

    [Fact]
    public async Task ExistsByTurmaAsync_ActiveTurma_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var turma = await CreateTurmaAsync(context);
        var service = CreateService(context);

        var result = await service.ExistsByTurmaAsync(turma.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByTurmaAsync_NotFound_ReturnsFalse()
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
            DeletedAt = DateTime.UtcNow
        };

        context.Turmas.Add(turma);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.ExistsByTurmaAsync(turma.Id);

        Assert.False(result);
    }

    // ExistsByProfessorAsync

    [Fact]
    public async Task ExistsByProfessorAsync_ActiveProfessor_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var professor = await CreateProfessorAsync(context, "Professor Ativo", true);
        var service = CreateService(context);

        var result = await service.ExistsByProfessorAsync(professor.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByProfessorAsync_InactiveProfessor_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var professor = await CreateProfessorAsync(context, "Professor Inativo", false);
        var service = CreateService(context);

        var result = await service.ExistsByProfessorAsync(professor.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByProfessorAsync_NotFound_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.ExistsByProfessorAsync(999);

        Assert.False(result);
    }
}
