using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class RecuperaSenhaDto
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}