namespace API_NAA_DDM.Dtos;

public sealed class LoginRequestDto
{
    public string Account { get; set; } = null!;
    public string Password { get; set; } = null!;
}
