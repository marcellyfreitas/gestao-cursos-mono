using ApiSgc.Models.ViewModels;

namespace ApiSgc.Models.Extensions;

public static class AulaExtensions
{
    public static AulaViewModel ToViewModel(this Aula aula)
    {
        return new AulaViewModel
        {
            Id = aula.Id,
            TurmaId = aula.TurmaId,
            TurmaNome = aula.Turma?.Nome,
            Titulo = aula.Titulo,
            DataAula = aula.DataAula,
            ProfessorId = aula.ProfessorId,
            ProfessorNome = aula.Professor?.Nome,
            Descricao = aula.Descricao,
        };
    }
}
