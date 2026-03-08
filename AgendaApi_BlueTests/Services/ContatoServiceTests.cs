using AgendaApi_Blue.Models;
using AgendaApi_Blue.Repositories.Interfaces;
using AgendaApi_Blue.Services;
using FluentAssertions;
using Moq;

namespace AgendaApi_BlueTests.Services;

public class ContatoServiceTests
{
    [Fact]
    public async Task CriarContato_ShouldCallRepositoryAndReturnResult()
    {
        var repository = new Mock<IContatoRepository>();
        repository.Setup(r => r.CriarContato(It.IsAny<Contato>())).ReturnsAsync(true);

        var service = new ContatoService(repository.Object);

        var result = await service.CriarContato(new Contato { Nome = "Ana" });

        result.Should().BeTrue();
        repository.Verify(r => r.CriarContato(It.IsAny<Contato>()), Times.Once);
    }

    [Fact]
    public async Task ObterContatosPorUsuario_ShouldReturnRepositoryResult()
    {
        var repository = new Mock<IContatoRepository>();
        var expected = new List<Contato?> { new() { Id = 1, Nome = "Ana" } };
        repository.Setup(r => r.ObterContatosPorUsuario(7)).ReturnsAsync(expected);

        var service = new ContatoService(repository.Object);

        var result = await service.ObterContatosPorUsuario(7);

        result.Should().BeEquivalentTo(expected);
    }
}
