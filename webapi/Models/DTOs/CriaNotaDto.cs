using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class CriaNotaDto
{
    [Required]
    public int MatriculaId { get; set; }

    [Required]
    public int AvaliacaoId { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal Valor { get; set; }
}
