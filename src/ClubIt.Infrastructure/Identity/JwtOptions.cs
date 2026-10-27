using System.ComponentModel.DataAnnotations;

namespace ClubIt.Infrastructure.Identity;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = "ClubIt";

    [Required]
    public string Audience { get; set; } = "ClubIt.Mobile";

    /// <summary>HMAC-SHA256 key; at least 32 characters. Must come from configuration/secrets, never source control.</summary>
    [Required, MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 365)]
    public int ExpirationDays { get; set; } = 30;
}
