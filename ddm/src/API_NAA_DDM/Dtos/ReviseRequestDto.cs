namespace API_NAA_DDM.Dtos;

public class ReviseRequestDto
{
    public string? ConversationId { get; set; }
    public string? ChatTitle { get; set; }
    public string InputText { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string OriginCode { get; set; } = "DDM";
}
