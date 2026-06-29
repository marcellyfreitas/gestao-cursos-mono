using ApiSgc.Models.ViewModels;

namespace ApiSgc.Models.Extensions;

public static class NotaExtensions
{
    public static NotaViewModel ToViewModel(this Nota nota)
    {
        return new NotaViewModel
        {
            Id = nota.Id,
            MatriculaId = nota.MatriculaId,
            AvaliacaoId = nota.AvaliacaoId,
            Valor = nota.Valor,
        };
    }
}
