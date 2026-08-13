using API_NAA_DDM.Dtos;
using ResponseModel;

namespace API_NAA_DDM.Interfaces;

public interface INaaHttpServices
{
    Task<ResponseModel<LoginResponseDto>> AuthenticateUserAsync(LoginRequestDto dto);

    /// <summary>
    /// 儲存對話歷史紀錄
    /// </summary>
    Task<ResponseModel<HistoryResponseDto>> SaveHistoryAsync(HistoryCreateDto dto);

    /// <summary>
    /// 依帳號查詢對話歷史紀錄
    /// </summary>
    Task<ResponseModel<ConversationSummaryDto>> GetConversationSummariesAsync(HistoryQueryDto dto);

    Task<ResponseModel<HistoryResponseDto>> GetConversationByIdAsync(HistoryQueryDto dto);

    Task<ResponseModel<AgentContextDto>> GetAgentContextAsync(HistoryQueryDto dto);

    /// <summary>
    /// 依帳號查詢員工詳細資料
    /// </summary>
    Task<ResponseModel<UserQueryResponseDto>> GetUserByAccountAsync(UserQueryDto dto);

    /// <summary>
    /// 更新員工帳號相關資訊
    /// </summary>
    Task<ResponseModel<UserQueryResponseDto>> UpdateUserAsync(UserQueryDto dto);

    /// <summary>
    /// 新增員工
    /// </summary>
    Task<ResponseModel<UserQueryResponseDto>> CreateUserAsync(UserQueryDto dto);

    /// <summary>
    /// 刪除指定對話紀錄
    /// </summary>
    Task<ResponseModel<string>> SoftDeleteConversationAsync(ConversationMutationDto dto);

    /// <summary>
    /// 封存指定對話紀錄
    /// </summary>
    Task<ResponseModel<string>> RestoreConversationAsync(ConversationMutationDto dto);

    // 連agent builder 新增(純文字)
    Task<string?> GenerateRevisedTextAsync(ReviseRequestDto reviseRequestDto);

}

