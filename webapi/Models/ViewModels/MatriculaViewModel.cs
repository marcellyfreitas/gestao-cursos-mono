using ApiSgc.Models.Enums;

namespace ApiSgc.Models.ViewModels;

public class MatriculaViewModel
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int TurmaId { get; set; }
    public string NomeAluno { get; set; } = string.Empty;
    public string NomeTurma { get; set; } = string.Empty;
    public DateTime DataMatricula { get; set; }
    public SituacaoMatricula Situacao { get; set; }
}
