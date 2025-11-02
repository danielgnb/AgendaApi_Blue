using AgendaApi_Blue.Models.ViewModels.Contato;
using Microsoft.AspNetCore.Authorization;
using AgendaApi_Blue.Services.Interfaces;
using AgendaApi_Blue.Utilitaries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using AgendaApi_Blue.Models;
using FluentValidation;
using System.Net;
using AutoMapper;

namespace AgendaApi_Blue.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContatoController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly IContatoService _contatoService;
        private readonly IValidator<Contato> _contatoValidator;

        public ContatoController(IContatoService _contatoService, IValidator<Contato> _contatoValidator, IMapper _mapper, IRabbitMqService _rabbitMqService)
        {
            this._contatoService = _contatoService;
            this._contatoValidator = _contatoValidator;
            this._mapper = _mapper;
            this._rabbitMqService = _rabbitMqService;
        }

        [HttpGet]
        [Authorize(Roles = nameof(Enums.Role.Admin))]
        public async Task<IActionResult> GetContatos()
        {
            try
            {
                var contatos = await _contatoService.ObterContatos();
                return Ok(contatos);
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

        [HttpGet("Usuario/{idUsuario}")]
        public async Task<IActionResult> GetContatosPorIdUsuario(int idUsuario)
        {
            try
            {
                var contatos = await _contatoService.ObterContatosPorUsuario(idUsuario);

                if (contatos == null || contatos.Count == 0)
                    return NotFound("Nenhum contato encontrado para este usuário.");

                return Ok(contatos);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContato(int id)
        {
            try
            {
                var contato = await _contatoService.ObterContato(id);

                if (contato == null)
                    return NotFound("Contato não encontrado.");

                return Ok(contato);
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

        [HttpPost]
        public async Task<IActionResult> CriarContato([FromBody] ContatoViewModel request)
        {
            try
            {
                #region Validações
                var usuarioId = User.Claims.FirstOrDefault(c => c.Type == "UsuarioId")?.Value;
                if (usuarioId == null)
                    return Unauthorized("Usuário não autenticado.");

                var contato = _mapper.Map<Contato>(request);
                contato.IdUsuario = int.Parse(usuarioId);

                var validationResult = await _contatoValidator.ValidateAsync(contato);
                if (!validationResult.IsValid)
                    return BadRequest(validationResult.Errors);
                #endregion

                var sucesso = await _contatoService.CriarContato(contato);

                if (sucesso)
                {
                    _rabbitMqService.EnviarMensagem($"Novo contato criado: {contato.Nome}");
                    return CreatedAtAction(nameof(CriarContato), "Contato criado com sucesso.");
                }
                else
                {
                    return BadRequest("Erro ao criar o contato.");
                }
            }
            catch (SqlException ex) when (ex.Number == -2)
            {
                return StatusCode((int)HttpStatusCode.RequestTimeout, "O tempo de conexão com o banco de dados expirou. Tente novamente mais tarde.");
            }
            catch (SqlException ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Erro ao se comunicar com o banco de dados. Tente novamente mais tarde.");
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado. Tente novamente mais tarde.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarContato(int id, [FromBody] ContatoViewModel request)
        {
            try
            {
                #region Validações
                var usuarioId = User.Claims.FirstOrDefault(c => c.Type == "UsuarioId")?.Value;
                if (usuarioId == null)
                    return Unauthorized("Usuário não autenticado.");

                var contato = _mapper.Map<Contato>(request);
                contato.Id = id;
                contato.IdUsuario = int.Parse(usuarioId);

                var validationResult = await _contatoValidator.ValidateAsync(contato);
                if (!validationResult.IsValid)
                    return BadRequest(validationResult.Errors);
                #endregion

                var sucesso = await _contatoService.EditarContato(contato);

                if (sucesso)
                {
                    _rabbitMqService.EnviarMensagem($"Contato editado: {contato.Nome}");
                    return Ok(contato);
                }
                else
                    return NotFound("Contato não encontrado.");
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoverContato(int id)
        {
            try
            {
                var sucesso = await _contatoService.RemoverContato(id);

                if (sucesso)
                {
                    _rabbitMqService.EnviarMensagem($"Contato removido: {id}");
                    return Ok("Contato removido com sucesso.");
                }
                else
                {
                    return NotFound("Contato não encontrado.");
                }
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