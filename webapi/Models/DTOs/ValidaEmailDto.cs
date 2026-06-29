using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class ValidaEmailDto
{
    [Required]
    public string? Email { get; set; }

    [Required]
    public string? Token { get; set; }
}