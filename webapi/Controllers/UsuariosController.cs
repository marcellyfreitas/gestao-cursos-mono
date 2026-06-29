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
[Route("api/v1/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;
    private readonly IEmailService _emailService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(
        IUsuarioService service,
        IEmailService emailService,
        ILogger<UsuariosController> logger)
    {
        _service = service;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioViewModel>>> GetAll(
        [FromQuery] string? nome,
        [FromQuery] string? email,
        [FromQuery] UserRole? role,
        [FromQuery] bool? estaEmCelula,
        [FromQuery] bool? estaSendoDiscipulado,
        [FromQuery] bool? batizado,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 10)
    {
        try
        {
            var filtro = new UsuarioFiltroDto
            {
                Nome = nome,
                Email = email,
                Role = role,
                EstaEmCelula = estaEmCelula,
                EstaSendoDiscipulado = estaSendoDiscipulado,
                Batizado = batizado,
                Page = page,
                PerPage = perPage
            };

            var (items, totalCount) = await _service.GetAllAsync(filtro);

            var list = items.Select(u => u.ToViewModel());

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
    public async Task<ActionResult<UsuarioViewModel>> GetById([FromRoute] int id)
    {
        try
        {
            var model = await _service.GetAsync(id);

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
    public async Task<IActionResult> AddAsync([FromBody] CriaUsuarioDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var existingEmail = await _service.GetByEmailAsync(dto.Email);

            if (existingEmail != null)
                return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Email está em uso"));

            var model = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Senha = PasswordHelper.HashPassword(dto.Senha),
                Role = dto.Role,
                Telefone = dto.Telefone,
                DataNascimento = dto.DataNascimento,
                Equipe = dto.Equipe,
                EstaEmCelula = dto.EstaEmCelula,
                NomeCelula = dto.NomeCelula,
                EstaSendoDiscipulado = dto.EstaSendoDiscipulado,
                NomeDiscipulador = dto.NomeDiscipulador,
                FezEncontro = dto.FezEncontro,
                Batizado = dto.Batizado,
                // Admin cria usuário já ativo e com email validado
                Ativo = true,
                EmailValidado = true
            };

            await _service.AddAsync(model);

            // Email de validação desativado — admin aprova diretamente
            // await _emailService.SendEmailValidationAsync(model.Email, model.Nome, model.TokenValidacaoEmail);

            _logger.LogInformation("[UsuariosController.Add] Usuário criado pelo admin. Id={UsuarioId}, Email={Email}, Role={Role}, Ativo=true",
                model.Id, model.Email, model.Role);

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
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] AtualizaUsuarioDto dto)
    {
        try
        {
            if (dto == null)
                return StatusCode(400, ApiHelper.BadRequest("Corpo da requisição inválido"));

            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var model = await _service.GetAsync(id);
            if (model == null)
                return StatusCode(404, ApiHelper.NotFound());

            if (dto.Email != null)
            {
                var existingEmail = await _service.GetByEmailAsync(dto.Email, id);

                if (existingEmail != null)
                    return StatusCode(422, ApiHelper.UnprocessableEntity(Array.Empty<int>(), "Email está em uso"));
            }

            model.Nome = dto.Nome ?? model.Nome;
            model.Email = dto.Email ?? model.Email;
            model.Senha = dto.Senha != null ? PasswordHelper.HashPassword(dto.Senha) : model.Senha;
            model.Role = dto.Role ?? model.Role;
            model.Telefone = dto.Telefone ?? model.Telefone;
            model.DataNascimento = dto.DataNascimento ?? model.DataNascimento;
            model.Equipe = dto.Equipe ?? model.Equipe;
            model.EstaEmCelula = dto.EstaEmCelula ?? model.EstaEmCelula;
            model.NomeCelula = dto.NomeCelula ?? model.NomeCelula;
            model.EstaSendoDiscipulado = dto.EstaSendoDiscipulado ?? model.EstaSendoDiscipulado;
            model.NomeDiscipulador = dto.NomeDiscipulador ?? model.NomeDiscipulador;
            model.FezEncontro = dto.FezEncontro ?? model.FezEncontro;
            model.Batizado = dto.Batizado ?? model.Batizado;
            model.UpdatedAt = DateTime.UtcNow;

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
            var user = await _service.GetAsync(id);

            if (user == null)
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

    [HttpPut("{id}/status")]
    public async Task<IActionResult> ToggleStatus([FromRoute] int id)
    {
        try
        {
            var user = await _service.GetAsync(id);

            if (user == null)
                return StatusCode(404, ApiHelper.NotFound());

            user.Ativo = !user.Ativo;
            user.UpdatedAt = DateTime.UtcNow;

            await _service.UpdateAsync(user);

            _logger.LogInformation("[UsuariosController.ToggleStatus] UsuarioId={UsuarioId}, Email={Email} → Ativo={Ativo}",
                id, user.Email, user.Ativo);

            return StatusCode(200, ApiHelper.Ok(user.ToViewModel()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UsuariosController.ToggleStatus] Erro ao alterar status. UsuarioId={UsuarioId}", id);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPut("{id}/senha")]
    public async Task<IActionResult> ResetPassword([FromRoute] int id, [FromBody] AdminResetaSenhaDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var user = await _service.GetAsync(id);

            if (user == null)
                return StatusCode(404, ApiHelper.NotFound());

            user.Senha = PasswordHelper.HashPassword(dto.NovaSenha);
            user.UpdatedAt = DateTime.UtcNow;

            await _service.UpdateAsync(user);

            _logger.LogInformation("[UsuariosController.ResetPassword] Senha resetada pelo admin. UsuarioId={UsuarioId}, Email={Email}",
                id, user.Email);

            return StatusCode(200, ApiHelper.Ok(new { mensagem = "Senha do usuário redefinida com sucesso!" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UsuariosController.ResetPassword] Erro ao resetar senha. UsuarioId={UsuarioId}", id);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }
}