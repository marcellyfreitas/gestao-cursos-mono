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
/// Preservation Property Tests — These tests verify that existing CORRECT behavior
/// is preserved after bug fixes are applied. They test non-buggy scenarios that
/// already work correctly on unfixed code.
/// 
/// These tests are written BEFORE any fix and are EXPECTED TO PASS on unfixed code.
/// After fixes, they should CONTINUE TO PASS (confirming no regressions).
/// 
/// **Validates: Requirements 3.1, 3.3, 3.4, 3.7, 3.8**
/// </summary>
public class PreservationPropertyTests
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

    #region Preservation Bug 3: Aluno with totalFaltas < faltasLimite → Reprovado = false; totalFaltas > faltasLimite → Reprovado = true

    /// <summary>
    /// Preservation Bug 3: When totalFaltas is strictly LESS than faltasLimite,
    /// the student should NOT be marked as Reprovado. This behavior already works correctly.
    /// 
    /// **Validates: Requirements 3.3**
    /// </summary>
    [Theory]
    [InlineData(0, 3)]
    [InlineData(1, 3)]
    [InlineData(2, 3)]
    [InlineData(1, 5)]
    [InlineData(3, 5)]
    public async Task Preservation_Bug3_FaltasBelowLimit_ShouldNotBeReprovado(int totalFaltas, int faltasLimite)
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var curso = await SeedCurso(context);
        var turma = await SeedTurma(context, curso, faltasParaReprovacao: faltasLimite);
        var usuario = await SeedUsuario(context);
        var matricula = await SeedMatricula(context, usuario, turma);

        // Create the aula we'll query
        var aulaAlvo = await SeedAula(context, turma, "Aula Alvo");

        // Seed faltas below the limit
        await SeedFaltas(context, matricula, totalFaltas);

        var service = CreateFrequenciaService(context);

        // Act
        var result = await service.GetAlunosComFrequenciaAsync(aulaAlvo.Id);

        // Assert — student below the limit should NOT be reprovado
        var aluno = result.FirstOrDefault(a => a.MatriculaId == matricula.Id);
        Assert.NotNull(aluno);
        Assert.Equal(totalFaltas, aluno.TotalFaltas);
        Assert.False(aluno.Reprovado);
    }

    /// <summary>
    /// Preservation Bug 3: When totalFaltas is strictly GREATER than faltasLimite,
    /// the student SHOULD be marked as Reprovado. This behavior already works correctly.
    /// 
    /// **Validates: Requirements 3.3**
    /// </summary>
    [Theory]
    [InlineData(4, 3)]
    [InlineData(5, 3)]
    [InlineData(6, 5)]
    [InlineData(3, 2)]
    public async Task Preservation_Bug3_FaltasAboveLimit_ShouldBeReprovado(int totalFaltas, int faltasLimite)
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var curso = await SeedCurso(context);
        var turma = await SeedTurma(context, curso, faltasParaReprovacao: faltasLimite);
        var usuario = await SeedUsuario(context);
        var matricula = await SeedMatricula(context, usuario, turma);

        // Create the aula we'll query
        var aulaAlvo = await SeedAula(context, turma, "Aula Alvo");

        // Seed faltas above the limit
        await SeedFaltas(context, matricula, totalFaltas);

        var service = CreateFrequenciaService(context);

        // Act
        var result = await service.GetAlunosComFrequenciaAsync(aulaAlvo.Id);

        // Assert — student above the limit SHOULD be reprovado
        var aluno = result.FirstOrDefault(a => a.MatriculaId == matricula.Id);
        Assert.NotNull(aluno);
        Assert.Equal(totalFaltas, aluno.TotalFaltas);
        Assert.True(aluno.Reprovado);
    }

    #endregion

    #region Preservation Bug 4: Aluno with faltas within limit and notas incompletas → status remains CURSANDO

    /// <summary>
    /// Preservation Bug 4: When a student has faltas WITHIN the limit (frequenciaSuficiente=true)
    /// and notas are incomplete, the status should remain CURSANDO. This behavior already works correctly.
    /// 
    /// **Validates: Requirements 3.4**
    /// </summary>
    [Fact]
    public async Task Preservation_Bug4_FaltasWithinLimit_NotasIncompletas_ShouldRemainCursando()
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var curso = await SeedCurso(context);
        var turma = await SeedTurma(context, curso,
            faltasParaReprovacao: 5,
            necessitaAtividades: true,
            mediaMinima: 7.0m);
        var usuario = await SeedUsuario(context);
        var matricula = await SeedMatricula(context, usuario, turma);

        // Create 3 avaliacoes for the turma
        var avaliacoes = await SeedAvaliacoes(context, turma, 3);

        // Seed 2 faltas (within limit of 5)
        await SeedFaltas(context, matricula, totalFaltas: 2);

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

        // Only add 1 nota (incomplete — turma has 3 avaliacoes)
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

        // Assert — faltas within limit + incomplete notas → CURSANDO
        Assert.Equal(SituacaoMatricula.CURSANDO, result.Situacao);
    }

    /// <summary>
    /// Preservation Bug 4: When a student has zero faltas and no notas at all,
    /// the status should remain CURSANDO. This behavior already works correctly.
    /// 
    /// **Validates: Requirements 3.4**
    /// </summary>
    [Fact]
    public async Task Preservation_Bug4_ZeroFaltas_NoNotas_ShouldRemainCursando()
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

        // Create avaliacoes
        await SeedAvaliacoes(context, turma, 2);

        // Add a presença so frequenciaPendente is false (but no faltas)
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

        // Assert — no notas → CURSANDO (regardless of frequency being ok)
        Assert.Equal(SituacaoMatricula.CURSANDO, result.Situacao);
    }

    #endregion

    #region Preservation Bug 7: Creating a NEW matrícula (not reactivation) works normally

    /// <summary>
    /// Preservation Bug 7: Creating a brand new matrícula (no soft-deleted record exists)
    /// should work normally with CURSANDO status and no pre-existing records.
    /// 
    /// **Validates: Requirements 3.7**
    /// </summary>
    [Fact]
    public async Task Preservation_Bug7_NewMatricula_NotReactivation_ShouldCreateNormally()
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var curso = await SeedCurso(context);
        var turma = await SeedTurma(context, curso);
        var usuario = await SeedUsuario(context);

        var service = CreateMatriculaService(context);

        // Act — create a brand new matrícula (no soft-deleted record exists)
        var novaMatricula = new Matricula
        {
            UsuarioId = usuario.Id,
            TurmaId = turma.Id,
            Situacao = SituacaoMatricula.CURSANDO,
        };
        var result = await service.CreateAsync(novaMatricula);

        // Assert — matrícula created normally
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(SituacaoMatricula.CURSANDO, result.Situacao);
        Assert.Equal(usuario.Id, result.UsuarioId);
        Assert.Equal(turma.Id, result.TurmaId);
        Assert.Null(result.DeletedAt);
    }

    /// <summary>
    /// Preservation Bug 7: A newly created matrícula should have no associated
    /// Frequencia or Nota records.
    /// 
    /// **Validates: Requirements 3.7**
    /// </summary>
    [Fact]
    public async Task Preservation_Bug7_NewMatricula_ShouldHaveNoFrequenciasOrNotas()
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var curso = await SeedCurso(context);
        var turma = await SeedTurma(context, curso);
        var usuario = await SeedUsuario(context);

        var service = CreateMatriculaService(context);

        // Act — create a brand new matrícula
        var novaMatricula = new Matricula
        {
            UsuarioId = usuario.Id,
            TurmaId = turma.Id,
            Situacao = SituacaoMatricula.CURSANDO,
        };
        var result = await service.CreateAsync(novaMatricula);

        // Assert — no frequencias or notas exist for this matrícula
        var frequencias = await context.Frequencias
            .Where(f => f.MatriculaId == result.Id)
            .ToListAsync();
        var notas = await context.Notas
            .Where(n => n.MatriculaId == result.Id)
            .ToListAsync();

        Assert.Empty(frequencias);
        Assert.Empty(notas);
    }

    #endregion

    #region Preservation Bug 8: Deleting a user with NO matrículas succeeds without error

    /// <summary>
    /// Preservation Bug 8: Deleting a user that has NO matrículas should succeed
    /// without any error. This behavior already works correctly.
    /// 
    /// **Validates: Requirements 3.8**
    /// </summary>
    [Fact]
    public async Task Preservation_Bug8_DeleteUsuario_WithNoMatriculas_ShouldSucceed()
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var usuario = await SeedUsuario(context);

        // Ensure no matrículas exist for this user
        var matriculasCount = await context.Matriculas
            .CountAsync(m => m.UsuarioId == usuario.Id);
        Assert.Equal(0, matriculasCount);

        var service = CreateUsuarioService(context);

        // Act & Assert — should NOT throw exception
        var exception = await Record.ExceptionAsync(() => service.DeleteAsync(usuario.Id));
        Assert.Null(exception);
    }

    /// <summary>
    /// Preservation Bug 8: After deleting a user with no matrículas,
    /// the user should no longer exist in the database.
    /// 
    /// **Validates: Requirements 3.8**
    /// </summary>
    [Fact]
    public async Task Preservation_Bug8_DeleteUsuario_WithNoMatriculas_UserShouldBeRemoved()
    {
        // Arrange
        using var context = TestHelpers.CreateDbContext();
        var usuario = await SeedUsuario(context);

        var service = CreateUsuarioService(context);

        // Act
        var result = await service.DeleteAsync(usuario.Id);

        // Assert — user should be removed
        Assert.True(result);
        var userStillExists = await context.Usuarios.FindAsync(usuario.Id);
        Assert.Null(userStillExists);
    }

    #endregion
}
