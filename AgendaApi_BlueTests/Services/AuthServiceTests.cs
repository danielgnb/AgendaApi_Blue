using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AgendaApi_Blue.Models;
using AgendaApi_Blue.Models.DTOs.Auth;
using AgendaApi_Blue.Repositories.Interfaces;
using AgendaApi_Blue.Services;
using AgendaApi_Blue.Utilitaries;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AgendaApi_BlueTests.Services;

public class AuthServiceTests
{
    [Fact]
    public void GerarToken_ShouldReturnAccessAndRefreshTokens_WithExpectedClaims()
    {
        var repository = new Mock<IAuthRepository>();
        var service = CreateService("Development", repository.Object);

        var (accessToken, refreshToken) = service.GerarToken("john", 99, Enums.Role.Admin);

        accessToken.Should().NotBeNullOrWhiteSpace();
        refreshToken.Should().NotBeNullOrWhiteSpace();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        jwt.Claims.First(c => c.Type == "UsuarioId").Value.Should().Be("99");
        jwt.Claims.Should().Contain(c =>
            c.Value == "Admin" &&
            (c.Type == "role" || c.Type == ClaimTypes.Role));
    }

    [Fact]
    public void ObterPorRefreshToken_ShouldHashTokenInProduction()
    {
        var repository = new Mock<IAuthRepository>();
        Login? captured = null;

        repository
            .Setup(r => r.ObterPorRefreshToken(It.IsAny<Login>()))
            .Callback<Login>(l => captured = l)
            .Returns(new Login());

        var service = CreateService("Production", repository.Object);

        service.ObterPorRefreshToken("raw-token");

        captured.Should().NotBeNull();
        captured!.RefreshToken.Should().Be(Utils.GerarHashToken("raw-token"));
    }

    [Fact]
    public void ObterPorRefreshToken_ShouldNotHashTokenInDevelopment()
    {
        var repository = new Mock<IAuthRepository>();
        Login? captured = null;

        repository
            .Setup(r => r.ObterPorRefreshToken(It.IsAny<Login>()))
            .Callback<Login>(l => captured = l)
            .Returns(new Login());

        var service = CreateService("Development", repository.Object);

        service.ObterPorRefreshToken("raw-token");

        captured.Should().NotBeNull();
        captured!.RefreshToken.Should().Be("raw-token");
    }

    [Fact]
    public async Task RegistrarAcesso_ShouldHashTokensInProduction()
    {
        var repository = new Mock<IAuthRepository>();
        Login? captured = null;

        repository
            .Setup(r => r.RegistrarAcesso(It.IsAny<Login>()))
            .Callback<Login>(l => captured = l)
            .Returns(Task.CompletedTask);

        var service = CreateService("Production", repository.Object);

        await service.RegistrarAcesso(new AcessoDTO
        {
            IdUsuario = 7,
            AccessToken = "access",
            RefreshToken = "refresh"
        });

        captured.Should().NotBeNull();
        captured!.IdUsuario.Should().Be(7);
        captured.AccessToken.Should().Be(Utils.GerarHashToken("access"));
        captured.RefreshToken.Should().Be(Utils.GerarHashToken("refresh"));
    }

    [Fact]
    public async Task RegistrarAcesso_ShouldKeepTokensRawInDevelopment()
    {
        var repository = new Mock<IAuthRepository>();
        Login? captured = null;

        repository
            .Setup(r => r.RegistrarAcesso(It.IsAny<Login>()))
            .Callback<Login>(l => captured = l)
            .Returns(Task.CompletedTask);

        var service = CreateService("Development", repository.Object);

        await service.RegistrarAcesso(new AcessoDTO
        {
            IdUsuario = 7,
            AccessToken = "access",
            RefreshToken = "refresh"
        });

        captured.Should().NotBeNull();
        captured!.AccessToken.Should().Be("access");
        captured.RefreshToken.Should().Be("refresh");
    }

    private static AuthService CreateService(string environmentName, IAuthRepository repository)
    {
        var inMemoryConfig = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "my-super-secret-key-1234567890-abc",
            ["Jwt:Issuer"] = "AgendaApi",
            ["Jwt:Audience"] = "AgendaApiUsers"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        var env = new Mock<IWebHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns(environmentName);

        return new AuthService(configuration, env.Object, repository);
    }
}

