using ApiSgc.Models.ViewModels;

namespace ApiSgc.Models.Extensions;

public static class CursoPrerequisitoExtensions
{
    public static CursoPrerequisitoViewModel ToViewModel(this CursoPrerequisito cursoPrerequisito)
    {
        return new CursoPrerequisitoViewModel
        {
            Id = cursoPrerequisito.Id,
            CursoId = cursoPrerequisito.CursoId,
            CursoPrerequisitoId = cursoPrerequisito.CursoPrerequisitoId,
        };
    }
}
