using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ApiSgc.Database;
using ApiSgc.Database.Seeders;
using ApiSgc.Models;
using ApiSgc.Tests.Helpers;
using Xunit;

namespace ApiSgc.Tests.Services;

public class AvaliacaoSeederTests
{
    private static AvaliacaoSeeder CreateSeeder(ApplicationDbContext context)
    {
        return new AvaliacaoSeeder(context);
    }

    private async Task<(Curso Curso, Turma Turma)> SeedTurma(
        ApplicationDbContext context,
        string nomeCurso,
        bool necessitaAtividades,
        string nomeTurma = "Turma Padrao")
    {
        var curso = new Curso
        {
            Nome = nomeCurso,
        };
        context.Cursos.Add(curso);
        await context.SaveChangesAsync();

        var turma = new Turma
        {
            CursoId = curso.Id,
            Nome = nomeTurma,
            NecessitaAtividades = necessitaAtividades,
            FaltasParaReprovacao = 4,
        };
        context.Turmas.Add(turma);
        await context.SaveChangesAsync();

        return (curso, turma);
    }

    [Fact]
    public async Task Seed_TurmaComNecessitaAtividades_CriaAvaliacoes()
    {
        using var context = TestHelpers.CreateDbContext();
        var (_, turma) = await SeedTurma(context,
            nomeCurso: "Discipulado Básico",
            necessitaAtividades: true);

        var seeder = CreateSeeder(context);

        await seeder.Seed();

        var avaliacoes = await context.Avaliacoes
            .Where(a => a.TurmaId == turma.Id && a.DeletedAt == null)
            .ToListAsync();

        Assert.NotEmpty(avaliacoes);
        Assert.Contains(avaliacoes, a => a.Nome == "Teste 1 - Fundamentos");
        Assert.Contains(avaliacoes, a => a.Nome == "Teste 2 - Doutrina");
        Assert.Contains(avaliacoes, a => a.Nome == "Avaliação Final");
    }

    [Fact]
    public async Task Seed_TurmaSemNecessitaAtividades_NaoCriaAvaliacoes()
    {
        using var context = TestHelpers.CreateDbContext();
        var (_, turma) = await SeedTurma(context,
            nomeCurso: "Consolidação",
            necessitaAtividades: false);

        var seeder = CreateSeeder(context);

        await seeder.Seed();

        var avaliacoes = await context.Avaliacoes
            .Where(a => a.TurmaId == turma.Id && a.DeletedAt == null)
            .ToListAsync();

        Assert.Empty(avaliacoes);
    }

    [Fact]
    public async Task Seed_TurmasJaTemAvaliacoes_NaoDuplica()
    {
        using var context = TestHelpers.CreateDbContext();
        var (_, turma) = await SeedTurma(context,
            nomeCurso: "Formação de Líderes",
            necessitaAtividades: true);

        context.Avaliacoes.Add(new Avaliacao
        {
            TurmaId = turma.Id,
            Nome = "Projeto Liderança",
        });
        await context.SaveChangesAsync();

        var seeder = CreateSeeder(context);

        await seeder.Seed();

        var avaliacoes = await context.Avaliacoes
            .Where(a => a.TurmaId == turma.Id && a.DeletedAt == null)
            .ToListAsync();

        Assert.Single(avaliacoes);
        Assert.Equal("Projeto Liderança", avaliacoes[0].Nome);
    }

    [Fact]
    public async Task Seed_SemTurmas_NaoFazNada()
    {
        using var context = TestHelpers.CreateDbContext();
        var seeder = CreateSeeder(context);

        await seeder.Seed();

        var avaliacoes = await context.Avaliacoes
            .Where(a => a.DeletedAt == null)
            .ToListAsync();

        Assert.Empty(avaliacoes);
    }

    [Fact]
    public async Task Seed_TurmaComNecessitaAtividadesECursoGenerico_CriaAvaliacoesPadrao()
    {
        using var context = TestHelpers.CreateDbContext();
        var (_, turma) = await SeedTurma(context,
            nomeCurso: "Curso Genérico",
            necessitaAtividades: true);

        var seeder = CreateSeeder(context);

        await seeder.Seed();

        var avaliacoes = await context.Avaliacoes
            .Where(a => a.TurmaId == turma.Id && a.DeletedAt == null)
            .ToListAsync();

        Assert.Equal(2, avaliacoes.Count);
        Assert.Contains(avaliacoes, a => a.Nome == "Avaliação 1");
        Assert.Contains(avaliacoes, a => a.Nome == "Avaliação 2");
    }

    [Fact]
    public async Task Seed_MultiplasTurmas_SoCriaParaNecessitaAtividades()
    {
        using var context = TestHelpers.CreateDbContext();

        var (_, turmaCom) = await SeedTurma(context,
            nomeCurso: "Discipulado Básico",
            necessitaAtividades: true,
            nomeTurma: "Turma Com Atividades");

        var (_, turmaSem) = await SeedTurma(context,
            nomeCurso: "Consolidação",
            necessitaAtividades: false,
            nomeTurma: "Turma Sem Atividades");

        var seeder = CreateSeeder(context);

        await seeder.Seed();

        var avaliacoesCom = await context.Avaliacoes
            .Where(a => a.TurmaId == turmaCom.Id && a.DeletedAt == null)
            .ToListAsync();

        var avaliacoesSem = await context.Avaliacoes
            .Where(a => a.TurmaId == turmaSem.Id && a.DeletedAt == null)
            .ToListAsync();

        Assert.NotEmpty(avaliacoesCom);
        Assert.Empty(avaliacoesSem);
    }

    [Fact]
    public async Task Seed_DiferentesCursos_CriaAvaliacoesEspecificas()
    {
        using var context = TestHelpers.CreateDbContext();

        var (cursoEbt, turmaEbt) = await SeedTurma(context,
            nomeCurso: "Escola Bíblica de Treinamento",
            necessitaAtividades: true,
            nomeTurma: "EBT");

        var (cursoLouvor, turmaLouvor) = await SeedTurma(context,
            nomeCurso: "Ministério de Louvor",
            necessitaAtividades: true,
            nomeTurma: "Louvor");

        var seeder = CreateSeeder(context);

        await seeder.Seed();

        var avaliacoesEbt = await context.Avaliacoes
            .Where(a => a.TurmaId == turmaEbt.Id && a.DeletedAt == null)
            .ToListAsync();

        var avaliacoesLouvor = await context.Avaliacoes
            .Where(a => a.TurmaId == turmaLouvor.Id && a.DeletedAt == null)
            .ToListAsync();

        Assert.Equal(4, avaliacoesEbt.Count);
        Assert.Contains(avaliacoesEbt, a => a.Nome == "Exame Final");

        Assert.Equal(3, avaliacoesLouvor.Count);
        Assert.Contains(avaliacoesLouvor, a => a.Nome == "Avaliação Prática Vocal");
    }
}
