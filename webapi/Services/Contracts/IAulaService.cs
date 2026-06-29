using ApiSgc.Models;

namespace ApiSgc.Services.Contracts;

public interface IAulaService
{
    Task<(IEnumerable<Aula> Items, int TotalCount)> GetAllAsync(string? titulo, int? turmaId, int page, int perPage);
    Task<Aula?> GetByIdAsync(int id);
    Task<Aula> CreateAsync(Aula aula);
    Task<Aula> UpdateAsync(Aula aula);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByTurmaAsync(int turmaId);
    Task<bool> ExistsByProfessorAsync(int professorId);
}