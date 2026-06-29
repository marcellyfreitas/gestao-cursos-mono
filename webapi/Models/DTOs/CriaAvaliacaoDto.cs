using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class CriaAvaliacaoDto
{
    [Required]
    public int TurmaId { get; set; }

    [Required]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;
}
