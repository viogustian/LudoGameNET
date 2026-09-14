namespace LudoGameNET.Api.Models;

public class RoomCacheOptions
{
    public const string SectionName = "RoomCache";

    /// <summary>
    /// Sliding expiration in minutes. The cache entry will expire if not accessed within this window.
    /// </summary>
    public int SlidingExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Absolute expiration in hours. The cache entry will expire regardless of activity after this period.
    /// </summary>
    public int AbsoluteExpirationHours { get; set; } = 2;

    /// <summary>
    /// Optional limit on the total number of cached rooms.
    /// </summary>
    public int? SizeLimit { get; set; }
}
