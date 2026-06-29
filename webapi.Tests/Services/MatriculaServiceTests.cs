using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.Enums;
using ApiSgc.Services;
using ApiSgc.Tests.Helpers;
using Xunit;

namespace ApiSgc.Tests.Services;

public class MatriculaServiceTests
{
    private static MatriculaService CreateService(ApplicationDbContext context)
    {
        return new MatriculaService(
            context,
            Mock.Of<ILogger<MatriculaService>>());
    }

    private static async Task<Curso> CreateCursoAsync(ApplicationDbContext context, string nome = "Curso Teste")
    {
        var curso = new Curso { Nome = nome };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();
        return curso;
    }

    private static async Task<Turma> CreateTurmaAsync(ApplicationDbContext context, string nome = "Turma Teste", int faltasParaReprovacao = 4)
    {
        var curso = await CreateCursoAsync(context);
        var turma = new Turma
        {
            CursoId = curso.Id,
            Nome = nome,
            FaltasParaReprovacao = faltasParaReprovacao,
        };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();
        return turma;
    }

    private static async Task<Usuario> CreateUsuarioAsync(ApplicationDbContext context, string? email = null)
    {
        var usuario = new Usuario
        {
            Nome = "Aluno Teste",
            Email = email ?? $"aluno{Guid.NewGuid()}@email.com",
            Senha = "senha123"
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        return usuario;
    }

    private static async Task<Matricula> CreateMatriculaAsync(ApplicationDbContext context, Turma? turma = null, Usuario? usuario = null)
    {
        turma ??= await CreateTurmaAsync(context);
        usuario ??= await CreateUsuarioAsync(context);

        var matricula = new Matricula
        {
            TurmaId = turma.Id,
            UsuarioId = usuario.Id,
            Situacao = SituacaoMatricula.CURSANDO,
        };
        context.Matriculas.Add(matricula);
        await context.SaveChangesAsync();
        return matricula;
    }

    private static async Task<Aula> CreateAulaAsync(ApplicationDbContext context, Turma turma, string titulo = "Aula Teste", int numero = 1)
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

    private static async Task<Avaliacao> CreateAvaliacaoAsync(ApplicationDbContext context, Turma turma, string nome = "Avaliação Teste")
    {
        var avaliacao = new Avaliacao
        {
            TurmaId = turma.Id,
            Nome = nome,
        };
        context.Avaliacoes.Add(avaliacao);
        await context.SaveChangesAsync();
        return avaliacao;
    }

    // RF-011: Vínculo automático com aulas da turma
    // NOTA: Esta funcionalidade ainda não foi implementada.
    // Os testes a seguir descrevem o comportamento ESPERADO do RF-011.
    // Quando implementado, CreateAsync deve criar frequências automaticamente para todas as aulas da turma.

    [Fact]
    public async Task CreateAsync_Success_CreatesMatricula()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);
        
        var turma = await CreateTurmaAsync(context);
        var usuario = await CreateUsuarioAsync(context);

        var matricula = new Matricula
        {
            TurmaId = turma.Id,
            UsuarioId = usuario.Id,
            Situacao = SituacaoMatricula.CURSANDO,
        };

        var result = await service.CreateAsync(matricula);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(turma.Id, result.TurmaId);
        Assert.Equal(usuario.Id, result.UsuarioId);
        Assert.Null(result.DeletedAt);
    }

    // RF-011: Behavior - when student is enrolled, frequencies should be created for all class lectures
    // [Fact]
    // [Skipped until RF-011 is fully implemented in MatriculaService]
    // public async Task CreateAsync_WithExistingAulas_AutomaticallyLinksAlunoToAulas()
    // {
    //     using var context = TestHelpers.CreateDbContext();
    //     var service = CreateService(context);

    //     var turma = await CreateTurmaAsync(context);
    //     var aula1 = await CreateAulaAsync(context, turma, "Aula 1");
    //     var aula2 = await CreateAulaAsync(context, turma, "Aula 2");
    //     var usuario = await CreateUsuarioAsync(context);

    //     var matricula = new Matricula
    //     {
    //         TurmaId = turma.Id,
    //         UsuarioId = usuario.Id,
    //         Situacao = SituacaoMatricula.CURSANDO,
    //     };

    //     var result = await service.CreateAsync(matricula);

    //     // Verifica se frequências foram criadas automaticamente para as aulas existentes
    //     var frequencias = await context.Frequencias
    //         .Where(f => f.MatriculaId == result.Id)
    //         .ToListAsync();

    //     Assert.Equal(2, frequencias.Count);
    //     Assert.Contains(frequencias, f => f.AulaId == aula1.Id);
    //     Assert.Contains(frequencias, f => f.AulaId == aula2.Id);
    // }

    // [Fact]
    // [Skipped until RF-011 is fully implemented in MatriculaService]
    // public async Task CreateAsync_MultipleAlunosInTurma_EachLinkedToAllAulas()
    // {
    //     using var context = TestHelpers.CreateDbContext();
    //     var service = CreateService(context);

    //     var turma = await CreateTurmaAsync(context);
    //     var aula1 = await CreateAulaAsync(context, turma, "Aula 1");
    //     var aula2 = await CreateAulaAsync(context, turma, "Aula 2");
    //     var usuario1 = await CreateUsuarioAsync(context, "aluno1@email.com");
    //     var usuario2 = await CreateUsuarioAsync(context, "aluno2@email.com");

    //     var matricula1 = new Matricula { TurmaId = turma.Id, UsuarioId = usuario1.Id, Situacao = SituacaoMatricula.CURSANDO };
    //     var matricula2 = new Matricula { TurmaId = turma.Id, UsuarioId = usuario2.Id, Situacao = SituacaoMatricula.CURSANDO };

    //     await service.CreateAsync(matricula1);
    //     await service.CreateAsync(matricula2);

    //     var freq1 = await context.Frequencias.Where(f => f.MatriculaId == matricula1.Id).ToListAsync();
    //     var freq2 = await context.Frequencias.Where(f => f.MatriculaId == matricula2.Id).ToListAsync();

    //     Assert.Equal(2, freq1.Count);
    //     Assert.Equal(2, freq2.Count);
    // }

    [Fact]
    public async Task CreateAsync_ReusesSoftDeletedMatricula()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var usuario = await CreateUsuarioAsync(context);

        var deletedMatricula = new Matricula
        {
            TurmaId = turma.Id,
            UsuarioId = usuario.Id,
            Situacao = SituacaoMatricula.CURSANDO,
            DeletedAt = DateTime.UtcNow,
        };
        context.Matriculas.Add(deletedMatricula);
        await context.SaveChangesAsync();

        var newMatricula = new Matricula
        {
            TurmaId = turma.Id,
            UsuarioId = usuario.Id,
            Situacao = SituacaoMatricula.APROVADO,
        };

        var result = await service.CreateAsync(newMatricula);

        Assert.Equal(deletedMatricula.Id, result.Id);
        Assert.Equal(SituacaoMatricula.APROVADO, result.Situacao);
        Assert.Null(result.DeletedAt);
    }

    // CRUD Operations

    [Fact]
    public async Task GetByIdAsync_Exists_ReturnsMatricula()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var matricula = await CreateMatriculaAsync(context);

        var result = await service.GetByIdAsync(matricula.Id);

        Assert.NotNull(result);
        Assert.Equal(matricula.Id, result.Id);
        Assert.Equal(matricula.UsuarioId, result.UsuarioId);
        Assert.Equal(matricula.TurmaId, result.TurmaId);
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
        var usuario = await CreateUsuarioAsync(context);

        var matricula = new Matricula
        {
            TurmaId = turma.Id,
            UsuarioId = usuario.Id,
            DeletedAt = DateTime.UtcNow
        };
        context.Matriculas.Add(matricula);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(matricula.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Success_UpdatesMatricula()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var usuario = await CreateUsuarioAsync(context);

        var matricula = new Matricula
        {
            TurmaId = turma.Id,
            UsuarioId = usuario.Id,
            Situacao = SituacaoMatricula.CURSANDO,
        };
        context.Matriculas.Add(matricula);
        await context.SaveChangesAsync();

        matricula.Situacao = SituacaoMatricula.APROVADO;
        var result = await service.UpdateAsync(matricula);

        Assert.NotNull(result);
        Assert.Equal(SituacaoMatricula.APROVADO, result.Situacao);
        Assert.Equal(matricula.Id, result.Id);
    }

    [Fact]
    public async Task DeleteAsync_Success_SoftDeletes()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var matricula = await CreateMatriculaAsync(context);

        var result = await service.DeleteAsync(matricula.Id);

        Assert.True(result);

        var saved = await context.Matriculas.FindAsync(matricula.Id);
        Assert.NotNull(saved?.DeletedAt);
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
        var service = CreateService(context);

        var matricula = await CreateMatriculaAsync(context);

        var result = await service.ExistsAsync(matricula.Id);

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
    public async Task ExistsByAlunoAndTurmaAsync_Exists_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var matricula = await CreateMatriculaAsync(context);

        var result = await service.ExistsByAlunoAndTurmaAsync(matricula.UsuarioId, matricula.TurmaId);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByAlunoAndTurmaAsync_NotExists_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.ExistsByAlunoAndTurmaAsync(999, 999);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByAlunoAndTurmaAsync_ExcludesOwnId()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var matricula = await CreateMatriculaAsync(context);

        var result = await service.ExistsByAlunoAndTurmaAsync(matricula.UsuarioId, matricula.TurmaId, matricula.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAll()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        for (int i = 0; i < 3; i++)
        {
            await CreateMatriculaAsync(context, turma);
        }

        var (items, total) = await service.GetAllAsync(null, null, 1, 10);

        Assert.Equal(3, total);
        Assert.Equal(3, items.Count());
    }

    [Fact]
    public async Task GetAllAsync_FilterByTurma_ReturnsFiltered()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma1 = await CreateTurmaAsync(context, "Turma 1");
        var turma2 = await CreateTurmaAsync(context, "Turma 2");

        await CreateMatriculaAsync(context, turma1);
        await CreateMatriculaAsync(context, turma1);
        await CreateMatriculaAsync(context, turma2);

        var (items, total) = await service.GetAllAsync(turma1.Id, null, 1, 10);

        Assert.Equal(2, total);
        Assert.All(items, m => Assert.Equal(turma1.Id, m.TurmaId));
    }

    // CalcularAprovacao

    [Fact]
    public async Task CalcularAprovacaoAsync_SemAtividades_FrequenciaOk_Aprovado()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context, "Turma Teste", 4);
        var matricula = await CreateMatriculaAsync(context, turma);

        // Adiciona 2 faltas (< 4)
        for (int i = 0; i < 3; i++)
        {
            context.Frequencias.Add(new Frequencia
            {
                MatriculaId = matricula.Id,
                AulaId = i,
                Status = i < 2 ? StatusFrequencia.FALTA : StatusFrequencia.PRESENTE,
            });
        }
        await context.SaveChangesAsync();

        var result = await service.CalcularAprovacaoAsync(matricula.Id);

        Assert.Equal(SituacaoMatricula.APROVADO, result.Situacao);
    }

    [Fact]
    public async Task CalcularAprovacaoAsync_SemAtividades_FrequenciaRuim_ReprovadoFrequencia()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context, "Turma Teste", 2);
        var matricula = await CreateMatriculaAsync(context, turma);

        // Adiciona 3 faltas (> 2)
        for (int i = 0; i < 4; i++)
        {
            context.Frequencias.Add(new Frequencia
            {
                MatriculaId = matricula.Id,
                AulaId = i,
                Status = i < 3 ? StatusFrequencia.FALTA : StatusFrequencia.PRESENTE,
            });
        }
        await context.SaveChangesAsync();

        var result = await service.CalcularAprovacaoAsync(matricula.Id);

        Assert.Equal(SituacaoMatricula.REPROVADO_FREQUENCIA, result.Situacao);
    }

    [Fact]
    public async Task CalcularAprovacaoAsync_MatriculaNotFound_ThrowsException()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CalcularAprovacaoAsync(999));

        Assert.Equal("Matrícula não encontrada", exception.Message);
    }
}
