using API_NAA.Dtos.Input.Create;
using API_NAA.Dtos.Input.Query;
using API_NAA.Dtos.Input.Update;
using API_NAA.Dtos.Output.Origin;
using ResponseModel;

namespace API_NAA.Interfaces;

public interface IDbServices
{
    Task<ResponseModel<LoginResponseDto>> AuthenticateUserAsync(LoginRequestDto dto);
    Task<ResponseModel<HistoryResponseDto>> SaveHistoryAsync(HistoryCreateDto dto);
    Task<ResponseModel<ConversationSummaryDto>> GetConversationSummariesAsync(HistoryQueryDto dto);
    Task<ResponseModel<HistoryResponseDto>> GetConversationByIdAsync(HistoryQueryDto dto);
    Task<ResponseModel<AgentContextDto>> GetAgentContextAsync(HistoryQueryDto dto);
    Task<ResponseModel<string>> SoftDeleteConversationAsync(ConversationMutationDto dto);
    Task<ResponseModel<string>> RestoreConversationAsync(ConversationMutationDto dto);
}
