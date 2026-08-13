namespace API_NAA.Dtos.Input.Query;

public sealed class LoginRequestDto
{
    public string Account { get; set; } = null!;
    public string Password { get; set; } = null!;
}
