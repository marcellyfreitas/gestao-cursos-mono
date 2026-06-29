using ApiSgc.Database;
using ApiSgc.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Database.Seeders;

public class TurmaSeeder : ISeeder
{
    private readonly ApplicationDbContext _context;

    internal static readonly (string CursoNome, string NomeTurma, bool NecessitaAtividades, decimal? MediaMinima, int Faltas, DateOnly Inicio, DateOnly Fim)[] TurmasDef =
    [
        ("Fundamentos da Fé Cristã", "Fundamentos da Fé - 1º Semestre 2026", true, 7.0m, 5, new DateOnly(2026, 2, 3), new DateOnly(2026, 6, 27)),
        ("Discipulado e Crescimento", "Discipulado - 1º Semestre 2026", true, 7.0m, 4, new DateOnly(2026, 2, 3), new DateOnly(2026, 6, 27)),
        ("Liderança Ministerial", "Liderança Ministerial - Turma A 2026", true, 7.5m, 3, new DateOnly(2026, 3, 3), new DateOnly(2026, 7, 25)),
        ("Teologia Bíblica", "Teologia Bíblica - Turma A 2026", true, 7.0m, 4, new DateOnly(2026, 2, 10), new DateOnly(2026, 6, 30)),
        ("Música e Adoração", "Louvor e Adoração - 1º Semestre 2026", false, null, 3, new DateOnly(2026, 2, 5), new DateOnly(2026, 6, 25)),
        ("Evangelismo e Missões", "Evangelismo Urbano - Turma 2026", true, 7.0m, 5, new DateOnly(2026, 3, 10), new DateOnly(2026, 7, 18)),
    ];

    public TurmaSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        if (await _context.Turmas.AnyAsync())
            return;

        foreach (var (cursoNome, nomeTurma, necessitaAtividades, mediaMinima, faltas, inicio, fim) in TurmasDef)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Nome == cursoNome);

            if (curso == null)
                continue;

            await _context.Turmas.AddAsync(new Turma
            {
                CursoId = curso.Id,
                Nome = nomeTurma,
                DataInicio = inicio,
                DataFim = fim,
                NecessitaAtividades = necessitaAtividades,
                MediaMinima = mediaMinima,
                FaltasParaReprovacao = faltas
            });
        }

        await _context.SaveChangesAsync();
    }
}
