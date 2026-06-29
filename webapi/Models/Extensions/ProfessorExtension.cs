using ApiSgc.Models.ViewModels;

namespace ApiSgc.Models.Extensions;

public static class ProfessorExtensions
{
    public static ProfessorViewModel ToViewModel(this Professor professor)
    {
        return new ProfessorViewModel
        {
            Id = professor.Id,
            Nome = professor.Nome,
            Email = professor.Email,
            Telefone = professor.Telefone,
            Status = professor.Ativo ? "Ativo" : "Inativo"
        };
    }
}