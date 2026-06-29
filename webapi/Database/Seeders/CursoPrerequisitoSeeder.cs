using ApiSgc.Database;
using ApiSgc.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Database.Seeders;

public class CursoPrerequisitoSeeder : ISeeder
{
    private readonly ApplicationDbContext _context;
    public CursoPrerequisitoSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        if (await _context.CursoPrerequisitos.AnyAsync())
            return;

        var fundamentos = await _context.Cursos.FirstOrDefaultAsync(c => c.Nome == "Fundamentos da Fé Cristã");
        var discipulado = await _context.Cursos.FirstOrDefaultAsync(c => c.Nome == "Discipulado e Crescimento");
        var teologia = await _context.Cursos.FirstOrDefaultAsync(c => c.Nome == "Teologia Bíblica");
        var lideranca = await _context.Cursos.FirstOrDefaultAsync(c => c.Nome == "Liderança Ministerial");
        var evangelismo = await _context.Cursos.FirstOrDefaultAsync(c => c.Nome == "Evangelismo e Missões");
        var musica = await _context.Cursos.FirstOrDefaultAsync(c => c.Nome == "Música e Adoração");

        if (fundamentos == null || discipulado == null || teologia == null || lideranca == null || evangelismo == null)
            return;

        var prerequisitos = new[]
        {
            new CursoPrerequisito { CursoId = discipulado.Id, CursoPrerequisitoId = fundamentos.Id },
            new CursoPrerequisito { CursoId = teologia.Id, CursoPrerequisitoId = discipulado.Id },
            new CursoPrerequisito { CursoId = lideranca.Id, CursoPrerequisitoId = teologia.Id },
            new CursoPrerequisito { CursoId = evangelismo.Id, CursoPrerequisitoId = fundamentos.Id },
        };

        foreach (var prereq in prerequisitos)
            await _context.CursoPrerequisitos.AddAsync(prereq);

        await _context.SaveChangesAsync();
    }
}
