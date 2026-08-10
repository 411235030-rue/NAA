using System.Net.Http.Json;
using System.Text.Json;

namespace WEB_NAA.Services;

public sealed class DdmApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public DdmApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<DdmCallResult> ReviseTextAsync(
        string inputText,
        string account,
        string threadId,
        string chatTitle,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["Ddm:BaseUrl"]
            ?? Environment.GetEnvironmentVariable("DDM_BASE_URL");

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return DdmCallResult.NotSent("DDM BaseUrl 尚未設定。");
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            return DdmCallResult.NotSent($"DDM BaseUrl 格式不正確：{baseUrl}");
        }

        var endpoint = new Uri(baseUri, "/ReviseText");

        // 對應 DDM 專案目前的 ReviseRequestDto：
        // InputText / Account / EmployeeId / OriginCode / AgentCode
        var request = new DdmReviseRequest
        {
            ThreadId = threadId,
            ChatTitle = chatTitle,
            InputText = inputText,
            Account = account,
            EmployeeId = account,
            OriginCode = "DDM",
            AgentCode = "Local"
        };

        try
        {
            var client = _httpClientFactory.CreateClient("DDM");
            using var response = await client.PostAsJsonAsync(endpoint, request, cancellationToken);
            var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var answer = TryExtractAnswer(rawBody);

            return new DdmCallResult(
                RequestSent: true,
                HttpSuccess: response.IsSuccessStatusCode,
                StatusCode: (int)response.StatusCode,
                Answer: answer,
                RawBody: rawBody,
                Error: response.IsSuccessStatusCode
                    ? null
                    : $"DDM 回傳 HTTP {(int)response.StatusCode}: {rawBody}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return DdmCallResult.NotSent($"無法連線到 DDM：{ex.Message}");
        }
    }

    private static string? TryExtractAnswer(string rawBody)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
            return null;

        try
        {
            using var document = JsonDocument.Parse(rawBody);
            return FindAnswer(document.RootElement);
        }
        catch (JsonException)
        {
            return rawBody.Trim().Trim('"');
        }
    }

    private static string? FindAnswer(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var value = element.GetString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            string[] preferredNames =
            [
                "revisedText", "answer", "response", "result", "output", "message", "data"
            ];

            foreach (var name in preferredNames)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (!property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var found = FindAnswer(property.Value);
                    if (!string.IsNullOrWhiteSpace(found))
                        return found;
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                var found = FindAnswer(property.Value);
                if (!string.IsNullOrWhiteSpace(found))
                    return found;
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var found = FindAnswer(item);
                if (!string.IsNullOrWhiteSpace(found))
                    return found;
            }
        }

        return null;
    }
}

public sealed class DdmReviseRequest
{
    public string ThreadId { get; set; } = string.Empty;
    public string ChatTitle { get; set; } = string.Empty;
    public string InputText { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string OriginCode { get; set; } = "DDM";
    public string AgentCode { get; set; } = "Local";
}

public sealed record DdmCallResult(
    bool RequestSent,
    bool HttpSuccess,
    int? StatusCode,
    string? Answer,
    string? RawBody,
    string? Error)
{
    public static DdmCallResult NotSent(string error)
        => new(false, false, null, null, null, error);
}
