using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var ddmBaseUrl = builder.Configuration["Ddm:BaseUrl"]
    ?? Environment.GetEnvironmentVariable("DDM_BASE_URL")
    ?? "http://localhost:7079";

if (!Uri.TryCreate(ddmBaseUrl, UriKind.Absolute, out var ddmBaseUri))
    throw new InvalidOperationException($"Ddm:BaseUrl 不是有效網址：{ddmBaseUrl}");

builder.Services.AddHttpClient("DDM", client =>
{
    client.BaseAddress = ddmBaseUri;
    client.Timeout = TimeSpan.FromSeconds(20);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/ddm/{endpoint}", async (
    string endpoint,
    HttpRequest incomingRequest,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
{
    if (endpoint is not ("ReviseText" or "GetHistoryByAccount"))
        return Results.NotFound();

    try
    {
        using var outgoingRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StreamContent(incomingRequest.Body)
        };

        if (!string.IsNullOrWhiteSpace(incomingRequest.ContentType))
        {
            outgoingRequest.Content.Headers.ContentType =
                MediaTypeHeaderValue.Parse(incomingRequest.ContentType);
        }

        var client = httpClientFactory.CreateClient("DDM");
        using var response = await client.SendAsync(outgoingRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var mediaType = response.Content.Headers.ContentType?.MediaType ?? "application/json";

        return Results.Content(
            body,
            mediaType,
            Encoding.UTF8,
            (int)response.StatusCode);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        // Browser refreshes and navigation abort the in-flight proxy request.
        // Treat that as a normal client disconnect instead of an application error.
        return Results.StatusCode(499);
    }
    catch (TaskCanceledException)
    {
        return Results.Problem(
            title: "DDM 回應逾時",
            detail: "請確認 DDM 服務已在設定的網址啟動。",
            statusCode: StatusCodes.Status504GatewayTimeout);
    }
    catch (HttpRequestException exception)
    {
        return Results.Problem(
            title: "無法連線至 DDM",
            detail: exception.Message,
            statusCode: StatusCodes.Status502BadGateway);
    }
});

app.MapFallbackToFile("index.html");

app.Run();
