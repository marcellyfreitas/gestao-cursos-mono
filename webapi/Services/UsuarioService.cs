using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.DTOs;
using ApiSgc.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Services;

public class UsuarioService : IUsuarioService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(ApplicationDbContext context, ILogger<UsuarioService> logger)
    {
        _context = context;
        _logger = logger;
    }


    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        var usuarios = await _context.Usuarios
            .OrderByDescending(u => u.Id)
            .ToListAsync();
        return usuarios;
    }

    public async Task<(IEnumerable<Usuario> Items, int TotalCount)> GetAllAsync(UsuarioFiltroDto filtro)
    {
        var query = _context.Usuarios.Where(u => u.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
        {
            query = query.Where(u => u.Nome.Contains(filtro.Nome));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Email))
        {
            query = query.Where(u => u.Email.Contains(filtro.Email));
        }

        if (filtro.EstaEmCelula.HasValue)
        {
            query = query.Where(u => u.EstaEmCelula == filtro.EstaEmCelula);
        }

        if (filtro.EstaSendoDiscipulado.HasValue)
        {
            query = query.Where(u => u.EstaSendoDiscipulado == filtro.EstaSendoDiscipulado);
        }

        if (filtro.Batizado.HasValue)
        {
            query = query.Where(u => u.Batizado == filtro.Batizado);
        }

        if (filtro.Role.HasValue)
        {
            query = query.Where(u => u.Role == filtro.Role);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.Id)
            .Skip((filtro.Page - 1) * filtro.PerPage)
            .Take(filtro.PerPage)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Usuario> GetAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        return usuario!;
    }

    public async Task<Usuario> AddAsync(Usuario entity)
    {
        try
        {
            await _context.Usuarios.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Usuario.Add] Falha ao cadastrar usuário. Nome={Nome}, Email={Email}, Role={Role}",
                entity.Nome, entity.Email, entity.Role);
            throw;
        }

    }

    public Task<Usuario> UpdateAsync(Usuario entity)
    {
        try
        {
            _context.Usuarios.Update(entity);
            _context.SaveChanges();
            return Task.FromResult(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Usuario.Update] Falha ao atualizar usuário Id={UsuarioId}, Nome={Nome}, Email={Email}",
                entity.Id, entity.Nome, entity.Email);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;

            // Cascade delete: remove all matrículas and their dependents
            var matriculas = await _context.Matriculas
                .Where(m => m.UsuarioId == id)
                .ToListAsync();

            foreach (var matricula in matriculas)
            {
                var frequencias = await _context.Frequencias
                    .Where(f => f.MatriculaId == matricula.Id)
                    .ToListAsync();
                _context.Frequencias.RemoveRange(frequencias);

                var notas = await _context.Notas
                    .Where(n => n.MatriculaId == matricula.Id)
                    .ToListAsync();
                _context.Notas.RemoveRange(notas);
            }

            _context.Matriculas.RemoveRange(matriculas);
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Usuario.Delete] Falha ao deletar usuário Id={UsuarioId}", id);
            throw;
        }
    }

    public async Task<Usuario?> GetByEmailAsync(string email, int? id = null)
    {
        try
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && (id == null || u.Id != id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Usuario.GetByEmail] Falha ao buscar usuário por email. Email={Email}", email);
            throw;
        }
    }
}