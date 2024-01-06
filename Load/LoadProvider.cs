using Load.DTO.Income;
using Load.DTO.Outcome;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Load;
public class LoadProvider(int index, string baseUrl, CancellationToken token)
{
    private readonly HttpClient _httpClient = new HttpClient()
    {
        BaseAddress = new Uri(baseUrl),
    };

    private readonly Random _random = new Random();
    private readonly CancellationToken _token = token;
    private readonly int _index = index;

    public async Task GenerateLoad()
    {
        while(!_token.IsCancellationRequested)
        {
            await ImitateDelay();
            
            var (id, versionId) = await Create();
            await Get(id);
            await Edit(id, versionId);
            await Delete(id);
        }

        WriteToConsole("Done.");
    }

    private async Task GetAll()
    {
        while (true)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.GetAsync("/users");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                if (ex is not HttpRequestException)
                {
                    WriteToConsole($"{ex.GetType()}: {ex.Message}");
                }

                if (ex is HttpRequestException)
                {
                    continue;
                }
            }
            finally
            {
                if (response != null)
                {
                    WriteToConsole($"GetAll: {response.StatusCode}");
                }
            }

            response?.Dispose();
            break;
        }
    }

    private async Task Get(string id)
    {
        while (true)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.GetAsync($"/user/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                if (ex is not HttpRequestException)
                {
                    WriteToConsole($"{ex.GetType()}: {ex.Message}");
                }

                if (ex is HttpRequestException)
                {
                    continue;
                }
            }
            finally
            {
                if (response != null)
                {
                    WriteToConsole($"Get: {response.StatusCode}");
                }
            }

            response?.Dispose();
            break;
        }
    }

    private async Task Delete(string id)
    {
        while (true)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.DeleteAsync($"/user/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                if (ex is not HttpRequestException)
                {
                    WriteToConsole($"{ex.GetType()}: {ex.Message}");
                }

                if (ex is HttpRequestException)
                {
                    continue;
                }
            }
            finally
            {
                if (response != null)
                {
                    WriteToConsole($"Delete: {response.StatusCode}");
                }
            }

            response?.Dispose();
            break;
        }
    }

    private async Task Edit(string id, string versionId)
    {
        UserUpdateRequest request = new(
            id,
            versionId,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString());

        while (true)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.PutAsync(
                    "/user",
                    JsonContent.Create(request, new MediaTypeHeaderValue("application/json")));

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                if (ex is not HttpRequestException)
                {
                    WriteToConsole($"{ex.GetType()}: {ex.Message}");
                }

                if (ex is HttpRequestException)
                {
                    WriteToConsole($"{ex.GetType()}: {ex.Message}");
                    continue;
                }
            }
            finally
            {
                if (response != null)
                {
                    WriteToConsole($"Edit: {response.StatusCode}");
                }
            }

            response?.Dispose();
            break;
        }
    }

    private async Task<(string, string)> Create()
    {
        UserAddRequest request = new(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString());
        
        string requestId = Guid.NewGuid().ToString();

        while (true)
        {
            HttpResponseMessage? response = null;
            try
            {
                HttpRequestMessage requestMessage = new(HttpMethod.Post, "/user")
                {
                    Content = JsonContent.Create(request, new MediaTypeHeaderValue("application/json"))
                };
                requestMessage.Headers.Add("X-Request-Id", requestId);   

                response = await _httpClient.SendAsync(requestMessage);

                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                var userResponse = JsonSerializer.Deserialize<UserResponse>(stream, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                string id = userResponse!.Id!.ToString()!;
                string versionId = userResponse!.VersionId!.ToString()!;
                return (id, versionId);
            }
            catch (Exception ex)
            {
                if (ex is not HttpRequestException)
                {
                    WriteToConsole($"{ex.GetType()}: {ex.Message}");
                }

                continue;
            }
            finally
            {
                if (response != null)
                {
                    WriteToConsole($"Create: {response.StatusCode}");
                    response.Dispose();
                }
            }
        }
    }

    private void WriteToConsole(string message) => ConsoleWriter.WriteLine($"{_index}: {message}");

    private async Task ImitateDelay()
    {
        var value = _random.Next(10, 101);
        await Task.Delay(value);
    }
}
