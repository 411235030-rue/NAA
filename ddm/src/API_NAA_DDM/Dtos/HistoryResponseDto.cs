namespace API_NAA_DDM.Dtos;

public class HistoryResponseDto
{
    public string UniqueId { get; set; } = null!;
    public string? ThreadId { get; set; }
    public string? Account { get; set; }
    public string? ChatTitle { get; set; }
    public string? QuestionText { get; set; }
    public string? AnswerText { get; set; }
    public string? OriginCode { get; set; }
    public DateTime? InsertDt { get; set; }
    public string? EmployeeId { get; set; }
}
