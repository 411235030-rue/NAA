namespace API_NAA_DDM.Dtos;

public class HistoryCreateDto
{
    public string? ConversationId { get; set; }
    public string? AgentThreadId { get; set; }
    public string Account { get; set; } = null!;
    public string QuestionText { get; set; } = null!;
    public string AnswerText { get; set; } = null!;
    public string? ChatTitle { get; set; }
    public string? OriginCode { get; set; }
}
