using System.ComponentModel.DataAnnotations;
using ApiSgc.Models.Enums;

namespace ApiSgc.Models.DTOs;

public class SalvarFrequenciaLoteDto
{
    [Required]
    public int AulaId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Informe ao menos um aluno")]
    public List<FrequenciaItemDto> Items { get; set; } = new();
}

public class FrequenciaItemDto
{
    [Required]
    public int MatriculaId { get; set; }

    [Required]
    public StatusFrequencia Status { get; set; }
}
