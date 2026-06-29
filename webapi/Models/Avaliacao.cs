using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSgc.Models;

[Table("avaliacao")]
public class Avaliacao
{
    [Key]
    public int Id { get; set; }

    public int TurmaId { get; set; }
    public Turma Turma { get; set; } = null!;

    [Required]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; } = null;

    public ICollection<Nota> Notas { get; set; } = new List<Nota>();
}
