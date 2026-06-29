using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Services;

public class AvaliacaoService : IAvaliacaoService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AvaliacaoService> _logger;

    public AvaliacaoService(ApplicationDbContext context, ILogger<AvaliacaoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<Avaliacao> Items, int TotalCount)> GetAllAsync(int? turmaId, int page, int perPage)
    {
        var query = _context.Avaliacoes
            .Include(a => a.Turma)
            .Where(a => a.DeletedAt == null);

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

    public async Task<Avaliacao?> GetByIdAsync(int id)
    {
        return await _context.Avaliacoes
            .Include(a => a.Turma)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);
    }

    public async Task<Avaliacao> CreateAsync(Avaliacao avaliacao)
    {
        try
        {
            avaliacao.CreatedAt = DateTime.UtcNow;
            await _context.Avaliacoes.AddAsync(avaliacao);
            await _context.SaveChangesAsync();
            return avaliacao;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Avaliacao.Create] Falha ao criar avaliação. TurmaId={TurmaId}, Nome={Nome}",
                avaliacao.TurmaId, avaliacao.Nome);
            throw;
        }
    }

    public async Task<Avaliacao> UpdateAsync(Avaliacao avaliacao)
    {
        try
        {
            avaliacao.UpdatedAt = DateTime.UtcNow;
            _context.Avaliacoes.Update(avaliacao);
            await _context.SaveChangesAsync();
            return avaliacao;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Avaliacao.Update] Falha ao atualizar avaliação Id={AvaliacaoId}, TurmaId={TurmaId}, Nome={Nome}",
                avaliacao.Id, avaliacao.TurmaId, avaliacao.Nome);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao == null) return false;

            avaliacao.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Avaliacao.Delete] Falha ao deletar avaliação Id={AvaliacaoId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Avaliacoes.AnyAsync(a => a.Id == id && a.DeletedAt == null);
    }

    public async Task<bool> ExistsByTurmaAsync(int turmaId)
    {
        return await _context.Turmas.AnyAsync(t => t.Id == turmaId && t.DeletedAt == null);
    }

    public async Task<bool> TurmaNecessitaAtividadesAsync(int turmaId)
    {
        var turma = await _context.Turmas
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == turmaId && t.DeletedAt == null);
        return turma?.NecessitaAtividades ?? false;
    }
}