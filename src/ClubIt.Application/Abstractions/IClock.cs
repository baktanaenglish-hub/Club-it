namespace ClubIt.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }

    /// <summary>The current date in Israel, used for benefit validity checks.</summary>
    DateOnly Today { get; }
}
