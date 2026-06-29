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
[Route("api/v1/professores")]
[Authorize]
public class ProfessoresController : ControllerBase
{
    private readonly IProfessorService _service;
    private readonly ILogger<ProfessoresController> _logger;

    public ProfessoresController(
        IProfessorService service,
        ILogger<ProfessoresController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] string? nome,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 10)
    {
        try
        {
            var (items, totalCount) = await _service.GetAllAsync(nome, page, perPage);
            var list = items.Select(p => p.ToViewModel());

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
    public async Task<ActionResult<ProfessorViewModel>> GetById([FromRoute] int id)
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
    public async Task<IActionResult> Create([FromBody] CriaProfessorDto dto)
    {
        try
        {
            if (dto == null)
                return StatusCode(400, ApiHelper.BadRequest("Corpo da requisição inválido"));

            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            dto.Nome = dto.Nome?.Trim() ?? string.Empty;
            dto.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            dto.Telefone = string.IsNullOrWhiteSpace(dto.Telefone) ? null : dto.Telefone.Trim();

            var existingNome = await _service.ExistsByNomeAsync(dto.Nome);
            if (existingNome)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Nome do professor já está em uso"));

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existingEmail = await _service.ExistsByEmailAsync(dto.Email);
                if (existingEmail)
                    return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Email do professor já está em uso"));
            }

            var model = new Professor
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Ativo = true
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
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] AtualizaProfessorDto dto)
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

            if (dto.Nome != null)
            {
                dto.Nome = dto.Nome.Trim();

                var existingNome = await _service.ExistsByNomeAsync(dto.Nome, id);
                if (existingNome)
                    return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Nome do professor já está em uso"));

                model.Nome = dto.Nome;
            }

            if (dto.Email != null)
            {
                dto.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();

                var existingEmail = await _service.ExistsByEmailAsync(dto.Email, id);
                if (existingEmail)
                    return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Email do professor já está em uso"));

                model.Email = dto.Email;
            }

            if (dto.Telefone != null)
                model.Telefone = string.IsNullOrWhiteSpace(dto.Telefone) ? null : dto.Telefone.Trim();

            await _service.UpdateAsync(model);

            return StatusCode(200, ApiHelper.Ok(model.ToViewModel()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPut("{id}/status")]
public async Task<IActionResult> ToggleStatus([FromRoute] int id)
{
    try
    {
        var model = await _service.GetByIdAsync(id);

        if (model == null)
            return StatusCode(404, ApiHelper.NotFound());

        model.Ativo = !model.Ativo;

        await _service.UpdateAsync(model);

        return StatusCode(200, ApiHelper.Ok(model.ToViewModel()));
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
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return StatusCode(404, ApiHelper.NotFound());

            return StatusCode(200, ApiHelper.Ok());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }
    
}