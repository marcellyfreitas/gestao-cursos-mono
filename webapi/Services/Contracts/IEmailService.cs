namespace ApiSgc.Services.Contracts;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
    Task<bool> SendEmailValidationAsync(string to, string nome, string token);
    Task<bool> SendPasswordRecoveryAsync(string to, string nome, string token);
}
