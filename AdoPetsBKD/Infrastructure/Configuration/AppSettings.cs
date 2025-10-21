namespace AdoPetsBKD.Infrastructure.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 30;
}

public class CorsSettings
{
    public const string SectionName = "Cors";

    public string PolicyName { get; set; } = "AdoPetsPolicy";
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

public class AzureBlobSettings
{
    public const string SectionName = "AzureBlob";

    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}

public class PayPalSettings
{
    public const string SectionName = "PayPal";

    public string Mode { get; set; } = "sandbox";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class FirebaseSettings
{
    public const string SectionName = "Firebase";

    public string ProjectId { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
}

public class PoliciesSettings
{
    public const string SectionName = "Policies";

    public string CurrentVersion { get; set; } = "1.0.0";
}

public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public int PermitLimit { get; set; } = 100;
    public int Window { get; set; } = 60;
    public int QueueLimit { get; set; } = 10;
}
