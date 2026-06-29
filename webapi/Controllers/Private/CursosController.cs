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
[Route("api/v1/cursos")]
[Authorize]
public class CursosController : ControllerBase
{
    private readonly ICursoService _service;
    private readonly ICursoPrerequisitoService _prerequisitoService;
    private readonly ILogger<CursosController> _logger;

    public CursosController(ICursoService service, ICursoPrerequisitoService prerequisitoService, ILogger<CursosController> logger)
    {
        _service = service;
        _prerequisitoService = prerequisitoService;
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
            var list = items.Select(c => c.ToViewModel());

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
    public async Task<ActionResult<CursoViewModel>> GetById([FromRoute] int id)
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
    public async Task<IActionResult> Create([FromBody] CriaCursoDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var existingNome = await _service.ExistsByNomeAsync(dto.Nome);
            if (existingNome)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Nome do curso já está em uso"));

            var model = new Curso
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
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
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] AtualizaCursoDto dto)
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
                var existingNome = await _service.ExistsByNomeAsync(dto.Nome, id);
                if (existingNome)
                    return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Nome do curso já está em uso"));
                model.Nome = dto.Nome;
            }

            if (dto.Descricao != null) model.Descricao = dto.Descricao;

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

    [HttpGet("{id}/prerequisitos")]
    public async Task<ActionResult> GetPrerequisitos([FromRoute] int id)
    {
        try
        {
            var curso = await _service.GetByIdAsync(id);
            if (curso == null)
                return StatusCode(404, ApiHelper.NotFound());

            var prerequisitos = await _prerequisitoService.GetByCursoIdAsync(id);
            var list = prerequisitos.Select(cp => cp.PrerequisitoCurso.ToViewModel());

            return StatusCode(200, ApiHelper.Ok(list));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPost("{id}/prerequisitos")]
    public async Task<IActionResult> AddPrerequisito([FromRoute] int id, [FromBody] CriaCursoPrerequisitoDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var curso = await _service.GetByIdAsync(id);
            if (curso == null)
                return StatusCode(404, ApiHelper.NotFound());

            var prerequisito = await _service.GetByIdAsync(dto.CursoPrerequisitoId);
            if (prerequisito == null)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Curso pré-requisito não encontrado"));

            var exists = await _prerequisitoService.ExistsByCursoAndPrerequisitoAsync(id, dto.CursoPrerequisitoId);
            if (exists)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Pré-requisito já existe"));

            var isCircular = await _prerequisitoService.IsCircularReferenceAsync(id, dto.CursoPrerequisitoId);
            if (isCircular)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Referência circular detectada"));

            var model = new CursoPrerequisito
            {
                CursoId = id,
                CursoPrerequisitoId = dto.CursoPrerequisitoId
            };

            await _prerequisitoService.CreateAsync(model);

            return StatusCode(201, ApiHelper.Ok());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpDelete("{id}/prerequisitos/{prerequisitoId}")]
    public async Task<IActionResult> RemovePrerequisito([FromRoute] int id, [FromRoute] int prerequisitoId)
    {
        try
        {
            var curso = await _service.GetByIdAsync(id);
            if (curso == null)
                return StatusCode(404, ApiHelper.NotFound());

            var exists = await _prerequisitoService.ExistsByCursoAndPrerequisitoAsync(id, prerequisitoId);
            if (!exists)
                return StatusCode(404, ApiHelper.NotFound());

            var prereqs = await _prerequisitoService.GetByCursoIdAsync(id);
            var prereq = prereqs.FirstOrDefault(cp => cp.CursoPrerequisitoId == prerequisitoId);
            
            if (prereq == null)
                return StatusCode(404, ApiHelper.NotFound());

            await _prerequisitoService.DeleteAsync(prereq.Id);

            return StatusCode(200, ApiHelper.Ok());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }
}