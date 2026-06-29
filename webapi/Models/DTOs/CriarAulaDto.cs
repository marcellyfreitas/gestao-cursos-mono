using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class CriaAulaDto
{
    [Required]
    public int TurmaId { get; set; }

    [StringLength(150)]
    public string? Titulo { get; set; }

    public DateOnly? DataAula { get; set; }

    public int? ProfessorId { get; set; }

    [StringLength(500)]
    public string? Descricao { get; set; }
}
