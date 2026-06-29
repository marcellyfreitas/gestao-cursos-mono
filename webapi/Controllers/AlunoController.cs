using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ApiSgc.Database;
using ApiSgc.Models.Enums;
using ApiSgc.Models.ViewModels;
using ApiSgc.Utils;
using Microsoft.EntityFrameworkCore;

namespace ApiSgc.Controllers.Private;

[ApiController]
[Route("api/v1/aluno")]
[Authorize]
public class AlunoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AlunoController> _logger;

    public AlunoController(ApplicationDbContext context, ILogger<AlunoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int? GetUsuarioId()
    {
        var value = User.Claims.FirstOrDefault(c =>
            c.Type == JwtRegisteredClaimNames.Sub ||
            c.Type == ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(value, out var id) ? id : null;
    }

    [HttpGet("turmas")]
    public async Task<IActionResult> GetMinhasTurmas()
    {
        try
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null)
                return StatusCode(401, ApiHelper.Unauthorized("Token inválido"));

            var matriculas = await _context.Matriculas
                .Include(m => m.Turma).ThenInclude(t => t.Curso)
                .Include(m => m.Turma).ThenInclude(t => t.Aulas.Where(a => a.DeletedAt == null))
                .Include(m => m.Turma).ThenInclude(t => t.Avaliacoes.Where(a => a.DeletedAt == null))
                .Include(m => m.Frequencias.Where(f => f.DeletedAt == null))
                .Include(m => m.Notas.Where(n => n.DeletedAt == null))
                    .ThenInclude(n => n.Avaliacao)
                .Where(m => m.UsuarioId == usuarioId && m.DeletedAt == null)
                .AsNoTracking()
                .ToListAsync();

            var result = matriculas.Select(m =>
            {
                var totalAulas = m.Turma.Aulas.Count;
                var presencas = m.Frequencias.Count(f => f.Status == StatusFrequencia.PRESENTE);
                var faltas = m.Frequencias.Count(f => f.Status == StatusFrequencia.FALTA);

                var notas = m.Turma.Avaliacoes.Select(a =>
                {
                    var nota = m.Notas.FirstOrDefault(n => n.AvaliacaoId == a.Id);
                    return new AlunoNotaViewModel
                    {
                        AvaliacaoId = a.Id,
                        NomeAvaliacao = a.Nome,
                        Valor = nota?.Valor,
                    };
                }).ToList();

                return new AlunoTurmaViewModel
                {
                    MatriculaId = m.Id,
                    TurmaId = m.TurmaId,
                    NomeTurma = m.Turma.Nome,
                    NomeCurso = m.Turma.Curso?.Nome ?? string.Empty,
                    Situacao = m.Situacao.ToString(),
                    NecessitaAtividades = m.Turma.NecessitaAtividades,
                    FaltasParaReprovacao = m.Turma.FaltasParaReprovacao,
                    TotalAulas = totalAulas,
                    TotalPresencas = presencas,
                    TotalFaltas = faltas,
                    Notas = notas,
                };
            }).ToList();

            return StatusCode(200, ApiHelper.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }
}