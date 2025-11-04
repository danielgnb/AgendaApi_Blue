using AgendaApi_Blue.Models.ViewModels.Auth;
using AgendaApi_Blue.Services.Interfaces;
using AgendaApi_Blue.Models.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using AgendaApi_Blue.Models;
using FluentValidation;
using AutoMapper;

namespace AgendaApi_Blue.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IUsuarioService _usuarioService;
        private readonly IValidator<Usuario> _loginValidator;

        public AuthController(IAuthService authService, IValidator<Usuario> loginValidator, IUsuarioService usuarioService, IMapper mapper)
        {
            _authService = authService;
            _loginValidator = loginValidator;
            _usuarioService = usuarioService;
            _mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel request)
        {
            try
            {
                var usuario = _mapper.Map<Usuario>(request);

                var validationResult = await _loginValidator.ValidateAsync(usuario);
                if (!validationResult.IsValid)
                    return BadRequest(validationResult.Errors);

                var validateUser = await _usuarioService.ValidarUsuario(usuario);
                if (validateUser is null)
                    return Unauthorized("Credenciais inválidas.");

                var token = _authService.GerarToken(validateUser.Username, validateUser.Id, validateUser.Role);
                await _authService.RegistrarAcesso(new AcessoDTO() { IdUsuario = validateUser.Id, AccessToken = token.accessToken, RefreshToken = token.refreshToken });

                return Ok(new { token.accessToken, token.refreshToken });
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro inesperado.");
            }         
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] TokenRefreshRequest request)
        {
            try
            {
                #region Validações de token
                var login = _authService.ObterPorRefreshToken(request.RefreshToken);
                if (login == null || login.RefreshTokenExpiration < DateTime.Now)
                    return Unauthorized("Refresh token inválido ou expirado.");

                if (login.AccessTokenExpiration > DateTime.Now + TimeSpan.FromSeconds(30))
                {
                    return StatusCode(StatusCodes.Status409Conflict, new
                    {
                        message = "Access token ainda válido; não é necessário refresh.",
                        expiresAt = login.AccessTokenExpiration.ToString("dd/MM/yyyy HH:mm:ss")
                    });
                }
                #endregion

                var (newAccessToken, newRefreshToken) = _authService.GerarToken(
                    login.Usuario.Username,
                    login.IdUsuario,
                    login.Usuario.Role
                );

                await _authService.RegistrarAcesso(new AcessoDTO
                {
                    IdUsuario = login.IdUsuario,
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                });

                return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });

                // TODO: Remover linha
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro inesperado.");
            }
        }
    }
}