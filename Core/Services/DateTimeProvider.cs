// Ignore Spelling: Utc

namespace Core.Services;

/// <summary>
/// provides a unified way to access date and time thru out the app
/// may not be needed, butt i like it!
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
}
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime LocalNow => DateTime.Now;
}