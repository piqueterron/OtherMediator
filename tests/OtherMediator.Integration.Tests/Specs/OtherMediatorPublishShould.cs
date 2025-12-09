namespace OtherMediator.Integration.Tests.Specs;

using System.Net.Http.Json;
using OtherMediator.Integration;
using OtherMediator.Integration.Tests.Fixtures;
using OtherMediator.Integration.Tests.Handlers;
using Xunit;

[Trait("OtherMediator", "Publish")]
[Collection(nameof(OtherMediatorFixtureCollection))]
public class OtherMediatorPublishShould
{
    private readonly HttpClient _httpClient;

    public OtherMediatorPublishShould(OtherMediatorFixture fixture)
    {
        _httpClient = fixture.Client;
    }

    [Fact(DisplayName = "When publishing a message to all subscribers, a request should be given that invokes all listeners and provides them with a copy of the message.")]
    public async Task GiveRequest_WhenPublishMessageWithManySubscribers_ShouldExecuteAllHandlersListenin()
    {
        _httpClient.PostAsJsonAsync("notification", new TestNotification { Message = "Test Notification" });

        var result = await MonitorManager.WaitAsync(2);

        Assert.True(result);
    }
}
