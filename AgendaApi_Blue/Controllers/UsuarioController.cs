using AgendaApi_Blue.Models.ViewModels.Usuario;
using Microsoft.AspNetCore.Authorization;
using AgendaApi_Blue.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AgendaApi_Blue.Utilitaries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using AgendaApi_Blue.Models;
using FluentValidation;
using AutoMapper;
using System.Net;
using AgendaApi_Blue.Services;
using System.Security.Claims;
using AgendaApi_Blue.Exceptions;

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
        public async Task<IActionResult> Criar([FromBody] UsuarioViewModel request)
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
                return CreatedAtAction(nameof(Criar), "Usuário criado com sucesso.");
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

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Editar([FromBody] UsuarioViewModel request, int id)
        {
            try
            {
                // TODO: Atualizar Role da Claim após edição dos mesmos

                var usuarioLogado = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "UsuarioId")?.Value);
                var roleLogada = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                await _usuarioService.ValidarEditar(usuarioLogado, roleLogada, id);

                var usuario = _mapper.Map<Usuario>(request);

                var validationResult = await _usuarioValidator.ValidateAsync(usuario);
                if (!validationResult.IsValid)
                    return BadRequest(validationResult.Errors);

                var sucesso = await _usuarioService.EditarUsuario(usuario, id);
                if (!sucesso)
                    return BadRequest("Falha ao editar usuário.");

                _rabbitMqService.EnviarMensagem($"Usuário editado: {usuario.Username}");
                return Ok("Usuário editado com sucesso.");
            }
            catch (UsuarioNaoAutorizadoException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (UsuarioNaoEncontradoException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
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
        [Authorize(Roles = nameof(Enums.Role.Admin))]
        public async Task<IActionResult> Excluir(int id)
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

        [HttpGet("{id}")]
        [Authorize(Roles = nameof(Enums.Role.Admin))]
        public async Task<IActionResult> Obter(int id)
        {
            try
            {
                var usuario = await _usuarioService.ObterUsuario(id);
                return Ok(usuario);
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                return StatusCode((int)HttpStatusCode.RequestTimeout, "O tempo de conexão com o banco de dados expirou. Tente novamente mais tarde.");
            }
            catch (SqlException ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao se comunicar com o banco de dados. Tente novamente mais tarde.");
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado. Tente novamente mais tarde.");
            }
        }
    }
}