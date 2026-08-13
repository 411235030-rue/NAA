namespace API_NAA_DDM.Dtos;

public class HistoryQueryDto
{
    public string? Account { get; set; }
    public string? ConversationId { get; set; }
    public string? OriginCode { get; set; }
    public bool IsDeleted { get; set; }
    public bool IncludeDeleted { get; set; }
}
