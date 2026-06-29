using ApiSgc.Database;
using ApiSgc.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Database.Seeders;

public class NotaSeeder : ISeeder
{
    private readonly ApplicationDbContext _context;
    public NotaSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        if (await _context.Notas.AnyAsync())
            return;

        var matriculas = await _context.Matriculas.Where(m => m.DeletedAt == null).ToListAsync();
        var avaliacoes = await _context.Avaliacoes.Where(a => a.DeletedAt == null).ToListAsync();

        if (!matriculas.Any() || !avaliacoes.Any()) return;

        var avaliacoesPorTurma = avaliacoes.GroupBy(a => a.TurmaId).ToDictionary(g => g.Key, g => g.ToList());
        var random = new Random(42);

        var batch = new List<Nota>();

        foreach (var matricula in matriculas)
        {
            if (!avaliacoesPorTurma.TryGetValue(matricula.TurmaId, out var avaliacoesDaTurma))
                continue;

            foreach (var avaliacao in avaliacoesDaTurma)
            {
                var valor = Math.Round((decimal)(random.NextDouble() * 4 + 6), 2);

                batch.Add(new Nota
                {
                    MatriculaId = matricula.Id,
                    AvaliacaoId = avaliacao.Id,
                    Valor = valor
                });
            }
        }

        await _context.Notas.AddRangeAsync(batch);
        await _context.SaveChangesAsync();
    }
}
