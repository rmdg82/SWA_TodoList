namespace Client.Constants;

public static class AuthEndpoint
{
    public const string Me = "/.auth/me";

    public const string Logout = "/.auth/logout";

    public static class LoginProviders
    {
        public const string GitHub = "/.auth/login/github";
        public const string Microsoft = "/.auth/login/aad";
        public const string Twitter = "/.auth/login/twitter";
    }
}