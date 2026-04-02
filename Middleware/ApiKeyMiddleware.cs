namespace Middleware;

public class ApiKeyMiddleware
{
  private readonly RequestDelegate _next;
  private const string API_KEY_HEADER_NAME = "X-API-Key";

  public ApiKeyMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
  {
    if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
    {
      context.Response.StatusCode = 401; //Unauthorized
      await context.Response.WriteAsync("API Key was not provided.");
      return;
    }

    var expectedApiKey = configuration.GetValue<string>("ApiKeySettings:ApiKey");

    if (expectedApiKey != extractedApiKey)
    {
      context.Response.StatusCode = 401; //Unauthorized
      await context.Response.WriteAsync("Unauthorized client.");
      return;
    }

    await _next(context);
  }
}


