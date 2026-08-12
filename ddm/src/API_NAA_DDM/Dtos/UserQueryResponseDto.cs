namespace API_NAA_DDM.Dtos
{
    public class UserQueryResponseDto
    {
        public string UniqueId { get; set; } = null!;
        public DateTime? InsertDt { get; set; } 
        public string? InsertOp { get; set; } 
        public DateTime? UpdateDt { get; set; }
        public string? UpdateOp { get; set; } 
        public string? UserAccount1 { get; set; } 
      
    }
}
