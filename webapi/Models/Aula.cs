using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSgc.Models;

[Table("aula")]
public class Aula
{
    [Key]
    public int Id { get; set; }

    public int TurmaId { get; set; }
    public Turma Turma { get; set; } = null!;

    [StringLength(150)]
    public string? Titulo { get; set; }

    public DateOnly? DataAula { get; set; }

    public int? ProfessorId { get; set; }
    public Professor? Professor { get; set; }

    [StringLength(500)]
    public string? Descricao { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public ICollection<Frequencia> Frequencias { get; set; } = new List<Frequencia>();
}
