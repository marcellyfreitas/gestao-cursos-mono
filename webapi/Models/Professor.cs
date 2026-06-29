using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSgc.Models;

[Table("professor")]
public class Professor
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(150, ErrorMessage = "Nome deve ter no máximo 150 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(150, ErrorMessage = "Email deve ter no máximo 150 caracteres")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string? Telefone { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public ICollection<Aula> Aulas { get; set; } = new List<Aula>();
}