using AgendaApi_Blue.Exceptions;
using AgendaApi_Blue.Models;
using AgendaApi_Blue.Repositories.Interfaces;
using AgendaApi_Blue.Services;
using AgendaApi_Blue.Utilitaries;
using FluentAssertions;
using Moq;

namespace AgendaApi_BlueTests.Services;

public class UsuarioServiceTests
{
    [Fact]
    public async Task ValidarUsuario_ShouldHashPasswordBeforeCallingRepository()
    {
        var repository = new Mock<IUsuarioRepository>();
        Usuario? captured = null;

        repository
            .Setup(r => r.ValidarUsuario(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => captured = u)
            .ReturnsAsync(new Usuario { Id = 1, Username = "john", Password = "hash" });

        var service = new UsuarioService(repository.Object);
        var usuario = new Usuario { Username = "john", Password = "123456" };

        await service.ValidarUsuario(usuario);

        captured.Should().NotBeNull();
        captured!.Password.Should().Be(Utils.GerarHashSenha("123456"));
    }

    [Fact]
    public async Task CriarUsuario_ShouldHashPasswordBeforeCallingRepository()
    {
        var repository = new Mock<IUsuarioRepository>();
        Usuario? captured = null;

        repository
            .Setup(r => r.CriarUsuario(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => captured = u)
            .ReturnsAsync(true);

        var service = new UsuarioService(repository.Object);
        var usuario = new Usuario { Username = "john", Password = "123456" };

        await service.CriarUsuario(usuario);

        captured.Should().NotBeNull();
        captured!.Password.Should().Be(Utils.GerarHashSenha("123456"));
    }

    [Fact]
    public async Task EditarUsuario_ShouldThrow_WhenUserDoesNotExist()
    {
        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.ObterUsuario(1)).ReturnsAsync((Usuario?)null);

        var service = new UsuarioService(repository.Object);

        Func<Task> act = () => service.EditarUsuario(new Usuario(), 1);

        await act.Should().ThrowAsync<UsuarioNaoEncontradoException>();
    }

    [Fact]
    public async Task EditarUsuario_ShouldUpdateAndSave_WhenUserExists()
    {
        var repository = new Mock<IUsuarioRepository>();
        var existingUser = new Usuario { Id = 1, Username = "old", Password = "old", Role = Enums.Role.User };

        repository.Setup(r => r.ObterUsuario(1)).ReturnsAsync(existingUser);
        repository.Setup(r => r.EditarUsuario(It.IsAny<Usuario>())).ReturnsAsync(true);

        var service = new UsuarioService(repository.Object);
        var updated = new Usuario { Username = "new", Password = "123456", Role = Enums.Role.Admin };

        var result = await service.EditarUsuario(updated, 1);

        result.Should().BeTrue();
        existingUser.Username.Should().Be("new");
        existingUser.Password.Should().Be(Utils.GerarHashSenha("123456"));
        existingUser.Role.Should().Be(Enums.Role.Admin);
        repository.Verify(r => r.EditarUsuario(existingUser), Times.Once);
    }
}
