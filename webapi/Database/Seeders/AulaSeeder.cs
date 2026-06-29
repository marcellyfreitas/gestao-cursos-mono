using ApiSgc.Database;
using ApiSgc.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Database.Seeders;

public class AulaSeeder : ISeeder
{
    private readonly ApplicationDbContext _context;

    // Temas de aula por curso (indexado pelo nome do curso)
    private static readonly Dictionary<string, string[]> TemasAula = new()
    {
        ["Fundamentos da Fé Cristã"] =
        [
            "O que é a Bíblia e como estudá-la",
            "A natureza de Deus: Pai, Filho e Espírito Santo",
            "A criação e a queda do homem",
            "O plano de salvação",
            "Arrependimento e conversão",
            "A graça de Deus",
            "Justificação pela fé",
            "O novo nascimento",
            "Batismo nas águas",
            "Batismo no Espírito Santo",
            "A vida de oração",
            "A importância da comunhão",
        ],
        ["Discipulado e Crescimento"] =
        [
            "O que é discipulado",
            "A vida devocional diária",
            "Disciplinas espirituais: jejum e oração",
            "Leitura bíblica sistemática",
            "Mordomia cristã: tempo e talentos",
            "Mordomia cristã: finanças e dízimos",
            "Relacionamentos saudáveis na igreja",
            "Servindo no corpo de Cristo",
            "Lidando com tentações",
            "Crescendo em santidade",
            "Frutos do Espírito",
            "Compartilhando a fé no dia a dia",
        ],
        ["Liderança Ministerial"] =
        [
            "Princípios bíblicos de liderança",
            "O líder servo: modelo de Jesus",
            "Gestão de equipes ministeriais",
            "Comunicação eficaz na liderança",
            "Resolução de conflitos",
            "Mentoria e formação de novos líderes",
            "Planejamento estratégico ministerial",
            "Liderança em tempos de crise",
            "Ética e integridade na liderança",
            "Delegação e empoderamento",
        ],
        ["Teologia Bíblica"] =
        [
            "Introdução à hermenêutica bíblica",
            "Panorama do Antigo Testamento",
            "Panorama do Novo Testamento",
            "Teologia Sistemática: Doutrina de Deus",
            "Teologia Sistemática: Cristologia",
            "Teologia Sistemática: Pneumatologia",
            "Teologia Sistemática: Soteriologia",
            "Teologia Sistemática: Eclesiologia",
            "Teologia Sistemática: Escatologia",
            "História da Igreja: primeiros séculos",
            "História da Igreja: Reforma Protestante",
            "Apologética cristã contemporânea",
        ],
        ["Música e Adoração"] =
        [
            "Fundamentos bíblicos da adoração",
            "O papel do ministério de louvor",
            "Técnica vocal para adoração",
            "Liderança de adoração congregacional",
            "Escolha de repertório e liturgia",
            "Dinâmica de ensaio e equipe",
            "Adoração espontânea e profética",
            "Uso de tecnologia no louvor",
        ],
        ["Evangelismo e Missões"] =
        [
            "O mandato missionário",
            "Evangelismo pessoal: abordagem prática",
            "Evangelismo urbano e contextualização",
            "Missões transculturais: desafios e preparo",
            "Plantação de igrejas",
            "Ação social e evangelismo integral",
            "Discipulado de novos convertidos",
            "Testemunho cristão no ambiente de trabalho",
            "Evangelismo digital e redes sociais",
            "Mobilização missionária na igreja local",
        ],
    };

    public AulaSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        if (await _context.Aulas.AnyAsync())
            return;

        var turmas = await _context.Turmas
            .Include(t => t.Curso)
            .Where(t => t.DeletedAt == null)
            .ToListAsync();

        var professores = await _context.Professores
            .Where(p => p.Ativo && p.DeletedAt == null)
            .ToListAsync();

        if (!turmas.Any() || !professores.Any())
            return;

        var random = new Random(42);
        var batch = new List<Aula>();

        foreach (var turma in turmas)
        {
            var cursoNome = turma.Curso?.Nome ?? "";
            var temas = TemasAula.GetValueOrDefault(cursoNome, ["Aula"]);
            var professor = professores[random.Next(professores.Count)];

            var dataInicio = turma.DataInicio ?? new DateOnly(2026, 2, 3);

            // Uma aula por semana (sábado), com temas do curso
            var dataAtual = dataInicio;
            for (var i = 0; i < temas.Length; i++)
            {
                // Avança para o próximo sábado
                while (dataAtual.DayOfWeek != DayOfWeek.Saturday)
                    dataAtual = dataAtual.AddDays(1);

                batch.Add(new Aula
                {
                    TurmaId = turma.Id,
                    Titulo = temas[i],
                    DataAula = dataAtual,
                    ProfessorId = professor.Id,
                    Descricao = $"Aula {i + 1} de {temas.Length} — {turma.Nome}"
                });

                dataAtual = dataAtual.AddDays(7); // Próxima semana
            }
        }

        await _context.Aulas.AddRangeAsync(batch);
        await _context.SaveChangesAsync();
    }
}
