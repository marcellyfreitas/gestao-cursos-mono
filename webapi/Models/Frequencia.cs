using System.ComponentModel.DataAnnotations.Schema;
using ApiSgc.Models.Enums;

namespace ApiSgc.Models;

[Table("frequencia")]
public class Frequencia
{
    public int Id { get; set; }

    public int MatriculaId { get; set; }
    public Matricula Matricula { get; set; } = null!;

    public int AulaId { get; set; }
    public Aula Aula { get; set; } = null!;

    public StatusFrequencia Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; } = null;
}
