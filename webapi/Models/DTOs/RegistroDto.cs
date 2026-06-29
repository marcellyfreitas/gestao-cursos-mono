using System.ComponentModel.DataAnnotations;
using ApiSgc.Models.Enums;

namespace ApiSgc.Models.DTOs;

public class RegistroDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;

    [Required]
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
}