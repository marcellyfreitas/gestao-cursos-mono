using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.Enums;
using ApiSgc.Utils;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Database.Seeders;

public class UsuarioSeeder : ISeeder
{
    private readonly ApplicationDbContext _context;

    public UsuarioSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Seed()
    {
        if (await _context.Usuarios.AnyAsync())
            return;

        _context.Usuarios.Add(new Usuario
        {
            Email = "fernando-moura@live.com",
            Nome = "Administrador",
            Senha = PasswordHelper.HashPassword("admin123"),
            Role = UserRole.ADMIN,
            Telefone = "11999999999",
            EmailValidado = true,
            Ativo = true
        });

        await _context.SaveChangesAsync();
    }
}
