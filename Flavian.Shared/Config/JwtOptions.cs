namespace Flavian.Shared.Config;

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}

public class AuthSecurityOptions
{
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;
}

public class BruteForceOptions
{
    public int MaxAttempts { get; set; } = 5;
    public int LockMinutes { get; set; } = 15;
    public int AttemptWindowMinutes { get; set; } = 10;
}

public class SchemaSettings
{
    public string DefaultSchema { get; set; } = "dbo";
}
