using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Database.Seeders;

public class MatriculaSeeder : ISeeder
{
    private readonly ApplicationDbContext _context;
    public MatriculaSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        if (await _context.Matriculas.AnyAsync())
            return;

        var turmas = await _context.Turmas.Where(t => t.DeletedAt == null).OrderBy(t => t.Id).ToListAsync();
        var alunos = await _context.Usuarios
            .Where(u => u.Role == UserRole.ALUNO && u.DeletedAt == null && u.Ativo)
            .OrderBy(u => u.Id)
            .ToListAsync();

        if (!turmas.Any() || !alunos.Any()) return;

        var random = new Random(42);
        var batch = new List<Matricula>();
        var matriculasExistentes = new HashSet<(int, int)>();

        foreach (var turma in turmas)
        {
            // Cada turma recebe entre 60% e 90% dos alunos disponíveis
            var qtdAlunos = Math.Max(3, (int)(alunos.Count * (0.6 + random.NextDouble() * 0.3)));
            var alunosShuffled = alunos.OrderBy(_ => random.Next()).Take(qtdAlunos).ToList();

            foreach (var aluno in alunosShuffled)
            {
                var chave = (aluno.Id, turma.Id);
                if (matriculasExistentes.Contains(chave))
                    continue;

                matriculasExistentes.Add(chave);

                batch.Add(new Matricula
                {
                    UsuarioId = aluno.Id,
                    TurmaId = turma.Id,
                    DataMatricula = DateTime.UtcNow.AddDays(-random.Next(10, 60)),
                    Situacao = SituacaoMatricula.CURSANDO
                });
            }
        }

        await _context.Matriculas.AddRangeAsync(batch);
        await _context.SaveChangesAsync();
    }
}
