using System.Security.Claims;
using AgendaApi_Blue.Controllers;
using AgendaApi_Blue.Models;
using AgendaApi_Blue.Models.DTOs.Auth;
using AgendaApi_Blue.Models.ViewModels.Auth;
using AgendaApi_Blue.Services.Interfaces;
using AgendaApi_Blue.Utilitaries;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AgendaApi_BlueTests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenValidationFails()
    {
        var authService = new Mock<IAuthService>();
        var usuarioService = new Mock<IUsuarioService>();
        var validator = new Mock<IValidator<Usuario>>();
        var mapper = new Mock<IMapper>();

        TestHelper.SetupMapper<LoginViewModel, Usuario>(mapper, new Usuario());
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelper.InvalidResult("Username", "Obrigatório"));

        var controller = new AuthController(authService.Object, validator.Object, usuarioService.Object, mapper.Object);

        var result = await controller.Login(new LoginViewModel());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var authService = new Mock<IAuthService>();
        var usuarioService = new Mock<IUsuarioService>();
        var validator = new Mock<IValidator<Usuario>>();
        var mapper = new Mock<IMapper>();

        var mappedUser = new Usuario { Username = "john", Password = "123" };
        TestHelper.SetupMapper<LoginViewModel, Usuario>(mapper, mappedUser);
        TestHelper.SetupValidatorSuccess(validator);
        usuarioService.Setup(s => s.ValidarUsuario(It.IsAny<Usuario>())).ReturnsAsync((Usuario?)null);

        var controller = new AuthController(authService.Object, validator.Object, usuarioService.Object, mapper.Object);

        var result = await controller.Login(new LoginViewModel());

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.Value.Should().Be("Credenciais inválidas.");
    }

    [Fact]
    public async Task Login_ShouldReturnOkAndRegisterAccess_WhenCredentialsAreValid()
    {
        var authService = new Mock<IAuthService>();
        var usuarioService = new Mock<IUsuarioService>();
        var validator = new Mock<IValidator<Usuario>>();
        var mapper = new Mock<IMapper>();

        var mappedUser = new Usuario { Username = "john", Password = "123" };
        var validUser = new Usuario { Id = 10, Username = "john", Role = Enums.Role.Admin };

        TestHelper.SetupMapper<LoginViewModel, Usuario>(mapper, mappedUser);
        TestHelper.SetupValidatorSuccess(validator);
        usuarioService.Setup(s => s.ValidarUsuario(It.IsAny<Usuario>())).ReturnsAsync(validUser);
        authService.Setup(s => s.GerarToken(validUser.Username, validUser.Id, validUser.Role)).Returns(("access", "refresh"));
        authService.Setup(s => s.RegistrarAcesso(It.IsAny<AcessoDTO>())).Returns(Task.CompletedTask);

        var controller = new AuthController(authService.Object, validator.Object, usuarioService.Object, mapper.Object);

        var result = await controller.Login(new LoginViewModel());

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new { accessToken = "access", refreshToken = "refresh" });

        authService.Verify(
            s => s.RegistrarAcesso(It.Is<AcessoDTO>(a =>
                a.IdUsuario == validUser.Id &&
                a.AccessToken == "access" &&
                a.RefreshToken == "refresh")),
            Times.Once);
    }

    [Fact]
    public async Task Refresh_ShouldReturnUnauthorized_WhenRefreshTokenIsInvalid()
    {
        var authService = new Mock<IAuthService>();
        var usuarioService = new Mock<IUsuarioService>();
        var validator = new Mock<IValidator<Usuario>>();
        var mapper = new Mock<IMapper>();

        authService.Setup(s => s.ObterPorRefreshToken("invalid")).Returns((Login?)null);

        var controller = new AuthController(authService.Object, validator.Object, usuarioService.Object, mapper.Object);

        var result = await controller.Refresh(new TokenRefreshRequest { RefreshToken = "invalid" });

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.Value.Should().Be("Refresh token inválido ou expirado.");
    }

    [Fact]
    public async Task Refresh_ShouldReturnConflict_WhenAccessTokenStillValid()
    {
        var authService = new Mock<IAuthService>();
        var usuarioService = new Mock<IUsuarioService>();
        var validator = new Mock<IValidator<Usuario>>();
        var mapper = new Mock<IMapper>();

        authService.Setup(s => s.ObterPorRefreshToken("refresh")).Returns(new Login
        {
            IdUsuario = 1,
            AccessTokenExpiration = DateTime.Now.AddMinutes(5),
            RefreshTokenExpiration = DateTime.Now.AddDays(1),
            Usuario = new Usuario { Username = "john", Role = Enums.Role.User }
        });

        var controller = new AuthController(authService.Object, validator.Object, usuarioService.Object, mapper.Object);

        var result = await controller.Refresh(new TokenRefreshRequest { RefreshToken = "refresh" });

        var conflict = result.Should().BeOfType<ObjectResult>().Subject;
        conflict.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Refresh_ShouldReturnOkAndRegisterNewTokens_WhenRefreshIsValid()
    {
        var authService = new Mock<IAuthService>();
        var usuarioService = new Mock<IUsuarioService>();
        var validator = new Mock<IValidator<Usuario>>();
        var mapper = new Mock<IMapper>();

        authService.Setup(s => s.ObterPorRefreshToken("refresh")).Returns(new Login
        {
            IdUsuario = 1,
            AccessTokenExpiration = DateTime.Now.AddSeconds(10),
            RefreshTokenExpiration = DateTime.Now.AddDays(1),
            Usuario = new Usuario { Username = "john", Role = Enums.Role.User }
        });
        authService.Setup(s => s.GerarToken("john", 1, Enums.Role.User)).Returns(("new-access", "new-refresh"));
        authService.Setup(s => s.RegistrarAcesso(It.IsAny<AcessoDTO>())).Returns(Task.CompletedTask);

        var controller = new AuthController(authService.Object, validator.Object, usuarioService.Object, mapper.Object);

        var result = await controller.Refresh(new TokenRefreshRequest { RefreshToken = "refresh" });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new { accessToken = "new-access", refreshToken = "new-refresh" });

        authService.Verify(
            s => s.RegistrarAcesso(It.Is<AcessoDTO>(a =>
                a.IdUsuario == 1 &&
                a.AccessToken == "new-access" &&
                a.RefreshToken == "new-refresh")),
            Times.Once);
    }
}
