using ApiSgc.Models.Enums;

namespace ApiSgc.Models.ViewModels;

public class FrequenciaAlunoViewModel
{
    public int MatriculaId { get; set; }
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public StatusFrequencia? Status { get; set; }
    public int? AulaId { get; set; }
    public bool Reprovado { get; set; }
    public int TotalFaltas { get; set; }
}
