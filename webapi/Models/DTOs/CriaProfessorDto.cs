using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class CriaProfessorDto
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(150, ErrorMessage = "Nome deve ter no máximo 150 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(150, ErrorMessage = "Email deve ter no máximo 150 caracteres")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string? Telefone { get; set; }
}