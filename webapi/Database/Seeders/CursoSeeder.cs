using ApiSgc.Database;
using ApiSgc.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Database.Seeders;

public class CursoSeeder : ISeeder
{
    private readonly ApplicationDbContext _context;
    public CursoSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        if (await _context.Cursos.AnyAsync())
            return;

        var cursos = new[]
        {
            new Curso
            {
                Nome = "Fundamentos da Fé Cristã",
                Descricao = "Curso introdutório que aborda os pilares da fé cristã: a Bíblia como Palavra de Deus, a Trindade, salvação pela graça, arrependimento, batismo e vida no Espírito Santo. Indicado para novos convertidos e membros que desejam fortalecer suas bases doutrinárias.",
            },
            new Curso
            {
                Nome = "Discipulado e Crescimento",
                Descricao = "Formação prática para o crescimento espiritual contínuo. Aborda disciplinas espirituais (oração, jejum, leitura bíblica), mordomia cristã (tempo, talentos e finanças), relacionamentos na igreja e serviço ao próximo. Pré-requisito: Fundamentos da Fé.",
            },
            new Curso
            {
                Nome = "Liderança Ministerial",
                Descricao = "Capacitação para líderes de células, departamentos e ministérios. Ensina princípios bíblicos de liderança servidora, gestão de equipes, comunicação, resolução de conflitos, mentoria e planejamento estratégico ministerial.",
            },
            new Curso
            {
                Nome = "Teologia Bíblica",
                Descricao = "Estudo aprofundado das Escrituras com foco em hermenêutica, panorama do AT e NT, teologia sistemática (Deus, Cristo, Espírito Santo, Salvação, Igreja, Últimas Coisas), história da igreja e apologética cristã contemporânea.",
            },
            new Curso
            {
                Nome = "Música e Adoração",
                Descricao = "Formação para integrantes do ministério de louvor. Aborda fundamentos bíblicos da adoração, técnica vocal, liderança de adoração congregacional, escolha de repertório, dinâmica de ensaio e uso de tecnologia no louvor. Não exige avaliações formais.",
            },
            new Curso
            {
                Nome = "Evangelismo e Missões",
                Descricao = "Treinamento teórico e prático para evangelismo pessoal, urbano e digital. Inclui missões transculturais, plantação de igrejas, ação social, discipulado de novos convertidos e mobilização missionária na igreja local.",
            },
        };

        foreach (var curso in cursos)
            await _context.Cursos.AddAsync(curso);

        await _context.SaveChangesAsync();
    }
}
