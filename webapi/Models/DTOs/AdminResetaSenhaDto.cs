using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class AdminResetaSenhaDto
{
    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Nova senha deve ter pelo menos 6 caracteres")]
    [StringLength(255)]
    public string NovaSenha { get; set; } = string.Empty;
}
