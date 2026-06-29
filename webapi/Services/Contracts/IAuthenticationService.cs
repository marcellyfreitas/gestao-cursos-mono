using ApiSgc.Models;
using ApiSgc.Models.DTOs;

namespace ApiSgc.Services;

public interface IAuthenticationService
{
    Task<(string? Token, string? Error)> LoginAsync(LoginDto dto);
    Task<(bool Success, string? Message)> RegisterAsync(Usuario usuario);
    Task<(bool Success, string? Message)> ResendValidationEmailAsync(string email);
    Task<(bool Success, string? Error)> ValidateEmailAsync(ValidaEmailDto dto);
    Task<(bool Success, string? Error)> RequestPasswordRecoveryAsync(RecuperaSenhaDto dto);
    Task<(bool Success, string? Error)> ResetPasswordAsync(ResetaSenhaDto dto);
    Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, AlteraSenhaDto dto);
    Task<(bool Success, string? Error)> AdminResetPasswordAsync(int userId, string novaSenha);
    Task<Usuario?> GetUserAsync(string id);
    Task<bool> CheckIfUserExists(string email);
}