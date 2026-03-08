using System.Security.Claims;
using AgendaApi_Blue.Controllers;
using AgendaApi_Blue.Models;
using AgendaApi_Blue.Models.ViewModels.Contato;
using AgendaApi_Blue.Services.Interfaces;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AgendaApi_BlueTests.Controllers;

public class ContatoControllerTests
{
    [Fact]
    public async Task CriarContato_ShouldReturnUnauthorized_WhenUserClaimIsMissing()
    {
        var service = new Mock<IContatoService>();
        var validator = new Mock<IValidator<Contato>>();
        var mapper = new Mock<IMapper>();
        var rabbit = new Mock<IRabbitMqService>();

        var controller = new ContatoController(service.Object, validator.Object, mapper.Object, rabbit.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.CriarContato(new ContatoViewModel());

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.Value.Should().Be("Usuário não autenticado.");
    }

    [Fact]
    public async Task CriarContato_ShouldReturnBadRequest_WhenValidationFails()
    {
        var service = new Mock<IContatoService>();
        var validator = new Mock<IValidator<Contato>>();
        var mapper = new Mock<IMapper>();
        var rabbit = new Mock<IRabbitMqService>();

        var contato = new Contato { Nome = "Ana", Email = "ana@teste.com", Telefone = "999" };
        TestHelper.SetupMapper<ContatoViewModel, Contato>(mapper, contato);
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<Contato>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelper.InvalidResult());

        var controller = BuildControllerWithUser(service, validator, mapper, rabbit, 5);

        var result = await controller.CriarContato(new ContatoViewModel());

        result.Should().BeOfType<BadRequestObjectResult>();
        service.Verify(s => s.CriarContato(It.IsAny<Contato>()), Times.Never);
    }

    [Fact]
    public async Task CriarContato_ShouldReturnCreatedAndSendMessage_WhenSuccessful()
    {
        var service = new Mock<IContatoService>();
        var validator = new Mock<IValidator<Contato>>();
        var mapper = new Mock<IMapper>();
        var rabbit = new Mock<IRabbitMqService>();

        var contato = new Contato { Nome = "Ana", Email = "ana@teste.com", Telefone = "999" };
        TestHelper.SetupMapper<ContatoViewModel, Contato>(mapper, contato);
        TestHelper.SetupValidatorSuccess(validator);
        service.Setup(s => s.CriarContato(It.IsAny<Contato>())).ReturnsAsync(true);

        var controller = BuildControllerWithUser(service, validator, mapper, rabbit, 5);

        var result = await controller.CriarContato(new ContatoViewModel());

        result.Should().BeOfType<CreatedAtActionResult>();
        service.Verify(s => s.CriarContato(It.Is<Contato>(c => c.IdUsuario == 5)), Times.Once);
        rabbit.Verify(r => r.EnviarMensagem("Novo contato criado: Ana"), Times.Once);
    }

    [Fact]
    public async Task GetContatosPorIdUsuario_ShouldReturnNotFound_WhenListIsEmpty()
    {
        var service = new Mock<IContatoService>();
        var validator = new Mock<IValidator<Contato>>();
        var mapper = new Mock<IMapper>();
        var rabbit = new Mock<IRabbitMqService>();

        service.Setup(s => s.ObterContatosPorUsuario(2)).ReturnsAsync(new List<Contato?>());

        var controller = new ContatoController(service.Object, validator.Object, mapper.Object, rabbit.Object);

        var result = await controller.GetContatosPorIdUsuario(2);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.Value.Should().Be("Nenhum contato encontrado para este usuário.");
    }

    [Fact]
    public async Task EditarContato_ShouldReturnOk_WhenSuccessful()
    {
        var service = new Mock<IContatoService>();
        var validator = new Mock<IValidator<Contato>>();
        var mapper = new Mock<IMapper>();
        var rabbit = new Mock<IRabbitMqService>();

        var contato = new Contato { Nome = "Ana", Email = "ana@teste.com", Telefone = "999" };
        TestHelper.SetupMapper<ContatoViewModel, Contato>(mapper, contato);
        TestHelper.SetupValidatorSuccess(validator);
        service.Setup(s => s.EditarContato(It.IsAny<Contato>())).ReturnsAsync(true);

        var controller = BuildControllerWithUser(service, validator, mapper, rabbit, 7);

        var result = await controller.EditarContato(11, new ContatoViewModel());

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContato = ok.Value.Should().BeOfType<Contato>().Subject;
        returnedContato.Id.Should().Be(11);
        returnedContato.IdUsuario.Should().Be(7);
        rabbit.Verify(r => r.EnviarMensagem("Contato editado: Ana"), Times.Once);
    }

    private static ContatoController BuildControllerWithUser(
        Mock<IContatoService> service,
        Mock<IValidator<Contato>> validator,
        Mock<IMapper> mapper,
        Mock<IRabbitMqService> rabbit,
        int usuarioId)
    {
        var controller = new ContatoController(service.Object, validator.Object, mapper.Object, rabbit.Object);
        var claims = new[] { new Claim("UsuarioId", usuarioId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        return controller;
    }
}
