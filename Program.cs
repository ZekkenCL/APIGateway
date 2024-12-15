using DotNetEnv;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Google.Protobuf.WellKnownTypes;
using System.Text;
using CarrerasService;
using CarrerasService.Protos;

var builder = WebApplication.CreateBuilder(args);

// Cargar variables desde .env
Env.Load();
Console.WriteLine($"AUTHSERVICE_BASEADDRESS: {Environment.GetEnvironmentVariable("AUTHSERVICE_BASEADDRESS")}");

// Configurar autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = Environment.GetEnvironmentVariable("JWT_AUTHORITY");
        options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuthorizationMiddleware>();

// Endpoint para registrar usuarios
app.Map("/auth/register", async (HttpContext context) =>
{
    var baseAddress = Environment.GetEnvironmentVariable("AUTHSERVICE_BASEADDRESS");
    var targetUri = $"{baseAddress}/register";

    // Crear un nuevo HttpRequestMessage
    var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);

    // Copiar encabezados
    foreach (var header in context.Request.Headers)
    {
        if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    // Leer el cuerpo de la solicitud y asignarlo al HttpRequestMessage
    if (context.Request.ContentLength > 0)
    {
        using (var streamReader = new StreamReader(context.Request.Body))
        {
            var bodyContent = await streamReader.ReadToEndAsync();
            requestMessage.Content = new StringContent(bodyContent, Encoding.UTF8, context.Request.ContentType);
        }
    }

    // Enviar la solicitud al microservicio destino
    using var client = new HttpClient();
    var response = await client.SendAsync(requestMessage);

    // Leer el cuerpo de la respuesta
    var responseBody = await response.Content.ReadAsStringAsync();

    // Establecer el código de estado y el tipo de contenido de la respuesta
    context.Response.StatusCode = (int)response.StatusCode;
    context.Response.ContentType = "application/json";

    // Escribir la respuesta como JSON al cliente
    await context.Response.WriteAsync(responseBody);
});


app.MapPost("/auth/login", async (HttpContext context) =>
{
    var baseAddress = Environment.GetEnvironmentVariable("AUTHSERVICE_BASEADDRESS");
    var targetUri = $"{baseAddress}/login";

    Console.WriteLine($"Proxying request to: {targetUri}");

    // Crear la solicitud para reenviar
    var requestMessage = new HttpRequestMessage(HttpMethod.Post, targetUri);

    // Copiar encabezados de la solicitud
    foreach (var header in context.Request.Headers)
    {
        if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    // Leer el cuerpo de la solicitud
    if (context.Request.ContentLength > 0)
    {
        context.Request.EnableBuffering(); // Habilitar el rebobinado del cuerpo
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // Rebobinar el cuerpo de la solicitud
            requestMessage.Content = new StringContent(body, Encoding.UTF8, context.Request.ContentType);
        }
    }

    // Enviar la solicitud al AuthService
    using var client = new HttpClient();
    var response = await client.SendAsync(requestMessage);

    // Configurar el código de estado
    context.Response.StatusCode = (int)response.StatusCode;

    // Leer y devolver el contenido como JSON
    var responseBody = await response.Content.ReadAsStringAsync();

    // Establecer encabezados de contenido
    context.Response.ContentType = "application/json";

    await context.Response.WriteAsync(responseBody);
});




app.Map("/token/validate-token", async (HttpContext context) =>
{
    var baseAddress = Environment.GetEnvironmentVariable("JWT_AUTHORITY");
    var targetUri = $"{baseAddress}/validate-token";
    Console.WriteLine($"Proxying to: {targetUri}");

    var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);

    foreach (var header in context.Request.Headers)
    {
        if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    if (context.Request.ContentLength > 0)
    {
        using (var streamReader = new StreamReader(context.Request.Body))
        {
            var bodyContent = await streamReader.ReadToEndAsync();
            Console.WriteLine($"Request Body: {bodyContent}");
            requestMessage.Content = new StringContent(bodyContent, Encoding.UTF8, context.Request.ContentType);
        }
    }

    using var client = new HttpClient();
    var response = await client.SendAsync(requestMessage);

    context.Response.StatusCode = (int)response.StatusCode;

    var responseBody = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Response Body: {responseBody}");

    // Asegurarse de que la respuesta sea JSON
    context.Response.ContentType = "application/json";

    // Evitar problemas con headers redundantes
    if (!context.Response.Headers.ContainsKey("Content-Length"))
    {
        context.Response.Headers["Content-Length"] = responseBody.Length.ToString();
    }

    // Escribir la respuesta
    await context.Response.WriteAsync(responseBody);
});



app.Map("/token/revoke-token", async (HttpContext context) =>
{
    var baseAddress = Environment.GetEnvironmentVariable("JWT_AUTHORITY");
    var targetUri = $"{baseAddress}/revoke-token";

    Console.WriteLine($"Proxying to: {targetUri}");

    var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);

    foreach (var header in context.Request.Headers)
    {
        if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    if (context.Request.ContentLength > 0)
    {
        using (var streamReader = new StreamReader(context.Request.Body))
        {
            var bodyContent = await streamReader.ReadToEndAsync();
            Console.WriteLine($"Request Body: {bodyContent}");
            requestMessage.Content = new StringContent(bodyContent, Encoding.UTF8, context.Request.ContentType);
        }
    }

    using var client = new HttpClient();
    var response = await client.SendAsync(requestMessage);

    var responseBody = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Response Body: {responseBody}");

    // Configuración de la respuesta como JSON
    context.Response.StatusCode = (int)response.StatusCode;
    context.Response.ContentType = "application/json";

    await context.Response.WriteAsync(responseBody);
});


// Proxy para CarrerasService (gRPC)
app.Map("/carreras", async context =>
{
    var baseAddress = Environment.GetEnvironmentVariable("CARRERASSERVICE_BASEADDRESS");
    using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(baseAddress);

    var client = new Carreras.CarrerasClient(channel);
    var carrerasResponse = await client.GetAllAsync(new CarrerasService.Empty());

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(carrerasResponse);
});

// Proxy para SubjectsService (gRPC)
app.Map("/subjects", async context =>
{
    var baseAddress = Environment.GetEnvironmentVariable("SUBJECTSERVICE_BASEADDRESS");
    using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(baseAddress);

    var client = new SubjectsService.SubjectsServiceClient(channel);
    var subjectsResponse = await client.GetAllSubjectsAsync(new CarrerasService.Protos.Empty());

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(subjectsResponse);
});

app.Map("/subjects/prerequisites", async context =>
{
    var baseAddress = Environment.GetEnvironmentVariable("SUBJECTSERVICE_BASEADDRESS");
    using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(baseAddress);

    var client = new SubjectsService.SubjectsServiceClient(channel);
    var prerequisitesResponse = await client.GetAllPrerequisitesAsync(new CarrerasService.Protos.Empty());

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(prerequisitesResponse);
});

app.Map("/subjects/postrequisites", async context =>
{
    var baseAddress = Environment.GetEnvironmentVariable("SUBJECTSERVICE_BASEADDRESS");
    using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(baseAddress);

    var client = new SubjectsService.SubjectsServiceClient(channel);
    var postrequisitesResponse = await client.GetAllPostrequisitesAsync(new CarrerasService.Protos.Empty());

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(postrequisitesResponse);
});

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    Console.WriteLine($"Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}"))}");
    await next();
    Console.WriteLine($"Response Status Code: {context.Response.StatusCode}");
});

// Endpoint Placeholder /my-progress
app.MapGet("/my-progress", () =>
{
    return Results.Ok(new { message = "Listo" });
});

app.MapGet("/my-progress/add-subject", () =>
{
    return Results.Ok(new { message = "2 asignaturas agregadas" });
});

app.MapGet("/my-progress/remove-subject", () =>
{
    return Results.Ok(new { message = "1 asignatura eliminada" });
});




app.Run();