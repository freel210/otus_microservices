using Gateway.ConfigOptions;
using Gateway.DTO.Income;
using Gateway.DTO.Outcome;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net;

namespace Gateway.Services
{
    public class AuthService(
        IHttpClientFactory httpClientFactory,
        IOptions<ApiPointsOptions> options,
        IKafkaService kafkaService) : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        private readonly IKafkaService _kafkaService = kafkaService;
        private readonly string _userCreatedTopic = "user-created";

        private readonly string _authServiceUrl = options.Value.AuthUrl!;
        private readonly string _demoServiceUrl = options.Value.DemoUrl!;

        private readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
        };

        public async Task RegisterUser(RegistrationRequest request)
        {
            using var authClient = _httpClientFactory.CreateClient();
            authClient.BaseAddress = new Uri(_authServiceUrl);

            using var response = await authClient.PostAsync(
                    "/register",
                    JsonContent.Create(request, new MediaTypeHeaderValue("application/json"), JsonSerializerOptions));

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var stream = await response.Content.ReadAsStreamAsync();
                    var userResponse = JsonSerializer.Deserialize<RegisterResponse>(stream, JsonSerializerOptions);
                    await CreateUser(userResponse!.UserId!.Value);

                    string message = JsonSerializer.Serialize(new 
                    { 
                        d = Guid.NewGuid().ToString(),
                        UserId = userResponse!.UserId!.Value 
                    });
                    await _kafkaService.Publish(_userCreatedTopic, message);

                    break;

                case HttpStatusCode.Conflict:
                    throw new ArgumentOutOfRangeException();

                default:
                    throw new Exception($"Received unexpected response status code '{response.StatusCode}'");
            }

            async Task CreateUser(Guid userId)
            {
                using var demoClient = _httpClientFactory.CreateClient();
                demoClient.BaseAddress = new Uri(_demoServiceUrl);

                UserAddRequest request = new(userId, null, null, null, null, null);
                string requestId = Guid.NewGuid().ToString();

                HttpRequestMessage requestMessage = new(HttpMethod.Post, "/user")
                {
                    Content = JsonContent.Create(request, new MediaTypeHeaderValue("application/json"))
                };
                requestMessage.Headers.Add("X-Request-Id", requestId);

                var response = await demoClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<TokensBundleResponse> LoginUser(RegistrationRequest request)
        {
            using var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_authServiceUrl);

            using var response = await client.PostAsync(
                    "/login",
                    JsonContent.Create(request, new MediaTypeHeaderValue("application/json"), JsonSerializerOptions));

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var responseContent = JsonSerializer.Deserialize<TokensBundleResponse>(await response.Content.ReadAsStringAsync(), JsonSerializerOptions);
                    return responseContent!;

                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedAccessException();

                case HttpStatusCode.NotFound:
                    throw new KeyNotFoundException();

                default:
                    throw new Exception($"Received unexpected response status code '{response.StatusCode}'");
            }
        }
    }
}
