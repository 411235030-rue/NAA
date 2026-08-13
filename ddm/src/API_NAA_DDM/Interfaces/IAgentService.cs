using API_NAA_DDM.Dtos;

namespace API_NAA_DDM.Interfaces;

public interface IAgentService
{
    Task<AgentMessageResponse> GenerateResponseAsync(
        AgentMessageRequest request,
        CancellationToken cancellationToken = default);
}
