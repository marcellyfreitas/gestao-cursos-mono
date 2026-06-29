using ApiSgc.Models.Enums;

namespace ApiSgc.Models.ViewModels;

public class FrequenciaViewModel
{
    public int Id { get; set; }
    public int MatriculaId { get; set; }
    public int AulaId { get; set; }
    public StatusFrequencia Status { get; set; }
}
