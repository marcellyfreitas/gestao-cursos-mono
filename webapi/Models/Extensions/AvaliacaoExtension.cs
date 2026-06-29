using ApiSgc.Models.ViewModels;

namespace ApiSgc.Models.Extensions;

public static class AvaliacaoExtensions
{
    public static AvaliacaoViewModel ToViewModel(this Avaliacao avaliacao)
    {
        return new AvaliacaoViewModel
        {
            Id = avaliacao.Id,
            TurmaId = avaliacao.TurmaId,
            NomeTurma = avaliacao.Turma?.Nome,
            Nome = avaliacao.Nome,
        };
    }
}
