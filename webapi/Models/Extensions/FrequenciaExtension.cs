using ApiSgc.Models.ViewModels;

namespace ApiSgc.Models.Extensions;

public static class FrequenciaExtensions
{
    public static FrequenciaViewModel ToViewModel(this Frequencia frequencia)
    {
        return new FrequenciaViewModel
        {
            Id = frequencia.Id,
            MatriculaId = frequencia.MatriculaId,
            AulaId = frequencia.AulaId,
            Status = frequencia.Status,
        };
    }
}
