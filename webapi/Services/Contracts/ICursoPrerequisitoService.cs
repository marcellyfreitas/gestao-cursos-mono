using ApiSgc.Models;

namespace ApiSgc.Services.Contracts;

public interface ICursoPrerequisitoService
{
    Task<IEnumerable<CursoPrerequisito>> GetByCursoIdAsync(int cursoId);
    Task<CursoPrerequisito> CreateAsync(CursoPrerequisito cursoPrerequisito);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByCursoAndPrerequisitoAsync(int cursoId, int prerequisitoId, int? id = null);
    Task<bool> IsCircularReferenceAsync(int cursoId, int prerequisitoId);
}