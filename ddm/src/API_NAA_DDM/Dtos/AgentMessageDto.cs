namespace API_NAA_DDM.Dtos;

public sealed record AgentMessageRequest(
    string InputText,
    string UserId,
    string? AgentThreadId);

public sealed record AgentMessageResponse(
    string Answer,
    string? AgentThreadId);
