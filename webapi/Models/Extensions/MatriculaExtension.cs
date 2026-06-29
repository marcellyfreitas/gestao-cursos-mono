using ApiSgc.Models.ViewModels;

namespace ApiSgc.Models.Extensions;

public static class MatriculaExtensions
{
public static MatriculaViewModel ToViewModel(this Matricula matricula)
{
    return new MatriculaViewModel
    {
        Id = matricula.Id,
        UsuarioId = matricula.UsuarioId,
        TurmaId = matricula.TurmaId,
        NomeAluno = matricula.Usuario?.Nome ?? string.Empty,
        NomeTurma = matricula.Turma?.Nome ?? string.Empty,
        DataMatricula = matricula.DataMatricula,
        Situacao = matricula.Situacao,
    };
}
}
