using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.DTOs;
using ApiSgc.Models.Enums;
using ApiSgc.Services;
using ApiSgc.Services.Contracts;
using ApiSgc.Tests.Helpers;
using Xunit;

namespace ApiSgc.Tests.Services;

public class NotaServiceTests
{
    private static NotaService CreateService(ApplicationDbContext context, IMatriculaService? matriculaService = null)
    {
        var matriculaMock = matriculaService ?? Mock.Of<IMatriculaService>(m => m.CalcularAprovacaoAsync(It.IsAny<int>()) == Task.FromResult(new Matricula()));
        return new NotaService(context, Mock.Of<ILogger<NotaService>>(), matriculaMock);
    }

    private static async Task<Curso> CreateCursoAsync(ApplicationDbContext context, string nome = "Curso Teste")
    {
        var curso = new Curso { Nome = nome };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();
        return curso;
    }

    private static async Task<Turma> CreateTurmaAsync(ApplicationDbContext context, string nome = "Turma Teste", int faltasParaReprovacao = 3)
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

    private static async Task<Matricula> CreateMatriculaAsync(ApplicationDbContext context, Turma? turma = null, SituacaoMatricula situacao = SituacaoMatricula.CURSANDO)
    {
        turma ??= await CreateTurmaAsync(context);
        var usuario = await CreateUsuarioAsync(context);
        var matricula = new Matricula
        {
            UsuarioId = usuario.Id,
            TurmaId = turma.Id,
            Situacao = situacao
        };
        context.Matriculas.Add(matricula);
        await context.SaveChangesAsync();
        return matricula;
    }

    private static async Task<Avaliacao> CreateAvaliacaoAsync(ApplicationDbContext context, Turma turma, string nome = "Avaliacao Teste")
    {
        var avaliacao = new Avaliacao
        {
            TurmaId = turma.Id,
            Nome = nome
        };
        context.Avaliacoes.Add(avaliacao);
        await context.SaveChangesAsync();
        return avaliacao;
    }

    private static async Task<Aula> CreateAulaAsync(ApplicationDbContext context, Turma turma, string titulo = "Aula Teste")
    {
        var aula = new Aula
        {
            TurmaId = turma.Id,
            Titulo = titulo
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
        var avaliacao = await CreateAvaliacaoAsync(context, turma);

        var nota = new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacao.Id,
            Valor = 8.5m
        };

        var result = await service.CreateAsync(nota);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(matricula.Id, result.MatriculaId);
        Assert.Equal(avaliacao.Id, result.AvaliacaoId);
        Assert.Equal(8.5m, result.Valor);
        Assert.Null(result.DeletedAt);

        var saved = await context.Notas.FindAsync(result.Id);
        Assert.NotNull(saved);
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_Exists_ReturnsNota()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var avaliacao = await CreateAvaliacaoAsync(context, turma);

        var nota = new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacao.Id,
            Valor = 7.0m
        };

        context.Notas.Add(nota);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(nota.Id);

        Assert.NotNull(result);
        Assert.Equal(nota.Id, result.Id);
        Assert.Equal(matricula.Id, result.MatriculaId);
        Assert.Equal(avaliacao.Id, result.AvaliacaoId);
        Assert.Equal(7.0m, result.Valor);
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
        var avaliacao = await CreateAvaliacaoAsync(context, turma);

        var nota = new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacao.Id,
            Valor = 6.0m,
            DeletedAt = DateTime.UtcNow
        };

        context.Notas.Add(nota);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(nota.Id);

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
        var avaliacao = await CreateAvaliacaoAsync(context, turma);

        var nota = new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacao.Id,
            Valor = 6.0m
        };

        context.Notas.Add(nota);
        await context.SaveChangesAsync();

        nota.Valor = 9.0m;

        var result = await service.UpdateAsync(nota);

        Assert.Equal(9.0m, result.Valor);

        var saved = await context.Notas.FindAsync(nota.Id);
        Assert.NotNull(saved);
        Assert.Equal(9.0m, saved.Valor);
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_Success_SoftDeletes()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var avaliacao = await CreateAvaliacaoAsync(context, turma);

        var nota = new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacao.Id,
            Valor = 5.0m
        };

        context.Notas.Add(nota);
        await context.SaveChangesAsync();

        var result = await service.DeleteAsync(nota.Id);

        Assert.True(result);

        var saved = await context.Notas.FindAsync(nota.Id);
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
        var avaliacao = await CreateAvaliacaoAsync(context, turma);

        var nota = new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacao.Id,
            Valor = 8.0m
        };

        context.Notas.Add(nota);
        await context.SaveChangesAsync();

        var result = await service.ExistsAsync(nota.Id);

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
        var avaliacao = await CreateAvaliacaoAsync(context, turma);

        var nota = new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacao.Id,
            Valor = 8.0m,
            DeletedAt = DateTime.UtcNow
        };

        context.Notas.Add(nota);
        await context.SaveChangesAsync();

        var result = await service.ExistsAsync(nota.Id);

        Assert.False(result);
    }

    // ExistsByMatriculaAndAvaliacaoAsync

    [Fact]
    public async Task ExistsByMatriculaAndAvaliacaoAsync_Exists_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var avaliacao = await CreateAvaliacaoAsync(context, turma);

        context.Notas.Add(new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacao.Id,
            Valor = 8.0m
        });
        await context.SaveChangesAsync();

        var result = await service.ExistsByMatriculaAndAvaliacaoAsync(matricula.Id, avaliacao.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByMatriculaAndAvaliacaoAsync_NotExists_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var avaliacao = await CreateAvaliacaoAsync(context, turma);

        var result = await service.ExistsByMatriculaAndAvaliacaoAsync(matricula.Id, avaliacao.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByMatriculaAndAvaliacaoAsync_ExcludesOwnId_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var avaliacao = await CreateAvaliacaoAsync(context, turma);

        var nota = new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacao.Id,
            Valor = 7.0m
        };
        context.Notas.Add(nota);
        await context.SaveChangesAsync();

        var result = await service.ExistsByMatriculaAndAvaliacaoAsync(matricula.Id, avaliacao.Id, nota.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByMatriculaAndAvaliacaoAsync_SoftDeleted_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var avaliacao = await CreateAvaliacaoAsync(context, turma);

        context.Notas.Add(new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacao.Id,
            Valor = 7.0m,
            DeletedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var result = await service.ExistsByMatriculaAndAvaliacaoAsync(matricula.Id, avaliacao.Id);

        Assert.False(result);
    }

    // CalcularMediaPonderadaAsync

    [Fact]
    public async Task CalcularMediaPonderadaAsync_NoNotas_ReturnsZero()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);

        var result = await service.CalcularMediaPonderadaAsync(matricula.Id);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CalcularMediaPonderadaAsync_WithNotas_ReturnsAverage()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context);
        var matricula = await CreateMatriculaAsync(context, turma);
        var avaliacao1 = await CreateAvaliacaoAsync(context, turma, "Avaliacao 1");
        var avaliacao2 = await CreateAvaliacaoAsync(context, turma, "Avaliacao 2");

        context.Notas.AddRange(
            new Nota { MatriculaId = matricula.Id, AvaliacaoId = avaliacao1.Id, Valor = 6.0m },
            new Nota { MatriculaId = matricula.Id, AvaliacaoId = avaliacao2.Id, Valor = 8.0m }
        );
        await context.SaveChangesAsync();

        var result = await service.CalcularMediaPonderadaAsync(matricula.Id);

        Assert.Equal(7.0m, result);
    }

    [Fact]
    public async Task GetAlunosComNotasAsync_ReturnsStudentsWithCorrespondingNotas()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var turma = await CreateTurmaAsync(context, faltasParaReprovacao: 3);
        var aula = await CreateAulaAsync(context, turma);
        var avaliacao = await CreateAvaliacaoAsync(context, turma);

        var matricula1 = await CreateMatriculaAsync(context, turma, SituacaoMatricula.CURSANDO);
        var matricula2 = await CreateMatriculaAsync(context, turma, SituacaoMatricula.APROVADO);
        var matricula3 = await CreateMatriculaAsync(context, turma, SituacaoMatricula.REPROVADO_NOTA);

        context.Notas.AddRange(
            new Nota { MatriculaId = matricula1.Id, AvaliacaoId = avaliacao.Id, Valor = 9.0m },
            new Nota { MatriculaId = matricula2.Id, AvaliacaoId = avaliacao.Id, Valor = 7.0m }
        );
        await context.SaveChangesAsync();

        var result = (await service.GetAlunosComNotasAsync(turma.Id, avaliacao.Id)).ToList();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, item => item.MatriculaId == matricula1.Id && item.NotaId.HasValue && item.Valor == 9.0m);
        Assert.Contains(result, item => item.MatriculaId == matricula2.Id && item.NotaId.HasValue && item.Valor == 7.0m);
        Assert.Contains(result, item => item.MatriculaId == matricula3.Id && item.NotaId == null && item.Valor == null);
    }

    [Fact]
    public async Task SalvarNotasLoteAsync_CreatesAndUpdatesNotas_AndRecalculatesAprovacao()
    {
        using var context = TestHelpers.CreateDbContext();
        var matriculaServiceMock = new Mock<IMatriculaService>();
        matriculaServiceMock
            .Setup(m => m.CalcularAprovacaoAsync(It.IsAny<int>()))
            .ReturnsAsync(new Matricula());

        var service = CreateService(context, matriculaServiceMock.Object);

        var turma = await CreateTurmaAsync(context);
        var avaliacao = await CreateAvaliacaoAsync(context, turma);
        var matricula1 = await CreateMatriculaAsync(context, turma);
        var matricula2 = await CreateMatriculaAsync(context, turma);

        var existingNota = new Nota
        {
            MatriculaId = matricula1.Id,
            AvaliacaoId = avaliacao.Id,
            Valor = 5.0m
        };
        context.Notas.Add(existingNota);
        await context.SaveChangesAsync();

        var dto = new SalvarNotasLoteDto
        {
            AvaliacaoId = avaliacao.Id,
            Items = new List<NotaItemDto>
            {
                new NotaItemDto { MatriculaId = matricula1.Id, Valor = 8.0m },
                new NotaItemDto { MatriculaId = matricula2.Id, Valor = 7.5m },
            }
        };

        await service.SalvarNotasLoteAsync(dto);

        var updatedNota = await context.Notas.FirstOrDefaultAsync(n => n.Id == existingNota.Id);
        var createdNota = await context.Notas.FirstOrDefaultAsync(n => n.MatriculaId == matricula2.Id && n.AvaliacaoId == avaliacao.Id);

        Assert.NotNull(updatedNota);
        Assert.Equal(8.0m, updatedNota!.Valor);
        Assert.NotNull(createdNota);
        Assert.Equal(7.5m, createdNota!.Valor);

        matriculaServiceMock.Verify(m => m.CalcularAprovacaoAsync(matricula1.Id), Times.Once);
        matriculaServiceMock.Verify(m => m.CalcularAprovacaoAsync(matricula2.Id), Times.Once);
    }

    [Fact]
    public async Task SalvarNotasLoteAsync_AvaliacaoNotFound_ThrowsInvalidOperationException()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var dto = new SalvarNotasLoteDto
        {
            AvaliacaoId = 999,
            Items = new List<NotaItemDto>
            {
                new NotaItemDto { MatriculaId = 1, Valor = 5.0m }
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SalvarNotasLoteAsync(dto));
    }
}
