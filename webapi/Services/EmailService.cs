using SendGrid;
using SendGrid.Helpers.Mail;
using ApiSgc.Services.Contracts;
using ApiSgc.Settings;

namespace ApiSgc.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly string _frontendUrl;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _emailSettings = configuration.GetSection("SmtpSettings").Get<EmailSettings>() ?? new EmailSettings();
        _frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:3000";
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var client = new SendGridClient(_emailSettings.SendGridApiKey);
            var from = new EmailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            var toEmail = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, "", body);
            msg.HtmlContent = body;

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Email enviado com sucesso para {To}", to);
                return true;
            }

            var responseBody = await response.Body.ReadAsStringAsync();
            _logger.LogWarning("Falha ao enviar email para {To}. Status: {Status}, Response: {Response}", to, response.StatusCode, responseBody);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para {To}", to);
            return false;
        }
    }

    public async Task<bool> SendEmailValidationAsync(string to, string nome, string token)
    {
        var link = $"{_frontendUrl}/authentication/validar-email?token={token}&email={to}";
        var body = $@"
            <h2>Olá, {nome}!</h2>
            <p>Bem-vindo à Escola Ministerial!</p>
            <p>Para validar seu email, clique no botão abaixo:</p>
            <p>
                <a href='{link}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Validar Email</a>
            </p>
            <p>Ou copie e cole o seguinte link no seu navegador:</p>
            <p><a href='{link}'>{link}</a></p>
            <p>Este link expira em 24 horas.</p>
            <p>Atenciosamente,<br>Escola Ministerial</p>
        ";

        return await SendEmailAsync(to, "Validação de Email - Escola Ministerial", body);
    }

    public async Task<bool> SendPasswordRecoveryAsync(string to, string nome, string token)
    {
        var link = $"{_frontendUrl}/authentication/recuperar-senha?token={token}&email={to}";
        var body = $@"
            <h2>Olá, {nome}!</h2>
            <p>Você solicitou a recuperação de senha.</p>
            <p>Clique no botão abaixo para criar uma nova senha:</p>
            <p>
                <a href='{link}' style='background-color: #2196F3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Recuperar Senha</a>
            </p>
            <p>Ou copie e cole o seguinte link no seu navegador:</p>
            <p><a href='{link}'>{link}</a></p>
            <p>Este link expira em 1 hora.</p>
            <p>Se você não solicitou esta recuperação, por favor ignore este email.</p>
            <p>Atenciosamente,<br>Escola Ministerial</p>
        ";

        return await SendEmailAsync(to, "Recuperação de Senha - Escola Ministerial", body);
    }
}