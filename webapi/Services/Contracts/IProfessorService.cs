using ApiSgc.Models;

namespace ApiSgc.Services.Contracts;

public interface IProfessorService
{
    Task<(IEnumerable<Professor> Items, int TotalCount)> GetAllAsync(string? nome, int page, int perPage);
    Task<Professor?> GetByIdAsync(int id);
    Task<Professor> CreateAsync(Professor professor);
    Task<Professor> UpdateAsync(Professor professor);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNomeAsync(string nome, int? id = null);
    Task<bool> ExistsByEmailAsync(string? email, int? id = null);
    Task<bool> ToggleStatusAsync(int id);
}