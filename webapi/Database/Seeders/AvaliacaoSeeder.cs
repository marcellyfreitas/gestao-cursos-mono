using ApiSgc.Database;
using ApiSgc.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Database.Seeders;

public class AvaliacaoSeeder : ISeeder
{
    private readonly ApplicationDbContext _context;

    // Nomes de avaliações por curso
    private static readonly Dictionary<string, string[]> AvaliacoesPorCurso = new()
    {
        ["Fundamentos da Fé Cristã"] =
        [
            "Prova 1 - Doutrina da Salvação",
            "Prova 2 - Vida no Espírito",
            "Trabalho Final - Testemunho Pessoal",
        ],
        ["Discipulado e Crescimento"] =
        [
            "Avaliação 1 - Disciplinas Espirituais",
            "Avaliação 2 - Mordomia Cristã",
            "Projeto Final - Plano de Discipulado",
        ],
        ["Liderança Ministerial"] =
        [
            "Estudo de Caso - Liderança Bíblica",
            "Avaliação - Gestão e Comunicação",
            "Projeto Final - Plano Ministerial",
        ],
        ["Teologia Bíblica"] =
        [
            "Prova 1 - Antigo Testamento e Hermenêutica",
            "Prova 2 - Novo Testamento e Teologia Sistemática",
            "Monografia - Tema Teológico",
        ],
        ["Evangelismo e Missões"] =
        [
            "Relatório - Evangelismo Prático",
            "Avaliação - Missões e Contextualização",
            "Projeto Final - Estratégia Missionária",
        ],
    };

    public AvaliacaoSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        if (await _context.Avaliacoes.AnyAsync())
            return;

        var turmas = await _context.Turmas
            .Include(t => t.Curso)
            .Where(t => t.DeletedAt == null)
            .ToListAsync();

        foreach (var turma in turmas)
        {
            if (!turma.NecessitaAtividades)
                continue;

            var cursoNome = turma.Curso?.Nome ?? "";
            var nomes = AvaliacoesPorCurso.GetValueOrDefault(cursoNome, 
                [$"Avaliação 1 - {cursoNome}", $"Avaliação 2 - {cursoNome}", $"Avaliação Final - {cursoNome}"]);

            foreach (var nome in nomes)
            {
                await _context.Avaliacoes.AddAsync(new Avaliacao
                {
                    TurmaId = turma.Id,
                    Nome = nome
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}
