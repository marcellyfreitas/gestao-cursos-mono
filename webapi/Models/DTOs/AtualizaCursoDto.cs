using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class AtualizaCursoDto
{
    [Required]
    public int Id { get; set; }

    [StringLength(150)]
    public string? Nome { get; set; }

    public string? Descricao { get; set; }
}
