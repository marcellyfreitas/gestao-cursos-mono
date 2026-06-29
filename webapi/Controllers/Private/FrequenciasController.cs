using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiSgc.Services.Contracts;
using ApiSgc.Utils;
using ApiSgc.Models.DTOs;

namespace ApiSgc.Controllers.Private;

[ApiController]
[Route("api/v1/frequencias")]
[Authorize]
public class FrequenciasController : ControllerBase
{
    private readonly IFrequenciaService _service;
    private readonly ILogger<FrequenciasController> _logger;

    public FrequenciasController(IFrequenciaService service, ILogger<FrequenciasController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("alunos")]
    public async Task<ActionResult> GetAlunos(
        [FromQuery] int aulaId)
    {
        try
        {
            if (aulaId <= 0)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Aula é obrigatória"));

            var alunos = await _service.GetAlunosComFrequenciaAsync(aulaId);
            return StatusCode(200, ApiHelper.Ok(alunos));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[FrequenciasController.GetAlunos] Erro de negócio. AulaId={AulaId}", aulaId);
            return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FrequenciasController.GetAlunos] Erro inesperado. AulaId={AulaId}", aulaId);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPost("lote")]
    public async Task<IActionResult> SalvarLote([FromBody] SalvarFrequenciaLoteDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            await _service.SalvarFrequenciaLoteAsync(dto);

            return StatusCode(200, ApiHelper.Ok("Frequência salva com sucesso!"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[FrequenciasController.SalvarLote] Erro de negócio. AulaId={AulaId}, QtdItems={QtdItems}",
                dto.AulaId, dto.Items?.Count ?? 0);
            return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FrequenciasController.SalvarLote] Erro inesperado. AulaId={AulaId}, QtdItems={QtdItems}",
                dto.AulaId, dto.Items?.Count ?? 0);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }
}
