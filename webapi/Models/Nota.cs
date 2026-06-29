using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSgc.Models;

[Table("nota")]
public class Nota
{
    public int Id { get; set; }

    public int MatriculaId { get; set; }
    public Matricula Matricula { get; set; } = null!;

    public int AvaliacaoId { get; set; }
    public Avaliacao Avaliacao { get; set; } = null!;

    [Column(TypeName = "decimal(5,2)")]
    public decimal Valor { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; } = null;
}
