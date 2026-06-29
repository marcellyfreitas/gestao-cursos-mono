using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.DTOs;
using ApiSgc.Models.Enums;
using ApiSgc.Services;
using ApiSgc.Services.Contracts;
using ApiSgc.Tests.Helpers;
using Xunit;

namespace ApiSgc.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly IConfiguration _configuration;

    public AuthenticationServiceTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _emailServiceMock
            .Setup(e => e.SendEmailValidationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _emailServiceMock
            .Setup(e => e.SendPasswordRecoveryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        _configuration = TestHelpers.MockConfiguration();
    }

    private AuthenticationService CreateService(ApplicationDbContext context)
    {
        return new AuthenticationService(
            context,
            _configuration,
            _emailServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com", UserRole.ALUNO, emailValidado: true);
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (token, error) = await service.LoginAsync(new LoginDto
        {
            Email = "test@email.com",
            Senha = TestHelpers.DefaultPassword
        });

        Assert.NotNull(token);
        Assert.Null(error);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var claims = jwt.Claims.ToDictionary(c => c.Type, c => c.Value);

        Assert.Equal(user.Id.ToString(), claims[JwtRegisteredClaimNames.Sub]);
        Assert.Equal(user.Email, claims[JwtRegisteredClaimNames.Email]);
        Assert.Equal(user.Nome, claims[ClaimTypes.Name]);
        Assert.Equal(user.Role.ToString(), claims[ClaimTypes.Role]);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com", emailValidado: true);
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (token, error) = await service.LoginAsync(new LoginDto
        {
            Email = "test@email.com",
            Senha = "WrongPassword123!"
        });

        Assert.Null(token);
        Assert.Equal("Credenciais inválidas", error);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var (token, error) = await service.LoginAsync(new LoginDto
        {
            Email = "nonexistent@email.com",
            Senha = TestHelpers.DefaultPassword
        });

        Assert.Null(token);
        Assert.Equal("Credenciais inválidas", error);
    }

    [Fact]
    public async Task LoginAsync_EmailNotValidated_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com", emailValidado: false);
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (token, error) = await service.LoginAsync(new LoginDto
        {
            Email = "test@email.com",
            Senha = TestHelpers.DefaultPassword
        });

        Assert.Null(token);
        Assert.Equal("Email não validado", error);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsSuccess()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var user = TestHelpers.CreateUser("new@email.com");

        var (success, message) = await service.RegisterAsync(user);

        Assert.True(success);
        Assert.Contains("Registro realizado", message);

        var savedUser = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "new@email.com");
        Assert.NotNull(savedUser);
        Assert.NotNull(savedUser.TokenValidacaoEmail);
        Assert.NotNull(savedUser.DataExpiracaoToken);

        _emailServiceMock.Verify(
            e => e.SendEmailValidationAsync("new@email.com", user.Nome, savedUser.TokenValidacaoEmail),
            Times.Once
        );
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var existingUser = TestHelpers.CreateUser("existing@email.com");
        context.Usuarios.Add(existingUser);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var duplicateUser = TestHelpers.CreateUser("existing@email.com");

        var (success, message) = await service.RegisterAsync(duplicateUser);

        Assert.False(success);
        Assert.Equal("Email em uso", message);

        _emailServiceMock.Verify(
            e => e.SendEmailValidationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ResendValidationEmailAsync_UserFound_ReturnsSuccess()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com", emailValidado: false);
        var oldToken = "old-token";
        user.TokenValidacaoEmail = oldToken;
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (success, message) = await service.ResendValidationEmailAsync("test@email.com");

        Assert.True(success);
        Assert.Contains("reenviado", message);

        var savedUser = await context.Usuarios.FirstAsync(u => u.Email == "test@email.com");
        Assert.NotEqual(oldToken, savedUser.TokenValidacaoEmail);
        Assert.NotNull(savedUser.TokenValidacaoEmail);

        _emailServiceMock.Verify(
            e => e.SendEmailValidationAsync("test@email.com", user.Nome, savedUser.TokenValidacaoEmail),
            Times.Once
        );
    }

    [Fact]
    public async Task ResendValidationEmailAsync_UserNotFound_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var (success, message) = await service.ResendValidationEmailAsync("nonexistent@email.com");

        Assert.False(success);
        Assert.Equal("Usuário não encontrado", message);
    }

    [Fact]
    public async Task ResendValidationEmailAsync_EmailAlreadyValidated_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com", emailValidado: true);
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (success, message) = await service.ResendValidationEmailAsync("test@email.com");

        Assert.False(success);
        Assert.Equal("Email já validado", message);
    }

    [Fact]
    public async Task ValidateEmailAsync_ValidToken_ReturnsSuccess()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com", emailValidado: false);
        user.TokenValidacaoEmail = "valid-token-123";
        user.DataExpiracaoToken = DateTime.UtcNow.AddHours(24);
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (success, error) = await service.ValidateEmailAsync(new ValidaEmailDto
        {
            Email = "test@email.com",
            Token = "valid-token-123"
        });

        Assert.True(success);
        Assert.Null(error);

        var savedUser = await context.Usuarios.FirstAsync(u => u.Email == "test@email.com");
        Assert.True(savedUser.EmailValidado);
        Assert.Null(savedUser.TokenValidacaoEmail);
        Assert.Null(savedUser.DataExpiracaoToken);
    }

    [Fact]
    public async Task ValidateEmailAsync_UserNotFound_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var (success, error) = await service.ValidateEmailAsync(new ValidaEmailDto
        {
            Email = "nonexistent@email.com",
            Token = "some-token"
        });

        Assert.False(success);
        Assert.Equal("Usuário não encontrado", error);
    }

    [Fact]
    public async Task ValidateEmailAsync_EmailAlreadyValidated_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com", emailValidado: true);
        user.TokenValidacaoEmail = "some-token";
        user.DataExpiracaoToken = DateTime.UtcNow.AddHours(24);
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (success, error) = await service.ValidateEmailAsync(new ValidaEmailDto
        {
            Email = "test@email.com",
            Token = "some-token"
        });

        Assert.False(success);
        Assert.Equal("Email já validado", error);
    }

    [Fact]
    public async Task ValidateEmailAsync_InvalidToken_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com", emailValidado: false);
        user.TokenValidacaoEmail = "correct-token";
        user.DataExpiracaoToken = DateTime.UtcNow.AddHours(24);
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (success, error) = await service.ValidateEmailAsync(new ValidaEmailDto
        {
            Email = "test@email.com",
            Token = "wrong-token"
        });

        Assert.False(success);
        Assert.Equal("Token inválido", error);
    }

    [Fact]
    public async Task ValidateEmailAsync_ExpiredToken_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com", emailValidado: false);
        user.TokenValidacaoEmail = "expired-token";
        user.DataExpiracaoToken = DateTime.UtcNow.AddHours(-1);
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (success, error) = await service.ValidateEmailAsync(new ValidaEmailDto
        {
            Email = "test@email.com",
            Token = "expired-token"
        });

        Assert.False(success);
        Assert.Equal("Token expirado", error);
    }

    [Fact]
    public async Task RequestPasswordRecoveryAsync_UserFound_ReturnsSuccess()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com");
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (success, message) = await service.RequestPasswordRecoveryAsync(new RecuperaSenhaDto
        {
            Email = "test@email.com"
        });

        Assert.True(success);
        Assert.Contains("link de recuperação", message);

        var savedUser = await context.Usuarios.FirstAsync(u => u.Email == "test@email.com");
        Assert.NotNull(savedUser.TokenRecuperaSenha);
        Assert.True(savedUser.DataExpiracaoToken > DateTime.UtcNow.AddMinutes(55));

        _emailServiceMock.Verify(
            e => e.SendPasswordRecoveryAsync("test@email.com", user.Nome, savedUser.TokenRecuperaSenha),
            Times.Once
        );
    }

    [Fact]
    public async Task RequestPasswordRecoveryAsync_UserNotFound_ReturnsGenericMessage()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var (success, message) = await service.RequestPasswordRecoveryAsync(new RecuperaSenhaDto
        {
            Email = "nonexistent@email.com"
        });

        Assert.False(success);
        Assert.Equal("Se o email existir, um link de recuperação será enviado", message);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_ReturnsSuccess()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com");
        user.TokenRecuperaSenha = "recovery-token-123";
        user.DataExpiracaoToken = DateTime.UtcNow.AddHours(1);
        var oldPasswordHash = user.Senha;
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (success, error) = await service.ResetPasswordAsync(new ResetaSenhaDto
        {
            Email = "test@email.com",
            Token = "recovery-token-123",
            NovaSenha = "NewPassword@456"
        });

        Assert.True(success);
        Assert.Null(error);

        var savedUser = await context.Usuarios.FirstAsync(u => u.Email == "test@email.com");
        Assert.NotEqual(oldPasswordHash, savedUser.Senha);
        Assert.Null(savedUser.TokenRecuperaSenha);
        Assert.Null(savedUser.DataExpiracaoToken);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPassword@456", savedUser.Senha));
    }

    [Fact]
    public async Task ResetPasswordAsync_UserNotFound_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var (success, error) = await service.ResetPasswordAsync(new ResetaSenhaDto
        {
            Email = "nonexistent@email.com",
            Token = "some-token",
            NovaSenha = "NewPassword@456"
        });

        Assert.False(success);
        Assert.Equal("Usuário não encontrado", error);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidToken_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com");
        user.TokenRecuperaSenha = "correct-token";
        user.DataExpiracaoToken = DateTime.UtcNow.AddHours(1);
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (success, error) = await service.ResetPasswordAsync(new ResetaSenhaDto
        {
            Email = "test@email.com",
            Token = "wrong-token",
            NovaSenha = "NewPassword@456"
        });

        Assert.False(success);
        Assert.Equal("Token inválido", error);
    }

    [Fact]
    public async Task ResetPasswordAsync_ExpiredToken_ReturnsError()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com");
        user.TokenRecuperaSenha = "expired-token";
        user.DataExpiracaoToken = DateTime.UtcNow.AddHours(-2);
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var (success, error) = await service.ResetPasswordAsync(new ResetaSenhaDto
        {
            Email = "test@email.com",
            Token = "expired-token",
            NovaSenha = "NewPassword@456"
        });

        Assert.False(success);
        Assert.Equal("Token expirado", error);
    }

    [Fact]
    public async Task GetUserAsync_UserExists_ReturnsUser()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com");
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetUserAsync(user.Id.ToString());

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetUserAsync_UserNotFound_ReturnsNull()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var result = await service.GetUserAsync("999");

        Assert.Null(result);
    }

    [Fact]
    public async Task CheckIfUserExists_EmailExists_ReturnsTrue()
    {
        using var context = TestHelpers.CreateDbContext();
        var user = TestHelpers.CreateUser("test@email.com");
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var exists = await service.CheckIfUserExists("test@email.com");

        Assert.True(exists);
    }

    [Fact]
    public async Task CheckIfUserExists_EmailNotExists_ReturnsFalse()
    {
        using var context = TestHelpers.CreateDbContext();
        var service = CreateService(context);

        var exists = await service.CheckIfUserExists("nonexistent@email.com");

        Assert.False(exists);
    }
}
