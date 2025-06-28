namespace OtherMediator.Integration.Tests.Specs;

using OtherMediator.Integration.Tests.Fixtures;
using Xunit;

[Trait("OtherMediator", "Integration")]
public class OtherMediatorShould(OtherMediatorFixture fixture) : IClassFixture<OtherMediatorFixture>
{
    private readonly HttpClient _httpClient = fixture.ApiServer().CreateClient();

    [Fact]
    public async Task Test1()
    {
        var response = await _httpClient.GetAsync("mediator");

        Assert.NotNull(response);
    }
}