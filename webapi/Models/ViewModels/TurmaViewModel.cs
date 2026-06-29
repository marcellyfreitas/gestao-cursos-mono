namespace ApiSgc.Models.ViewModels;

public class TurmaViewModel
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public string NomeCurso { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public bool NecessitaAtividades { get; set; }
    public decimal? MediaMinima { get; set; }
    public int FaltasParaReprovacao { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}