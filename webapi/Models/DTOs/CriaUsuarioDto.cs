using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ApiSgc.Models.Enums;

namespace ApiSgc.Models.DTOs;

public class CriaUsuarioDto
{
    [Required]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Senha { get; set; } = string.Empty;

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
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