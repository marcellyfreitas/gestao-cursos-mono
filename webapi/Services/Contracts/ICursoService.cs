using ApiSgc.Models;

namespace ApiSgc.Services.Contracts;

public interface ICursoService
{
    Task<(IEnumerable<Curso> Items, int TotalCount)> GetAllAsync(string? nome, int page, int perPage);
    Task<Curso?> GetByIdAsync(int id);
    Task<Curso> CreateAsync(Curso curso);
    Task<Curso> UpdateAsync(Curso curso);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNomeAsync(string nome, int? id = null);
}