using Gateway.ConfigOptions;
using Gateway.DTO.Income;
using Gateway.DTO.Outcome;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gateway.Services;

public class UserService(IHttpClientFactory httpClientFactory, IOptions<ApiPointsOptions> options) : IUserService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly string _demoServiceUrl = options.Value.DemoUrl!;

    private readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task<UserResponse> Get(Guid id)
    {
        using var demoClient = _httpClientFactory.CreateClient();
        demoClient.BaseAddress = new Uri(_demoServiceUrl);

        var response = await demoClient.GetAsync($"/user/{id}");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(stream, JsonSerializerOptions);

        return userResponse!;
    }

    public async Task Update(UserDemoUpdateRequest request)
    {
        using var demoClient = _httpClientFactory.CreateClient();
        demoClient.BaseAddress = new Uri(_demoServiceUrl);

        var response = await demoClient.PutAsync(
            "/user",
            JsonContent.Create(request, new MediaTypeHeaderValue("application/json")));

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new ArgumentOutOfRangeException();
        }

        response.EnsureSuccessStatusCode();
    }
}
