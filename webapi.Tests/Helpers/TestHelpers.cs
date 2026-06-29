using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.Enums;

namespace ApiSgc.Tests.Helpers;

public static class TestHelpers
{
    public const string SecretKey = "ChaveSuperSecretaParaTestesCom32Chars!";
    public const string Issuer = "TestIssuer";
    public const string Audience = "TestAudience";
    public const string DefaultPassword = "Test@123";

    public static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, Mock.Of<IConfiguration>());
    }

    public static IConfiguration MockConfiguration()
    {
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(s => s["SecretKey"]).Returns(SecretKey);
        jwtSection.Setup(s => s["Issuer"]).Returns(Issuer);
        jwtSection.Setup(s => s["Audience"]).Returns(Audience);
        jwtSection.Setup(s => s["Key"]).Returns("JwtSettings");

        var config = new Mock<IConfiguration>();
        config.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSection.Object);

        return config.Object;
    }

    public static Usuario CreateUser(
        string email,
        UserRole role = UserRole.ALUNO,
        bool emailValidado = true,
        string? customPassword = null)
    {
        var password = customPassword ?? DefaultPassword;
        return new Usuario
        {
            Nome = $"User {email}",
            Email = email,
            Senha = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            EmailValidado = emailValidado
        };
    }
}
