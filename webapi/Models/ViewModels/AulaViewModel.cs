namespace ApiSgc.Models.ViewModels;

public class AulaViewModel
{
    public int Id { get; set; }
    public int TurmaId { get; set; }
    public string? TurmaNome { get; set; }
    public string? Titulo { get; set; }
    public DateOnly? DataAula { get; set; }
    public int? ProfessorId { get; set; }
    public string? ProfessorNome { get; set; }
    public string? Descricao { get; set; }
}
