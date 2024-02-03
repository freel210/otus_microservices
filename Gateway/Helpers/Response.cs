using System.Text;
using System.Text.Json;

namespace Gateway.Helpers;

public class Responses
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static async Task SendJson(HttpContext context, object data, int statusCode = StatusCodes.Status200OK)
    {
        string text = JsonSerializer.Serialize(data, Options);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(text, Encoding.UTF8);
    }

    public static async Task Response401Unauthorized(HttpContext context) => await SendJson(
        context,
        new { },
        StatusCodes.Status401Unauthorized);
}
