using API_NAA_DDM.Configs;
using API_NAA_DDM.Dtos;
using API_NAA_DDM.Interfaces;
using ResponseModel;
using System.Text.Json;
using static ResponseModel.ResponseMapper;

namespace API_NAA_DDM.Services;

public class NaaHttpServices : INaaHttpServices
{
    private readonly HttpClient _client;
    private readonly ILogger<NaaHttpServices> _logger;

    public NaaHttpServices(HttpClient client, ILogger<NaaHttpServices> logger)
    {
        _client = client;
        _logger = logger;
    }

    public Task<ResponseModel<HistoryResponseDto>> SaveHistoryAsync(HistoryCreateDto dto)
    {
        return PostToNaaAsync<HistoryCreateDto, HistoryResponseDto>("/SaveHistory", dto);
    }

    public Task<ResponseModel<HistoryResponseDto>> GetHistoryByAccountAsync(HistoryQueryDto dto)
    {
        return PostToNaaAsync<HistoryQueryDto, HistoryResponseDto>("/GetHistoryByAccount", dto);
    }

    public Task<ResponseModel<UserQueryResponseDto>> GetUserByAccountAsync(UserQueryDto dto)
    {
        return Task.FromResult(LocalUserResponse(dto.Account, "Local user loaded"));
    }

    public Task<ResponseModel<UserQueryResponseDto>> UpdateUserAsync(UserQueryDto dto)
    {
        return Task.FromResult(LocalUserResponse(dto.Account, "Local user updated"));
    }

    public Task<ResponseModel<UserQueryResponseDto>> CreateUserAsync(UserQueryDto dto)
    {
        return Task.FromResult(LocalUserResponse(dto.Account, "Local user created"));
    }

    public Task<ResponseModel<string>> DeleteHistoryAsync(string uniqueId)
    {
        return Task.FromResult(new[] { uniqueId }.ToResponse("History deleted locally", "No history id supplied"));
    }

    public Task<ResponseModel<string>> ArchiveHistoryAsync(string uniqueId)
    {
        return Task.FromResult(new[] { uniqueId }.ToResponse("History archived locally", "No history id supplied"));
    }

    public async Task<string?> GenerateRevisedTextAsync(ReviseRequestDto reviseRequestDto)
    {
        var revisedText = BuildLocalRevisedText(reviseRequestDto.InputText);

        if (string.IsNullOrWhiteSpace(reviseRequestDto.Account) ||
            string.IsNullOrWhiteSpace(reviseRequestDto.InputText))
        {
            return revisedText;
        }

        var historyDto = new HistoryCreateDto
        {
            ThreadId = reviseRequestDto.ThreadId,
            Account = reviseRequestDto.Account,
            QuestionText = reviseRequestDto.InputText,
            AnswerText = revisedText,
            ChatTitle = string.IsNullOrWhiteSpace(reviseRequestDto.ChatTitle)
                ? reviseRequestDto.InputText
                : reviseRequestDto.ChatTitle,
            OriginCode = reviseRequestDto.OriginCode,
            EmployeeId = reviseRequestDto.EmployeeId
        };

        try
        {
            await _client.PostAsJsonAsync($"{NaaConfig.NaaServiceDomain}/SaveHistory", historyDto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Local revision succeeded, but history could not be saved.");
        }

        return revisedText;
    }

    private async Task<ResponseModel<TResponse>> PostToNaaAsync<TRequest, TResponse>(string path, TRequest dto)
    {
        try
        {
            var response = await _client.PostAsJsonAsync($"{NaaConfig.NaaServiceDomain}{path}", dto);

            if (!response.IsSuccessStatusCode)
            {
                return GenerateErrorResponse<TResponse>($"NAA API returned {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<ResponseModel<TResponse>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? GenerateErrorResponse<TResponse>("NAA API returned an empty response");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NAA API call failed: {Path}", path);
            return GenerateErrorResponse<TResponse>(ex.Message);
        }
    }

    private static ResponseModel<UserQueryResponseDto> LocalUserResponse(string? account, string description)
    {
        var user = new UserQueryResponseDto
        {
            UniqueId = string.IsNullOrWhiteSpace(account) ? Guid.NewGuid().ToString() : account,
            UserAccount1 = string.IsNullOrWhiteSpace(account) ? "local.user" : account,
            InsertDt = DateTime.Now,
            UpdateDt = DateTime.Now
        };

        return new[] { user }.ToResponse(description, "No local user data");
    }

    private static string BuildLocalRevisedText(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "請輸入要修正的文字。";
        }

        var normalized = input.Trim();
        return $"本機示範修正版：{normalized}";
    }
}
