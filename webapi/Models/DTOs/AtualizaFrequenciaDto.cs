using System.ComponentModel.DataAnnotations;
using ApiSgc.Models.Enums;

namespace ApiSgc.Models.DTOs;

public class AtualizaFrequenciaDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    public StatusFrequencia Status { get; set; }
}
