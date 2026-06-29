using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ApiSgc.Models;
using ApiSgc.Services;
using ApiSgc.Models.DTOs;
using ApiSgc.Utils;
using ApiSgc.Models.Extensions;

namespace ApiSgc.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var (token, error) = await _authService.LoginAsync(dto);

            if (token == null)
            {
                _logger.LogError("Falha no login: {Error}", error);

                if (error == "Conta pendente de aprovação")
                    return StatusCode(401, ApiHelper.Unauthorized("Conta pendente de aprovação. Aguarde a liberação de um administrador."));

                if (error == "Email não validado")
                    return StatusCode(401, ApiHelper.Unauthorized("Email não validado. Verifique sua caixa de entrada."));

                return StatusCode(401, ApiHelper.Unauthorized("Credenciais inválidas"));
            }

            return StatusCode(200, ApiHelper.Ok(new { token }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegistroDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var user = new Usuario
            {
                Nome = dto.Name,
                Email = dto.Email,
                Senha = PasswordHelper.HashPassword(dto.Password),
                Role = Models.Enums.UserRole.ALUNO,
                Telefone = dto.Telefone,
                DataNascimento = dto.DataNascimento,
                Equipe = dto.Equipe,
                EstaEmCelula = dto.EstaEmCelula,
                NomeCelula = dto.NomeCelula,
                EstaSendoDiscipulado = dto.EstaSendoDiscipulado,
                NomeDiscipulador = dto.NomeDiscipulador,
                FezEncontro = dto.FezEncontro,
                Batizado = dto.Batizado
            };

            var (success, mensagem) = await _authService.RegisterAsync(user);

            if (!success)
            {
                return StatusCode(400, ApiHelper.BadRequest(mensagem ?? "Erro desconhecido"));
            }

            return StatusCode(200, ApiHelper.Ok(new { mensagem }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPost("reenvia-email")]
    public async Task<IActionResult> ResendValidationEmail([FromBody] RecuperaSenhaDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var (success, mensagem) = await _authService.ResendValidationEmailAsync(dto.Email);

            if (!success)
            {
                return StatusCode(400, ApiHelper.BadRequest(mensagem ?? "Erro desconhecido"));
            }

            return StatusCode(200, ApiHelper.Ok(new { mensagem }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPost("valida-email")]
    public async Task<IActionResult> ValidateEmail([FromBody] ValidaEmailDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var (success, error) = await _authService.ValidateEmailAsync(dto);

            if (!success)
            {
                return StatusCode(400, ApiHelper.BadRequest(error ?? "Erro desconhecido"));
            }

            return StatusCode(200, ApiHelper.Ok(new { mensagem = "Email validado com sucesso!" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPost("recupera-senha")]
    public async Task<IActionResult> RecoverPassword([FromBody] RecuperaSenhaDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var (success, mensagem) = await _authService.RequestPasswordRecoveryAsync(dto);

            return StatusCode(200, ApiHelper.Ok(new { mensagem }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPost("reseta-senha")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetaSenhaDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var (success, error) = await _authService.ResetPasswordAsync(dto);

            if (!success)
            {
                return StatusCode(400, ApiHelper.BadRequest(error ?? "Erro desconhecido"));
            }

            return StatusCode(200, ApiHelper.Ok(new { mensagem = "Senha redefinida com sucesso!" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserAsync()
    {
        try
        {
            var userId = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Sub ||
                c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return StatusCode(401, ApiHelper.Unauthorized("Token inválido"));

            var user = await _authService.GetUserAsync(userId);

            if (user == null)
                return StatusCode(404, ApiHelper.NotFound());

            var model = user.ToViewModel();

            return StatusCode(200, ApiHelper.Ok(model));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao carregar dados do usuário");
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }

    [HttpPost("altera-senha")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] AlteraSenhaDto dto)
    {
        try
        {
            if (!TryValidateModel(dto))
                return StatusCode(422, ApiHelper.UnprocessableEntity(ApiHelper.GetErrorMessages(ModelState)));

            var userId = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Sub ||
                c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userIdInt))
                return StatusCode(401, ApiHelper.Unauthorized("Token inválido"));

            var (success, error) = await _authService.ChangePasswordAsync(userIdInt, dto);

            if (!success)
            {
                return StatusCode(400, ApiHelper.BadRequest(error ?? "Erro ao alterar senha"));
            }

            return StatusCode(200, ApiHelper.Ok(new { mensagem = "Senha alterada com sucesso!" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao alterar senha");
            return StatusCode(500, ApiHelper.InternalServerError());
        }
    }
}