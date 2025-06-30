namespace OtherMediator.Integration.Tests.Specs;

using System.Net;
using System.Net.Http.Json;
using OtherMediator.Integration.Tests.Fixtures;
using OtherMediator.Integration.Tests.Handlers;
using Xunit;

[Trait("OtherMediator", "Integration")]
[Collection(nameof(OtherMediatorFixtureCollection))]
public class OtherMediatorShould
{
    private readonly HttpClient _httpClient;

    public OtherMediatorShould(OtherMediatorFixture fixture)
    {
        _httpClient = fixture.Client;
    }

    [Theory(DisplayName = "Give request when send command found correct handler, should return expected response")]
    [ClassData(typeof(TestRequestTheory))]
    public async Task GiveRequest_WhenSendCommandFoundHandler_ShouldReturnExpectedResponse(TestRequest request, HttpStatusCode httpStatusCodeExpected, string valueExpected)
    {
        var response = await _httpClient.PostAsJsonAsync("mediator", request);

        Assert.Equal(httpStatusCodeExpected, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<TestResponse>();

        Assert.NotNull(result);
        Assert.Equal(valueExpected, result.Value);
        Assert.True(result.Check);
    }
}

public class TestRequestTheory : TheoryData<TestRequest, HttpStatusCode, string>
{
    public TestRequestTheory()
    {
        Add(new TestRequest { Value = "TestA" }, HttpStatusCode.OK, "TestA");
        Add(new TestRequest { Value = "TestB" }, HttpStatusCode.OK, "TestB");
    }
}