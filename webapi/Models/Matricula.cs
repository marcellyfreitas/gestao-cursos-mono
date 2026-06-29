using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApiSgc.Models.Enums;

namespace ApiSgc.Models;

[Table("matricula")]
public class Matricula
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Usuario))]
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    [ForeignKey(nameof(Turma))]
    public int TurmaId { get; set; }
    public Turma Turma { get; set; } = null!;

    public DateTime DataMatricula { get; set; } = DateTime.UtcNow;
    public SituacaoMatricula Situacao { get; set; } = SituacaoMatricula.CURSANDO;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; } = null;

    public ICollection<Frequencia> Frequencias { get; set; } = new List<Frequencia>();
    public ICollection<Nota> Notas { get; set; } = new List<Nota>();
}