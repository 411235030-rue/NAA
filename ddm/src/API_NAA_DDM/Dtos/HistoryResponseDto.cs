namespace API_NAA_DDM.Dtos;

public class HistoryResponseDto
{
    public string UniqueId { get; set; } = null!;
    public string ConversationId { get; set; } = null!;
    public string? Account { get; set; }
    public string? ChatTitle { get; set; }
    public string? QuestionText { get; set; }
    public string? AnswerText { get; set; }
    public string? OriginCode { get; set; }
    public DateTime? InsertDt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
