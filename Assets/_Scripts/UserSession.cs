public static class UserSession
{
    public static string UserId;
    public static string Username;
    public static string Email;

    public static bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(UserId);
    }

    public static void Clear()
    {
        UserId = "";
        Username = "";
        Email = "";
    }
}