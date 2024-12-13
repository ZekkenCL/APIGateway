using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Endpoints protegidos
        var protectedEndpoints = new[] { "/carreras", "/subjects", "/subjects/prerequisites", "/subjects/postrequisites" };

        // Validar si la solicitud es a un endpoint protegido
        if (protectedEndpoints.Any(endpoint => context.Request.Path.StartsWithSegments(endpoint)))
        {
            // Obtén el token del encabezado Authorization
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("{\"error\": \"Unauthorized: Token is missing\"}");
                return;
            }

            // Valida el token llamando al microservicio de autenticación
            using var client = new HttpClient();
            var validateTokenUrl = Environment.GetEnvironmentVariable("JWT_AUTHORITY") + "/validate-token";

            var response = await client.PostAsJsonAsync(validateTokenUrl, new { token });

            if (!response.IsSuccessStatusCode)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("{\"error\": \"Unauthorized: Invalid token\"}");
                return;
            }
        }

        // Si el token es válido, continuar con la solicitud
        await _next(context);
    }
}
