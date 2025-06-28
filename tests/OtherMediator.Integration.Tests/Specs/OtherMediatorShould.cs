namespace OtherMediator.Integration.Tests.Specs;

using OtherMediator.Integration.Tests.Fixtures;
using Xunit;

[Trait("OtherMediator", "Integration")]
public class OtherMediatorShould : IClassFixture<OtherMediatorFixture>
{
    private readonly HttpClient _httpClient;

    public OtherMediatorShould(OtherMediatorFixture fixture)
    {
        _httpClient = fixture.ApiServer().CreateClient();
    }

    [Fact]
    public async Task Test1()
    {
        await Task.Delay(10000);

        var response = await _httpClient.GetAsync("mediator");

        Assert.NotNull(response);
    }
}
