using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class AtualizaAvaliacaoDto
{
    [Required]
    public int Id { get; set; }

    public int? TurmaId { get; set; }

    [StringLength(150)]
    public string? Nome { get; set; }
}
