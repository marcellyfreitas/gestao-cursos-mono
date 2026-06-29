using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class CriaCursoDto
{
    [Required]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }
}
