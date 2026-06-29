using Microsoft.Extensions.Logging;
using Moq;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.Enums;
using ApiSgc.Services;
using ApiSgc.Services.Contracts;
using ApiSgc.Tests.Helpers;
using Xunit;

namespace ApiSgc.Tests.Services;

public class FrequenciaServiceTests
{
    private static FrequenciaService CreateService(ApplicationDbContext context)
    {
        var matriculaService = new MatriculaService(context, Mock.Of<ILogger<MatriculaService>>());
        return new FrequenciaService(context, Mock.Of<ILogger<FrequenciaService>>(), matriculaService);
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

    private static async Task<Usuario> CreateUsuarioAsync(ApplicationDbContext context)
    {
        var usuario = new Usuario
        {
            Nome = "Aluno Teste",
            Email = $"aluno{Guid.NewGuid()}@email.com",
            Senha = "senha123"
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        return usuario;
    }

    private static async Task<Matricula> CreateMatriculaAsync(ApplicationDbContext context, Turma? turma = null)
    {
        turma ??= await CreateTurmaAsync(context);
        var usuario = await CreateUsuarioAsync(context);

        var matricula = new Matricula
        {
            UsuarioId = usuario.Id,
            TurmaId = turma.Id
        };

        context.Matriculas.Add(matricula);
        await context.SaveChangesAsync();

        return matricula;
    }

    private static async Task<Aula> CreateAulaAsync(
        ApplicationDbContext context,
        Turma turma,
        string titulo = "Aula Teste",
        int numero = 1)
    {
        var aula = new Aula
        {
            TurmaId = turma.Id,
            Titulo = titulo,
        };

        context.Aulas.Add(aula);
        await context.SaveChangesAsync();

        return aula;
    }

    // CreateAsync

    [Fact]
    public async Task CreateAsync_Success_ReturnsEntityWithId()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var aula = await CreateAulaAsync(context, turma);

        var frequencia = new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula.Id,
            Status = StatusFrequencia.PRESENTE
        };

        var result = await service.CreateAsync(frequencia);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(matricula.Id, result.MatriculaId);
        Assert.Equal(aula.Id, result.AulaId);
        Assert.Equal(StatusFrequencia.PRESENTE, result.Status);
        Assert.Null(result.DeletedAt);

        var saved = await context.Frequencias.FindAsync(result.Id);
        Assert.NotNull(saved);
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_Exists_ReturnsFrequencia()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var aula = await CreateAulaAsync(context, turma);

        var frequencia = new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula.Id,
            Status = StatusFrequencia.PRESENTE
        };

        context.Frequencias.Add(frequencia);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(frequencia.Id);

        Assert.NotNull(result);
        Assert.Equal(frequencia.Id, result.Id);
        Assert.Equal(matricula.Id, result.MatriculaId);
        Assert.Equal(aula.Id, result.AulaId);
        Assert.Equal(StatusFrequencia.PRESENTE, result.Status);
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
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var aula = await CreateAulaAsync(context, turma);

        var frequencia = new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula.Id,
            Status = StatusFrequencia.PRESENTE,
            DeletedAt = DateTime.UtcNow
        };

        context.Frequencias.Add(frequencia);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(frequencia.Id);

        Assert.Null(result);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_Success_UpdatesEntity()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var aula = await CreateAulaAsync(context, turma);

        var frequencia = new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula.Id,
            Status = StatusFrequencia.PRESENTE
        };

        context.Frequencias.Add(frequencia);
        await context.SaveChangesAsync();

        frequencia.Status = StatusFrequencia.FALTA;

        var result = await service.UpdateAsync(frequencia);

        Assert.Equal(StatusFrequencia.FALTA, result.Status);

        var saved = await context.Frequencias.FindAsync(frequencia.Id);
        Assert.NotNull(saved);
        Assert.Equal(StatusFrequencia.FALTA, saved.Status);
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_Success_SoftDeletes()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var aula = await CreateAulaAsync(context, turma);

        var frequencia = new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula.Id,
            Status = StatusFrequencia.PRESENTE
        };

        context.Frequencias.Add(frequencia);
        await context.SaveChangesAsync();

        var result = await service.DeleteAsync(frequencia.Id);

        Assert.True(result);

        var saved = await context.Frequencias.FindAsync(frequencia.Id);
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
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var aula = await CreateAulaAsync(context, turma);

        var frequencia = new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula.Id,
            Status = StatusFrequencia.PRESENTE
        };

        context.Frequencias.Add(frequencia);
        await context.SaveChangesAsync();

        var result = await service.ExistsAsync(frequencia.Id);

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
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var aula = await CreateAulaAsync(context, turma);

        var frequencia = new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula.Id,
            Status = StatusFrequencia.PRESENTE,
            DeletedAt = DateTime.UtcNow
        };

        context.Frequencias.Add(frequencia);
        await context.SaveChangesAsync();

        var result = await service.ExistsAsync(frequencia.Id);

        Assert.False(result);
    }

    // ExistsByMatriculaAndAulaAsync

    [Fact]
    public async Task ExistsByMatriculaAndAulaAsync_Exists_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var aula = await CreateAulaAsync(context, turma);

        context.Frequencias.Add(new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula.Id,
            Status = StatusFrequencia.PRESENTE
        });
        await context.SaveChangesAsync();

        var result = await service.ExistsByMatriculaAndAulaAsync(matricula.Id, aula.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByMatriculaAndAulaAsync_NotExists_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var aula = await CreateAulaAsync(context, turma);

        var result = await service.ExistsByMatriculaAndAulaAsync(matricula.Id, aula.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByMatriculaAndAulaAsync_ExcludesOwnId()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var aula = await CreateAulaAsync(context, turma);

        var frequencia = new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula.Id,
            Status = StatusFrequencia.PRESENTE
        };

        context.Frequencias.Add(frequencia);
        await context.SaveChangesAsync();

        var result = await service.ExistsByMatriculaAndAulaAsync(matricula.Id, aula.Id, frequencia.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByMatriculaAndAulaAsync_SoftDeleted_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var aula = await CreateAulaAsync(context, turma);

        context.Frequencias.Add(new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula.Id,
            Status = StatusFrequencia.PRESENTE,
            DeletedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var result = await service.ExistsByMatriculaAndAulaAsync(matricula.Id, aula.Id);

        Assert.False(result);
    }

    // CalcularFrequenciaAsync

    [Fact]
    public async Task CalcularFrequenciaAsync_MatriculaNotFound_ReturnsZero()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.CalcularFrequenciaAsync(999);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CalcularFrequenciaAsync_TurmaSemAulas_ReturnsCem()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);

        var result = await service.CalcularFrequenciaAsync(matricula.Id);

        Assert.Equal(100, result);
    }

    [Fact]
    public async Task CalcularFrequenciaAsync_ComPresencasEFaltas_ReturnsPercentualCorreto()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);

        var aula1 = await CreateAulaAsync(context, turma, "Aula 1", 1);
        var aula2 = await CreateAulaAsync(context, turma, "Aula 2", 2);
        var aula3 = await CreateAulaAsync(context, turma, "Aula 3", 3);
        var aula4 = await CreateAulaAsync(context, turma, "Aula 4", 4);

        context.Frequencias.AddRange(
            new Frequencia { MatriculaId = matricula.Id, AulaId = aula1.Id, Status = StatusFrequencia.PRESENTE },
            new Frequencia { MatriculaId = matricula.Id, AulaId = aula2.Id, Status = StatusFrequencia.PRESENTE },
            new Frequencia { MatriculaId = matricula.Id, AulaId = aula3.Id, Status = StatusFrequencia.FALTA },
            new Frequencia { MatriculaId = matricula.Id, AulaId = aula4.Id, Status = StatusFrequencia.FALTA }
        );
        await context.SaveChangesAsync();

        var result = await service.CalcularFrequenciaAsync(matricula.Id);

        Assert.Equal(50, result);
    }

    [Fact]
    public async Task CalcularFrequenciaAsync_IgnoraFrequenciasSoftDeleted()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);

        var aula1 = await CreateAulaAsync(context, turma, "Aula 1", 1);
        var aula2 = await CreateAulaAsync(context, turma, "Aula 2", 2);

        context.Frequencias.AddRange(
            new Frequencia { MatriculaId = matricula.Id, AulaId = aula1.Id, Status = StatusFrequencia.PRESENTE },
            new Frequencia { MatriculaId = matricula.Id, AulaId = aula2.Id, Status = StatusFrequencia.PRESENTE, DeletedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var result = await service.CalcularFrequenciaAsync(matricula.Id);

        Assert.Equal(50, result);
    }
}