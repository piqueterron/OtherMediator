namespace OtherMediator.Integration.Tests.Specs;

using OtherMediator.Integration.Tests.Fixtures;
using Xunit;

[Trait("OtherMediator", "Integration")]
public class OtherMediatorShould(OtherMediatorFixture _fixture) : IClassFixture<OtherMediatorFixture>
{
    [Fact]
    public async Task Test1()
    {
        var server = _fixture.Server;
        var client = server.CreateClient();

        var response = await client.GetAsync("mediator");

        await Task.Delay(10000);

        Assert.NotNull("");
    }
}