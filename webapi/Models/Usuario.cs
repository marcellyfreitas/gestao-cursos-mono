using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApiSgc.Models.Enums;

namespace ApiSgc.Models;

[Table("usuario")]
public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Senha { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    public DateTime? DataNascimento { get; set; }

    [StringLength(100)]
    public string? Equipe { get; set; }

    public bool EstaEmCelula { get; set; } = false;

    [StringLength(150)]
    public string? NomeCelula { get; set; }

    public bool EstaSendoDiscipulado { get; set; } = false;

    [StringLength(150)]
    public string? NomeDiscipulador { get; set; }

    public bool FezEncontro { get; set; } = false;

    public bool Batizado { get; set; } = false;

    public bool EmailValidado { get; set; } = false;

    /// <summary>
    /// Indica se o usuário está ativo (aprovado) no sistema.
    /// Alunos auto-registrados começam com Ativo=false e precisam de aprovação do admin.
    /// Usuários criados pelo admin podem começar com Ativo=true.
    /// </summary>
    public bool Ativo { get; set; } = false;

    [StringLength(255)]
    public string? TokenValidacaoEmail { get; set; }

    [StringLength(255)]
    public string? TokenRecuperaSenha { get; set; }

    public DateTime? DataExpiracaoToken { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}