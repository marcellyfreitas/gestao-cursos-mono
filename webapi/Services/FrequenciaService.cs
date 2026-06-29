using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.DTOs;
using ApiSgc.Models.Enums;
using ApiSgc.Models.ViewModels;
using ApiSgc.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Services;

public class FrequenciaService : IFrequenciaService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FrequenciaService> _logger;
    private readonly IMatriculaService _matriculaService;

    public FrequenciaService(ApplicationDbContext context, ILogger<FrequenciaService> logger, IMatriculaService matriculaService)
    {
        _context = context;
        _logger = logger;
        _matriculaService = matriculaService;
    }

    public async Task<Frequencia?> GetByIdAsync(int id)
    {
        return await _context.Frequencias
            .Include(f => f.Aula)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id && f.DeletedAt == null);
    }

    public async Task<Frequencia> CreateAsync(Frequencia frequencia)
    {
        try
        {
            frequencia.CreatedAt = DateTime.UtcNow;
            await _context.Frequencias.AddAsync(frequencia);
            await _context.SaveChangesAsync();
            return frequencia;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Frequencia.Create] Falha ao criar frequência. MatriculaId={MatriculaId}, AulaId={AulaId}, Status={Status}",
                frequencia.MatriculaId, frequencia.AulaId, frequencia.Status);
            throw;
        }
    }

    public async Task<Frequencia> UpdateAsync(Frequencia frequencia)
    {
        try
        {
            frequencia.UpdatedAt = DateTime.UtcNow;
            _context.Frequencias.Update(frequencia);
            await _context.SaveChangesAsync();
            return frequencia;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Frequencia.Update] Falha ao atualizar frequência Id={FrequenciaId}. MatriculaId={MatriculaId}, AulaId={AulaId}, Status={Status}",
                frequencia.Id, frequencia.MatriculaId, frequencia.AulaId, frequencia.Status);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var frequencia = await _context.Frequencias.FindAsync(id);
            if (frequencia == null) return false;

            frequencia.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Frequencia.Delete] Falha ao deletar frequência Id={FrequenciaId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Frequencias.AnyAsync(f => f.Id == id && f.DeletedAt == null);
    }

    public async Task<bool> ExistsByMatriculaAndAulaAsync(int matriculaId, int aulaId, int? id = null)
    {
        return await _context.Frequencias
            .AnyAsync(f => f.MatriculaId == matriculaId && f.AulaId == aulaId && f.DeletedAt == null && (id == null || f.Id != id));
    }

    public async Task<decimal> CalcularFrequenciaAsync(int matriculaId)
    {
        var matricula = await _context.Matriculas
            .Include(m => m.Turma)
            .ThenInclude(t => t.Aulas.Where(a => a.DeletedAt == null))
            .FirstOrDefaultAsync(m => m.Id == matriculaId && m.DeletedAt == null);

        if (matricula == null)
            return 0;

        var totalAulas = matricula.Turma.Aulas.Count;
        if (totalAulas == 0)
            return 100;

        var frequencias = await _context.Frequencias
            .Where(f => f.MatriculaId == matriculaId && f.DeletedAt == null)
            .ToListAsync();

        var presencas = frequencias.Count(f => f.Status == StatusFrequencia.PRESENTE);
        return (presencas * 100.0m) / totalAulas;
    }

    public async Task<IEnumerable<FrequenciaAlunoViewModel>> GetAlunosComFrequenciaAsync(int aulaId)
    {
        var aula = await _context.Aulas
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == aulaId && a.DeletedAt == null)
            ?? throw new InvalidOperationException("Aula não encontrada");

        var turmaId = aula.TurmaId;

        var turma = await _context.Turmas
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == turmaId && t.DeletedAt == null)
            ?? throw new InvalidOperationException("Turma não encontrada");

        var faltasLimite = turma.FaltasParaReprovacao > 0 ? turma.FaltasParaReprovacao : 3;

        var situacoesValidas = new[] { SituacaoMatricula.CURSANDO, SituacaoMatricula.REPROVADO_FREQUENCIA, SituacaoMatricula.APROVADO, SituacaoMatricula.REPROVADO_NOTA };
        var matriculas = await _context.Matriculas
            .Include(m => m.Usuario)
            .Where(m => m.TurmaId == turmaId && m.DeletedAt == null && situacoesValidas.Contains(m.Situacao))
            .OrderBy(m => m.Usuario!.Nome)
            .AsNoTracking()
            .ToListAsync();

        var matriculaIds = matriculas.Select(m => m.Id).ToList();

        var statusFalta = new[] { StatusFrequencia.FALTA };
        var faltasPorMatricula = await _context.Frequencias
            .Where(f => matriculaIds.Contains(f.MatriculaId) && f.DeletedAt == null && statusFalta.Contains(f.Status))
            .GroupBy(f => f.MatriculaId)
            .Select(g => new { MatriculaId = g.Key, Total = g.Count() })
            .AsNoTracking()
            .ToListAsync();

        var faltasMap = faltasPorMatricula.ToDictionary(x => x.MatriculaId, x => x.Total);

        var frequencias = await _context.Frequencias
            .Where(f => f.AulaId == aula.Id && f.DeletedAt == null)
            .AsNoTracking()
            .ToListAsync();

        return matriculas.Select(m =>
        {
            var freq = frequencias.FirstOrDefault(f => f.MatriculaId == m.Id);
            var totalFaltas = faltasMap.GetValueOrDefault(m.Id, 0);
            return new FrequenciaAlunoViewModel
            {
                MatriculaId = m.Id,
                AlunoId = m.UsuarioId,
                AlunoNome = m.Usuario?.Nome ?? string.Empty,
                Status = freq?.Status,
                AulaId = aula.Id,
                TotalFaltas = totalFaltas,
                Reprovado = totalFaltas > faltasLimite,
            };
        });
    }

    public async Task SalvarFrequenciaLoteAsync(SalvarFrequenciaLoteDto dto)
    {
        var aula = await _context.Aulas
            .FirstOrDefaultAsync(a => a.Id == dto.AulaId && a.DeletedAt == null)
            ?? throw new InvalidOperationException("Aula não encontrada");

        var turma = await _context.Turmas
            .FirstOrDefaultAsync(t => t.Id == aula.TurmaId && t.DeletedAt == null)
            ?? throw new InvalidOperationException("Turma não encontrada");

        _logger.LogInformation("[Frequencia.SalvarLote] Iniciando lote. AulaId={AulaId}, TurmaId={TurmaId}, Turma={TurmaNome}, FaltasParaReprovacao={FaltasLimite}, QtdItems={QtdItems}",
            dto.AulaId, turma.Id, turma.Nome, turma.FaltasParaReprovacao, dto.Items.Count);

        try
        {
            foreach (var item in dto.Items)
            {
                var frequencia = await _context.Frequencias
                    .FirstOrDefaultAsync(f => f.MatriculaId == item.MatriculaId && f.AulaId == aula.Id && f.DeletedAt == null);

                if (frequencia == null)
                {
                    frequencia = new Frequencia
                    {
                        MatriculaId = item.MatriculaId,
                        AulaId = aula.Id,
                        Status = item.Status,
                        CreatedAt = DateTime.UtcNow,
                    };
                    _context.Frequencias.Add(frequencia);
                }
                else
                {
                    frequencia.Status = item.Status;
                    frequencia.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            // Recalcular aprovação para cada matrícula afetada usando a lógica unificada
            var matriculaIds = dto.Items.Select(i => i.MatriculaId).Distinct().ToList();

            _logger.LogInformation("[Frequencia.SalvarLote] Frequências salvas. Recalculando aprovação para {QtdMatriculas} matrículas...", matriculaIds.Count);

            foreach (var matriculaId in matriculaIds)
            {
                try
                {
                    await _matriculaService.CalcularAprovacaoAsync(matriculaId);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "[Frequencia.SalvarLote] Não foi possível calcular aprovação para MatriculaId={MatriculaId}", matriculaId);
                }
            }

            _logger.LogInformation("[Frequencia.SalvarLote] Lote concluído com sucesso. AulaId={AulaId}, MatriculasProcessadas={QtdMatriculas}",
                dto.AulaId, matriculaIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Frequencia.SalvarLote] Falha ao salvar lote de frequência. AulaId={AulaId}, TurmaId={TurmaId}, QtdItems={QtdItems}",
                dto.AulaId, turma.Id, dto.Items.Count);
            throw;
        }
    }
}
