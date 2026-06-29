using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiSgc.Models.DTOs;
using ApiSgc.Models.ViewModels;
using ApiSgc.Services.Contracts;
using ApiSgc.Utils;
using ApiSgc.Models.Extensions;
using ApiSgc.Models;

namespace ApiSgc.Controllers.Private;

[ApiController]
[Route("api/v1/notas")]
[Authorize]
public class NotasController : ControllerBase
{
    private readonly INotaService _service;
    private readonly ILogger<NotasController> _logger;

    public NotasController(INotaService service, ILogger<NotasController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NotaViewModel>> GetById([FromRoute] int id)
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
    public async Task<IActionResult> Create([FromBody] CriaNotaDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var existing = await _service.ExistsByMatriculaAndAvaliacaoAsync(dto.MatriculaId, dto.AvaliacaoId);
            if (existing)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Nota já lançada para esta matrícula e avaliação"));

            var model = new Nota
            {
                MatriculaId = dto.MatriculaId,
                AvaliacaoId = dto.AvaliacaoId,
                Valor = dto.Valor,
            };

            await _service.CreateAsync(model);

            var result = new NotaViewModel
            {
                Id = model.Id,
                MatriculaId = model.MatriculaId,
                AvaliacaoId = model.AvaliacaoId,
                Valor = model.Valor,
            };

            return StatusCode(201, ApiHelper.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] AtualizaNotaDto dto)
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

            if (dto.Valor.HasValue)
                model.Valor = dto.Valor.Value;

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

    [HttpGet("alunos")]
    public async Task<ActionResult> GetAlunos(
        [FromQuery] int turmaId,
        [FromQuery] int avaliacaoId)
    {
        try
        {
            if (turmaId <= 0)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Turma é obrigatória"));

            if (avaliacaoId <= 0)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Avaliação é obrigatória"));

            var alunos = await _service.GetAlunosComNotasAsync(turmaId, avaliacaoId);
            return StatusCode(200, ApiHelper.Ok(alunos));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[NotasController.GetAlunos] Erro de negócio. TurmaId={TurmaId}, AvaliacaoId={AvaliacaoId}",
                turmaId, avaliacaoId);
            return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NotasController.GetAlunos] Erro inesperado. TurmaId={TurmaId}, AvaliacaoId={AvaliacaoId}",
                turmaId, avaliacaoId);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPost("lote")]
    public async Task<IActionResult> SalvarLote([FromBody] SalvarNotasLoteDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            await _service.SalvarNotasLoteAsync(dto);

            return StatusCode(200, ApiHelper.Ok("Notas salvas com sucesso!"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[NotasController.SalvarLote] Erro de negócio. AvaliacaoId={AvaliacaoId}, QtdItems={QtdItems}",
                dto.AvaliacaoId, dto.Items?.Count ?? 0);
            return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NotasController.SalvarLote] Erro inesperado. AvaliacaoId={AvaliacaoId}, QtdItems={QtdItems}",
                dto.AvaliacaoId, dto.Items?.Count ?? 0);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }
}
