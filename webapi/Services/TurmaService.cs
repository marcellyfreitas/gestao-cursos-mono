using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Services;

public class TurmaService : ITurmaService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TurmaService> _logger;

    public TurmaService(ApplicationDbContext context, ILogger<TurmaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<Turma> Items, int TotalCount)> GetAllAsync(int? cursoId, string? nome, int page, int perPage)
    {
        var query = _context.Turmas.Where(t => t.DeletedAt == null);

        if (cursoId.HasValue)
        {
            query = query.Where(t => t.CursoId == cursoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(nome))
        {
            query = query.Where(t => t.Nome.Contains(nome));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Include(t => t.Curso)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Turma?> GetByIdAsync(int id)
    {
        return await _context.Turmas
            .Include(t => t.Curso)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
    }

    public async Task<Turma> CreateAsync(Turma turma)
    {
        try
        {
            turma.CreatedAt = DateTime.UtcNow;
            await _context.Turmas.AddAsync(turma);
            await _context.SaveChangesAsync();
            return turma;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Turma.Create] Falha ao criar turma. Nome={Nome}, CursoId={CursoId}, NecessitaAtividades={NecessitaAtividades}, MediaMinima={MediaMinima}, FaltasParaReprovacao={FaltasParaReprovacao}",
                turma.Nome, turma.CursoId, turma.NecessitaAtividades, turma.MediaMinima, turma.FaltasParaReprovacao);
            throw;
        }
    }

    public async Task<Turma> UpdateAsync(Turma turma)
    {
        try
        {
            turma.UpdatedAt = DateTime.UtcNow;
            _context.Turmas.Update(turma);
            await _context.SaveChangesAsync();
            return turma;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Turma.Update] Falha ao atualizar turma Id={TurmaId}, Nome={Nome}, CursoId={CursoId}",
                turma.Id, turma.Nome, turma.CursoId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var turma = await _context.Turmas.FindAsync(id);
            if (turma == null) return false;

            turma.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Turma.Delete] Falha ao deletar turma Id={TurmaId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Turmas.AnyAsync(t => t.Id == id && t.DeletedAt == null);
    }

    public async Task<bool> ExistsByNomeAndCursoAsync(string nome, int cursoId, int? id = null)
    {
        return await _context.Turmas
            .AnyAsync(t => t.Nome == nome && t.CursoId == cursoId && t.DeletedAt == null && (id == null || t.Id != id));
    }

    public async Task<bool> CursoExistsAsync(int cursoId)
    {
        return await _context.Cursos.AnyAsync(c => c.Id == cursoId && c.DeletedAt == null);
    }

    public async Task<bool> PossuiMatriculasAtivasAsync(int turmaId)
    {
        return await _context.Matriculas
            .AnyAsync(m => m.TurmaId == turmaId && m.DeletedAt == null);
    }
}