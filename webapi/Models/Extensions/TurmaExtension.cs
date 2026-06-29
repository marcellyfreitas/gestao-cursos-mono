using ApiSgc.Models.ViewModels;

namespace ApiSgc.Models.Extensions;

public static class TurmaExtensions
{
    public static TurmaViewModel ToViewModel(this Turma turma)
    {
        return new TurmaViewModel
        {
            Id = turma.Id,
            CursoId = turma.CursoId,
            NomeCurso = turma.Curso?.Nome ?? string.Empty,
            Nome = turma.Nome,
            DataInicio = turma.DataInicio,
            DataFim = turma.DataFim,
            NecessitaAtividades = turma.NecessitaAtividades,
            MediaMinima = turma.MediaMinima,
            FaltasParaReprovacao = turma.FaltasParaReprovacao,
            CreatedAt = turma.CreatedAt,
            UpdatedAt = turma.UpdatedAt,
        };
    }
}