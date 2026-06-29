using ApiSgc.Models;

namespace ApiSgc.Services.Contracts;

public interface ITurmaService
{
    Task<(IEnumerable<Turma> Items, int TotalCount)> GetAllAsync(int? cursoId, string? nome, int page, int perPage);
    Task<Turma?> GetByIdAsync(int id);
    Task<Turma> CreateAsync(Turma turma);
    Task<Turma> UpdateAsync(Turma turma);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNomeAndCursoAsync(string nome, int cursoId, int? id = null);
    Task<bool> CursoExistsAsync(int cursoId);
    Task<bool> PossuiMatriculasAtivasAsync(int turmaId);
}