namespace ApiSgc.Models.ViewModels;

public class NotaAlunoViewModel
{
    public int MatriculaId { get; set; }
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public int? NotaId { get; set; }
    public decimal? Valor { get; set; }
    public int AvaliacaoId { get; set; }
    public string Situacao { get; set; } = string.Empty;
    public bool Reprovado { get; set; }
}
