namespace Core.Settings;
public class RefreshTokenSettings
{
    public const string RefreshTokenSettingsSection = "RefreshTokenSettings";
    public int ExpiryMinutes { get; init; }
}
