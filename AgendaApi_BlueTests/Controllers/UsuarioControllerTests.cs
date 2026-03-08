using AgendaApi_Blue.Controllers;
using AgendaApi_Blue.Exceptions;
using AgendaApi_Blue.Models;
using AgendaApi_Blue.Models.ViewModels.Usuario;
using AgendaApi_Blue.Services.Interfaces;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AgendaApi_BlueTests.Controllers;

public class UsuarioControllerTests
{
    [Fact]
    public async Task Criar_ShouldReturnBadRequest_WhenValidationFails()
    {
        var validator = new Mock<IValidator<Usuario>>();
        var service = new Mock<IUsuarioService>();
        var mapper = new Mock<IMapper>();
        var rabbit = new Mock<IRabbitMqService>();

        TestHelper.SetupMapper<UsuarioViewModel, Usuario>(mapper, new Usuario());
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelper.InvalidResult());

        var controller = new UsuarioController(validator.Object, service.Object, mapper.Object, rabbit.Object);

        var result = await controller.Criar(new UsuarioViewModel());

        result.Should().BeOfType<BadRequestObjectResult>();
        rabbit.Verify(r => r.EnviarMensagem(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Criar_ShouldReturnBadRequest_WhenServiceReturnsFalse()
    {
        var validator = new Mock<IValidator<Usuario>>();
        var service = new Mock<IUsuarioService>();
        var mapper = new Mock<IMapper>();
        var rabbit = new Mock<IRabbitMqService>();

        var mapped = new Usuario { Username = "john", Password = "123" };
        TestHelper.SetupMapper<UsuarioViewModel, Usuario>(mapper, mapped);
        TestHelper.SetupValidatorSuccess(validator);
        service.Setup(s => s.CriarUsuario(It.IsAny<Usuario>())).ReturnsAsync(false);

        var controller = new UsuarioController(validator.Object, service.Object, mapper.Object, rabbit.Object);

        var result = await controller.Criar(new UsuarioViewModel());

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("Usuário já existe.");
        rabbit.Verify(r => r.EnviarMensagem(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Criar_ShouldReturnCreatedAndSendMessage_WhenSuccessful()
    {
        var validator = new Mock<IValidator<Usuario>>();
        var service = new Mock<IUsuarioService>();
        var mapper = new Mock<IMapper>();
        var rabbit = new Mock<IRabbitMqService>();

        var mapped = new Usuario { Username = "john", Password = "123" };
        TestHelper.SetupMapper<UsuarioViewModel, Usuario>(mapper, mapped);
        TestHelper.SetupValidatorSuccess(validator);
        service.Setup(s => s.CriarUsuario(It.IsAny<Usuario>())).ReturnsAsync(true);

        var controller = new UsuarioController(validator.Object, service.Object, mapper.Object, rabbit.Object);

        var result = await controller.Criar(new UsuarioViewModel());

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(UsuarioController.Criar));
        rabbit.Verify(r => r.EnviarMensagem("Usuário criado: john"), Times.Once);
    }

    [Fact]
    public async Task Editar_ShouldReturnNotFound_WhenUsuarioNaoEncontradoExceptionIsThrown()
    {
        var validator = new Mock<IValidator<Usuario>>();
        var service = new Mock<IUsuarioService>();
        var mapper = new Mock<IMapper>();
        var rabbit = new Mock<IRabbitMqService>();

        var mapped = new Usuario { Username = "john", Password = "123" };
        TestHelper.SetupMapper<UsuarioViewModel, Usuario>(mapper, mapped);
        TestHelper.SetupValidatorSuccess(validator);
        service
            .Setup(s => s.EditarUsuario(It.IsAny<Usuario>(), 1))
            .ThrowsAsync(new UsuarioNaoEncontradoException("Usuário não encontrado."));

        var controller = new UsuarioController(validator.Object, service.Object, mapper.Object, rabbit.Object);

        var result = await controller.Editar(new UsuarioViewModel(), 1);

        var notFound = result.Should().BeOfType<ObjectResult>().Subject;
        notFound.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Excluir_ShouldReturnNotFound_WhenServiceReturnsFalse()
    {
        var validator = new Mock<IValidator<Usuario>>();
        var service = new Mock<IUsuarioService>();
        var mapper = new Mock<IMapper>();
        var rabbit = new Mock<IRabbitMqService>();

        service.Setup(s => s.ExcluirUsuario(1)).ReturnsAsync(false);
        var controller = new UsuarioController(validator.Object, service.Object, mapper.Object, rabbit.Object);

        var result = await controller.Excluir(1);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.Value.Should().Be("Usuário não encontrado.");
        rabbit.Verify(r => r.EnviarMensagem(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Obter_ShouldReturnOk_WhenUserExists()
    {
        var validator = new Mock<IValidator<Usuario>>();
        var service = new Mock<IUsuarioService>();
        var mapper = new Mock<IMapper>();
        var rabbit = new Mock<IRabbitMqService>();

        var usuario = new Usuario { Id = 1, Username = "john", Password = "hash" };
        service.Setup(s => s.ObterUsuario(1)).ReturnsAsync(usuario);

        var controller = new UsuarioController(validator.Object, service.Object, mapper.Object, rabbit.Object);

        var result = await controller.Obter(1);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(usuario);
    }
}
