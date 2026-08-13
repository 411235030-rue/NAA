namespace API_NAA.Dtos.Input.Query;

public class HistoryQueryDto
{
    public string? Account { get; set; }
    public string? ConversationId { get; set; }
    public string? OriginCode { get; set; }
    public bool IsDeleted { get; set; }
    public bool IncludeDeleted { get; set; }
}
