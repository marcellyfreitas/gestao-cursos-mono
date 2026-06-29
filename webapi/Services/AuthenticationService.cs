using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.DTOs;
using ApiSgc.Services.Contracts;
using ApiSgc.Utils;

namespace ApiSgc.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        ApplicationDbContext context, 
        IConfiguration config,
        IEmailService emailService,
        ILogger<AuthenticationService> logger)
    {
        _context = context;
        _config = config;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<(string? Token, string? Error)> LoginAsync(LoginDto dto)
    {
        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !PasswordHelper.VerifyPassword(dto.Senha, user.Senha))
        {
            _logger.LogWarning("[Auth.Login] Credenciais inválidas para Email={Email}", dto.Email);
            return (null, "Credenciais inválidas");
        }

        if (!user.Ativo)
        {
            _logger.LogWarning("[Auth.Login] Tentativa de login com conta não aprovada. Email={Email}, UsuarioId={UsuarioId}", dto.Email, user.Id);
            return (null, "Conta pendente de aprovação");
        }

        _logger.LogInformation("[Auth.Login] Login bem-sucedido. UsuarioId={UsuarioId}, Email={Email}, Role={Role}", user.Id, user.Email, user.Role);
        return (GenerateJwtToken(user), null);
    }

    public async Task<(bool Success, string? Message)> RegisterAsync(Usuario user)
    {
        var existingUser = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == user.Email);

        if (existingUser != null)
        {
            _logger.LogWarning("[Auth.Register] Tentativa de registro com email já existente. Email={Email}", user.Email);
            return (false, "Email em uso");
        }

        // Aluno auto-registrado: Ativo=false, precisa de aprovação do admin
        user.Ativo = false;
        user.EmailValidado = true; // Mantém true pois não usamos mais validação por email

        await _context.Usuarios.AddAsync(user);
        await _context.SaveChangesAsync();

        // Email de validação desativado — aprovação agora é feita pelo admin
        // await _emailService.SendEmailValidationAsync(user.Email, user.Nome, user.TokenValidacaoEmail);

        _logger.LogInformation("[Auth.Register] Usuário registrado (pendente de aprovação). UsuarioId={UsuarioId}, Email={Email}, Nome={Nome}",
            user.Id, user.Email, user.Nome);

        return (true, "Registro realizado. Aguarde a aprovação de um administrador para acessar o sistema.");
    }

    public async Task<(bool Success, string? Message)> ResendValidationEmailAsync(string email)
    {
        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            _logger.LogWarning("[Auth.ResendValidation] Usuário não encontrado. Email={Email}", email);
            return (false, "Usuário não encontrado");
        }

        if (user.EmailValidado)
        {
            _logger.LogWarning("[Auth.ResendValidation] Email já validado. Email={Email}, UsuarioId={UsuarioId}", email, user.Id);
            return (false, "Email já validado");
        }

        user.TokenValidacaoEmail = GenerateRandomToken();
        user.DataExpiracaoToken = DateTime.UtcNow.AddHours(24);

        await _context.SaveChangesAsync();

        await _emailService.SendEmailValidationAsync(
            user.Email,
            user.Nome,
            user.TokenValidacaoEmail
        );

        _logger.LogInformation("[Auth.ResendValidation] Email de validação reenviado. UsuarioId={UsuarioId}, Email={Email}", user.Id, email);

        return (true, "Email de validação reenviado");
    }

    public async Task<(bool Success, string? Error)> ValidateEmailAsync(ValidaEmailDto dto)
    {
        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            _logger.LogWarning("[Auth.ValidateEmail] Usuário não encontrado. Email={Email}", dto.Email);
            return (false, "Usuário não encontrado");
        }

        if (user.EmailValidado)
        {
            return (false, "Email já validado");
        }

        if (user.TokenValidacaoEmail != dto.Token)
        {
            _logger.LogWarning("[Auth.ValidateEmail] Token inválido. Email={Email}, UsuarioId={UsuarioId}", dto.Email, user.Id);
            return (false, "Token inválido");
        }

        if (user.DataExpiracaoToken.HasValue && user.DataExpiracaoToken.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("[Auth.ValidateEmail] Token expirado. Email={Email}, UsuarioId={UsuarioId}, Expirou={Expiracao}",
                dto.Email, user.Id, user.DataExpiracaoToken.Value);
            return (false, "Token expirado");
        }

        user.EmailValidado = true;
        user.TokenValidacaoEmail = null;
        user.DataExpiracaoToken = null;
        await _context.SaveChangesAsync();

        _logger.LogInformation("[Auth.ValidateEmail] Email validado com sucesso. UsuarioId={UsuarioId}, Email={Email}", user.Id, dto.Email);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RequestPasswordRecoveryAsync(RecuperaSenhaDto dto)
    {
        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            _logger.LogWarning("[Auth.RecuperaSenha] Email não encontrado no sistema. Email={Email}", dto.Email);
            return (false, "Se o email existir, um link de recuperação será enviado");
        }

        user.TokenRecuperaSenha = GenerateRandomToken();
        user.DataExpiracaoToken = DateTime.UtcNow.AddHours(1);

        await _context.SaveChangesAsync();

        await _emailService.SendPasswordRecoveryAsync(
            user.Email,
            user.Nome,
            user.TokenRecuperaSenha
        );

        _logger.LogInformation("[Auth.RecuperaSenha] Link de recuperação enviado. UsuarioId={UsuarioId}, Email={Email}", user.Id, dto.Email);

        return (true, "Se o email existir, um link de recuperação será enviado");
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(ResetaSenhaDto dto)
    {
        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            _logger.LogWarning("[Auth.ResetaSenha] Usuário não encontrado. Email={Email}", dto.Email);
            return (false, "Usuário não encontrado");
        }

        if (user.TokenRecuperaSenha != dto.Token)
        {
            _logger.LogWarning("[Auth.ResetaSenha] Token inválido. Email={Email}, UsuarioId={UsuarioId}", dto.Email, user.Id);
            return (false, "Token inválido");
        }

        if (user.DataExpiracaoToken.HasValue && user.DataExpiracaoToken.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("[Auth.ResetaSenha] Token expirado. Email={Email}, UsuarioId={UsuarioId}, Expirou={Expiracao}",
                dto.Email, user.Id, user.DataExpiracaoToken.Value);
            return (false, "Token expirado");
        }

        user.Senha = PasswordHelper.HashPassword(dto.NovaSenha);
        user.TokenRecuperaSenha = null;
        user.DataExpiracaoToken = null;
        await _context.SaveChangesAsync();

        _logger.LogInformation("[Auth.ResetaSenha] Senha redefinida com sucesso. UsuarioId={UsuarioId}, Email={Email}", user.Id, dto.Email);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, AlteraSenhaDto dto)
    {
        var user = await _context.Usuarios.FindAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("[Auth.AlteraSenha] Usuário não encontrado. UsuarioId={UsuarioId}", userId);
            return (false, "Usuário não encontrado");
        }

        if (!PasswordHelper.VerifyPassword(dto.SenhaAtual, user.Senha))
        {
            _logger.LogWarning("[Auth.AlteraSenha] Senha atual incorreta. UsuarioId={UsuarioId}", userId);
            return (false, "Senha atual incorreta");
        }

        user.Senha = PasswordHelper.HashPassword(dto.NovaSenha);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("[Auth.AlteraSenha] Senha alterada com sucesso. UsuarioId={UsuarioId}", userId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> AdminResetPasswordAsync(int userId, string novaSenha)
    {
        var user = await _context.Usuarios.FindAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("[Auth.AdminResetaSenha] Usuário não encontrado. UsuarioId={UsuarioId}", userId);
            return (false, "Usuário não encontrado");
        }

        user.Senha = PasswordHelper.HashPassword(novaSenha);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("[Auth.AdminResetaSenha] Senha resetada pelo admin. UsuarioId={UsuarioId}", userId);
        return (true, null);
    }

    public async Task<Usuario?> GetUserAsync(string id)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id.ToString() == id);
    }

    public async Task<bool> CheckIfUserExists(string email)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Email == email);
    }

    private string GenerateJwtToken(Usuario user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");

        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Nome),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRandomToken()
    {
        return Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks.ToString("X");
    }
}