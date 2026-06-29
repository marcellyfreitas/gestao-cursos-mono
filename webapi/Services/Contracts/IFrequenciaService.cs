using ApiSgc.Models;
using ApiSgc.Models.DTOs;
using ApiSgc.Models.Enums;
using ApiSgc.Models.ViewModels;

namespace ApiSgc.Services.Contracts;

public interface IFrequenciaService
{
    Task<Frequencia?> GetByIdAsync(int id);
    Task<Frequencia> CreateAsync(Frequencia frequencia);
    Task<Frequencia> UpdateAsync(Frequencia frequencia);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByMatriculaAndAulaAsync(int matriculaId, int aulaId, int? id = null);
    Task<decimal> CalcularFrequenciaAsync(int matriculaId);
    Task<IEnumerable<FrequenciaAlunoViewModel>> GetAlunosComFrequenciaAsync(int aulaId);
    Task SalvarFrequenciaLoteAsync(SalvarFrequenciaLoteDto dto);
}