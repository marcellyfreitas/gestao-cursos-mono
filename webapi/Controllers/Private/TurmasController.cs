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
[Route("api/v1/turmas")]
[Authorize]
public class TurmasController : ControllerBase
{
    private readonly ITurmaService _service;
    private readonly ILogger<TurmasController> _logger;

    public TurmasController(ITurmaService service, ILogger<TurmasController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? cursoId,
        [FromQuery] string? nome,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 10)
    {
        try
        {
            var (items, totalCount) = await _service.GetAllAsync(cursoId, nome, page, perPage);

            var result = new
            {
                Items = items.Select(t => t.ToViewModel()),
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
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        try
        {
            var model = await _service.GetByIdAsync(id);
            if (model is null)
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
    public async Task<IActionResult> Create([FromBody] CriaTurmaDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var cursoExists = await _service.CursoExistsAsync(dto.CursoId);
            if (!cursoExists)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Curso não encontrado"));

            var existingNome = await _service.ExistsByNomeAndCursoAsync(dto.Nome, dto.CursoId);
            if (existingNome)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Nome da turma já está em uso para este curso"));

            var model = new Turma
            {
                CursoId = dto.CursoId,
                Nome = dto.Nome,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                NecessitaAtividades = dto.NecessitaAtividades,
                MediaMinima = dto.MediaMinima,
                FaltasParaReprovacao = dto.FaltasParaReprovacao
            };

            await _service.CreateAsync(model);

            var created = await _service.GetByIdAsync(model.Id);
            return StatusCode(201, ApiHelper.Ok(created!.ToViewModel()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] AtualizaTurmaDto dto)
    {
        try
        {
            if (dto is null)
                return StatusCode(400, ApiHelper.BadRequest("Corpo da requisição inválido"));

            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var model = await _service.GetByIdAsync(id);
            if (model is null)
                return StatusCode(404, ApiHelper.NotFound());

            if (dto.CursoId.HasValue)
            {
                var cursoExists = await _service.CursoExistsAsync(dto.CursoId.Value);
                if (!cursoExists)
                    return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Curso não encontrado"));

                model.CursoId = dto.CursoId.Value;
            }

            if (dto.Nome is not null)
            {
                var existingNome = await _service.ExistsByNomeAndCursoAsync(dto.Nome, model.CursoId, id);
                if (existingNome)
                    return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Nome da turma já está em uso para este curso"));

                model.Nome = dto.Nome;
            }

            model.DataInicio = dto.LimparDataInicio ? null : dto.DataInicio ?? model.DataInicio;
            model.DataFim = dto.LimparDataFim ? null : dto.DataFim ?? model.DataFim;

            if (dto.NecessitaAtividades.HasValue)
                model.NecessitaAtividades = dto.NecessitaAtividades.Value;

            if (dto.MediaMinima.HasValue)
                model.MediaMinima = dto.MediaMinima.Value;

            if (dto.FaltasParaReprovacao.HasValue)
                model.FaltasParaReprovacao = dto.FaltasParaReprovacao.Value;

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
            if (model is null)
                return StatusCode(404, ApiHelper.NotFound());

            var possuiMatriculas = await _service.PossuiMatriculasAtivasAsync(id);
            if (possuiMatriculas)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Não é possível excluir a turma, pois existem alunos matriculados."));

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