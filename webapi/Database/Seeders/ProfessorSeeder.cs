using ApiSgc.Database;
using ApiSgc.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Database.Seeders;

public class ProfessorSeeder : ISeeder
{
    private readonly ApplicationDbContext _context;
    public ProfessorSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        if (await _context.Professores.AnyAsync())
            return;

        var professores = new[]
        {
            new Professor { Nome = "Pr. João Carlos Silva", Email = "pr.joao@igreja.org", Telefone = "11999998888", Ativo = true },
            new Professor { Nome = "Pra. Maria Helena Oliveira", Email = "pra.maria@igreja.org", Telefone = "11888887777", Ativo = true },
            new Professor { Nome = "Ev. Carlos Eduardo Santos", Email = "ev.carlos@igreja.org", Telefone = "11777776666", Ativo = true },
            new Professor { Nome = "Mis. Ana Paula Costa", Email = "mis.ana@igreja.org", Telefone = "11666665555", Ativo = true },
            new Professor { Nome = "Pr. Pedro Augusto Almeida", Email = "pr.pedro@igreja.org", Telefone = "11555554444", Ativo = true },
            new Professor { Nome = "Pra. Lucia Regina Ferreira", Email = "pra.lucia@igreja.org", Telefone = "11444443333", Ativo = true },
            new Professor { Nome = "Ev. Marcos Vinícius Ribeiro", Email = "ev.marcos@igreja.org", Telefone = "11333332222", Ativo = true },
            new Professor { Nome = "Pr. André Luiz Campos", Email = "pr.andre@igreja.org", Telefone = "11222221111", Ativo = true },
            new Professor { Nome = "Pra. Juliana Cristina Martins", Email = "pra.juliana@igreja.org", Telefone = "11111110000", Ativo = true },
            new Professor { Nome = "Mis. Roberto Carlos Lima", Email = "mis.roberto@igreja.org", Telefone = "11900009999", Ativo = true },
            new Professor { Nome = "Ev. Patricia de Souza", Email = "ev.patricia@igreja.org", Telefone = "11988887777", Ativo = true },
            new Professor { Nome = "Pr. Thiago Henrique Gomes", Email = "pr.thiago@igreja.org", Telefone = "11877776666", Ativo = false },
        };

        foreach (var professor in professores)
            await _context.Professores.AddAsync(professor);

        await _context.SaveChangesAsync();
    }
}
