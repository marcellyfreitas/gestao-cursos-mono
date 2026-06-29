namespace ApiSgc.Models.ViewModels;

public class AlunoTurmaViewModel
{
    public int MatriculaId { get; set; }
    public int TurmaId { get; set; }
    public string NomeTurma { get; set; } = string.Empty;
    public string NomeCurso { get; set; } = string.Empty;
    public string Situacao { get; set; } = string.Empty;
    public bool NecessitaAtividades { get; set; }
    public int FaltasParaReprovacao { get; set; }
    public int TotalAulas { get; set; }
    public int TotalPresencas { get; set; }
    public int TotalFaltas { get; set; }
    public List<AlunoNotaViewModel> Notas { get; set; } = new();
}

public class AlunoNotaViewModel
{
    public int AvaliacaoId { get; set; }
    public string NomeAvaliacao { get; set; } = string.Empty;
    public decimal? Valor { get; set; }
}