namespace OtherMediator.Integration.Tests.Specs;

using OtherMediator.Integration.Tests.Fixtures;
using Xunit;

[Trait("OtherMediator", "Integration")]
[Collection(nameof(OtherMediatorFixtureCollection))]
public class OtherMediatorShould : IClassFixture<OtherMediatorFixture>
{
    private readonly HttpClient _httpClient;

    public OtherMediatorShould(OtherMediatorFixture fixture)
    {
        _httpClient = fixture.CreateApiServer().CreateClient();
    }

    [Fact]
    public async Task Test1()
    {
        await _httpClient.GetAsync("mediator");

        await Task.Delay(10000);

        Assert.NotNull("");
    }

    [Fact]
    public async Task Test2()
    {
        await _httpClient.GetAsync("mediator");

        await Task.Delay(10000);

        Assert.NotNull("");
    }
}