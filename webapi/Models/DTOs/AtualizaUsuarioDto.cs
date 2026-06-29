using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ApiSgc.Models.Enums;

namespace ApiSgc.Models.DTOs;

public class AtualizaUsuarioDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string? Nome { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? Senha { get; set; }

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole? Role { get; set; }

    [StringLength(20)]
    public string? Telefone { get; set; }

    public DateTime? DataNascimento { get; set; }

    [StringLength(100)]
    public string? Equipe { get; set; }

    public bool? EstaEmCelula { get; set; }

    [StringLength(150)]
    public string? NomeCelula { get; set; }

    public bool? EstaSendoDiscipulado { get; set; }

    [StringLength(150)]
    public string? NomeDiscipulador { get; set; }

    public bool? FezEncontro { get; set; }

    public bool? Batizado { get; set; }
}