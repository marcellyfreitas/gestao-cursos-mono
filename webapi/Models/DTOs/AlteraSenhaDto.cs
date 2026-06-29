using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class AlteraSenhaDto
{
    [Required(ErrorMessage = "Senha atual é obrigatória")]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Nova senha deve ter pelo menos 6 caracteres")]
    [StringLength(255)]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare("NovaSenha", ErrorMessage = "As senhas não conferem")]
    public string ConfirmaNovaSenha { get; set; } = string.Empty;
}
