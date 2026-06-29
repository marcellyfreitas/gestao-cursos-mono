using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.Enums;
using ApiSgc.Models.ViewModels;
using ApiSgc.Controllers.Private;
using ApiSgc.Tests.Helpers;
using Xunit;

namespace ApiSgc.Tests.Controllers;

public class AlunoControllerTests
{
    private static AlunoController CreateController(ApplicationDbContext context, int? usuarioId = null)
    {
        var controller = new AlunoController(context, Mock.Of<ILogger<AlunoController>>());

        var claims = new List<Claim>();
        if (usuarioId.HasValue)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, usuarioId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        return controller;
    }

    private static async Task<Usuario> CreateUsuarioAsync(ApplicationDbContext context, string email = "aluno@teste.com")
    {
        var usuario = TestHelpers.CreateUser(email);
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        return usuario;
    }

    private static async Task<Curso> CreateCursoAsync(ApplicationDbContext context, string nome = "Curso Teste")
    {
        var curso = new Curso { Nome = nome };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();
        return curso;
    }

    private static async Task<Turma> CreateTurmaAsync(
        ApplicationDbContext context,
        int cursoId,
        string nome = "Turma Teste",
        bool necessitaAtividades = false,
        int faltasParaReprovacao = 3)
    {
        var turma = new Turma
        {
            CursoId = cursoId,
            Nome = nome,
            NecessitaAtividades = necessitaAtividades,
            FaltasParaReprovacao = faltasParaReprovacao,
        };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();
        return turma;
    }

    private static async Task<Matricula> CreateMatriculaAsync(
        ApplicationDbContext context,
        int usuarioId,
        int turmaId,
        SituacaoMatricula situacao = SituacaoMatricula.CURSANDO)
    {
        var matricula = new Matricula
        {
            UsuarioId = usuarioId,
            TurmaId = turmaId,
            Situacao = situacao,
        };
        context.Matriculas.Add(matricula);
        await context.SaveChangesAsync();
        return matricula;
    }

    // GetMinhasTurmas

    [Fact]
    public async Task GetMinhasTurmas_SemToken_Returns401()
    {
        using var context = TestHelpers.CreateDbContext();
        var controller = CreateController(context, usuarioId: null);

        var result = await controller.GetMinhasTurmas() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(401, result!.StatusCode);
    }

    [Fact]
    public async Task GetMinhasTurmas_SemMatriculas_ReturnsListaVazia()
    {
        using var context = TestHelpers.CreateDbContext();
        var usuario = await CreateUsuarioAsync(context);
        var controller = CreateController(context, usuario.Id);

        var result = await controller.GetMinhasTurmas() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result!.StatusCode);
    }

    [Fact]
    public async Task GetMinhasTurmas_ComMatricula_RetornaTurma()
    {
        using var context = TestHelpers.CreateDbContext();
        var usuario = await CreateUsuarioAsync(context);
        var curso = await CreateCursoAsync(context);
        var turma = await CreateTurmaAsync(context, curso.Id, "Turma A");
        await CreateMatriculaAsync(context, usuario.Id, turma.Id);

        var controller = CreateController(context, usuario.Id);

        var result = await controller.GetMinhasTurmas() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result!.StatusCode);
    }

    [Fact]
    public async Task GetMinhasTurmas_NaoRetornaMatriculasDeOutrosAlunos()
    {
        using var context = TestHelpers.CreateDbContext();
        var usuario1 = await CreateUsuarioAsync(context, "aluno1@teste.com");
        var usuario2 = await CreateUsuarioAsync(context, "aluno2@teste.com");
        var curso = await CreateCursoAsync(context);
        var turma = await CreateTurmaAsync(context, curso.Id);
        await CreateMatriculaAsync(context, usuario2.Id, turma.Id);

        var controller = CreateController(context, usuario1.Id);

        var result = await controller.GetMinhasTurmas() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result!.StatusCode);
    }

    [Fact]
    public async Task GetMinhasTurmas_NaoRetornaMatriculasSoftDeleted()
    {
        using var context = TestHelpers.CreateDbContext();
        var usuario = await CreateUsuarioAsync(context);
        var curso = await CreateCursoAsync(context);
        var turma = await CreateTurmaAsync(context, curso.Id);

        var matricula = await CreateMatriculaAsync(context, usuario.Id, turma.Id);
        matricula.DeletedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        var controller = CreateController(context, usuario.Id);

        var result = await controller.GetMinhasTurmas() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result!.StatusCode);
    }

    [Fact]
    public async Task GetMinhasTurmas_ComFrequencias_RetornaContagemCorreta()
    {
        using var context = TestHelpers.CreateDbContext();
        var usuario = await CreateUsuarioAsync(context);
        var curso = await CreateCursoAsync(context);
        var turma = await CreateTurmaAsync(context, curso.Id, faltasParaReprovacao: 5);
        var matricula = await CreateMatriculaAsync(context, usuario.Id, turma.Id);

        var professor = new Professor { Nome = "Professor Teste" };
        context.Professores.Add(professor);
        await context.SaveChangesAsync();

        var aulas = new[]
        {
            new Aula { TurmaId = turma.Id, Titulo = "Aula 1", DataAula = DateOnly.FromDateTime(DateTime.Today), ProfessorId = professor.Id },
            new Aula { TurmaId = turma.Id, Titulo = "Aula 2", DataAula = DateOnly.FromDateTime(DateTime.Today), ProfessorId = professor.Id },
            new Aula { TurmaId = turma.Id, Titulo = "Aula 3", DataAula = DateOnly.FromDateTime(DateTime.Today), ProfessorId = professor.Id },
        };
        context.Aulas.AddRange(aulas);
        await context.SaveChangesAsync();

        context.Frequencias.AddRange(
            new Frequencia { MatriculaId = matricula.Id, AulaId = aulas[0].Id, Status = StatusFrequencia.PRESENTE },
            new Frequencia { MatriculaId = matricula.Id, AulaId = aulas[1].Id, Status = StatusFrequencia.PRESENTE },
            new Frequencia { MatriculaId = matricula.Id, AulaId = aulas[2].Id, Status = StatusFrequencia.FALTA }
        );
        await context.SaveChangesAsync();

        var controller = CreateController(context, usuario.Id);
        var result = await controller.GetMinhasTurmas() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result!.StatusCode);
    }

    [Fact]
    public async Task GetMinhasTurmas_ComNotas_RetornaNotasCorretamente()
    {
        using var context = TestHelpers.CreateDbContext();
        var usuario = await CreateUsuarioAsync(context);
        var curso = await CreateCursoAsync(context);
        var turma = await CreateTurmaAsync(context, curso.Id, necessitaAtividades: true);
        var matricula = await CreateMatriculaAsync(context, usuario.Id, turma.Id);

        var avaliacao = new Avaliacao { TurmaId = turma.Id, Nome = "Prova 1" };
        context.Avaliacoes.Add(avaliacao);
        await context.SaveChangesAsync();

        context.Notas.Add(new Nota
        {
            MatriculaId = matricula.Id,
            AvaliacaoId = avaliacao.Id,
            Valor = 8.5m,
        });
        await context.SaveChangesAsync();

        var controller = CreateController(context, usuario.Id);
        var result = await controller.GetMinhasTurmas() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result!.StatusCode);
    }

    [Fact]
    public async Task GetMinhasTurmas_TurmaSemAtividades_RetornaNotasVazias()
    {
        using var context = TestHelpers.CreateDbContext();
        var usuario = await CreateUsuarioAsync(context);
        var curso = await CreateCursoAsync(context);
        var turma = await CreateTurmaAsync(context, curso.Id, necessitaAtividades: false);
        await CreateMatriculaAsync(context, usuario.Id, turma.Id);

        var controller = CreateController(context, usuario.Id);
        var result = await controller.GetMinhasTurmas() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result!.StatusCode);
    }

    [Fact]
    public async Task GetMinhasTurmas_ComMultiplasTurmas_RetornaTodasAsMatriculas()
    {
        using var context = TestHelpers.CreateDbContext();
        var usuario = await CreateUsuarioAsync(context);
        var curso = await CreateCursoAsync(context);
        var turma1 = await CreateTurmaAsync(context, curso.Id, "Turma 1");
        var turma2 = await CreateTurmaAsync(context, curso.Id, "Turma 2");
        var turma3 = await CreateTurmaAsync(context, curso.Id, "Turma 3");

        await CreateMatriculaAsync(context, usuario.Id, turma1.Id);
        await CreateMatriculaAsync(context, usuario.Id, turma2.Id);
        await CreateMatriculaAsync(context, usuario.Id, turma3.Id);

        var controller = CreateController(context, usuario.Id);
        var result = await controller.GetMinhasTurmas() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result!.StatusCode);
    }
}