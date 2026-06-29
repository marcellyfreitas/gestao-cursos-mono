using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class CriaMatriculaDto
{
    [Required]
    public int AlunoId { get; set; }

    [Required]
    public int TurmaId { get; set; }
}
