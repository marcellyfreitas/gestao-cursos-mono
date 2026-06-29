using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class AtualizaMatriculaDto
{
    [Required]
    public int Id { get; set; }

    public int? AlunoId { get; set; }

    public int? TurmaId { get; set; }
}
