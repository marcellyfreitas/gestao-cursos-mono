using Microsoft.EntityFrameworkCore;
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

/// <summary>
/// Bug Condition Exploration Tests — These tests encode the EXPECTED CORRECT behavior.
/// They are written BEFORE any fix is applied and are EXPECTED TO FAIL on unfixed code.
/// Failure confirms the bugs exist. After fixes are applied, these tests should PASS.
/// 
/// **Validates: Requirements 2.3, 2.4, 2.7, 2.8**
/// </summary>
public class BugConditionExplorationTests
{
    private readonly Mock<ILogger<FrequenciaService>> _frequenciaLoggerMock = new();
    private readonly Mock<ILogger<MatriculaService>> _matriculaLoggerMock = new();
    private readonly Mock<ILogger<UsuarioService>> _usuarioLoggerMock = new();

    #region Helpers

    private FrequenciaService CreateFrequenciaService(ApplicationDbContext context)
    {
        var matriculaService = new MatriculaService(context, _matriculaLoggerMock.Object);
        return new FrequenciaService(context, _frequenciaLoggerMock.Object, matriculaService);
    }

    private MatriculaService CreateMatriculaService(ApplicationDbContext context)
    {
        return new MatriculaService(context, _matriculaLoggerMock.Object);
    }

    private UsuarioService CreateUsuarioService(ApplicationDbContext context)
    {
        return new UsuarioService(context, _usuarioLoggerMock.Object);
    }

    private static async Task<Curso> SeedCurso(ApplicationDbContext context, string nome = "Curso Teste")
    {
        var curso = new Curso { Nome = nome };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();
        return curso;
    }

    private static async Task<Turma> SeedTurma(
        ApplicationDbContext context,
        Curso curso,
        int faltasParaReprovacao = 3,
        bool necessitaAtividades = false,
        decimal? mediaMinima = null)
    {
        var turma = new Turma
        {
            CursoId = curso.Id,
            Nome = $"Turma {Guid.NewGuid():N}",
            FaltasParaReprovacao = faltasParaReprovacao,
            NecessitaAtividades = necessitaAtividades,
            MediaMinima = mediaMinima,
        };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();
        return turma;
    }

    private static async Task<Usuario> SeedUsuario(ApplicationDbContext context, string? email = null)
    {
        var usuario = new Usuario
        {
            Nome = $"Aluno {Guid.NewGuid():N}",
            Email = email ?? $"aluno{Guid.NewGuid():N}@email.com",
            Senha = BCrypt.Net.BCrypt.HashPassword("senha123"),
            Role = UserRole.ALUNO,
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        return usuario;
    }

    private static async Task<Matricula> SeedMatricula(
        ApplicationDbContext context,
        Usuario usuario,
        Turma turma,
        SituacaoMatricula situacao = SituacaoMatricula.CURSANDO,
        DateTime? deletedAt = null)
    {
        var matricula = new Matricula
        {
            UsuarioId = usuario.Id,
            TurmaId = turma.Id,
            Situacao = situacao,
            DeletedAt = deletedAt,
        };
        context.Matriculas.Add(matricula);
        await context.SaveChangesAsync();
        return matricula;
    }

    private static async Task<Aula> SeedAula(ApplicationDbContext context, Turma turma, string titulo = "Aula Teste")
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

    private static async Task SeedFaltas(ApplicationDbContext context, Matricula matricula, int totalFaltas)
    {
        for (int i = 0; i < totalFaltas; i++)
        {
            var aula = new Aula
            {
                TurmaId = matricula.TurmaId,
                Titulo = $"Aula Falta {i + 1}",
            };
            context.Aulas.Add(aula);
            await context.SaveChangesAsync();

            context.Frequencias.Add(new Frequencia
            {
                MatriculaId = matricula.Id,
                AulaId = aula.Id,
                Status = StatusFrequencia.FALTA,
            });
        }
        await context.SaveChangesAsync();
    }

    private static async Task<List<Avaliacao>> SeedAvaliacoes(ApplicationDbContext context, Turma turma, int quantidade)
    {
        var avaliacoes = new List<Avaliacao>();
        for (int i = 0; i < quantidade; i++)
        {
            var avaliacao = new Avaliacao
            {
                TurmaId = turma.Id,
                Nome = $"Avaliacao {i + 1}",
            };
            context.Avaliacoes.Add(avaliacao);
            avaliacoes.Add(avaliacao);
        }
        await context.SaveChangesAsync();
        return avaliacoes;
    }

    #endregion

    #region Bug 3: GetAlunosComFrequenciaAsync uses >= instead of >

    /// <summary>
    /// Bug 3: When totalFaltas == turma.FaltasParaReprovacao, the student should NOT be
    /// marked as Reprovado because FaltasParaReprovacao means "maximum allowed faltas" (inclusive).
    /// 
    /// Current buggy code uses >= which incorrectly marks the student as Reprovado
    /// when they are exactly at the limit.
    /// 
    /// Expected: Reprovado = false (student is at the limit, not over it)
    /// Actual (bug): Reprovado = true (because >= treats limit as exceeded)
    /// 
    /// **Validates: Requirements 2.3**
    /// </summary>
    [Fact]
    public async Task Bug3_GetAlunosComFrequencia_FaltasEqualToLimit_ShouldNotBeReprovado()
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var curso = await SeedCurso(context);
        var turma = await SeedTurma(context, curso, faltasParaReprovacao: 3);
        var usuario = await SeedUsuario(context);
        var matricula = await SeedMatricula(context, usuario, turma);

        // Create the aula we'll query
        var aulaAlvo = await SeedAula(context, turma, "Aula Alvo");

        // Seed exactly 3 faltas (== FaltasParaReprovacao)
        await SeedFaltas(context, matricula, totalFaltas: 3);

        var service = CreateFrequenciaService(context);

        // Act
        var result = await service.GetAlunosComFrequenciaAsync(aulaAlvo.Id);

        // Assert — student at the limit should NOT be reprovado
        var aluno = result.FirstOrDefault(a => a.MatriculaId == matricula.Id);
        Assert.NotNull(aluno);
        Assert.Equal(3, aluno.TotalFaltas);
        // Bug 3: Student with totalFaltas == FaltasParaReprovacao should NOT be marked as Reprovado.
        // FaltasParaReprovacao=3 means up to 3 faltas are allowed (inclusive).
        Assert.False(aluno.Reprovado);
    }

    #endregion

    #region Bug 4: CalcularAprovacaoAsync doesn't prioritize frequency reprovação

    /// <summary>
    /// Bug 4: When NecessitaAtividades=true and totalFaltas > FaltasParaReprovacao but notas
    /// are incomplete, the system should immediately set REPROVADO_FREQUENCIA.
    /// 
    /// Current buggy code checks notas first and returns CURSANDO when notas are incomplete,
    /// even though the student has already exceeded the falta limit.
    /// 
    /// Expected: Situacao = REPROVADO_FREQUENCIA (frequency reprovação has priority)
    /// Actual (bug): Situacao = CURSANDO (because notas incomplete check runs first)
    /// 
    /// **Validates: Requirements 2.4**
    /// </summary>
    [Fact]
    public async Task Bug4_CalcularAprovacao_FaltasExceedLimit_NotasIncompletas_ShouldBeReprovadoFrequencia()
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var curso = await SeedCurso(context);
        var turma = await SeedTurma(context, curso,
            faltasParaReprovacao: 3,
            necessitaAtividades: true,
            mediaMinima: 7.0m);
        var usuario = await SeedUsuario(context);
        var matricula = await SeedMatricula(context, usuario, turma);

        // Create 2 avaliacoes for the turma
        var avaliacoes = await SeedAvaliacoes(context, turma, 2);

        // Seed 5 faltas (> FaltasParaReprovacao=3)
        await SeedFaltas(context, matricula, totalFaltas: 5);

        // Add a presença so frequenciaPendente is false (at least 1 frequencia lançada)
        var aulaPresenca = new Aula { TurmaId = turma.Id, Titulo = "Aula Presença" };
        context.Aulas.Add(aulaPresenca);
        await context.SaveChangesAsync();
        context.Frequencias.Add(new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aulaPresenca.Id,
            Status = StatusFrequencia.PRESENTE,
        });
        await context.SaveChangesAsync();

        // Only add 1 nota (incomplete — turma has 2 avaliacoes)
        context.Notas.Add(new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacoes[0].Id,
            Valor = 8.0m,
        });
        await context.SaveChangesAsync();

        var service = CreateMatriculaService(context);

        // Act
        var result = await service.CalcularAprovacaoAsync(matricula.Id);

        // Assert — frequency reprovação should take priority over incomplete notas
        // Bug 4: When totalFaltas > FaltasParaReprovacao, status should be REPROVADO_FREQUENCIA
        // regardless of whether notas are complete. Frequency reprovação has priority.
        Assert.Equal(SituacaoMatricula.REPROVADO_FREQUENCIA, result.Situacao);
    }

    /// <summary>
    /// Bug 4 variant: Even with NO notas at all, if faltas exceed limit, should be REPROVADO_FREQUENCIA.
    /// 
    /// **Validates: Requirements 2.4**
    /// </summary>
    [Fact]
    public async Task Bug4_CalcularAprovacao_FaltasExceedLimit_NoNotas_ShouldBeReprovadoFrequencia()
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var curso = await SeedCurso(context);
        var turma = await SeedTurma(context, curso,
            faltasParaReprovacao: 2,
            necessitaAtividades: true,
            mediaMinima: 7.0m);
        var usuario = await SeedUsuario(context);
        var matricula = await SeedMatricula(context, usuario, turma);

        // Create avaliacoes
        await SeedAvaliacoes(context, turma, 1);

        // Seed 4 faltas (> FaltasParaReprovacao=2)
        await SeedFaltas(context, matricula, totalFaltas: 4);

        // Add a presença so frequenciaPendente is false
        var aulaPresenca = new Aula { TurmaId = turma.Id, Titulo = "Aula Presença" };
        context.Aulas.Add(aulaPresenca);
        await context.SaveChangesAsync();
        context.Frequencias.Add(new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aulaPresenca.Id,
            Status = StatusFrequencia.PRESENTE,
        });
        await context.SaveChangesAsync();

        // NO notas at all
        var service = CreateMatriculaService(context);

        // Act
        var result = await service.CalcularAprovacaoAsync(matricula.Id);

        // Assert — Bug 4: Even with no notas, if faltas exceed limit, should be REPROVADO_FREQUENCIA.
        Assert.Equal(SituacaoMatricula.REPROVADO_FREQUENCIA, result.Situacao);
    }

    #endregion

    #region Bug 7: Reactivation doesn't clean old Frequencia/Nota records

    /// <summary>
    /// Bug 7: When a soft-deleted matrícula is reactivated, old Frequencia and Nota records
    /// should be hard-deleted so the student starts fresh.
    /// 
    /// Current buggy code only resets matrícula fields but keeps old records.
    /// 
    /// Expected: After reactivation, no Frequencia/Nota records exist for the matrícula
    /// Actual (bug): Old Frequencia/Nota records remain linked to the reactivated matrícula
    /// 
    /// **Validates: Requirements 2.7**
    /// </summary>
    [Fact]
    public async Task Bug7_Reactivation_ShouldDeleteOldFrequenciaRecords()
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var curso = await SeedCurso(context);
        var turma = await SeedTurma(context, curso);
        var usuario = await SeedUsuario(context);

        // Create a soft-deleted matrícula with old frequencia records
        var matricula = await SeedMatricula(context, usuario, turma,
            situacao: SituacaoMatricula.REPROVADO_FREQUENCIA,
            deletedAt: DateTime.UtcNow.AddDays(-1));

        var aula1 = await SeedAula(context, turma, "Aula Antiga 1");
        var aula2 = await SeedAula(context, turma, "Aula Antiga 2");
        context.Frequencias.Add(new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula1.Id,
            Status = StatusFrequencia.FALTA,
        });
        context.Frequencias.Add(new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula2.Id,
            Status = StatusFrequencia.PRESENTE,
        });
        await context.SaveChangesAsync();

        var service = CreateMatriculaService(context);

        // Act — reactivate by creating a new matrícula for same usuario+turma
        var novaMatricula = new Matricula
        {
            UsuarioId = usuario.Id,
            TurmaId = turma.Id,
            Situacao = SituacaoMatricula.CURSANDO,
        };
        var result = await service.CreateAsync(novaMatricula);

        // Assert — old frequencia records should be deleted
        // Bug 7: After reactivation, old Frequencia records should be hard-deleted.
        // Student should start fresh without inherited attendance data.
        var frequenciasRestantes = await context.Frequencias
            .Where(f => f.MatriculaId == result.Id)
            .ToListAsync();

        Assert.Empty(frequenciasRestantes);
    }

    /// <summary>
    /// Bug 7: Reactivation should also delete old Nota records.
    /// 
    /// **Validates: Requirements 2.7**
    /// </summary>
    [Fact]
    public async Task Bug7_Reactivation_ShouldDeleteOldNotaRecords()
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var curso = await SeedCurso(context);
        var turma = await SeedTurma(context, curso, necessitaAtividades: true, mediaMinima: 7.0m);
        var usuario = await SeedUsuario(context);

        // Create a soft-deleted matrícula with old nota records
        var matricula = await SeedMatricula(context, usuario, turma,
            situacao: SituacaoMatricula.REPROVADO_NOTA,
            deletedAt: DateTime.UtcNow.AddDays(-1));

        var avaliacoes = await SeedAvaliacoes(context, turma, 2);
        context.Notas.Add(new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacoes[0].Id,
            Valor = 3.0m,
        });
        context.Notas.Add(new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacoes[1].Id,
            Valor = 4.0m,
        });
        await context.SaveChangesAsync();

        var service = CreateMatriculaService(context);

        // Act — reactivate
        var novaMatricula = new Matricula
        {
            UsuarioId = usuario.Id,
            TurmaId = turma.Id,
            Situacao = SituacaoMatricula.CURSANDO,
        };
        var result = await service.CreateAsync(novaMatricula);

        // Assert — old nota records should be deleted
        // Bug 7: After reactivation, old Nota records should be hard-deleted.
        // Student should start fresh without inherited grade data.
        var notasRestantes = await context.Notas
            .Where(n => n.MatriculaId == result.Id)
            .ToListAsync();

        Assert.Empty(notasRestantes);
    }

    #endregion

    #region Bug 8: Delete usuario with matrículas throws FK constraint

    /// <summary>
    /// Bug 8: Deleting a user that has matrículas should cascade-delete all related records
    /// (matrículas, frequências, notas) without throwing an FK constraint violation.
    /// 
    /// Current buggy code calls Remove(usuario) directly, which fails because
    /// Matricula→Usuario has DeleteBehavior.Restrict.
    /// 
    /// Expected: User is deleted successfully (cascade removes matrículas and dependents)
    /// Actual (bug): DbUpdateException / FK constraint violation
    /// 
    /// **Validates: Requirements 2.8**
    /// </summary>
    [Fact]
    public async Task Bug8_DeleteUsuario_WithMatriculas_ShouldNotThrowException()
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var curso = await SeedCurso(context);
        var turma = await SeedTurma(context, curso);
        var usuario = await SeedUsuario(context);

        // Create matrícula for the user
        var matricula = await SeedMatricula(context, usuario, turma);

        // Add frequencia and nota records
        var aula = await SeedAula(context, turma);
        context.Frequencias.Add(new Frequencia
        {
            MatriculaId = matricula.Id,
            AulaId = aula.Id,
            Status = StatusFrequencia.PRESENTE,
        });
        await context.SaveChangesAsync();

        var avaliacoes = await SeedAvaliacoes(context, turma, 1);
        context.Notas.Add(new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacoes[0].Id,
            Valor = 8.0m,
        });
        await context.SaveChangesAsync();

        var service = CreateUsuarioService(context);

        // Act & Assert — should NOT throw exception
        // Bug 8: Deleting a user with matrículas should not throw an FK constraint exception.
        // The system should cascade-delete matrículas (and their frequências/notas) before removing the user.
        var exception = await Record.ExceptionAsync(() => service.DeleteAsync(usuario.Id));
        Assert.Null(exception);
    }

    /// <summary>
    /// Bug 8: After deleting a user with matrículas, the user should no longer exist in the database.
    /// 
    /// **Validates: Requirements 2.8**
    /// </summary>
    [Fact]
    public async Task Bug8_DeleteUsuario_WithMatriculas_UserShouldBeRemoved()
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var curso = await SeedCurso(context);
        var turma = await SeedTurma(context, curso);
        var usuario = await SeedUsuario(context);
        var matricula = await SeedMatricula(context, usuario, turma);

        var service = CreateUsuarioService(context);

        // Act — attempt to delete (may throw on unfixed code)
        try
        {
            await service.DeleteAsync(usuario.Id);
        }
        catch
        {
            // If it throws, the bug is confirmed — user still exists
        }

        // Assert — user should be removed from database
        var userStillExists = await context.Usuarios.FindAsync(usuario.Id);
        Assert.Null(userStillExists);
    }

    #endregion
}
