namespace API_NAA.Dtos.Input.Create;

public class HistoryCreateDto
{
    public string? ThreadId { get; set; }
    public string Account { get; set; } = null!;
    public string QuestionText { get; set; } = null!;
    public string AnswerText { get; set; } = null!;
    public string? ChatTitle { get; set; }
    public string? OriginCode { get; set; }
}
