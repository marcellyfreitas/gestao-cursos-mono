using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class CriaCursoPrerequisitoDto
{
    [Required]
    public int CursoId { get; set; }

    [Required]
    public int CursoPrerequisitoId { get; set; }
}
