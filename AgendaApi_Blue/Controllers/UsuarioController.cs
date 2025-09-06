using AgendaApi_Blue.Models.ViewModels.Usuario;
using AgendaApi_Blue.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgendaApi_Blue.Services.Interfaces;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace AgendaApi_Blue.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly IUsuarioService _usuarioService;
        private readonly IValidator<Usuario> _usuarioValidator;

        public UsuarioController(IValidator<Usuario> usuarioValidator, IUsuarioService usuarioService, IMapper mapper, IRabbitMqService rabbitMqService)
        {
            _usuarioValidator = usuarioValidator;
            _usuarioService = usuarioService;
            _mapper = mapper;
            _rabbitMqService = rabbitMqService;
        }

        [HttpPost]
        public async Task<IActionResult> CriarUsuario([FromBody] UsuarioViewModel request)
        {
            try
            {
                var usuario = _mapper.Map<Usuario>(request);

                var validationResult = await _usuarioValidator.ValidateAsync(usuario);
                if (!validationResult.IsValid)
                    return BadRequest(validationResult.Errors);

                var sucesso = await _usuarioService.CriarUsuario(usuario);

                if (!sucesso)
                    return BadRequest("Usuário já existe.");

                _rabbitMqService.EnviarMensagem($"Usuário criado: {usuario.Username}");
                return CreatedAtAction(nameof(CriarUsuario), "Usuário criado com sucesso.");
            }
            catch (DbUpdateException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao salvar usuário no banco de dados.");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> ExcluirUsuario(int id)
        {
            try
            {
                var sucesso = await _usuarioService.ExcluirUsuario(id);

                if (!sucesso)
                    return NotFound("Usuário não encontrado.");

                _rabbitMqService.EnviarMensagem($"Usuário excluido: {id}");
                return Ok("Usuário excluido.");
            }
            catch (DbUpdateException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao excluir usuário do banco de dados.");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado.");
            }
        }
    }
}
