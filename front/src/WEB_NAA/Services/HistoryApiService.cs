using System.Net.Http.Json;
using System.Text.Json;

namespace WEB_NAA.Services;

public sealed class HistoryApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public HistoryApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<HistoryLoadResult> GetHistoryByAccountAsync(
        string account,
        CancellationToken cancellationToken = default)
    {
        var baseUriResult = GetBaseUri();
        if (baseUriResult.Uri is null)
            return HistoryLoadResult.Fail(baseUriResult.Error!);

        var endpoint = new Uri(baseUriResult.Uri, "/GetHistoryByAccount");
        var request = new HistoryQueryRequest
        {
            Account = account,
            OriginCode = "DDM"
        };

        try
        {
            var client = _httpClientFactory.CreateClient("DDM");
            using var response = await client.PostAsJsonAsync(endpoint, request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HistoryLoadResult.Fail(
                    $"GetHistoryByAccount HTTP {(int)response.StatusCode}: {body}",
                    (int)response.StatusCode);
            }

            var items = ParseHistory(body)
                .OrderByDescending(x => x.CreatedAt ?? DateTimeOffset.MinValue)
                .ToList();

            return HistoryLoadResult.Ok(items, (int)response.StatusCode);
        }
        catch (JsonException ex)
        {
            return HistoryLoadResult.Fail($"歷史紀錄回傳格式無法解析：{ex.Message}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return HistoryLoadResult.Fail($"無法透過 DDM 載入歷史紀錄：{ex.Message}");
        }
    }

    private (Uri? Uri, string? Error) GetBaseUri()
    {
        var baseUrl = _configuration["Ddm:BaseUrl"]
            ?? Environment.GetEnvironmentVariable("DDM_BASE_URL");

        if (string.IsNullOrWhiteSpace(baseUrl))
            return (null, "DDM BaseUrl 尚未設定。");

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
            return (null, $"DDM BaseUrl 格式不正確：{baseUrl}");

        return (uri, null);
    }

    private static string BuildTitle(string question)
    {
        var text = question.Trim();
        return text.Length <= 24 ? text : $"{text[..24]}…";
    }

    private static IReadOnlyList<HistoryRecord> ParseHistory(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return [];

        using var document = JsonDocument.Parse(body);
        var results = new List<HistoryRecord>();
        CollectHistory(document.RootElement, results);
        return results
            .GroupBy(x => x.ThreadId ?? x.UniqueId ?? $"legacy-{x.CreatedAt:O}")
            .Select(group =>
            {
                var turns = group
                    .OrderBy(x => x.CreatedAt ?? DateTimeOffset.MinValue)
                    .ToList();
                var first = turns[0];
                var latest = turns[^1];

                return new HistoryRecord
                {
                    UniqueId = group.Key,
                    ThreadId = group.Key,
                    Account = first.Account,
                    ChatTitle = first.ChatTitle,
                    QuestionText = latest.QuestionText,
                    AnswerText = latest.AnswerText,
                    OriginCode = first.OriginCode,
                    CreatedAt = latest.CreatedAt,
                    Turns = turns.Select(x => new HistoryTurn
                    {
                        UniqueId = x.UniqueId,
                        QuestionText = x.QuestionText,
                        AnswerText = x.AnswerText,
                        CreatedAt = x.CreatedAt
                    }).ToList()
                };
            })
            .ToList();
    }

    private static void CollectHistory(JsonElement element, List<HistoryRecord> results)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                CollectHistory(item, results);
            return;
        }

        if (element.ValueKind != JsonValueKind.Object)
            return;

        var question = GetString(element, "questionText", "question", "inputText", "content");
        var answer = GetString(element, "answerText", "answer", "responseText", "revisedText", "response");

        if (!string.IsNullOrWhiteSpace(question) || !string.IsNullOrWhiteSpace(answer))
        {
            results.Add(new HistoryRecord
            {
                UniqueId = GetString(element, "uniqueId", "historyId", "id", "uuid"),
                ThreadId = GetString(element, "threadId", "conversationThreadId"),
                Account = GetString(element, "account", "employeeId"),
                ChatTitle = GetString(element, "chatTitle", "title") ?? BuildTitle(question ?? "歷史對話"),
                QuestionText = question ?? string.Empty,
                AnswerText = answer ?? string.Empty,
                OriginCode = GetString(element, "originCode"),
                CreatedAt = GetDateTime(element, "createdAt", "createTime", "createdTime", "createDate", "createdDate", "insertTime")
            });
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind is JsonValueKind.Array or JsonValueKind.Object)
                CollectHistory(property.Value, results);
        }
    }

    private static string? GetString(JsonElement element, params string[] names)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (!names.Any(name => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                continue;

            return property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.GetRawText(),
                _ => null
            };
        }

        return null;
    }

    private static DateTimeOffset? GetDateTime(JsonElement element, params string[] names)
    {
        var value = GetString(element, names);
        return DateTimeOffset.TryParse(value, out var date) ? date : null;
    }
}

public sealed class HistoryQueryRequest
{
    public string Account { get; set; } = string.Empty;
    public string OriginCode { get; set; } = "DDM";
}

public sealed class HistoryRecord
{
    public string? UniqueId { get; set; }
    public string? ThreadId { get; set; }
    public string? Account { get; set; }
    public string ChatTitle { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public string? OriginCode { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public List<HistoryTurn> Turns { get; set; } = [];
}

public sealed class HistoryTurn
{
    public string? UniqueId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
}

public sealed record HistoryLoadResult(bool Success, IReadOnlyList<HistoryRecord> Items, int? StatusCode, string? Error)
{
    public static HistoryLoadResult Ok(IReadOnlyList<HistoryRecord> items, int? statusCode = null)
        => new(true, items, statusCode, null);

    public static HistoryLoadResult Fail(string error, int? statusCode = null)
        => new(false, [], statusCode, error);
}
