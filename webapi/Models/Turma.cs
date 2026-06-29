using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSgc.Models;

[Table("turma")]
public class Turma
{
    [Key]
    public int Id { get; set; }

    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }

    public bool NecessitaAtividades { get; set; } = false;

    [Column(TypeName = "decimal(5,2)")]
    public decimal? MediaMinima { get; set; }

    [Range(0, int.MaxValue)]
    public int FaltasParaReprovacao { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public ICollection<Aula> Aulas { get; set; } = new List<Aula>();
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
}