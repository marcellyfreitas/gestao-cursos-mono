using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Services;

public class CursoService : ICursoService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CursoService> _logger;

    public CursoService(ApplicationDbContext context, ILogger<CursoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<Curso> Items, int TotalCount)> GetAllAsync(string? nome, int page, int perPage)
    {
        var query = _context.Cursos.Where(c => c.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(nome))
        {
            query = query.Where(c => c.Nome.Contains(nome));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Curso?> GetByIdAsync(int id)
    {
        return await _context.Cursos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
    }

    public async Task<Curso> CreateAsync(Curso curso)
    {
        try
        {
            curso.CreatedAt = DateTime.UtcNow;
            await _context.Cursos.AddAsync(curso);
            await _context.SaveChangesAsync();
            return curso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Curso.Create] Falha ao criar curso. Nome={Nome}", curso.Nome);
            throw;
        }
    }

    public async Task<Curso> UpdateAsync(Curso curso)
    {
        try
        {
            curso.UpdatedAt = DateTime.UtcNow;
            _context.Cursos.Update(curso);
            await _context.SaveChangesAsync();
            return curso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Curso.Update] Falha ao atualizar curso Id={CursoId}, Nome={Nome}", curso.Id, curso.Nome);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return false;

            curso.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Curso.Delete] Falha ao deletar curso Id={CursoId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Cursos.AnyAsync(c => c.Id == id && c.DeletedAt == null);
    }

    public async Task<bool> ExistsByNomeAsync(string nome, int? id = null)
    {
        return await _context.Cursos
            .AnyAsync(c => c.Nome == nome && c.DeletedAt == null && (id == null || c.Id != id));
    }
}