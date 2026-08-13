using API_NAA_DDM.Dtos;
using API_NAA_DDM.Interfaces;

namespace API_NAA_DDM.Services;

public class LocalAgentService : IAgentService
{
    public Task<AgentMessageResponse> GenerateResponseAsync(
        AgentMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.InputText))
            return Task.FromResult(new AgentMessageResponse("請輸入問題。", request.AgentThreadId));

        return Task.FromResult(new AgentMessageResponse(
            $"本機示範回答：{request.InputText.Trim()}",
            request.AgentThreadId));
    }
}
