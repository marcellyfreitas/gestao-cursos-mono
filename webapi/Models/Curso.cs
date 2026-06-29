using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSgc.Models;

[Table("curso")]
public class Curso
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; } = null;

    public ICollection<CursoPrerequisito> PrerequisitosDe { get; set; } = new List<CursoPrerequisito>();
    public ICollection<CursoPrerequisito> PrerequisitosPara { get; set; } = new List<CursoPrerequisito>();
    public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
}
