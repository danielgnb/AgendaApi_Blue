using AgendaApi_Blue.Utilitaries;
using FluentAssertions;

namespace AgendaApi_BlueTests.Utilitaries;

public class UtilsTests
{
    [Fact]
    public void GerarHashSenha_ShouldBeDeterministic()
    {
        var hash1 = Utils.GerarHashSenha("123456");
        var hash2 = Utils.GerarHashSenha("123456");

        hash1.Should().Be(hash2);
        hash1.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GerarHashToken_ShouldGenerateDifferentHashes_ForDifferentInputs()
    {
        var hash1 = Utils.GerarHashToken("token-a");
        var hash2 = Utils.GerarHashToken("token-b");

        hash1.Should().NotBe(hash2);
    }
}
