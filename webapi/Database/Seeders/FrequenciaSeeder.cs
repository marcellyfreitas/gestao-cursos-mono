using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Database.Seeders;

public class FrequenciaSeeder : ISeeder
{
    private readonly ApplicationDbContext _context;
    public FrequenciaSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        if (await _context.Frequencias.AnyAsync())
            return;

        var matriculas = await _context.Matriculas.Where(m => m.DeletedAt == null).ToListAsync();
        var aulas = await _context.Aulas.Where(a => a.DeletedAt == null).ToListAsync();

        if (!matriculas.Any() || !aulas.Any()) return;

        var aulasPorTurma = aulas.GroupBy(a => a.TurmaId).ToDictionary(g => g.Key, g => g.ToList());
        var random = new Random(42);

        var batch = new List<Frequencia>();

        foreach (var matricula in matriculas)
        {
            if (!aulasPorTurma.TryGetValue(matricula.TurmaId, out var aulasDaTurma))
                continue;

            foreach (var aula in aulasDaTurma)
            {
                var sorteio = random.Next(100);
                var status = sorteio < 70
                    ? StatusFrequencia.PRESENTE
                    : sorteio < 90
                        ? StatusFrequencia.FALTA
                        : StatusFrequencia.FALTA_JUSTIFICADA;

                batch.Add(new Frequencia
                {
                    MatriculaId = matricula.Id,
                    AulaId = aula.Id,
                    Status = status
                });
            }
        }

        await _context.Frequencias.AddRangeAsync(batch);
        await _context.SaveChangesAsync();
    }
}
