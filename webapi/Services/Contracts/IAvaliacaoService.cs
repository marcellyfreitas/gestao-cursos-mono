using ApiSgc.Models;

namespace ApiSgc.Services.Contracts;

public interface IAvaliacaoService
{
    Task<(IEnumerable<Avaliacao> Items, int TotalCount)> GetAllAsync(int? turmaId, int page, int perPage);
    Task<Avaliacao?> GetByIdAsync(int id);
    Task<Avaliacao> CreateAsync(Avaliacao avaliacao);
    Task<Avaliacao> UpdateAsync(Avaliacao avaliacao);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByTurmaAsync(int turmaId);
    Task<bool> TurmaNecessitaAtividadesAsync(int turmaId);
}