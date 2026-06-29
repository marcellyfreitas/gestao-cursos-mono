using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiSgc.Models.ViewModels;
using ApiSgc.Services.Contracts;
using ApiSgc.Utils;
using ApiSgc.Models.Extensions;
using ApiSgc.Models.DTOs;
using ApiSgc.Models;

namespace ApiSgc.Controllers.Private;

[ApiController]
[Route("api/v1/aulas")]
[Authorize]
public class AulasController : ControllerBase
{
    private readonly IAulaService _service;
    private readonly ILogger<AulasController> _logger;

    public AulasController(IAulaService service, ILogger<AulasController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] string? titulo,
        [FromQuery] int? turmaId,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 10)
    {
        try
        {
            var (items, totalCount) = await _service.GetAllAsync(titulo, turmaId, page, perPage);
            var list = items.Select(a => a.ToViewModel());

            var result = new
            {
                Items = list,
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

    [HttpGet("{id}")]
    public async Task<ActionResult<AulaViewModel>> GetById([FromRoute] int id)
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
    public async Task<IActionResult> Create([FromBody] CriaAulaDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var turmaExists = await _service.ExistsByTurmaAsync(dto.TurmaId);
            if (!turmaExists)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Turma não encontrada"));

            if (!dto.ProfessorId.HasValue || dto.ProfessorId.Value <= 0)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Professor é obrigatório"));

            var professorExists = await _service.ExistsByProfessorAsync(dto.ProfessorId.Value);
            if (!professorExists)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Professor não encontrado"));

            var model = new Aula
            {
                TurmaId = dto.TurmaId,
                Titulo = dto.Titulo,
                DataAula = dto.DataAula,
                ProfessorId = dto.ProfessorId,
                Descricao = dto.Descricao
            };

            await _service.CreateAsync(model);

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
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] AtualizaAulaDto dto)
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

            if (dto.TurmaId.HasValue)
            {
                var turmaExists = await _service.ExistsByTurmaAsync(dto.TurmaId.Value);
                if (!turmaExists)
                    return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Turma não encontrada"));
                model.TurmaId = dto.TurmaId.Value;
            }

            if (dto.Titulo != null) model.Titulo = dto.Titulo;
            if (dto.DataAula.HasValue) model.DataAula = dto.DataAula;

            if (!dto.ProfessorId.HasValue || dto.ProfessorId.Value <= 0)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Professor é obrigatório"));

            var professorExists = await _service.ExistsByProfessorAsync(dto.ProfessorId.Value);
            if (!professorExists)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Professor não encontrado"));

            model.ProfessorId = dto.ProfessorId;
            if (dto.Descricao != null) model.Descricao = dto.Descricao;

            model.Turma = null!;
            model.Professor = null!;

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

            await _service.DeleteAsync(id);

            return StatusCode(200, ApiHelper.Ok());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }
}