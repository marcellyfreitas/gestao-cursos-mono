using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Services;

public class ProfessorService : IProfessorService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProfessorService> _logger;

    public ProfessorService(ApplicationDbContext context, ILogger<ProfessorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<Professor> Items, int TotalCount)> GetAllAsync(string? nome, int page, int perPage)
    {
        var query = _context.Professores.Where(p => p.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(nome))
        {
            query = query.Where(p => p.Nome.Contains(nome));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Professor?> GetByIdAsync(int id)
    {
        return await _context.Professores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);
    }

    public async Task<Professor> CreateAsync(Professor professor)
    {
        try
        {
            professor.CreatedAt = DateTime.UtcNow;
            professor.UpdatedAt = DateTime.UtcNow;
            professor.Ativo = true;

            await _context.Professores.AddAsync(professor);
            await _context.SaveChangesAsync();

            return professor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Professor.Create] Falha ao criar professor. Nome={Nome}, Email={Email}",
                professor.Nome, professor.Email);
            throw;
        }
    }

    public async Task<Professor> UpdateAsync(Professor professor)
    {
        try
        {
            professor.UpdatedAt = DateTime.UtcNow;
            _context.Professores.Update(professor);
            await _context.SaveChangesAsync();

            return professor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Professor.Update] Falha ao atualizar professor Id={ProfessorId}, Nome={Nome}",
                professor.Id, professor.Nome);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var professor = await _context.Professores.FindAsync(id);
            if (professor == null)
                return false;

            professor.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Professor.Delete] Falha ao excluir professor Id={ProfessorId}", id);
            throw;
        }
    }

    public async Task<bool> ToggleStatusAsync(int id)
    {
        try
        {
            var professor = await _context.Professores.FindAsync(id);
            if (professor == null)
                return false;

            professor.Ativo = !professor.Ativo;
            professor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("[Professor.ToggleStatus] Professor Id={ProfessorId}, Nome={Nome} → Ativo={Ativo}",
                id, professor.Nome, professor.Ativo);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Professor.ToggleStatus] Falha ao alterar status do professor Id={ProfessorId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Professores.AnyAsync(p => p.Id == id && p.DeletedAt == null);
    }

    public async Task<bool> ExistsByNomeAsync(string nome, int? id = null)
    {
        return await _context.Professores
            .AnyAsync(p => p.Nome == nome && p.DeletedAt == null && (id == null || p.Id != id));
    }

    public async Task<bool> ExistsByEmailAsync(string? email, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return await _context.Professores
            .AnyAsync(p => p.Email == email && p.DeletedAt == null && (id == null || p.Id != id));
    }
}