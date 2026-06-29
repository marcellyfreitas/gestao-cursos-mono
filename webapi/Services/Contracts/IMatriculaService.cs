using ApiSgc.Models;
using ApiSgc.Models.DTOs;
using ApiSgc.Models.Enums;

namespace ApiSgc.Services.Contracts;

public interface IMatriculaService
{
    Task<(IEnumerable<Matricula> Items, int TotalCount)> GetAllAsync(int? turmaId, int? alunoId, int page, int perPage);
    Task<Matricula?> GetByIdAsync(int id);
    Task<Matricula> CreateAsync(Matricula matricula);
    Task<Matricula> UpdateAsync(Matricula matricula);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByAlunoAndTurmaAsync(int alunoId, int turmaId, int? id = null);
    Task<Matricula> CalcularAprovacaoAsync(int id);
}