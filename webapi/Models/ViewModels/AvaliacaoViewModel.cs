namespace ApiSgc.Models.ViewModels;

public class AvaliacaoViewModel
{
    public int Id { get; set; }
    public int TurmaId { get; set; }
    public string? NomeTurma { get; set; }
    public string Nome { get; set; } = string.Empty;
}
