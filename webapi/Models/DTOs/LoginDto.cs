using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Senha { get; set; } = string.Empty;
}