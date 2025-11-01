using AgendaApi_Blue.Models.ViewModels.Auth;
using AgendaApi_Blue.Services.Interfaces;
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
                return Ok(new { Token = token });
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocorreu um erro inesperado.");
            }         
        }
    }
}