using System.ComponentModel.DataAnnotations;
using ApiSgc.Models.Enums;

namespace ApiSgc.Models.DTOs;

public class CriaFrequenciaDto
{
    [Required]
    public int MatriculaId { get; set; }

    [Required]
    public int AulaId { get; set; }

    [Required]
    public StatusFrequencia Status { get; set; }
}
