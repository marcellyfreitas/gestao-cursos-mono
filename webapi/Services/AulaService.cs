using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.Enums;
using ApiSgc.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Services;

public class AulaService : IAulaService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AulaService> _logger;

    public AulaService(ApplicationDbContext context, ILogger<AulaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<Aula> Items, int TotalCount)> GetAllAsync(string? titulo, int? turmaId, int page, int perPage)
    {
        var query = _context.Aulas
            .Include(a => a.Professor) 
            .Include(a => a.Turma)
            .Where(a => a.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(titulo))
        {
            query = query.Where(a => a.Titulo.Contains(titulo));
        }

        if (turmaId.HasValue)
        {
            query = query.Where(a => a.TurmaId == turmaId.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Aula?> GetByIdAsync(int id)
    {
        return await _context.Aulas
            .Include(a => a.Turma)
            .Include(a => a.Professor)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);
    }

    public async Task<Aula> CreateAsync(Aula aula)
    {
        try
        {
            aula.CreatedAt = DateTime.UtcNow;
            await _context.Aulas.AddAsync(aula);
            await _context.SaveChangesAsync();
            return aula;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Aula.Create] Falha ao criar aula. TurmaId={TurmaId}, Titulo={Titulo}, ProfessorId={ProfessorId}, DataAula={DataAula}",
                aula.TurmaId, aula.Titulo, aula.ProfessorId, aula.DataAula);
            throw;
        }
    }

    public async Task<Aula> UpdateAsync(Aula aula)
    {
        try
        {
            aula.UpdatedAt = DateTime.UtcNow;
            _context.Aulas.Update(aula);
            await _context.SaveChangesAsync();
            return aula;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Aula.Update] Falha ao atualizar aula Id={AulaId}, TurmaId={TurmaId}, Titulo={Titulo}",
                aula.Id, aula.TurmaId, aula.Titulo);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var aula = await _context.Aulas.FindAsync(id);
            if (aula == null) return false;

            aula.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Aula.Delete] Falha ao deletar aula Id={AulaId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Aulas.AnyAsync(a => a.Id == id && a.DeletedAt == null);
    }

    public async Task<bool> ExistsByTurmaAsync(int turmaId)
    {
        return await _context.Turmas.AnyAsync(t => t.Id == turmaId && t.DeletedAt == null);
    }

    public async Task<bool> ExistsByProfessorAsync(int professorId)
    {
        return await _context.Professores.AnyAsync(p => p.Id == professorId && p.Ativo && p.DeletedAt == null);
    }
}