using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class ResetaSenhaDto
{
    [Required]
    public string? Email { get; set; }

    [Required]
    public string? Token { get; set; }

    [Required]
    [StringLength(255)]
    public string? NovaSenha { get; set; }
}