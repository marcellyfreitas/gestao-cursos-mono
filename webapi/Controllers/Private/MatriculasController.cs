using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiSgc.Models.ViewModels;
using ApiSgc.Services.Contracts;
using ApiSgc.Utils;
using ApiSgc.Models.Extensions;
using ApiSgc.Models.DTOs;
using ApiSgc.Models;
using ApiSgc.Models.Enums;

namespace ApiSgc.Controllers.Private;

[ApiController]
[Route("api/v1/matriculas")]
[Authorize]
public class MatriculasController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? turmaId = null, [FromQuery] int? alunoId = null, [FromQuery] int page = 1, [FromQuery] int perPage = 10)
    {
        try
        {
            var (items, totalCount) = await _service.GetAllAsync(turmaId, alunoId, page, perPage);

            var result = new
            {
                Items = items.Select(m => m.ToViewModel()),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = perPage,
                TotalPages = (int)Math.Ceiling(totalCount / (double)perPage)
            };

            return StatusCode(200, ApiHelper.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }
    private readonly IMatriculaService _service;
    private readonly ILogger<MatriculasController> _logger;

    public MatriculasController(IMatriculaService service, ILogger<MatriculasController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MatriculaViewModel>> GetById([FromRoute] int id)
    {
        try
        {
            var model = await _service.GetByIdAsync(id);
            if (model == null)
                return StatusCode(404, ApiHelper.NotFound());

            return StatusCode(200, ApiHelper.Ok(model.ToViewModel()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriaMatriculaDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var existing = await _service.ExistsByAlunoAndTurmaAsync(dto.AlunoId, dto.TurmaId);
            if (existing)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Aluno já matriculado nesta turma"));

            var model = new Matricula
            {
                UsuarioId = dto.AlunoId,
                TurmaId = dto.TurmaId,
                // Status inicial de matrícula é automático.
                Situacao = SituacaoMatricula.CURSANDO
            };

            // O try/catch deve envolver apenas a chamada que pode lançar exceção
            try
            {
                await _service.CreateAsync(model);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("Duplicate entry"))
                {
                    return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Aluno já matriculado nesta turma (erro de duplicidade)."));
                }
                throw;
            }

            var result = await GetById(model.Id);
            if (result.Result is ObjectResult objectResult)
            {
                objectResult.StatusCode = 201;
                return objectResult;
            }

            return StatusCode(201, result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] AtualizaMatriculaDto dto)
    {
        try
        {
            if (dto == null)
                return StatusCode(400, ApiHelper.BadRequest("Corpo da requisição inválido"));

            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var model = await _service.GetByIdAsync(id);
            if (model == null)
                return StatusCode(404, ApiHelper.NotFound());

            if (dto.AlunoId.HasValue)
            {
                var existing = await _service.ExistsByAlunoAndTurmaAsync(dto.AlunoId.Value, model.TurmaId, id);
                if (existing)
                    return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Aluno já matriculado nesta turma"));
                model.UsuarioId = dto.AlunoId.Value;
            }

            if (dto.TurmaId.HasValue)
            {
                var existing = await _service.ExistsByAlunoAndTurmaAsync(model.UsuarioId, dto.TurmaId.Value, id);
                if (existing)
                    return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Aluno já matriculado nesta turma"));
                model.TurmaId = dto.TurmaId.Value;
            }

            await _service.UpdateAsync(model);

            return StatusCode(200, ApiHelper.Ok());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            var model = await _service.GetByIdAsync(id);
            if (model == null)
                return StatusCode(404, ApiHelper.NotFound());

            try
            {
                await _service.DeleteAsync(id);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("a foreign key constraint fails"))
                {
                    return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Não é possível excluir a matrícula pois existem registros dependentes (restrição de integridade)."));
                }
                throw;
            }

            return StatusCode(200, ApiHelper.Ok());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPost("{id}/calcular-aprovacao")]
    public async Task<IActionResult> CalcularAprovacao([FromRoute] int id)
    {
        try
        {
            var model = await _service.GetByIdAsync(id);
            if (model == null)
                return StatusCode(404, ApiHelper.NotFound());

            var updated = await _service.CalcularAprovacaoAsync(id);
            var viewModel = updated.ToViewModel();

            return StatusCode(200, ApiHelper.Ok(viewModel));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[MatriculasController.CalcularAprovacao] Erro de negócio ao calcular aprovação. MatriculaId={MatriculaId}", id);
            return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MatriculasController.CalcularAprovacao] Erro inesperado. MatriculaId={MatriculaId}", id);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }
}