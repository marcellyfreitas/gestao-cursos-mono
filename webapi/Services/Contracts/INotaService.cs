using ApiSgc.Models;
using ApiSgc.Models.DTOs;
using ApiSgc.Models.ViewModels;

namespace ApiSgc.Services.Contracts;

public interface INotaService
{
    Task<Nota?> GetByIdAsync(int id);
    Task<Nota> CreateAsync(Nota nota);
    Task<Nota> UpdateAsync(Nota nota);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByMatriculaAndAvaliacaoAsync(int matriculaId, int avaliacaoId, int? id = null);
    Task<decimal> CalcularMediaPonderadaAsync(int matriculaId);
    Task<IEnumerable<NotaAlunoViewModel>> GetAlunosComNotasAsync(int turmaId, int avaliacaoId);
    Task SalvarNotasLoteAsync(SalvarNotasLoteDto dto);
}
