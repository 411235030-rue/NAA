namespace WEB_NAA.Services;

public sealed class UserSession
{
    public string? Account { get; private set; }

    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(Account);

    public void Login(string account)
    {
        Account = account.Trim();
    }

    public void Logout()
    {
        Account = null;
    }
}
