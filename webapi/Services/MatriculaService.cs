using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.Enums;
using ApiSgc.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Services;

public class MatriculaService : IMatriculaService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MatriculaService> _logger;

    public MatriculaService(
        ApplicationDbContext context, 
        ILogger<MatriculaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<Matricula> Items, int TotalCount)> GetAllAsync(int? turmaId, int? alunoId, int page, int perPage)
    {
        var query = _context.Matriculas
            .Include(m => m.Usuario)
            .Include(m => m.Turma).ThenInclude(t => t.Curso)
            .Where(m => m.DeletedAt == null);

        if (turmaId.HasValue)
        {
            query = query.Where(m => m.TurmaId == turmaId.Value);
        }

        if (alunoId.HasValue)
        {
            query = query.Where(m => m.UsuarioId == alunoId.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(m => m.Id)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Matricula?> GetByIdAsync(int id)
    {
        return await _context.Matriculas
            .Include(m => m.Usuario)
            .Include(m => m.Turma).ThenInclude(t => t.Curso)
            .Include(m => m.Frequencias)
            .Include(m => m.Notas)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null);
    }

    public async Task<Matricula> CreateAsync(Matricula matricula)
    {
        try
        {
            var softDeletedMatricula = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.UsuarioId == matricula.UsuarioId && m.TurmaId == matricula.TurmaId && m.DeletedAt != null);

            if (softDeletedMatricula != null)
            {
                _logger.LogInformation("[Matricula.Create] Reativando matrícula soft-deleted Id={MatriculaId} para UsuarioId={UsuarioId}, TurmaId={TurmaId}",
                    softDeletedMatricula.Id, matricula.UsuarioId, matricula.TurmaId);

                softDeletedMatricula.Situacao = matricula.Situacao;
                softDeletedMatricula.DataMatricula = DateTime.UtcNow;
                softDeletedMatricula.DeletedAt = null;
                softDeletedMatricula.UpdatedAt = DateTime.UtcNow;

                // Hard-delete old frequências and notas so student starts fresh
                var oldFrequencias = await _context.Frequencias
                    .Where(f => f.MatriculaId == softDeletedMatricula.Id)
                    .ToListAsync();
                _context.Frequencias.RemoveRange(oldFrequencias);

                var oldNotas = await _context.Notas
                    .Where(n => n.MatriculaId == softDeletedMatricula.Id)
                    .ToListAsync();
                _context.Notas.RemoveRange(oldNotas);

                await _context.SaveChangesAsync();
                return softDeletedMatricula;
            }

            matricula.DataMatricula = DateTime.UtcNow;
            matricula.CreatedAt = DateTime.UtcNow;
            await _context.Matriculas.AddAsync(matricula);
            await _context.SaveChangesAsync();
            return matricula;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Matricula.Create] Falha ao criar matrícula. UsuarioId={UsuarioId}, TurmaId={TurmaId}, Situacao={Situacao}",
                matricula.UsuarioId, matricula.TurmaId, matricula.Situacao);
            throw;
        }
    }

    public async Task<Matricula> UpdateAsync(Matricula matricula)
    {
        try
        {
            matricula.UpdatedAt = DateTime.UtcNow;

            matricula.Usuario = null!;
            matricula.Turma = null!;
            matricula.Frequencias = new List<Frequencia>();
            matricula.Notas = new List<Nota>();

            _context.Matriculas.Update(matricula);
            await _context.SaveChangesAsync();
            return matricula;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Matricula.Update] Falha ao atualizar matrícula Id={MatriculaId}. UsuarioId={UsuarioId}, TurmaId={TurmaId}, Situacao={Situacao}",
                matricula.Id, matricula.UsuarioId, matricula.TurmaId, matricula.Situacao);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null) return false;

            matricula.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Matricula.Delete] Falha ao deletar matrícula Id={MatriculaId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Matriculas.AnyAsync(m => m.Id == id && m.DeletedAt == null);
    }

    public async Task<bool> ExistsByAlunoAndTurmaAsync(int alunoId, int turmaId, int? id = null)
    {
        return await _context.Matriculas
            .AnyAsync(m => m.UsuarioId == alunoId && m.TurmaId == turmaId && m.DeletedAt == null && (id == null || m.Id != id));
    }

    public async Task<Matricula> CalcularAprovacaoAsync(int id)
    {
        var matricula = await _context.Matriculas
            .Include(m => m.Turma)
            .Include(m => m.Frequencias)
            .Include(m => m.Notas).ThenInclude(n => n.Avaliacao)
            .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null);

        if (matricula == null)
            throw new InvalidOperationException("Matrícula não encontrada");

        var turma = matricula.Turma;

        // Contar aulas da turma para verificar se há frequência pendente
        var totalAulasTurma = await _context.Aulas
            .CountAsync(a => a.TurmaId == turma.Id && a.DeletedAt == null);

        var frequenciasLancadas = matricula.Frequencias
            .Count(f => f.DeletedAt == null);

        var totalFaltas = matricula.Frequencias
            .Count(f => f.Status == StatusFrequencia.FALTA && f.DeletedAt == null);

        // Frequência incompleta: existem aulas sem registro de frequência
        var frequenciaIncompleta = totalAulasTurma > 0 && frequenciasLancadas < totalAulasTurma;

        var frequenciaSuficiente = totalFaltas <= turma.FaltasParaReprovacao;

        _logger.LogInformation("[Matricula.CalcularAprovacao] MatriculaId={MatriculaId}, TurmaId={TurmaId}, Turma={TurmaNome}, NecessitaAtividades={NecessitaAtividades}, FaltasParaReprovacao={FaltasLimite}, TotalFaltas={TotalFaltas}, FrequenciaSuficiente={FreqOk}, TotalAulas={TotalAulas}, FrequenciasLancadas={FreqLancadas}, FrequenciaIncompleta={FreqIncompleta}",
            id, turma.Id, turma.Nome, turma.NecessitaAtividades, turma.FaltasParaReprovacao, totalFaltas, frequenciaSuficiente, totalAulasTurma, frequenciasLancadas, frequenciaIncompleta);

        if (!turma.NecessitaAtividades)
        {
            // Turma sem atividades: aprovação baseada apenas em frequência
            if (frequenciaIncompleta)
            {
                // Reprovação imediata se ultrapassou o limite de faltas
                if (!frequenciaSuficiente)
                {
                    _logger.LogInformation("[Matricula.CalcularAprovacao] Turma sem atividades, faltas excedem limite. MatriculaId={MatriculaId} → REPROVADO_FREQUENCIA", id);
                    matricula.Situacao = SituacaoMatricula.REPROVADO_FREQUENCIA;
                    matricula.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return matricula;
                }

                _logger.LogInformation("[Matricula.CalcularAprovacao] Turma sem atividades, frequência incompleta. MatriculaId={MatriculaId} → mantém CURSANDO", id);
                matricula.Situacao = SituacaoMatricula.CURSANDO;
                matricula.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return matricula;
            }

            // Todas as aulas têm frequência: decidir aprovação final
            matricula.Situacao = frequenciaSuficiente
                ? SituacaoMatricula.APROVADO
                : SituacaoMatricula.REPROVADO_FREQUENCIA;

            _logger.LogInformation("[Matricula.CalcularAprovacao] Turma sem atividades, todas as aulas com frequência. MatriculaId={MatriculaId} → Situacao={Situacao}",
                id, matricula.Situacao);

            matricula.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return matricula;
        }

        // Turma com atividades: precisa de notas E frequência

        // Reprovação por frequência tem prioridade sobre notas incompletas
        if (frequenciasLancadas > 0 && !frequenciaSuficiente)
        {
            _logger.LogInformation("[Matricula.CalcularAprovacao] Faltas excedem limite (prioridade sobre notas). MatriculaId={MatriculaId} → REPROVADO_FREQUENCIA", id);
            matricula.Situacao = SituacaoMatricula.REPROVADO_FREQUENCIA;
            matricula.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return matricula;
        }

        var notas = matricula.Notas.Where(n => n.DeletedAt == null).ToList();
        if (!notas.Any())
        {
            _logger.LogInformation("[Matricula.CalcularAprovacao] Sem notas lançadas. MatriculaId={MatriculaId} → mantém CURSANDO", id);
            matricula.Situacao = SituacaoMatricula.CURSANDO;
            matricula.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return matricula;
        }

        var totalAvaliacoes = await _context.Avaliacoes
            .CountAsync(a => a.TurmaId == turma.Id && a.DeletedAt == null);

        if (notas.Count < totalAvaliacoes)
        {
            _logger.LogInformation("[Matricula.CalcularAprovacao] Notas incompletas ({NotasCount}/{TotalAvaliacoes}). MatriculaId={MatriculaId} → mantém CURSANDO",
                notas.Count, totalAvaliacoes, id);
            matricula.Situacao = SituacaoMatricula.CURSANDO;
            matricula.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return matricula;
        }

        // Se há aulas mas nenhuma frequência lançada, não pode aprovar mesmo com notas completas
        if (totalAulasTurma > 0 && frequenciasLancadas == 0)
        {
            _logger.LogInformation("[Matricula.CalcularAprovacao] Notas completas mas frequência pendente. MatriculaId={MatriculaId} → mantém CURSANDO", id);
            matricula.Situacao = SituacaoMatricula.CURSANDO;
            matricula.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return matricula;
        }

        var media = notas.Average(n => n.Valor);
        var mediaSuficiente = media >= (turma.MediaMinima ?? 0);

        _logger.LogInformation("[Matricula.CalcularAprovacao] MatriculaId={MatriculaId}, Media={Media:F2}, MediaMinima={MediaMinima}, MediaSuficiente={MediaOk}, FrequenciaSuficiente={FreqOk}",
            id, media, turma.MediaMinima ?? 0, mediaSuficiente, frequenciaSuficiente);

        if (!frequenciaSuficiente)
        {
            matricula.Situacao = SituacaoMatricula.REPROVADO_FREQUENCIA;
        }
        else if (!mediaSuficiente)
        {
            matricula.Situacao = SituacaoMatricula.REPROVADO_NOTA;
        }
        else
        {
            matricula.Situacao = SituacaoMatricula.APROVADO;
        }

        _logger.LogInformation("[Matricula.CalcularAprovacao] MatriculaId={MatriculaId} → Situacao={Situacao}", id, matricula.Situacao);

        matricula.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return matricula;
    }
}
