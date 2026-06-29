using ApiSgc.Models.ViewModels;

namespace ApiSgc.Models.Extensions;

public static class CursoExtensions
{
    public static CursoViewModel ToViewModel(this Curso curso)
    {
        return new CursoViewModel
        {
            Id = curso.Id,
            Nome = curso.Nome,
            Descricao = curso.Descricao,
        };
    }
}
