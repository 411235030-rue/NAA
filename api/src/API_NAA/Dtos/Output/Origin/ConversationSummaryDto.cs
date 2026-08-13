namespace API_NAA.Dtos.Output.Origin;

public class ConversationSummaryDto
{
    public string ConversationId { get; set; } = null!;
    public string? Account { get; set; }
    public string? ChatTitle { get; set; }
    public string? LastQuestionText { get; set; }
    public string? LastAnswerText { get; set; }
    public string? OriginCode { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int TurnCount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
