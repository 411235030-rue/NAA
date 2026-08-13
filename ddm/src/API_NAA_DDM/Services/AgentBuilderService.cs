using API_NAA_DDM.Configs;
using API_NAA_DDM.Dtos;
using API_NAA_DDM.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace API_NAA_DDM.Services;

public sealed class AgentBuilderService : IAgentService
{
    private readonly HttpClient _client;
    private readonly AgentOptions _options;
    private readonly ILogger<AgentBuilderService> _logger;

    public AgentBuilderService(
        HttpClient client,
        IOptions<AgentOptions> options,
        ILogger<AgentBuilderService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AgentMessageResponse> GenerateResponseAsync(
        AgentMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.InputText))
            throw new AgentServiceException("請輸入問題。");

        var payload = new AgentBuilderChatRequest
        {
            Query = request.InputText.Trim(),
            ConversationId = request.AgentThreadId ?? string.Empty,
            User = request.UserId,
            ResponseMode = "blocking"
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, _options.ChatPath)
        {
            Content = JsonContent.Create(payload)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        try
        {
            using var response = await _client.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "AgentBuilder request failed with HTTP {StatusCode}",
                    (int)response.StatusCode);
                throw new AgentServiceException($"醫院 Agent 回應 HTTP {(int)response.StatusCode}。");
            }

            var result = await response.Content.ReadFromJsonAsync<AgentBuilderChatResponse>(
                cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(result?.Answer))
                throw new AgentServiceException("醫院 Agent 沒有回傳回答內容。");

            return new AgentMessageResponse(result.Answer, result.ConversationId);
        }
        catch (AgentServiceException)
        {
            throw;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AgentServiceException("醫院 Agent 連線逾時。");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "AgentBuilder connection failed");
            throw new AgentServiceException("無法連線到醫院 Agent，請確認院內網路或 VPN 與 DNS。");
        }
    }

    private sealed class AgentBuilderChatRequest
    {
        [JsonPropertyName("inputs")]
        public Dictionary<string, object?> Inputs { get; init; } = new();

        [JsonPropertyName("query")]
        public string Query { get; init; } = string.Empty;

        [JsonPropertyName("response_mode")]
        public string ResponseMode { get; init; } = "blocking";

        [JsonPropertyName("conversation_id")]
        public string ConversationId { get; init; } = string.Empty;

        [JsonPropertyName("user")]
        public string User { get; init; } = string.Empty;
    }

    private sealed class AgentBuilderChatResponse
    {
        [JsonPropertyName("answer")]
        public string? Answer { get; init; }

        [JsonPropertyName("conversation_id")]
        public string? ConversationId { get; init; }
    }
}

public sealed class AgentServiceException(string message) : Exception(message);
