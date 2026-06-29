using ApiSgc.Database;
using ApiSgc.Models;
using ApiSgc.Models.DTOs;
using ApiSgc.Models.Enums;
using ApiSgc.Models.ViewModels;
using ApiSgc.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Services;

public class NotaService : INotaService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotaService> _logger;
    private readonly IMatriculaService _matriculaService;

    public NotaService(
        ApplicationDbContext context,
        ILogger<NotaService> logger,
        IMatriculaService matriculaService)
    {
        _context = context;
        _logger = logger;
        _matriculaService = matriculaService;
    }

    public async Task<Nota?> GetByIdAsync(int id)
    {
        return await _context.Notas
            .Include(n => n.Avaliacao)
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAt == null);
    }

    public async Task<Nota> CreateAsync(Nota nota)
    {
        try
        {
            nota.CreatedAt = DateTime.UtcNow;
            await _context.Notas.AddAsync(nota);
            await _context.SaveChangesAsync();
            return nota;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Nota.Create] Falha ao criar nota. MatriculaId={MatriculaId}, AvaliacaoId={AvaliacaoId}, Valor={Valor}",
                nota.MatriculaId, nota.AvaliacaoId, nota.Valor);
            throw;
        }
    }

    public async Task<Nota> UpdateAsync(Nota nota)
    {
        try
        {
            nota.UpdatedAt = DateTime.UtcNow;
            _context.Notas.Update(nota);
            await _context.SaveChangesAsync();
            return nota;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Nota.Update] Falha ao atualizar nota Id={NotaId}. MatriculaId={MatriculaId}, AvaliacaoId={AvaliacaoId}, Valor={Valor}",
                nota.Id, nota.MatriculaId, nota.AvaliacaoId, nota.Valor);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var nota = await _context.Notas.FindAsync(id);
            if (nota == null) return false;

            nota.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Nota.Delete] Falha ao deletar nota Id={NotaId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Notas.AnyAsync(n => n.Id == id && n.DeletedAt == null);
    }

    public async Task<bool> ExistsByMatriculaAndAvaliacaoAsync(int matriculaId, int avaliacaoId, int? id = null)
    {
        return await _context.Notas
            .AnyAsync(n => n.MatriculaId == matriculaId && n.AvaliacaoId == avaliacaoId && n.DeletedAt == null && (id == null || n.Id != id));
    }

    public async Task<decimal> CalcularMediaPonderadaAsync(int matriculaId)
    {
        var notas = await _context.Notas
            .Where(n => n.MatriculaId == matriculaId && n.DeletedAt == null)
            .ToListAsync();

        if (!notas.Any())
            return 0;

        return notas.Average(n => n.Valor);
    }

    public async Task<IEnumerable<NotaAlunoViewModel>> GetAlunosComNotasAsync(int turmaId, int avaliacaoId)
    {
        var turma = await _context.Turmas
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == turmaId && t.DeletedAt == null)
            ?? throw new InvalidOperationException("Turma não encontrada");

        var situacoesValidas = new[] { SituacaoMatricula.CURSANDO, SituacaoMatricula.APROVADO, SituacaoMatricula.REPROVADO_NOTA };
        var matriculas = await _context.Matriculas
            .Include(m => m.Usuario)
            .Where(m => m.TurmaId == turmaId && m.DeletedAt == null && situacoesValidas.Contains(m.Situacao))
            .OrderBy(m => m.Usuario!.Nome)
            .AsNoTracking()
            .ToListAsync();

        var matriculaIds = matriculas.Select(m => m.Id).ToList();

        var notasExistentes = await _context.Notas
            .Where(n => matriculaIds.Contains(n.MatriculaId) && n.AvaliacaoId == avaliacaoId && n.DeletedAt == null)
            .AsNoTracking()
            .ToListAsync();

        var notasMap = notasExistentes.ToDictionary(n => n.MatriculaId);

        return matriculas.Select(m =>
        {
            var nota = notasMap.GetValueOrDefault(m.Id);
            return new NotaAlunoViewModel
            {
                MatriculaId = m.Id,
                AlunoId = m.UsuarioId,
                AlunoNome = m.Usuario?.Nome ?? string.Empty,
                NotaId = nota?.Id,
                Valor = nota?.Valor,
                AvaliacaoId = avaliacaoId,
                Situacao = m.Situacao.ToString(),
                Reprovado = m.Situacao == SituacaoMatricula.REPROVADO_NOTA,
            };
        });
    }

    public async Task SalvarNotasLoteAsync(SalvarNotasLoteDto dto)
    {
        var avaliacao = await _context.Avaliacoes
            .FirstOrDefaultAsync(a => a.Id == dto.AvaliacaoId && a.DeletedAt == null)
            ?? throw new InvalidOperationException("Avaliação não encontrada");

        _logger.LogInformation("[Nota.SalvarLote] Iniciando lote. AvaliacaoId={AvaliacaoId}, Avaliacao={AvaliacaoNome}, TurmaId={TurmaId}, QtdItems={QtdItems}",
            dto.AvaliacaoId, avaliacao.Nome, avaliacao.TurmaId, dto.Items.Count);

        try
        {
            foreach (var item in dto.Items)
            {
                var nota = await _context.Notas
                    .FirstOrDefaultAsync(n => n.MatriculaId == item.MatriculaId && n.AvaliacaoId == dto.AvaliacaoId && n.DeletedAt == null);

                if (nota == null)
                {
                    nota = new Nota
                    {
                        MatriculaId = item.MatriculaId,
                        AvaliacaoId = dto.AvaliacaoId,
                        Valor = item.Valor,
                        CreatedAt = DateTime.UtcNow,
                    };
                    _context.Notas.Add(nota);
                }
                else
                {
                    nota.Valor = item.Valor;
                    nota.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            var matriculaIds = dto.Items.Select(i => i.MatriculaId).Distinct().ToList();

            _logger.LogInformation("[Nota.SalvarLote] Notas salvas. Calculando aprovação para {QtdMatriculas} matrículas...", matriculaIds.Count);

            foreach (var matriculaId in matriculaIds)
            {
                await _matriculaService.CalcularAprovacaoAsync(matriculaId);
            }

            _logger.LogInformation("[Nota.SalvarLote] Lote concluído com sucesso. AvaliacaoId={AvaliacaoId}", dto.AvaliacaoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Nota.SalvarLote] Falha ao salvar lote de notas. AvaliacaoId={AvaliacaoId}, TurmaId={TurmaId}, QtdItems={QtdItems}",
                dto.AvaliacaoId, avaliacao.TurmaId, dto.Items.Count);
            throw;
        }
    }
}
