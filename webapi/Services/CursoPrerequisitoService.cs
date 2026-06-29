using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Services;

public class CursoPrerequisitoService : ICursoPrerequisitoService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CursoPrerequisitoService> _logger;

    public CursoPrerequisitoService(ApplicationDbContext context, ILogger<CursoPrerequisitoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CursoPrerequisito>> GetByCursoIdAsync(int cursoId)
    {
        return await _context.CursoPrerequisitos
            .Include(cp => cp.PrerequisitoCurso)
            .Where(cp => cp.CursoId == cursoId && cp.DeletedAt == null)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CursoPrerequisito> CreateAsync(CursoPrerequisito cursoPrerequisito)
    {
        try
        {
            cursoPrerequisito.CreatedAt = DateTime.UtcNow;
            await _context.CursoPrerequisitos.AddAsync(cursoPrerequisito);
            await _context.SaveChangesAsync();
            return cursoPrerequisito;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CursoPrerequisito.Create] Falha ao criar pré-requisito. CursoId={CursoId}, PrerequisitoId={PrerequisitoId}",
                cursoPrerequisito.CursoId, cursoPrerequisito.CursoPrerequisitoId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var prereq = await _context.CursoPrerequisitos.FindAsync(id);
            if (prereq == null) return false;

            prereq.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CursoPrerequisito.Delete] Falha ao deletar pré-requisito Id={PrereqId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.CursoPrerequisitos.AnyAsync(cp => cp.Id == id && cp.DeletedAt == null);
    }

    public async Task<bool> ExistsByCursoAndPrerequisitoAsync(int cursoId, int prerequisitoId, int? id = null)
    {
        return await _context.CursoPrerequisitos
            .AnyAsync(cp => cp.CursoId == cursoId && cp.CursoPrerequisitoId == prerequisitoId && cp.DeletedAt == null && (id == null || cp.Id != id));
    }

    public async Task<bool> IsCircularReferenceAsync(int cursoId, int prerequisitoId)
    {
        if (cursoId == prerequisitoId)
            return true;

        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(prerequisitoId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == cursoId)
                return true;

            if (visited.Contains(current))
                continue;

            visited.Add(current);

            var prereqs = await _context.CursoPrerequisitos
                .Where(cp => cp.CursoId == current && cp.DeletedAt == null)
                .Select(cp => cp.CursoPrerequisitoId)
                .ToListAsync();

            foreach (var prereq in prereqs)
            {
                queue.Enqueue(prereq);
            }
        }

        return false;
    }
}